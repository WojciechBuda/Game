using System;
using System.Collections.Generic;

namespace JellyDrop.Core.Economy
{
    [Serializable]
    public sealed class EconomyConfig
    {
        public int StartingCoins = 100;
        public int ScorePerCoin = 100;
        public int PersonalBestBonus = 20;
        public int SprintCompletedBonus = 30;
        public float PremiumCoinMultiplier = 1.5f;

        public int RewardedAdCoins = 100;
        public int MaxRewardedAdsPerDay = 2;

        public int InterstitialEveryNGames = 3;

        public int DailyMissionCount = 3;
        public int[] StreakRewards = { 50, 75, 100, 150, 200, 300, 500 };
        /// <summary>Starter power-ups so new players can try them.</summary>
        public int StarterPowerUpsEach = 1;
    }

    public enum PurchaseResult
    {
        Success,
        AlreadyOwned,
        NotEnoughCoins,
        NotEnoughShards,
        RequiresPremium,
        UnknownItem,
        /// <summary>Level or season reward: earned, never sold.</summary>
        NotForSale
    }

    public sealed class GameRewards
    {
        public int ScoreCoins;
        public int CollectedCoins;
        public int PersonalBestBonus;
        public int SprintBonus;
        public int PremiumBonus;
        /// <summary>Extra coins from city buildings (parks, tower, castle).</summary>
        /// <summary>Extra coins from the city and player level bonus (see <see cref="BonusPercent"/>).</summary>
        public int CityBonus;
        public int BonusPercent;
        public bool IsPersonalBest;
        /// <summary>Extra coins from a wheel booster (coins x2).</summary>
        public int BoosterBonus;
        public int Total => ScoreCoins + CollectedCoins + PersonalBestBonus + SprintBonus + PremiumBonus + CityBonus + BoosterBonus;
        public List<MissionDef> MissionsCompleted = new List<MissionDef>();
    }

    /// <summary>
    /// All persistent progression rules: coins, shop, inventory, streaks, missions, ad caps.
    /// Pure C#, so it is unit-tested without Unity. Call <see cref="Changed"/> listeners to save.
    /// </summary>
    public sealed class EconomyService
    {
        public event Action Changed;
        /// <summary>Raised when a daily mission reward is claimed (season pass points listen to it).</summary>
        public event Action<MissionDef> MissionClaimed;

        public PlayerData Data { get; }
        public Catalog Catalog { get; }
        public EconomyConfig Config { get; }
        public LuckyWheel Wheel { get; }

        private readonly IClock _clock;

        public EconomyService(PlayerData data, Catalog catalog, EconomyConfig config, IClock clock, LuckyWheel wheel = null)
        {
            Data = data ?? new PlayerData();
            Catalog = catalog;
            Config = config;
            _clock = clock;
            Wheel = wheel ?? LuckyWheel.CreateDefault();
            EnsureDefaults();
        }

        public int Today => _clock.TodayIndex;
        public IClock Clock => _clock;

        /// <summary>Extra % on game coins, supplied by the city (parks, tower, castle). Null = none.</summary>
        public Func<int> CoinBonusPercent;

        // ------------------------------------------------------------ setup

        private void EnsureDefaults()
        {
            if (Data.version < 2)
            {
                // v1 saves had no difficulty: their records were all played on Normal.
                foreach (var b in Data.bests) b.difficulty = (int)Difficulty.Normal;
                Data.selectedDifficulty = (int)Difficulty.Normal;
                // The default look changed to Neon Arcade. Players still on the old free defaults get the new ones
                // (they keep the old items, which are now shop items they own).
                if (Data.equippedSkin == "skin_jelly") Data.equippedSkin = null;
                if (Data.equippedTheme == "theme_candy") Data.equippedTheme = null;
            }
            if (Data.version < 3)
            {
                // The default look changed again, to the soft arcade style.
                if (Data.equippedSkin == "skin_neon") Data.equippedSkin = null;
                if (Data.equippedTheme == "theme_arcade") Data.equippedTheme = null;
            }
            if (Data.version < 5)
            {
                // The default look went darker (Dusk). Daydream stays owned and can be equipped in the shop.
                if (Data.equippedTheme == "theme_daydream") Data.equippedTheme = null;
            }
            Data.version = PlayerData.CurrentVersion;

            if (Data.totalGames == 0 && Data.owned.Count == 0)
            {
                Data.coins = Config.StartingCoins;
                foreach (PowerUpType t in Enum.GetValues(typeof(PowerUpType))) AddPowerUp(t, Config.StarterPowerUpsEach, notify: false);
            }
            foreach (var item in Catalog.Items)
            {
                if (item.IsFree && !Data.owned.Contains(item.Id)) Data.owned.Add(item.Id);
                if (item.PremiumOnly && Data.premium && !Data.owned.Contains(item.Id)) Data.owned.Add(item.Id);
            }
            if (!IsOwned(Data.equippedSkin)) Data.equippedSkin = Catalog.DefaultId(ItemCategory.Skin);
            if (!IsOwned(Data.equippedTheme)) Data.equippedTheme = Catalog.DefaultId(ItemCategory.Theme);
            if (!IsOwned(Data.equippedAvatar)) Data.equippedAvatar = Catalog.DefaultId(ItemCategory.Avatar);
            RefreshMissionsIfNewDay();
        }

        private void Notify() => Changed?.Invoke();

        /// <summary>For other services that change <see cref="Data"/> (city, level progress) so the game saves.</summary>
        public void NotifyChanged() => Notify();

        // ------------------------------------------------------------ coins

        public int Coins => Data.coins;
        public int Shards => Data.shards;

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Data.coins += amount;
            Data.totalCoinsEarned += amount;
            Notify();
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount < 0 || Data.coins < amount) return false;
            Data.coins -= amount;
            Notify();
            return true;
        }

        // ------------------------------------------------------------ shop

        public bool IsOwned(string id) => id != null && Data.owned.Contains(id);

        public PurchaseResult CanBuy(string itemId, bool withShards = false)
        {
            var item = Catalog.Get(itemId);
            if (item == null) return PurchaseResult.UnknownItem;
            if (IsOwned(itemId)) return PurchaseResult.AlreadyOwned;
            if (item.PremiumOnly) return PurchaseResult.RequiresPremium;
            if (item.IsReward) return PurchaseResult.NotForSale;
            if (withShards)
            {
                if (item.ShardPrice <= 0) return PurchaseResult.UnknownItem;
                return Data.shards >= item.ShardPrice ? PurchaseResult.Success : PurchaseResult.NotEnoughShards;
            }
            return Data.coins >= item.CoinPrice ? PurchaseResult.Success : PurchaseResult.NotEnoughCoins;
        }

        public PurchaseResult Buy(string itemId, bool withShards = false)
        {
            var check = CanBuy(itemId, withShards);
            if (check != PurchaseResult.Success) return check;
            var item = Catalog.Get(itemId);
            if (withShards) Data.shards -= item.ShardPrice;
            else Data.coins -= item.CoinPrice;
            Data.owned.Add(itemId);
            Notify();
            return PurchaseResult.Success;
        }

        /// <summary>Gives an item for free (level and season rewards). Returns false if already owned.</summary>
        public bool GrantItem(string itemId)
        {
            if (Catalog.Get(itemId) == null || IsOwned(itemId)) return false;
            Data.owned.Add(itemId);
            Notify();
            return true;
        }

        public void AddShards(int amount)
        {
            if (amount <= 0) return;
            Data.shards += amount;
            Notify();
        }

        // ------------------------------------------------------------ Block Run bonus

        /// <summary>Coins per metre run, on top of the coins collected on the track.</summary>
        public const int RunMetresPerCoin = 40;

        /// <summary>
        /// Block Run is always free to play. Bonus tickets (one per day, more from Boost levels) make a run pay
        /// double coins; a run uses one automatically when there is one.
        /// </summary>
        public int RunTickets
        {
            get
            {
                if (Data.runTicketDay != Today)
                {
                    Data.runTicketDay = Today;
                    if (Data.runTickets < 1) Data.runTickets = 1;
                }
                return Data.runTickets;
            }
        }

        public void GrantRunTicket(int count = 1)
        {
            if (count <= 0) return;
            int _ = RunTickets; // make sure today's free run is counted first
            Data.runTickets += count;
            Notify();
        }

        /// <summary>Starts a run (always allowed). Returns true when a bonus ticket was used: that run pays x2.</summary>
        public bool StartRun()
        {
            if (RunTickets <= 0) return false;
            Data.runTickets--;
            Notify();
            return true;
        }

        /// <summary>Pays out a finished run and returns the coins earned.</summary>
        public int ApplyRunResult(int coinsCollected, float distance, bool doubled = false)
        {
            int coins = Math.Max(0, coinsCollected) + (int)(Math.Max(0f, distance) / RunMetresPerCoin);
            if (doubled) coins *= 2;
            if (Data.premium) coins = (int)Math.Round(coins * Config.PremiumCoinMultiplier);
            Data.coins += coins;
            Data.totalCoinsEarned += coins;
            if ((int)distance > Data.runBest) Data.runBest = (int)distance;
            Notify();
            return coins;
        }

        // ------------------------------------------------------------ wardrobe

        public bool OwnsWardrobe(string id) => id != null && Data.wardrobe.Contains(id);

        public PurchaseResult CanBuyWardrobe(string id)
        {
            var w = Wardrobe.Get(id);
            if (w == null) return PurchaseResult.UnknownItem;
            if (OwnsWardrobe(id)) return PurchaseResult.AlreadyOwned;
            return Data.coins >= w.CoinPrice ? PurchaseResult.Success : PurchaseResult.NotEnoughCoins;
        }

        /// <summary>Buys a wardrobe item with coins and puts it on.</summary>
        public PurchaseResult BuyWardrobe(string id)
        {
            var check = CanBuyWardrobe(id);
            if (check != PurchaseResult.Success) return check;
            Data.coins -= Wardrobe.Get(id).CoinPrice;
            Data.wardrobe.Add(id);
            SetWorn(Wardrobe.Get(id));
            Notify();
            return PurchaseResult.Success;
        }

        public bool IsWearing(string id)
        {
            var w = Wardrobe.Get(id);
            if (w == null) return false;
            return w.Slot == WardrobeSlot.Color ? Data.wearColor == id : w.Slot == WardrobeSlot.Hat ? Data.wearHat == id : Data.wearGlasses == id;
        }

        /// <summary>Puts an owned item on, or takes it off if it is already worn.</summary>
        public bool ToggleWear(string id)
        {
            var w = Wardrobe.Get(id);
            if (w == null || !OwnsWardrobe(id)) return false;
            if (IsWearing(id)) ClearSlot(w.Slot);
            else SetWorn(w);
            Notify();
            return true;
        }

        public void ClearSlot(WardrobeSlot slot)
        {
            if (slot == WardrobeSlot.Color) Data.wearColor = "";
            else if (slot == WardrobeSlot.Hat) Data.wearHat = "";
            else Data.wearGlasses = "";
        }

        private void SetWorn(WardrobeItem w)
        {
            if (w.Slot == WardrobeSlot.Color) Data.wearColor = w.Id;
            else if (w.Slot == WardrobeSlot.Hat) Data.wearHat = w.Id;
            else Data.wearGlasses = w.Id;
        }

        /// <summary>How the player's avatar looks right now.</summary>
        public AvatarLook Look
        {
            get
            {
                var color = Wardrobe.Get(Data.wearColor);
                var hat = Wardrobe.Get(Data.wearHat);
                var glasses = Wardrobe.Get(Data.wearGlasses);
                return new AvatarLook
                {
                    BodyColor = color?.Color,
                    Hat = hat?.Style, HatColor = hat?.Color,
                    Glasses = glasses?.Style, GlassesColor = glasses?.Color,
                };
            }
        }

        public bool Equip(string itemId)
        {
            var item = Catalog.Get(itemId);
            if (item == null || !IsOwned(itemId)) return false;
            switch (item.Category)
            {
                case ItemCategory.Skin: Data.equippedSkin = itemId; break;
                case ItemCategory.Theme: Data.equippedTheme = itemId; break;
                case ItemCategory.Avatar: Data.equippedAvatar = itemId; break;
            }
            Notify();
            return true;
        }

        public ShopItem EquippedSkin => Catalog.Get(Data.equippedSkin);
        public ShopItem EquippedTheme => Catalog.Get(Data.equippedTheme);
        public ShopItem EquippedAvatar => Catalog.Get(Data.equippedAvatar);

        // ------------------------------------------------------------ power-ups

        public int PowerUpCount(PowerUpType type)
        {
            foreach (var s in Data.powerUps) if (s.type == (int)type) return s.count;
            return 0;
        }

        public void AddPowerUp(PowerUpType type, int amount, bool notify = true)
        {
            if (amount <= 0) return;
            var stack = Data.powerUps.Find(s => s.type == (int)type);
            if (stack == null) Data.powerUps.Add(stack = new PowerUpStack { type = (int)type });
            stack.count += amount;
            if (notify) Notify();
        }

        public bool ConsumePowerUp(PowerUpType type)
        {
            var stack = Data.powerUps.Find(s => s.type == (int)type);
            if (stack == null || stack.count <= 0) return false;
            stack.count--;
            Notify();
            return true;
        }

        public PurchaseResult BuyPowerUps(string offerId)
        {
            var offer = Catalog.PowerUpOffers.Find(o => o.Id == offerId);
            if (offer == null) return PurchaseResult.UnknownItem;
            if (Data.coins < offer.CoinPrice) return PurchaseResult.NotEnoughCoins;
            Data.coins -= offer.CoinPrice;
            AddPowerUp(offer.Type, offer.Quantity, notify: false);
            Notify();
            return PurchaseResult.Success;
        }

        // ------------------------------------------------------------ premium

        public bool IsPremium => Data.premium;

        /// <summary>Call after a successful (or restored) Premium in-app purchase.</summary>
        public void GrantPremium()
        {
            Data.premium = true;
            foreach (var item in Catalog.Items)
                if (item.PremiumOnly && !Data.owned.Contains(item.Id)) Data.owned.Add(item.Id);
            Notify();
        }

        // ------------------------------------------------------------ rewarded ads

        public int RewardedAdsLeftToday
        {
            get
            {
                int watched = Data.rewardedAdDay == Today ? Data.rewardedAdsWatchedToday : 0;
                return Math.Max(0, Config.MaxRewardedAdsPerDay - watched);
            }
        }

        public bool CanWatchRewardedAd => RewardedAdsLeftToday > 0;

        /// <summary>Call only from the ad SDK's "user earned reward" callback. Returns coins granted.</summary>
        public int GrantRewardedAdReward()
        {
            if (!CanWatchRewardedAd) return 0;
            if (Data.rewardedAdDay != Today)
            {
                Data.rewardedAdDay = Today;
                Data.rewardedAdsWatchedToday = 0;
            }
            Data.rewardedAdsWatchedToday++;
            Data.coins += Config.RewardedAdCoins;
            Data.totalCoinsEarned += Config.RewardedAdCoins;
            Notify();
            return Config.RewardedAdCoins;
        }

        // ------------------------------------------------------------ interstitials

        /// <summary>Called after each game over. True when an interstitial should be shown now.</summary>
        public bool ShouldShowInterstitial()
        {
            if (Data.premium) return false;
            Data.gamesSinceInterstitial++;
            if (Data.gamesSinceInterstitial < Config.InterstitialEveryNGames) return false;
            Data.gamesSinceInterstitial = 0;
            return true;
        }

        // ------------------------------------------------------------ daily streak

        public bool CanClaimStreak => Data.lastStreakClaimDay != Today;

        /// <summary>The streak day (1-7) that would be claimed today.</summary>
        public int NextStreakDay
        {
            get
            {
                if (!CanClaimStreak) return Data.streakDay;
                bool continues = Data.lastStreakClaimDay == Today - 1;
                return continues ? (Data.streakDay % Config.StreakRewards.Length) + 1 : 1;
            }
        }

        public int StreakReward(int day) => Config.StreakRewards[Math.Max(0, Math.Min(Config.StreakRewards.Length - 1, day - 1))];

        public int ClaimStreak()
        {
            if (!CanClaimStreak) return 0;
            int day = NextStreakDay;
            int reward = StreakReward(day);
            Data.streakDay = day;
            Data.lastStreakClaimDay = Today;
            Data.coins += reward;
            Data.totalCoinsEarned += reward;
            Notify();
            return reward;
        }

        // ------------------------------------------------------------ missions

        public void RefreshMissionsIfNewDay()
        {
            if (Data.missionsDay == Today && Data.missions.Count > 0) return;
            Data.missionsDay = Today;
            Data.missions.Clear();
            foreach (var def in MissionPool.PickForDay(Today, Config.DailyMissionCount))
                Data.missions.Add(new MissionProgress { id = def.Id });
        }

        public IEnumerable<(MissionDef def, MissionProgress progress)> Missions()
        {
            RefreshMissionsIfNewDay();
            foreach (var p in Data.missions)
            {
                var def = MissionPool.Get(p.id);
                if (def != null) yield return (def, p);
            }
        }

        public bool IsMissionComplete(MissionProgress p)
        {
            var def = MissionPool.Get(p.id);
            return def != null && p.progress >= def.Target;
        }

        public int ClaimMission(string id)
        {
            var p = Data.missions.Find(m => m.id == id);
            var def = MissionPool.Get(id);
            if (p == null || def == null || p.claimed || p.progress < def.Target) return 0;
            p.claimed = true;
            MissionClaimed?.Invoke(def);
            Data.coins += def.RewardCoins;
            Data.totalCoinsEarned += def.RewardCoins;
            Notify();
            return def.RewardCoins;
        }

        public int UnclaimedMissionCount
        {
            get
            {
                int n = 0;
                foreach (var (def, p) in Missions()) if (!p.claimed && p.progress >= def.Target) n++;
                return n;
            }
        }

        private List<MissionDef> UpdateMissions(GameResult result)
        {
            RefreshMissionsIfNewDay();
            var completed = new List<MissionDef>();
            foreach (var p in Data.missions)
            {
                var def = MissionPool.Get(p.id);
                if (def == null || p.claimed) continue;
                bool wasDone = p.progress >= def.Target;
                int value = MissionPool.ValueFrom(def.Type, result);
                p.progress = def.IsSingleGameBest ? Math.Max(p.progress, value) : p.progress + value;
                p.progress = Math.Min(p.progress, def.Target);
                if (!wasDone && p.progress >= def.Target) completed.Add(def);
            }
            return completed;
        }

        // ------------------------------------------------------------ bests

        /// <summary>Personal best for a mode on a difficulty. The Daily Challenge is always Normal.</summary>
        public ModeBest BestFor(GameMode mode, Difficulty difficulty)
        {
            if (mode == GameMode.Daily) difficulty = Difficulty.Normal;
            var best = Data.bests.Find(b => b.mode == (int)mode && b.difficulty == (int)difficulty);
            if (best == null) Data.bests.Add(best = new ModeBest { mode = (int)mode, difficulty = (int)difficulty });
            return best;
        }

        public ModeBest BestFor(GameMode mode) => BestFor(mode, SelectedDifficulty);

        // ------------------------------------------------------------ difficulty

        public Difficulty SelectedDifficulty
        {
            get
            {
                int d = Data.selectedDifficulty;
                return d >= 0 && d < DifficultySettings.All.Length ? (Difficulty)d : Difficulty.Normal;
            }
        }

        public void SelectDifficulty(Difficulty difficulty)
        {
            if (Data.selectedDifficulty == (int)difficulty) return;
            Data.selectedDifficulty = (int)difficulty;
            Notify();
        }

        /// <summary>The difficulty a mode is actually played on (Daily is fixed so everyone has the same game).</summary>
        public Difficulty DifficultyFor(GameMode mode) => mode == GameMode.Daily ? Difficulty.Normal : SelectedDifficulty;

        public int DailyChallengeBestToday => Data.dailyChallengeDay == Today ? Data.dailyChallengeBest : 0;

        /// <summary>Seed shared by every player today, so the daily challenge is the same piece order for all.</summary>
        public int DailyChallengeSeed => Today * 104729 + 7;

        // ------------------------------------------------------------ end of game

        public GameRewards ApplyGameResult(GameResult result)
        {
            var rewards = new GameRewards
            {
                ScoreCoins = result.Score / Math.Max(1, Config.ScorePerCoin),
                CollectedCoins = result.CoinsCollected
            };

            bool pathLevel = result.Mode == GameMode.Level || result.Mode == GameMode.Boost;
            var best = pathLevel ? null : BestFor(result.Mode, result.Difficulty);
            if (pathLevel)
            {
                // Levels track stars instead of personal bests (see ProgressService).
            }
            else if (result.Mode == GameMode.Sprint)
            {
                if (result.Completed && (best.bestTime <= 0f || result.TimeSeconds < best.bestTime))
                {
                    best.bestTime = result.TimeSeconds;
                    rewards.IsPersonalBest = true;
                }
                if (result.Completed) rewards.SprintBonus = Config.SprintCompletedBonus;
                best.bestScore = Math.Max(best.bestScore, result.Score);
            }
            else if (result.Score > best.bestScore && result.Score > 0)
            {
                best.bestScore = result.Score;
                rewards.IsPersonalBest = true;
            }

            if (result.Mode == GameMode.Daily)
            {
                if (Data.dailyChallengeDay != Today) { Data.dailyChallengeDay = Today; Data.dailyChallengeBest = 0; }
                Data.dailyChallengeBest = Math.Max(Data.dailyChallengeBest, result.Score);
            }

            if (rewards.IsPersonalBest) rewards.PersonalBestBonus = Config.PersonalBestBonus;

            if (Data.premium)
            {
                int baseTotal = rewards.ScoreCoins + rewards.CollectedCoins + rewards.PersonalBestBonus + rewards.SprintBonus;
                rewards.PremiumBonus = (int)Math.Round(baseTotal * (Config.PremiumCoinMultiplier - 1f));
            }

            int cityPercent = CoinBonusPercent?.Invoke() ?? 0;
            rewards.BonusPercent = cityPercent;
            if (cityPercent > 0)
            {
                int baseTotal = rewards.ScoreCoins + rewards.CollectedCoins + rewards.PersonalBestBonus + rewards.SprintBonus;
                rewards.CityBonus = (int)Math.Round(baseTotal * cityPercent / 100.0);
            }

            if (Data.coinBoostGames > 0)
            {
                rewards.BoosterBonus = rewards.ScoreCoins + rewards.CollectedCoins + rewards.PersonalBestBonus + rewards.SprintBonus;
                Data.coinBoostGames--;
            }

            rewards.MissionsCompleted = UpdateMissions(result);

            Data.totalGames++;
            Data.coins += rewards.Total;
            Data.totalCoinsEarned += rewards.Total;
            Notify();
            return rewards;
        }

        public const int PaidSpinCost = 60;
        public const int MaxPaidSpinsPerDay = 5;

        /// <summary>Spins for coins still allowed today.</summary>
        public int PaidSpinsLeft
        {
            get
            {
                if (Data.paidSpinDay != Today) { Data.paidSpinDay = Today; Data.paidSpinsToday = 0; }
                return Math.Max(0, MaxPaidSpinsPerDay - Data.paidSpinsToday);
            }
        }

        public bool DailyFreeSpinReady => Data.wheelFreeDay != Today;

        /// <summary>Uses today's free spin from the home screen. False if already used.</summary>
        public bool UseDailyFreeSpin()
        {
            if (!DailyFreeSpinReady) return false;
            Data.wheelFreeDay = Today;
            Notify();
            return true;
        }

        /// <summary>Pays for one spin with coins. False when out of coins or out of spins for today.</summary>
        public bool TryBuySpin()
        {
            if (PaidSpinsLeft <= 0 || Data.coins < PaidSpinCost) return false;
            Data.coins -= PaidSpinCost;
            Data.paidSpinsToday++;
            Notify();
            return true;
        }

        public int CoinBoostGames => Data.coinBoostGames;

        public WheelResult SpinWheel(IRandom rng)
        {
            var result = Wheel.Spin(rng);
            int amount = result.Segment.Amount;
            switch (result.Segment.Kind)
            {
                case WheelRewardKind.Coins:
                case WheelRewardKind.Jackpot:
                    Data.coins += amount;
                    Data.totalCoinsEarned += amount;
                    break;
                case WheelRewardKind.PowerUp:
                    AddPowerUp(result.PowerUp, amount, notify: false);
                    break;
                case WheelRewardKind.Shard:
                    Data.shards += amount;
                    break;
                case WheelRewardKind.Supplies:
                    Data.wood += amount * 2;
                    Data.brick += amount;
                    Data.glass += Math.Max(1, amount / 2);
                    break;
                case WheelRewardKind.RunTicket:
                    GrantRunTicket(amount);
                    break;
                case WheelRewardKind.CoinBooster:
                    Data.coinBoostGames += amount;
                    break;
                // PetEnergy: paid by the app (see App.ApplyWheelExtras).
            }
            Notify();
            return result;
        }

        // ------------------------------------------------------------ settings

        /// <summary>The language picked in Settings, or null when following the device language.</summary>
        public Language? ChosenLanguage
        {
            get
            {
                int l = Data.language;
                return l >= 0 && l < Loc.LanguageCount ? (Language?)l : null;
            }
        }

        /// <summary>Pass null to follow the device language again.</summary>
        public void SetLanguage(Language? language)
        {
            Data.language = language.HasValue ? (int)language.Value : -1;
            Notify();
        }

        public void SetMusic(bool on) { Data.musicOn = on; Notify(); }
        public void SetSfx(bool on) { Data.sfxOn = on; Notify(); }
        public void SetHaptics(bool on) { Data.hapticsOn = on; Notify(); }
        public void MarkTutorialSeen() { Data.tutorialSeen = true; Notify(); }
    }
}
