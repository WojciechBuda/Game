using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Islands;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class GardenTests
    {
        private FixedClock _clock;
        private EconomyService _e;

        private GardenService New()
        {
            _clock = new FixedClock(200000);
            _e = new EconomyService(new PlayerData(), Catalog.CreateDefault(), new EconomyConfig(), _clock);
            _e.Data.coins = 100000;
            return new GardenService(_e, seed: 2);
        }

        [Test]
        public void PlantWaterAndHarvest()
        {
            var g = New();
            Assert.AreEqual(GardenCatalog.StartBeds, g.OpenBeds);
            Assert.AreEqual(GardenResult.Locked, g.Plant(0, CropType.Pumpkin), "pumpkins need a higher garden level");
            Assert.AreEqual(GardenResult.Ok, g.Plant(0, CropType.Carrot));
            Assert.AreEqual(GardenResult.BedBusy, g.Plant(0, CropType.Carrot));
            Assert.AreEqual(0, g.Harvest(0));
            Assert.AreEqual(GardenResult.Ok, g.Water(0));
            Assert.AreEqual(GardenResult.BedBusy, g.Water(0), "once per planting");
            Assert.Less(g.SecondsLeft(0), GardenCatalog.Crop(CropType.Carrot).GrowSeconds);
            _clock.UnixSeconds += GardenCatalog.Crop(CropType.Carrot).GrowSeconds;
            Assert.IsTrue(g.IsRipe(0));
            Assert.AreEqual(3, g.Harvest(0));
            Assert.AreEqual(3, g.Crops(CropType.Carrot));
            Assert.IsTrue(g.IsEmpty(0));
            Assert.Greater(g.Xp, 0);
        }

        [Test]
        public void LevelsOpenBedsAndCrops()
        {
            var g = New();
            _e.Data.gardenXp = GardenCatalog.LevelXp[3];
            Assert.AreEqual(4, g.Level);
            Assert.AreEqual(GardenCatalog.StartBeds + 6, g.OpenBeds);
            Assert.IsTrue(g.IsCropUnlocked(CropType.Pumpkin));
        }

        [Test]
        public void BuildingsHelp()
        {
            var g = New();
            _e.Data.wood = 500; _e.Data.brick = 500; _e.Data.mushrooms = 20;
            _e.Data.gardenXp = GardenCatalog.LevelXp[2];
            Assert.AreEqual(GardenResult.Ok, g.Upgrade(GardenBuilding.Greenhouse));
            Assert.Less(g.GrowSeconds(CropType.Carrot), GardenCatalog.Crop(CropType.Carrot).GrowSeconds);
            Assert.AreEqual(GardenResult.Ok, g.Upgrade(GardenBuilding.Well));
            g.Plant(0, CropType.Carrot); g.Plant(1, CropType.Tomato);
            Assert.AreEqual(2, g.WaterAll());
            Assert.AreEqual(GardenResult.Ok, g.Upgrade(GardenBuilding.Compost));
            Assert.AreEqual(GardenResult.Ok, g.MakeCompost());
            Assert.AreEqual(1, g.Compost);
            g.Plant(2, CropType.Carrot);
            Assert.IsTrue(g.HasCompost(2));
            _clock.UnixSeconds += 3600;
            Assert.AreEqual(4, g.Harvest(2), "compost gives one more");
            Assert.AreEqual(GardenResult.Ok, g.Upgrade(GardenBuilding.Coop));
            _clock.UnixSeconds += 2 * 3600;
            Assert.AreEqual(4, g.CollectEggs());
        }

        [Test]
        public void KitchenCooksDishesWithBonuses()
        {
            var g = New();
            var cake = GardenCatalog.Recipes[2];
            Assert.IsFalse(g.CanCook(cake));
            _e.Data.crops[(int)CropType.Strawberry] = 3; _e.Data.eggs = 2; _e.Data.honey = 1;
            Assert.IsTrue(g.Cook(cake));
            Assert.AreEqual(2, _e.CoinBoostGames);
            Assert.AreEqual(0, _e.Data.eggs);
            Assert.IsFalse(g.Cook(cake));
        }

        [Test]
        public void CropsSellAndCrowsComeBack()
        {
            var g = New();
            _e.Data.crops[0] = 2; _e.Data.crops[3] = 1;
            int coins = _e.Coins;
            Assert.AreEqual(2 * 6 + 45, g.SellAllCrops());
            Assert.AreEqual(coins + 57, _e.Coins);
            _e.Data.gardenXp = GardenCatalog.LevelXp[1];
            g.Plant(0, CropType.Carrot);
            _clock.UnixSeconds += 16 * 60;
            Assert.IsTrue(g.CrowsReady || g.IsRipe(0));
            g.FinishCrows(true);
            Assert.IsFalse(g.CrowsReady);
        }
    }
}
