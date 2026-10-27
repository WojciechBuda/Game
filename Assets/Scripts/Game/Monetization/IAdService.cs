using System;

namespace JellyDrop.Monetization
{
    public interface IAdService
    {
        /// <summary>Runs consent (GDPR/UMP) if needed, then initialises the SDK and preloads ads.</summary>
        void Initialize(Action onReady);

        bool IsRewardedReady { get; }

        /// <summary>
        /// Shows a rewarded ad. The callback receives true only if the user earned the reward
        /// (watched to the end). Grant coins only in that case.
        /// </summary>
        void ShowRewarded(Action<bool> onFinished);

        /// <summary>Shows an interstitial if one is loaded; always calls <paramref name="onClosed"/>.</summary>
        void ShowInterstitial(Action onClosed);

        /// <summary>True when the user must be able to reopen the consent form (EU). Show a button in Settings.</summary>
        bool PrivacyOptionsRequired { get; }

        void ShowPrivacyOptions();
    }

    public interface IIapService
    {
        /// <summary>Connects to the store. Calls back with true if the user already owns Premium (restores on Android).</summary>
        void Initialize(Action<bool> onPremiumOwned);

        /// <summary>Localised price from the store, e.g. "12,99 zł".</summary>
        string PremiumPrice { get; }

        void BuyPremium(Action<bool, string> onResult);

        /// <summary>Required by Apple. On Android purchases restore automatically at startup.</summary>
        void RestorePurchases(Action<bool> onResult);
    }
}
