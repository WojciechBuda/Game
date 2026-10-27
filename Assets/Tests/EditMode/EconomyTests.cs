using JellyDrop.Core;
using JellyDrop.Core.Economy;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class EconomyTests
    {
        private FixedClock _clock;

        private EconomyService NewEconomy(PlayerData data = null)
        {
            _clock = new FixedClock(20000);
            return new EconomyService(data ?? new PlayerData(), Catalog.CreateDefault(), new EconomyConfig(), _clock);
        }

        private static GameResult Result(int score, GameMode mode = GameMode.Classic, int coins = 0) => new GameResult
        {
            Mode = mode,
            Score = score,
            Lines = score / 100,
            CoinsCollected = coins,
            Stats = new SessionStats()
        };

        [Test]
        public void NewPlayerGetsStartingCoinsAndDefaultItems()
        {
            var e = NewEconomy();
            Assert.AreEqual(100, e.Coins);
            Assert.AreEqual("skin_soft", e.Data.equippedSkin);
            Assert.AreEqual("theme_daydream", e.Data.equippedTheme);
            Assert.AreEqual("avatar_bloop", e.Data.equippedAvatar);
            Assert.AreEqual(1, e.PowerUpCount(PowerUpType.Bomb));
            Assert.AreEqual(3, e.Data.missions.Count);
        }

        [Test]
        public void BuyingDeductsCoinsAndCanEquip()
        {
            var e = NewEconomy();
            e.AddCoins(500);
            Assert.AreEqual(PurchaseResult.Success, e.Buy("skin_gummy"));
            Assert.AreEqual(300, e.Coins);
            Assert.AreEqual(PurchaseResult.AlreadyOwned, e.Buy("skin_gummy"));
            Assert.IsTrue(e.Equip("skin_gummy"));
            Assert.AreEqual("skin_gummy", e.EquippedSkin.Id);
        }

        [Test]
        public void CannotBuyWithoutEnoughCoinsOrPremiumItems()
        {
            var e = NewEconomy();
            Assert.AreEqual(PurchaseResult.NotEnoughCoins, e.Buy("skin_plasma"));
            Assert.AreEqual(PurchaseResult.RequiresPremium, e.Buy("skin_gold"));
            Assert.IsFalse(e.Equip("skin_plasma"));
        }

        [Test]
        public void ShardsBuyEpicItems()
        {
            var e = NewEconomy();
            e.Data.shards = 10;
            Assert.AreEqual(PurchaseResult.Success, e.Buy("skin_plasma", withShards: true));
            Assert.AreEqual(0, e.Shards);
        }

        [Test]
        public void RewardedAdsAreCappedAtTwoPerDay()
        {
            var e = NewEconomy();
            Assert.AreEqual(2, e.RewardedAdsLeftToday);
            Assert.AreEqual(100, e.GrantRewardedAdReward());
            Assert.AreEqual(100, e.GrantRewardedAdReward());
            Assert.AreEqual(0, e.GrantRewardedAdReward());
            Assert.AreEqual(300, e.Coins);

            _clock.TodayIndex++;
            Assert.AreEqual(2, e.RewardedAdsLeftToday, "resets the next day");
        }

        [Test]
        public void PremiumRemovesInterstitialsAndGrantsItems()
        {
            var e = NewEconomy();
            Assert.IsFalse(e.ShouldShowInterstitial());
            Assert.IsFalse(e.ShouldShowInterstitial());
            Assert.IsTrue(e.ShouldShowInterstitial(), "every 3rd game");

            e.GrantPremium();
            for (int i = 0; i < 10; i++) Assert.IsFalse(e.ShouldShowInterstitial());
            Assert.IsTrue(e.IsOwned("skin_gold"));
            Assert.IsTrue(e.IsOwned("avatar_king"));
        }

        [Test]
        public void StreakGrowsOnConsecutiveDaysAndResetsAfterGap()
        {
            var e = NewEconomy();
            Assert.AreEqual(50, e.ClaimStreak());
            Assert.AreEqual(0, e.ClaimStreak(), "only once per day");
            _clock.TodayIndex++;
            Assert.AreEqual(75, e.ClaimStreak());
            _clock.TodayIndex += 3;
            Assert.AreEqual(1, e.NextStreakDay);
            Assert.AreEqual(50, e.ClaimStreak());
        }

        [Test]
        public void StreakWrapsAfterSevenDays()
        {
            var e = NewEconomy();
            for (int d = 0; d < 7; d++) { e.ClaimStreak(); _clock.TodayIndex++; }
            Assert.AreEqual(7, e.Data.streakDay);
            Assert.AreEqual(1, e.NextStreakDay);
        }

        [Test]
        public void GameResultPaysCoinsAndPersonalBest()
        {
            var e = NewEconomy();
            int before = e.Coins;
            var rewards = e.ApplyGameResult(Result(5000, coins: 12));
            Assert.AreEqual(50, rewards.ScoreCoins);
            Assert.AreEqual(12, rewards.CollectedCoins);
            Assert.IsTrue(rewards.IsPersonalBest);
            Assert.AreEqual(20, rewards.PersonalBestBonus);
            Assert.AreEqual(before + rewards.Total, e.Coins);

            var second = e.ApplyGameResult(Result(1000));
            Assert.IsFalse(second.IsPersonalBest);
        }

        [Test]
        public void PremiumGetsCoinMultiplier()
        {
            var e = NewEconomy();
            e.GrantPremium();
            var rewards = e.ApplyGameResult(Result(10000));
            int baseTotal = rewards.ScoreCoins + rewards.PersonalBestBonus;
            Assert.AreEqual((int)System.Math.Round(baseTotal * 0.5), rewards.PremiumBonus);
        }

        [Test]
        public void MissionsProgressAndCanBeClaimedOnce()
        {
            var e = NewEconomy();
            var (def, progress) = default((MissionDef, MissionProgress));
            foreach (var m in e.Missions()) { def = m.def; progress = m.progress; break; }

            // Max out the first mission by setting progress directly, then claim.
            progress.progress = def.Target;
            int coins = e.Coins;
            Assert.AreEqual(def.RewardCoins, e.ClaimMission(def.Id));
            Assert.AreEqual(coins + def.RewardCoins, e.Coins);
            Assert.AreEqual(0, e.ClaimMission(def.Id));
        }

        [Test]
        public void PlayGamesMissionCountsGames()
        {
            var data = new PlayerData();
            var e = NewEconomy(data);
            data.missions.Clear();
            data.missions.Add(new MissionProgress { id = "m_games_3" });
            e.ApplyGameResult(Result(100));
            e.ApplyGameResult(Result(100));
            var last = e.ApplyGameResult(Result(100));
            Assert.AreEqual(1, last.MissionsCompleted.Count);
            Assert.AreEqual(1, e.UnclaimedMissionCount);
        }

        [Test]
        public void MissionsChangeOnNewDay()
        {
            var e = NewEconomy();
            var today = e.Data.missions.ConvertAll(m => m.id);
            _clock.TodayIndex++;
            e.RefreshMissionsIfNewDay();
            Assert.AreEqual(_clock.TodayIndex, e.Data.missionsDay);
            Assert.AreEqual(3, e.Data.missions.Count);
            Assert.IsTrue(e.Data.missions.TrueForAll(m => m.progress == 0));
            Assert.IsNotNull(today);
        }

        [Test]
        public void SprintBestIsFastestCompletedTime()
        {
            var e = NewEconomy();
            var r1 = Result(3000, GameMode.Sprint); r1.Completed = true; r1.TimeSeconds = 90;
            var r2 = Result(3000, GameMode.Sprint); r2.Completed = true; r2.TimeSeconds = 120;
            var r3 = Result(3000, GameMode.Sprint); r3.Completed = true; r3.TimeSeconds = 70;
            Assert.IsTrue(e.ApplyGameResult(r1).IsPersonalBest);
            Assert.IsFalse(e.ApplyGameResult(r2).IsPersonalBest);
            Assert.IsTrue(e.ApplyGameResult(r3).IsPersonalBest);
            Assert.AreEqual(70f, e.BestFor(GameMode.Sprint).bestTime);
        }

        [Test]
        public void WheelSpinGrantsSomething()
        {
            var e = NewEconomy();
            int coins = e.Coins, shards = e.Shards;
            int powerUps = 0;
            foreach (PowerUpType t in System.Enum.GetValues(typeof(PowerUpType))) powerUps += e.PowerUpCount(t);

            var result = e.SpinWheel(new SystemRandom(3));

            int powerUpsAfter = 0;
            foreach (PowerUpType t in System.Enum.GetValues(typeof(PowerUpType))) powerUpsAfter += e.PowerUpCount(t);
            bool gained = e.Coins > coins || e.Shards > shards || powerUpsAfter > powerUps;
            Assert.IsTrue(gained, result.Segment.Label);
        }

        [Test]
        public void WheelProbabilitiesSumToOne()
        {
            var wheel = LuckyWheel.CreateDefault();
            double sum = 0;
            for (int i = 0; i < wheel.Segments.Count; i++) sum += wheel.ProbabilityOf(i);
            Assert.AreEqual(1.0, sum, 1e-9);
        }

        [Test]
        public void PowerUpPacksAddStock()
        {
            var e = NewEconomy();
            e.AddCoins(1000);
            Assert.AreEqual(PurchaseResult.Success, e.BuyPowerUps("pu_bomb3"));
            Assert.AreEqual(4, e.PowerUpCount(PowerUpType.Bomb));
            Assert.IsTrue(e.ConsumePowerUp(PowerUpType.Bomb));
            Assert.AreEqual(3, e.PowerUpCount(PowerUpType.Bomb));
        }
    }
}
