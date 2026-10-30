using System;
using System.Collections.Generic;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.GameInput;
using JellyDrop.Services;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.Gameplay
{
    /// <summary>
    /// Runs one game: owns the <see cref="GameSession"/>, feeds it input and time, and turns its
    /// events into juice (sounds, particles, shakes, floating text, avatar reactions).
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        // World layout (units = board cells)
        private const float TopBand = 6.4f;
        private const float BottomBand = 3.4f;
        private const float SideMargin = 0.6f;

        public event Action<GameResult> GameEnded;
        public event Action<string, Vector3, Color, float> FloatingText;
        public event Action<LuckyEvent> LuckyHappened;
        public event Action StateChanged;

        public GameSession Session { get; private set; }
        public bool IsPlaying => Session != null && !Session.IsOver;
        public bool IsPaused { get; private set; }

        public BoardView Board { get; private set; }
        public PieceSlot HoldSlot { get; private set; }
        public PieceSlot[] NextSlots { get; private set; }
        public AvatarView Avatar { get; private set; }
        public ParticleField Particles { get; private set; }

        private Camera _camera;
        private CameraShake _shake;
        private AudioManager _audio;
        private GameConfig _config;
        private readonly GestureController _gestures = new GestureController();
        private GameObject _worldRoot;
        private float _endDelay = -1f;
        private GameResult _pendingResult;

        public void Init(Camera cam, AudioManager audio, GameConfig config)
        {
            _camera = cam;
            _audio = audio;
            _config = config;
            _shake = cam.GetComponent<CameraShake>();
            if (_shake == null) _shake = cam.gameObject.AddComponent<CameraShake>();

            _worldRoot = new GameObject("GameWorld");
            _worldRoot.transform.SetParent(transform, false);

            Board = new GameObject("Board").AddComponent<BoardView>();
            Board.transform.SetParent(_worldRoot.transform, false);
            Board.transform.localPosition = new Vector3(-config.BoardWidth / 2f, -config.BoardVisibleHeight / 2f, 0f);
            Board.Build(config.BoardWidth, config.BoardVisibleHeight);

            float rowY = config.BoardVisibleHeight / 2f + 2.0f;
            HoldSlot = MakeSlot("Hold", new Vector3(-3.8f, rowY, 0f), 2.4f, 0.5f);
            NextSlots = new[]
            {
                MakeSlot("Next1", new Vector3(2.9f, rowY, 0f), 2.2f, 0.48f),
                MakeSlot("Next2", new Vector3(4.75f, rowY + 0.7f, 0f), 1.3f, 0.28f),
                MakeSlot("Next3", new Vector3(4.75f, rowY - 0.7f, 0f), 1.3f, 0.28f),
            };

            Avatar = new GameObject("Avatar").AddComponent<AvatarView>();
            Avatar.transform.SetParent(_worldRoot.transform, false);
            Avatar.transform.localPosition = new Vector3(-0.3f, rowY - 0.1f, 0f);
            Avatar.Build(2.9f);
            Avatar.SetBasePosition(new Vector3(-0.3f, rowY - 0.1f, 0f));

            Particles = new GameObject("Particles").AddComponent<ParticleField>();
            Particles.transform.SetParent(transform, false);

            _gestures.OnRotate = () => { if (Session.RotateClockwise()) { _audio.Play(Sfx.Rotate, 0.7f); Haptics.Tap(); } };
            _gestures.OnRotateCCW = () => { if (Session.RotateCounterClockwise()) { _audio.Play(Sfx.Rotate, 0.7f); Haptics.Tap(); } };
            _gestures.OnMove = () => _audio.Play(Sfx.Move, 0.5f, 0.08f);
            _gestures.OnHardDrop = () => Session.HardDrop();
            _gestures.OnHold = () => { if (Session.Hold()) _audio.Play(Sfx.Hold, 0.8f); };

            SetWorldVisible(false);
        }

        private PieceSlot MakeSlot(string name, Vector3 pos, float box, float cellScale)
        {
            var slot = new GameObject(name).AddComponent<PieceSlot>();
            slot.transform.SetParent(_worldRoot.transform, false);
            slot.transform.localPosition = pos;
            slot.Build(box, cellScale);
            return slot;
        }

        public void SetWorldVisible(bool visible) => _worldRoot.SetActive(visible);

        public void ApplyCosmetics(ShopItem skin, ShopItem avatar, ShopItem theme)
        {
            Board.SetSkin(skin);
            Avatar.SetAvatar(avatar);
            var accent = ColorUtil.Hex(theme.Accent);
            Board.SetAccent(accent);
            HoldSlot.SetAccent(accent);
            foreach (var slot in NextSlots) slot.SetAccent(ColorUtil.Blue);
        }

        /// <summary>Fits the camera so board + HUD bands are always visible, respecting notches.</summary>
        public void LayoutCamera()
        {
            var safe = Screen.safeArea;
            float sh = Mathf.Max(1, Screen.height);
            float topInset = Mathf.Clamp01((sh - safe.yMax) / sh);
            float bottomInset = Mathf.Clamp01(safe.yMin / sh);

            float boardH = _config.BoardVisibleHeight;
            float h = (boardH + TopBand + BottomBand) / Mathf.Max(0.5f, 1f - topInset - bottomInset);
            float minWidth = _config.BoardWidth + 2f * SideMargin + 1.2f;
            float size = Mathf.Max(h / 2f, minWidth / (2f * Mathf.Max(0.1f, _camera.aspect)));
            float total = size * 2f;
            // Distribute any extra height evenly.
            float extra = total - h;
            float top = TopBand + topInset * total + extra / 2f;
            float bottom = BottomBand + bottomInset * total + extra / 2f;

            _camera.orthographicSize = size;
            float centerY = (boardH / 2f + top + (-boardH / 2f - bottom)) / 2f;
            _shake.SetBasePosition(new Vector3(0f, centerY, -10f));
        }

        /// <summary>Screen pixels covered by one board cell.</summary>
        public float PixelsPerCell => Screen.height / (2f * _camera.orthographicSize);

        // ------------------------------------------------------------------ lifecycle

        /// <param name="config">Game rules for this run (base config with the difficulty applied).</param>
        public void StartGame(GameMode mode, int seed, GameConfig config)
        {
            EndSessionSubscriptions();
            Particles.ClearAll();

            IRandom rng = seed != 0 ? new SystemRandom(seed) : new SystemRandom();
            Session = new GameSession(config ?? _config, mode, rng);
            Subscribe(Session);
            IsPaused = false;
            _endDelay = -1f;
            _pendingResult = null;
            _gestures.Reset();
            SetWorldVisible(true);
            Avatar.SetBaseMood(AvatarMood.Calm);
            Avatar.React(AvatarMood.Excited, 1f);
            _audio.SetIntensity(Session.Level);
            Session.Start();
            StateChanged?.Invoke();
        }

        public void Pause()
        {
            if (!IsPlaying || IsPaused) return;
            IsPaused = true;
            Session.SetSoftDrop(false);
            _gestures.Reset();
            _audio.Duck(0.4f);
            StateChanged?.Invoke();
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            _gestures.Reset();
            _audio.Duck(1f);
            StateChanged?.Invoke();
        }

        public void Quit()
        {
            if (Session != null && !Session.IsOver) Session.Abandon();
        }

        public void Leave()
        {
            EndSessionSubscriptions();
            Session = null;
            IsPaused = false;
            Particles.ClearAll();
            SetWorldVisible(false);
            _audio.ResetIntensity();
            _audio.Duck(1f);
        }

        /// <summary>Uses a power-up from the player's inventory. Returns false if it can't be used now.</summary>
        public bool TryUsePowerUp(PowerUpType type, EconomyService economy)
        {
            if (!IsPlaying || IsPaused) return false;
            if (economy.PowerUpCount(type) <= 0 || !Session.CanUsePowerUp(type)) return false;
            economy.ConsumePowerUp(type);
            Session.UsePowerUp(type);
            return true;
        }

        private void Update()
        {
            if (Session == null) return;
            float dt = Time.unscaledDeltaTime;

            if (!Session.IsOver && !IsPaused)
            {
                _gestures.Update(Session, PixelsPerCell, dt);
                Session.Tick(Mathf.Min(dt, 0.1f));
                Avatar.SetBaseMood(Session.Board.StackHeight() >= _config.BoardVisibleHeight - 5 ? AvatarMood.Shocked : AvatarMood.Calm);
            }

            Board.Render(Session, dt);
            RenderPreviews();

            if (_endDelay >= 0f)
            {
                _endDelay -= dt;
                if (_endDelay < 0f && _pendingResult != null)
                {
                    var r = _pendingResult;
                    _pendingResult = null;
                    GameEnded?.Invoke(r);
                }
            }
        }

        private void RenderPreviews()
        {
            HoldSlot.gameObject.SetActive(Session.Config.HoldEnabled);
            HoldSlot.Show(Session.Held, false, Board, dimmed: Session.HoldUsedThisTurn);
            var preview = Session.Preview;
            int shown = Mathf.Min(Session.Config.PreviewCount, preview.Count);
            for (int i = 0; i < NextSlots.Length; i++)
            {
                bool visible = i < shown;
                if (NextSlots[i].gameObject.activeSelf != visible) NextSlots[i].gameObject.SetActive(visible);
                if (visible) NextSlots[i].Show(preview[i].Type, preview[i].Golden, Board);
            }
        }

        // ------------------------------------------------------------------ event juice

        private void Subscribe(GameSession s)
        {
            s.PieceLocked += OnLocked;
            s.HardDropped += OnHardDropped;
            s.LinesCleared += OnLinesCleared;
            s.Lucky += OnLucky;
            s.LevelUp += OnLevelUp;
            s.BombExploded += OnBomb;
            s.PowerUpActivated += OnPowerUp;
            s.GameEnded += OnGameEnded;
            s.GarbageRose += OnGarbageRose;
            s.TargetReached += OnTargetReached;
        }

        private void EndSessionSubscriptions()
        {
            var s = Session;
            if (s == null) return;
            s.PieceLocked -= OnLocked;
            s.HardDropped -= OnHardDropped;
            s.LinesCleared -= OnLinesCleared;
            s.Lucky -= OnLucky;
            s.LevelUp -= OnLevelUp;
            s.BombExploded -= OnBomb;
            s.PowerUpActivated -= OnPowerUp;
            s.GameEnded -= OnGameEnded;
            s.GarbageRose -= OnGarbageRose;
            s.TargetReached -= OnTargetReached;
        }

        private void OnLocked(ActivePiece piece)
        {
            Board.Squish(piece);
            _audio.Play(Sfx.Lock, 0.8f);
            Haptics.Tap();
            var col = piece.Golden ? ColorUtil.Gold : Board.ColorOf(piece.Type);
            foreach (var c in piece.BoardCells())
                if (c.y < _config.BoardVisibleHeight)
                    Particles.Burst(Board.CellWorld(c.x, c.y) - new Vector3(0, 0.45f, 0), 2, new[] { Color.white, col }, 2.5f, 0.35f, 0.18f);
            StateChanged?.Invoke();
        }

        private void OnHardDropped(int distance)
        {
            _audio.Play(Sfx.HardDrop, 0.9f);
            Haptics.Medium();
            if (distance > 4) _shake.Shake(0.12f, 0.15f);
        }

        private void OnLinesCleared(LineClearResult result)
        {
            var colors = new List<Color>();
            foreach (var row in result.Rows)
            {
                for (int x = 0; x < row.Cells.Length; x++)
                {
                    var cell = row.Cells[x];
                    bool golden = (cell.Flags & CellFlags.Golden) != 0;
                    var col = golden ? ColorUtil.Gold : Board.ColorOf(cell.PieceType);
                    colors.Add(col);
                    var pos = Board.CellWorld(x, row.Row);
                    Particles.Burst(pos, result.IsQuad ? 6 : 4, new[] { col, Color.white, PixelCanvas.Lighten(col, 0.5f) },
                        result.IsQuad ? 9f : 6f, 0.9f, 0.4f);
                    if ((cell.Flags & CellFlags.Coin) != 0)
                        Particles.Burst(pos, 5, new[] { Color.white }, 7f, 1.1f, 0.5f, SpriteFactory.Coin());
                }
            }

            int lowest = int.MaxValue;
            foreach (var r in result.Rows) lowest = Mathf.Min(lowest, r.Row);
            var center = Board.CellWorld(_config.BoardWidth / 2, Mathf.Min(lowest, _config.BoardVisibleHeight - 1));

            switch (result.LineCount)
            {
                case 1: _audio.Play(Sfx.Clear1); Haptics.Medium(); break;
                case 2: _audio.Play(Sfx.Clear2); Haptics.Medium(); break;
                case 3: _audio.Play(Sfx.Clear3); Haptics.Big(); break;
                default:
                    _audio.Play(Sfx.Quad);
                    _shake.Shake(0.45f, 0.4f);
                    Board.Flash();
                    Particles.Confetti(Board.WorldRect, 50);
                    Haptics.Big();
                    break;
            }

            string label;
            switch (result.LineCount)
            {
                case 1: label = Loc.T("fx.nice"); break;
                case 2: label = Loc.T("fx.sweet"); break;
                case 3: label = Loc.T("fx.delicious"); break;
                default: label = result.BackToBack ? Loc.T("fx.b2b", Loc.T("fx.blast")) : Loc.T("fx.blast"); break;
            }
            FloatingText?.Invoke(label, center + new Vector3(0, 1.2f, 0), ColorUtil.Rainbow[Mathf.Min(result.LineCount, 6)], result.IsQuad ? 1.6f : 1.1f);
            FloatingText?.Invoke("+" + Loc.Number(result.ScoreGained), center, Color.white, 0.9f);

            if (result.Combo > 0)
            {
                _audio.PlayPitched(Sfx.Combo, 1f + Mathf.Min(result.Combo, 8) * 0.06f);
                FloatingText?.Invoke(Loc.T("fx.combo", result.Combo + 1), center + new Vector3(0, 2.4f, 0), ColorUtil.Yellow, 1.0f);
            }

            Avatar.React(result.IsQuad ? AvatarMood.Excited : AvatarMood.Happy, result.IsQuad ? 2f : 1f);
            StateChanged?.Invoke();
        }

        private void OnLucky(LuckyEvent e)
        {
            var center = Board.CellWorld(_config.BoardWidth / 2, _config.BoardVisibleHeight / 2);
            switch (e.Type)
            {
                case LuckyEventType.GoldenPiece:
                    _audio.Play(Sfx.Golden, 0.7f);
                    FloatingText?.Invoke(Loc.T("fx.golden_piece"), Board.CellWorld(_config.BoardWidth / 2, _config.BoardVisibleHeight - 2), ColorUtil.Gold, 0.9f);
                    break;
                case LuckyEventType.CoinCell:
                    _audio.Play(Sfx.Coin);
                    break;
                case LuckyEventType.GoldenClear:
                    _audio.Play(Sfx.Golden);
                    FloatingText?.Invoke(Loc.T("fx.golden_clear", Session.Config.Lucky.GoldenCoinMultiplier, Loc.Coins(e.Coins)), center, ColorUtil.Gold, 1.4f);
                    Particles.Burst(center, 30, new[] { ColorUtil.Gold, Color.white }, 10f, 1.4f, 0.4f, SpriteFactory.Sparkle());
                    Avatar.React(AvatarMood.Excited, 2f);
                    break;
                case LuckyEventType.Rainbow:
                    _audio.Play(Sfx.Rainbow);
                    FloatingText?.Invoke(Loc.T("fx.rainbow", Loc.Coins(e.Coins)), center, ColorUtil.Pink, 1.4f);
                    Particles.Confetti(Board.WorldRect, 80);
                    Avatar.React(AvatarMood.Excited, 1.5f);
                    break;
                case LuckyEventType.Jackpot:
                    _audio.Play(Sfx.Jackpot);
                    _audio.Duck(0.2f);
                    JellyDrop.Util.Tweener.Delay(2.2f, () => _audio.Duck(IsPaused ? 0.4f : 1f), this);
                    FloatingText?.Invoke(Loc.T("fx.jackpot", Loc.Coins(e.Coins)), center, ColorUtil.Yellow, 2f);
                    Particles.CoinRain(Board.WorldRect, 60);
                    Board.Flash();
                    _shake.Shake(0.5f, 0.6f);
                    Haptics.Big();
                    Avatar.React(AvatarMood.Excited, 3f);
                    break;
            }
            LuckyHappened?.Invoke(e);
            StateChanged?.Invoke();
        }

        private void OnLevelUp(int level)
        {
            _audio.Play(Sfx.LevelUp);
            _audio.SetIntensity(level);
            FloatingText?.Invoke(Loc.T("fx.level", level), Board.CellWorld(_config.BoardWidth / 2, _config.BoardVisibleHeight - 4), ColorUtil.Mint, 1.3f);
        }

        private void OnTargetReached()
        {
            _audio.Play(Sfx.Win);
            Avatar.React(AvatarMood.Excited, 3f);
            FloatingText?.Invoke(Loc.T("level.goal_reached"), Board.CellWorld(_config.BoardWidth / 2, _config.BoardVisibleHeight - 6), ColorUtil.Gold, 1.8f);
            StateChanged?.Invoke();
        }

        private void OnBomb(List<Int2> destroyed)
        {
            _audio.Play(Sfx.Bomb);
            _shake.Shake(0.6f, 0.45f);
            Haptics.Big();
            foreach (var p in destroyed)
                Particles.Burst(Board.CellWorld(p.x, p.y), 6, new[] { ColorUtil.Orange, ColorUtil.Yellow, Color.white }, 10f, 0.9f, 0.5f);
            Avatar.React(AvatarMood.Shocked, 1f);
        }

        private void OnPowerUp(PowerUpType type)
        {
            _audio.Play(type == PowerUpType.LineZap ? Sfx.Zap : Sfx.PowerUp);
            var pos = Board.CellWorld(_config.BoardWidth / 2, 2);
            string text;
            switch (type)
            {
                case PowerUpType.Bomb: text = Loc.T("fx.bomb_ready"); break;
                case PowerUpType.LineZap:
                    text = Loc.T("fx.zap");
                    for (int x = 0; x < _config.BoardWidth; x++)
                        Particles.Burst(Board.CellWorld(x, 0), 4, new[] { ColorUtil.Yellow, Color.white }, 8f, 0.6f, 0.35f);
                    _shake.Shake(0.25f, 0.2f);
                    break;
                case PowerUpType.SlowMo: text = Loc.T("fx.slowmo"); break;
                default: text = Loc.T("fx.stick"); break;
            }
            FloatingText?.Invoke(text, pos, ColorUtil.Blue, 1.2f);
            StateChanged?.Invoke();
        }

        private void OnGarbageRose()
        {
            _audio.Play(Sfx.Lock, 1f, 0f);
            _shake.Shake(0.2f, 0.25f);
            for (int x = 0; x < _config.BoardWidth; x++)
                Particles.Burst(Board.CellWorld(x, 0) - new Vector3(0, 0.5f, 0), 2, new[] { ColorUtil.Red, Color.white }, 4f, 0.4f, 0.22f);
            FloatingText?.Invoke(Loc.T("fx.junk"), Board.CellWorld(_config.BoardWidth / 2, 3), ColorUtil.Red, 0.9f);
            StateChanged?.Invoke();
        }

        private void OnGameEnded(GameResult result)
        {
            Session.SetSoftDrop(false);
            _audio.Play(result.Completed ? Sfx.Win : Sfx.GameOver);
            _audio.ResetIntensity();
            Avatar.React(result.Completed ? AvatarMood.Excited : AvatarMood.Sad, 5f);
            if (result.Completed) Particles.Confetti(Board.WorldRect, 120);
            else _shake.Shake(0.3f, 0.3f);
            _pendingResult = result;
            _endDelay = 1.3f; // let the moment land before the results screen
            StateChanged?.Invoke();
        }
    }
}
