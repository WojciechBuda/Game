using JellyDrop.Core;
using JellyDrop.Core.Economy;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class WardrobeTests
    {
        private static EconomyService NewEconomy(int coins)
        {
            var data = new PlayerData();
            var e = new EconomyService(data, Catalog.CreateDefault(), new EconomyConfig(), new FixedClock(10));
            data.coins = coins;
            return e;
        }

        [Test]
        public void EveryItemHasAPriceAndAnId()
        {
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (var w in Wardrobe.All)
            {
                Assert.IsTrue(ids.Add(w.Id), "duplicate " + w.Id);
                Assert.Greater(w.CoinPrice, 0);
                Assert.IsFalse(string.IsNullOrEmpty(w.Color));
                if (w.Slot != WardrobeSlot.Color) Assert.IsFalse(string.IsNullOrEmpty(w.Style));
            }
        }

        [Test]
        public void BuyingCostsCoinsAndPutsTheItemOn()
        {
            var e = NewEconomy(1000);
            Assert.IsTrue(e.Look.IsEmpty);
            Assert.AreEqual(PurchaseResult.Success, e.BuyWardrobe("hat_cap"));
            Assert.AreEqual(1000 - Wardrobe.Get("hat_cap").CoinPrice, e.Coins);
            Assert.IsTrue(e.IsWearing("hat_cap"));
            Assert.AreEqual("cap", e.Look.Hat);
            Assert.AreEqual(PurchaseResult.AlreadyOwned, e.BuyWardrobe("hat_cap"));
        }

        [Test]
        public void CannotBuyWithoutCoins()
        {
            var e = NewEconomy(10);
            Assert.AreEqual(PurchaseResult.NotEnoughCoins, e.BuyWardrobe("hat_crown"));
            Assert.AreEqual(10, e.Coins);
            Assert.IsFalse(e.OwnsWardrobe("hat_crown"));
        }

        [Test]
        public void OneItemPerSlotAndToggleTakesItOff()
        {
            var e = NewEconomy(5000);
            e.BuyWardrobe("hat_cap");
            e.BuyWardrobe("hat_beanie");
            Assert.IsTrue(e.IsWearing("hat_beanie"));
            Assert.IsFalse(e.IsWearing("hat_cap"));
            e.BuyWardrobe("glasses_sun");
            e.BuyWardrobe("color_mint");
            var look = e.Look;
            Assert.AreEqual("beanie", look.Hat);
            Assert.AreEqual("sun", look.Glasses);
            Assert.AreEqual("#3DDBA8", look.BodyColor);
            Assert.IsTrue(e.ToggleWear("hat_beanie"));
            Assert.IsNull(e.Look.Hat);
            Assert.IsTrue(e.ToggleWear("hat_cap"));
            Assert.AreEqual("cap", e.Look.Hat);
            Assert.IsFalse(e.ToggleWear("hat_crown"), "not owned");
        }
    }
}
