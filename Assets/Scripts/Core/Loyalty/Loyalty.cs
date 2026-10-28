using System;
using System.Collections.Generic;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Levels;

namespace JellyDrop.Core.Loyalty
{
    /// <summary>A bundle of things a player can receive. Zero fields are ignored.</summary>
    public sealed class Reward
    {
        public int Coins, Shards, Wood, Brick, Glass;
        public PowerUpType PowerUp;
        public int PowerUps;
        /// <summary>A shop item (skin, theme or avatar), or null.</summary>
        public string ItemId;

        public bool IsEmpty => Coins == 0 && Shards == 0 && Wood == 0 && Brick == 0 && Glass == 0 && PowerUps == 0 && ItemId == null;

        public void Add(Reward r)
        {
            Coins += r.Coins; Shards += r.Shards; Wood += r.Wood; Brick += r.Brick; Glass += r.Glass;
            if (r.PowerUps > 0) { PowerUp = r.PowerUp; PowerUps += r.PowerUps; }
            if (r.ItemId != null) ItemId = r.ItemId;
        }
    }

    /// <summary>Player level: grows with everything you do, never resets, and gives a small permanent coin bonus.</summary>
    public static class PlayerLevels
    {
        public const int MaxLevel = 100;

        /// <summary>XP needed to go from <paramref name="level"/> to the next one.</summary>
        public static int XpForNext(int level) => 100 + 25 * (Math.Max(1, level) - 1);

        /// <summary>Total XP needed to reach a level (level 1 = 0).</summary>
        public static int TotalXpFor(int level)
        {
            int total = 0;
            for (int l = 1; l < Math.Min(level, MaxLevel); l++) total += XpForNext(l);
            return total;
        }

        public static int LevelFor(int xp)
        {
            int level = 1;
            while (level < MaxLevel && xp >= TotalXpFor(level + 1)) level++;
            return level;
        }

        /// <summary>+1% coins from games for every 5 levels, up to +20%.</summary>
        public static int CoinBonusPercent(int level) => Math.Min(20, level / 5);

        /// <summary>What reaching this level gives.</summary>
        public static Reward RewardFor(int level, Catalog catalog)
        {
            var r = new Reward { Coins = 50 + 10 * Math.Min(level, 50) };
            if (level % 5 == 0) r.Shards = 3;
            if (level % 10 == 0) { r.Wood = 3; r.Brick = 3; }
            foreach (var item in catalog.Items)
                if (item.UnlockLevel == level) r.ItemId = item.Id;
            return r;
        }
    }

    /// <summary>
    /// The season pass: a new 28-day season with 30 tiers. Points from games, levels and missions fill the
    /// tiers. Every tier has a free reward and a pass reward (bought once per season). Each season gives
    /// exclusive items from one of four rotating sets.
    /// </summary>
    public static class SeasonPass
    {
        public const int Tiers = 30;
        public const int PointsPerTier = 150;
        public const long SeasonSeconds = 28L * 24 * 3600;
        /// <summary>Season 1 starts on Monday 5 January 2026, 00:00 UTC.</summary>
        public const long Epoch = 1767571200L;

        public static int SeasonIndex(long unixSeconds) => (int)(Math.Max(0, unixSeconds - Epoch) / SeasonSeconds) + 1;

        public static long SecondsLeft(long unixSeconds)
        {
            long into = Math.Max(0, unixSeconds - Epoch) % SeasonSeconds;
            return SeasonSeconds - into;
        }

        /// <summary>0 spring, 1 summer, 2 autumn, 3 winter.</summary>
        public static int SetFor(int season) => (season - 1) % 4;

        private static readonly string[][] Sets =
        {
            new[] { "theme_bloom", "avatar_bunny", "skin_blossom" },
            new[] { "theme_splash", "avatar_surfer", "skin_tropical" },
            new[] { "theme_harvest", "avatar_pumpkin", "skin_maple" },
            new[] { "theme_frost", "avatar_snowy", "skin_ice" },
        };

        /// <summary>The season's exclusive theme (free track), avatar and skin (pass track).</summary>
        public static string[] ItemsFor(int season) => Sets[SetFor(season)];

        private static readonly PowerUpType[] PowerUpCycle = { PowerUpType.Bomb, PowerUpType.LineZap, PowerUpType.SlowMo, PowerUpType.Swap };

        public static Reward FreeReward(int season, int tier)
        {
            if (tier == 25) return new Reward { ItemId = ItemsFor(season)[0] };
            if (tier == Tiers) return new Reward { Coins = 1000 };
            if (tier == 10) return new Reward { Wood = 5, Brick = 5 };
            if (tier == 20) return new Reward { Brick = 5, Glass = 3 };
            if (tier % 5 == 0) return new Reward { Shards = 3 };
            if (tier % 3 == 0) return new Reward { PowerUp = PowerUpCycle[(tier / 3) % 4], PowerUps = 2 };
            return new Reward { Coins = 50 + 5 * tier };
        }

        public static Reward PassReward(int season, int tier)
        {
            if (tier == 15) return new Reward { ItemId = ItemsFor(season)[1] };
            if (tier == Tiers) return new Reward { ItemId = ItemsFor(season)[2], Coins = 2000 };
            if (tier == 5 || tier == 20) return new Reward { Shards = 10 };
            if (tier % 4 == 0) return new Reward { Wood = 4, Brick = 4, Glass = 2 };
            if (tier % 3 == 0) return new Reward { PowerUp = PowerUpCycle[(tier / 3 + 2) % 4], PowerUps = 3 };
            return new Reward { Coins = 100 + 10 * tier };
        }
    }

    /// <summary>What one game, level or mission earned for the player level and season.</summary>
    public sealed class LoyaltyGain
    {
        public int Points;
        public int LevelBefore, LevelAfter;
        public int TierBefore, TierAfter;
        /// <summary>Everything paid out for the levels gained (granted immediately).</summary>
        public Reward LevelRewards = new Reward();
        public bool LeveledUp => LevelAfter > LevelBefore;
    }

    /// <summary>Player level + season pass. Pure C#; uses the economy clock so it is testable.</summary>
    public sealed class LoyaltyService
    {
        private readonly EconomyService _economy;
        private PlayerData Data => _economy.Data;
        private long Now => _economy.Clock.UnixSeconds;

        /// <summary>Raised when a new season started; carries the rewards auto-claimed from the old one.</summary>
        public event Action<Reward> SeasonRolledOver;

        public LoyaltyService(EconomyService economy)
        {
            _economy = economy;
            _economy.MissionClaimed += _ => AddPoints(PointsPerMission);
            // The season is checked lazily (and by the app after it subscribes to SeasonRolledOver).
        }

        public const int PointsPerMission = 10;

        // ------------------------------------------------------------ player level

        public int Xp => Data.xp;
        public int Level => PlayerLevels.LevelFor(Data.xp);
        public int CoinBonusPercent => PlayerLevels.CoinBonusPercent(Level);

        /// <summary>0..1 progress to the next level.</summary>
        public float LevelProgress
        {
            get
            {
                int lvl = Level;
                if (lvl >= PlayerLevels.MaxLevel) return 1f;
                int start = PlayerLevels.TotalXpFor(lvl);
                return (Data.xp - start) / (float)PlayerLevels.XpForNext(lvl);
            }
        }

        // ------------------------------------------------------------ season

        public int Season { get { CheckSeason(); return Data.seasonIndex; } }
        public long SecondsLeft => SeasonPass.SecondsLeft(Now);
        public int Points { get { CheckSeason(); return Data.seasonPoints; } }
        /// <summary>Tiers reached (0..30).</summary>
        public int Tier => Math.Min(SeasonPass.Tiers, Points / SeasonPass.PointsPerTier);
        public float TierProgress => Tier >= SeasonPass.Tiers ? 1f : (Points % SeasonPass.PointsPerTier) / (float)SeasonPass.PointsPerTier;
        public bool HasPass { get { CheckSeason(); return Data.seasonPremium; } }

        public bool IsClaimed(int tier, bool pass) => (pass ? Data.seasonPremiumClaimed : Data.seasonFreeClaimed).Contains(tier);

        public bool CanClaim(int tier, bool pass) =>
            tier >= 1 && tier <= Tier && !IsClaimed(tier, pass) && (!pass || HasPass);

        public int ClaimableCount
        {
            get
            {
                int n = 0;
                for (int t = 1; t <= Tier; t++)
                {
                    if (CanClaim(t, false)) n++;
                    if (CanClaim(t, true)) n++;
                }
                return n;
            }
        }

        public Reward Claim(int tier, bool pass)
        {
            if (!CanClaim(tier, pass)) return null;
            var r = pass ? SeasonPass.PassReward(Data.seasonIndex, tier) : SeasonPass.FreeReward(Data.seasonIndex, tier);
            (pass ? Data.seasonPremiumClaimed : Data.seasonFreeClaimed).Add(tier);
            Grant(r);
            _economy.NotifyChanged();
            return r;
        }

        public Reward ClaimAll()
        {
            var total = new Reward();
            for (int t = 1; t <= Tier; t++)
            {
                if (CanClaim(t, false)) total.Add(Claim(t, false));
                if (CanClaim(t, true)) total.Add(Claim(t, true));
            }
            return total;
        }

        /// <summary>Call after the season pass purchase succeeded.</summary>
        public void GrantPass()
        {
            CheckSeason();
            Data.seasonPremium = true;
            _economy.NotifyChanged();
        }

        // ------------------------------------------------------------ earning

        /// <summary>Points for a finished game: playing always counts, winning levels counts more.</summary>
        public static int PointsFor(GameResult result, LevelOutcome outcome)
        {
            // Quitting right away earns nothing (stops start-and-quit farming).
            if (result.Stats != null && result.Stats.PiecesPlaced < 5 && !(outcome != null && outcome.Won)) return 0;
            int p = 10 + Math.Min(40, result.Lines * 2);
            if (outcome != null && outcome.Won) p += outcome.Level.IsBoost ? 60 : 20 + 10 * outcome.Stars;
            if (result.Mode == GameMode.Daily) p += 15;
            return p;
        }

        public LoyaltyGain AwardGame(GameResult result, LevelOutcome outcome) => AddPoints(PointsFor(result, outcome));

        /// <summary>Adds the same amount to XP and to season points, and pays level-up rewards.</summary>
        public LoyaltyGain AddPoints(int points)
        {
            CheckSeason();
            var gain = new LoyaltyGain { Points = Math.Max(0, points), LevelBefore = Level, TierBefore = Tier };
            if (gain.Points > 0)
            {
                Data.xp += gain.Points;
                Data.seasonPoints = Math.Min(SeasonPass.Tiers * SeasonPass.PointsPerTier, Data.seasonPoints + gain.Points);
            }
            gain.LevelAfter = Level;
            gain.TierAfter = Tier;
            for (int l = gain.LevelBefore + 1; l <= gain.LevelAfter; l++)
            {
                var r = PlayerLevels.RewardFor(l, _economy.Catalog);
                Grant(r);
                gain.LevelRewards.Add(r);
            }
            if (gain.Points > 0) _economy.NotifyChanged();
            return gain;
        }

        // ------------------------------------------------------------ internals

        /// <summary>Starts a new season when the calendar moved on. Unclaimed rewards of the old one are given automatically.</summary>
        public void CheckSeason()
        {
            if (_checking) return;
            int current = SeasonPass.SeasonIndex(Now);
            if (Data.seasonIndex == current) return;
            _checking = true;
            try { RollOver(current); }
            finally { _checking = false; }
        }

        private bool _checking;

        private void RollOver(int current)
        {
            Reward carried = null;
            if (Data.seasonIndex > 0) carried = ClaimAll();
            Data.seasonIndex = current;
            Data.seasonPoints = 0;
            Data.seasonPremium = false;
            Data.seasonFreeClaimed.Clear();
            Data.seasonPremiumClaimed.Clear();
            _economy.NotifyChanged();
            if (carried != null && !carried.IsEmpty) SeasonRolledOver?.Invoke(carried);
        }

        private void Grant(Reward r)
        {
            if (r == null) return;
            if (r.Coins > 0) { Data.coins += r.Coins; Data.totalCoinsEarned += r.Coins; }
            Data.shards += r.Shards;
            Data.wood += r.Wood;
            Data.brick += r.Brick;
            Data.glass += r.Glass;
            if (r.PowerUps > 0) _economy.AddPowerUp(r.PowerUp, r.PowerUps, notify: false);
            if (r.ItemId != null && !Data.owned.Contains(r.ItemId) && _economy.Catalog.Get(r.ItemId) != null) Data.owned.Add(r.ItemId);
        }
    }
}
