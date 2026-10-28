#if UNITY_PURCHASING
using System;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace JellyDrop.Monetization
{
    /// <summary>
    /// Real in-app purchases via Unity IAP (Google Play Billing / Apple StoreKit).
    /// Written against Unity IAP 4.12.x. Compiled only with the UNITY_PURCHASING define.
    /// </summary>
    public sealed class UnityIapService : IIapService, IDetailedStoreListener
    {
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private Action<bool> _onInit;
        private Action<bool, string> _pendingPurchase;
        private Action<bool, string> _pendingSeasonPass;

        public string PremiumPrice
        {
            get
            {
                var product = _controller?.products.WithID(MonetizationIds.PremiumProductId);
                return product != null && product.availableToPurchase
                    ? product.metadata.localizedPriceString
                    : MonetizationIds.PremiumFallbackPrice;
            }
        }

        public async void Initialize(Action<bool> onPremiumOwned)
        {
            _onInit = onPremiumOwned;
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IAP] Unity Services init failed: " + e.Message);
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(MonetizationIds.PremiumProductId, ProductType.NonConsumable);
            builder.AddProduct(MonetizationIds.SeasonPassProductId, ProductType.Consumable);
            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            var product = controller.products.WithID(MonetizationIds.PremiumProductId);
            _onInit?.Invoke(product != null && product.hasReceipt);
            _onInit = null;
        }

        public void OnInitializeFailed(InitializationFailureReason error) => OnInitializeFailed(error, null);

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogWarning($"[IAP] Init failed: {error} {message}");
            _onInit?.Invoke(false);
            _onInit = null;
        }

        public void BuyPremium(Action<bool, string> onResult)
        {
            if (_controller == null)
            {
                onResult?.Invoke(false, JellyDrop.Core.Loc.T("iap.unavailable"));
                return;
            }
            _pendingPurchase = onResult;
            _controller.InitiatePurchase(MonetizationIds.PremiumProductId);
        }

        public string SeasonPassPrice
        {
            get
            {
                var product = _controller?.products.WithID(MonetizationIds.SeasonPassProductId);
                return product != null && product.availableToPurchase
                    ? product.metadata.localizedPriceString
                    : MonetizationIds.SeasonPassFallbackPrice;
            }
        }

        public void BuySeasonPass(Action<bool, string> onResult)
        {
            if (_controller == null)
            {
                onResult?.Invoke(false, JellyDrop.Core.Loc.T("iap.unavailable"));
                return;
            }
            _pendingSeasonPass = onResult;
            _controller.InitiatePurchase(MonetizationIds.SeasonPassProductId);
        }

        /// <summary>Raised when a season pass purchase completes outside a BuySeasonPass call (deferred payments).</summary>
        public event Action SeasonPassPurchased;

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (args.purchasedProduct.definition.id == MonetizationIds.SeasonPassProductId)
            {
                // TODO before launch: validate the receipt on a server.
                if (_pendingSeasonPass != null) _pendingSeasonPass(true, null);
                else SeasonPassPurchased?.Invoke();
                _pendingSeasonPass = null;
                return PurchaseProcessingResult.Complete;
            }
            if (args.purchasedProduct.definition.id == MonetizationIds.PremiumProductId)
            {
                // TODO before launch: validate the receipt on a server to block fake purchases.
                _pendingPurchase?.Invoke(true, null);
                _pendingPurchase = null;
                PremiumRestored?.Invoke();
            }
            return PurchaseProcessingResult.Complete;
        }

        /// <summary>Raised when a purchase arrives outside a BuyPremium call (restores, deferred payments).</summary>
        public event Action PremiumRestored;

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failure)
        {
            if (product != null && product.definition.id == MonetizationIds.SeasonPassProductId)
            {
                _pendingSeasonPass?.Invoke(false, failure.reason == PurchaseFailureReason.UserCancelled ? null : failure.message);
                _pendingSeasonPass = null;
                return;
            }
            _pendingPurchase?.Invoke(false, failure.reason == PurchaseFailureReason.UserCancelled ? null : failure.message);
            _pendingPurchase = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            if (product != null && product.definition.id == MonetizationIds.SeasonPassProductId)
            {
                _pendingSeasonPass?.Invoke(false, reason == PurchaseFailureReason.UserCancelled ? null : reason.ToString());
                _pendingSeasonPass = null;
                return;
            }
            _pendingPurchase?.Invoke(false, reason == PurchaseFailureReason.UserCancelled ? null : reason.ToString());
            _pendingPurchase = null;
        }

        public void RestorePurchases(Action<bool> onResult)
        {
            if (_extensions == null)
            {
                onResult?.Invoke(false);
                return;
            }
#if UNITY_IOS
            _extensions.GetExtension<IAppleExtensions>().RestoreTransactions((success, error) =>
            {
                var product = _controller.products.WithID(MonetizationIds.PremiumProductId);
                onResult?.Invoke(success && product != null && product.hasReceipt);
            });
#else
            var p = _controller.products.WithID(MonetizationIds.PremiumProductId);
            onResult?.Invoke(p != null && p.hasReceipt);
#endif
        }
    }
}
#endif
