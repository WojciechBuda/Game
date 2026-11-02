using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Tower;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class TowerTests
    {
        private static void RunBot(TowerSession s, float seconds = 600f)
        {
            for (int k = 0; k < seconds * 60 && !s.IsOver && !s.IsWon; k++)
            {
                TowerBot.Decide(s, out var move, out var jump);
                s.Step(1f / 60f, move, jump);
            }
        }

        [Test]
        public void SameSeedSameClimb()
        {
            var a = new TowerSession(TowerConfig.Endless(TowerMode.Classic, TowerScenery.Ice), 42);
            var b = new TowerSession(TowerConfig.Endless(TowerMode.Classic, TowerScenery.Ice), 42);
            RunBot(a, 60f);
            RunBot(b, 60f);
            Assert.AreEqual(a.HighestFloor, b.HighestFloor);
            Assert.AreEqual(a.Score, b.Score);
        }

        [Test]
        public void RunningFasterJumpsHigher()
        {
            var slow = new TowerSession(TowerConfig.Endless(TowerMode.Classic, TowerScenery.Ice), 1);
            slow.Step(1f / 60f, 0f, true);
            var fast = new TowerSession(TowerConfig.Endless(TowerMode.Classic, TowerScenery.Ice), 1);
            for (int i = 0; i < 20; i++) fast.Step(1f / 60f, 1f, false);
            fast.Step(1f / 60f, 1f, true);
            Assert.Greater(fast.Vy, slow.Vy + 5f);
            Assert.GreaterOrEqual(slow.Vy, TowerSession.JumpBase - 1f);
        }

        [Test]
        public void WallsBounceTheAvatarBack()
        {
            var s = new TowerSession(TowerConfig.Endless(TowerMode.Classic, TowerScenery.Ice), 1);
            bool bounced = false;
            s.Bounced += () => bounced = true;
            for (int i = 0; i < 120 && !bounced; i++) s.Step(1f / 60f, 1f, false);
            Assert.IsTrue(bounced);
            Assert.LessOrEqual(s.X, TowerSession.Width);
        }

        [Test]
        public void StandingStillTheTowerEventuallyWins()
        {
            var s = new TowerSession(TowerConfig.Endless(TowerMode.Classic, TowerScenery.Ice), 1);
            for (int i = 0; i < 60 * 60 && !s.IsOver; i++) s.Step(1f / 60f, 0f, false);
            Assert.IsTrue(s.IsOver);
            Assert.AreEqual(0, s.HighestFloor);
        }

        [Test]
        public void ABotClimbsTheEndlessTowerInEveryMode()
        {
            foreach (TowerMode mode in System.Enum.GetValues(typeof(TowerMode)))
            {
                int total = 0;
                for (int seed = 1; seed <= 5; seed++)
                {
                    var s = new TowerSession(TowerConfig.Endless(mode, TowerScenery.Space), seed);
                    RunBot(s);
                    Assert.IsTrue(s.IsOver, "the endless tower has no top");
                    total += s.HighestFloor;
                }
                Assert.Greater(total / 5, 40, mode.ToString());
            }
        }

        [Test]
        public void TheFirstTowersCanBeClimbedByASimpleBot()
        {
            for (int i = 0; i < 5; i++)
            {
                int wins = 0;
                for (int seed = 1; seed <= 6; seed++)
                {
                    var s = new TowerSession(TowerCatalog.Get(i).Config(), seed);
                    RunBot(s);
                    if (s.IsWon) wins++;
                }
                Assert.GreaterOrEqual(wins, 4, "tower " + i);
            }
        }

        [Test]
        public void TowersHaveATopFloorAndGetHarder()
        {
            for (int i = 0; i < TowerCatalog.Count; i++)
            {
                var d = TowerCatalog.Get(i);
                Assert.Greater(d.Floors, 0);
                if (i > 0) Assert.Greater(d.Floors, TowerCatalog.Get(i - 1).Floors);
                if (i > 0) Assert.Greater(d.Reward, TowerCatalog.Get(i - 1).Reward);
                var s = new TowerSession(d.Config(), 3);
                Assert.IsNotNull(s.PlatformAt(1));
            }
        }

        [Test]
        public void ClearingATowerPaysOnceBigAndUnlocksTheNext()
        {
            var data = new PlayerData();
            var economy = new EconomyService(data, Catalog.CreateDefault(), new EconomyConfig(), new FixedClock(1000));
            data.coins = 0;
            var t = new TowerService(economy);
            Assert.IsTrue(t.IsUnlocked(0));
            Assert.IsFalse(t.IsUnlocked(1));
            Assert.AreEqual(0, t.FinishTower(0, false, out _));
            int paid = t.FinishTower(0, true, out bool first);
            Assert.IsTrue(first);
            Assert.AreEqual(TowerCatalog.Get(0).Reward, paid);
            Assert.IsTrue(t.IsUnlocked(1));
            Assert.AreEqual(TowerCatalog.Get(0).ReplayReward, t.FinishTower(0, true, out first));
            Assert.IsFalse(first);

            int coins = t.FinishEndless(TowerMode.Speed, 120, 500, out bool record);
            Assert.IsTrue(record);
            Assert.AreEqual(120 / 5 + 500 / 50, coins);
            Assert.AreEqual(120, t.Best(TowerMode.Speed));
            t.FinishEndless(TowerMode.Speed, 50, 0, out record);
            Assert.IsFalse(record);
            Assert.AreEqual(120, t.Best(TowerMode.Speed));
        }
    }
}
