using System;
using System.Collections.Generic;

namespace JellyDrop.Core.Runner
{
    /// <summary>What blocks a lane: jump over Low, slide under High, switch lanes around Block.</summary>
    public enum RunObstacle { None = 0, Low = 1, High = 2, Block = 3 }

    public sealed class RunRow
    {
        public float Z;
        public readonly RunObstacle[] Lanes = new RunObstacle[RunnerSession.LaneCount];
    }

    public sealed class RunCoin
    {
        public int Lane;
        public float Z;
        /// <summary>Floating coin, only reachable with a jump.</summary>
        public bool High;
        public bool Taken;
    }

    /// <summary>
    /// Block Run: a three-lane endless runner (think Subway Surfers) starring the player's avatar block.
    /// Swipe left/right to change lanes, up to jump over low barriers, down to slide under high ones;
    /// crates block a whole lane. Speed rises over time. Coins along the way are the reward.
    /// Pure C#, deterministic for a given seed, so it is unit-tested and the 3D view only draws it.
    /// </summary>
    public sealed class RunnerSession
    {
        public const int LaneCount = 3;
        public const float StartSpeed = 10f;
        public const float MaxSpeed = 24f;
        public const float Acceleration = 0.22f;
        public const float JumpSeconds = 0.72f;
        public const float JumpPeak = 1.7f;
        public const float SlideSeconds = 0.7f;
        /// <summary>Length of a Block (crate) along the track.</summary>
        public const float BlockLength = 2.6f;
        public const float SpawnAhead = 90f;
        private const float HitWindow = 0.45f;

        public event Action<RunRow> RowSpawned;
        public event Action<RunCoin> CoinSpawned;
        public event Action<RunCoin> CoinCollected;
        public event Action Crashed;
        public event Action<int> LaneChanged;

        public int Lane { get; private set; } = 1;
        public float Distance { get; private set; }
        public float Speed { get; private set; } = StartSpeed;
        public bool IsOver { get; private set; }
        public int Coins { get; private set; }
        public float Time { get; private set; }

        /// <summary>Height above the ground (jump arc).</summary>
        public float Height => _jump >= 0f ? 4f * JumpPeak * (_jump / JumpSeconds) * (1f - _jump / JumpSeconds) : 0f;
        public bool IsJumping => _jump >= 0f;
        public bool IsSliding => _slide >= 0f;

        public IReadOnlyList<RunRow> Rows => _rows;
        public IReadOnlyList<RunCoin> CoinsOnTrack => _coins;

        public int Score => (int)Distance + Coins * 10;

        private readonly Random _rng;
        private readonly List<RunRow> _rows = new List<RunRow>();
        private readonly List<RunCoin> _coins = new List<RunCoin>();
        private float _nextRowZ = 26f;
        private float _jump = -1f;
        private float _slide = -1f;

        public RunnerSession(int seed)
        {
            _rng = new Random(seed);
        }

        public void Start() => Generate();

        // ------------------------------------------------------------ input

        public void Left() => MoveLane(-1);
        public void Right() => MoveLane(1);

        private void MoveLane(int dir)
        {
            if (IsOver) return;
            int target = Math.Max(0, Math.Min(LaneCount - 1, Lane + dir));
            if (target == Lane) return;
            Lane = target;
            LaneChanged?.Invoke(Lane);
            // Sidestepping into a crate is a crash too.
            CheckCollisions(Distance, Distance);
        }

        public void Jump()
        {
            if (IsOver || IsJumping) return;
            _slide = -1f;
            _jump = 0f;
        }

        /// <summary>Slide; in the air it cuts the jump short and slams down.</summary>
        public void Slide()
        {
            if (IsOver) return;
            _jump = -1f;
            _slide = 0f;
        }

        // ------------------------------------------------------------ simulation

        public void Tick(float dt)
        {
            if (IsOver || dt <= 0f) return;
            // Small steps keep collisions exact even on slow frames.
            while (dt > 0f && !IsOver)
            {
                float step = Math.Min(dt, 0.02f);
                dt -= step;
                Step(step);
            }
        }

        private void Step(float dt)
        {
            Time += dt;
            Speed = Math.Min(MaxSpeed, Speed + Acceleration * dt);
            if (_jump >= 0f) { _jump += dt; if (_jump >= JumpSeconds) _jump = -1f; }
            if (_slide >= 0f) { _slide += dt; if (_slide >= SlideSeconds) _slide = -1f; }

            float before = Distance;
            Distance += Speed * dt;
            CheckCollisions(before, Distance);
            CollectCoins(before, Distance);
            Generate();
            Cleanup();
        }

        private void CheckCollisions(float from, float to)
        {
            foreach (var row in _rows)
            {
                var o = row.Lanes[Lane];
                if (o == RunObstacle.None) continue;
                bool hit;
                if (o == RunObstacle.Block)
                {
                    // A crate occupies [Z, Z + length]: touching it at any point is a crash.
                    hit = to >= row.Z - HitWindow && from <= row.Z + BlockLength;
                }
                else
                {
                    bool crossing = to >= row.Z - HitWindow && from <= row.Z + HitWindow;
                    if (!crossing) continue;
                    hit = o == RunObstacle.Low ? Height < 0.75f : !IsSliding;
                }
                if (hit)
                {
                    IsOver = true;
                    Crashed?.Invoke();
                    return;
                }
            }
        }

        private void CollectCoins(float from, float to)
        {
            foreach (var c in _coins)
            {
                if (c.Taken || c.Lane != Lane) continue;
                if (c.Z < from - 0.6f || c.Z > to + 0.6f) continue;
                bool reachable = c.High ? Height > 0.9f : Height < 1.2f;
                if (!reachable) continue;
                c.Taken = true;
                Coins++;
                CoinCollected?.Invoke(c);
            }
        }

        private void Cleanup()
        {
            _rows.RemoveAll(r => r.Z + BlockLength < Distance - 12f);
            _coins.RemoveAll(c => c.Z < Distance - 12f);
        }

        // ------------------------------------------------------------ track generation

        private void Generate()
        {
            while (_nextRowZ < Distance + SpawnAhead)
            {
                var row = new RunRow { Z = _nextRowZ };
                FillRow(row);
                _rows.Add(row);
                RowSpawned?.Invoke(row);
                AddCoins(row);
                // Gaps grow with speed so there is always time to react.
                float gap = 9f + Speed * 0.55f + (float)_rng.NextDouble() * 6f;
                _nextRowZ += gap;
            }
        }

        private void FillRow(RunRow row)
        {
            // How hard: more blocked lanes as the run goes on, but always at least one way through.
            double difficulty = Math.Min(1.0, Distance / 1500.0);
            int obstacles = 1 + (_rng.NextDouble() < 0.35 + difficulty * 0.45 ? 1 : 0);
            var lanes = new List<int> { 0, 1, 2 };
            for (int k = 0; k < obstacles; k++)
            {
                int pick = lanes[_rng.Next(lanes.Count)];
                lanes.Remove(pick);
                double r = _rng.NextDouble();
                row.Lanes[pick] = r < 0.4 ? RunObstacle.Block : r < 0.72 ? RunObstacle.Low : RunObstacle.High;
            }
            // Never block all three lanes with crates.
            int crates = 0;
            foreach (var o in row.Lanes) if (o == RunObstacle.Block) crates++;
            if (crates == LaneCount) row.Lanes[_rng.Next(LaneCount)] = RunObstacle.None;
        }

        private void AddCoins(RunRow row)
        {
            int lane = _rng.Next(LaneCount);
            var o = row.Lanes[lane];
            if (o == RunObstacle.Block) return; // no coins inside crates
            int count = 5;
            for (int i = 0; i < count; i++)
            {
                // A trail leading up to the row; over a low barrier the last coins float, so they take a jump.
                float z = o == RunObstacle.None ? row.Z - 4f + i * 1.6f : row.Z - (count - 1 - i) * 1.6f;
                bool high = o == RunObstacle.Low && i >= count - 2;
                var coin = new RunCoin { Lane = lane, Z = z, High = high };
                _coins.Add(coin);
                CoinSpawned?.Invoke(coin);
            }
        }
    }
}
