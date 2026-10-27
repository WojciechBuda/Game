using System;

namespace JellyDrop.Core.Levels
{
    /// <summary>One level on the path.</summary>
    public sealed class LevelDef
    {
        public int Number;
        /// <summary>Every 10th level: no target, score as much as possible in a time limit.</summary>
        public bool IsBoost;
        public int TargetScore;
        public int PieceLimit;
        public float TimeLimitSeconds;
        public int StartSpeed;
        public int GarbageRows;
        public float ScoreMultiplier = 1f;
        public float LuckMultiplier = 1f;
        /// <summary>Scores for 1, 2 and 3 stars.</summary>
        public int[] StarScores = new int[3];
        /// <summary>
        /// Fixed piece order for this level. Every player gets the same pieces, which keeps scores fair
        /// for leaderboards and player-vs-player challenges later.
        /// </summary>
        public int Seed;
    }

    public sealed class LevelReward
    {
        public int Coins;
        public int Wood;
        public int Brick;
        public int Glass;
        public bool IsEmpty => Coins == 0 && Wood == 0 && Brick == 0 && Glass == 0;
    }

    /// <summary>
    /// Generates the endless level path. Targets grow with the level number, speed rises every few levels
    /// and junk rows appear from level 5. Every 10th level is a 90-second Boost level with big rewards.
    /// All numbers are here so the difficulty curve can be tuned in one place.
    /// </summary>
    public static class LevelPath
    {
        public const int BoostEvery = 10;
        public const float BoostSeconds = 90f;

        public static bool IsBoost(int number) => number > 0 && number % BoostEvery == 0;

        public static LevelDef Get(int number)
        {
            if (number < 1) number = 1;
            var def = new LevelDef { Number = number, Seed = 7919 * number + 104729 };

            if (IsBoost(number))
            {
                int tier = number / BoostEvery; // 1 for level 10, 2 for level 20...
                def.IsBoost = true;
                def.TimeLimitSeconds = BoostSeconds;
                def.StartSpeed = Math.Min(10, 1 + number / 8);
                def.ScoreMultiplier = 2f;
                def.LuckMultiplier = 2f;
                float scale = 1f + (tier - 1) * 0.6f;
                def.StarScores = new[] { Round50(3000 * scale), Round50(6000 * scale), Round50(10000 * scale) };
                return def;
            }

            def.TargetScore = Round50(400 + 260 * number + 8 * number * number);
            def.PieceLimit = 28 + number / 3;
            def.StartSpeed = Math.Min(12, 1 + number / 4);
            def.GarbageRows = number < 5 ? 0 : Math.Min(6, 1 + (number - 5) / 5);
            def.StarScores = new[] { def.TargetScore, Round50(def.TargetScore * 1.35f), Round50(def.TargetScore * 1.7f) };
            return def;
        }

        public static int StarsFor(LevelDef def, int score)
        {
            int stars = 0;
            foreach (var s in def.StarScores) if (score >= s) stars++;
            return stars;
        }

        /// <summary>Builds the rules for a level on top of the base config (Normal difficulty feel).</summary>
        public static GameConfig ConfigFor(LevelDef def, GameConfig baseConfig)
        {
            var c = DifficultySettings.For(Difficulty.Normal).Apply(baseConfig);
            c.LevelNumber = def.Number;
            c.StartLevel = def.StartSpeed;
            c.StartingGarbageRows = def.GarbageRows;
            c.PieceLimit = def.PieceLimit;
            c.TimeLimitSeconds = def.TimeLimitSeconds;
            c.TargetScore = def.TargetScore;
            c.ScoreMultiplier = def.ScoreMultiplier;
            c.Lucky.GoldenPieceChance *= def.LuckMultiplier;
            c.Lucky.CoinPieceChance *= def.LuckMultiplier;
            c.Lucky.RainbowChancePerClear *= def.LuckMultiplier;
            c.Lucky.JackpotChancePerClear *= def.LuckMultiplier;
            // Fewer power-ups in levels so they stay a puzzle; Boost allows more for big scores.
            c.MaxPowerUpsPerGame = def.IsBoost ? 3 : 1;
            return c;
        }

        /// <summary>
        /// Reward for finishing a level. A first clear pays in full and gives building supplies;
        /// replays pay half the coins (so replaying for stars is still worth it). A Boost pays by score.
        /// </summary>
        public static LevelReward RewardFor(LevelDef def, int score, int stars, bool firstClear)
        {
            var r = new LevelReward();
            if (def.IsBoost)
            {
                int tier = def.Number / BoostEvery;
                r.Coins = score / 20;
                r.Wood = 3 + tier + score / 2500;
                r.Brick = 2 + tier + score / 3500;
                r.Glass = 1 + tier / 2 + score / 5000;
                if (!firstClear)
                {
                    r.Coins /= 2; r.Wood /= 2; r.Brick /= 2; r.Glass /= 2;
                }
                return r;
            }

            if (stars <= 0) return r;
            r.Coins = 15 + 2 * def.Number + 8 * stars;
            if (firstClear)
            {
                r.Wood = 1 + (stars >= 2 ? 1 : 0);
                r.Brick = stars >= 3 ? 1 : 0;
                r.Glass = def.Number >= 15 && stars >= 3 ? 1 : 0;
            }
            else
            {
                r.Coins /= 2;
            }
            return r;
        }

        private static int Round50(float v) => Math.Max(50, (int)Math.Round(v / 50f) * 50);
    }
}
