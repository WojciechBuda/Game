using System;
using System.Collections.Generic;

namespace JellyDrop.Core.Economy
{
    // All save data uses public fields and Lists so Unity's JsonUtility can serialise it.

    [Serializable]
    public sealed class PowerUpStack
    {
        public int type;
        public int count;
    }

    [Serializable]
    public sealed class ModeBest
    {
        public int mode;
        /// <summary>(int)Difficulty. Saves from version 1 had no difficulty and are migrated to Normal.</summary>
        public int difficulty = 1;
        public int bestScore;
        /// <summary>Best completion time in seconds (Sprint only); 0 = none yet.</summary>
        public float bestTime;
    }

    [Serializable]
    public sealed class MissionProgress
    {
        public string id;
        public int progress;
        public bool claimed;
    }

    [Serializable]
    public sealed class PlotData
    {
        /// <summary>Building id from CityCatalog, or empty for a free plot.</summary>
        public string building = "";
        public int level;
        /// <summary>Unix seconds of the last collection; production accrues from here.</summary>
        public long lastCollect;
        /// <summary>Unix seconds when the current construction finishes (0 = nothing being built).</summary>
        public long buildEnds;
        /// <summary>Level the building will have when the construction finishes.</summary>
        public int targetLevel;
    }

    [Serializable]
    public sealed class PlayerData
    {
        public int version = PlayerData.CurrentVersion;
        public const int CurrentVersion = 5;

        public int coins;
        public int shards;
        public bool premium;

        public List<string> owned = new List<string>();
        public string equippedSkin;
        public string equippedTheme;
        public string equippedAvatar;

        public List<PowerUpStack> powerUps = new List<PowerUpStack>();
        public List<ModeBest> bests = new List<ModeBest>();

        // Daily login streak
        public int streakDay;
        public int lastStreakClaimDay = -1;

        // Rewarded ads (capped per day)
        public int rewardedAdDay = -1;
        public int rewardedAdsWatchedToday;

        // Daily missions
        public int missionsDay = -1;
        public List<MissionProgress> missions = new List<MissionProgress>();

        // Daily challenge
        public int dailyChallengeDay = -1;
        public int dailyChallengeBest;

        // Ads pacing / stats
        public int gamesSinceInterstitial;
        public int totalGames;
        public long totalCoinsEarned;

        // Level path: stars per level (index 0 = level 1). Count = levels cleared at least once.
        public List<int> levelStars = new List<int>();

        // City
        public int wood;
        public int brick;
        public int glass;
        public List<PlotData> plots = new List<PlotData>();

        // Player level and season pass
        public int xp;
        /// <summary>Season the fields below belong to (see SeasonPass.SeasonIndex).</summary>
        public int seasonIndex;
        public int seasonPoints;
        public bool seasonPremium;
        public List<int> seasonFreeClaimed = new List<int>();
        public List<int> seasonPremiumClaimed = new List<int>();

        // Random village missions (catch the chicken, the thief...)
        public int cityMissionType = -1;
        public long cityMissionOfferEnds;
        public long cityMissionNext;
        public int cityMissionsDone;

        // Pet care (the avatar as a little pet, like Pou). Needs are 0..100.
        public float petFood, petEnergy, petFun, petClean;
        public long petUpdated;      // last time the needs were simulated (0 = never: start fresh)
        public long petLastCare;     // last time the player looked after the pet
        public bool petSleeping;
        public bool petSick;
        public int petDressDay = -1, petDressCount;
        public int petCareTotal;

        // Tower climb (Icy Tower style)
        public List<int> towerBest = new List<int>();   // best floor per endless mode
        public int towerCleared;                          // towers with a floor limit climbed so far
        public int towerScenery;

        // Lucky wheel
        public int wheelFreeDay = -1;        // day the daily free spin (home screen) was used
        public int paidSpinDay = -1, paidSpinsToday;
        public int coinBoostGames;           // games left with coins x2 (from the wheel)

        // Forest & garden goods
        public int mushrooms, goldMushrooms, berries, honey, eggs;
        public bool runExtraLife;            // spend mushrooms for a 4th life in Block Run
        public bool towerSpringStart;        // spend mushrooms for a spring start in the tower

        // Block Run bonus: tickets (one free each day, more from Boost levels) and best distance
        public int runTickets;
        public int runTicketDay = -1;
        public int runBest;

        // Avatar wardrobe (bought with coins) and what is worn
        public List<string> wardrobe = new List<string>();
        public string wearColor = "";
        public string wearHat = "";
        public string wearGlasses = "";

        /// <summary>(int)Difficulty last picked on the home screen.</summary>
        public int selectedDifficulty = 1;

        // Settings
        /// <summary>(int)Language picked in Settings, or -1 to follow the phone's language.</summary>
        public int language = -1;
        public bool musicOn = true;
        public bool sfxOn = true;
        public bool hapticsOn = true;
        public float musicVolume = 0.8f;   // 0..1
        public float sfxVolume = 1f;       // 0..1
        public int frameRateMode;          // 0 = as smooth as the screen allows, 1 = 60 fps, 2 = battery saver (30 fps)
        public int privacyVersionSeen;     // version of the privacy policy the player has seen
        public bool tutorialSeen;
    }
}
