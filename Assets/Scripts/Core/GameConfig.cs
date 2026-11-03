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

        /// <summary>Chance a piece is a small one (single or double block) instead of the next one from the bag.</summary>
        public double SmallPieceChance = 0;
        /// <summary>Chance for a big 3x3 square to slip into the queue.</summary>
        public double BigPieceChance = 0;
        /// <summary>Chance a falling bomb shows up (explodes a 3x3 area where it lands).</summary>
        public double BombPieceChance = 0;
        /// <summary>Chance of a row blast and of a column blast (each).</summary>
        public double LineBlastChance = 0;
        /// <summary>Chance a piece carries a mushroom (clear its row to pick it).</summary>
        public double MushroomPieceChance = 0;

        /// <summary>The surprise pieces used in the real game (tests keep them off for exact piece orders).</summary>
        public LuckyConfig WithSurprisePieces()
        {
            SmallPieceChance = 0.06;
            BombPieceChance = 0.02;
            LineBlastChance = 0.015;
            MushroomPieceChance = 0.04;
            return this;
        }

        /// <summary>Arcade: many more single and double blocks, plus the occasional big 3x3 square.</summary>
        public LuckyConfig WithArcadePieces()
        {
            SmallPieceChance = 0.16;
            BigPieceChance = 0.045;
            return this;
        }

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

        public float FirstLevelFallSeconds = 0.55f;
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

        // Level path settings (see Levels/LevelPath.cs). 0 = no limit / no target.
        public int LevelNumber;
        public int PieceLimit;
        public float TimeLimitSeconds;
        public int TargetScore;

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
