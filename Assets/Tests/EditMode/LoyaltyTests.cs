using System.Linq;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Levels;
using JellyDrop.Core.Loyalty;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class LoyaltyTests
    {
        private FixedClock _clock;
        private EconomyService _economy;

        private LoyaltyService NewService(long unix = SeasonPass.Epoch + 3600)
        {
            _clock = new FixedClock(20000) { UnixSeconds = unix };
            _economy = new EconomyService(new PlayerData(), Catalog.CreateDefault(), new EconomyConfig(), _clock);
            return new LoyaltyService(_economy);
        }

        private static GameResult Game(int lines, int pieces = 40, GameMode mode = GameMode.Classic) =>
            new GameResult { Mode = mode, Lines = lines, Stats = new SessionStats { PiecesPlaced = pieces } };

        [Test]
        public void LevelCurveIsIncreasingAndConsistent()
        {
            Assert.AreEqual(1, PlayerLevels.LevelFor(0));
            Assert.AreEqual(1, PlayerLevels.LevelFor(99));
            Assert.AreEqual(2, PlayerLevels.LevelFor(100));
            for (int l = 1; l < 60; l++)
            {
                Assert.Greater(PlayerLevels.XpForNext(l + 1), PlayerLevels.XpForNext(l));
                Assert.AreEqual(l, PlayerLevels.LevelFor(PlayerLevels.TotalXpFor(l)));
                Assert.AreEqual(l, PlayerLevels.LevelFor(PlayerLevels.TotalXpFor(l + 1) - 1));
            }
            Assert.AreEqual(PlayerLevels.MaxLevel, PlayerLevels.LevelFor(int.MaxValue / 2));
        }

        [Test]
        public void LevelUpPaysRewardsAndUnlocksLevelItems()
        {
            var s = NewService();
            int coins = _economy.Coins;
            var gain = s.AddPoints(PlayerLevels.TotalXpFor(5));
            Assert.AreEqual(1, gain.LevelBefore);
            Assert.AreEqual(5, gain.LevelAfter);
            Assert.IsTrue(gain.LeveledUp);
            int expected = 0;
            for (int l = 2; l <= 5; l++) expected += PlayerLevels.RewardFor(l, _economy.Catalog).Coins;
            Assert.AreEqual(coins + expected, _economy.Coins);
            Assert.IsTrue(_economy.IsOwned("skin_bubble"));
            Assert.AreEqual(1, s.CoinBonusPercent);
        }

        [Test]
        public void EveryLevelRewardItemExists()
        {
            var catalog = Catalog.CreateDefault();
            foreach (var item in catalog.Items.Where(i => i.UnlockLevel > 0))
                Assert.AreEqual(item.Id, PlayerLevels.RewardFor(item.UnlockLevel, catalog).ItemId);
        }

        [Test]
        public void RewardItemsAreNotSoldAndNotOwnedAtStart()
        {
            NewService();
            foreach (var item in _economy.Catalog.Items.Where(i => i.IsReward))
            {
                Assert.IsFalse(_economy.IsOwned(item.Id), item.Id);
                Assert.AreEqual(PurchaseResult.NotForSale, _economy.CanBuy(item.Id), item.Id);
            }
        }

        [Test]
        public void SeasonCalendar()
        {
            Assert.AreEqual(1, SeasonPass.SeasonIndex(SeasonPass.Epoch));
            Assert.AreEqual(1, SeasonPass.SeasonIndex(SeasonPass.Epoch + SeasonPass.SeasonSeconds - 1));
            Assert.AreEqual(2, SeasonPass.SeasonIndex(SeasonPass.Epoch + SeasonPass.SeasonSeconds));
            Assert.AreEqual(SeasonPass.SeasonSeconds, SeasonPass.SecondsLeft(SeasonPass.Epoch));
            Assert.AreEqual(1, SeasonPass.SecondsLeft(SeasonPass.Epoch + SeasonPass.SeasonSeconds - 1));
        }

        [Test]
        public void EverySeasonHasRealExclusiveItems()
        {
            var catalog = Catalog.CreateDefault();
            for (int season = 1; season <= 8; season++)
            {
                foreach (var id in SeasonPass.ItemsFor(season))
                {
                    var item = catalog.Get(id);
                    Assert.IsNotNull(item, id);
                    Assert.IsTrue(item.SeasonReward, id);
                }
                Assert.IsNotNull(SeasonPass.FreeReward(season, 25).ItemId);
                Assert.IsNotNull(SeasonPass.PassReward(season, 15).ItemId);
                Assert.IsNotNull(SeasonPass.PassReward(season, SeasonPass.Tiers).ItemId);
            }
            for (int t = 1; t <= SeasonPass.Tiers; t++)
            {
                Assert.IsFalse(SeasonPass.FreeReward(1, t).IsEmpty, "free " + t);
                Assert.IsFalse(SeasonPass.PassReward(1, t).IsEmpty, "pass " + t);
            }
        }

        [Test]
        public void PointsFillTiersAndFreeRewardsCanBeClaimedOnce()
        {
            var s = NewService();
            s.AddPoints(SeasonPass.PointsPerTier * 3 + SeasonPass.PointsPerTier * 2 / 5);
            Assert.AreEqual(3, s.Tier);
            Assert.AreEqual(0.4f, s.TierProgress, 0.001f);
            Assert.AreEqual(3, s.ClaimableCount);
            Assert.IsTrue(s.CanClaim(1, false));
            Assert.IsFalse(s.CanClaim(4, false));
            Assert.IsFalse(s.CanClaim(1, true), "pass rewards need the pass");
            Assert.IsNotNull(s.Claim(1, false));
            Assert.IsNull(s.Claim(1, false));
            Assert.AreEqual(2, s.ClaimableCount);
        }

        [Test]
        public void PassUnlocksPassRewardsIncludingEarlierTiers()
        {
            var s = NewService();
            s.AddPoints(SeasonPass.PointsPerTier * 15);
            s.GrantPass();
            Assert.IsTrue(s.HasPass);
            Assert.AreEqual(30, s.ClaimableCount);
            var all = s.ClaimAll();
            Assert.AreEqual(0, s.ClaimableCount);
            Assert.IsTrue(_economy.IsOwned(SeasonPass.ItemsFor(s.Season)[1]));
            Assert.Greater(all.Coins, 0);
        }

        [Test]
        public void SeasonPointsAreCappedButXpKeepsGrowing()
        {
            var s = NewService();
            int max = SeasonPass.Tiers * SeasonPass.PointsPerTier;
            s.AddPoints(max + 5000);
            Assert.AreEqual(max, s.Points);
            Assert.AreEqual(SeasonPass.Tiers, s.Tier);
            Assert.AreEqual(max + 5000, s.Xp);
        }

        [Test]
        public void NewSeasonResetsAndAutoClaimsLeftovers()
        {
            var s = NewService();
            Reward carried = null;
            s.SeasonRolledOver += r => carried = r;
            s.AddPoints(SeasonPass.PointsPerTier * 2);
            s.GrantPass();
            int season = s.Season;
            int xp = s.Xp;

            _clock.UnixSeconds += SeasonPass.SeasonSeconds;
            Assert.AreEqual(season + 1, s.Season);
            Assert.AreEqual(0, s.Points);
            Assert.IsFalse(s.HasPass);
            Assert.AreEqual(0, s.ClaimableCount);
            Assert.AreEqual(xp, s.Xp, "player level never resets");
            Assert.IsNotNull(carried);
            Assert.IsFalse(carried.IsEmpty);
        }

        [Test]
        public void GamesAndMissionsGivePoints()
        {
            var s = NewService();
            Assert.AreEqual(0, LoyaltyService.PointsFor(Game(10, pieces: 2), null), "instant quit earns nothing");
            Assert.AreEqual(30, LoyaltyService.PointsFor(Game(10), null));
            Assert.AreEqual(50, LoyaltyService.PointsFor(Game(100), null), "line points are capped");
            var outcome = new LevelOutcome { Level = LevelPath.Get(3), Won = true, Stars = 2 };
            Assert.AreEqual(30 + 40, LoyaltyService.PointsFor(Game(10, mode: GameMode.Level), outcome));
            var boost = new LevelOutcome { Level = LevelPath.Get(10), Won = true, Stars = 1 };
            Assert.AreEqual(30 + 60, LoyaltyService.PointsFor(Game(10, mode: GameMode.Boost), boost));

            var gain = s.AwardGame(Game(10), null);
            Assert.AreEqual(30, gain.Points);
            Assert.AreEqual(30, s.Points);

            // Complete and claim a mission.
            var p = _economy.Data.missions[0];
            p.progress = 1000000;
            _economy.ClaimMission(p.id);
            Assert.AreEqual(30 + LoyaltyService.PointsPerMission, s.Points);
        }

        [Test]
        public void OldSavesMigrateToVersion4()
        {
            var data = new PlayerData { version = 3, coins = 500 };
            var e = new EconomyService(data, Catalog.CreateDefault(), new EconomyConfig(), new FixedClock(20000));
            Assert.AreEqual(PlayerData.CurrentVersion, e.Data.version);
            Assert.AreEqual(4, PlayerData.CurrentVersion);
            Assert.AreEqual(0, e.Data.xp);
        }
    }
}
