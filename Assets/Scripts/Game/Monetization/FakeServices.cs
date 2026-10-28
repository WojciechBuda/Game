using System;
using UnityEngine;

namespace JellyDrop.Monetization
{
    /// <summary>
    /// Stand-in ads used until the Google Mobile Ads SDK is installed (and always in the editor).
    /// Shows a full-screen "test ad" with a countdown so the whole reward flow can be played.
    /// </summary>
    public sealed class FakeAdService : IAdService
    {
        /// <summary>UI hook: (isRewarded, onClosed(watchedToEnd)).</summary>
        private readonly Action<bool, Action<bool>> _showOverlay;

        public FakeAdService(Action<bool, Action<bool>> showOverlay)
        {
            _showOverlay = showOverlay;
        }

        public bool IsRewardedReady => true;
        public bool PrivacyOptionsRequired => false;

        public void Initialize(Action onReady)
        {
            Debug.Log("[Ads] Using FakeAdService. Add the ADMOB scripting define after installing Google Mobile Ads.");
            onReady?.Invoke();
        }

        public void ShowRewarded(Action<bool> onFinished) =>
            _showOverlay(true, completed => onFinished?.Invoke(completed));

        public void ShowInterstitial(Action onClosed) =>
            _showOverlay(false, _ => onClosed?.Invoke());

        public void ShowPrivacyOptions() { }
    }

    /// <summary>Stand-in store: purchases succeed instantly. Used in the editor and until Unity IAP is installed.</summary>
    public sealed class FakeIapService : IIapService
    {
        private const string FakeOwnedKey = "jellydrop.fakeiap.premium";

        public string PremiumPrice => MonetizationIds.PremiumFallbackPrice;

        public void Initialize(Action<bool> onPremiumOwned)
        {
            Debug.Log("[IAP] Using FakeIapService. Add the UNITY_PURCHASING scripting define after installing Unity IAP.");
            onPremiumOwned?.Invoke(PlayerPrefs.GetInt(FakeOwnedKey, 0) == 1);
        }

        public void BuyPremium(Action<bool, string> onResult)
        {
            PlayerPrefs.SetInt(FakeOwnedKey, 1);
            PlayerPrefs.Save();
            onResult?.Invoke(true, null);
        }

        public string SeasonPassPrice => MonetizationIds.SeasonPassFallbackPrice;

        public void BuySeasonPass(Action<bool, string> onResult) => onResult?.Invoke(true, null);

        public void RestorePurchases(Action<bool> onResult) =>
            onResult?.Invoke(PlayerPrefs.GetInt(FakeOwnedKey, 0) == 1);
    }
}
