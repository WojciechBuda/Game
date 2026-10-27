using JellyDrop.Core;
using JellyDrop.Core.City;
using JellyDrop.Core.Economy;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class CityTests
    {
        private FixedClock _clock;
        private EconomyService _economy;

        private CityService NewCity(int coins = 100000, int wood = 500, int brick = 500, int glass = 500)
        {
            _clock = new FixedClock(20000);
            var data = new PlayerData();
            _economy = new EconomyService(data, Catalog.CreateDefault(), new EconomyConfig(), _clock);
            data.coins = coins; data.wood = wood; data.brick = brick; data.glass = glass;
            return new CityService(_economy);
        }

        [Test]
        public void NewCityHasFourOpenPlotsAtLevelOne()
        {
            var c = NewCity();
            Assert.AreEqual(1, c.CityLevel);
            Assert.AreEqual(0, c.Population);
            Assert.AreEqual(4, c.UnlockedPlots);
            Assert.IsTrue(c.IsPlotUnlocked(3));
            Assert.IsFalse(c.IsPlotUnlocked(4));
            Assert.AreEqual(CityCatalog.MaxPlots, _economy.Data.plots.Count);
        }

        [Test]
        public void BuildingCostsSuppliesAndCoins()
        {
            var c = NewCity(coins: 1000, wood: 10, brick: 10, glass: 0);
            var cost = CityCatalog.Get("house").Costs[0];
            Assert.AreEqual(BuildResult.Success, c.Build(0, "house"));
            Assert.AreEqual(1000 - cost.Coins, _economy.Coins);
            Assert.AreEqual(10 - cost.Wood, c.Amount(Supply.Wood));
            Assert.AreEqual(10 - cost.Brick, c.Amount(Supply.Brick));
            Assert.AreEqual(5, c.Population);
            Assert.AreEqual(BuildResult.PlotOccupied, c.Build(0, "house"));
        }

        [Test]
        public void CannotBuildWithoutSuppliesOrOnLockedPlots()
        {
            var c = NewCity(coins: 1000, wood: 0, brick: 0, glass: 0);
            Assert.AreEqual(BuildResult.NotEnoughSupplies, c.Build(0, "house"));
            Assert.AreEqual(BuildResult.PlotLocked, c.Build(8, "house"));
            var poor = NewCity(coins: 0);
            Assert.AreEqual(BuildResult.NotEnoughCoins, poor.Build(0, "house"));
        }

        [Test]
        public void BuildingsUnlockWithCityLevel()
        {
            var c = NewCity();
            Assert.AreEqual(BuildResult.CityLevelTooLow, c.Build(0, "bakery"));
            c.Build(0, "house");
            c.Build(1, "house");
            Assert.AreEqual(2, c.CityLevel, "10 population reaches level 2");
            Assert.AreEqual(6, c.UnlockedPlots);
            Assert.AreEqual(BuildResult.Success, c.Build(2, "bakery"));
        }

        [Test]
        public void UniqueBuildingsOnlyOnce()
        {
            var c = NewCity();
            // Reach city level 7 with upgraded houses.
            for (int i = 0; i < 4; i++) { c.Build(i, "house"); c.Upgrade(i); c.Upgrade(i); }
            for (int i = 4; i < c.UnlockedPlots && c.CityLevel < 7; i++) { c.Build(i, "house"); c.Upgrade(i); c.Upgrade(i); }
            Assert.GreaterOrEqual(c.CityLevel, 7);
            int free = -1;
            for (int i = 0; i < c.UnlockedPlots; i++) if (string.IsNullOrEmpty(c.Plot(i).building)) { free = i; break; }
            if (free < 0) { Assert.Pass("no free plot to test with"); return; }
            Assert.AreEqual(BuildResult.Success, c.Build(free, "castle"));
            Assert.IsTrue(c.HasBuilding("castle"));
            foreach (var b in c.BuildOptions()) Assert.AreNotEqual("castle", b.Id);
        }

        [Test]
        public void UpgradesRaisePopulationUpToMaxLevel()
        {
            var c = NewCity();
            c.Build(0, "house");
            Assert.AreEqual(BuildResult.Success, c.Upgrade(0));
            Assert.AreEqual(12, c.Population);
            Assert.AreEqual(BuildResult.Success, c.Upgrade(0));
            Assert.AreEqual(22, c.Population);
            Assert.AreEqual(BuildResult.MaxLevel, c.Upgrade(0));
            Assert.IsNull(c.UpgradeCost(0));
        }

        [Test]
        public void ProductionAccruesAndIsCappedAtStorage()
        {
            var c = NewCity();
            c.Build(0, "house");   // 20 coins/hour
            c.Build(1, "sawmill"); // 1 wood/hour
            Assert.AreEqual(0, c.Pending(0));
            _clock.UnixSeconds += 3600 * 3;
            Assert.AreEqual(60, c.Pending(0));
            Assert.AreEqual(3, c.Pending(1));
            _clock.UnixSeconds += 3600 * 100;
            Assert.AreEqual((int)(20 * CityCatalog.StorageHours), c.Pending(0), "stops at storage cap");
            Assert.AreEqual(1f, c.StorageFill(0), 0.001);
        }

        [Test]
        public void CollectingPaysAndResetsTheTimer()
        {
            var c = NewCity();
            c.Build(0, "house");
            c.Build(1, "sawmill");
            int coins = _economy.Coins, wood = c.Amount(Supply.Wood);
            _clock.UnixSeconds += 3600 * 2;
            var h = c.CollectAll();
            Assert.AreEqual(40, h.Coins);
            Assert.AreEqual(2, h.Wood);
            Assert.AreEqual(coins + 40, _economy.Coins);
            Assert.AreEqual(wood + 2, c.Amount(Supply.Wood));
            Assert.AreEqual(0, c.Pending(0));
            Assert.IsTrue(c.CollectAll().IsEmpty);
        }

        [Test]
        public void PartialHoursAreNotLost()
        {
            var c = NewCity();
            c.Build(1, "sawmill"); // 1 wood per hour
            _clock.UnixSeconds += 5400; // 1.5 hours
            Assert.AreEqual(1, c.CollectAll().Wood);
            _clock.UnixSeconds += 1800; // another half hour completes the second unit
            Assert.AreEqual(1, c.CollectAll().Wood);
        }

        [Test]
        public void MarketSellsSuppliesForCoins()
        {
            var c = NewCity(coins: 100, wood: 0, brick: 0, glass: 0);
            Assert.IsTrue(c.BuySupply(Supply.Wood, 2));
            Assert.AreEqual(100 - 2 * CityCatalog.Price(Supply.Wood), _economy.Coins);
            Assert.AreEqual(2, c.Amount(Supply.Wood));
            Assert.IsFalse(c.BuySupply(Supply.Glass, 5), "not enough coins");
        }

        [Test]
        public void ParksBoostCoinsFromGames()
        {
            var c = NewCity();
            _economy.CoinBonusPercent = () => c.CoinBonusPercent;
            c.Build(0, "house"); c.Build(1, "house"); c.Build(2, "house"); c.Build(3, "house");
            c.Upgrade(0); c.Upgrade(1);
            Assert.GreaterOrEqual(c.CityLevel, 3);
            int free = 4;
            Assert.AreEqual(BuildResult.Success, c.Build(free, "park"));
            Assert.AreEqual(3, c.CoinBonusPercent);
            var rewards = _economy.ApplyGameResult(new GameResult { Mode = GameMode.Classic, Score = 10000, Stats = new SessionStats() });
            int baseTotal = rewards.ScoreCoins + rewards.CollectedCoins + rewards.PersonalBestBonus + rewards.SprintBonus;
            Assert.AreEqual((int)System.Math.Round(baseTotal * 0.03), rewards.CityBonus);
        }

        [Test]
        public void EveryBuildingHasThreeLevelsOfData()
        {
            foreach (var b in CityCatalog.All)
            {
                Assert.AreEqual(3, b.MaxLevel, b.Id);
                Assert.AreEqual(3, b.Population.Length, b.Id);
                Assert.AreEqual(3, b.PerHour.Length, b.Id);
                Assert.AreEqual(3, b.CoinBonusPercent.Length, b.Id);
                Assert.IsTrue(Loc.Has("building." + b.Id), b.Id);
            }
        }
    }
}
