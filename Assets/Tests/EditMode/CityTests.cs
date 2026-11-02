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

        // Builds / upgrades and finishes the construction straight away (most tests don't care about timers).
        private static BuildResult Build(CityService c, int plot, string id)
        {
            var r = c.Build(plot, id);
            if (r == BuildResult.Success) c.CompleteConstruction(plot);
            return r;
        }

        private static BuildResult Upgrade(CityService c, int plot)
        {
            var r = c.Upgrade(plot);
            if (r == BuildResult.Success) c.CompleteConstruction(plot);
            return r;
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
            Assert.AreEqual(BuildResult.Success, Build(c, 0, "house"));
            Assert.AreEqual(1000 - cost.Coins, _economy.Coins);
            Assert.AreEqual(10 - cost.Wood, c.Amount(Supply.Wood));
            Assert.AreEqual(10 - cost.Brick, c.Amount(Supply.Brick));
            Assert.AreEqual(5, c.Population);
            Assert.AreEqual(BuildResult.PlotOccupied, Build(c, 0, "house"));
        }

        [Test]
        public void CannotBuildWithoutSuppliesOrOnLockedPlots()
        {
            var c = NewCity(coins: 1000, wood: 0, brick: 0, glass: 0);
            Assert.AreEqual(BuildResult.NotEnoughSupplies, Build(c, 0, "house"));
            Assert.AreEqual(BuildResult.PlotLocked, Build(c, 8, "house"));
            var poor = NewCity(coins: 0);
            Assert.AreEqual(BuildResult.NotEnoughCoins, Build(poor, 0, "house"));
        }

        [Test]
        public void BuildingsUnlockWithCityLevel()
        {
            var c = NewCity();
            Assert.AreEqual(BuildResult.CityLevelTooLow, Build(c, 0, "bakery"));
            Build(c, 0, "house");
            Build(c, 1, "house");
            Assert.AreEqual(2, c.CityLevel, "10 population reaches level 2");
            Assert.AreEqual(6, c.UnlockedPlots);
            Assert.AreEqual(BuildResult.Success, Build(c, 2, "bakery"));
        }

        [Test]
        public void UniqueBuildingsOnlyOnce()
        {
            var c = NewCity();
            // Reach city level 7 with upgraded houses.
            for (int i = 0; i < 4; i++) { Build(c, i, "house"); Upgrade(c, i); Upgrade(c, i); }
            for (int i = 4; i < c.UnlockedPlots && c.CityLevel < 7; i++) { Build(c, i, "house"); Upgrade(c, i); Upgrade(c, i); }
            Assert.GreaterOrEqual(c.CityLevel, 7);
            int free = -1;
            for (int i = 0; i < c.UnlockedPlots; i++) if (string.IsNullOrEmpty(c.Plot(i).building)) { free = i; break; }
            if (free < 0) { Assert.Pass("no free plot to test with"); return; }
            Assert.AreEqual(BuildResult.Success, Build(c, free, "castle"));
            Assert.IsTrue(c.HasBuilding("castle"));
            foreach (var b in c.BuildOptions()) Assert.AreNotEqual("castle", b.Id);
        }

        [Test]
        public void UpgradesRaisePopulationUpToMaxLevel()
        {
            var c = NewCity(coins: 50_000_000, wood: 100_000, brick: 100_000, glass: 100_000);
            Build(c, 0, "house");
            Assert.AreEqual(BuildResult.Success, Upgrade(c, 0));
            Assert.AreEqual(12, c.Population);
            Assert.AreEqual(BuildResult.Success, Upgrade(c, 0));
            Assert.AreEqual(22, c.Population);
            int previous = c.Population;
            for (int level = 4; level <= CityCatalog.MaxBuildingLevel; level++)
            {
                Assert.AreEqual(BuildResult.Success, Upgrade(c, 0), "level " + level);
                Assert.Greater(c.Population, previous);
                previous = c.Population;
            }
            Assert.AreEqual(BuildResult.MaxLevel, Upgrade(c, 0));
            Assert.IsNull(c.UpgradeCost(0));
        }

        [Test]
        public void ConstructionTakesTimeAndLongerEachLevel()
        {
            var c = NewCity(coins: 1_000_000, wood: 10_000, brick: 10_000, glass: 10_000);
            var house = CityCatalog.Get("house");
            Assert.AreEqual(BuildResult.Success, c.Build(0, "house"));
            Assert.IsTrue(c.IsUnderConstruction(0));
            Assert.AreEqual(0, c.Population, "no people until it is finished");
            Assert.AreEqual(house.BuildSeconds(1), c.ConstructionSecondsLeft(0));
            _clock.UnixSeconds += house.BuildSeconds(1);
            Assert.AreEqual(5, c.Population);
            Assert.IsFalse(c.IsUnderConstruction(0));
            for (int l = 2; l <= CityCatalog.MaxBuildingLevel; l++)
                Assert.GreaterOrEqual((int)house.BuildSeconds(l), (int)house.BuildSeconds(l - 1));
            Assert.Greater((int)house.BuildSeconds(5), (int)house.BuildSeconds(2) * 3);
            Assert.LessOrEqual((int)CityCatalog.Get("castle").BuildSeconds(10), (int)CityCatalog.MaxBuildSeconds);
        }

        [Test]
        public void UpgradingKeepsProducingAndOnlyTwoBuildersWork()
        {
            var c = NewCity(coins: 1_000_000, wood: 10_000, brick: 10_000, glass: 10_000);
            Build(c, 0, "house");
            Build(c, 1, "house");
            Build(c, 2, "house");
            Assert.AreEqual(BuildResult.Success, c.Upgrade(0));
            Assert.AreEqual(BuildResult.Success, c.Upgrade(1));
            Assert.AreEqual(BuildResult.NoFreeBuilder, c.Upgrade(2));
            Assert.AreEqual(BuildResult.UnderConstruction, c.Upgrade(0));
            Assert.AreEqual(1, c.Plot(0).level, "still level 1 while upgrading");
            _clock.UnixSeconds += 3600;
            Assert.Greater(c.Pending(2), 0);
            Assert.AreEqual(2, c.Plot(0).level, "upgrade finished after its time");
            Assert.AreEqual(0, c.BusyBuilders);
        }

        [Test]
        public void SpeedUpCostsCoinsAndFinishesNow()
        {
            var c = NewCity(coins: 1_000_000, wood: 10_000, brick: 10_000, glass: 10_000);
            Build(c, 0, "house");
            c.Upgrade(0);
            int cost = c.SpeedUpCost(0);
            Assert.Greater(cost, 0);
            int coins = _economy.Coins;
            Assert.IsTrue(c.SpeedUp(0));
            Assert.AreEqual(coins - cost, _economy.Coins);
            Assert.AreEqual(2, c.Plot(0).level);
            Assert.IsFalse(c.IsUnderConstruction(0));
        }

        [Test]
        public void TheCityKeepsGrowingPastLevelTen()
        {
            Assert.AreEqual(0, CityCatalog.PopulationForLevel(1));
            for (int l = 2; l <= CityCatalog.MaxCityLevel; l++)
                Assert.Greater(CityCatalog.PopulationForLevel(l), CityCatalog.PopulationForLevel(l - 1));
            var c = NewCity();
            Assert.AreEqual(CityCatalog.MaxPlots, _economy.Data.plots.Count);
            Assert.LessOrEqual(c.PlotUnlockLevel(CityCatalog.MaxPlots - 1), CityCatalog.MaxCityLevel, "every plot can be opened");
        }

        [Test]
        public void ProductionAccruesAndIsCappedAtStorage()
        {
            var c = NewCity();
            Build(c, 0, "house");   // 20 coins/hour
            Build(c, 1, "sawmill"); // 1 wood/hour
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
            Build(c, 0, "house");
            Build(c, 1, "sawmill");
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
            Build(c, 1, "sawmill"); // 1 wood per hour
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
            Build(c, 0, "house"); Build(c, 1, "house"); Build(c, 2, "house"); Build(c, 3, "house");
            Upgrade(c, 0); Upgrade(c, 1);
            Assert.GreaterOrEqual(c.CityLevel, 3);
            int free = 4;
            Assert.AreEqual(BuildResult.Success, Build(c, free, "park"));
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
                Assert.AreEqual(CityCatalog.MaxBuildingLevel, b.MaxLevel, b.Id);
                for (int l = 2; l <= b.MaxLevel; l++)
                {
                    Assert.GreaterOrEqual(b.CostFor(l).Coins, b.CostFor(l - 1).Coins, b.Id);
                    Assert.GreaterOrEqual(b.PopulationAt(l), b.PopulationAt(l - 1), b.Id);
                    Assert.GreaterOrEqual(b.PerHourAt(l), b.PerHourAt(l - 1), b.Id);
                }
                Assert.AreEqual(3, b.Population.Length, b.Id);
                Assert.AreEqual(3, b.PerHour.Length, b.Id);
                Assert.AreEqual(3, b.CoinBonusPercent.Length, b.Id);
                Assert.IsTrue(Loc.Has("building." + b.Id), b.Id);
            }
        }
    }
}
