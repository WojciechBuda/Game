using System.Linq;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using NUnit.Framework;

namespace JellyDrop.Tests
{
    public class LocalizationTests
    {
        [Test]
        public void EveryKeyIsTranslatedIntoEveryLanguage()
        {
            foreach (var key in Loc.Keys)
            {
                var row = Loc.Row(key);
                Assert.AreEqual(Loc.LanguageCount, row.Length, key);
                for (int i = 0; i < row.Length; i++)
                    Assert.IsFalse(string.IsNullOrWhiteSpace(row[i]), $"{key} is empty in {(Language)i}");
            }
        }

        [Test]
        public void PlaceholdersMatchAcrossLanguages()
        {
            foreach (var key in Loc.Keys)
            {
                var row = Loc.Row(key);
                var english = Loc.PlaceholdersIn(row[0]);
                for (int i = 1; i < row.Length; i++)
                    Assert.IsTrue(english.SetEquals(Loc.PlaceholdersIn(row[i])), $"{key}: placeholders differ in {(Language)i}");
            }
        }

        [Test]
        public void GameContentHasTranslations()
        {
            foreach (var d in DifficultySettings.All)
            {
                Assert.IsTrue(Loc.Has("difficulty." + d.ToString().ToLowerInvariant()), d.ToString());
                Assert.IsTrue(Loc.Has("difficulty." + d.ToString().ToLowerInvariant() + ".desc"), d.ToString());
            }
            foreach (PowerUpType t in System.Enum.GetValues(typeof(PowerUpType)))
            {
                Assert.IsTrue(Loc.Has("powerup." + t + ".name"), t.ToString());
                Assert.IsTrue(Loc.Has("powerup." + t + ".short"), t.ToString());
                Assert.IsTrue(Loc.Has("powerup." + t + ".desc"), t.ToString());
            }
            foreach (MissionType t in System.Enum.GetValues(typeof(MissionType)))
                Assert.IsTrue(Loc.Has("mission." + t), t.ToString());
            foreach (Rarity r in System.Enum.GetValues(typeof(Rarity)))
                Assert.IsTrue(Loc.Has("rarity." + r), r.ToString());
            foreach (MusicStyle m in System.Enum.GetValues(typeof(MusicStyle)))
                Assert.IsTrue(Loc.Has("music." + m), m.ToString());
            // Skins and themes are translated; character avatars (Bloop, Choco...) keep their names.
            foreach (var item in Catalog.CreateDefault().Items.Where(i => i.Category != ItemCategory.Avatar))
                Assert.IsTrue(Loc.Has("item." + item.Id), item.Id);
        }

        [Test]
        public void SwitchingLanguageChangesText()
        {
            var previous = Loc.Current;
            try
            {
                Loc.Current = Language.English;
                Assert.AreEqual("PLAY", Loc.T("home.play"));
                Loc.Current = Language.Polish;
                Assert.AreEqual("GRAJ", Loc.T("home.play"));
                Assert.AreEqual("POZIOM 3  ·  LINIE: 12", Loc.T("hud.level_lines", 3, 12));
                Loc.Current = Language.German;
                Assert.AreEqual("SCHWER", Loc.DifficultyName(Difficulty.Hard));
            }
            finally { Loc.Current = previous; }
        }

        [Test]
        public void MissingKeysFallBackToTheKey()
        {
            Assert.AreEqual("no.such.key", Loc.T("no.such.key"));
        }

        [Test]
        public void PolishCoinPluralsAreCorrect()
        {
            var previous = Loc.Current;
            try
            {
                Loc.Current = Language.Polish;
                Assert.AreEqual("1 moneta", Loc.Coins(1));
                Assert.AreEqual("2 monety", Loc.Coins(2));
                Assert.AreEqual("4 monety", Loc.Coins(4));
                Assert.AreEqual("5 monet", Loc.Coins(5));
                Assert.AreEqual("12 monet", Loc.Coins(12));
                Assert.AreEqual("22 monety", Loc.Coins(22));
                Assert.AreEqual("100 monet", Loc.Coins(100));
                Loc.Current = Language.English;
                Assert.AreEqual("1 coin", Loc.Coins(1));
                Assert.AreEqual("1,250 coins", Loc.Coins(1250));
                Loc.Current = Language.French;
                Assert.AreEqual("0 pièce", Loc.Coins(0));
            }
            finally { Loc.Current = previous; }
        }

        [Test]
        public void NumbersUseTheLanguageSeparator()
        {
            var previous = Loc.Current;
            try
            {
                Loc.Current = Language.English; Assert.AreEqual("12,345", Loc.Number(12345));
                Loc.Current = Language.German; Assert.AreEqual("12.345", Loc.Number(12345));
                Loc.Current = Language.Polish; Assert.AreEqual("12 345", Loc.Number(12345));
                Assert.AreEqual("1250", Loc.Number(1250));
                Loc.Current = Language.Spanish; Assert.AreEqual("1250", Loc.Number(1250));
                Assert.AreEqual("1,5", Loc.Decimal(1.5f));
            }
            finally { Loc.Current = previous; }
        }

        [Test]
        public void IsoCodesMapToLanguages()
        {
            Assert.AreEqual(Language.Polish, Loc.FromIsoCode("pl"));
            Assert.AreEqual(Language.Portuguese, Loc.FromIsoCode("pt-BR"));
            Assert.AreEqual(Language.German, Loc.FromIsoCode("DE"));
            Assert.AreEqual(Language.English, Loc.FromIsoCode("ja"));
            Assert.AreEqual(Language.English, Loc.FromIsoCode(null));
        }

        [Test]
        public void ChosenLanguageIsSaved()
        {
            var data = new PlayerData();
            var e = new EconomyService(data, Catalog.CreateDefault(), new EconomyConfig(), new FixedClock(1));
            Assert.IsNull(e.ChosenLanguage, "follows the device by default");
            e.SetLanguage(Language.French);
            Assert.AreEqual((int)Language.French, data.language);
            Assert.AreEqual(Language.French, e.ChosenLanguage);
            e.SetLanguage(null);
            Assert.AreEqual(-1, data.language);
        }
    }
}
