using System;
using System.Collections.Generic;
using JellyDrop.Core.Economy;

namespace JellyDrop.Core.Islands
{
    public enum CropType { Carrot = 0, Tomato = 1, Strawberry = 2, Pumpkin = 3, Sunflower = 4 }

    public enum GardenBuilding { Greenhouse = 0, Well = 1, Compost = 2, Coop = 3 }

    public sealed class CropDef
    {
        public CropType Type;
        public int SeedCost;        // coins
        public long GrowSeconds;
        public int Yield;
        public int UnlockLevel;
        public int SellPrice;       // coins each at the stall
    }

    public sealed class GardenBuildingDef
    {
        public GardenBuilding Id;
        public int UnlockLevel;
        public int[] Coins, Wood, Brick;   // per level
        public int MaxLevel => Coins.Length;
    }

    /// <summary>A dish from the kitchen: what it takes and what it gives.</summary>
    public sealed class Recipe
    {
        public int Index;
        public string Id;
        public int[] Crops = new int[5];
        public int Mushrooms, Eggs, Honey;
    }

    public static class GardenCatalog
    {
        public const int MaxBeds = 12;
        public const int StartBeds = 4;
        public const int EggCapHours = 8;
        public const int CompostMushrooms = 3;
        public const long CrowsMinSeconds = 20 * 60, CrowsMaxSeconds = 45 * 60;
        public const float WaterSpeedUp = 0.3f;

        public static readonly int[] LevelXp = { 0, 25, 70, 140, 240, 380, 560, 800, 1100, 1500 };
        public static int MaxLevel => LevelXp.Length;

        public static readonly CropDef[] Crops =
        {
            new CropDef { Type = CropType.Carrot,     SeedCost = 10, GrowSeconds = 3 * 60,  Yield = 3, UnlockLevel = 1, SellPrice = 6 },
            new CropDef { Type = CropType.Tomato,     SeedCost = 20, GrowSeconds = 8 * 60,  Yield = 3, UnlockLevel = 2, SellPrice = 11 },
            new CropDef { Type = CropType.Strawberry, SeedCost = 30, GrowSeconds = 15 * 60, Yield = 4, UnlockLevel = 3, SellPrice = 12 },
            new CropDef { Type = CropType.Pumpkin,    SeedCost = 50, GrowSeconds = 40 * 60, Yield = 2, UnlockLevel = 4, SellPrice = 45 },
            new CropDef { Type = CropType.Sunflower,  SeedCost = 25, GrowSeconds = 20 * 60, Yield = 3, UnlockLevel = 5, SellPrice = 14 },
        };

        public static readonly GardenBuildingDef[] Buildings =
        {
            new GardenBuildingDef { Id = GardenBuilding.Greenhouse, UnlockLevel = 2, Coins = new[] { 300, 800, 1800 }, Wood = new[] { 10, 25, 45 }, Brick = new[] { 5, 15, 30 } },
            new GardenBuildingDef { Id = GardenBuilding.Well,       UnlockLevel = 1, Coins = new[] { 150, 450, 1000 }, Wood = new[] { 8, 20, 35 },  Brick = new[] { 8, 18, 30 } },
            new GardenBuildingDef { Id = GardenBuilding.Compost,    UnlockLevel = 3, Coins = new[] { 200, 500, 1100 }, Wood = new[] { 15, 30, 50 }, Brick = new[] { 0, 5, 12 } },
            new GardenBuildingDef { Id = GardenBuilding.Coop,       UnlockLevel = 2, Coins = new[] { 250, 650, 1400 }, Wood = new[] { 20, 35, 60 }, Brick = new[] { 2, 8, 16 } },
        };

        public static readonly Recipe[] Recipes =
        {
            new Recipe { Index = 0, Id = "veggie_soup",     Crops = new[] { 2, 1, 0, 0, 0 }, Mushrooms = 1 },
            new Recipe { Index = 1, Id = "omelette",        Crops = new[] { 0, 1, 0, 0, 0 }, Mushrooms = 1, Eggs = 2 },
            new Recipe { Index = 2, Id = "strawberry_cake", Crops = new[] { 0, 0, 3, 0, 0 }, Eggs = 2, Honey = 1 },
            new Recipe { Index = 3, Id = "pumpkin_pie",     Crops = new[] { 0, 0, 0, 1, 0 }, Eggs = 2, Honey = 1 },
            new Recipe { Index = 4, Id = "sunflower_bread", Crops = new[] { 0, 0, 0, 0, 3 }, Eggs = 1 },
        };

        public static CropDef Crop(CropType t) => Crops[(int)t];
        public static GardenBuildingDef Get(GardenBuilding b) => Buildings[(int)b];
    }

    public enum GardenResult { Ok, Locked, BedBusy, NotReady, NotEnoughCoins, NotEnoughWood, NotEnoughBrick, MaxLevel, Missing }

    /// <summary>
    /// The garden island. Plant seeds in beds; crops grow in real time (watering speeds them up, compost gives
    /// an extra crop) and are harvested by tapping. Harvests give garden XP, which opens more beds and crops.
    /// Garden buildings: the greenhouse makes everything grow faster, the well waters all beds at once, the
    /// compost bin turns mushrooms into compost and the chicken coop lays eggs. The kitchen cooks dishes from
    /// crops, mushrooms, eggs and honey, each with its own bonus. Crows come now and then: chase them away.
    /// </summary>
    public sealed class GardenService
    {
        private readonly EconomyService _economy;
        private PlayerData Data => _economy.Data;
        private long Now => _economy.Clock.UnixSeconds;
        private readonly Random _rng;

        public GardenService(EconomyService economy, int seed = 0)
        {
            _economy = economy;
            _rng = new Random(seed == 0 ? Environment.TickCount : seed);
            Fit(Data.gardenBedCrop, GardenCatalog.MaxBeds, -1);
            Fit(Data.gardenBedReady, GardenCatalog.MaxBeds);
            Fit(Data.gardenBedPlanted, GardenCatalog.MaxBeds);
            Fit(Data.gardenBedWatered, GardenCatalog.MaxBeds, 0);
            Fit(Data.gardenBedCompost, GardenCatalog.MaxBeds, 0);
            Fit(Data.crops, 5, 0);
            Fit(Data.gardenBuildings, GardenCatalog.Buildings.Length, 0);
            if (Data.gardenCrowsNext == 0) Data.gardenCrowsNext = Now + 15 * 60;
        }

        private static void Fit(List<long> l, int n) { while (l.Count < n) l.Add(0); }
        private static void Fit(List<int> l, int n, int v) { while (l.Count < n) l.Add(v); }

        // ------------------------------------------------------------------ levels

        public int Xp => Data.gardenXp;

        public int Level
        {
            get
            {
                int level = 1;
                for (int i = 1; i < GardenCatalog.LevelXp.Length; i++) if (Data.gardenXp >= GardenCatalog.LevelXp[i]) level = i + 1;
                return level;
            }
        }

        public int NextLevelXp => Level >= GardenCatalog.MaxLevel ? 0 : GardenCatalog.LevelXp[Level];

        public float LevelProgress
        {
            get
            {
                if (Level >= GardenCatalog.MaxLevel) return 1f;
                int from = GardenCatalog.LevelXp[Level - 1], to = GardenCatalog.LevelXp[Level];
                return Math.Max(0f, Math.Min(1f, (Data.gardenXp - from) / (float)(to - from)));
            }
        }

        public event Action<int> LeveledUp;

        private void AddXp(int xp)
        {
            int before = Level;
            Data.gardenXp += xp;
            if (Level > before) LeveledUp?.Invoke(Level);
        }

        public int OpenBeds => Math.Min(GardenCatalog.MaxBeds, GardenCatalog.StartBeds + 2 * (Level - 1));
        public bool IsBedOpen(int bed) => bed >= 0 && bed < OpenBeds;
        public bool IsCropUnlocked(CropType t) => Level >= GardenCatalog.Crop(t).UnlockLevel;

        // ------------------------------------------------------------------ beds

        public CropType? CropIn(int bed) => Data.gardenBedCrop[bed] < 0 ? (CropType?)null : (CropType)Data.gardenBedCrop[bed];
        public bool IsEmpty(int bed) => Data.gardenBedCrop[bed] < 0;
        public bool IsRipe(int bed) => !IsEmpty(bed) && Now >= Data.gardenBedReady[bed];
        public bool IsWatered(int bed) => Data.gardenBedWatered[bed] == 1;
        public bool HasCompost(int bed) => Data.gardenBedCompost[bed] == 1;
        public long SecondsLeft(int bed) => IsEmpty(bed) ? 0 : Math.Max(0, Data.gardenBedReady[bed] - Now);

        /// <summary>0..1 growth of the crop in a bed.</summary>
        public float Growth(int bed)
        {
            if (IsEmpty(bed)) return 0f;
            long total = Math.Max(1, Data.gardenBedReady[bed] - Data.gardenBedPlanted[bed]);
            return Math.Max(0f, Math.Min(1f, (Now - Data.gardenBedPlanted[bed]) / (float)total));
        }

        public long GrowSeconds(CropType t) =>
            (long)(GardenCatalog.Crop(t).GrowSeconds * (1.0 - 0.2 * BuildingLevel(GardenBuilding.Greenhouse)));

        public GardenResult Plant(int bed, CropType t)
        {
            if (!IsBedOpen(bed) || !IsCropUnlocked(t)) return GardenResult.Locked;
            if (!IsEmpty(bed)) return GardenResult.BedBusy;
            var def = GardenCatalog.Crop(t);
            if (Data.coins < def.SeedCost) return GardenResult.NotEnoughCoins;
            Data.coins -= def.SeedCost;
            Data.gardenBedCrop[bed] = (int)t;
            Data.gardenBedPlanted[bed] = Now;
            Data.gardenBedReady[bed] = Now + GrowSeconds(t);
            Data.gardenBedWatered[bed] = 0;
            // Compost (if any) is dug in automatically: one extra crop.
            Data.gardenBedCompost[bed] = 0;
            if (Data.compost > 0) { Data.compost--; Data.gardenBedCompost[bed] = 1; }
            _economy.NotifyChanged();
            return GardenResult.Ok;
        }

        /// <summary>Watering takes 30% off the time that is left (once per planting).</summary>
        public GardenResult Water(int bed)
        {
            if (!IsBedOpen(bed) || IsEmpty(bed) || IsRipe(bed)) return GardenResult.NotReady;
            if (IsWatered(bed)) return GardenResult.BedBusy;
            long left = Data.gardenBedReady[bed] - Now;
            Data.gardenBedReady[bed] = Now + (long)(left * (1.0 - GardenCatalog.WaterSpeedUp));
            Data.gardenBedWatered[bed] = 1;
            _economy.NotifyChanged();
            return GardenResult.Ok;
        }

        /// <summary>The well waters every growing bed. Returns how many got water.</summary>
        public int WaterAll()
        {
            if (BuildingLevel(GardenBuilding.Well) <= 0) return 0;
            int n = 0;
            for (int i = 0; i < OpenBeds; i++) if (Water(i) == GardenResult.Ok) n++;
            return n;
        }

        /// <summary>Harvests a ripe bed. Returns how many crops (0 if not ripe).</summary>
        public int Harvest(int bed)
        {
            if (!IsBedOpen(bed) || !IsRipe(bed)) return 0;
            var t = (CropType)Data.gardenBedCrop[bed];
            int n = GardenCatalog.Crop(t).Yield + Data.gardenBedCompost[bed];
            Data.crops[(int)t] += n;
            Data.gardenBedCrop[bed] = -1;
            Data.gardenBedWatered[bed] = 0;
            Data.gardenBedCompost[bed] = 0;
            AddXp(2 + (int)t * 2 + n);
            _economy.NotifyChanged();
            return n;
        }

        public int Crops(CropType t) => Data.crops[(int)t];

        /// <summary>Sells every crop at the garden stall. Returns the coins.</summary>
        public int SellAllCrops()
        {
            int coins = 0;
            for (int i = 0; i < 5; i++)
            {
                coins += Data.crops[i] * GardenCatalog.Crops[i].SellPrice;
                Data.crops[i] = 0;
            }
            Data.coins += coins;
            Data.totalCoinsEarned += coins;
            _economy.NotifyChanged();
            return coins;
        }

        // ------------------------------------------------------------------ buildings

        public int BuildingLevel(GardenBuilding b) => Data.gardenBuildings[(int)b];

        public GardenResult CanUpgrade(GardenBuilding b)
        {
            var def = GardenCatalog.Get(b);
            int level = BuildingLevel(b);
            if (Level < def.UnlockLevel) return GardenResult.Locked;
            if (level >= def.MaxLevel) return GardenResult.MaxLevel;
            if (Data.coins < def.Coins[level]) return GardenResult.NotEnoughCoins;
            if (Data.wood < def.Wood[level]) return GardenResult.NotEnoughWood;
            if (Data.brick < def.Brick[level]) return GardenResult.NotEnoughBrick;
            return GardenResult.Ok;
        }

        public GardenResult Upgrade(GardenBuilding b)
        {
            var r = CanUpgrade(b);
            if (r != GardenResult.Ok) return r;
            var def = GardenCatalog.Get(b);
            int level = BuildingLevel(b);
            Data.coins -= def.Coins[level];
            Data.wood -= def.Wood[level];
            Data.brick -= def.Brick[level];
            if (b == GardenBuilding.Coop && level == 0) Data.gardenEggsFrom = Now;
            Data.gardenBuildings[(int)b] = level + 1;
            AddXp(12 * (level + 1));
            _economy.NotifyChanged();
            return GardenResult.Ok;
        }

        public int EggsPerHour => 2 * BuildingLevel(GardenBuilding.Coop);

        public int EggsPending
        {
            get
            {
                if (EggsPerHour <= 0) return 0;
                double hours = Math.Min(GardenCatalog.EggCapHours, (Now - Data.gardenEggsFrom) / 3600.0);
                return (int)(hours * EggsPerHour);
            }
        }

        public int CollectEggs()
        {
            int eggs = EggsPending;
            if (eggs <= 0) return 0;
            Data.eggs += eggs;
            Data.gardenEggsFrom = Now;
            AddXp(eggs);
            _economy.NotifyChanged();
            return eggs;
        }

        /// <summary>Compost bin: mushrooms -> compost (fewer mushrooms at higher levels).</summary>
        public int CompostCost => Math.Max(1, GardenCatalog.CompostMushrooms - (BuildingLevel(GardenBuilding.Compost) - 1));

        public GardenResult MakeCompost()
        {
            if (BuildingLevel(GardenBuilding.Compost) <= 0) return GardenResult.Locked;
            if (Data.mushrooms < CompostCost) return GardenResult.Missing;
            Data.mushrooms -= CompostCost;
            Data.compost++;
            AddXp(2);
            _economy.NotifyChanged();
            return GardenResult.Ok;
        }

        public int Compost => Data.compost;

        // ------------------------------------------------------------------ kitchen

        public bool CanCook(Recipe r)
        {
            for (int i = 0; i < 5; i++) if (Data.crops[i] < r.Crops[i]) return false;
            return Data.mushrooms >= r.Mushrooms && Data.eggs >= r.Eggs && Data.honey >= r.Honey;
        }

        /// <summary>
        /// Cooks a dish and applies the bonuses that live in the economy (coins x2 games, run ticket and power-up,
        /// supplies). Pet bonuses (soup, omelette) are applied by the caller. False if ingredients are missing.
        /// </summary>
        public bool Cook(Recipe r)
        {
            if (!CanCook(r)) return false;
            for (int i = 0; i < 5; i++) Data.crops[i] -= r.Crops[i];
            Data.mushrooms -= r.Mushrooms;
            Data.eggs -= r.Eggs;
            Data.honey -= r.Honey;
            switch (r.Id)
            {
                case "strawberry_cake": Data.coinBoostGames += 2; break;
                case "pumpkin_pie": _economy.GrantRunTicket(1); _economy.AddPowerUp((PowerUpType)_rng.Next(4), 1, notify: false); break;
                case "sunflower_bread": Data.wood += 10; Data.brick += 5; Data.glass += 3; break;
            }
            Data.dishesCooked++;
            AddXp(8);
            _economy.NotifyChanged();
            return true;
        }

        // ------------------------------------------------------------------ crows

        public bool CrowsReady => Level >= 2 && Now >= Data.gardenCrowsNext && HasGrowingBed;

        private bool HasGrowingBed
        {
            get
            {
                for (int i = 0; i < OpenBeds; i++) if (!IsEmpty(i)) return true;
                return false;
            }
        }

        public const int CrowCoins = 40, CrowEggs = 2;

        /// <summary>Crows chased away: coins and eggs from a grateful farmer. Escaped: they peck a growing bed (it needs more time).</summary>
        public void FinishCrows(bool chased)
        {
            Data.gardenCrowsNext = Now + GardenCatalog.CrowsMinSeconds + _rng.Next((int)(GardenCatalog.CrowsMaxSeconds - GardenCatalog.CrowsMinSeconds));
            if (chased)
            {
                Data.coins += CrowCoins;
                Data.totalCoinsEarned += CrowCoins;
                Data.eggs += CrowEggs;
                AddXp(10);
            }
            else
            {
                for (int i = 0; i < OpenBeds; i++)
                    if (!IsEmpty(i) && !IsRipe(i)) { Data.gardenBedReady[i] += 120; break; }
            }
            _economy.NotifyChanged();
        }
    }
}
