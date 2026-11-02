using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Islands;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class ForestTests
    {
        private FixedClock _clock;
        private EconomyService _e;

        private ForestService New()
        {
            _clock = new FixedClock(100000);
            _e = new EconomyService(new PlayerData(), Catalog.CreateDefault(), new EconomyConfig(), _clock);
            _e.Data.coins = 100000;
            _e.Data.wood = 0;
            return new ForestService(_e, seed: 5);
        }

        [Test]
        public void MushroomsArePickedAndGrowBack()
        {
            var f = New();
            Assert.AreEqual(5, f.ReadyMushrooms, "the clearing has 5 spots without the hut");
            Assert.AreNotEqual(ForestPick.None, f.PickSpot(0));
            Assert.AreEqual(ForestPick.None, f.PickSpot(0), "gone until it grows back");
            Assert.AreEqual(1, _e.Data.mushrooms + _e.Data.goldMushrooms);
            Assert.AreEqual(ForestPick.None, f.PickSpot(7), "the thicket is not open yet");
            _clock.UnixSeconds += ForestCatalog.MushroomRegrowSeconds + 100;
            Assert.IsTrue(f.IsSpotReady(0));
        }

        [Test]
        public void TreesGiveWoodAndBushesBerries()
        {
            var f = New();
            Assert.AreEqual(2, f.Chop(0));
            Assert.AreEqual(0, f.Chop(0));
            Assert.Less(f.TreeGrowth(0), 0.1f);
            Assert.AreEqual(ForestCatalog.BerriesPerBush, f.PickBush(0));
            Assert.AreEqual(2, _e.Data.wood);
            Assert.AreEqual(ForestCatalog.BerriesPerBush, _e.Data.berries);
        }

        [Test]
        public void GatheringLevelsTheForestAndOpensAreas()
        {
            var f = New();
            Assert.AreEqual(1, f.Level);
            Assert.AreEqual(1, f.AreasOpen);
            _e.Data.forestXp = ForestCatalog.LevelXp[2];
            Assert.AreEqual(3, f.Level);
            Assert.AreEqual(2, f.AreasOpen);
            Assert.IsTrue(f.IsSpotOpen(7));
            Assert.IsTrue(f.IsTreeOpen(5));
        }

        [Test]
        public void BuildingsCostGoodsUnlockByLevelAndHelp()
        {
            var f = New();
            Assert.AreEqual(ForestBuildResult.NotEnoughWood, f.Upgrade(ForestBuilding.MushroomHut));
            _e.Data.wood = 500;
            _e.Data.mushrooms = 100;
            Assert.AreEqual(ForestBuildResult.Ok, f.Upgrade(ForestBuilding.MushroomHut));
            Assert.IsTrue(f.IsSpotOpen(5), "the hut adds a spot");
            Assert.Less(f.MushroomRegrowSeconds, ForestCatalog.MushroomRegrowSeconds);
            Assert.AreEqual(ForestBuildResult.Locked, f.Upgrade(ForestBuilding.Herbalist));
            _e.Data.forestXp = ForestCatalog.LevelXp[4];
            Assert.AreEqual(ForestBuildResult.Ok, f.Upgrade(ForestBuilding.Apiary));
            _clock.UnixSeconds += 3 * 3600;
            Assert.AreEqual(6, f.HoneyPending);
            Assert.AreEqual(6, f.CollectHoney());
            Assert.AreEqual(ForestBuildResult.Ok, f.Upgrade(ForestBuilding.Herbalist));
            int mush = _e.Data.mushrooms;
            Assert.IsTrue(f.Brew().HasValue);
            Assert.AreEqual(mush - f.BrewCost, _e.Data.mushrooms);
        }

        [Test]
        public void TheFoxComesBackLaterAndPaysWhenCaught()
        {
            var f = New();
            _e.Data.forestXp = ForestCatalog.LevelXp[1];
            Assert.IsFalse(f.FoxReady);
            _clock.UnixSeconds += 11 * 60;
            Assert.IsTrue(f.FoxReady);
            Assert.AreEqual(ForestService.FoxMushrooms, f.FinishFox(true));
            Assert.IsFalse(f.FoxReady);
            Assert.GreaterOrEqual(_e.Data.mushrooms, ForestService.FoxMushrooms);
        }
    }
}
