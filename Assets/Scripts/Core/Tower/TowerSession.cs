using System;
using System.Collections.Generic;

namespace JellyDrop.Core.Tower
{
    /// <summary>Endless modes for the tower climb.</summary>
    public enum TowerMode { Classic = 0, Speed = 1, Ice = 2 }

    /// <summary>Look of the tower (walls, platforms, background). Purely visual.</summary>
    public enum TowerScenery { Ice = 0, Candy = 1, Jungle = 2, Space = 3, Lava = 4, Forest = 5 }

    /// <summary>Things to grab on the way up: coins, energy for the pet and supplies for the city.</summary>
    public enum TowerItemKind { Coin = 0, Energy = 1, Wood = 2, Brick = 3, Glass = 4, Mushroom = 5 }

    public sealed class TowerItem
    {
        public TowerItemKind Kind;
        public float X, Y;
        public bool Taken;
    }

    public enum PlatformKind { Normal, Ice, Moving, Crumble, Spring, Goal }

    public sealed class TowerPlatform
    {
        public int Floor;
        public float X, Width;
        public PlatformKind Kind;
        public float Vx;
        public float CrumbleTime = -1f;   // counts down once stood on
        public bool Broken;
        public float Left => X - Width / 2f;
        public float Right => X + Width / 2f;
    }

    /// <summary>Rules for one climb: an endless mode or a tower with a fixed number of floors.</summary>
    public sealed class TowerConfig
    {
        public int Floors;                 // 0 = endless
        public TowerScenery Scenery;
        public bool CycleScenery;          // endless: the look changes every 50 floors
        public float ScrollStart = 0.95f;   // units per second once the tower starts moving
        public float ScrollAccel = 0.022f; // per second
        public float ScrollMax = 5.2f;
        public float MaxWidth = 4.1f, MinWidth = 2.2f;
        public int NarrowAfter = 200;      // floors until platforms reach MinWidth
        public float IceChance, MovingChance, CrumbleChance, SpringChance;
        public bool AllIce;
        /// <summary>Mushroom power: the climb starts with a big spring jump.</summary>
        public bool SpringStart;

        public static TowerConfig Endless(TowerMode mode, TowerScenery scenery)
        {
            var c = new TowerConfig { Scenery = scenery, CycleScenery = true, MovingChance = 0.08f, CrumbleChance = 0.05f, SpringChance = 0.04f, IceChance = 0.08f };
            // Scroll speeds are in units; floors are 2.6 apart, so these keep the same floors-per-second pace as before.
            if (mode == TowerMode.Speed) { c.ScrollStart = 1.9f; c.ScrollAccel = 0.035f; c.ScrollMax = 6.1f; }
            if (mode == TowerMode.Ice) { c.AllIce = true; c.IceChance = 1f; c.ScrollStart = 0.8f; }
            return c;
        }
    }

    /// <summary>
    /// An Icy Tower style climb. The avatar runs left and right inside a tower and jumps from floor to floor;
    /// the faster it runs, the higher it jumps, and it bounces off the walls. Jumping over two or more floors at
    /// once builds a combo. After a few floors the tower starts scrolling up, faster and faster: fall off the
    /// bottom of the screen and the climb is over. Units: the tower is 10 wide, floors are 3 apart.
    /// Fully deterministic for a given seed (same inputs, same climb).
    /// </summary>
    public sealed class TowerSession
    {
        public const int SceneryCount = 6;
        public const float Width = 10f;
        public const float PickupRadius = 0.85f;
        public const float Gap = 2.6f;
        public const float ViewHeight = 16f;
        public const float HalfBody = 0.4f;
        public const float Gravity = -40f;
        public const float JumpBase = 17.5f;
        public const float JumpPerSpeed = 1.0f;
        public const float MaxSpeed = 15f;
        /// <summary>Upward speed of the double jump (double tap while in the air).</summary>
        public const float AirJumpBase = 14f;
        public const float SpringJump = 30f;
        public const float ComboTime = 3f;
        private const float StepDt = 1f / 120f;

        public TowerConfig Config { get; }
        public float X { get; private set; } = Width / 2f;
        public float Y { get; private set; }
        public float Vx { get; private set; }
        public float Vy { get; private set; }
        public bool Grounded { get; private set; } = true;
        public TowerPlatform Ground { get; private set; }
        public int HighestFloor { get; private set; }
        public float CameraY { get; private set; }
        public float ScrollSpeed { get; private set; }
        public bool Scrolling { get; private set; }
        public int Combo { get; private set; }
        public float ComboTimer { get; private set; }
        public int BestCombo { get; private set; }
        public int ComboScore { get; private set; }
        public int Score => HighestFloor * 10 + ComboScore;
        public float Time { get; private set; }
        public bool IsOver { get; private set; }
        public bool IsWon { get; private set; }
        public int Jumps { get; private set; }

        /// <summary>Raised when the avatar lands (floor, floors jumped).</summary>
        public event Action<int, int> Landed;
        public event Action<int> ComboEnded;
        public event Action Bounced;
        public event Action DoubleJumped;
        public event Action Finished;
        public event Action<TowerItem> ItemCollected;

        /// <summary>How many of each item were collected (index = (int)TowerItemKind).</summary>
        public readonly int[] Collected = new int[6];
        private readonly List<TowerItem> _items = new List<TowerItem>();
        private Random _itemRng;
        public IReadOnlyList<TowerItem> Items => _items;

        private readonly List<TowerPlatform> _platforms = new List<TowerPlatform>();
        private readonly Random _rng;
        private int _generatedTo = -1;
        private int _jumpFrom;
        private bool _airJumped;
        private float _acc;

        public IReadOnlyList<TowerPlatform> Platforms => _platforms;

        public TowerSession(TowerConfig config, int seed)
        {
            Config = config;
            _rng = new Random(seed);
            _itemRng = new Random(seed * 31 + 7);
            Generate(0);
            EnsureGenerated();
            Ground = _platforms[0];
            Y = 0f;
            if (config.SpringStart)
            {
                Grounded = false;
                Ground = null;
                Vy = SpringJump * 1.25f;
                _jumpFrom = 0;
            }
        }

        /// <summary>The look for the current height (endless towers change every 50 floors).</summary>
        public TowerScenery SceneryAt(int floor) =>
            Config.CycleScenery ? (TowerScenery)(((int)Config.Scenery + floor / 50) % SceneryCount) : Config.Scenery;

        public bool IsMilestone(int floor) => Config.Floors == 0 && floor > 0 && floor % 50 == 0;

        // ------------------------------------------------------------------ generation

        private void EnsureGenerated()
        {
            int need = (int)((CameraY + ViewHeight) / Gap) + 4;
            if (Config.Floors > 0) need = Math.Min(need, Config.Floors);
            while (_generatedTo < need) Generate(_generatedTo + 1);
            // Forget floors far below the screen.
            _platforms.RemoveAll(p => p.Floor * Gap < CameraY - 4f && p != Ground);
            _items.RemoveAll(i => i.Taken || i.Y < CameraY - 4f);
        }

        private float Rand(float a, float b) => a + (float)_rng.NextDouble() * (b - a);

        private void Generate(int floor)
        {
            _generatedTo = floor;
            var c = Config;
            var p = new TowerPlatform { Floor = floor };
            bool goal = c.Floors > 0 && floor == c.Floors;
            if (floor == 0 || goal || IsMilestone(floor))
            {
                p.X = Width / 2f;
                p.Width = Width;
                p.Kind = goal ? PlatformKind.Goal : PlatformKind.Normal;
                if (c.AllIce && !goal) p.Kind = PlatformKind.Ice;
                _platforms.Add(p);
                return;
            }
            float t = Math.Min(1f, floor / (float)Math.Max(1, c.NarrowAfter));
            p.Width = c.MaxWidth + (c.MinWidth - c.MaxWidth) * t + Rand(-0.4f, 0.4f);
            p.Width = Math.Max(1.5f, Math.Min(Width - 1f, p.Width));
            p.X = Rand(p.Width / 2f, Width - p.Width / 2f);
            p.Kind = c.AllIce ? PlatformKind.Ice : PlatformKind.Normal;
            if (floor > 4)
            {
                double r = _rng.NextDouble();
                if ((r -= c.MovingChance) < 0) { p.Kind = PlatformKind.Moving; p.Vx = Rand(1.4f, 2.6f + Math.Min(2f, floor / 100f)) * (_rng.Next(2) == 0 ? -1 : 1); }
                else if ((r -= c.CrumbleChance) < 0) p.Kind = PlatformKind.Crumble;
                else if ((r -= c.SpringChance) < 0) p.Kind = PlatformKind.Spring;
                else if (!c.AllIce && (r -= c.IceChance) < 0) p.Kind = PlatformKind.Ice;
            }
            _platforms.Add(p);
            SpawnItems(p);
        }

        /// <summary>Puts coins and the odd bonus above a platform (and sometimes a coin in the gap above it).</summary>
        private void SpawnItems(TowerPlatform p)
        {
            if (p.Floor < 2 || p.Kind == PlatformKind.Goal) return;
            var r = _itemRng;
            float y = p.Floor * Gap + 0.9f;
            double roll = r.NextDouble();
            TowerItemKind? kind = null;
            if (roll < 0.05) kind = TowerItemKind.Energy;
            else if (roll < 0.11) kind = TowerItemKind.Wood;
            else if (roll < 0.15) kind = TowerItemKind.Brick;
            else if (roll < 0.175 && p.Floor >= 15) kind = TowerItemKind.Glass;
            else if (roll < 0.6) kind = TowerItemKind.Coin;
            // Mushrooms grow in the green towers.
            var look = SceneryAt(p.Floor);
            double mushChance = look == TowerScenery.Forest || look == TowerScenery.Jungle ? 0.12 : 0.04;
            if (kind != TowerItemKind.Energy && r.NextDouble() < mushChance) kind = TowerItemKind.Mushroom;
            if (kind.HasValue)
            {
                float x = p.Kind == PlatformKind.Moving ? p.X : p.Left + 0.5f + (float)r.NextDouble() * Math.Max(0f, p.Width - 1f);
                _items.Add(new TowerItem { Kind = kind.Value, X = x, Y = y });
                // Coins often come in a little row.
                if (kind == TowerItemKind.Coin && p.Width > 3f && r.NextDouble() < 0.5)
                {
                    float x2 = Math.Min(p.Right - 0.4f, x + 0.9f);
                    if (x2 > x + 0.5f) _items.Add(new TowerItem { Kind = TowerItemKind.Coin, X = x2, Y = y });
                }
            }
            // A coin floating between floors rewards big jumps.
            if (r.NextDouble() < 0.18)
                _items.Add(new TowerItem { Kind = TowerItemKind.Coin, X = 1f + (float)r.NextDouble() * (Width - 2f), Y = p.Floor * Gap + Gap * 1.5f });
        }

        private void CollectItems()
        {
            float cy = Y + 0.5f;
            foreach (var it in _items)
            {
                if (it.Taken) continue;
                float dx = it.X - X, dy = it.Y - cy;
                if (dx * dx + dy * dy > PickupRadius * PickupRadius) continue;
                it.Taken = true;
                Collected[(int)it.Kind]++;
                ItemCollected?.Invoke(it);
            }
        }

        // ------------------------------------------------------------------ simulation

        /// <summary>
        /// Advances the climb. <paramref name="move"/> is -1..1 (left/right), <paramref name="jump"/> is true while
        /// the jump button is held (the avatar jumps as soon as it lands).
        /// </summary>
        public void Step(float dt, float move, bool jump)
        {
            if (IsOver || IsWon) return;
            _acc += Math.Min(dt, 0.1f);
            while (_acc >= StepDt && !IsOver && !IsWon)
            {
                _acc -= StepDt;
                Tick(StepDt, Math.Max(-1f, Math.Min(1f, move)), jump);
            }
        }

        private void Tick(float dt, float move, bool jump)
        {
            Time += dt;
            MovePlatforms(dt);

            bool ice = Grounded && Ground != null && Ground.Kind == PlatformKind.Ice;
            // Horizontal: build up speed, slide on ice, steer a bit in the air.
            if (move != 0f)
            {
                float accel = Grounded ? (ice ? 14f : 52f) : 32f;
                // Turning around is quicker than speeding up.
                if (Math.Sign(move) != Math.Sign(Vx) && Vx != 0f) accel *= 1.8f;
                Vx = Toward(Vx, move * MaxSpeed, accel * dt);
            }
            else if (Grounded)
            {
                Vx = Toward(Vx, 0f, (ice ? 2.5f : 40f) * dt);
            }

            if (Grounded && Ground != null && Ground.Kind == PlatformKind.Moving) X += Ground.Vx * dt;
            X += Vx * dt;
            // Walls: bounce back keeping most of the speed (the classic trick for big jumps).
            if (X < HalfBody) { X = HalfBody; if (Vx < 0) { Vx = -Vx * 0.9f; Bounced?.Invoke(); } }
            if (X > Width - HalfBody) { X = Width - HalfBody; if (Vx > 0) { Vx = -Vx * 0.9f; Bounced?.Invoke(); } }

            if (Grounded)
            {
                var g = Ground;
                bool off = g == null || g.Broken || X < g.Left - HalfBody * 0.5f || X > g.Right + HalfBody * 0.5f;
                if (g != null && g.Kind == PlatformKind.Crumble && g.CrumbleTime < 0f) g.CrumbleTime = 0.55f;
                if (off)
                {
                    Grounded = false;
                    Ground = null;
                    Vy = 0f;
                    _jumpFrom = g?.Floor ?? HighestFloor;
                }
                else if (jump)
                {
                    Vy = JumpBase + Math.Abs(Vx) * JumpPerSpeed;
                    Grounded = false;
                    Ground = null;
                    _jumpFrom = g.Floor;
                    Jumps++;
                }
            }

            if (!Grounded)
            {
                float oldY = Y;
                Vy += Gravity * dt;
                Y += Vy * dt;
                if (Vy <= 0f) TryLand(oldY);
            }

            CollectItems();
            UpdateCombo(dt);
            UpdateCamera(dt);
            EnsureGenerated();

            if (Y < CameraY - 1f)
            {
                IsOver = true;
                EndCombo();
                Finished?.Invoke();
            }
        }

        /// <summary>True while a double jump is still available (in the air, not used yet).</summary>
        public bool CanAirJump => !Grounded && !_airJumped && !IsOver && !IsWon;

        /// <summary>The double jump: a second, smaller jump in mid-air. Once per jump; resets on landing.</summary>
        public bool AirJump()
        {
            if (!CanAirJump) return false;
            Vy = Math.Max(Vy, 0f) * 0.25f + AirJumpBase + Math.Abs(Vx) * 0.35f;
            _airJumped = true;
            Jumps++;
            DoubleJumped?.Invoke();
            return true;
        }

        private void MovePlatforms(float dt)
        {
            foreach (var p in _platforms)
            {
                if (p.Kind == PlatformKind.Moving)
                {
                    p.X += p.Vx * dt;
                    if (p.Left < 0f) { p.X = p.Width / 2f; p.Vx = Math.Abs(p.Vx); }
                    if (p.Right > Width) { p.X = Width - p.Width / 2f; p.Vx = -Math.Abs(p.Vx); }
                }
                if (p.CrumbleTime >= 0f && !p.Broken)
                {
                    p.CrumbleTime -= dt;
                    if (p.CrumbleTime <= 0f) p.Broken = true;
                }
            }
        }

        private void TryLand(float oldY)
        {
            TowerPlatform best = null;
            foreach (var p in _platforms)
            {
                if (p.Broken) continue;
                float py = p.Floor * Gap;
                if (py > oldY + 0.0001f || py < Y) continue;
                if (X < p.Left - HalfBody * 0.6f || X > p.Right + HalfBody * 0.6f) continue;
                if (best == null || p.Floor > best.Floor) best = p;
            }
            if (best == null) return;

            Y = best.Floor * Gap;
            int jumped = best.Floor - _jumpFrom;
            if (best.Kind == PlatformKind.Spring)
            {
                // Springs launch you straight away.
                Vy = SpringJump;
                Grounded = false;
                Ground = null;
                _jumpFrom = best.Floor;
                _airJumped = false;
            }
            else
            {
                Vy = 0f;
                Grounded = true;
                Ground = best;
                _airJumped = false;
            }

            if (best.Floor > HighestFloor) HighestFloor = best.Floor;
            if (jumped >= 2)
            {
                Combo += jumped;
                ComboTimer = ComboTime;
            }
            else if (jumped < 2 && Combo > 0 && best.Kind != PlatformKind.Spring)
            {
                EndCombo();
            }
            Landed?.Invoke(best.Floor, jumped);

            if (best.Kind == PlatformKind.Goal)
            {
                IsWon = true;
                EndCombo();
                Finished?.Invoke();
            }
        }

        private void UpdateCombo(float dt)
        {
            if (Combo <= 0) return;
            ComboTimer -= dt;
            if (ComboTimer <= 0f) EndCombo();
        }

        private void EndCombo()
        {
            if (Combo <= 0) return;
            int c = Combo;
            Combo = 0;
            ComboTimer = 0f;
            // Only real combos (two or more big jumps) score.
            if (c >= 4)
            {
                ComboScore += c * c;
                if (c > BestCombo) BestCombo = c;
            }
            ComboEnded?.Invoke(c);
        }

        private void UpdateCamera(float dt)
        {
            if (!Scrolling && (HighestFloor >= 3 || Time > 5f)) Scrolling = true;
            if (Scrolling)
            {
                ScrollSpeed = Math.Min(Config.ScrollMax, Config.ScrollStart + Config.ScrollAccel * Time);
                // Don't push the player off a finished tower's top.
                if (!(Config.Floors > 0 && CameraY + ViewHeight >= Config.Floors * Gap + 4f))
                    CameraY += ScrollSpeed * dt;
            }
            // Follow the player up when they climb fast.
            float follow = Y - ViewHeight * 0.55f;
            if (follow > CameraY) CameraY = follow;
        }

        private static float Toward(float v, float target, float maxDelta)
        {
            if (Math.Abs(target - v) <= maxDelta) return target;
            return v + Math.Sign(target - v) * maxDelta;
        }

        /// <summary>Highest platform under the avatar's floor + n, if it is generated (used by the demo bot).</summary>
        public TowerPlatform PlatformAt(int floor)
        {
            foreach (var p in _platforms) if (p.Floor == floor) return p;
            return null;
        }
    }

    /// <summary>A simple bot that climbs one floor at a time (tests check every tower can be finished).</summary>
    public static class TowerBot
    {
        public static void Decide(TowerSession s, out float move, out bool jump)
        {
            int from = s.Grounded && s.Ground != null ? s.Ground.Floor : s.HighestFloor;
            var target = s.PlatformAt(from + 1) ?? s.PlatformAt(from);
            move = 0f;
            jump = false;
            if (target == null) return;
            // Aim where a moving platform will be when we come down on it.
            float aim = target.X;
            if (target.Kind == PlatformKind.Moving) aim += target.Vx * (s.Grounded ? 0.8f : 0.3f);
            aim = Math.Max(target.Width / 2f, Math.Min(TowerSession.Width - target.Width / 2f, aim));
            float dx = aim - s.X;
            if (s.Grounded)
            {
                move = Math.Abs(dx) > 0.3f ? Math.Sign(dx) : 0f;
                // Jump early and steer in the air; don't jump while racing the wrong way.
                bool wrongWay = Math.Sign(s.Vx) != Math.Sign(dx) && Math.Abs(s.Vx) > 6f && Math.Abs(dx) > 1f;
                jump = !wrongWay && Math.Abs(dx) < target.Width / 2f + 3.2f;
                if (s.Ground != null && s.Ground.Kind == PlatformKind.Crumble) jump = true;
            }
            else
            {
                // Falling short of the next floor: use the double jump.
                if (s.CanAirJump && s.Vy < 0f && s.Y < target.Floor * TowerSession.Gap + 0.3f) s.AirJump();
                // Steer toward the target; brake near it.
                if (Math.Abs(dx) > 0.25f) move = Math.Sign(dx);
                else move = -Math.Sign(s.Vx) * 0.5f;
                if (Math.Abs(dx) < Math.Abs(s.Vx) * 0.12f && Math.Sign(s.Vx) == Math.Sign(dx)) move = -Math.Sign(dx);
            }
        }
    }
}
