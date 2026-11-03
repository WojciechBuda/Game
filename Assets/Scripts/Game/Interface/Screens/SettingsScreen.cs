using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Services;
using JellyDrop.Monetization;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>
    /// Options: sound and music volume, vibration, language, smoothness / battery saver, Google account,
    /// privacy (policy, ad choices, delete my data) and help. Scrolls on small screens.
    /// </summary>
    public sealed class SettingsScreen : UIScreen
    {
        private RectTransform _content;
        private float _y;
        private VolumeSlider _music;
        private VolumeSlider _sfx;
        private CandyButton _language;
        private CandyButton _haptics;
        private CandyButton _fps;
        private CandyButton _privacy;
        private CandyButton _account;

        protected override void Build()
        {
            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 120);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(84, -84), new Vector2(120, 120));
            var title = UIKit.Text(Root, Loc.T("settings.title"), 86, ColorUtil.Purple).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -84), new Vector2(700, 120));

            var viewport = UIKit.NeonCard(Root, ColorUtil.Blue, ColorUtil.Panel, 0.8f, true, "Card");
            viewport.rectTransform.Fill(40, 50, 40, 180);
            viewport.gameObject.AddComponent<RectMask2D>();
            _content = UIKit.Node("Content", viewport.transform);
            _content.anchorMin = new Vector2(0, 1);
            _content.anchorMax = new Vector2(1, 1);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.offsetMin = _content.offsetMax = Vector2.zero;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport.rectTransform;
            scroll.content = _content;
            scroll.horizontal = false;

            _y = -40;
            Section("settings.sound_section");
            _music = Slider(Loc.T("settings.music"), ColorUtil.Pink, v => App.SetVolumes(v, App.Economy.Data.sfxVolume));
            _sfx = Slider(Loc.T("settings.sound"), ColorUtil.Orange, v => App.SetVolumes(App.Economy.Data.musicVolume, v));
            _sfx.Released = () => App.Audio.Play(Sfx.Coin);
            _haptics = Row(ColorUtil.Pink, ToggleHaptics, "", SpriteFactory.VibrateIcon(true));

            Section("settings.general_section");
            _language = Row(ColorUtil.Blue, () => App.Screens.PushModal(App.LanguagePicker));
            _fps = Row(ColorUtil.Blue, () => App.SetFrameRateMode(App.Economy.Data.frameRateMode + 1));

            Section("settings.account_section");
            _account = Row(ColorUtil.Blue, App.SignInWithGoogle);
            Row(ColorUtil.Purple, App.RestorePurchases, Loc.T("common.restore"));

            Section("settings.privacy_section");
            Row(ColorUtil.Mint, () => App.OpenPrivacy(), Loc.T("settings.privacy_policy"));
            _privacy = Row(ColorUtil.Mint, () => App.Ads.ShowPrivacyOptions(), Loc.T("settings.privacy_choices"));
            Row(ColorUtil.Red, ConfirmDelete, Loc.T("privacy.delete"));

            Section("settings.help_section");
            Row(ColorUtil.Blue, () => App.Screens.PushModal(App.Tutorial), Loc.T("settings.how"));
            Row(ColorUtil.Blue, ContactSupport, Loc.T("settings.contact"));

            // Test helper, only in the editor and development builds.
            if (Application.isEditor || Debug.isDebugBuild)
                Row(ColorUtil.Red, () => App.Economy.AddCoins(5000), "DEBUG: +5000");

            var version = UIKit.Text(_content, $"Stackville v{Application.version}", 30, ColorUtil.InkDim, TextAnchor.MiddleCenter, false);
            version.rectTransform.At(UIKit.Top, new Vector2(0, _y - 30), new Vector2(800, 60));
            _y -= 100;
            _content.sizeDelta = new Vector2(0, -_y);
        }

        private void Section(string key)
        {
            _y -= 20;
            var t = UIKit.Text(_content, Loc.T(key), 34, ColorUtil.InkDim, TextAnchor.MiddleLeft, false).Fit(20);
            t.rectTransform.At(UIKit.Top, new Vector2(0, _y - 25), new Vector2(800, 50));
            _y -= 66;
        }

        private CandyButton Row(Color color, System.Action onClick, string label = "", Sprite icon = null)
        {
            var b = UIKit.Button(_content, label, color, onClick, new Vector2(800, 112), 40, icon);
            b.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(0, _y - 56), new Vector2(800, 112));
            _y -= 128;
            return b;
        }

        private VolumeSlider Slider(string label, Color color, System.Action<float> changed)
        {
            var row = UIKit.Node("Slider", _content);
            row.At(UIKit.Top, new Vector2(0, _y - 70), new Vector2(800, 140));
            var slider = VolumeSlider.Create(row, label, color);
            slider.Changed = changed;
            _y -= 150;
            return slider;
        }

        private void ToggleHaptics()
        {
            bool on = !App.Economy.Data.hapticsOn;
            App.SetHaptics(on);
            if (on) Haptics.Medium();
        }

        private void ConfirmDelete()
        {
            App.Popup.ShowConfirm(Loc.T("privacy.delete"), Loc.T("privacy.delete_body"), Loc.T("privacy.delete_confirm"), App.DeleteAllData);
        }

        private static void ContactSupport()
        {
            string subject = System.Uri.EscapeDataString($"Stackville v{Application.version} ({SystemInfo.operatingSystem})");
            Application.OpenURL($"mailto:{PrivacyPolicyText.Contact}?subject={subject}");
        }

        private static string OnOff(string key, bool on) => $"{Loc.T(key)}: {Loc.T(on ? "common.on" : "common.off")}";

        public override void Refresh()
        {
            var d = App.Economy.Data;
            var chosen = App.Economy.ChosenLanguage;
            string languageName = chosen.HasValue
                ? Loc.NativeName(chosen.Value)
                : Loc.T("settings.auto", Loc.NativeName(Loc.Current));
            _account.SetLabel(App.Online.IsSignedIn ? Loc.T("account.google_name", App.Online.PlayerName) : Loc.T("account.sign_in"));
            _account.SetColor(App.Online.IsSignedIn ? ColorUtil.Mint : ColorUtil.Blue);
            _language.SetLabel($"{Loc.T("settings.language")}: {languageName}");
            _music.Value = d.musicOn ? d.musicVolume : 0f;
            _sfx.Value = d.sfxOn ? d.sfxVolume : 0f;
            _haptics.SetLabel(OnOff("settings.vibration", d.hapticsOn));
            _haptics.SetColor(d.hapticsOn ? ColorUtil.Pink : Color.gray);
            if (_haptics.Icon != null) _haptics.Icon.sprite = SpriteFactory.VibrateIcon(d.hapticsOn);
            _music.Icon.sprite = SpriteFactory.MusicIcon(d.musicOn && d.musicVolume > 0.01f);
            _sfx.Icon.sprite = SpriteFactory.SoundIcon(d.sfxOn && d.sfxVolume > 0.01f);
            _fps.SetLabel(Loc.T("settings.fps." + d.frameRateMode));
            _privacy.gameObject.SetActive(App.Ads != null && App.Ads.PrivacyOptionsRequired);
        }

        public override bool OnBack()
        {
            App.GoHome();
            return true;
        }
    }

    /// <summary>A labelled 0..100% slider: drag or tap anywhere on the track.</summary>
    /// <summary>
    /// The mute icon inside a volume row. It takes the press itself (down + drag), otherwise the slider row
    /// would grab the touch and the tap would just drag the volume to 0.
    /// </summary>
    public sealed class MuteButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerClickHandler
    {
        public System.Action OnTap;
        public void OnPointerDown(PointerEventData e) { }
        public void OnDrag(PointerEventData e) { }
        public void OnPointerClick(PointerEventData e)
        {
            UIKit.ClickSound?.Invoke();
            OnTap?.Invoke();
        }
    }

    public sealed class VolumeSlider : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public System.Action<float> Changed;
        public System.Action Released;
        public Image Icon;
        private float _beforeMute;
        private RectTransform _track;
        private Image _fill;
        private RectTransform _knob;
        private Text _label;
        private Text _percent;
        private string _name;
        private float _value;

        public float Value
        {
            get => _value;
            set { _value = Mathf.Clamp01(value); Draw(); }
        }

        public static VolumeSlider Create(RectTransform row, string label, Color color)
        {
            var hit = UIKit.Image(row, null, new Color(0, 0, 0, 0), true, "Hit");
            hit.rectTransform.Fill();
            var s = row.gameObject.AddComponent<VolumeSlider>();
            s._name = label;
            // Layout (row is 800 x 140): big mute icon on the left, label + percentage on top, track below,
            // all to the right of the icon so the knob never slides under it.
            s._label = UIKit.Text(row, label, 38, ColorUtil.Ink, TextAnchor.MiddleLeft, false).Fit(20);
            s._label.rectTransform.At(UIKit.Top, new Vector2(-10, -34), new Vector2(480, 56));
            // Speaker / note icon: tap it to mute or unmute quickly.
            var iconBg = UIKit.Image(row, SpriteFactory.Circle(), color, true, "IconBg");
            iconBg.rectTransform.At(UIKit.Top, new Vector2(-344, -70), new Vector2(96, 96));
            s.Icon = UIKit.Image(iconBg.rectTransform, null, Color.white, false, "Icon");
            s.Icon.rectTransform.Fill(18, 18, 18, 18);
            iconBg.gameObject.AddComponent<MuteButton>().OnTap = () =>
            {
                float v = s._value > 0.01f ? 0f : (s._beforeMute > 0.01f ? s._beforeMute : 0.8f);
                if (s._value > 0.01f) s._beforeMute = s._value;
                s.Value = v;
                s.Changed?.Invoke(v);
            };
            s._percent = UIKit.Text(row, "", 36, color, TextAnchor.MiddleRight, false);
            s._percent.rectTransform.At(UIKit.Top, new Vector2(290, -34), new Vector2(200, 56));
            var track = UIKit.Panel(row, PixelCanvas.WithAlpha(ColorUtil.Night, 0.35f), 2.5f, false, "Track");
            track.rectTransform.At(UIKit.Bottom, new Vector2(70, 36), new Vector2(620, 30));
            s._track = track.rectTransform;
            s._fill = UIKit.Panel(track.transform, color, 2.5f, false, "Fill");
            s._fill.rectTransform.Fill(0, 0, 0, 0);
            s._fill.type = Image.Type.Filled;
            s._fill.fillMethod = Image.FillMethod.Horizontal;
            var knob = UIKit.Image(track.transform, SpriteFactory.Circle(), Color.white, false, "Knob");
            knob.rectTransform.anchorMin = knob.rectTransform.anchorMax = new Vector2(0, 0.5f);
            knob.rectTransform.sizeDelta = new Vector2(64, 64);
            s._knob = knob.rectTransform;
            return s;
        }

        private void Draw()
        {
            if (_fill == null) return;
            _fill.fillAmount = _value;
            _knob.anchoredPosition = new Vector2(_value * _track.rect.width, 0);
            int pct = Mathf.RoundToInt(_value * 100f);
            _percent.text = pct == 0 ? Loc.T("common.off") : pct + "%";
        }

        public void OnPointerDown(PointerEventData e) => Set(e);
        public void OnDrag(PointerEventData e) => Set(e);
        public void OnPointerUp(PointerEventData e) => Released?.Invoke();

        private void Set(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_track, e.position, e.pressEventCamera, out var local)) return;
            float v = Mathf.Clamp01((local.x - _track.rect.xMin) / Mathf.Max(1f, _track.rect.width));
            // Snap to 5% steps so 0 and 100 are easy to hit.
            v = Mathf.Round(v * 20f) / 20f;
            if (Mathf.Approximately(v, _value)) return;
            _value = v;
            Draw();
            Changed?.Invoke(v);
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
