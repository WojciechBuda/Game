using System;

namespace JellyDrop.Core
{
    /// <summary>Chances for the random "lucky shots". All tunable remotely later.</summary>
    [Serializable]
    public sealed class LuckyConfig
    {
        /// <summary>Chance a spawned piece is golden (clears with it multiply coins).</summary>
        public double GoldenPieceChance = 1.0 / 50.0;
        /// <summary>Chance a non-golden piece carries one coin cell.</summary>
        public double CoinPieceChance = 0.03;
        /// <summary>Chance any line clear triggers a rainbow bonus.</summary>
        public double RainbowChancePerClear = 0.08;
        /// <summary>Chance any line clear triggers the jackpot.</summary>
        public double JackpotChancePerClear = 0.004;

        public int GoldenCoinMultiplier = 5;
        public int RainbowCoins = 20;
        public int RainbowScore = 500;
        public int JackpotCoins = 250;

        public object CloneShallow() => MemberwiseClone();
    }

    /// <summary>All gameplay tunables in one place.</summary>
    [Serializable]
    public sealed class GameConfig
    {
        public int BoardWidth = 10;
        public int BoardVisibleHeight = 20;
        public int PreviewCount = 3;

        /// <summary>Row on which the lowest cell of a new piece appears.</summary>
        public int SpawnRow = 18;

        public int LinesPerLevel = 10;
        public int MaxLevel = 20;
        public int SprintLines = 40;

        public float FirstLevelFallSeconds = 0.9f;
        public float FallSpeedupPerLevel = 0.85f;
        public float MinFallSeconds = 0.04f;
        public float SoftDropMultiplier = 20f;
        public float LockDelaySeconds = 0.5f;
        public int MaxLockResets = 15;

        public float SlowMoSeconds = 15f;
        public float SlowMoFactor = 0.4f;
        public int MaxPowerUpsPerGame = 2;
        public int BombRadius = 1;

        public int[] LineClearScores = { 0, 100, 300, 500, 800 };
        public int ComboScore = 50;
        public float BackToBackMultiplier = 1.5f;
        public int SoftDropScorePerCell = 1;
        public int HardDropScorePerCell = 2;

        public int CoinsPerLine = 1;
        public int CoinsPerCoinCell = 5;

        public LuckyConfig Lucky = new LuckyConfig();

        // Difficulty-driven settings (see DifficultySettings.Apply)
        public Difficulty Difficulty = Difficulty.Normal;
        public int StartLevel = 1;
        public bool ShowGhost = true;
        public bool HoldEnabled = true;
        public int StartingGarbageRows = 0;
        public float GarbageRiseSeconds = 0f;
        public float ScoreMultiplier = 1f;
        public float CoinMultiplier = 1f;

        /// <summary>Deep copy so a difficulty can tweak values without touching the base config.</summary>
        public GameConfig Clone()
        {
            var c = (GameConfig)MemberwiseClone();
            c.LineClearScores = (int[])LineClearScores.Clone();
            c.Lucky = (LuckyConfig)Lucky.CloneShallow();
            return c;
        }

        public float FallSecondsForLevel(int level)
        {
            double seconds = FirstLevelFallSeconds * Math.Pow(FallSpeedupPerLevel, Math.Max(0, level - 1));
            return (float)Math.Max(MinFallSeconds, seconds);
        }
    }
}
