using JellyDrop.Core;
using JellyDrop.Core.City;
using JellyDrop.Core.Economy;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class CityMissionTests
    {
        private FixedClock _clock;
        private EconomyService _economy;
        private CityService _city;

        private CityMissionService New()
        {
            _clock = new FixedClock(20000);
            var data = new PlayerData { coins = 100000, wood = 500, brick = 500, glass = 500 };
            _economy = new EconomyService(data, Catalog.CreateDefault(), new EconomyConfig(), _clock);
            data.coins = 100000;
            _city = new CityService(_economy);
            return new CityMissionService(_economy, _city, seed: 3);
        }

        [Test]
        public void NoMissionsBeforeTheVillageHasPeople()
        {
            var m = New();
            _clock.UnixSeconds += 3600;
            Assert.IsNull(m.Offer);
            _city.Build(0, "house");
            _clock.UnixSeconds += 600;
            Assert.IsNotNull(m.Offer);
        }

        [Test]
        public void SuccessPaysAndSchedulesTheNextOne()
        {
            var m = New();
            _city.Build(0, "house");
            _clock.UnixSeconds += 3600;
            var offer = m.Offer;
            Assert.IsNotNull(offer);
            int coins = _economy.Coins;
            var reward = m.Finish(true);
            Assert.IsNotNull(reward);
            Assert.AreEqual(coins + reward.Coins, _economy.Coins);
            Assert.AreEqual(1, m.MissionsDone);
            Assert.IsNull(m.Offer, "the next one comes later");
            _clock.UnixSeconds += 46 * 60;
            Assert.IsNotNull(m.Offer);
        }

        [Test]
        public void FailingPaysNothingAndOffersExpire()
        {
            var m = New();
            _city.Build(0, "house");
            _clock.UnixSeconds += 3600;
            Assert.IsNotNull(m.Offer);
            int coins = _economy.Coins;
            Assert.IsNull(m.Finish(false));
            Assert.AreEqual(coins, _economy.Coins);
            _clock.UnixSeconds += 11 * 60;
            Assert.IsNotNull(m.Offer);
            _clock.UnixSeconds += CityMissionService.OfferSeconds + 1;
            Assert.IsNull(m.Offer, "an ignored offer goes away");
        }

        [Test]
        public void EveryTypeHasATimeAndTapCount()
        {
            for (int t = 0; t < CityMissionService.TypeCount; t++)
            {
                var type = (CityMissionType)t;
                Assert.Greater(CityMissionService.TimeLimit(type), 5f);
                Assert.Greater(CityMissionService.TapsNeeded(type), 0);
                Assert.IsTrue(Loc.Has("mission." + type), type.ToString());
                Assert.IsTrue(Loc.Has("mission." + type + ".do"), type.ToString());
            }
        }
    }
}
