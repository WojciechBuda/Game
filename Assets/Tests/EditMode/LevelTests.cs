using System.Collections.Generic;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Levels;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class LevelTests
    {
        private static EconomyService Economy() =>
            new EconomyService(new PlayerData(), Catalog.CreateDefault(), new EconomyConfig(), new FixedClock(20000));

        private static GameSession Session(LevelDef def)
        {
            var config = LevelPath.ConfigFor(def, new GameConfig());
            config.Lucky.GoldenPieceChance = 0;
            config.Lucky.CoinPieceChance = 0;
            config.Lucky.RainbowChancePerClear = 0;
            config.Lucky.JackpotChancePerClear = 0;
            var s = new GameSession(config, def.IsBoost ? GameMode.Boost : GameMode.Level, new SystemRandom(def.Seed));
            s.Start();
            return s;
        }

        private static GameResult Result(int level, int score, bool completed = true) => new GameResult
        {
            Mode = LevelPath.IsBoost(level) ? GameMode.Boost : GameMode.Level,
            LevelNumber = level,
            Score = score,
            Completed = completed,
            Stats = new SessionStats()
        };

        [Test]
        public void EveryTenthLevelIsABoost()
        {
            Assert.IsFalse(LevelPath.Get(9).IsBoost);
            Assert.IsTrue(LevelPath.Get(10).IsBoost);
            Assert.IsTrue(LevelPath.Get(20).IsBoost);
            Assert.AreEqual(90f, LevelPath.Get(10).TimeLimitSeconds, 0.001);
            Assert.AreEqual(0, LevelPath.Get(10).TargetScore);
        }

        [Test]
        public void TargetsGrowAndStarsAreOrdered()
        {
            int previous = 0;
            for (int n = 1; n < 60; n++)
            {
                var d = LevelPath.Get(n);
                if (d.IsBoost) continue;
                Assert.Greater(d.TargetScore, previous);
                previous = d.TargetScore;
                Assert.AreEqual(d.TargetScore, d.StarScores[0]);
                Assert.Greater(d.StarScores[1], d.StarScores[0]);
                Assert.Greater(d.StarScores[2], d.StarScores[1]);
                Assert.Greater(d.PieceLimit, 20);
            }
        }

        [Test]
        public void SameLevelAlwaysGivesTheSamePieces()
        {
            var a = Session(LevelPath.Get(7));
            var b = Session(LevelPath.Get(7));
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(a.Current.Type, b.Current.Type);
                a.HardDrop();
                b.HardDrop();
                if (a.IsOver || b.IsOver) break;
            }
        }

        [Test]
        public void LevelEndsWhenPiecesRunOut()
        {
            var def = LevelPath.Get(1);
            var s = Session(def);
            GameResult ended = null;
            s.GameEnded += r => ended = r;
            int guard = 0;
            while (!s.IsOver && guard++ < 200)
            {
                // Spread pieces out so the stack doesn't top out first.
                for (int k = 0; k < guard % 5; k++) s.MoveLeft();
                for (int k = 0; k < (guard / 5) % 5; k++) s.MoveRight();
                s.HardDrop();
                if (s.Board.StackHeight() > 14) s.Board.Clear();
            }
            Assert.IsNotNull(ended);
            Assert.AreEqual(def.PieceLimit, ended.Stats.PiecesPlaced);
            Assert.AreEqual(1, ended.LevelNumber);
            Assert.AreEqual(ended.Score >= def.TargetScore, ended.Completed);
            Assert.AreEqual(0, s.PiecesLeft);
        }

        [Test]
        public void TargetReachedFiresOnce()
        {
            var def = LevelPath.Get(1);
            var s = Session(def);
            int fired = 0;
            s.TargetReached += () => fired++;
            for (int round = 0; round < 8 && !s.IsOver; round++)
            {
                for (int y = 0; y < 4; y++)
                    for (int x = 0; x < 9; x++) s.Board[x, y] = BoardCell.Of(PieceType.Z);
                s.Current.Type = PieceType.I;
                s.Current.Rotation = 0;
                s.Current.Origin = PieceData.SpawnOrigin(PieceType.I, 10, s.Config.SpawnRow);
                s.RotateClockwise();
                while (s.MoveRight()) { }
                s.HardDrop();
            }
            Assert.IsTrue(s.TargetHit);
            Assert.AreEqual(1, fired);
        }

        [Test]
        public void BoostEndsOnTimerAndAlwaysPays()
        {
            var def = LevelPath.Get(10);
            var s = Session(def);
            GameResult ended = null;
            s.GameEnded += r => ended = r;
            var start = s.Current.Origin;
            for (int i = 0; i < 2000 && !s.IsOver; i++)
            {
                s.Current.Origin = start; // hover so nothing locks
                s.Tick(0.05f);
            }
            Assert.IsNotNull(ended);
            Assert.IsTrue(ended.Completed);
            Assert.AreEqual(90f, ended.TimeSeconds, 0.01);
            Assert.AreEqual(0f, s.TimeLeft, 0.001);
        }

        [Test]
        public void AbandoningNeverWins()
        {
            var s = Session(LevelPath.Get(10));
            GameResult ended = null;
            s.GameEnded += r => ended = r;
            s.Abandon();
            Assert.IsFalse(ended.Completed);
        }

        [Test]
        public void WinningUnlocksNextLevelAndPaysOnce()
        {
            var e = Economy();
            var p = new ProgressService(e);
            Assert.AreEqual(1, p.CurrentLevel);
            Assert.IsFalse(p.IsUnlocked(2));

            var def = LevelPath.Get(1);
            int coins = e.Coins;
            var won = p.Apply(Result(1, def.StarScores[2]));
            Assert.IsTrue(won.Won);
            Assert.IsTrue(won.FirstClear);
            Assert.AreEqual(3, won.Stars);
            Assert.AreEqual(coins + won.Reward.Coins, e.Coins);
            Assert.Greater(won.Reward.Wood, 0);
            Assert.AreEqual(2, p.CurrentLevel);
            Assert.IsTrue(p.IsUnlocked(2));

            var replay = p.Apply(Result(1, def.StarScores[2]));
            Assert.IsFalse(replay.FirstClear);
            Assert.AreEqual(won.Reward.Coins / 2, replay.Reward.Coins);
            Assert.AreEqual(0, replay.Reward.Wood);
            Assert.AreEqual(2, p.CurrentLevel);
        }

        [Test]
        public void LosingPaysNothingAndReportsShortfall()
        {
            var e = Economy();
            var p = new ProgressService(e);
            var def = LevelPath.Get(1);
            int coins = e.Coins;
            var lost = p.Apply(Result(1, def.TargetScore - 120, completed: false));
            Assert.IsFalse(lost.Won);
            Assert.AreEqual(120, lost.PointsShort);
            Assert.AreEqual(coins, e.Coins);
            Assert.AreEqual(1, p.CurrentLevel);
        }

        [Test]
        public void StarsOnlyEverGoUp()
        {
            var e = Economy();
            var p = new ProgressService(e);
            var def = LevelPath.Get(1);
            p.Apply(Result(1, def.StarScores[2]));
            p.Apply(Result(1, def.StarScores[0]));
            Assert.AreEqual(3, p.StarsOf(1));
        }

        [Test]
        public void BoostGivesSuppliesScaledByScore()
        {
            var small = LevelPath.RewardFor(LevelPath.Get(10), 2000, 1, true);
            var big = LevelPath.RewardFor(LevelPath.Get(10), 20000, 3, true);
            Assert.Greater(big.Coins, small.Coins);
            Assert.Greater(big.Wood, small.Wood);
            Assert.Greater(big.Glass, 0);
        }

        [Test]
        public void LevelModesDoNotTouchArcadeBests()
        {
            var e = Economy();
            var r = e.ApplyGameResult(Result(3, 50000));
            Assert.IsFalse(r.IsPersonalBest);
            Assert.AreEqual(0, e.BestFor(GameMode.Classic, Difficulty.Normal).bestScore);
        }
    }
}
