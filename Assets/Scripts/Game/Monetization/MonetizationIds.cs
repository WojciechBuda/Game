namespace JellyDrop.Monetization
{
    /// <summary>
    /// Store and ad identifiers. Android uses Stackville's real AdMob units; iOS still has Google's public test
    /// units. Never click your own live ads: add your phone as a test device in AdMob (Settings > Test devices).
    /// </summary>
    public static class MonetizationIds
    {
        /// <summary>Non-consumable "Remove ads + Premium pack". Create the same ID in Play Console and App Store Connect.</summary>
        public const string PremiumProductId = "stackville_premium";

        /// <summary>Shown when the store price cannot be loaded (editor, offline).</summary>
        public const string PremiumFallbackPrice = "12,99 zł";

        /// <summary>Consumable season pass, bought again each season. Create it as a consumable product in both stores.</summary>
        public const string SeasonPassProductId = "stackville_season_pass";
        public const string SeasonPassFallbackPrice = "19,99 zł";

#if UNITY_IOS
        public const string RewardedAdUnit = "ca-app-pub-3940256099942544/1712485313";
        public const string InterstitialAdUnit = "ca-app-pub-3940256099942544/4411468910";
#else
        // Stackville's own AdMob units (Android).
        public const string RewardedAdUnit = "ca-app-pub-1930401474155411/4264423801";
        public const string InterstitialAdUnit = "ca-app-pub-1930401474155411/1515183565";
#endif

        /// <summary>AdMob app id (Android). Goes into Assets > Google Mobile Ads > Settings (Stackville > Release does it for you).</summary>
        public const string AndroidAdMobAppId = "ca-app-pub-1930401474155411~6208060865";

        /// <summary>
        /// AdMob app id (iOS). Until you add an iOS app in AdMob this is Google's sample id, which only
        /// serves test ads (fine for local test installs). Put the real one here before the App Store.
        /// </summary>
        public const string IosAdMobAppId = "ca-app-pub-3940256099942544~1458002511";

        /// <summary>
        /// The hosted privacy policy (required by both stores), on the neutral Stackville website: the files in Site/
        /// dropped onto Netlify as the site "stackville" (see Store/README.md, step 9).
        /// Change it (and SITE in Tools/PrivacyPolicy/policy.py) if you host it elsewhere.
        /// </summary>
        public const string PrivacyPolicyUrl = "https://stackville.netlify.app/privacy-policy.html";
    }
}
