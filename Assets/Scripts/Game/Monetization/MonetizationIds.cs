namespace JellyDrop.Monetization
{
    /// <summary>
    /// Store and ad identifiers. The ad unit IDs below are Google's public TEST units:
    /// they always serve test ads. Replace them with your own AdMob IDs before release,
    /// and never click your own live ads while testing.
    /// </summary>
    public static class MonetizationIds
    {
        /// <summary>Non-consumable "Remove ads + Premium pack". Create the same ID in Play Console and App Store Connect.</summary>
        public const string PremiumProductId = "jellydrop_premium";

        /// <summary>Shown when the store price cannot be loaded (editor, offline).</summary>
        public const string PremiumFallbackPrice = "12,99 zł";

        /// <summary>Consumable season pass, bought again each season. Create it as a consumable product in both stores.</summary>
        public const string SeasonPassProductId = "jellydrop_season_pass";
        public const string SeasonPassFallbackPrice = "19,99 zł";

#if UNITY_IOS
        public const string RewardedAdUnit = "ca-app-pub-3940256099942544/1712485313";
        public const string InterstitialAdUnit = "ca-app-pub-3940256099942544/4411468910";
#else
        public const string RewardedAdUnit = "ca-app-pub-3940256099942544/5224354917";
        public const string InterstitialAdUnit = "ca-app-pub-3940256099942544/1033173712";
#endif

        /// <summary>Replace with your hosted privacy policy (required by both stores).</summary>
        public const string PrivacyPolicyUrl = "https://example.com/jellydrop/privacy";
    }
}
