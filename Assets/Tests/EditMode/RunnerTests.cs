using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Runner;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class RunnerTests
    {
        private static RunnerSession New(int seed = 3)
        {
            var s = new RunnerSession(seed);
            s.Start();
            return s;
        }

        [Test]
        public void EveryRowLeavesAWayThrough()
        {
            for (int seed = 0; seed < 30; seed++)
            {
                var s = New(seed);
                for (int i = 0; i < 2000 && !s.IsOver; i++) { s.Tick(0.05f); }
                foreach (var row in s.Rows)
                {
                    int crates = 0;
                    foreach (var o in row.Lanes) if (o == RunObstacle.Block) crates++;
                    Assert.Less(crates, RunnerSession.LaneCount);
                }
            }
        }

        [Test]
        public void LanesClampAtTheEdges()
        {
            var s = New();
            Assert.AreEqual(1, s.Lane);
            s.Left(); s.Left(); s.Left();
            Assert.AreEqual(0, s.Lane);
            s.Right(); s.Right(); s.Right();
            Assert.AreEqual(2, s.Lane);
        }

        [Test]
        public void JumpGoesUpAndComesBackDown()
        {
            var s = New();
            s.Jump();
            s.Tick(RunnerSession.JumpSeconds / 2f);
            Assert.Greater(s.Height, 1.5f);
            s.Tick(RunnerSession.JumpSeconds);
            Assert.AreEqual(0f, s.Height);
            Assert.IsFalse(s.IsJumping);
        }

        /// <summary>A bot that dodges perfectly should survive a long time, so the track is always fair.</summary>
        [Test]
        public void PerfectPlayerSurvives()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var s = New(seed);
                for (int i = 0; i < 6000 && !s.IsOver; i++)
                {
                    Bot(s);
                    s.Tick(1f / 60f);
                }
                Assert.IsFalse(s.IsOver, $"seed {seed} crashed at {s.Distance:0}");
                Assert.Greater(s.Distance, 1000f);
                // 100 seconds in: the speed has risen all the way to the cap.
                Assert.AreEqual(RunnerSession.MaxSpeed, s.Speed, 0.001f);
            }
        }

        [Test]
        public void ThreeHitsEndTheRun()
        {
            var s = New(5);
            bool crashed = false;
            int hits = 0;
            s.Crashed += () => crashed = true;
            s.Hit += lives => { hits++; Assert.AreEqual(RunnerSession.StartLives - hits, lives); Assert.Greater(s.Invincible, 0f); };
            Assert.AreEqual(3, s.Lives);
            for (int i = 0; i < 20000 && !s.IsOver; i++) s.Tick(1f / 60f); // never dodges
            Assert.IsTrue(s.IsOver);
            Assert.IsTrue(crashed);
            Assert.AreEqual(2, hits);
            Assert.AreEqual(0, s.Lives);
        }

        [Test]
        public void CoinsAreCollected()
        {
            int total = 0;
            for (int seed = 0; seed < 10; seed++)
            {
                var s = New(seed);
                for (int i = 0; i < 3000 && !s.IsOver; i++) { Bot(s, chaseCoins: true); s.Tick(1f / 60f); }
                total += s.Coins;
            }
            Assert.Greater(total, 50);
        }

        [Test]
        public void OneFreeRunPerDayAndBoostTicketsStack()
        {
            var clock = new FixedClock(100);
            var e = new EconomyService(new PlayerData(), Catalog.CreateDefault(), new EconomyConfig(), clock);
            Assert.AreEqual(1, e.RunTickets);
            Assert.IsTrue(e.TryStartRun());
            Assert.AreEqual(0, e.RunTickets);
            Assert.IsFalse(e.TryStartRun());
            e.GrantRunTicket(2);
            Assert.AreEqual(2, e.RunTickets);
            clock.TodayIndex = 101;
            Assert.AreEqual(2, e.RunTickets, "the daily free run does not stack on top of saved tickets");
            e.TryStartRun(); e.TryStartRun();
            clock.TodayIndex = 102;
            Assert.AreEqual(1, e.RunTickets);
        }

        [Test]
        public void RunRewardsCoinsAndDistanceAndKeepsTheBest()
        {
            var e = new EconomyService(new PlayerData(), Catalog.CreateDefault(), new EconomyConfig(), new FixedClock(5));
            int before = e.Coins;
            int earned = e.ApplyRunResult(30, 800f);
            Assert.AreEqual(30 + 800 / EconomyService.RunMetresPerCoin, earned);
            Assert.AreEqual(before + earned, e.Coins);
            Assert.AreEqual(800, e.Data.runBest);
            e.ApplyRunResult(0, 100f);
            Assert.AreEqual(800, e.Data.runBest);
        }

        // Looks at the next row and picks a safe lane, jumping / sliding when needed.
        private static void Bot(RunnerSession s, bool chaseCoins = false)
        {
            RunRow next = null;
            foreach (var r in s.Rows)
                if (r.Z + RunnerSession.BlockLength >= s.Distance - 0.5f && (next == null || r.Z < next.Z)) next = r;
            if (next == null) return;
            float ahead = next.Z - s.Distance;
            int best = s.Lane;
            if (next.Lanes[s.Lane] == RunObstacle.Block)
            {
                for (int l = 0; l < RunnerSession.LaneCount; l++)
                    if (next.Lanes[l] != RunObstacle.Block && System.Math.Abs(l - s.Lane) == 1) best = l;
                if (best == s.Lane)
                    for (int l = 0; l < RunnerSession.LaneCount; l++)
                        if (next.Lanes[l] != RunObstacle.Block) best = l;
            }
            else if (chaseCoins)
            {
                foreach (var c in s.CoinsOnTrack)
                    if (!c.Taken && c.Z > s.Distance && c.Z < s.Distance + 12f && next.Lanes[c.Lane] != RunObstacle.Block && System.Math.Abs(c.Lane - s.Lane) == 1 && ahead > 6f)
                    { best = c.Lane; break; }
            }
            if (best < s.Lane) s.Left();
            else if (best > s.Lane) s.Right();
            var o = next.Lanes[s.Lane];
            float lead = s.Speed * RunnerSession.JumpSeconds * 0.5f;
            if (o == RunObstacle.Low && ahead < lead && ahead > 0f && !s.IsJumping) s.Jump();
            if (o == RunObstacle.High && ahead < s.Speed * 0.3f && ahead > -0.3f && !s.IsSliding) s.Slide();
        }
    }
}
