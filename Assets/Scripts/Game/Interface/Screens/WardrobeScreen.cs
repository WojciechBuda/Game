using System.Collections.Generic;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>
    /// Avatar customisation: buy colours, hats and glasses with coins and put them on.
    /// The big preview always shows the avatar as it looks now.
    /// </summary>
    public sealed class WardrobeScreen : UIScreen
    {
        private static readonly Color[] TabColors = { ColorUtil.Pink, ColorUtil.Orange, ColorUtil.Blue };
        private static readonly string[] TabKeys = { "wardrobe.colors", "wardrobe.hats", "wardrobe.glasses" };

        private Text _coins;
        private Image _preview;
        private RectTransform _content;
        private ScrollRect _scroll;
        private WardrobeSlot _tab = WardrobeSlot.Hat;
        private readonly Dictionary<WardrobeSlot, CandyButton> _tabs = new Dictionary<WardrobeSlot, CandyButton>();

        protected override void Build()
        {
            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, () => OnBack(), 120);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(84, -84), new Vector2(120, 120));
            var title = UIKit.Text(Root, Loc.T("wardrobe.title"), 90, ColorUtil.Pink).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(-30, -84), new Vector2(460, 120));
            _coins = UIKit.CurrencyPill(Root, SpriteFactory.Coin(), ColorUtil.Ink, out var pill, 260);
            pill.At(UIKit.TopRight, new Vector2(-150, -84), pill.sizeDelta);

            // Spotlight behind the preview
            var glow = UIKit.Image(Root, SpriteFactory.SoftDot(), PixelCanvas.WithAlpha(ColorUtil.Pink, 0.35f), false, "Spot");
            glow.rectTransform.At(UIKit.Top, new Vector2(0, -400), new Vector2(560, 560));
            _preview = UIKit.Image(Root, null, Color.white, false, "Preview");
            _preview.rectTransform.At(UIKit.Top, new Vector2(0, -400), new Vector2(400, 400));
            var bob = _preview.gameObject.AddComponent<Bobber>();
            bob.Amplitude = 10f;
            bob.Speed = 2f;

            for (int i = 0; i < 3; i++)
            {
                var slot = (WardrobeSlot)i;
                var b = UIKit.Button(Root, Loc.T(TabKeys[i]), TabColors[i], () => SelectTab(slot), new Vector2(300, 110), 38);
                b.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(-320 + i * 320, -680), new Vector2(300, 110));
                _tabs[slot] = b;
            }

            var viewport = UIKit.Panel(Root, ColorUtil.Glass, 0.8f, true, "Viewport");
            viewport.rectTransform.Fill(30, 40, 30, 760);
            viewport.gameObject.AddComponent<RectMask2D>();
            _content = UIKit.Node("Content", viewport.transform);
            _content.anchorMin = new Vector2(0, 1);
            _content.anchorMax = new Vector2(1, 1);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.offsetMin = _content.offsetMax = Vector2.zero;
            var grid = _content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(320, 400);
            grid.spacing = new Vector2(18, 18);
            grid.padding = new RectOffset(14, 14, 18, 30);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            _content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _scroll = viewport.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = viewport.rectTransform;
            _scroll.content = _content;
            _scroll.horizontal = false;
        }

        protected override void OnShow() => Rebuild();

        public override void Refresh()
        {
            _coins.text = Loc.Number(App.Economy.Coins);
            _preview.sprite = SpriteFactory.Avatar(App.Economy.EquippedAvatar, AvatarMood.Excited);
        }

        private void SelectTab(WardrobeSlot slot)
        {
            _tab = slot;
            App.Audio.Play(Sfx.Pop);
            Rebuild();
        }

        private void Rebuild()
        {
            foreach (var kv in _tabs)
            {
                bool active = kv.Key == _tab;
                kv.Value.SetColor(active ? TabColors[(int)kv.Key] : Color.Lerp(TabColors[(int)kv.Key], ColorUtil.Muted, 0.55f));
                kv.Value.transform.localScale = Vector3.one * (active ? 1.06f : 0.94f);
            }
            UIKit.ClearChildren(_content);
            if (_tab == WardrobeSlot.Color) OriginalColorCard();
            foreach (var item in Wardrobe.InSlot(_tab)) ItemCard(item);
            _content.anchoredPosition = Vector2.zero;
            Refresh();
        }

        private RectTransform Card(string name, bool worn)
        {
            var card = UIKit.NeonCard(_content, worn ? ColorUtil.Mint : PixelCanvas.WithAlpha(ColorUtil.Blue, 0.6f), ColorUtil.PanelLight, 1.2f, false, "Card", 0.18f);
            var t = UIKit.Text(card.rectTransform, name, 32, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(18);
            t.rectTransform.At(UIKit.Top, new Vector2(0, -48), new Vector2(290, 50));
            return card.rectTransform;
        }

        private void OriginalColorCard()
        {
            var e = App.Economy;
            bool worn = string.IsNullOrEmpty(e.Data.wearColor);
            var card = Card(Loc.T("wardrobe.original"), worn);
            var icon = UIKit.Image(card, SpriteFactory.Avatar(e.EquippedAvatar, AvatarMood.Happy, AvatarLook.None), Color.white, false, "Icon");
            icon.rectTransform.At(UIKit.Center, new Vector2(0, 20), new Vector2(190, 190));
            var b = UIKit.Button(card, worn ? Loc.T("wardrobe.worn") : Loc.T("wardrobe.wear"), worn ? ColorUtil.Mint : ColorUtil.Blue, () =>
            {
                e.ClearSlot(WardrobeSlot.Color);
                e.NotifyChanged();
                LookChanged();
            }, new Vector2(280, 100), 34);
            b.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 66), new Vector2(280, 100));
            b.Interactable = !worn;
        }

        private void ItemCard(WardrobeItem item)
        {
            var e = App.Economy;
            bool owned = e.OwnsWardrobe(item.Id);
            bool worn = e.IsWearing(item.Id);
            var card = Card(Loc.T("wardrobe." + item.Id), worn);

            // Preview: the player's avatar shape wearing just this item (colours show as a swatch avatar).
            var look = new AvatarLook();
            if (item.Slot == WardrobeSlot.Color) look.BodyColor = item.Color;
            else if (item.Slot == WardrobeSlot.Hat) { look.Hat = item.Style; look.HatColor = item.Color; }
            else { look.Glasses = item.Style; look.GlassesColor = item.Color; }
            var icon = UIKit.Image(card, SpriteFactory.Avatar(e.EquippedAvatar, AvatarMood.Happy, look), Color.white, false, "Icon");
            icon.rectTransform.At(UIKit.Center, new Vector2(0, 20), new Vector2(190, 190));

            CandyButton b;
            if (!owned)
            {
                b = UIKit.Button(card, Loc.Number(item.CoinPrice), ColorUtil.Orange, () => Buy(item), new Vector2(280, 100), 38, SpriteFactory.Coin());
                b.Interactable = e.Coins >= item.CoinPrice;
                b.ClickWhenDisabled = true;
            }
            else
            {
                b = UIKit.Button(card, worn ? Loc.T("wardrobe.worn") : Loc.T("wardrobe.wear"), worn ? ColorUtil.Mint : ColorUtil.Blue, () =>
                {
                    e.ToggleWear(item.Id);
                    if (e.IsWearing(item.Id)) App.Pet.OnDressed();
                    LookChanged();
                }, new Vector2(280, 100), 34, worn ? SpriteFactory.CheckIcon() : null);
            }
            b.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 66), new Vector2(280, 100));
        }

        private void Buy(WardrobeItem item)
        {
            var r = App.Economy.BuyWardrobe(item.Id);
            if (r == PurchaseResult.NotEnoughCoins)
            {
                App.Audio.Play(Sfx.Error);
                App.Popup.ShowMessage(Loc.T("wardrobe.title"), Loc.T("city.not_enough_coins"), SpriteFactory.Coin());
                return;
            }
            if (r != PurchaseResult.Success) return;
            App.Audio.Play(Sfx.Purchase);
            LookChanged();
            UIKit.Pop(_preview.transform, 0.7f, 0.4f);
        }

        private void LookChanged()
        {
            App.ApplyLook();
            Rebuild();
        }

        public override bool OnBack()
        {
            if (ReturnToPet) App.OpenPet(); else App.GoHome();
            return true;
        }

        /// <summary>Set when opened from the pet room, so "back" returns there.</summary>
        public bool ReturnToPet;
    }

    /// <summary>Makes any graphic tappable (e.g. the mascot opens the wardrobe).</summary>
    public sealed class TapTarget : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public System.Action OnTap;

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            UIKit.ClickSound?.Invoke();
            OnTap?.Invoke();
        }
    }
}
