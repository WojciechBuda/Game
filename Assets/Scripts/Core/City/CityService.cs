using System;
using System.Collections.Generic;
using JellyDrop.Core.Economy;

namespace JellyDrop.Core.City
{
    public enum BuildResult
    {
        Success,
        PlotLocked,
        PlotOccupied,
        PlotEmpty,
        UnknownBuilding,
        CityLevelTooLow,
        AlreadyBuilt,
        MaxLevel,
        NotEnoughCoins,
        NotEnoughSupplies
    }

    /// <summary>What a collection paid out.</summary>
    public sealed class Harvest
    {
        public int Coins, Wood, Brick, Glass;
        public bool IsEmpty => Coins == 0 && Wood == 0 && Brick == 0 && Glass == 0;

        public void Add(Product p, int amount)
        {
            switch (p)
            {
                case Product.Coins: Coins += amount; break;
                case Product.Wood: Wood += amount; break;
                case Product.Brick: Brick += amount; break;
                case Product.Glass: Glass += amount; break;
            }
        }
    }

    /// <summary>
    /// The city meta-game. Coins earned by playing buy supplies at the market; supplies and coins build and
    /// upgrade buildings on plots. Buildings raise population (which raises the city level and unlocks plots
    /// and building types), produce coins or supplies over time, and some boost coins from every game.
    /// Pure C#, unit-tested without Unity.
    /// </summary>
    public sealed class CityService
    {
        private readonly EconomyService _economy;
        private PlayerData Data => _economy.Data;
        private long Now => _economy.Clock.UnixSeconds;

        public CityService(EconomyService economy)
        {
            _economy = economy;
            EnsurePlots();
        }

        private void EnsurePlots()
        {
            while (Data.plots.Count < CityCatalog.MaxPlots) Data.plots.Add(new PlotData());
        }

        // ------------------------------------------------------------ supplies

        public int Amount(Supply s) => s == Supply.Wood ? Data.wood : s == Supply.Brick ? Data.brick : Data.glass;

        public void AddSupply(Supply s, int amount)
        {
            if (amount <= 0) return;
            switch (s)
            {
                case Supply.Wood: Data.wood += amount; break;
                case Supply.Brick: Data.brick += amount; break;
                default: Data.glass += amount; break;
            }
            _economy.NotifyChanged();
        }

        /// <summary>Buys supplies at the market with coins.</summary>
        public bool BuySupply(Supply s, int amount)
        {
            if (amount <= 0) return false;
            int cost = CityCatalog.Price(s) * amount;
            if (_economy.Coins < cost) return false;
            Data.coins -= cost;
            switch (s)
            {
                case Supply.Wood: Data.wood += amount; break;
                case Supply.Brick: Data.brick += amount; break;
                default: Data.glass += amount; break;
            }
            _economy.NotifyChanged();
            return true;
        }

        // ------------------------------------------------------------ city level

        public int Population
        {
            get
            {
                int pop = 0;
                foreach (var p in Data.plots)
                {
                    var def = CityCatalog.Get(p.building);
                    if (def != null && p.level > 0) pop += def.Population[p.level - 1];
                }
                return pop;
            }
        }

        public int CityLevel
        {
            get
            {
                int pop = Population, level = 0;
                foreach (int need in CityCatalog.LevelPopulation) if (pop >= need) level++;
                return Math.Max(1, level);
            }
        }

        /// <summary>Population needed for the next city level, or -1 at max level.</summary>
        public int NextLevelPopulation
        {
            get
            {
                int lvl = CityLevel;
                return lvl < CityCatalog.LevelPopulation.Length ? CityCatalog.LevelPopulation[lvl] : -1;
            }
        }

        public int UnlockedPlots => Math.Min(CityCatalog.MaxPlots, CityCatalog.StartingPlots + 2 * (CityLevel - 1));

        public bool IsPlotUnlocked(int plot) => plot >= 0 && plot < UnlockedPlots;

        /// <summary>City level needed to open this plot.</summary>
        public int PlotUnlockLevel(int plot) => Math.Max(1, (plot - CityCatalog.StartingPlots) / 2 + 2);

        /// <summary>Extra % coins after every game, from parks, the tower and the castle.</summary>
        public int CoinBonusPercent
        {
            get
            {
                int sum = 0;
                foreach (var p in Data.plots)
                {
                    var def = CityCatalog.Get(p.building);
                    if (def != null && p.level > 0) sum += def.CoinBonusPercent[p.level - 1];
                }
                return sum;
            }
        }

        // ------------------------------------------------------------ plots

        public PlotData Plot(int index) => Data.plots[index];

        public BuildingDef BuildingAt(int index) => CityCatalog.Get(Data.plots[index].building);

        public bool HasBuilding(string id)
        {
            foreach (var p in Data.plots) if (p.building == id && p.level > 0) return true;
            return false;
        }

        /// <summary>Buildings the player could put on an empty plot right now (unlocked by city level).</summary>
        public IEnumerable<BuildingDef> BuildOptions()
        {
            foreach (var b in CityCatalog.All)
                if (!(b.Unique && HasBuilding(b.Id))) yield return b;
        }

        public bool CanAfford(BuildingCost cost) =>
            _economy.Coins >= cost.Coins && Data.wood >= cost.Wood && Data.brick >= cost.Brick && Data.glass >= cost.Glass;

        public BuildResult CanBuild(int plot, string buildingId)
        {
            if (!IsPlotUnlocked(plot)) return BuildResult.PlotLocked;
            if (!string.IsNullOrEmpty(Data.plots[plot].building)) return BuildResult.PlotOccupied;
            var def = CityCatalog.Get(buildingId);
            if (def == null) return BuildResult.UnknownBuilding;
            if (CityLevel < def.UnlockCityLevel) return BuildResult.CityLevelTooLow;
            if (def.Unique && HasBuilding(def.Id)) return BuildResult.AlreadyBuilt;
            return CheckCost(def.Costs[0]);
        }

        public BuildResult Build(int plot, string buildingId)
        {
            var check = CanBuild(plot, buildingId);
            if (check != BuildResult.Success) return check;
            var def = CityCatalog.Get(buildingId);
            Pay(def.Costs[0]);
            var p = Data.plots[plot];
            p.building = buildingId;
            p.level = 1;
            p.lastCollect = Now;
            _economy.NotifyChanged();
            return BuildResult.Success;
        }

        /// <summary>Cost of the next upgrade, or null at max level / empty plot.</summary>
        public BuildingCost UpgradeCost(int plot)
        {
            var p = Data.plots[plot];
            var def = CityCatalog.Get(p.building);
            if (def == null || p.level >= def.MaxLevel) return null;
            return def.Costs[p.level];
        }

        public BuildResult CanUpgrade(int plot)
        {
            var p = Data.plots[plot];
            var def = CityCatalog.Get(p.building);
            if (def == null) return BuildResult.PlotEmpty;
            if (p.level >= def.MaxLevel) return BuildResult.MaxLevel;
            return CheckCost(def.Costs[p.level]);
        }

        public BuildResult Upgrade(int plot)
        {
            var check = CanUpgrade(plot);
            if (check != BuildResult.Success) return check;
            // Collect first so the stored amount isn't recalculated at the new rate.
            CollectPlot(plot, notify: false);
            var p = Data.plots[plot];
            Pay(CityCatalog.Get(p.building).Costs[p.level]);
            p.level++;
            _economy.NotifyChanged();
            return BuildResult.Success;
        }

        private BuildResult CheckCost(BuildingCost c)
        {
            if (_economy.Coins < c.Coins) return BuildResult.NotEnoughCoins;
            if (Data.wood < c.Wood || Data.brick < c.Brick || Data.glass < c.Glass) return BuildResult.NotEnoughSupplies;
            return BuildResult.Success;
        }

        private void Pay(BuildingCost c)
        {
            Data.coins -= c.Coins;
            Data.wood -= c.Wood;
            Data.brick -= c.Brick;
            Data.glass -= c.Glass;
        }

        // ------------------------------------------------------------ production

        /// <summary>How much a plot has produced and stored so far (capped at StorageHours).</summary>
        public int Pending(int plot)
        {
            var p = Data.plots[plot];
            var def = CityCatalog.Get(p.building);
            if (def == null || p.level <= 0 || def.Produces == Product.None) return 0;
            double hours = Math.Max(0, Now - p.lastCollect) / 3600.0;
            hours = Math.Min(hours, CityCatalog.StorageHours);
            return (int)Math.Floor(hours * def.PerHour[p.level - 1]);
        }

        /// <summary>0..1, how full the plot's storage is (for the UI).</summary>
        public float StorageFill(int plot)
        {
            var p = Data.plots[plot];
            var def = CityCatalog.Get(p.building);
            if (def == null || p.level <= 0 || def.Produces == Product.None) return 0f;
            double hours = Math.Max(0, Now - p.lastCollect) / 3600.0;
            return (float)Math.Min(1.0, hours / CityCatalog.StorageHours);
        }

        public Harvest PendingTotal()
        {
            var h = new Harvest();
            for (int i = 0; i < Data.plots.Count; i++)
            {
                var def = BuildingAt(i);
                if (def != null) h.Add(def.Produces, Pending(i));
            }
            return h;
        }

        public Harvest CollectPlot(int plot, bool notify = true)
        {
            var h = new Harvest();
            var p = Data.plots[plot];
            var def = CityCatalog.Get(p.building);
            if (def == null || def.Produces == Product.None) return h;
            int amount = Pending(plot);
            if (amount > 0)
            {
                // Keep the leftover fraction of an hour so nothing is lost to rounding.
                long secondsPerUnit = Math.Max(1, 3600 / Math.Max(1, def.PerHour[p.level - 1]));
                bool capped = StorageFill(plot) >= 1f;
                p.lastCollect = capped ? Now : p.lastCollect + amount * secondsPerUnit;
                if (p.lastCollect > Now) p.lastCollect = Now;
                h.Add(def.Produces, amount);
                Grant(h);
                if (notify) _economy.NotifyChanged();
            }
            return h;
        }

        public Harvest CollectAll()
        {
            var total = new Harvest();
            for (int i = 0; i < Data.plots.Count; i++)
            {
                var h = CollectPlot(i, notify: false);
                total.Coins += h.Coins; total.Wood += h.Wood; total.Brick += h.Brick; total.Glass += h.Glass;
            }
            if (!total.IsEmpty) _economy.NotifyChanged();
            return total;
        }

        private void Grant(Harvest h)
        {
            Data.coins += h.Coins;
            Data.totalCoinsEarned += h.Coins;
            Data.wood += h.Wood;
            Data.brick += h.Brick;
            Data.glass += h.Glass;
        }
    }
}
