using System;
using System.Collections.Generic;
using JellyDrop.Core.Economy;

namespace JellyDrop.Core.Islands
{
    public enum ForestBuilding { MushroomHut = 0, LumberCamp = 1, Apiary = 2, Watchtower = 3, Herbalist = 4 }

    public enum ForestPick { None, Mushroom, GoldMushroom }

    /// <summary>One forest building: what it costs per level and when it unlocks.</summary>
    public sealed class ForestBuildingDef
    {
        public ForestBuilding Id;
        public int UnlockLevel;
        public int[] Coins, Wood, Mushrooms;   // per level (index 0 = level 1)
        public int MaxLevel => Coins.Length;
    }

    public static class ForestCatalog
    {
        /// <summary>Most mushroom spots the forest can have (when every area is open and the hut is maxed).</summary>
        public const int MaxSpots = 20;
        public const int Trees = 10;
        public const int Bushes = 6;
        public const long MushroomRegrowSeconds = 6 * 60;
        public const long TreeRegrowSeconds = 20 * 60;
        public const long BushRegrowSeconds = 15 * 60;
        public const int BerriesPerBush = 3;
        public const int HoneyCapHours = 8;
        public const long FoxMinSeconds = 25 * 60, FoxMaxSeconds = 50 * 60;

        /// <summary>Forest XP needed for each level (index = level - 1).</summary>
        public static readonly int[] LevelXp = { 0, 30, 80, 160, 280, 450, 680, 1000, 1400, 2000 };
        public static int MaxLevel => LevelXp.Length;

        /// <summary>The forest grows: new areas with more mushroom spots, trees and bushes.</summary>
        public static readonly string[] Areas = { "clearing", "thicket", "old_forest", "magic_glade" };
        public static readonly int[] AreaLevel = { 1, 3, 5, 8 };

        public static readonly ForestBuildingDef[] Buildings =
        {
            new ForestBuildingDef { Id = ForestBuilding.MushroomHut, UnlockLevel = 1, Coins = new[] { 150, 400, 900 },  Wood = new[] { 10, 25, 50 }, Mushrooms = new[] { 0, 8, 20 } },
            new ForestBuildingDef { Id = ForestBuilding.LumberCamp,  UnlockLevel = 2, Coins = new[] { 200, 500, 1100 }, Wood = new[] { 15, 30, 60 }, Mushrooms = new[] { 0, 5, 12 } },
            new ForestBuildingDef { Id = ForestBuilding.Apiary,      UnlockLevel = 3, Coins = new[] { 300, 700, 1500 }, Wood = new[] { 20, 40, 70 }, Mushrooms = new[] { 5, 12, 25 } },
            new ForestBuildingDef { Id = ForestBuilding.Watchtower,  UnlockLevel = 4, Coins = new[] { 400, 900, 2000 }, Wood = new[] { 30, 55, 90 }, Mushrooms = new[] { 6, 15, 30 } },
            new ForestBuildingDef { Id = ForestBuilding.Herbalist,   UnlockLevel = 5, Coins = new[] { 500, 1200, 2500 }, Wood = new[] { 25, 50, 80 }, Mushrooms = new[] { 10, 20, 40 } },
        };

        public static ForestBuildingDef Get(ForestBuilding b) => Buildings[(int)b];
    }

    public enum ForestBuildResult { Ok, Locked, MaxLevel, NotEnoughCoins, NotEnoughWood, NotEnoughMushrooms }

    /// <summary>
    /// The forest island. Mushrooms grow back on their spots in real time (sometimes golden), trees can be
    /// chopped for wood and regrow, bushes give berries. Gathering gives forest XP; levels open new areas
    /// (thicket, old forest, magic glade) with more spots. Forest buildings make it better: the mushroom hut
    /// adds spots and speeds regrowth, the lumber camp gives more wood, the apiary makes honey, the watchtower
    /// finds more golden mushrooms and the herbalist brews power-ups from mushrooms. Now and then a fox runs
    /// off with a basket of mushrooms: catch it to get them.
    /// </summary>
    public sealed class ForestService
    {
        private readonly EconomyService _economy;
        private PlayerData Data => _economy.Data;
        private long Now => _economy.Clock.UnixSeconds;
        private readonly Random _rng;

        public ForestService(EconomyService economy, int seed = 0)
        {
            _economy = economy;
            _rng = new Random(seed == 0 ? Environment.TickCount : seed);
            Fit(Data.forestSpotReady, ForestCatalog.MaxSpots);
            Fit(Data.forestSpotGold, ForestCatalog.MaxSpots);
            Fit(Data.forestTreeReady, ForestCatalog.Trees);
            Fit(Data.forestBushReady, ForestCatalog.Bushes);
            Fit(Data.forestBuildings, ForestCatalog.Buildings.Length);
            if (Data.forestFoxNext == 0) Data.forestFoxNext = Now + 10 * 60;
        }

        private static void Fit(List<long> list, int n) { while (list.Count < n) list.Add(0); }
        private static void Fit(List<int> list, int n) { while (list.Count < n) list.Add(0); }

        // ------------------------------------------------------------------ levels and areas

        public int Xp => Data.forestXp;

        public int Level
        {
            get
            {
                int level = 1;
                for (int i = 1; i < ForestCatalog.LevelXp.Length; i++) if (Data.forestXp >= ForestCatalog.LevelXp[i]) level = i + 1;
                return level;
            }
        }

        /// <summary>XP needed for the next level (0 at max).</summary>
        public int NextLevelXp => Level >= ForestCatalog.MaxLevel ? 0 : ForestCatalog.LevelXp[Level];
        public float LevelProgress
        {
            get
            {
                if (Level >= ForestCatalog.MaxLevel) return 1f;
                int from = ForestCatalog.LevelXp[Level - 1], to = ForestCatalog.LevelXp[Level];
                return Math.Max(0f, Math.Min(1f, (Data.forestXp - from) / (float)(to - from)));
            }
        }

        public int AreasOpen
        {
            get
            {
                int n = 0;
                foreach (var l in ForestCatalog.AreaLevel) if (Level >= l) n++;
                return n;
            }
        }

        /// <summary>Area each mushroom spot belongs to (spots of locked areas don't grow).</summary>
        public static int AreaOfSpot(int spot) => spot < 6 ? 0 : spot < 11 ? 1 : spot < 16 ? 2 : 3;
        public static int AreaOfTree(int tree) => tree < 4 ? 0 : tree < 7 ? 1 : tree < 9 ? 2 : 3;
        public static int AreaOfBush(int bush) => bush < 2 ? 0 : bush < 4 ? 1 : bush < 5 ? 2 : 3;

        public bool IsSpotOpen(int spot)
        {
            if (spot < 0 || spot >= ForestCatalog.MaxSpots) return false;
            if (AreaOfSpot(spot) >= AreasOpen) return false;
            // The mushroom hut adds the last spot of each area.
            bool hutSpot = spot == 5 || spot == 10 || spot == 15;
            return !hutSpot || BuildingLevel(ForestBuilding.MushroomHut) >= 1;
        }

        public bool IsTreeOpen(int tree) => tree >= 0 && tree < ForestCatalog.Trees && AreaOfTree(tree) < AreasOpen;
        public bool IsBushOpen(int bush) => bush >= 0 && bush < ForestCatalog.Bushes && AreaOfBush(bush) < AreasOpen;

        private void AddXp(int xp)
        {
            int before = Level;
            Data.forestXp += xp;
            if (Level > before) LeveledUp?.Invoke(Level);
        }

        public event Action<int> LeveledUp;

        // ------------------------------------------------------------------ mushrooms

        public long MushroomRegrowSeconds =>
            (long)(ForestCatalog.MushroomRegrowSeconds * (1.0 - 0.15 * BuildingLevel(ForestBuilding.MushroomHut)));

        public double GoldChance => 0.06 + 0.04 * BuildingLevel(ForestBuilding.Watchtower) + (AreasOpen >= 4 ? 0.08 : 0);

        public bool IsSpotReady(int spot) => IsSpotOpen(spot) && Now >= Data.forestSpotReady[spot];

        /// <summary>Is the mushroom on this spot golden? (decided when it grows)</summary>
        public bool SpotIsGold(int spot) => Data.forestSpotGold[spot] == 1;

        public int ReadyMushrooms
        {
            get
            {
                int n = 0;
                for (int i = 0; i < ForestCatalog.MaxSpots; i++) if (IsSpotReady(i)) n++;
                return n;
            }
        }

        public ForestPick PickSpot(int spot)
        {
            if (!IsSpotReady(spot)) return ForestPick.None;
            bool gold = SpotIsGold(spot);
            if (gold) Data.goldMushrooms++; else Data.mushrooms++;
            AddXp(gold ? 10 : 2);
            // Grows back later; roll now whether the next one will be golden.
            Data.forestSpotReady[spot] = Now + MushroomRegrowSeconds + _rng.Next(90);
            Data.forestSpotGold[spot] = _rng.NextDouble() < GoldChance ? 1 : 0;
            _economy.NotifyChanged();
            return gold ? ForestPick.GoldMushroom : ForestPick.Mushroom;
        }

        public long SpotSecondsLeft(int spot) => Math.Max(0, Data.forestSpotReady[spot] - Now);

        // ------------------------------------------------------------------ trees and bushes

        public int WoodPerTree => 2 + BuildingLevel(ForestBuilding.LumberCamp);
        public long TreeRegrowSeconds => (long)(ForestCatalog.TreeRegrowSeconds * (1.0 - 0.2 * BuildingLevel(ForestBuilding.LumberCamp)));

        public bool IsTreeReady(int tree) => IsTreeOpen(tree) && Now >= Data.forestTreeReady[tree];

        /// <summary>Chops a tree: wood now, a sapling regrows. Returns the wood (0 if not ready).</summary>
        public int Chop(int tree)
        {
            if (!IsTreeReady(tree)) return 0;
            int wood = WoodPerTree;
            Data.wood += wood;
            Data.forestTreeReady[tree] = Now + TreeRegrowSeconds;
            AddXp(3);
            _economy.NotifyChanged();
            return wood;
        }

        /// <summary>How grown a chopped tree is again (0..1).</summary>
        public float TreeGrowth(int tree)
        {
            long left = Data.forestTreeReady[tree] - Now;
            return left <= 0 ? 1f : 1f - left / (float)Math.Max(1, TreeRegrowSeconds);
        }

        public bool IsBushReady(int bush) => IsBushOpen(bush) && Now >= Data.forestBushReady[bush];

        public int PickBush(int bush)
        {
            if (!IsBushReady(bush)) return 0;
            Data.berries += ForestCatalog.BerriesPerBush;
            Data.forestBushReady[bush] = Now + ForestCatalog.BushRegrowSeconds;
            AddXp(2);
            _economy.NotifyChanged();
            return ForestCatalog.BerriesPerBush;
        }

        // ------------------------------------------------------------------ buildings

        public int BuildingLevel(ForestBuilding b) => Data.forestBuildings[(int)b];

        public ForestBuildResult CanUpgrade(ForestBuilding b)
        {
            var def = ForestCatalog.Get(b);
            int level = BuildingLevel(b);
            if (Level < def.UnlockLevel) return ForestBuildResult.Locked;
            if (level >= def.MaxLevel) return ForestBuildResult.MaxLevel;
            if (Data.coins < def.Coins[level]) return ForestBuildResult.NotEnoughCoins;
            if (Data.wood < def.Wood[level]) return ForestBuildResult.NotEnoughWood;
            if (Data.mushrooms < def.Mushrooms[level]) return ForestBuildResult.NotEnoughMushrooms;
            return ForestBuildResult.Ok;
        }

        /// <summary>Builds level 1 or upgrades (instant: the forest folk are quick).</summary>
        public ForestBuildResult Upgrade(ForestBuilding b)
        {
            var r = CanUpgrade(b);
            if (r != ForestBuildResult.Ok) return r;
            var def = ForestCatalog.Get(b);
            int level = BuildingLevel(b);
            Data.coins -= def.Coins[level];
            Data.wood -= def.Wood[level];
            Data.mushrooms -= def.Mushrooms[level];
            if (b == ForestBuilding.Apiary && level == 0) Data.forestHoneyFrom = Now;
            Data.forestBuildings[(int)b] = level + 1;
            AddXp(15 * (level + 1));
            _economy.NotifyChanged();
            return ForestBuildResult.Ok;
        }

        // ------------------------------------------------------------------ apiary and herbalist

        public int HoneyPerHour => 2 * BuildingLevel(ForestBuilding.Apiary);

        public int HoneyPending
        {
            get
            {
                if (HoneyPerHour <= 0) return 0;
                double hours = Math.Min(ForestCatalog.HoneyCapHours, (Now - Data.forestHoneyFrom) / 3600.0);
                return (int)(hours * HoneyPerHour);
            }
        }

        public int CollectHoney()
        {
            int honey = HoneyPending;
            if (honey <= 0) return 0;
            Data.honey += honey;
            Data.forestHoneyFrom = Now;
            AddXp(honey);
            _economy.NotifyChanged();
            return honey;
        }

        /// <summary>Mushrooms the herbalist needs for one brew (cheaper with each level).</summary>
        public int BrewCost => Math.Max(3, 7 - BuildingLevel(ForestBuilding.Herbalist));

        /// <summary>Brews a random power-up from mushrooms. Returns null if it can't.</summary>
        public PowerUpType? Brew()
        {
            if (BuildingLevel(ForestBuilding.Herbalist) <= 0 || Data.mushrooms < BrewCost) return null;
            Data.mushrooms -= BrewCost;
            var type = (PowerUpType)_rng.Next(4);
            _economy.AddPowerUp(type, 1);
            AddXp(5);
            _economy.NotifyChanged();
            return type;
        }

        // ------------------------------------------------------------------ the fox

        public bool FoxReady => Level >= 2 && Now >= Data.forestFoxNext;

        public const int FoxMushrooms = 5;

        /// <summary>Ends a fox chase. Caught: the basket (5 mushrooms, sometimes a golden one). Returns mushrooms won.</summary>
        public int FinishFox(bool caught)
        {
            Data.forestFoxNext = Now + ForestCatalog.FoxMinSeconds + _rng.Next((int)(ForestCatalog.FoxMaxSeconds - ForestCatalog.FoxMinSeconds));
            int won = 0;
            if (caught)
            {
                won = FoxMushrooms;
                Data.mushrooms += won;
                if (_rng.NextDouble() < 0.3) Data.goldMushrooms++;
                AddXp(12);
            }
            _economy.NotifyChanged();
            return won;
        }
    }
}
