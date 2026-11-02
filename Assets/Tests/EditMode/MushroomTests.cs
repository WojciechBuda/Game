using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Pet;
using JellyDrop.Core.Runner;
using JellyDrop.Core.Tower;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class MushroomTests
    {
        private static EconomyService NewEconomy()
        {
            var e = new EconomyService(new PlayerData(), Catalog.CreateDefault(), new EconomyConfig(), new FixedClock(500));
            e.Data.coins = 1000;
            return e;
        }

        [Test]
        public void PiecesCarryMushroomsWhenEnabled()
        {
            var lucky = new LuckyConfig { GoldenPieceChance = 0, CoinPieceChance = 0, MushroomPieceChance = 1 };
            var gen = new PieceGenerator(new SystemRandom(3), lucky, 3);
            var p = gen.Next();
            Assert.IsTrue(p.Mushroom);
            Assert.GreaterOrEqual(p.CoinCellIndex, 0);
        }

        [Test]
        public void MushroomsFromAGameArePaid()
        {
            var config = new GameConfig();
            var s = new GameSession(config, GameMode.Classic, new SystemRandom(1));
            s.Start();
            var e = NewEconomy();
            var result = s.GetResult();
            result.Mushrooms = 3;
            var rewards = e.ApplyGameResult(result);
            Assert.AreEqual(3, rewards.Mushrooms);
            Assert.AreEqual(3, e.Mushrooms);
        }

        [Test]
        public void RunsDropMushroomsAndAFourthLifeCostsThree()
        {
            var s = new RunnerSession(9, RunnerSession.StartLives + 1);
            Assert.AreEqual(4, s.Lives);
            var e = NewEconomy();
            e.Data.mushrooms = 4;
            e.Data.runExtraLife = true;
            Assert.IsTrue(e.UseMushroomBoost(ref e.Data.runExtraLife));
            Assert.AreEqual(1, e.Mushrooms);
            Assert.IsFalse(e.UseMushroomBoost(ref e.Data.runExtraLife), "not enough left");
            Assert.IsFalse(e.Data.runExtraLife, "switches itself off");
            e.ApplyRunResult(0, 0f, false, 5);
            Assert.AreEqual(6, e.Mushrooms);
        }

        [Test]
        public void TowerSpringStartLaunchesUp()
        {
            var cfg = TowerConfig.Endless(TowerMode.Classic, TowerScenery.Forest);
            cfg.SpringStart = true;
            var s = new TowerSession(cfg, 4);
            for (int i = 0; i < 60; i++) s.Step(1f / 60f, 0f, false);
            Assert.Greater(s.Y, TowerSession.Gap * 2f);
        }

        [Test]
        public void GoodsSellAtTheMarketAndFeedThePet()
        {
            var e = NewEconomy();
            e.Data.mushrooms = 5; e.Data.goldMushrooms = 1; e.Data.berries = 4; e.Data.honey = 2;
            int coins = e.Coins;
            Assert.AreEqual(EconomyService.GoldMushroomPrice, e.SellGoods("gold_mushroom", 3));
            Assert.AreEqual(0, e.Data.goldMushrooms);
            Assert.AreEqual(coins + EconomyService.GoldMushroomPrice, e.Coins);

            var pet = new PetService(e);
            var clock = (FixedClock)e.Clock;
            clock.UnixSeconds += 10 * 3600;
            int before = e.Coins;
            Assert.AreEqual(PetResult.Ok, pet.Feed("mushroom_soup"));
            Assert.AreEqual(3, e.Mushrooms);
            Assert.AreEqual(before, e.Coins, "soup costs mushrooms, not coins");
            e.Data.mushrooms = 0;
            Assert.AreEqual(PetResult.NotEnoughGoods, pet.Feed("mushroom_soup"));
        }
    }
}
