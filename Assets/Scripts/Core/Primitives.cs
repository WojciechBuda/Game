using System;

namespace JellyDrop.Core
{
    /// <summary>Integer 2D point (y points up, row 0 is the bottom of the board).</summary>
    [Serializable]
    public struct Int2 : IEquatable<Int2>
    {
        public int x;
        public int y;

        public Int2(int x, int y) { this.x = x; this.y = y; }

        public static Int2 operator +(Int2 a, Int2 b) => new Int2(a.x + b.x, a.y + b.y);
        public static Int2 operator -(Int2 a, Int2 b) => new Int2(a.x - b.x, a.y - b.y);
        public static bool operator ==(Int2 a, Int2 b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Int2 a, Int2 b) => !(a == b);

        public bool Equals(Int2 other) => this == other;
        public override bool Equals(object obj) => obj is Int2 o && this == o;
        public override int GetHashCode() => (x * 397) ^ y;
        public override string ToString() => $"({x},{y})";
    }

    public enum PieceType
    {
        I = 0, O = 1, T = 2, S = 3, Z = 4, J = 5, L = 6,
        /// <summary>Single-cell power-up piece that explodes a 3x3 area when it lands.</summary>
        Bomb = 7,
        /// <summary>Junk block from difficulty rows. Only ever appears as a board cell, never as a falling piece.</summary>
        Garbage = 8
    }

    public enum GameMode
    {
        Classic = 0,
        Sprint = 1,
        Daily = 2,
        /// <summary>A numbered level on the path: reach the target score within a piece limit.</summary>
        Level = 3,
        /// <summary>Every 10th level: score as much as possible before the timer runs out.</summary>
        Boost = 4
    }

    public enum PowerUpType
    {
        Bomb = 0,
        LineZap = 1,
        SlowMo = 2,
        Swap = 3
    }

    public enum LuckyEventType
    {
        GoldenPiece,   // a golden piece spawned
        GoldenClear,   // a line clear included golden cells (coins multiplied)
        CoinCell,      // coin cell(s) cleared
        Rainbow,       // random rainbow bonus on a clear
        Jackpot        // very rare big coin rain
    }

    /// <summary>Random source abstraction so tests and daily challenges can be deterministic.</summary>
    public interface IRandom
    {
        /// <summary>Returns an int in [minInclusive, maxExclusive).</summary>
        int Range(int minInclusive, int maxExclusive);

        /// <summary>Returns a double in [0, 1).</summary>
        double NextDouble();
    }

    public sealed class SystemRandom : IRandom
    {
        private readonly Random _random;

        public SystemRandom() { _random = new Random(); }
        public SystemRandom(int seed) { _random = new Random(seed); }

        public int Range(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
        public double NextDouble() => _random.NextDouble();
    }

    /// <summary>Provides "today" as a day index so economy rules can be tested.</summary>
    public interface IClock
    {
        /// <summary>Days since 1970-01-01 in the player's local time zone.</summary>
        int TodayIndex { get; }

        /// <summary>Seconds since 1970-01-01 UTC (for production timers in the city).</summary>
        long UnixSeconds { get; }
    }

    public sealed class SystemClock : IClock
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1);
        public int TodayIndex => (int)(DateTime.Now.Date - Epoch).TotalDays;
        public long UnixSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public sealed class FixedClock : IClock
    {
        public int TodayIndex { get; set; }
        public long UnixSeconds { get; set; }
        public FixedClock(int today) { TodayIndex = today; UnixSeconds = today * 86400L; }
    }
}
