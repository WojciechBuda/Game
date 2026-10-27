using System;
using JellyDrop.Core.Economy;

namespace JellyDrop.Core.Levels
{
    public sealed class LevelOutcome
    {
        public LevelDef Level;
        public bool Won;
        public int Stars;
        /// <summary>Stars before this attempt.</summary>
        public int PreviousStars;
        public bool FirstClear;
        public LevelReward Reward = new LevelReward();
        /// <summary>Points still missing when a target level was lost.</summary>
        public int PointsShort;
    }

    /// <summary>Tracks the level path: which levels are open, stars per level, and pays level rewards.</summary>
    public sealed class ProgressService
    {
        private readonly EconomyService _economy;
        private PlayerData Data => _economy.Data;

        public ProgressService(EconomyService economy)
        {
            _economy = economy;
        }

        /// <summary>Levels cleared at least once (they are cleared in order).</summary>
        public int LevelsCleared => Data.levelStars.Count;

        /// <summary>The next level to play: the first one not yet cleared.</summary>
        public int CurrentLevel => LevelsCleared + 1;

        public bool IsUnlocked(int level) => level >= 1 && level <= CurrentLevel;

        public int StarsOf(int level) => level >= 1 && level <= Data.levelStars.Count ? Data.levelStars[level - 1] : 0;

        public int TotalStars
        {
            get
            {
                int sum = 0;
                foreach (var s in Data.levelStars) sum += s;
                return sum;
            }
        }

        public LevelOutcome Apply(GameResult result)
        {
            var def = LevelPath.Get(result.LevelNumber);
            var outcome = new LevelOutcome { Level = def, PreviousStars = StarsOf(def.Number) };
            bool firstClear = def.Number > LevelsCleared;

            if (def.IsBoost)
            {
                outcome.Won = result.Completed;
                outcome.Stars = Math.Max(1, LevelPath.StarsFor(def, result.Score));
            }
            else
            {
                outcome.Won = result.Completed && result.Score >= def.TargetScore;
                outcome.Stars = outcome.Won ? Math.Max(1, LevelPath.StarsFor(def, result.Score)) : 0;
                if (!outcome.Won) outcome.PointsShort = Math.Max(0, def.TargetScore - result.Score);
            }

            if (!outcome.Won) return outcome;

            outcome.FirstClear = firstClear;
            if (firstClear && def.Number == LevelsCleared + 1) Data.levelStars.Add(outcome.Stars);
            else if (def.Number <= Data.levelStars.Count)
                Data.levelStars[def.Number - 1] = Math.Max(Data.levelStars[def.Number - 1], outcome.Stars);

            outcome.Reward = LevelPath.RewardFor(def, result.Score, outcome.Stars, firstClear);
            var r = outcome.Reward;
            if (r.Coins > 0)
            {
                Data.coins += r.Coins;
                Data.totalCoinsEarned += r.Coins;
            }
            Data.wood += r.Wood;
            Data.brick += r.Brick;
            Data.glass += r.Glass;
            _economy.NotifyChanged();
            return outcome;
        }
    }
}
