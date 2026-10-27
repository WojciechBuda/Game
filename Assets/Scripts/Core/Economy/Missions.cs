using System.Collections.Generic;

namespace JellyDrop.Core.Economy
{
    public enum MissionType
    {
        ClearLines,
        ClearQuads,
        PlayGames,
        ReachScore,
        CollectCoins,
        UsePowerUps,
        GoldenClears,
        MaxCombo
    }

    public sealed class MissionDef
    {
        public string Id;
        public MissionType Type;
        public int Target;
        public int RewardCoins;
        public string Title;

        /// <summary>"Best in one game" missions keep the max value instead of summing.</summary>
        public bool IsSingleGameBest => Type == MissionType.ReachScore || Type == MissionType.MaxCombo;
    }

    public static class MissionPool
    {
        public static readonly List<MissionDef> All = new List<MissionDef>
        {
            new MissionDef { Id = "m_lines_20",  Type = MissionType.ClearLines,   Target = 20,    RewardCoins = 40,  Title = "Clear 20 lines" },
            new MissionDef { Id = "m_lines_60",  Type = MissionType.ClearLines,   Target = 60,    RewardCoins = 90,  Title = "Clear 60 lines" },
            new MissionDef { Id = "m_quads_2",   Type = MissionType.ClearQuads,   Target = 2,     RewardCoins = 60,  Title = "Clear 4 lines at once twice" },
            new MissionDef { Id = "m_quads_5",   Type = MissionType.ClearQuads,   Target = 5,     RewardCoins = 120, Title = "Clear 4 lines at once 5 times" },
            new MissionDef { Id = "m_games_3",   Type = MissionType.PlayGames,    Target = 3,     RewardCoins = 30,  Title = "Play 3 games" },
            new MissionDef { Id = "m_games_6",   Type = MissionType.PlayGames,    Target = 6,     RewardCoins = 60,  Title = "Play 6 games" },
            new MissionDef { Id = "m_score_10k", Type = MissionType.ReachScore,   Target = 10000, RewardCoins = 50,  Title = "Score 10,000 in one game" },
            new MissionDef { Id = "m_score_30k", Type = MissionType.ReachScore,   Target = 30000, RewardCoins = 120, Title = "Score 30,000 in one game" },
            new MissionDef { Id = "m_coins_50",  Type = MissionType.CollectCoins, Target = 50,    RewardCoins = 40,  Title = "Collect 50 coins while playing" },
            new MissionDef { Id = "m_power_2",   Type = MissionType.UsePowerUps,  Target = 2,     RewardCoins = 40,  Title = "Use 2 power-ups" },
            new MissionDef { Id = "m_golden_1",  Type = MissionType.GoldenClears, Target = 1,     RewardCoins = 70,  Title = "Clear a line with a golden piece" },
            new MissionDef { Id = "m_combo_3",   Type = MissionType.MaxCombo,     Target = 3,     RewardCoins = 60,  Title = "Make a 3x combo" },
        };

        public static MissionDef Get(string id)
        {
            foreach (var m in All) if (m.Id == id) return m;
            return null;
        }

        /// <summary>Picks <paramref name="count"/> different missions for a day, deterministic per day.</summary>
        public static List<MissionDef> PickForDay(int dayIndex, int count)
        {
            var rng = new SystemRandom(dayIndex * 7919 + 13);
            var pool = new List<MissionDef>(All);
            var picked = new List<MissionDef>();
            var usedTypes = new HashSet<MissionType>();
            while (picked.Count < count && pool.Count > 0)
            {
                int i = rng.Range(0, pool.Count);
                var m = pool[i];
                pool.RemoveAt(i);
                if (usedTypes.Add(m.Type)) picked.Add(m);
            }
            return picked;
        }

        public static int ValueFrom(MissionType type, GameResult result)
        {
            switch (type)
            {
                case MissionType.ClearLines: return result.Lines;
                case MissionType.ClearQuads: return result.Stats.Quads;
                case MissionType.PlayGames: return 1;
                case MissionType.ReachScore: return result.Score;
                case MissionType.CollectCoins: return result.CoinsCollected;
                case MissionType.UsePowerUps: return result.Stats.PowerUpsUsed;
                case MissionType.GoldenClears: return result.Stats.GoldenClears;
                case MissionType.MaxCombo: return result.Stats.MaxCombo;
                default: return 0;
            }
        }
    }
}
