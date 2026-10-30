using System;
using System.Collections.Generic;

namespace JellyDrop.Core
{
    public sealed class ActivePiece
    {
        public PieceType Type;
        public int Rotation;
        public Int2 Origin;
        public bool Golden;
        public int CoinCellIndex = -1;

        public Int2[] LocalCells => PieceData.GetCells(Type, Rotation);

        public IEnumerable<Int2> BoardCells()
        {
            foreach (var c in LocalCells) yield return Origin + c;
        }
    }

    public sealed class LineClearResult
    {
        public List<ClearedRow> Rows = new List<ClearedRow>();
        public int LineCount => Rows.Count;
        public int ScoreGained;
        public int CoinsGained;
        public int Combo;
        public bool BackToBack;
        public bool IsQuad => LineCount >= 4;
        public int CoinCells;
        public bool HadGolden;
    }

    public sealed class LuckyEvent
    {
        public LuckyEventType Type;
        public int Coins;
        public int Score;
    }

    [Serializable]
    public sealed class SessionStats
    {
        public int Singles, Doubles, Triples, Quads;
        public int MaxCombo;
        public int GoldenClears;
        public int CoinCellsCleared;
        public int Rainbows;
        public int Jackpots;
        public int PiecesPlaced;
        public int PowerUpsUsed;
        public int HardDrops;
    }

    public sealed class GameResult
    {
        public GameMode Mode;
        public int Score;
        public int Lines;
        public int Level;
        public float TimeSeconds;
        /// <summary>Coins collected during play (coin cells, golden clears, lucky bonuses).</summary>
        public int CoinsCollected;
        public Difficulty Difficulty = Difficulty.Normal;
        /// <summary>Level number on the path (Level and Boost modes), otherwise 0.</summary>
        public int LevelNumber;
        public int TargetScore;
        /// <summary>True when a Sprint was finished (not topped out).</summary>
        public bool Completed;
        public SessionStats Stats;
    }

    /// <summary>
    /// The complete rules of one game, independent of Unity. The view layer calls the input
    /// methods and <see cref="Tick"/>, then reads state and listens to events.
    /// </summary>
    public sealed class GameSession
    {
        public event Action<ActivePiece> PieceSpawned;
        public event Action<ActivePiece> PieceLocked;
        public event Action<LineClearResult> LinesCleared;
        public event Action<LuckyEvent> Lucky;
        public event Action<int> LevelUp;
        public event Action<List<Int2>> BombExploded;
        public event Action PieceMoved;
        public event Action PieceRotated;
        public event Action<int> HardDropped;
        public event Action HoldUsedEvent;
        public event Action<PowerUpType> PowerUpActivated;
        public event Action<GameResult> GameEnded;
        /// <summary>A junk row pushed up from the bottom (Insane difficulty).</summary>
        public event Action GarbageRose;
        /// <summary>The level's target score was reached (fires once).</summary>
        public event Action TargetReached;

        private readonly GameConfig _config;
        private readonly IRandom _rng;
        private readonly PieceGenerator _generator;

        private float _fallTimer;
        private float _fallInterval = 1f;
        private float _lockTimer;
        private int _lockResets;
        private int _lowestRowReached;
        private bool _softDrop;
        private bool _lastClearWasQuad;
        private int _combo = -1;
        private float _slowMoRemaining;
        private float _garbageTimer;

        public GameSession(GameConfig config, GameMode mode, IRandom rng)
        {
            _config = config;
            _rng = rng;
            Mode = mode;
            Board = new Board(config.BoardWidth, config.BoardVisibleHeight);
            _generator = new PieceGenerator(rng, config.Lucky, config.PreviewCount);
            Level = Math.Max(1, Math.Min(config.MaxLevel, config.StartLevel));
            Stats = new SessionStats();
        }

        public GameMode Mode { get; }
        public Board Board { get; }
        public ActivePiece Current { get; private set; }
        public PieceType? Held { get; private set; }
        public bool HoldUsedThisTurn { get; private set; }
        public int Score { get; private set; }
        public int Lines { get; private set; }
        public int Level { get; private set; }
        public int CoinsCollected { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public bool IsOver { get; private set; }
        public bool Completed { get; private set; }
        public int Combo => Math.Max(0, _combo);
        public bool BackToBackActive => _lastClearWasQuad;
        public float SlowMoRemaining => _slowMoRemaining;
        public int PowerUpsUsed => Stats.PowerUpsUsed;
        public int PowerUpsLeft => Math.Max(0, _config.MaxPowerUpsPerGame - Stats.PowerUpsUsed);
        public SessionStats Stats { get; }
        public GameConfig Config => _config;
        public Difficulty Difficulty => _config.Difficulty;
        public bool TargetHit { get; private set; }

        /// <summary>Pieces still allowed in a level, or -1 when there is no limit.</summary>
        /// <summary>
        /// 0..1: how far the current piece is towards dropping one more row. The view uses it to glide the piece
        /// down smoothly instead of jumping cell by cell. 0 when the piece rests on something.
        /// </summary>
        public float FallProgress => Current == null || IsOver || IsGrounded ? 0f : Math.Min(1f, Math.Max(0f, _fallTimer / Math.Max(0.0001f, _fallInterval)));

        public int PiecesLeft => _config.PieceLimit > 0 ? Math.Max(0, _config.PieceLimit - Stats.PiecesPlaced) : -1;

        /// <summary>Seconds left in a timed level, or -1 when there is no timer.</summary>
        public float TimeLeft => _config.TimeLimitSeconds > 0f ? Math.Max(0f, _config.TimeLimitSeconds - ElapsedSeconds) : -1f;

        /// <summary>Upcoming pieces (excluding the current one).</summary>
        public IReadOnlyList<QueuedPiece> Preview => _generator.Queue;

        /// <summary>Seconds until the next junk row rises, or -1 when rising junk is off.</summary>
        public float GarbageCountdown => _config.GarbageRiseSeconds > 0f ? Math.Max(0f, _config.GarbageRiseSeconds - _garbageTimer) : -1f;

        public bool IsGrounded => Current != null && !Board.Fits(Current.Type, Current.Rotation, Current.Origin + new Int2(0, -1));

        public void Start()
        {
            if (Current != null) return;
            for (int i = 0; i < _config.StartingGarbageRows; i++) FillGarbageRow(i);
            SpawnNext();
        }

        // ---------------------------------------------------------------- update

        public void Tick(float deltaTime)
        {
            if (IsOver || Current == null) return;

            ElapsedSeconds += deltaTime;
            if (_config.TimeLimitSeconds > 0f && ElapsedSeconds >= _config.TimeLimitSeconds)
            {
                ElapsedSeconds = _config.TimeLimitSeconds;
                EndGame(true);
                return;
            }
            if (_slowMoRemaining > 0f) _slowMoRemaining = Math.Max(0f, _slowMoRemaining - deltaTime);

            if (_config.GarbageRiseSeconds > 0f && _slowMoRemaining <= 0f)
            {
                _garbageTimer += deltaTime;
                if (_garbageTimer >= _config.GarbageRiseSeconds)
                {
                    _garbageTimer = 0f;
                    RaiseGarbage();
                    if (IsOver) return;
                }
            }

            float fall = _config.FallSecondsForLevel(Level);
            if (_slowMoRemaining > 0f) fall /= _config.SlowMoFactor;
            if (_softDrop) fall = Math.Min(fall, _config.FallSecondsForLevel(1)) / _config.SoftDropMultiplier;

            _fallInterval = fall;
            _fallTimer += deltaTime;
            while (_fallTimer >= fall)
            {
                _fallTimer -= fall;
                if (TryShift(0, -1))
                {
                    if (_softDrop) Score += _config.SoftDropScorePerCell;
                }
                else
                {
                    _fallTimer = 0f;
                    break;
                }
            }

            if (IsGrounded)
            {
                _lockTimer += deltaTime;
                if (_lockTimer >= _config.LockDelaySeconds) LockCurrent();
            }
            else
            {
                _lockTimer = 0f;
            }
            CheckTarget();
        }

        private void CheckTarget()
        {
            if (TargetHit || _config.TargetScore <= 0 || Score < _config.TargetScore) return;
            TargetHit = true;
            TargetReached?.Invoke();
        }

        // ---------------------------------------------------------------- input

        public bool MoveLeft() => PlayerShift(-1);
        public bool MoveRight() => PlayerShift(1);

        public void SetSoftDrop(bool active) => _softDrop = active;

        public bool RotateClockwise() => Rotate(1);
        public bool RotateCounterClockwise() => Rotate(-1);

        public bool Rotate(int direction)
        {
            if (!CanAct() || !PieceData.CanRotate(Current.Type)) return false;
            int from = Current.Rotation;
            int to = PieceData.Mod4(from + direction);
            foreach (var kick in PieceData.GetKicks(Current.Type, from, to))
            {
                var origin = Current.Origin + kick;
                if (!Board.Fits(Current.Type, to, origin)) continue;
                Current.Rotation = to;
                Current.Origin = origin;
                OnPlayerMoved();
                PieceRotated?.Invoke();
                return true;
            }
            return false;
        }

        public int HardDrop()
        {
            if (!CanAct()) return 0;
            int distance = 0;
            while (TryShift(0, -1)) distance++;
            Score += distance * _config.HardDropScorePerCell;
            Stats.HardDrops++;
            HardDropped?.Invoke(distance);
            LockCurrent();
            return distance;
        }

        public bool Hold()
        {
            if (!CanAct() || !_config.HoldEnabled || HoldUsedThisTurn || Current.Type == PieceType.Bomb) return false;
            var previous = Held;
            Held = Current.Type;
            HoldUsedThisTurn = true;
            if (previous.HasValue) SpawnPiece(QueuedPiece.Plain(previous.Value));
            else SpawnNext(keepHoldFlag: true);
            HoldUsedEvent?.Invoke();
            return true;
        }

        /// <summary>Row the current piece would land on (for the ghost preview).</summary>
        public Int2 GhostOrigin()
        {
            if (Current == null) return default;
            var origin = Current.Origin;
            while (Board.Fits(Current.Type, Current.Rotation, origin + new Int2(0, -1))) origin.y--;
            return origin;
        }

        // ---------------------------------------------------------------- power-ups

        public bool CanUsePowerUp(PowerUpType type)
        {
            if (!CanAct() || PowerUpsLeft <= 0) return false;
            switch (type)
            {
                case PowerUpType.Bomb: return Current.Type != PieceType.Bomb;
                case PowerUpType.LineZap: return Board.StackHeight() > 0;
                case PowerUpType.SlowMo: return _slowMoRemaining <= 0f;
                case PowerUpType.Swap: return Current.Type != PieceType.I && Current.Type != PieceType.Bomb;
                default: return false;
            }
        }

        public bool UsePowerUp(PowerUpType type)
        {
            if (!CanUsePowerUp(type)) return false;
            switch (type)
            {
                case PowerUpType.Bomb:
                    // The current piece goes back on top of the queue; a bomb takes its place.
                    _generator.PushFront(new QueuedPiece
                    {
                        Type = Current.Type,
                        Golden = Current.Golden,
                        CoinCellIndex = Current.CoinCellIndex
                    });
                    SpawnPiece(new QueuedPiece { Type = PieceType.Bomb, CoinCellIndex = -1 });
                    break;
                case PowerUpType.LineZap:
                    Board.RemoveRow(0);
                    ResolveOverlapAfterShift();
                    break;
                case PowerUpType.SlowMo:
                    _slowMoRemaining = _config.SlowMoSeconds;
                    break;
                case PowerUpType.Swap:
                    SpawnPiece(QueuedPiece.Plain(PieceType.I));
                    break;
            }
            Stats.PowerUpsUsed++;
            PowerUpActivated?.Invoke(type);
            return true;
        }

        // ---------------------------------------------------------------- internals

        private int Coins(double amount) => amount <= 0 ? 0 : Math.Max(1, (int)Math.Round(amount * _config.CoinMultiplier, MidpointRounding.AwayFromZero));
        private int Points(double amount) => (int)Math.Round(amount * _config.ScoreMultiplier, MidpointRounding.AwayFromZero);

        /// <summary>Fills a row with junk blocks, leaving one random gap.</summary>
        private void FillGarbageRow(int y)
        {
            int gap = _rng.Range(0, Board.Width);
            for (int x = 0; x < Board.Width; x++)
                Board[x, y] = x == gap ? BoardCell.Empty : BoardCell.Of(PieceType.Garbage);
        }

        /// <summary>Pushes the whole stack up one row and adds a junk row at the bottom.</summary>
        private void RaiseGarbage()
        {
            if (!Board.IsRowEmpty(Board.Height - 1))
            {
                EndGame(false);
                return;
            }
            Board.InsertRowAtBottom();
            FillGarbageRow(0);
            GarbageRose?.Invoke();

            // The falling piece rides up with the stack if it now overlaps.
            if (Current != null && !Board.Fits(Current.Type, Current.Rotation, Current.Origin))
            {
                var up = Current.Origin + new Int2(0, 1);
                if (Board.Fits(Current.Type, Current.Rotation, up)) Current.Origin = up;
                else EndGame(false);
            }
        }

        /// <summary>
        /// After the stack shifts down, a piece tucked under an overhang may now overlap a block.
        /// Move it down with the stack if possible, otherwise lift it until it fits.
        /// </summary>
        private void ResolveOverlapAfterShift()
        {
            if (Current == null || Board.Fits(Current.Type, Current.Rotation, Current.Origin)) return;
            var down = Current.Origin + new Int2(0, -1);
            if (Board.Fits(Current.Type, Current.Rotation, down))
            {
                Current.Origin = down;
                return;
            }
            var up = Current.Origin;
            for (int i = 0; i < Board.Height; i++)
            {
                up.y++;
                if (Board.Fits(Current.Type, Current.Rotation, up))
                {
                    Current.Origin = up;
                    _lowestRowReached = Math.Min(_lowestRowReached, up.y);
                    return;
                }
            }
            EndGame(false);
        }

        private bool CanAct() => !IsOver && Current != null;

        private bool PlayerShift(int dx)
        {
            if (!CanAct()) return false;
            if (!TryShift(dx, 0)) return false;
            OnPlayerMoved();
            PieceMoved?.Invoke();
            return true;
        }

        private void OnPlayerMoved()
        {
            if (IsGrounded || _lockTimer > 0f)
            {
                if (_lockResets < _config.MaxLockResets)
                {
                    _lockTimer = 0f;
                    _lockResets++;
                }
            }
        }

        private bool TryShift(int dx, int dy)
        {
            var origin = Current.Origin + new Int2(dx, dy);
            if (!Board.Fits(Current.Type, Current.Rotation, origin)) return false;
            Current.Origin = origin;
            if (origin.y < _lowestRowReached)
            {
                _lowestRowReached = origin.y;
                _lockResets = 0;
            }
            return true;
        }

        private void SpawnNext(bool keepHoldFlag = false)
        {
            SpawnPiece(_generator.Next(), keepHoldFlag);
        }

        private void SpawnPiece(QueuedPiece queued, bool keepHoldFlag = true)
        {
            var piece = new ActivePiece
            {
                Type = queued.Type,
                Rotation = 0,
                Golden = queued.Golden,
                CoinCellIndex = queued.CoinCellIndex,
                Origin = PieceData.SpawnOrigin(queued.Type, Board.Width, _config.SpawnRow)
            };
            if (!keepHoldFlag) HoldUsedThisTurn = false;

            Current = piece;
            _fallTimer = 0f;
            _lockTimer = 0f;
            _lockResets = 0;
            _lowestRowReached = piece.Origin.y;

            if (!Board.Fits(piece.Type, piece.Rotation, piece.Origin))
            {
                EndGame(false);
                return;
            }

            if (piece.Golden) Lucky?.Invoke(new LuckyEvent { Type = LuckyEventType.GoldenPiece });
            PieceSpawned?.Invoke(piece);
        }

        private void LockCurrent()
        {
            var piece = Current;
            Stats.PiecesPlaced++;

            if (piece.Type == PieceType.Bomb)
            {
                ExplodeBomb(piece);
            }
            else
            {
                bool allAboveVisible = true;
                var local = piece.LocalCells;
                for (int i = 0; i < local.Length; i++)
                {
                    var p = piece.Origin + local[i];
                    var flags = CellFlags.None;
                    if (piece.Golden) flags |= CellFlags.Golden;
                    if (i == piece.CoinCellIndex) flags |= CellFlags.Coin;
                    Board[p.x, p.y] = BoardCell.Of(piece.Type, flags);
                    if (p.y < Board.VisibleHeight) allAboveVisible = false;
                }
                PieceLocked?.Invoke(piece);

                if (allAboveVisible)
                {
                    EndGame(false);
                    return;
                }
            }

            ResolveLineClears(countsForBackToBack: piece.Type != PieceType.Bomb);
            if (IsOver) return;
            CheckTarget();

            if (_config.PieceLimit > 0 && Stats.PiecesPlaced >= _config.PieceLimit)
            {
                EndGame(true);
                return;
            }

            HoldUsedThisTurn = false;
            SpawnNext();
        }

        private void ExplodeBomb(ActivePiece bomb)
        {
            var destroyed = new List<Int2>();
            var center = bomb.Origin;
            Board.ClearArea(center.x, center.y, _config.BombRadius, destroyed);
            Board.ApplyColumnGravity();
            Score += Points(destroyed.Count * 10 * Level);
            BombExploded?.Invoke(destroyed);
        }

        private void ResolveLineClears(bool countsForBackToBack)
        {
            var rows = Board.ClearFullRows();
            if (rows.Count == 0)
            {
                _combo = -1;
                return;
            }

            _combo++;
            var result = new LineClearResult { Rows = rows, Combo = _combo };

            int n = Math.Min(rows.Count, _config.LineClearScores.Length - 1);
            double score = _config.LineClearScores[n] * Level;
            if (rows.Count >= 4 && countsForBackToBack)
            {
                if (_lastClearWasQuad)
                {
                    score *= _config.BackToBackMultiplier;
                    result.BackToBack = true;
                }
                _lastClearWasQuad = true;
            }
            else if (countsForBackToBack)
            {
                _lastClearWasQuad = false;
            }
            if (_combo > 0) score += _config.ComboScore * _combo * Level;
            result.ScoreGained = Points(score);

            // Coins
            int coins = rows.Count * _config.CoinsPerLine;
            foreach (var row in rows)
            {
                foreach (var cell in row.Cells)
                {
                    if ((cell.Flags & CellFlags.Coin) != 0) result.CoinCells++;
                    if ((cell.Flags & CellFlags.Golden) != 0) result.HadGolden = true;
                }
            }
            coins += result.CoinCells * _config.CoinsPerCoinCell;
            if (result.CoinCells > 0)
            {
                Stats.CoinCellsCleared += result.CoinCells;
                Lucky?.Invoke(new LuckyEvent { Type = LuckyEventType.CoinCell, Coins = Coins(result.CoinCells * _config.CoinsPerCoinCell) });
            }
            if (result.HadGolden)
            {
                int before = coins;
                coins *= _config.Lucky.GoldenCoinMultiplier;
                Stats.GoldenClears++;
                Lucky?.Invoke(new LuckyEvent { Type = LuckyEventType.GoldenClear, Coins = Coins(coins) - Coins(before) });
            }
            coins = Coins(coins);
            result.CoinsGained = coins;

            Score += result.ScoreGained;
            CoinsCollected += coins;
            UpdateStats(rows.Count);
            AddLines(rows.Count);

            LinesCleared?.Invoke(result);
            RollLuckyBonuses();
            CheckSprintFinished();
        }

        private void RollLuckyBonuses()
        {
            var lucky = _config.Lucky;
            if (_rng.NextDouble() < lucky.JackpotChancePerClear)
            {
                int coins = Coins(lucky.JackpotCoins);
                CoinsCollected += coins;
                Stats.Jackpots++;
                Lucky?.Invoke(new LuckyEvent { Type = LuckyEventType.Jackpot, Coins = coins });
            }
            else if (_rng.NextDouble() < lucky.RainbowChancePerClear)
            {
                int coins = Coins(lucky.RainbowCoins);
                int points = Points(lucky.RainbowScore * Level);
                CoinsCollected += coins;
                Score += points;
                Stats.Rainbows++;
                Lucky?.Invoke(new LuckyEvent { Type = LuckyEventType.Rainbow, Coins = coins, Score = points });
            }
        }

        private void UpdateStats(int lines)
        {
            switch (lines)
            {
                case 1: Stats.Singles++; break;
                case 2: Stats.Doubles++; break;
                case 3: Stats.Triples++; break;
                default: Stats.Quads++; break;
            }
            Stats.MaxCombo = Math.Max(Stats.MaxCombo, _combo);
        }

        private void AddLines(int count)
        {
            Lines += count;
            int start = Math.Max(1, _config.StartLevel);
            int newLevel = Math.Min(_config.MaxLevel, start + Lines / _config.LinesPerLevel);
            if (newLevel > Level)
            {
                Level = newLevel;
                LevelUp?.Invoke(Level);
            }
        }

        private void CheckSprintFinished()
        {
            if (Mode == GameMode.Sprint && Lines >= _config.SprintLines) EndGame(true);
        }

        private void EndGame(bool completed, bool abandoned = false)
        {
            if (IsOver) return;
            IsOver = true;
            if (!abandoned)
            {
                // A Boost level always pays out what was scored; a target level is won by reaching the target,
                // even if the stack tops out afterwards.
                if (Mode == GameMode.Boost) completed = true;
                else if (_config.TargetScore > 0) completed = Score >= _config.TargetScore;
            }
            Completed = completed;
            GameEnded?.Invoke(GetResult());
        }

        /// <summary>Ends the game early (e.g. player quits from pause). Never counts as a win.</summary>
        public void Abandon() => EndGame(false, abandoned: true);

        public GameResult GetResult() => new GameResult
        {
            Mode = Mode,
            Score = Score,
            Lines = Lines,
            Level = Level,
            TimeSeconds = ElapsedSeconds,
            CoinsCollected = CoinsCollected,
            Difficulty = _config.Difficulty,
            LevelNumber = _config.LevelNumber,
            TargetScore = _config.TargetScore,
            Completed = Completed,
            Stats = Stats
        };
    }
}
