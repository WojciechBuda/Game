using JellyDrop.Core;
using JellyDrop.Monetization;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    public sealed class SettingsScreen : UIScreen
    {
        private CandyButton _language;
        private CandyButton _music;
        private CandyButton _sfx;
        private CandyButton _haptics;
        private CandyButton _privacy;

        protected override void Build()
        {
            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 120);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(84, -84), new Vector2(120, 120));
            var title = UIKit.Text(Root, Loc.T("settings.title"), 86, ColorUtil.Purple).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -84), new Vector2(700, 120));

            var card = UIKit.NeonCard(Root, ColorUtil.Blue, ColorUtil.Panel, 0.8f, true, "Card");
            card.rectTransform.Fill(60, 140, 60, 200);
            var c = card.rectTransform;

            float y = -100;
            _language = Toggle(c, ref y, () => App.Screens.PushModal(App.LanguagePicker), ColorUtil.Blue);
            _music = Toggle(c, ref y, () => App.SetMusic(!App.Economy.Data.musicOn));
            _sfx = Toggle(c, ref y, () => App.SetSfx(!App.Economy.Data.sfxOn));
            _haptics = Toggle(c, ref y, () => App.SetHaptics(!App.Economy.Data.hapticsOn));

            y -= 20;
            Action(c, ref y, Loc.T("settings.how"), ColorUtil.Blue, () => App.Screens.PushModal(App.Tutorial));
            Action(c, ref y, Loc.T("common.restore"), ColorUtil.Purple, App.RestorePurchases);
            _privacy = Action(c, ref y, Loc.T("settings.privacy_choices"), ColorUtil.Mint, () => App.Ads.ShowPrivacyOptions());
            Action(c, ref y, Loc.T("settings.privacy_policy"), ColorUtil.Mint, () => Application.OpenURL(MonetizationIds.PrivacyPolicyUrl));

            // Test helper, only in the editor and development builds.
            if (Application.isEditor || Debug.isDebugBuild)
                Action(c, ref y, "DEBUG: +5000", ColorUtil.Red, () => App.Economy.AddCoins(5000));

            var version = UIKit.Text(c, $"Jelly Drop v{Application.version}", 32, ColorUtil.InkDim, TextAnchor.MiddleCenter, false);
            version.rectTransform.At(UIKit.Bottom, new Vector2(0, 50), new Vector2(800, 60));
        }

        private CandyButton Toggle(RectTransform parent, ref float y, System.Action onClick, Color? color = null)
        {
            var b = UIKit.Button(parent, "", color ?? ColorUtil.Pink, onClick, new Vector2(760, 130), 50);
            b.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(0, y), new Vector2(760, 130));
            y -= 155;
            return b;
        }

        private CandyButton Action(RectTransform parent, ref float y, string label, Color color, System.Action onClick)
        {
            var b = UIKit.Button(parent, label, color, onClick, new Vector2(760, 110), 42);
            b.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(0, y), new Vector2(760, 110));
            y -= 135;
            return b;
        }

        private static string OnOff(string key, bool on) => $"{Loc.T(key)}: {Loc.T(on ? "common.on" : "common.off")}";

        public override void Refresh()
        {
            var d = App.Economy.Data;
            var chosen = App.Economy.ChosenLanguage;
            string languageName = chosen.HasValue
                ? Loc.NativeName(chosen.Value)
                : Loc.T("settings.auto", Loc.NativeName(Loc.Current));
            _language.SetLabel($"{Loc.T("settings.language")}: {languageName}");

            _music.SetLabel(OnOff("settings.music", d.musicOn));
            _music.SetColor(d.musicOn ? ColorUtil.Pink : Color.gray);
            _sfx.SetLabel(OnOff("settings.sound", d.sfxOn));
            _sfx.SetColor(d.sfxOn ? ColorUtil.Pink : Color.gray);
            _haptics.SetLabel(OnOff("settings.vibration", d.hapticsOn));
            _haptics.SetColor(d.hapticsOn ? ColorUtil.Pink : Color.gray);
            _privacy.gameObject.SetActive(App.Ads != null && App.Ads.PrivacyOptionsRequired);
        }

        public override bool OnBack()
        {
            App.GoHome();
            return true;
        }
    }

    /// <summary>Popup listing the languages. Picking one rebuilds the UI in that language.</summary>
    public sealed class LanguagePicker : UIScreen
    {
        private RectTransform _list;

        protected override void Build()
        {
            var card = BuildModalCard(new Vector2(820, 1380), ColorUtil.Panel, ColorUtil.Blue);
            var title = UIKit.Text(card, Loc.T("settings.language"), 72, ColorUtil.Blue).Fit(36);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -90), new Vector2(700, 110));
            _list = UIKit.Node("List", card).Fill(50, 170, 50, 170);

            var cancel = UIKit.Button(card, Loc.T("common.cancel"), ColorUtil.Purple, () => App.Screens.PopModal(this), new Vector2(420, 120), 44);
            cancel.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 90), new Vector2(420, 120));
        }

        public override void Refresh()
        {
            UIKit.ClearChildren(_list);
            var chosen = App.Economy.ChosenLanguage;
            float y = -10;

            // "Automatic" follows the phone's language.
            AddOption(Loc.T("settings.auto", Loc.NativeName(App.DeviceLanguage)), !chosen.HasValue, () => App.SetLanguage(null), ref y);
            foreach (var language in Loc.All)
            {
                var l = language;
                AddOption(Loc.NativeName(language), chosen == language, () => App.SetLanguage(l), ref y);
            }
        }

        private void AddOption(string label, bool selected, System.Action pick, ref float y)
        {
            var b = UIKit.Button(_list, label, selected ? ColorUtil.Mint : ColorUtil.Blue, () =>
            {
                App.Screens.PopModal(this);
                pick();
            }, new Vector2(700, 110), 44, selected ? SpriteFactory.CheckIcon() : null);
            b.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(0, y - 55), new Vector2(700, 110));
            y -= 140;
        }

        public override bool OnBack()
        {
            App.Screens.PopModal(this);
            return true;
        }
    }
}
