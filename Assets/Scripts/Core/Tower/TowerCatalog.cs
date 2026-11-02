using System;
using JellyDrop.Core.Economy;

namespace JellyDrop.Core.Tower
{
    /// <summary>A tower with a fixed number of floors and its own obstacles.</summary>
    public sealed class TowerDef
    {
        public int Index;
        public int Floors;
        public TowerScenery Scenery;
        public int Reward;          // coins for the first climb
        public int ReplayReward;    // coins for climbing it again
        public string Hazards;      // short keys for the UI: "ice", "moving", "crumble", "spring", "fast", "narrow"

        public TowerConfig Config()
        {
            var c = new TowerConfig { Floors = Floors, Scenery = Scenery, CycleScenery = false };
            float k = Index / (float)(TowerCatalog.Count - 1);   // 0..1 difficulty
            c.ScrollStart = 0.9f + 0.6f * k;
            c.ScrollAccel = 0.012f + 0.012f * k;
            c.ScrollMax = 2.6f + 0.9f * k;
            c.MaxWidth = 5.2f - 1.2f * k;
            c.MinWidth = 3.2f - 0.8f * k;
            c.NarrowAfter = Floors;
            if (Hazards.Contains("ice")) c.IceChance = 0.25f + 0.15f * k;
            if (Hazards.Contains("moving")) c.MovingChance = 0.18f + 0.12f * k;
            if (Hazards.Contains("crumble")) c.CrumbleChance = 0.15f + 0.1f * k;
            if (Hazards.Contains("spring")) c.SpringChance = 0.12f;
            if (Hazards.Contains("fast")) { c.ScrollStart += 0.4f; c.ScrollMax += 0.4f; }
            if (Hazards.Contains("narrow")) { c.MaxWidth -= 0.6f; c.MinWidth = Math.Max(2.1f, c.MinWidth - 0.5f); }
            return c;
        }
    }

    /// <summary>The list of climbable towers (unlocked one after another) and the endless rewards.</summary>
    public static class TowerCatalog
    {
        public const int Count = 12;

        private static readonly string[] HazardSets =
        {
            "", "ice", "moving", "crumble", "spring ice", "moving crumble",
            "fast", "narrow ice", "moving spring", "crumble narrow", "ice moving fast", "crumble moving narrow spring",
        };

        private static readonly int[] FloorCounts = { 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100 };

        public static TowerDef Get(int index)
        {
            index = Math.Max(0, Math.Min(Count - 1, index));
            return new TowerDef
            {
                Index = index,
                Floors = FloorCounts[index],
                Scenery = (TowerScenery)(index % TowerSession.SceneryCount),
                Hazards = HazardSets[index],
                Reward = 60 + 30 * index,
                ReplayReward = 10 + 4 * index,
            };
        }

        /// <summary>Coins for an endless climb: one per 5 floors plus the combo score / 50.</summary>
        public static int EndlessCoins(int floors, int comboScore) => floors / 5 + comboScore / 50;
    }

    /// <summary>Saves progress for the tower climbs and pays the rewards.</summary>
    public sealed class TowerService
    {
        private readonly EconomyService _economy;
        private PlayerData Data => _economy.Data;

        public TowerService(EconomyService economy)
        {
            _economy = economy;
            while (Data.towerBest.Count < 3) Data.towerBest.Add(0);
        }

        /// <summary>How many towers are climbed (the next one is unlocked).</summary>
        public int Cleared => Data.towerCleared;
        public bool IsUnlocked(int index) => index <= Data.towerCleared;
        public bool IsCleared(int index) => index < Data.towerCleared;
        public int Best(TowerMode mode) => Data.towerBest[(int)mode];
        public TowerScenery Scenery
        {
            get => (TowerScenery)Data.towerScenery;
            set { Data.towerScenery = (int)value; _economy.NotifyChanged(); }
        }

        /// <summary>Endless climb finished: pays coins, returns them; updates the best floor.</summary>
        public int FinishEndless(TowerMode mode, int floors, int comboScore, out bool record)
        {
            record = floors > Data.towerBest[(int)mode];
            if (record) Data.towerBest[(int)mode] = floors;
            int coins = TowerCatalog.EndlessCoins(floors, comboScore);
            Pay(coins);
            return coins;
        }

        /// <summary>A tower climb finished. Returns the coins paid (0 when lost).</summary>
        public int FinishTower(int index, bool won, out bool firstClear)
        {
            firstClear = false;
            if (!won) return 0;
            var def = TowerCatalog.Get(index);
            int coins;
            if (index >= Data.towerCleared)
            {
                firstClear = true;
                Data.towerCleared = Math.Min(TowerCatalog.Count, index + 1);
                coins = def.Reward;
            }
            else coins = def.ReplayReward;
            Pay(coins);
            return coins;
        }

        /// <summary>
        /// Pays what was picked up during a climb (coins and supplies; energy goes to the pet, see the caller).
        /// </summary>
        public void PayItems(int[] collected)
        {
            Data.coins += collected[(int)TowerItemKind.Coin];
            Data.totalCoinsEarned += collected[(int)TowerItemKind.Coin];
            Data.wood += collected[(int)TowerItemKind.Wood];
            Data.brick += collected[(int)TowerItemKind.Brick];
            Data.glass += collected[(int)TowerItemKind.Glass];
            if (collected.Length > (int)TowerItemKind.Mushroom) Data.mushrooms += collected[(int)TowerItemKind.Mushroom];
            _economy.NotifyChanged();
        }

        private void Pay(int coins)
        {
            Data.coins += coins;
            Data.totalCoinsEarned += coins;
            _economy.NotifyChanged();
        }
    }
}
