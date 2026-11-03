using System;
using System.Collections.Generic;

namespace JellyDrop.Core.City
{
    /// <summary>Building supplies. Bought at the market with coins, won in Boost levels or made by workshops.</summary>
    public enum Supply { Wood = 0, Brick = 1, Glass = 2 }

    /// <summary>What a building makes over time.</summary>
    public enum Product { None, Coins, Wood, Brick, Glass }

    public sealed class BuildingCost
    {
        public int Coins, Wood, Brick, Glass;

        public BuildingCost(int coins, int wood, int brick, int glass)
        {
            Coins = coins; Wood = wood; Brick = brick; Glass = glass;
        }

        public int Get(Supply s) => s == Supply.Wood ? Wood : s == Supply.Brick ? Brick : Glass;
    }

    /// <summary>A decoration for the islands: costs coins (and a few supplies), adds a little coin bonus.</summary>
    public sealed class DecorDef
    {
        public string Id;
        public int Coins, Wood, Brick, Glass;
        /// <summary>Extra % coins after every game while it stands.</summary>
        public int BonusPercent;
        public BuildingCost Cost => new BuildingCost(Coins, Wood, Brick, Glass);
    }

    /// <summary>A building type. Each has 3 levels: built at level 1, then upgraded twice.</summary>
    public sealed class BuildingDef
    {
        public string Id;
        /// <summary>City level needed before it can be built.</summary>
        public int UnlockCityLevel = 1;
        /// <summary>Only one per city (Tower, Castle).</summary>
        public bool Unique;
        /// <summary>Cost to build (index 0) and to upgrade to level 2 and 3.</summary>
        public BuildingCost[] Costs;
        public int[] Population;
        public Product Produces = Product.None;
        /// <summary>Amount produced per hour at each level.</summary>
        public int[] PerHour = { 0, 0, 0 };
        /// <summary>Extra % coins after every game (stacks across buildings).</summary>
        public int[] CoinBonusPercent = { 0, 0, 0 };
        /// <summary>Seconds to build level 1; every level takes longer (see BuildSeconds).</summary>
        public int BaseBuildSeconds = 30;

        /// <summary>Buildings go up to level 10. Levels past the hand-made data are extrapolated.</summary>
        public int MaxLevel => CityCatalog.MaxBuildingLevel;

        /// <summary>Cost to reach <paramref name="level"/> (1 = build).</summary>
        public BuildingCost CostFor(int level)
        {
            int i = level - 1;
            if (i < Costs.Length) return Costs[Math.Max(0, i)];
            var last = Costs[Costs.Length - 1];
            double k = Math.Pow(1.7, i - (Costs.Length - 1));
            return new BuildingCost(Round10(last.Coins * k), (int)Math.Ceiling(last.Wood * k), (int)Math.Ceiling(last.Brick * k), (int)Math.Ceiling(last.Glass * k));
        }

        public int PopulationAt(int level) => Extrapolate(Population, level, 1.0);
        public int PerHourAt(int level)
        {
            if (level <= 0) return 0;
            if (level <= PerHour.Length) return PerHour[level - 1];
            return (int)Math.Round(PerHour[PerHour.Length - 1] * Math.Pow(1.4, level - PerHour.Length));
        }
        public int CoinBonusAt(int level) => level <= 0 ? 0 : level <= CoinBonusPercent.Length
            ? CoinBonusPercent[level - 1]
            : CoinBonusPercent[CoinBonusPercent.Length - 1] + (CoinBonusPercent[CoinBonusPercent.Length - 1] > 0 ? 2 * (level - CoinBonusPercent.Length) : 0);

        /// <summary>Construction time for reaching <paramref name="level"/>: grows ~1.9x per level, capped at 12 hours.</summary>
        public long BuildSeconds(int level) =>
            (long)Math.Min(CityCatalog.MaxBuildSeconds, BaseBuildSeconds * Math.Pow(1.9, Math.Max(0, level - 1)));

        private static int Extrapolate(int[] values, int level, double stepScale)
        {
            if (level <= 0) return 0;
            if (level <= values.Length) return values[level - 1];
            int n = values.Length;
            int step = Math.Max(1, values[n - 1] - (n >= 2 ? values[n - 2] : 0));
            return values[n - 1] + (int)Math.Round(step * stepScale * (level - n));
        }

        private static int Round10(double v) => (int)Math.Round(v / 10.0) * 10;
    }

    /// <summary>
    /// All buildings and city tuning in one place. The order is the order they appear in the build menu.
    /// </summary>
    public static class CityCatalog
    {
        /// <summary>Hours of production a building stores before it stops (come back and collect!).</summary>
        public const float StorageHours = 8f;
        /// <summary>The main island grows a row of three plots at a time (with the city level), up to this many.</summary>
        public const int MainPlots = 30;
        /// <summary>Plots on each extra island the player can buy.</summary>
        public const int ExpansionPlots = 6;
        public const int MaxExpansions = 4;
        /// <summary>All plots: the main island plus every extra island.</summary>
        public const int MaxPlots = MainPlots + ExpansionPlots * MaxExpansions;

        /// <summary>Price of extra island number <paramref name="index"/> (0 = the first one).</summary>
        public static BuildingCost ExpansionCost(int index) =>
            new BuildingCost(3000 * (1 << index), 15 + 10 * index, 15 + 10 * index, 5 * index);

        /// <summary>City level needed before extra island <paramref name="index"/> can be bought.</summary>
        public static int ExpansionLevel(int index) => 3 + 2 * index;

        /// <summary>Spots for decorations: along the main island's edges, plus four on every extra island.</summary>
        public const int MainDecorSpots = 8;
        public const int DecorSpotsPerExpansion = 4;
        public const int MaxDecorSpots = MainDecorSpots + DecorSpotsPerExpansion * MaxExpansions;

        /// <summary>Share of the build cost (coins) paid back when a building is demolished.</summary>
        public const float DemolishRefund = 0.25f;

        public static readonly DecorDef[] Decorations =
        {
            new DecorDef { Id = "flowers", Coins = 400, BonusPercent = 1 },
            new DecorDef { Id = "lamp", Coins = 500, BonusPercent = 1 },
            new DecorDef { Id = "bench", Coins = 600, BonusPercent = 1 },
            new DecorDef { Id = "topiary", Coins = 900, BonusPercent = 1 },
            new DecorDef { Id = "pond", Coins = 1500, BonusPercent = 2 },
            new DecorDef { Id = "fountain", Coins = 2500, Brick = 4, BonusPercent = 2 },
            new DecorDef { Id = "statue", Coins = 4000, Brick = 6, Glass = 2, BonusPercent = 3 },
            new DecorDef { Id = "gazebo", Coins = 6000, Wood = 10, Glass = 3, BonusPercent = 3 },
        };

        public static DecorDef Decor(string id)
        {
            foreach (var d in Decorations) if (d.Id == id) return d;
            return null;
        }
        public const int StartingPlots = 4;
        public const int MaxBuildingLevel = 10;
        public const long MaxBuildSeconds = 12 * 3600;
        /// <summary>How many buildings can be under construction at once.</summary>
        public const int Builders = 2;
        public const int MaxCityLevel = 25;

        /// <summary>Population needed for the first city levels (index 0 = level 1); later levels follow PopulationForLevel.</summary>
        public static readonly int[] LevelPopulation = { 0, 10, 25, 50, 90, 140, 200, 280, 380, 500 };

        /// <summary>Population needed to reach a city level (level 1 needs 0). Keeps growing after level 10.</summary>
        public static int PopulationForLevel(int level)
        {
            if (level <= 1) return 0;
            if (level <= LevelPopulation.Length) return LevelPopulation[level - 1];
            int extra = level - LevelPopulation.Length;
            return LevelPopulation[LevelPopulation.Length - 1] + 140 * extra + 15 * extra * extra;
        }

        /// <summary>Market prices in coins per unit.</summary>
        public static int Price(Supply s) => s == Supply.Wood ? 25 : s == Supply.Brick ? 40 : 60;

        public static readonly List<BuildingDef> All = new List<BuildingDef>
        {
            new BuildingDef
            {
                Id = "house", BaseBuildSeconds = 20, UnlockCityLevel = 1,
                Costs = new[] { new BuildingCost(50, 3, 1, 0), new BuildingCost(150, 4, 4, 0), new BuildingCost(400, 6, 8, 2) },
                Population = new[] { 5, 12, 22 }, Produces = Product.Coins, PerHour = new[] { 20, 45, 90 }
            },
            new BuildingDef
            {
                Id = "sawmill", BaseBuildSeconds = 40, UnlockCityLevel = 1,
                Costs = new[] { new BuildingCost(80, 2, 2, 0), new BuildingCost(200, 3, 5, 0), new BuildingCost(500, 4, 8, 3) },
                Population = new[] { 2, 4, 6 }, Produces = Product.Wood, PerHour = new[] { 1, 2, 4 }
            },
            new BuildingDef
            {
                Id = "bakery", BaseBuildSeconds = 60, UnlockCityLevel = 2,
                Costs = new[] { new BuildingCost(120, 4, 3, 0), new BuildingCost(300, 5, 6, 1), new BuildingCost(700, 8, 10, 4) },
                Population = new[] { 3, 6, 10 }, Produces = Product.Coins, PerHour = new[] { 35, 80, 160 }
            },
            new BuildingDef
            {
                Id = "brickworks", BaseBuildSeconds = 80, UnlockCityLevel = 2,
                Costs = new[] { new BuildingCost(150, 4, 2, 0), new BuildingCost(350, 6, 4, 1), new BuildingCost(800, 8, 6, 4) },
                Population = new[] { 2, 4, 6 }, Produces = Product.Brick, PerHour = new[] { 1, 2, 4 }
            },
            new BuildingDef
            {
                Id = "park", BaseBuildSeconds = 50, UnlockCityLevel = 3,
                Costs = new[] { new BuildingCost(100, 3, 2, 0), new BuildingCost(250, 5, 5, 0), new BuildingCost(600, 6, 8, 3) },
                Population = new[] { 0, 0, 0 }, CoinBonusPercent = new[] { 3, 6, 10 }
            },
            new BuildingDef
            {
                Id = "windmill", BaseBuildSeconds = 120, UnlockCityLevel = 3,
                Costs = new[] { new BuildingCost(200, 8, 4, 1), new BuildingCost(450, 10, 8, 2), new BuildingCost(1000, 12, 12, 5) },
                Population = new[] { 4, 8, 12 }, Produces = Product.Coins, PerHour = new[] { 50, 110, 220 }
            },
            new BuildingDef
            {
                Id = "glassworks", BaseBuildSeconds = 160, UnlockCityLevel = 4,
                Costs = new[] { new BuildingCost(250, 6, 6, 0), new BuildingCost(600, 8, 10, 2), new BuildingCost(1200, 10, 14, 6) },
                Population = new[] { 3, 6, 9 }, Produces = Product.Glass, PerHour = new[] { 1, 2, 3 }
            },
            new BuildingDef
            {
                Id = "market", BaseBuildSeconds = 240, UnlockCityLevel = 4,
                Costs = new[] { new BuildingCost(300, 8, 8, 2), new BuildingCost(700, 10, 12, 4), new BuildingCost(1500, 14, 16, 8) },
                Population = new[] { 6, 12, 20 }, Produces = Product.Coins, PerHour = new[] { 70, 150, 300 }
            },
            new BuildingDef
            {
                Id = "tower", BaseBuildSeconds = 300, UnlockCityLevel = 5, Unique = true,
                Costs = new[] { new BuildingCost(400, 6, 14, 4), new BuildingCost(900, 8, 18, 8), new BuildingCost(2000, 12, 24, 12) },
                Population = new[] { 2, 4, 6 }, CoinBonusPercent = new[] { 5, 10, 15 }
            },
            new BuildingDef
            {
                Id = "castle", BaseBuildSeconds = 600, UnlockCityLevel = 7, Unique = true,
                Costs = new[] { new BuildingCost(1500, 20, 30, 15), new BuildingCost(3000, 30, 45, 25), new BuildingCost(6000, 40, 60, 40) },
                Population = new[] { 40, 80, 150 }, Produces = Product.Coins, PerHour = new[] { 150, 300, 600 },
                CoinBonusPercent = new[] { 5, 10, 20 }
            },
        };

        public static BuildingDef Get(string id)
        {
            foreach (var b in All) if (b.Id == id) return b;
            return null;
        }
    }
}
