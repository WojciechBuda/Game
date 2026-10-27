using System.Collections.Generic;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    public enum ShopTab { Skins, Themes, Avatars, PowerUps, Premium }

    /// <summary>Shop with tabs: block skins, themes (background + music), avatars, power-up packs and Premium.</summary>
    public sealed class ShopScreen : UIScreen
    {
        private ShopTab _tab = ShopTab.Skins;
        private RectTransform _content;
        private ScrollRect _scroll;
        private Text _coins;
        private Text _shards;
        private readonly Dictionary<ShopTab, CandyButton> _tabButtons = new Dictionary<ShopTab, CandyButton>();

        private static readonly Color[] TabColors = { ColorUtil.Pink, ColorUtil.Blue, ColorUtil.Orange, ColorUtil.Mint, ColorUtil.Gold };
        private static readonly string[] TabKeys = { "shop.tab.skins", "shop.tab.themes", "shop.tab.buddies", "shop.tab.power", "shop.tab.premium" };

        protected override void Build()
        {
            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 120);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(84, -84), new Vector2(120, 120));

            var title = UIKit.Text(Root, Loc.T("home.shop"), 90, Color.white).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(-40, -84), new Vector2(400, 120));

            _coins = UIKit.CurrencyPill(Root, SpriteFactory.Coin(), ColorUtil.Ink, out var coinPill, 260);
            coinPill.At(UIKit.TopRight, new Vector2(-150, -60), coinPill.sizeDelta);
            _shards = UIKit.CurrencyPill(Root, SpriteFactory.Shard(), ColorUtil.Ink, out var shardPill, 200);
            shardPill.At(UIKit.TopRight, new Vector2(-120, -160), shardPill.sizeDelta);

            // Tabs
            for (int i = 0; i < TabKeys.Length; i++)
            {
                var tab = (ShopTab)i;
                var b = UIKit.Button(Root, Loc.T(TabKeys[i]), TabColors[i], () => SelectTab(tab), new Vector2(200, 110), 32);
                b.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(-416 + i * 208, -270), new Vector2(200, 110));
                _tabButtons[tab] = b;
            }

            // Scroll area
            var viewport = UIKit.Panel(Root, PixelCanvas.WithAlpha(ColorUtil.Night, 0.45f), 0.8f, true, "Viewport");
            viewport.rectTransform.Fill(30, 40, 30, 350);
            viewport.gameObject.AddComponent<RectMask2D>();

            _content = UIKit.Node("Content", viewport.transform);
            _content.anchorMin = new Vector2(0, 1);
            _content.anchorMax = new Vector2(1, 1);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.offsetMin = _content.offsetMax = Vector2.zero;
            var grid = _content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(480, 600);
            grid.spacing = new Vector2(24, 24);
            grid.padding = new RectOffset(18, 18, 24, 40);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            var fitter = _content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _scroll = viewport.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = viewport.rectTransform;
            _scroll.content = _content;
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Elastic;
            _scroll.scrollSensitivity = 30f;
        }

        public void SelectTab(ShopTab tab)
        {
            _tab = tab;
            if (IsVisible) Rebuild();
        }

        protected override void OnShow() => Rebuild();

        public override void Refresh()
        {
            _coins.text = Loc.Number(App.Economy.Coins);
            _shards.text = App.Economy.Shards.ToString();
        }

        private void Rebuild()
        {
            foreach (var kv in _tabButtons)
            {
                bool active = kv.Key == _tab;
                kv.Value.transform.localScale = Vector3.one * (active ? 1.08f : 0.94f);
                kv.Value.Interactable = true;
                kv.Value.SetColor(active ? TabColors[(int)kv.Key] : Color.Lerp(TabColors[(int)kv.Key], Color.white, 0.45f));
            }

            UIKit.ClearChildren(_content);
            var grid = _content.GetComponent<GridLayoutGroup>();
            grid.cellSize = _tab == ShopTab.Premium ? new Vector2(980, 1200) : new Vector2(480, 600);
            grid.constraintCount = _tab == ShopTab.Premium ? 1 : 2;

            switch (_tab)
            {
                case ShopTab.Skins: BuildItems(ItemCategory.Skin); break;
                case ShopTab.Themes: BuildItems(ItemCategory.Theme); break;
                case ShopTab.Avatars: BuildItems(ItemCategory.Avatar); break;
                case ShopTab.PowerUps: BuildPowerUps(); break;
                case ShopTab.Premium: BuildPremium(); break;
            }
            _content.anchoredPosition = Vector2.zero;
            Refresh();
        }

        // ------------------------------------------------------------------ cosmetics

        private void BuildItems(ItemCategory category)
        {
            var e = App.Economy;
            foreach (var item in e.Catalog.InCategory(category))
            {
                var card = Card(Loc.ItemName(item), UIKit.RarityColor(item.Rarity));
                BuildPreview(card, item);

                var rarity = UIKit.Panel(card, UIKit.RarityColor(item.Rarity), 2f, false, "Rarity");
                rarity.rectTransform.At(UIKit.TopRight, new Vector2(-80, -28), new Vector2(140, 46));
                UIKit.Text(rarity.transform, Loc.RarityName(item.Rarity), 24, Color.white, TextAnchor.MiddleCenter, false).Fit(14).rectTransform.Fill(6, 0, 6, 0);

                bool owned = e.IsOwned(item.Id);
                bool equipped = owned && (e.Data.equippedSkin == item.Id || e.Data.equippedTheme == item.Id || e.Data.equippedAvatar == item.Id);

                if (equipped)
                {
                    var b = CardButton(card, Loc.T("shop.equipped"), ColorUtil.Mint, null, SpriteFactory.CheckIcon());
                    b.Interactable = false;
                }
                else if (owned)
                {
                    CardButton(card, Loc.T("shop.equip"), ColorUtil.Blue, () => Equip(item));
                }
                else if (item.PremiumOnly)
                {
                    CardButton(card, Loc.T("shop.premium_only"), ColorUtil.Gold, () => SelectTab(ShopTab.Premium), SpriteFactory.Star());
                }
                else
                {
                    var buy = CardButton(card, Loc.Number(item.CoinPrice), ColorUtil.Orange, () => Buy(item, false), SpriteFactory.Coin(),
                        item.ShardPrice > 0 ? -90 : 0, item.ShardPrice > 0 ? 250 : 420);
                    buy.Interactable = e.Coins >= item.CoinPrice;
                    buy.ClickWhenDisabled = true; // still tappable to explain what's missing
                    if (item.ShardPrice > 0)
                    {
                        var shard = CardButton(card, item.ShardPrice.ToString(), ColorUtil.Mint, () => Buy(item, true), SpriteFactory.Shard(), 135, 170);
                        shard.Interactable = e.Shards >= item.ShardPrice;
                        shard.ClickWhenDisabled = true;
                    }
                }
            }
        }

        private void BuildPreview(RectTransform card, ShopItem item)
        {
            var area = UIKit.Node("Preview", card).At(UIKit.Center, new Vector2(0, 40), new Vector2(380, 300));
            switch (item.Category)
            {
                case ItemCategory.Skin:
                    // A little T + L arrangement showing four colours of the skin.
                    int[] colors = { 2, 6, 0, 3 };
                    Vector2[] pos = { new Vector2(-75, 45), new Vector2(0, 45), new Vector2(75, 45), new Vector2(0, -30) };
                    for (int i = 0; i < 4; i++)
                    {
                        var col = ColorUtil.Hex(item.Palette[colors[i]]);
                        var img = UIKit.Image(area, SpriteFactory.Block(item.BlockStyle, i == 3 ? ColorUtil.Hex(item.Palette[2]) : col), Color.white);
                        img.rectTransform.At(UIKit.Center, pos[i], new Vector2(78, 78));
                    }
                    var single = UIKit.Image(area, SpriteFactory.Block(item.BlockStyle, ColorUtil.Hex(item.Palette[4])), Color.white);
                    single.rectTransform.At(UIKit.Center, new Vector2(-75, -105), new Vector2(78, 78));
                    var single2 = UIKit.Image(area, SpriteFactory.Block(item.BlockStyle, ColorUtil.Hex(item.Palette[1])), Color.white);
                    single2.rectTransform.At(UIKit.Center, new Vector2(75, -105), new Vector2(78, 78));
                    break;
                case ItemCategory.Theme:
                    var bg = UIKit.Panel(area, Color.white, 1.5f, false, "Swatch");
                    bg.rectTransform.Fill(10, 10, 10, 10);
                    bg.sprite = SpriteFactory.Gradient(ColorUtil.Hex(item.BackgroundTop), ColorUtil.Hex(item.BackgroundBottom));
                    bg.type = Image.Type.Simple;
                    for (int i = 0; i < 6; i++)
                    {
                        var b = UIKit.Image(bg.transform, SpriteFactory.SoftDot(), new Color(1f, 1f, 1f, 0.35f));
                        b.rectTransform.At(UIKit.Center, new Vector2(-120 + i * 50, -80 + (i * 37 % 160)), Vector2.one * (30 + i * 9));
                    }
                    var note = UIKit.Text(bg.transform, "♪ " + Loc.MusicName(item.Music), 34, Color.white).Fit(20);
                    note.rectTransform.At(UIKit.Bottom, new Vector2(0, 36), new Vector2(340, 50));
                    break;
                case ItemCategory.Avatar:
                    var av = UIKit.Image(area, SpriteFactory.Avatar(item, AvatarMood.Happy), Color.white);
                    av.rectTransform.At(UIKit.Center, Vector2.zero, new Vector2(270, 270));
                    av.gameObject.AddComponent<Bobber>().Amplitude = 6f;
                    break;
            }
        }

        private void Equip(ShopItem item)
        {
            if (!App.Economy.Equip(item.Id)) return;
            App.ApplyCosmetics();
            App.Audio.Play(Sfx.Pop);
            Rebuild();
        }

        private void Buy(ShopItem item, bool withShards)
        {
            var result = App.Economy.Buy(item.Id, withShards);
            switch (result)
            {
                case PurchaseResult.Success:
                    App.Audio.Play(Sfx.Purchase);
                    App.Economy.Equip(item.Id);
                    App.ApplyCosmetics();
                    Rebuild();
                    App.Popup.ShowReward(Loc.T("shop.unlocked"), Loc.T("shop.is_yours", Loc.ItemName(item)),
                        item.Category == ItemCategory.Avatar ? SpriteFactory.Avatar(item, AvatarMood.Excited)
                        : item.Category == ItemCategory.Skin ? SpriteFactory.Block(item.BlockStyle, ColorUtil.Hex(item.Palette[2]))
                        : SpriteFactory.Star());
                    break;
                case PurchaseResult.NotEnoughCoins:
                    NotEnoughCoins(item.CoinPrice - App.Economy.Coins);
                    break;
                case PurchaseResult.NotEnoughShards:
                    App.Audio.Play(Sfx.Error);
                    App.Popup.ShowMessage(Loc.T("shop.need_shards_title"), Loc.T("shop.need_shards_body"), SpriteFactory.Shard());
                    break;
            }
        }

        private void NotEnoughCoins(int missing)
        {
            App.Audio.Play(Sfx.Error);
            if (App.Economy.CanWatchRewardedAd)
                App.Popup.ShowConfirm(Loc.T("shop.almost"), Loc.T("shop.need_coins_ad", Loc.Coins(missing), App.Economy.Config.RewardedAdCoins), Loc.T("shop.watch"),
                    () => App.WatchAdForCoins(_ => Rebuild()));
            else
                App.Popup.ShowMessage(Loc.T("shop.almost"), Loc.T("shop.need_coins_play", Loc.Coins(missing)), SpriteFactory.Coin());
        }

        // ------------------------------------------------------------------ power-ups

        private void BuildPowerUps()
        {
            var e = App.Economy;
            foreach (var offer in e.Catalog.PowerUpOffers)
            {
                var card = Card(Loc.PowerUpName(offer.Type));
                var icon = UIKit.Image(card, SpriteFactory.PowerUpIcon(offer.Type), Color.white);
                icon.rectTransform.At(UIKit.Center, new Vector2(0, 100), new Vector2(180, 180));
                var desc = UIKit.Text(card, Loc.PowerUpDescription(offer.Type), 30, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(18);
                desc.rectTransform.At(UIKit.Center, new Vector2(0, -40), new Vector2(420, 90));
                var owned = UIKit.Text(card, Loc.T("shop.you_have", e.PowerUpCount(offer.Type)), 30, ColorUtil.Purple, TextAnchor.MiddleCenter, false);
                owned.rectTransform.At(UIKit.Center, new Vector2(0, -110), new Vector2(420, 50));
                var b = CardButton(card, $"x{offer.Quantity}  {offer.CoinPrice}", ColorUtil.Orange, () =>
                {
                    var r = App.Economy.BuyPowerUps(offer.Id);
                    if (r == PurchaseResult.Success) { App.Audio.Play(Sfx.Purchase); Rebuild(); }
                    else NotEnoughCoins(offer.CoinPrice - App.Economy.Coins);
                }, SpriteFactory.Coin());
                b.Interactable = e.Coins >= offer.CoinPrice;
                b.ClickWhenDisabled = true;
            }
        }

        // ------------------------------------------------------------------ premium

        private void BuildPremium()
        {
            var e = App.Economy;
            var card = Card(Loc.T(e.IsPremium ? "shop.you_are_premium" : "shop.go_premium"), ColorUtil.Gold);
            var king = e.Catalog.Get("avatar_king");
            if (king != null)
            {
                var av = UIKit.Image(card, SpriteFactory.Avatar(king, AvatarMood.Excited), Color.white);
                av.rectTransform.At(UIKit.Top, new Vector2(0, -270), new Vector2(300, 300));
                av.gameObject.AddComponent<Bobber>().Amplitude = 10f;
            }

            string[] perks =
            {
                Loc.T("shop.perk1"),
                Loc.T("shop.perk2"),
                Loc.T("shop.perk3"),
                Loc.T("shop.perk4"),
                Loc.T("shop.perk5")
            };
            for (int i = 0; i < perks.Length; i++)
            {
                var row = UIKit.Text(card, "•  " + perks[i], 46, ColorUtil.Ink, TextAnchor.MiddleLeft, false).Fit(26);
                row.rectTransform.At(UIKit.Center, new Vector2(40, 120 - i * 80), new Vector2(760, 70));
            }

            if (e.IsPremium)
            {
                var thanks = UIKit.Text(card, Loc.T("shop.thanks") + " ♥", 44, ColorUtil.Pink).Fit(26);
                thanks.rectTransform.At(UIKit.Bottom, new Vector2(0, 200), new Vector2(900, 80));
            }
            else
            {
                var buy = UIKit.Button(card, Loc.T("shop.buy", App.Iap.PremiumPrice), ColorUtil.Gold, App.BuyPremium, new Vector2(640, 170), 62, SpriteFactory.Star());
                buy.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 250), new Vector2(640, 170));
                var once = UIKit.Text(card, Loc.T("shop.one_time"), 32, ColorUtil.Ink, TextAnchor.MiddleCenter, false);
                once.rectTransform.At(UIKit.Bottom, new Vector2(0, 140), new Vector2(600, 50));
            }
            var restore = UIKit.Button(card, Loc.T("common.restore"), ColorUtil.Purple, App.RestorePurchases, new Vector2(440, 100), 36);
            restore.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 60), new Vector2(440, 100));
        }

        // ------------------------------------------------------------------ helpers

        private RectTransform Card(string name, Color? accent = null)
        {
            var card = UIKit.NeonCard(_content, accent ?? PixelCanvas.WithAlpha(ColorUtil.Blue, 0.7f), ColorUtil.PanelLight, 1.2f, false, "Card", 0.18f);
            var rt = card.rectTransform;
            var title = UIKit.Text(rt, name, 40, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(22);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -80), new Vector2(440, 60));
            if (_tab == ShopTab.Premium)
            {
                title.fontSize = 70;
                title.Fit(36);
                title.color = ColorUtil.Gold;
                title.rectTransform.At(UIKit.Top, new Vector2(0, -70), new Vector2(900, 100));
            }
            return rt;
        }

        private CandyButton CardButton(RectTransform card, string label, Color color, System.Action onClick,
            Sprite icon = null, float x = 0f, float width = 420f)
        {
            var b = UIKit.Button(card, label, color, onClick, new Vector2(width, 120), 42, icon);
            b.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(x, 80), new Vector2(width, 120));
            return b;
        }

        public override bool OnBack()
        {
            App.GoHome();
            return true;
        }
    }
}
