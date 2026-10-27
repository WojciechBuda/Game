#if ADMOB
using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace JellyDrop.Monetization
{
    /// <summary>
    /// Real ads via the Google Mobile Ads Unity plugin (AdMob), including the UMP consent form
    /// required in the EU. Compiled only when the ADMOB scripting define is set
    /// (Project Settings > Player > Other Settings > Scripting Define Symbols).
    /// </summary>
    public sealed class AdMobAdService : IAdService
    {
        private RewardedAd _rewarded;
        private InterstitialAd _interstitial;
        private bool _initialized;

        public bool IsRewardedReady => _rewarded != null && _rewarded.CanShowAd();

        public bool PrivacyOptionsRequired =>
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

        public void Initialize(Action onReady)
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;

            var request = new ConsentRequestParameters();
            ConsentInformation.Update(request, updateError =>
            {
                if (updateError != null) Debug.LogWarning("[Ads] Consent update failed: " + updateError.Message);

                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null) Debug.LogWarning("[Ads] Consent form error: " + formError.Message);
                    if (ConsentInformation.CanRequestAds()) StartSdk(onReady);
                    else onReady?.Invoke();
                });
            });

            // If consent was given in a previous session we can start right away.
            if (ConsentInformation.CanRequestAds()) StartSdk(onReady);
        }

        private void StartSdk(Action onReady)
        {
            if (_initialized) return;
            _initialized = true;
            MobileAds.Initialize(status =>
            {
                LoadRewarded();
                LoadInterstitial();
                onReady?.Invoke();
            });
        }

        private void LoadRewarded()
        {
            _rewarded?.Destroy();
            _rewarded = null;
            RewardedAd.Load(MonetizationIds.RewardedAdUnit, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning("[Ads] Rewarded failed to load: " + error);
                    return;
                }
                _rewarded = ad;
            });
        }

        private void LoadInterstitial()
        {
            _interstitial?.Destroy();
            _interstitial = null;
            InterstitialAd.Load(MonetizationIds.InterstitialAdUnit, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning("[Ads] Interstitial failed to load: " + error);
                    return;
                }
                _interstitial = ad;
            });
        }

        public void ShowRewarded(Action<bool> onFinished)
        {
            if (!IsRewardedReady)
            {
                onFinished?.Invoke(false);
                LoadRewarded();
                return;
            }

            bool earned = false;
            var ad = _rewarded;
            ad.OnAdFullScreenContentClosed += () =>
            {
                onFinished?.Invoke(earned);
                LoadRewarded();
            };
            ad.OnAdFullScreenContentFailed += err =>
            {
                onFinished?.Invoke(false);
                LoadRewarded();
            };
            ad.Show(reward => earned = true);
        }

        public void ShowInterstitial(Action onClosed)
        {
            if (_interstitial == null || !_interstitial.CanShowAd())
            {
                onClosed?.Invoke();
                LoadInterstitial();
                return;
            }
            var ad = _interstitial;
            ad.OnAdFullScreenContentClosed += () =>
            {
                onClosed?.Invoke();
                LoadInterstitial();
            };
            ad.OnAdFullScreenContentFailed += err =>
            {
                onClosed?.Invoke();
                LoadInterstitial();
            };
            ad.Show();
        }

        public void ShowPrivacyOptions()
        {
            ConsentForm.ShowPrivacyOptionsForm(err =>
            {
                if (err != null) Debug.LogWarning("[Ads] Privacy options error: " + err.Message);
            });
        }
    }
}
#endif
