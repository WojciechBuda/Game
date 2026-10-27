using System.Collections.Generic;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class DifficultyTests
    {
        private static GameSession Session(Difficulty d, int seed = 42)
        {
            var config = DifficultySettings.For(d).Apply(new GameConfig());
            config.Lucky.GoldenPieceChance = 0;
            config.Lucky.CoinPieceChance = 0;
            config.Lucky.RainbowChancePerClear = 0;
            config.Lucky.JackpotChancePerClear = 0;
            var s = new GameSession(config, GameMode.Classic, new SystemRandom(seed));
            s.Start();
            return s;
        }

        /// <summary>Turns the falling piece into an I at the spawn position (test helper).</summary>
        private static void ForceI(GameSession s)
        {
            s.Current.Type = PieceType.I;
            s.Current.Rotation = 0;
            s.Current.Golden = false;
            s.Current.CoinCellIndex = -1;
            s.Current.Origin = PieceData.SpawnOrigin(PieceType.I, s.Board.Width, s.Config.SpawnRow);
        }

        [Test]
        public void ApplyDoesNotChangeTheBaseConfig()
        {
            var baseConfig = new GameConfig();
            double golden = baseConfig.Lucky.GoldenPieceChance;
            var insane = DifficultySettings.For(Difficulty.Insane).Apply(baseConfig);
            Assert.AreEqual(golden, baseConfig.Lucky.GoldenPieceChance, 1e-12);
            Assert.AreEqual(1, baseConfig.StartLevel);
            Assert.AreEqual(10, insane.StartLevel);
            Assert.IsFalse(insane.ShowGhost);
        }

        [Test]
        public void HarderDifficultiesFallFaster()
        {
            float previous = 100f;
            foreach (var d in DifficultySettings.All)
            {
                var s = Session(d);
                float fall = s.Config.FallSecondsForLevel(s.Level);
                Assert.Less((int)(fall * 10000), (int)(previous * 10000), d.ToString());
                previous = fall;
            }
        }

        [Test]
        public void HardStartsAtHigherLevelWithJunkRows()
        {
            var s = Session(Difficulty.Hard);
            Assert.AreEqual(5, s.Level);
            Assert.AreEqual(4, s.Board.StackHeight());
            for (int y = 0; y < 4; y++)
            {
                int junk = 0, empty = 0;
                for (int x = 0; x < s.Board.Width; x++)
                {
                    if (s.Board[x, y].IsEmpty) empty++;
                    else if (s.Board[x, y].PieceType == PieceType.Garbage) junk++;
                }
                Assert.AreEqual(1, empty, "one gap per junk row");
                Assert.AreEqual(s.Board.Width - 1, junk);
            }
        }

        [Test]
        public void LevelKeepsRisingFromTheStartLevel()
        {
            var s = Session(Difficulty.Hard);
            s.Board.Clear();
            int ups = 0;
            s.LevelUp += _ => ups++;
            for (int round = 0; round < 3; round++)
            {
                for (int y = 0; y < 4; y++)
                    for (int x = 0; x < 9; x++) s.Board[x, y] = BoardCell.Of(PieceType.Z);
                ForceI(s);
                s.RotateClockwise();
                while (s.MoveRight()) { }
                s.HardDrop();
            }
            Assert.AreEqual(12, s.Lines);
            Assert.AreEqual(6, s.Level);
            Assert.AreEqual(1, ups);
        }

        [Test]
        public void ScoreAndCoinsScaleWithDifficulty()
        {
            var results = new Dictionary<Difficulty, LineClearResult>();
            foreach (var d in new[] { Difficulty.Easy, Difficulty.Normal, Difficulty.Insane })
            {
                var s = Session(d);
                s.Board.Clear();
                for (int x = 0; x < 9; x++) s.Board[x, 0] = BoardCell.Of(PieceType.Z);
                s.Board[4, 0] = BoardCell.Of(PieceType.T, CellFlags.Coin);
                LineClearResult r = null;
                s.LinesCleared += x => r = x;
                ForceI(s);
                s.RotateClockwise();
                while (s.MoveRight()) { }
                s.HardDrop();
                Assert.IsNotNull(r, d.ToString());
                results[d] = r;
            }
            // Raw single = 100 * level; coins raw = 1 line + 5 coin cell = 6
            Assert.AreEqual(75, results[Difficulty.Easy].ScoreGained);
            Assert.AreEqual(100, results[Difficulty.Normal].ScoreGained);
            Assert.AreEqual(2500, results[Difficulty.Insane].ScoreGained); // level 10 * 100 * 2.5
            Assert.AreEqual(5, results[Difficulty.Easy].CoinsGained);
            Assert.AreEqual(6, results[Difficulty.Normal].CoinsGained);
            Assert.AreEqual(15, results[Difficulty.Insane].CoinsGained);
        }

        [Test]
        public void InsaneHasNoPowerUpsAndRisingJunk()
        {
            var s = Session(Difficulty.Insane);
            Assert.AreEqual(0, s.PowerUpsLeft);
            Assert.IsFalse(s.UsePowerUp(PowerUpType.SlowMo));

            int height = s.Board.StackHeight();
            bool rose = false;
            s.GarbageRose += () => rose = true;
            // Keep the piece from landing by holding it high: only tick small amounts and reset position.
            var start = s.Current.Origin;
            for (int i = 0; i < 130 && !rose; i++)
            {
                s.Current.Origin = start;
                s.Tick(0.1f);
            }
            Assert.IsTrue(rose);
            Assert.GreaterOrEqual(s.Board.StackHeight(), height);
            Assert.IsTrue(s.IsOver || s.Board.Fits(s.Current.Type, s.Current.Rotation, s.Current.Origin));
        }

        [Test]
        public void PreviewLengthMatchesDifficulty()
        {
            Assert.AreEqual(3, Session(Difficulty.Easy).Preview.Count);
            Assert.AreEqual(2, Session(Difficulty.Hard).Preview.Count);
            Assert.AreEqual(1, Session(Difficulty.Insane).Preview.Count);
        }

        [Test]
        public void BestsAreKeptPerDifficulty()
        {
            var e = new EconomyService(new PlayerData(), Catalog.CreateDefault(), new EconomyConfig(), new FixedClock(1));
            var easy = new GameResult { Mode = GameMode.Classic, Score = 9000, Difficulty = Difficulty.Easy, Stats = new SessionStats() };
            var hard = new GameResult { Mode = GameMode.Classic, Score = 3000, Difficulty = Difficulty.Hard, Stats = new SessionStats() };
            Assert.IsTrue(e.ApplyGameResult(easy).IsPersonalBest);
            Assert.IsTrue(e.ApplyGameResult(hard).IsPersonalBest, "a lower score is still a best on another difficulty");
            Assert.AreEqual(9000, e.BestFor(GameMode.Classic, Difficulty.Easy).bestScore);
            Assert.AreEqual(3000, e.BestFor(GameMode.Classic, Difficulty.Hard).bestScore);
            Assert.AreEqual(0, e.BestFor(GameMode.Classic, Difficulty.Normal).bestScore);
        }

        [Test]
        public void SelectedDifficultyIsSavedAndDailyIsAlwaysNormal()
        {
            var data = new PlayerData();
            var e = new EconomyService(data, Catalog.CreateDefault(), new EconomyConfig(), new FixedClock(1));
            Assert.AreEqual(Difficulty.Normal, e.SelectedDifficulty);
            e.SelectDifficulty(Difficulty.Insane);
            Assert.AreEqual((int)Difficulty.Insane, data.selectedDifficulty);
            Assert.AreEqual(Difficulty.Insane, e.DifficultyFor(GameMode.Classic));
            Assert.AreEqual(Difficulty.Normal, e.DifficultyFor(GameMode.Daily));
        }

        [Test]
        public void OldSavesSwitchFromOldDefaultLookToNeon()
        {
            var data = new PlayerData { version = 1, totalGames = 3, equippedSkin = "skin_jelly", equippedTheme = "theme_candy" };
            data.owned.Add("skin_jelly");
            data.owned.Add("theme_candy");
            var e = new EconomyService(data, Catalog.CreateDefault(), new EconomyConfig(), new FixedClock(1));
            Assert.AreEqual("skin_neon", data.equippedSkin);
            Assert.AreEqual("theme_arcade", data.equippedTheme);
            Assert.IsTrue(e.IsOwned("skin_jelly"), "old items stay owned");
        }

        [Test]
        public void OldSavesMigrateBestsToNormal()
        {
            var data = new PlayerData { version = 1, totalGames = 5 };
            data.owned.Add("skin_jelly");
            data.bests.Add(new ModeBest { mode = (int)GameMode.Classic, difficulty = 0, bestScore = 12345 });
            var e = new EconomyService(data, Catalog.CreateDefault(), new EconomyConfig(), new FixedClock(1));
            Assert.AreEqual(12345, e.BestFor(GameMode.Classic, Difficulty.Normal).bestScore);
            Assert.AreEqual(PlayerData.CurrentVersion, data.version);
        }
    }
}
