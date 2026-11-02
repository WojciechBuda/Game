using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Pet;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class PetTests
    {
        private FixedClock _clock;
        private EconomyService _economy;

        private PetService New(int coins = 1000)
        {
            _clock = new FixedClock(100000);
            var data = new PlayerData();
            _economy = new EconomyService(data, Catalog.CreateDefault(), new EconomyConfig(), _clock);
            data.coins = coins;
            return new PetService(_economy);
        }

        [Test]
        public void ANewPetStartsHappy()
        {
            var p = New();
            Assert.AreEqual(80f, p.Food, 0.01f);
            Assert.AreEqual(PetMood.Happy, p.Mood);
            Assert.AreEqual(PetService.HappyCoinBonus, p.CoinBonusPercent);
            Assert.IsFalse(p.NeedsAttention);
        }

        [Test]
        public void NeedsDropOverTimeEvenWhileTheGameIsClosed()
        {
            var p = New();
            _clock.UnixSeconds += 4 * 3600;
            Assert.AreEqual(80f - 4 * 5f, p.Food, 0.1f);
            Assert.AreEqual(80f - 4 * 4f, p.Energy, 0.1f);
            Assert.AreEqual(80f - 4 * 3f, p.Clean, 0.1f);
        }

        [Test]
        public void IgnoredForLongItGetsLonelyThenSick()
        {
            var p = New();
            _clock.UnixSeconds += 11 * 3600;
            Assert.AreEqual(PetMood.Hungry, p.Mood);
            _clock.UnixSeconds += 14 * 3600;
            Assert.IsTrue(p.IsSick);
            Assert.AreEqual(PetMood.Sick, p.Mood);
            Assert.AreEqual(0, p.CoinBonusPercent);
            Assert.IsTrue(p.NeedsAttention);
        }

        [Test]
        public void MedicineCuresForCoins()
        {
            var p = New();
            Assert.AreEqual(PetResult.NotSick, p.Heal());
            _clock.UnixSeconds += 30 * 3600;
            Assert.IsTrue(p.IsSick);
            int coins = _economy.Data.coins;
            Assert.AreEqual(PetResult.Ok, p.Heal());
            Assert.AreEqual(coins - PetCatalog.MedicineCost, _economy.Data.coins);
            Assert.IsFalse(p.IsSick);
            Assert.GreaterOrEqual(p.Food, 40f);
        }

        [Test]
        public void FeedingCostsCoinsAndStopsWhenFull()
        {
            var p = New(coins: 30);
            _clock.UnixSeconds += 6 * 3600; // food 50
            Assert.AreEqual(PetResult.Ok, p.Feed("burger"));
            Assert.AreEqual(100f, p.Food, 0.01f);
            Assert.AreEqual(5, _economy.Data.coins);
            Assert.AreEqual(PetResult.Full, p.Feed("apple"));
            Assert.AreEqual(PetResult.Unknown, p.Feed("teddy"));
        }

        [Test]
        public void NoCoinsNoFood()
        {
            var p = New(coins: 0);
            _clock.UnixSeconds += 6 * 3600;
            Assert.AreEqual(PetResult.NotEnoughCoins, p.Feed("apple"));
        }

        [Test]
        public void SleepingRestoresEnergyAndWakesUpByItself()
        {
            var p = New();
            _clock.UnixSeconds += 5 * 3600; // energy 60
            Assert.AreEqual(PetResult.Ok, p.Sleep());
            Assert.AreEqual(PetMood.Sleeping, p.Mood);
            Assert.AreEqual(PetResult.Sleeping, p.Feed("apple"));
            _clock.UnixSeconds += 2 * 3600;
            Assert.IsFalse(p.IsSleeping);
            Assert.GreaterOrEqual(p.Energy, 95f); // woke up when full, then a little awake time
        }

        [Test]
        public void WashingAndStrokingAreFreeButStrokingIsCapped()
        {
            var p = New(coins: 0);
            _clock.UnixSeconds += 8 * 3600;
            Assert.AreEqual(PetResult.Ok, p.Wash());
            Assert.AreEqual(100f, p.Clean, 0.01f);
            Assert.AreEqual(PetResult.AlreadyClean, p.Wash());
            for (int i = 0; i < 50; i++) p.Stroke();
            Assert.AreEqual(PetService.StrokeCap, p.Fun, 0.01f);
        }

        [Test]
        public void GiftsDressingAndPlayingCheerItUp()
        {
            var p = New();
            _clock.UnixSeconds += 8 * 3600; // fun 40
            Assert.AreEqual(PetResult.Ok, p.Gift("teddy"));
            Assert.AreEqual(100f, p.Fun, 0.01f);
            _clock.UnixSeconds += 8 * 3600;
            float fun = p.Fun;
            for (int i = 0; i < 5; i++) p.OnDressed();
            Assert.AreEqual(fun + 8f * PetService.DressBonusesPerDay, p.Fun, 0.1f);
            float energy = p.Energy;
            p.OnGamePlayed();
            Assert.AreEqual(energy - 5f, p.Energy, 0.1f);
        }

        [Test]
        public void CaringResetsLoneliness()
        {
            var p = New();
            _clock.UnixSeconds += 23 * 3600;
            p.Wash();
            _clock.UnixSeconds += 2 * 3600;
            Assert.IsFalse(p.IsLonely);
        }
    }
}
