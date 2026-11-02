using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.Pet;
using JellyDrop.Services;
using JellyDrop.Util;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    internal static class PetUi
    {
        /// <summary>The avatar face that matches how the pet feels.</summary>
        public static AvatarMood Face(PetMood mood)
        {
            switch (mood)
            {
                case PetMood.Happy: return AvatarMood.Happy;
                case PetMood.Okay: return AvatarMood.Calm;
                case PetMood.Sick: return AvatarMood.Sick;
                case PetMood.Sleeping:
                case PetMood.Tired: return AvatarMood.Sleepy;
                default: return AvatarMood.Sad;
            }
        }
    }

    /// <summary>
    /// The pet room (like Pou): the avatar big in the middle with its four needs. Feed it, wash it, put it to
    /// bed, give presents, dress it up and give medicine when it is sick. Tap the avatar to stroke it.
    /// </summary>
    public sealed class PetScreen : UIScreen
    {
        private static readonly PetNeed[] Needs = { PetNeed.Food, PetNeed.Energy, PetNeed.Fun, PetNeed.Clean };

        private Text _coins;
        private Image _night;
        private Image _avatar;
        private Image _dirt;
        private Text _zzz;
        private Text _mood;
        private Text _bonus;
        private readonly Image[] _bars = new Image[4];
        private readonly Text[] _values = new Text[4];
        private CandyButton _sleep;
        private CandyButton _heal;
        private CandyButton _play;
        private RectTransform _tray;
        private Text _trayTitle;
        private RectTransform _trayContent;
        private float _refresh;
        private float _strokeCooldown;

        protected override void Build()
        {
            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 120);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(84, -84), new Vector2(120, 120));
            var title = UIKit.Text(Root, Loc.T("pet.title"), 84, ColorUtil.Yellow).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(-30, -84), new Vector2(460, 120));
            _coins = UIKit.CurrencyPill(Root, SpriteFactory.Coin(), ColorUtil.Ink, out var pill, 260);
            pill.At(UIKit.TopRight, new Vector2(-150, -84), pill.sizeDelta);

            // The room: a soft rug and a spotlight; it gets dark while the pet sleeps.
            var room = UIKit.Panel(Root, ColorUtil.Glass, 0.8f, false, "Room");
            room.rectTransform.At(UIKit.Top, new Vector2(0, -620), new Vector2(1000, 760));
            var rug = UIKit.Image(room.rectTransform, SpriteFactory.SoftDot(), PixelCanvas.WithAlpha(ColorUtil.Purple, 0.45f), false, "Rug");
            rug.rectTransform.At(UIKit.Bottom, new Vector2(0, 90), new Vector2(700, 160));
            var spot = UIKit.Image(room.rectTransform, SpriteFactory.SoftDot(), PixelCanvas.WithAlpha(ColorUtil.Yellow, 0.25f), false, "Spot");
            spot.rectTransform.At(UIKit.Center, new Vector2(0, 20), new Vector2(620, 620));

            _avatar = UIKit.Image(room.rectTransform, null, Color.white, true, "Pet");
            _avatar.rectTransform.At(UIKit.Center, new Vector2(0, 10), new Vector2(480, 480));
            var bob = _avatar.gameObject.AddComponent<Bobber>();
            bob.Amplitude = 12f;
            bob.Speed = 2f;
            _avatar.gameObject.AddComponent<TapTarget>().OnTap = Stroke;
            _dirt = UIKit.Image(_avatar.rectTransform, PetSprites.Dirt(), Color.white, false, "Dirt");
            _dirt.rectTransform.Fill();

            _night = UIKit.Panel(room.rectTransform, new Color(0.02f, 0.02f, 0.1f, 0.55f), 0.8f, false, "Night");
            _night.rectTransform.Fill();
            _zzz = UIKit.Text(room.rectTransform, "Z z z", 80, ColorUtil.Blue);
            _zzz.rectTransform.At(UIKit.Center, new Vector2(230, 230), new Vector2(300, 120));
            var zb = _zzz.gameObject.AddComponent<Bobber>();
            zb.Amplitude = 14f;
            zb.Speed = 1.4f;

            _mood = UIKit.Text(room.rectTransform, "", 48, ColorUtil.Ink).Fit(24);
            _mood.rectTransform.At(UIKit.Top, new Vector2(0, -50), new Vector2(900, 70));
            _bonus = UIKit.Text(room.rectTransform, "", 32, ColorUtil.Mint).Fit(18);
            _bonus.rectTransform.At(UIKit.Bottom, new Vector2(0, 40), new Vector2(900, 50));

            // Needs: icon + bar, two per row.
            for (int i = 0; i < Needs.Length; i++)
            {
                var need = Needs[i];
                float x = i % 2 == 0 ? -250 : 250;
                float y = -1060 - (i / 2) * 100;
                var icon = UIKit.Image(Root, PetSprites.NeedIcon(need), Color.white, false, "Need");
                icon.rectTransform.At(UIKit.Top, new Vector2(x - 190, y), new Vector2(76, 76));
                _bars[i] = UIKit.ProgressBar(Root, PetSprites.NeedColor(need), new Vector2(300, 40));
                _bars[i].transform.parent.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(x + 10, y), new Vector2(300, 40));
                _values[i] = UIKit.Text(Root, "", 30, ColorUtil.InkDim, TextAnchor.MiddleLeft, false);
                _values[i].rectTransform.At(UIKit.Top, new Vector2(x + 210, y), new Vector2(90, 50));
            }

            // Actions
            var feed = UIKit.Button(Root, Loc.T("pet.feed"), ColorUtil.Orange, () => OpenTray(false), new Vector2(320, 150), 40, PetSprites.Item("apple"));
            feed.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-340, 400), new Vector2(320, 150));
            var wash = UIKit.Button(Root, Loc.T("pet.wash"), ColorUtil.Blue, Wash, new Vector2(320, 150), 40, PetSprites.Soap());
            wash.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 400), new Vector2(320, 150));
            _sleep = UIKit.Button(Root, Loc.T("pet.sleep"), ColorUtil.Purple, ToggleSleep, new Vector2(320, 150), 40, PetSprites.Moon());
            _sleep.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(340, 400), new Vector2(320, 150));

            var gift = UIKit.Button(Root, Loc.T("pet.gift"), ColorUtil.Pink, () => OpenTray(true), new Vector2(320, 150), 40, PetSprites.Item("teddy"));
            gift.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-340, 230), new Vector2(320, 150));
            var dress = UIKit.Button(Root, Loc.T("pet.dress"), ColorUtil.Mint, App.OpenWardrobeFromPet, new Vector2(320, 150), 40, SpriteFactory.Star());
            dress.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 230), new Vector2(320, 150));
            _heal = UIKit.Button(Root, Loc.T("pet.heal"), ColorUtil.Red, Heal, new Vector2(320, 150), 40, PetSprites.Medicine());
            _heal.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(340, 230), new Vector2(320, 150));
            _play = UIKit.Button(Root, Loc.T("pet.play"), ColorUtil.Gold, App.OpenArcade, new Vector2(320, 150), 40, SpriteFactory.PlayIcon());
            _play.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(340, 230), new Vector2(320, 150));

            var hint = UIKit.Text(Root, Loc.T("pet.hint"), 30, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(18);
            hint.rectTransform.At(UIKit.Bottom, new Vector2(0, 100), new Vector2(980, 80));

            BuildTray();
        }

        private void BuildTray()
        {
            var tray = UIKit.NeonCard(Root, ColorUtil.Orange, ColorUtil.Panel, 0.8f, true, "Tray");
            tray.rectTransform.At(UIKit.Bottom, new Vector2(0, 330), new Vector2(1040, 560));
            _tray = tray.rectTransform;
            _trayTitle = UIKit.Text(_tray, "", 54, ColorUtil.Orange).Fit(28);
            _trayTitle.rectTransform.At(UIKit.Top, new Vector2(0, -60), new Vector2(800, 80));
            var close = UIKit.IconButton(_tray, SpriteFactory.CloseIcon(), ColorUtil.Purple, CloseTray, 90);
            close.GetComponent<RectTransform>().At(UIKit.TopRight, new Vector2(-70, -60), new Vector2(90, 90));
            // Scrolls sideways when there are more items than fit.
            var viewport = UIKit.Node("Viewport", _tray);
            viewport.Fill(20, 20, 20, 120);
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = UIKit.Image(viewport, null, new Color(0, 0, 0, 0), true, "Hit");
            hit.rectTransform.Fill();
            _trayContent = UIKit.Node("Items", viewport);
            _trayContent.anchorMin = new Vector2(0, 0);
            _trayContent.anchorMax = new Vector2(0, 1);
            _trayContent.pivot = new Vector2(0, 0.5f);
            _trayContent.offsetMin = _trayContent.offsetMax = Vector2.zero;
            _trayContent.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = _trayContent;
            scroll.vertical = false;
            scroll.horizontal = true;
            var row = _trayContent.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 12;
            row.padding = new RectOffset(4, 4, 0, 0);
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            row.childControlWidth = false;
            row.childControlHeight = false;
            _tray.gameObject.SetActive(false);
        }

        protected override void OnShow()
        {
            _tray.gameObject.SetActive(false);
            Refresh();
        }

        public override void Refresh()
        {
            var pet = App.Pet;
            _coins.text = Loc.Number(App.Economy.Coins);
            var mood = pet.Mood;
            _avatar.sprite = SpriteFactory.Avatar(App.Economy.EquippedAvatar, PetUi.Face(mood));
            _dirt.enabled = pet.Clean < 30f;
            bool sleeping = mood == PetMood.Sleeping;
            _night.enabled = sleeping;
            _zzz.enabled = sleeping;
            _mood.text = Loc.T("pet.mood." + mood);
            _mood.color = mood == PetMood.Happy ? ColorUtil.Mint : mood == PetMood.Okay || sleeping ? ColorUtil.Ink : ColorUtil.Red;
            _bonus.text = pet.CoinBonusPercent > 0 ? Loc.T("pet.bonus", pet.CoinBonusPercent) : Loc.T("pet.bonus_off", PetService.HappyCoinBonus);
            for (int i = 0; i < Needs.Length; i++)
            {
                float v = pet.Need(Needs[i]);
                _bars[i].fillAmount = v / 100f;
                _bars[i].color = v < 25f ? ColorUtil.Red : PetSprites.NeedColor(Needs[i]);
                _values[i].text = Mathf.RoundToInt(v) + "%";
            }
            _sleep.SetLabel(sleeping ? Loc.T("pet.wake") : Loc.T("pet.sleep"));
            bool sick = pet.IsSick;
            _heal.gameObject.SetActive(sick);
            _play.gameObject.SetActive(!sick);
            if (sick) _heal.SetLabel(Loc.T("pet.heal") + "  " + PetCatalog.MedicineCost);
        }

        private void Update()
        {
            if (_strokeCooldown > 0f) _strokeCooldown -= Time.unscaledDeltaTime;
            _refresh += Time.unscaledDeltaTime;
            if (_refresh < 2f) return;
            _refresh = 0f;
            if (!App.Screens.HasModal) Refresh();
        }

        // ------------------------------------------------------------------ actions

        private void Stroke()
        {
            if (_strokeCooldown > 0f) return;
            _strokeCooldown = 0.35f;
            var r = App.Pet.Stroke();
            if (r == PetResult.Sleeping) { Say(Loc.T("pet.shh")); return; }
            App.Audio.Play(Sfx.Pop);
            Haptics.Tap();
            UIKit.Punch(_avatar.transform, 0.08f);
            FloatIcon(PetSprites.Heart(), _avatar.rectTransform.anchoredPosition + new Vector2(Random.Range(-120f, 120f), 160f));
            _avatar.sprite = SpriteFactory.Avatar(App.Economy.EquippedAvatar, AvatarMood.Excited);
            Tweener.Delay(0.6f, Refresh, this);
            Save();
        }

        private void Wash()
        {
            Handle(App.Pet.Wash(), PetSprites.Drop());
        }

        private void ToggleSleep()
        {
            var pet = App.Pet;
            Handle(pet.IsSleeping ? pet.Wake() : pet.Sleep(), pet.IsSleeping ? PetSprites.Sun() : PetSprites.Moon());
        }

        private void Heal()
        {
            Handle(App.Pet.Heal(), PetSprites.Medicine());
        }

        private void Use(PetItem item)
        {
            var r = item.IsGift ? App.Pet.Gift(item.Id) : App.Pet.Feed(item.Id);
            if (r == PetResult.Ok) CloseTray();
            Handle(r, PetSprites.Item(item.Id));
        }

        private void Handle(PetResult r, Sprite icon)
        {
            switch (r)
            {
                case PetResult.Ok:
                    App.Audio.Play(Sfx.Purchase);
                    Haptics.Tap();
                    UIKit.Pop(_avatar.transform, 0.85f, 0.35f);
                    FloatIcon(icon, _avatar.rectTransform.anchoredPosition + new Vector2(0, 200));
                    Save();
                    break;
                case PetResult.NotEnoughCoins: Fail(Loc.T("city.not_enough_coins")); break;
                case PetResult.NotEnoughGoods: Fail(Loc.T("mush.not_enough")); break;
                case PetResult.Full: Fail(Loc.T("pet.full")); break;
                case PetResult.AlreadyClean: Fail(Loc.T("pet.already_clean")); break;
                case PetResult.Sleeping: Fail(Loc.T("pet.shh")); break;
                case PetResult.NotSleepy: Fail(Loc.T("pet.not_sleepy")); break;
                default: Fail(Loc.T("pet.mood.Okay")); break;
            }
            Refresh();
        }

        private void Fail(string text)
        {
            App.Audio.Play(Sfx.Error);
            Say(text);
        }

        private void Say(string text)
        {
            _mood.text = text;
            _mood.color = ColorUtil.Orange;
            UIKit.Punch(_mood.transform, 0.1f);
            _refresh = -1.5f; // keep the message up for a moment
        }

        private void Save() => App.Save.Save(App.Economy.Data);

        /// <summary>An icon that floats up from the pet and fades (hearts when stroking, the food when eating).</summary>
        private void FloatIcon(Sprite sprite, Vector2 from)
        {
            var img = UIKit.Image(_avatar.transform.parent, sprite, Color.white, false, "Float");
            var rt = img.rectTransform;
            rt.At(UIKit.Center, from, new Vector2(110, 110));
            Tweener.Run(0.9f, t =>
            {
                rt.anchoredPosition = from + new Vector2(0, 160f * t);
                img.color = new Color(1, 1, 1, 1f - t);
                rt.localScale = Vector3.one * (0.7f + 0.5f * t);
            }, Ease.OutQuad, () => Destroy(img.gameObject), 0f, img);
        }

        // ------------------------------------------------------------------ food / gift tray

        private void OpenTray(bool gifts)
        {
            if (App.Pet.IsSleeping) { Fail(Loc.T("pet.shh")); return; }
            _trayTitle.text = Loc.T(gifts ? "pet.gift" : "pet.feed");
            UIKit.ClearChildren(_trayContent);
            foreach (var item in gifts ? PetCatalog.Gifts : PetCatalog.Foods) ItemCard(item);
            _tray.gameObject.SetActive(true);
            UIKit.Pop(_tray, 0.85f, 0.3f);
            App.Audio.Play(Sfx.Pop);
        }

        private void CloseTray() => _tray.gameObject.SetActive(false);

        private void ItemCard(PetItem item)
        {
            var card = UIKit.NeonCard(_trayContent, PixelCanvas.WithAlpha(ColorUtil.Blue, 0.6f), ColorUtil.PanelLight, 1.2f, false, "Card", 0.18f);
            card.rectTransform.sizeDelta = new Vector2(item.IsGift ? 300 : 190, 400);
            float w = card.rectTransform.sizeDelta.x;
            var name = UIKit.Text(card.rectTransform, Loc.T("pet.item." + item.Id), 28, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(16);
            name.rectTransform.At(UIKit.Top, new Vector2(0, -40), new Vector2(w - 20, 50));
            var icon = UIKit.Image(card.rectTransform, PetSprites.Item(item.Id), Color.white, false, "Icon");
            icon.rectTransform.At(UIKit.Center, new Vector2(0, 40), new Vector2(120, 120));
            var gain = UIKit.Text(card.rectTransform, GainText(item), 24, ColorUtil.Mint, TextAnchor.MiddleCenter, false).Fit(14);
            gain.rectTransform.At(UIKit.Center, new Vector2(0, -55), new Vector2(w - 20, 80));
            var d = App.Economy.Data;
            string cost = item.Mushrooms > 0 ? item.Mushrooms.ToString() : item.Berries > 0 ? item.Berries.ToString() : item.Honey > 0 ? item.Honey.ToString() : item.Coins.ToString();
            var costIcon = item.Mushrooms > 0 ? IslandSprites.Mushroom() : item.Berries > 0 ? IslandSprites.Berries() : item.Honey > 0 ? IslandSprites.Honey() : SpriteFactory.Coin();
            var b = UIKit.Button(card.rectTransform, cost, item.UsesGoods ? ColorUtil.Mint : ColorUtil.Orange, () => Use(item), new Vector2(w - 30, 90), 34, costIcon);
            b.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 60), new Vector2(w - 30, 90));
            b.Interactable = App.Economy.Coins >= item.Coins && d.mushrooms >= item.Mushrooms && d.berries >= item.Berries && d.honey >= item.Honey;
            b.ClickWhenDisabled = true;
        }

        private static string GainText(PetItem item)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (item.Food > 0) parts.Add("+" + (int)item.Food + " " + Loc.T("pet.need.Food"));
            if (item.Energy > 0) parts.Add("+" + (int)item.Energy + " " + Loc.T("pet.need.Energy"));
            if (item.Fun > 0) parts.Add("+" + (int)item.Fun + " " + Loc.T("pet.need.Fun"));
            return string.Join("\n", parts);
        }

        public override bool OnBack()
        {
            if (_tray.gameObject.activeSelf) { CloseTray(); return true; }
            App.GoHome();
            return true;
        }
    }
}
