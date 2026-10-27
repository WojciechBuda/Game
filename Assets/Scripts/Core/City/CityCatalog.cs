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

        public int MaxLevel => Costs.Length;
    }

    /// <summary>
    /// All buildings and city tuning in one place. The order is the order they appear in the build menu.
    /// </summary>
    public static class CityCatalog
    {
        /// <summary>Hours of production a building stores before it stops (come back and collect!).</summary>
        public const float StorageHours = 8f;
        public const int MaxPlots = 12;
        public const int StartingPlots = 4;

        /// <summary>Population needed for each city level (index 0 = level 1).</summary>
        public static readonly int[] LevelPopulation = { 0, 10, 25, 50, 90, 140, 200, 280, 380, 500 };

        /// <summary>Market prices in coins per unit.</summary>
        public static int Price(Supply s) => s == Supply.Wood ? 25 : s == Supply.Brick ? 40 : 60;

        public static readonly List<BuildingDef> All = new List<BuildingDef>
        {
            new BuildingDef
            {
                Id = "house", UnlockCityLevel = 1,
                Costs = new[] { new BuildingCost(50, 3, 1, 0), new BuildingCost(150, 4, 4, 0), new BuildingCost(400, 6, 8, 2) },
                Population = new[] { 5, 12, 22 }, Produces = Product.Coins, PerHour = new[] { 20, 45, 90 }
            },
            new BuildingDef
            {
                Id = "sawmill", UnlockCityLevel = 1,
                Costs = new[] { new BuildingCost(80, 2, 2, 0), new BuildingCost(200, 3, 5, 0), new BuildingCost(500, 4, 8, 3) },
                Population = new[] { 2, 4, 6 }, Produces = Product.Wood, PerHour = new[] { 1, 2, 4 }
            },
            new BuildingDef
            {
                Id = "bakery", UnlockCityLevel = 2,
                Costs = new[] { new BuildingCost(120, 4, 3, 0), new BuildingCost(300, 5, 6, 1), new BuildingCost(700, 8, 10, 4) },
                Population = new[] { 3, 6, 10 }, Produces = Product.Coins, PerHour = new[] { 35, 80, 160 }
            },
            new BuildingDef
            {
                Id = "brickworks", UnlockCityLevel = 2,
                Costs = new[] { new BuildingCost(150, 4, 2, 0), new BuildingCost(350, 6, 4, 1), new BuildingCost(800, 8, 6, 4) },
                Population = new[] { 2, 4, 6 }, Produces = Product.Brick, PerHour = new[] { 1, 2, 4 }
            },
            new BuildingDef
            {
                Id = "park", UnlockCityLevel = 3,
                Costs = new[] { new BuildingCost(100, 3, 2, 0), new BuildingCost(250, 5, 5, 0), new BuildingCost(600, 6, 8, 3) },
                Population = new[] { 0, 0, 0 }, CoinBonusPercent = new[] { 3, 6, 10 }
            },
            new BuildingDef
            {
                Id = "windmill", UnlockCityLevel = 3,
                Costs = new[] { new BuildingCost(200, 8, 4, 1), new BuildingCost(450, 10, 8, 2), new BuildingCost(1000, 12, 12, 5) },
                Population = new[] { 4, 8, 12 }, Produces = Product.Coins, PerHour = new[] { 50, 110, 220 }
            },
            new BuildingDef
            {
                Id = "glassworks", UnlockCityLevel = 4,
                Costs = new[] { new BuildingCost(250, 6, 6, 0), new BuildingCost(600, 8, 10, 2), new BuildingCost(1200, 10, 14, 6) },
                Population = new[] { 3, 6, 9 }, Produces = Product.Glass, PerHour = new[] { 1, 2, 3 }
            },
            new BuildingDef
            {
                Id = "market", UnlockCityLevel = 4,
                Costs = new[] { new BuildingCost(300, 8, 8, 2), new BuildingCost(700, 10, 12, 4), new BuildingCost(1500, 14, 16, 8) },
                Population = new[] { 6, 12, 20 }, Produces = Product.Coins, PerHour = new[] { 70, 150, 300 }
            },
            new BuildingDef
            {
                Id = "tower", UnlockCityLevel = 5, Unique = true,
                Costs = new[] { new BuildingCost(400, 6, 14, 4), new BuildingCost(900, 8, 18, 8), new BuildingCost(2000, 12, 24, 12) },
                Population = new[] { 2, 4, 6 }, CoinBonusPercent = new[] { 5, 10, 15 }
            },
            new BuildingDef
            {
                Id = "castle", UnlockCityLevel = 7, Unique = true,
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
