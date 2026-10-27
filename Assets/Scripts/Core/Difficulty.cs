using System;

namespace JellyDrop.Core
{
    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Insane = 3
    }

    /// <summary>
    /// Everything a difficulty changes. Harder levels are faster, give less time to slide pieces,
    /// show fewer previews, may start with junk rows, and pay more coins to make them worth it.
    /// </summary>
    [Serializable]
    public sealed class DifficultySettings
    {
        public Difficulty Level;
        public string Name;
        public string Description;
        /// <summary>Hex colour used for this difficulty in the UI.</summary>
        public string Color;

        /// <summary>Speed level the game starts at (1 = slowest).</summary>
        public int StartLevel;
        /// <summary>Fall time per row at level 1, in seconds.</summary>
        public float FirstLevelFallSeconds;
        /// <summary>Fall time is multiplied by this every level (smaller = speeds up faster).</summary>
        public float FallSpeedupPerLevel;
        public float LockDelaySeconds;
        public int MaxLockResets;
        public int PreviewCount;
        public bool ShowGhost;
        public bool HoldEnabled;
        /// <summary>Rows of junk (one gap each) already on the board when the game starts.</summary>
        public int StartingGarbageRows;
        /// <summary>Every N seconds a junk row pushes up from the bottom; 0 = never.</summary>
        public float GarbageRiseSeconds;
        public int MaxPowerUpsPerGame;
        /// <summary>Applied to score points.</summary>
        public float ScoreMultiplier;
        /// <summary>Applied to all coins earned in the game (lines, lucky shots, end bonus).</summary>
        public float CoinMultiplier;
        /// <summary>Lucky chances (golden, rainbow, jackpot) are multiplied by this.</summary>
        public float LuckMultiplier;

        public static DifficultySettings For(Difficulty d)
        {
            switch (d)
            {
                case Difficulty.Easy:
                    return new DifficultySettings
                    {
                        Level = d, Name = "EASY", Description = "Slow and relaxed. Extra previews, more power-ups.", Color = "#2ED3A0",
                        StartLevel = 1, FirstLevelFallSeconds = 1.1f, FallSpeedupPerLevel = 0.9f,
                        LockDelaySeconds = 0.8f, MaxLockResets = 25, PreviewCount = 3, ShowGhost = true, HoldEnabled = true,
                        StartingGarbageRows = 0, GarbageRiseSeconds = 0f, MaxPowerUpsPerGame = 4,
                        ScoreMultiplier = 0.75f, CoinMultiplier = 0.75f, LuckMultiplier = 1.2f
                    };
                case Difficulty.Hard:
                    return new DifficultySettings
                    {
                        Level = d, Name = "HARD", Description = "Fast start, junk rows, less time to slide. x1.5 coins.", Color = "#FF9F43",
                        StartLevel = 5, FirstLevelFallSeconds = 0.9f, FallSpeedupPerLevel = 0.82f,
                        LockDelaySeconds = 0.4f, MaxLockResets = 10, PreviewCount = 2, ShowGhost = true, HoldEnabled = true,
                        StartingGarbageRows = 4, GarbageRiseSeconds = 0f, MaxPowerUpsPerGame = 1,
                        ScoreMultiplier = 1.5f, CoinMultiplier = 1.5f, LuckMultiplier = 1f
                    };
                case Difficulty.Insane:
                    return new DifficultySettings
                    {
                        Level = d, Name = "INSANE", Description = "No ghost, one preview, rising junk, no power-ups. x2.5 coins!", Color = "#FF5A6E",
                        StartLevel = 10, FirstLevelFallSeconds = 0.9f, FallSpeedupPerLevel = 0.8f,
                        LockDelaySeconds = 0.3f, MaxLockResets = 6, PreviewCount = 1, ShowGhost = false, HoldEnabled = true,
                        StartingGarbageRows = 6, GarbageRiseSeconds = 12f, MaxPowerUpsPerGame = 0,
                        ScoreMultiplier = 2.5f, CoinMultiplier = 2.5f, LuckMultiplier = 0.8f
                    };
                default:
                    return new DifficultySettings
                    {
                        Level = Difficulty.Normal, Name = "NORMAL", Description = "The classic experience.", Color = "#3FB6FF",
                        StartLevel = 1, FirstLevelFallSeconds = 0.9f, FallSpeedupPerLevel = 0.85f,
                        LockDelaySeconds = 0.5f, MaxLockResets = 15, PreviewCount = 3, ShowGhost = true, HoldEnabled = true,
                        StartingGarbageRows = 0, GarbageRiseSeconds = 0f, MaxPowerUpsPerGame = 2,
                        ScoreMultiplier = 1f, CoinMultiplier = 1f, LuckMultiplier = 1f
                    };
            }
        }

        /// <summary>Builds the game config for this difficulty on top of a base config.</summary>
        public GameConfig Apply(GameConfig baseConfig)
        {
            var c = baseConfig.Clone();
            c.Difficulty = Level;
            c.StartLevel = StartLevel;
            c.FirstLevelFallSeconds = FirstLevelFallSeconds;
            c.FallSpeedupPerLevel = FallSpeedupPerLevel;
            c.LockDelaySeconds = LockDelaySeconds;
            c.MaxLockResets = MaxLockResets;
            c.PreviewCount = PreviewCount;
            c.ShowGhost = ShowGhost;
            c.HoldEnabled = HoldEnabled;
            c.StartingGarbageRows = StartingGarbageRows;
            c.GarbageRiseSeconds = GarbageRiseSeconds;
            c.MaxPowerUpsPerGame = MaxPowerUpsPerGame;
            c.ScoreMultiplier = ScoreMultiplier;
            c.CoinMultiplier = CoinMultiplier;
            c.Lucky.GoldenPieceChance *= LuckMultiplier;
            c.Lucky.CoinPieceChance *= LuckMultiplier;
            c.Lucky.RainbowChancePerClear *= LuckMultiplier;
            c.Lucky.JackpotChancePerClear *= LuckMultiplier;
            return c;
        }

        public static readonly Difficulty[] All = { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard, Difficulty.Insane };
    }
}
