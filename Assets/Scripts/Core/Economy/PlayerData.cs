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
    public sealed class PlayerData
    {
        public int version = PlayerData.CurrentVersion;
        public const int CurrentVersion = 2;

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

        /// <summary>(int)Difficulty last picked on the home screen.</summary>
        public int selectedDifficulty = 1;

        // Settings
        /// <summary>(int)Language picked in Settings, or -1 to follow the phone's language.</summary>
        public int language = -1;
        public bool musicOn = true;
        public bool sfxOn = true;
        public bool hapticsOn = true;
        public bool tutorialSeen;
    }
}
