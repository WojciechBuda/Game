using System.Collections.Generic;
using JellyDrop.Core;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    /// <summary>Scripted random source: returns queued values, then falls back to a seeded generator.</summary>
    public sealed class ScriptedRandom : IRandom
    {
        private readonly SystemRandom _fallback = new SystemRandom(1);
        public readonly Queue<double> Doubles = new Queue<double>();

        public int Range(int minInclusive, int maxExclusive) => _fallback.Range(minInclusive, maxExclusive);
        public double NextDouble() => Doubles.Count > 0 ? Doubles.Dequeue() : 0.99;
    }

    public class PieceDataTests
    {
        [Test]
        public void EveryPieceHasFourCellsInEveryRotation()
        {
            for (int t = 0; t < PieceData.StandardPieceCount; t++)
                for (int r = 0; r < 4; r++)
                    Assert.AreEqual(4, PieceData.GetCells((PieceType)t, r).Length);
        }

        [Test]
        public void FourClockwiseRotationsReturnToSpawnState()
        {
            for (int t = 0; t < PieceData.StandardPieceCount; t++)
            {
                var spawn = new HashSet<Int2>(PieceData.GetCells((PieceType)t, 0));
                var again = new HashSet<Int2>(PieceData.GetCells((PieceType)t, 4));
                Assert.IsTrue(spawn.SetEquals(again), ((PieceType)t).ToString());
            }
        }

        [Test]
        public void IPieceVerticalStateIsColumnTwo()
        {
            foreach (var c in PieceData.GetCells(PieceType.I, 1)) Assert.AreEqual(2, c.x);
        }

        [Test]
        public void PiecesSpawnCentred()
        {
            var origin = PieceData.SpawnOrigin(PieceType.T, 10, 18);
            Assert.AreEqual(3, origin.x);
            var o = PieceData.SpawnOrigin(PieceType.O, 10, 18);
            Assert.AreEqual(4, o.x);
            Assert.AreEqual(18, o.y);
        }
    }

    public class BoardTests
    {
        [Test]
        public void FullRowsAreClearedAndStackCollapses()
        {
            var board = new Board(4, 6, 2);
            for (int x = 0; x < 4; x++) board[x, 0] = BoardCell.Of(PieceType.I);
            board[1, 1] = BoardCell.Of(PieceType.T);
            for (int x = 0; x < 4; x++) board[x, 2] = BoardCell.Of(PieceType.O);

            var cleared = board.ClearFullRows();

            Assert.AreEqual(2, cleared.Count);
            Assert.IsFalse(board[1, 0].IsEmpty, "T cell fell to the bottom");
            Assert.AreEqual(PieceType.T, board[1, 0].PieceType);
            Assert.AreEqual(1, board.StackHeight());
        }

        [Test]
        public void ColumnGravityDropsFloatingCells()
        {
            var board = new Board(3, 5, 1);
            board[0, 3] = BoardCell.Of(PieceType.S);
            board.ApplyColumnGravity();
            Assert.IsFalse(board[0, 0].IsEmpty);
            Assert.IsTrue(board[0, 3].IsEmpty);
        }
    }

    public class GameSessionTests
    {
        private static GameSession NewSession(GameMode mode = GameMode.Classic, IRandom rng = null, GameConfig config = null)
        {
            config = config ?? new GameConfig();
            // Disable lucky randomness unless a test asks for it.
            config.Lucky.GoldenPieceChance = 0;
            config.Lucky.CoinPieceChance = 0;
            config.Lucky.RainbowChancePerClear = 0;
            config.Lucky.JackpotChancePerClear = 0;
            var session = new GameSession(config, mode, rng ?? new SystemRandom(42));
            session.Start();
            return session;
        }

        private static void FillRowExcept(Board board, int row, params int[] gaps)
        {
            var skip = new HashSet<int>(gaps);
            for (int x = 0; x < board.Width; x++)
                if (!skip.Contains(x)) board[x, row] = BoardCell.Of(PieceType.Z);
        }

        [Test]
        public void HardDropLocksPieceAtBottomAndSpawnsNext()
        {
            var s = NewSession();
            var first = s.Current;
            int distance = s.HardDrop();
            Assert.Greater(distance, 10);
            Assert.AreNotSame(first, s.Current);
            Assert.AreEqual(1, s.Stats.PiecesPlaced);
            Assert.Greater(s.Board.StackHeight(), 0);
            Assert.AreEqual(distance * s.Config.HardDropScorePerCell, s.Score);
        }

        [Test]
        public void GravityMovesPieceDownOverTime()
        {
            var s = NewSession();
            int startY = s.Current.Origin.y;
            s.Tick(s.Config.FallSecondsForLevel(1) + 0.01f);
            Assert.AreEqual(startY - 1, s.Current.Origin.y);
        }

        [Test]
        public void PieceLocksAfterLockDelayWhenGrounded()
        {
            var s = NewSession();
            var piece = s.Current;
            while (!s.IsGrounded) s.Tick(0.05f);
            Assert.AreSame(piece, s.Current);
            s.Tick(s.Config.LockDelaySeconds + 0.01f);
            Assert.AreNotSame(piece, s.Current);
        }

        [Test]
        public void MovingStopsAtWalls()
        {
            var s = NewSession();
            int moves = 0;
            while (s.MoveLeft()) moves++;
            Assert.Less(moves, 10);
            foreach (var c in s.Current.BoardCells()) Assert.GreaterOrEqual(c.x, 0);
            Assert.IsFalse(s.MoveLeft());
        }

        [Test]
        public void ClearingFourLinesScoresQuadAndCoins()
        {
            var s = NewSession();
            for (int y = 0; y < 4; y++) FillRowExcept(s.Board, y, 9);

            LineClearResult result = null;
            s.LinesCleared += r => result = r;

            // Force an I piece, stand it up and drop it into column 9.
            s.UsePowerUp(PowerUpType.Swap);
            Assert.AreEqual(PieceType.I, s.Current.Type);
            Assert.IsTrue(s.RotateClockwise());
            while (s.MoveRight()) { }
            s.HardDrop();

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.LineCount);
            Assert.IsTrue(result.IsQuad);
            Assert.AreEqual(800, result.ScoreGained);
            Assert.AreEqual(4 * s.Config.CoinsPerLine, result.CoinsGained);
            Assert.AreEqual(4, s.Lines);
            Assert.AreEqual(0, s.Board.StackHeight());
            Assert.AreEqual(1, s.Stats.Quads);
        }

        [Test]
        public void GoldenCellsMultiplyCoins()
        {
            var s = NewSession();
            FillRowExcept(s.Board, 0, 0);
            s.Board[5, 0] = BoardCell.Of(PieceType.T, CellFlags.Golden | CellFlags.Coin);

            var events = new List<LuckyEventType>();
            s.Lucky += e => events.Add(e.Type);
            LineClearResult result = null;
            s.LinesCleared += r => result = r;

            s.UsePowerUp(PowerUpType.Swap);
            s.RotateClockwise();
            while (s.MoveLeft()) { }
            s.HardDrop();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.HadGolden);
            int expected = (1 * s.Config.CoinsPerLine + s.Config.CoinsPerCoinCell) * s.Config.Lucky.GoldenCoinMultiplier;
            Assert.AreEqual(expected, result.CoinsGained);
            CollectionAssert.Contains(events, LuckyEventType.GoldenClear);
            CollectionAssert.Contains(events, LuckyEventType.CoinCell);
        }

        [Test]
        public void JackpotRollAddsCoins()
        {
            var config = new GameConfig();
            var rng = new ScriptedRandom();
            var s = NewSession(GameMode.Classic, rng, config);
            config.Lucky.JackpotChancePerClear = 0.5;
            FillRowExcept(s.Board, 0, 0);
            rng.Doubles.Enqueue(0.1); // jackpot roll succeeds

            bool jackpot = false;
            s.Lucky += e => jackpot |= e.Type == LuckyEventType.Jackpot;
            s.UsePowerUp(PowerUpType.Swap);
            s.RotateClockwise();
            while (s.MoveLeft()) { }
            s.HardDrop();

            Assert.IsTrue(jackpot);
            Assert.AreEqual(1, s.Stats.Jackpots);
            Assert.GreaterOrEqual(s.CoinsCollected, config.Lucky.JackpotCoins);
        }

        [Test]
        public void HoldSwapsOncePerPiece()
        {
            var s = NewSession();
            var firstType = s.Current.Type;
            Assert.IsTrue(s.Hold());
            Assert.AreEqual(firstType, s.Held);
            Assert.IsFalse(s.Hold(), "cannot hold twice in a row");
            s.HardDrop();
            Assert.IsTrue(s.Hold());
        }

        [Test]
        public void PowerUpsAreLimitedPerGame()
        {
            var s = NewSession();
            Assert.IsTrue(s.UsePowerUp(PowerUpType.SlowMo));
            Assert.IsFalse(s.UsePowerUp(PowerUpType.SlowMo), "slow-mo already running");
            Assert.IsTrue(s.UsePowerUp(PowerUpType.Bomb));
            Assert.AreEqual(0, s.PowerUpsLeft);
            Assert.IsFalse(s.UsePowerUp(PowerUpType.LineZap));
        }

        [Test]
        public void BombClearsAreaAndGivesPieceBack()
        {
            var s = NewSession();
            for (int y = 0; y < 3; y++) FillRowExcept(s.Board, y, 0, 1);
            var original = s.Current.Type;

            List<Int2> destroyed = null;
            s.BombExploded += d => destroyed = d;
            Assert.IsTrue(s.UsePowerUp(PowerUpType.Bomb));
            Assert.AreEqual(PieceType.Bomb, s.Current.Type);
            s.HardDrop();

            Assert.IsNotNull(destroyed);
            Assert.Greater(destroyed.Count, 0);
            Assert.AreEqual(original, s.Current.Type, "the replaced piece comes back next");
        }

        [Test]
        public void LineZapRemovesBottomRow()
        {
            var s = NewSession();
            FillRowExcept(s.Board, 0, 3);
            s.Board[2, 1] = BoardCell.Of(PieceType.J);
            Assert.IsTrue(s.UsePowerUp(PowerUpType.LineZap));
            Assert.AreEqual(PieceType.J, s.Board[2, 0].PieceType);
            Assert.AreEqual(1, s.Board.StackHeight());
        }

        [Test]
        public void LineZapNeverPushesBlocksIntoTheFallingPiece()
        {
            var s = NewSession();
            // Overhang: a roof at row 5 over an empty cavity, with the piece tucked under it.
            for (int x = 0; x < s.Board.Width; x++) s.Board[x, 0] = BoardCell.Of(PieceType.Z);
            s.Board[0, 1] = BoardCell.Empty;
            for (int x = 0; x < s.Board.Width; x++) s.Board[x, 5] = BoardCell.Of(PieceType.J);
            s.UsePowerUp(PowerUpType.Swap); // I piece, horizontal
            s.Current.Origin = new Int2(3, 2); // cells on row 4, right under the roof
            Assert.IsTrue(s.Board.Fits(s.Current.Type, s.Current.Rotation, s.Current.Origin));

            Assert.IsTrue(s.UsePowerUp(PowerUpType.LineZap));

            Assert.IsTrue(s.Board.Fits(s.Current.Type, s.Current.Rotation, s.Current.Origin));
        }

        [Test]
        public void ToppingOutEndsGame()
        {
            var s = NewSession();
            GameResult ended = null;
            s.GameEnded += r => ended = r;
            for (int i = 0; i < 200 && !s.IsOver; i++) s.HardDrop();
            Assert.IsTrue(s.IsOver);
            Assert.IsNotNull(ended);
            Assert.IsFalse(ended.Completed);
        }

        [Test]
        public void SprintCompletesAfterTargetLines()
        {
            var config = new GameConfig { SprintLines = 4 };
            var s = NewSession(GameMode.Sprint, null, config);
            for (int y = 0; y < 4; y++) FillRowExcept(s.Board, y, 9);
            s.UsePowerUp(PowerUpType.Swap);
            s.RotateClockwise();
            while (s.MoveRight()) { }
            s.HardDrop();
            Assert.IsTrue(s.IsOver);
            Assert.IsTrue(s.Completed);
        }

        [Test]
        public void LevelRisesEveryTenLines()
        {
            var config = new GameConfig { MaxPowerUpsPerGame = 10 };
            var s = NewSession(GameMode.Classic, null, config);
            int levelUps = 0;
            s.LevelUp += _ => levelUps++;
            for (int round = 0; round < 3; round++)
            {
                for (int y = 0; y < 4; y++) FillRowExcept(s.Board, y, 9);
                s.UsePowerUp(PowerUpType.Swap);
                s.RotateClockwise();
                while (s.MoveRight()) { }
                s.HardDrop();
            }
            Assert.AreEqual(12, s.Lines);
            Assert.AreEqual(2, s.Level);
            Assert.AreEqual(1, levelUps);
        }

        [Test]
        public void BackToBackQuadsGetBonus()
        {
            var config = new GameConfig { MaxPowerUpsPerGame = 10 };
            var s = NewSession(GameMode.Classic, null, config);
            var results = new List<LineClearResult>();
            s.LinesCleared += r => results.Add(r);
            for (int round = 0; round < 2; round++)
            {
                for (int y = 0; y < 4; y++) FillRowExcept(s.Board, y, 9);
                s.UsePowerUp(PowerUpType.Swap);
                s.RotateClockwise();
                while (s.MoveRight()) { }
                s.HardDrop();
            }
            Assert.AreEqual(2, results.Count);
            Assert.IsFalse(results[0].BackToBack);
            Assert.IsTrue(results[1].BackToBack);
            // 800 * 1.5 plus combo bonus 50 * 1
            Assert.AreEqual(1200 + 50, results[1].ScoreGained);
        }

        [Test]
        public void SevenBagContainsEachPieceOnce()
        {
            var gen = new PieceGenerator(new SystemRandom(7), new LuckyConfig(), 3);
            var seen = new HashSet<PieceType>();
            for (int i = 0; i < 7; i++) seen.Add(gen.Next().Type);
            Assert.AreEqual(7, seen.Count);
        }

        [Test]
        public void SameSeedGivesSamePieceOrder()
        {
            var a = new PieceGenerator(new SystemRandom(99), new LuckyConfig(), 3);
            var b = new PieceGenerator(new SystemRandom(99), new LuckyConfig(), 3);
            for (int i = 0; i < 30; i++) Assert.AreEqual(a.Next().Type, b.Next().Type);
        }
    }
}
