using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using JellyDrop.Core.Economy;

namespace JellyDrop.Core
{
    /// <summary>Languages the game ships with. Add one by extending this enum and every row in <c>Strings.cs</c>.</summary>
    public enum Language
    {
        English = 0,
        Polish = 1,
        German = 2,
        Spanish = 3,
        French = 4,
        Portuguese = 5
    }

    /// <summary>
    /// Translations. <c>Loc.T("key")</c> returns the text in the current language, falling back to English,
    /// then to the key itself. Placeholders use string.Format syntax: {0}, {1}...
    /// Pure C#, so the tables are unit-tested for missing translations and placeholder mismatches.
    /// </summary>
    public static partial class Loc
    {
        public static readonly Language[] All =
            { Language.English, Language.Polish, Language.German, Language.Spanish, Language.French, Language.Portuguese };

        public static int LanguageCount => All.Length;

        /// <summary>Raised after the language changes.</summary>
        public static event Action Changed;

        private static Language _current = Language.English;
        private static readonly Dictionary<string, string[]> Table = new Dictionary<string, string[]>();

        static Loc()
        {
            Build();
        }

        public static Language Current
        {
            get => _current;
            set
            {
                if (_current == value) return;
                _current = value;
                Changed?.Invoke();
            }
        }

        /// <summary>Every translation key (for tests and tools).</summary>
        public static IEnumerable<string> Keys => Table.Keys;

        public static string T(string key) => Get(key, _current);

        public static string T(string key, params object[] args)
        {
            string format = Get(key, _current);
            try
            {
                return string.Format(CultureInfo.InvariantCulture, format, args);
            }
            catch (FormatException)
            {
                return format;
            }
        }

        public static string Get(string key, Language language)
        {
            if (key != null && Table.TryGetValue(key, out var row))
            {
                int i = (int)language;
                if (i >= 0 && i < row.Length && !string.IsNullOrEmpty(row[i])) return row[i];
                if (!string.IsNullOrEmpty(row[0])) return row[0];
            }
            return key ?? string.Empty;
        }

        public static bool Has(string key) => key != null && Table.ContainsKey(key);

        /// <summary>The language's own name, shown in the language picker.</summary>
        public static string NativeName(Language language)
        {
            switch (language)
            {
                case Language.Polish: return "Polski";
                case Language.German: return "Deutsch";
                case Language.Spanish: return "Español";
                case Language.French: return "Français";
                case Language.Portuguese: return "Português";
                default: return "English";
            }
        }

        /// <summary>Maps an ISO 639-1 code ("pl", "de-AT"...) to a supported language; English when unsupported.</summary>
        public static Language FromIsoCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return Language.English;
            code = code.Trim().ToLowerInvariant();
            if (code.Length > 2) code = code.Substring(0, 2);
            switch (code)
            {
                case "pl": return Language.Polish;
                case "de": return Language.German;
                case "es": return Language.Spanish;
                case "fr": return Language.French;
                case "pt": return Language.Portuguese;
                default: return Language.English;
            }
        }

        // ------------------------------------------------------------------ numbers & plurals

        /// <summary>Formats a whole number with the language's thousands separator (1,250 / 1 250 / 1.250).</summary>
        public static string Number(long n)
        {
            string sep;
            switch (_current)
            {
                case Language.Polish:
                case Language.French:
                    sep = " ";
                    break;
                case Language.German:
                case Language.Spanish:
                case Language.Portuguese:
                    sep = ".";
                    break;
                default:
                    sep = ",";
                    break;
            }
            // Spanish and Polish style guides skip the separator for 4-digit numbers.
            if ((_current == Language.Spanish || _current == Language.Polish) && Math.Abs(n) < 10000)
                return n.ToString(CultureInfo.InvariantCulture);
            var format = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            format.NumberGroupSeparator = sep;
            return n.ToString("#,0", format);
        }

        /// <summary>Formats a decimal multiplier like 1.5 as "1.5" or "1,5" depending on the language.</summary>
        public static string Decimal(float value)
        {
            string s = value.ToString("0.##", CultureInfo.InvariantCulture);
            return _current == Language.English ? s : s.Replace('.', ',');
        }

        /// <summary>"1 coin", "5 coins", and correct plural forms in every language ("2 monety", "5 monet").</summary>
        public static string Coins(long n) => Number(n) + " " + T(PluralKey("unit.coin", n));

        /// <summary>Picks key.one / key.few / key.many using the current language's plural rules.</summary>
        public static string PluralKey(string key, long n)
        {
            long abs = Math.Abs(n);
            switch (_current)
            {
                case Language.Polish:
                    if (abs == 1) return key + ".one";
                    long mod10 = abs % 10, mod100 = abs % 100;
                    if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return key + ".few";
                    return key + ".many";
                case Language.French:
                    return abs <= 1 ? key + ".one" : key + ".many";
                default:
                    return abs == 1 ? key + ".one" : key + ".many";
            }
        }

        // ------------------------------------------------------------------ game content

        public static string DifficultyName(Difficulty d) => T("difficulty." + d.ToString().ToLowerInvariant());
        public static string DifficultyDescription(Difficulty d) => T("difficulty." + d.ToString().ToLowerInvariant() + ".desc");

        /// <summary>Shop item name; character names that aren't in the table stay as written in the catalog.</summary>
        public static string ItemName(ShopItem item)
        {
            if (item == null) return string.Empty;
            string key = "item." + item.Id;
            return Has(key) ? T(key) : item.Name;
        }

        public static string RarityName(Rarity rarity) => T("rarity." + rarity);
        public static string MusicName(MusicStyle style) => T("music." + style);

        public static string PowerUpName(PowerUpType type) => T("powerup." + type + ".name");
        public static string PowerUpShortName(PowerUpType type) => T("powerup." + type + ".short");
        public static string PowerUpDescription(PowerUpType type) => T("powerup." + type + ".desc");

        public static string BuildingName(string id) => T("building." + id);
        public static string SupplyName(City.Supply s) => T("supply." + s);

        public static string MissionTitle(MissionDef mission) =>
            mission == null ? string.Empty : T("mission." + mission.Type, Number(mission.Target));

        // ------------------------------------------------------------------ table helpers

        /// <summary>Adds one row: English, Polish, German, Spanish, French, Portuguese.</summary>
        private static void Add(string key, string en, string pl, string de, string es, string fr, string pt)
        {
            Table[key] = new[] { en, pl, de, es, fr, pt };
        }

        /// <summary>Same text in every language (brand names, numbers).</summary>
        private static void Same(string key, string text) => Add(key, text, text, text, text, text, text);

        private static readonly Regex Placeholder = new Regex(@"\{(\d+)(?:[^}]*)\}", RegexOptions.Compiled);

        /// <summary>The set of placeholder indices used in a string ({0}, {1:N0} ...), for tests.</summary>
        public static SortedSet<int> PlaceholdersIn(string text)
        {
            var set = new SortedSet<int>();
            if (string.IsNullOrEmpty(text)) return set;
            foreach (Match m in Placeholder.Matches(text)) set.Add(int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture));
            return set;
        }

        /// <summary>The raw row of translations for a key (for tests and tools).</summary>
        public static string[] Row(string key) => Table.TryGetValue(key, out var row) ? row : null;
    }
}
