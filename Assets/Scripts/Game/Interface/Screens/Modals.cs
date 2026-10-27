using JellyDrop.Core;
using System;
using JellyDrop.Audio;
using JellyDrop.Util;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>Reusable popup: messages, confirmations, rewards and the daily login reward.</summary>
    public sealed class Popup : UIScreen
    {
        private RectTransform _card;
        private RectTransform _content;
        private Action _onBack;

        protected override void Build()
        {
            _card = BuildModalCard(new Vector2(900, 980), ColorUtil.Cream);
            _content = UIKit.Node("Content", _card).Fill(40, 40, 40, 40);
        }

        private void Begin(Action onBack)
        {
            UIKit.ClearChildren(_content);
            _onBack = onBack;
        }

        private void Open()
        {
            if (IsVisible && App.Screens.TopModal == this) { UIKit.Pop(_card, 0.85f, 0.25f); Refresh(); }
            else App.Screens.PushModal(this);
        }

        private void Close(Action then)
        {
            App.Screens.PopModal(this);
            then?.Invoke();
        }

        private Text Title(string title, float y = -60)
        {
            var t = UIKit.Text(_content, title, 70, ColorUtil.Pink).Fit(36);
            t.rectTransform.At(UIKit.Top, new Vector2(0, y), new Vector2(800, 110));
            return t;
        }

        private Text Body(string body, float y)
        {
            var t = UIKit.Text(_content, body, 44, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(26);
            t.rectTransform.At(UIKit.Center, new Vector2(0, y), new Vector2(780, 260));
            return t;
        }

        private Image Icon(Sprite icon, float y, float size = 220)
        {
            var img = UIKit.Image(_content, icon, Color.white, false, "Icon");
            img.rectTransform.At(UIKit.Center, new Vector2(0, y), new Vector2(size, size));
            img.gameObject.AddComponent<Bobber>().Amplitude = 8f;
            return img;
        }

        private CandyButton BottomButton(string label, Color color, Action onClick, float x = 0, float width = 520)
        {
            var b = UIKit.Button(_content, label, color, onClick, new Vector2(width, 150), 54);
            b.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(x, 90), new Vector2(width, 150));
            return b;
        }

        // ------------------------------------------------------------------ public API

        public void ShowMessage(string title, string body, Sprite icon, Action onClose = null)
        {
            Begin(() => Close(onClose));
            Title(title);
            if (icon != null) Icon(icon, 150);
            Body(body, icon != null ? -100 : 40);
            BottomButton(Loc.T("common.ok"), ColorUtil.Blue, () => Close(onClose));
            Open();
        }

        public void ShowConfirm(string title, string body, string confirmLabel, Action onConfirm, Action onCancel = null)
        {
            Begin(() => Close(onCancel));
            Title(title);
            Body(body, 60);
            BottomButton(Loc.T("common.cancel"), ColorUtil.Purple, () => Close(onCancel), -205, 380);
            BottomButton(confirmLabel, ColorUtil.Pink, () => Close(onConfirm), 205, 380);
            Open();
        }

        public void ShowReward(string bigText, string body, Sprite icon, Action onClose = null)
        {
            Begin(() => Close(onClose));
            Title(Loc.T("common.yay"));
            var rays = UIKit.Image(_content, SpriteFactory.Star(), new Color(1f, 0.85f, 0.3f, 0.35f), false, "Rays");
            rays.rectTransform.At(UIKit.Center, new Vector2(0, 170), new Vector2(460, 460));
            var raysRt = rays.rectTransform;
            Tweener.Run(30f, k => { if (raysRt != null) raysRt.localRotation = Quaternion.Euler(0, 0, -k * 1800f); }, Ease.Linear, owner: rays);
            if (icon != null) Icon(icon, 170, 240);
            var big = UIKit.Text(_content, bigText, 96, ColorUtil.Orange);
            big.rectTransform.At(UIKit.Center, new Vector2(0, -60), new Vector2(800, 130));
            UIKit.Pop(big.transform, 0.2f, 0.5f);
            Body(body, -190).rectTransform.sizeDelta = new Vector2(780, 140);
            BottomButton(Loc.T("common.awesome"), ColorUtil.Mint, () => Close(onClose));
            Open();
        }

        public void ShowDailyReward()
        {
            var e = App.Economy;
            if (!e.CanClaimStreak) return;
            Begin(() => Close(null));
            Title(Loc.T("missions.daily_gift"));
            int today = e.NextStreakDay;

            for (int d = 1; d <= 7; d++)
            {
                int row = d <= 4 ? 0 : 1;
                int col = row == 0 ? d - 1 : d - 5;
                float x = row == 0 ? -285 + col * 190 : -190 + col * 190;
                float y = row == 0 ? 170 : -20;
                bool claimed = d < today;
                bool isToday = d == today;
                var bg = UIKit.Panel(_content, isToday ? ColorUtil.Yellow : claimed ? ColorUtil.Mint : ColorUtil.PanelLight, 1.6f, false, "Day");
                bg.rectTransform.At(UIKit.Center, new Vector2(x, y), new Vector2(170, 170));
                var day = UIKit.Text(bg.transform, Loc.T("popup.day", d), 28, isToday || claimed ? ColorUtil.Night : ColorUtil.Ink, TextAnchor.UpperCenter, false);
                day.rectTransform.Fill(0, 0, 0, 10);
                var coin = UIKit.Image(bg.transform, claimed ? SpriteFactory.CheckIcon() : SpriteFactory.Coin(), claimed ? ColorUtil.Night : Color.white);
                coin.rectTransform.At(UIKit.Center, new Vector2(0, -2), new Vector2(64, 64));
                var amt = UIKit.Text(bg.transform, e.StreakReward(d).ToString(), 34, isToday || claimed ? ColorUtil.Night : ColorUtil.Ink, TextAnchor.LowerCenter, false);
                amt.rectTransform.Fill(0, 8, 0, 0);
                if (isToday) bg.gameObject.AddComponent<Bobber>().Amplitude = 6f;
            }

            var info = UIKit.Text(_content, Loc.T("popup.come_back"), 36, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(22);
            info.rectTransform.At(UIKit.Center, new Vector2(0, -170), new Vector2(800, 60));

            BottomButton(Loc.T("common.claim", e.StreakReward(today)), ColorUtil.Pink, () =>
            {
                int coins = e.ClaimStreak();
                App.Audio.Play(Sfx.Purchase);
                Close(null);
                if (coins > 0) ShowReward($"+{coins}", Loc.T("popup.gift_collected"), SpriteFactory.Coin());
            });
            Open();
        }

        public override bool OnBack()
        {
            _onBack?.Invoke();
            return true;
        }
    }

    public sealed class PauseScreen : UIScreen
    {
        private CandyButton _music;
        private CandyButton _sfx;

        protected override void Build()
        {
            var card = BuildModalCard(new Vector2(820, 1100), ColorUtil.Cream);
            var title = UIKit.Text(card, Loc.T("pause.title"), 90, ColorUtil.Pink).Fit(44);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -110), new Vector2(700, 130));

            float y = 230;
            Add(card, Loc.T("pause.resume"), ColorUtil.Mint, App.ResumeGame, ref y, SpriteFactory.PlayIcon());
            Add(card, Loc.T("pause.restart"), ColorUtil.Blue, () => { App.Screens.PopModal(this); App.RestartGame(); }, ref y);
            Add(card, Loc.T("pause.end"), ColorUtil.Orange, App.QuitGame, ref y, SpriteFactory.HomeIcon());

            _music = UIKit.Button(card, "", ColorUtil.Purple, () => App.SetMusic(!App.Economy.Data.musicOn), new Vector2(330, 120), 40);
            _music.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(-180, y - 20), new Vector2(330, 120));
            _sfx = UIKit.Button(card, "", ColorUtil.Purple, () => App.SetSfx(!App.Economy.Data.sfxOn), new Vector2(330, 120), 40);
            _sfx.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(180, y - 20), new Vector2(330, 120));

            var hint = UIKit.Text(card, Loc.T("pause.hint"), 30, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(18);
            hint.rectTransform.At(UIKit.Bottom, new Vector2(0, 70), new Vector2(740, 60));
        }

        private void Add(RectTransform card, string label, Color color, Action action, ref float y, Sprite icon = null)
        {
            var b = UIKit.Button(card, label, color, action, new Vector2(600, 150), 58, icon);
            b.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(0, y), new Vector2(600, 150));
            y -= 185;
        }

        private static string Toggle(string key, bool on) => $"{Loc.T(key)}: {Loc.T(on ? "common.on" : "common.off")}";

        public override void Refresh()
        {
            _music.SetLabel(Toggle("settings.music", App.Economy.Data.musicOn));
            _sfx.SetLabel(Toggle("settings.sound", App.Economy.Data.sfxOn));
        }

        public override bool OnBack()
        {
            App.ResumeGame();
            return true;
        }
    }

    /// <summary>First-game gesture guide.</summary>
    public sealed class TutorialOverlay : UIScreen
    {
        protected override void Build()
        {
            var card = BuildModalCard(new Vector2(900, 1260), ColorUtil.Cream);
            var title = UIKit.Text(card, Loc.T("tut.title"), 76, ColorUtil.Pink).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -100), new Vector2(800, 120));

            string[,] rows =
            {
                { Loc.T("tut.tap"), Loc.T("tut.tap_d") },
                { Loc.T("tut.swipe"), Loc.T("tut.swipe_d") },
                { Loc.T("tut.drag"), Loc.T("tut.drag_d") },
                { Loc.T("tut.flick_down"), Loc.T("tut.flick_down_d") },
                { Loc.T("tut.flick_up"), Loc.T("tut.flick_up_d") },
            };
            Color[] colors = { ColorUtil.Pink, ColorUtil.Blue, ColorUtil.Mint, ColorUtil.Orange, ColorUtil.Purple };
            for (int i = 0; i < rows.GetLength(0); i++)
            {
                float y = 330 - i * 135;
                var chip = UIKit.NeonCard(card, colors[i], ColorUtil.PanelLight, 1.6f, false, "Chip", 0.3f);
                chip.rectTransform.At(UIKit.Center, new Vector2(-200, y), new Vector2(360, 105));
                var a = UIKit.Text(chip.transform, rows[i, 0], 40, colors[i]).Fit(20);
                a.rectTransform.Fill(12, 0, 12, 0);
                var b = UIKit.Text(card, rows[i, 1], 40, ColorUtil.Ink, TextAnchor.MiddleLeft, false).Fit(22);
                b.rectTransform.At(UIKit.Center, new Vector2(210, y), new Vector2(420, 100));
            }

            var tip = UIKit.Text(card, Loc.T("tut.tip"), 38, ColorUtil.Purple, TextAnchor.MiddleCenter, false).Fit(22);
            tip.rectTransform.At(UIKit.Center, new Vector2(0, -360), new Vector2(800, 110));

            var go = UIKit.Button(card, Loc.T("tut.go"), ColorUtil.Pink, Close, new Vector2(560, 150), 60);
            go.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 110), new Vector2(560, 150));
        }

        private void Close()
        {
            App.Economy.MarkTutorialSeen();
            App.Screens.PopModal(this);
            App.Game.Resume();
        }

        public override bool OnBack()
        {
            Close();
            return true;
        }
    }

    /// <summary>
    /// Full-screen placeholder ad used in the editor and before the AdMob SDK is installed.
    /// Rewarded ads count down before they can be closed.
    /// </summary>
    public sealed class FakeAdOverlay : UIScreen
    {
        private Text _title;
        private Text _countdown;
        private CandyButton _close;
        private Action<bool> _onClosed;
        private float _remaining;
        private bool _rewarded;

        protected override void Build()
        {
            IsModal = true;
            var bg = UIKit.Image(Root, null, new Color(0.08f, 0.05f, 0.12f, 0.97f), true, "Bg");
            bg.rectTransform.Fill();
            _title = UIKit.Text(Root, "", 80, ColorUtil.Yellow);
            _title.rectTransform.At(UIKit.Center, new Vector2(0, 200), new Vector2(1000, 260));
            _countdown = UIKit.Text(Root, "", 60, Color.white);
            _countdown.rectTransform.At(UIKit.Center, new Vector2(0, -40), new Vector2(900, 120));
            _close = UIKit.Button(Root, Loc.T("common.close"), ColorUtil.Pink, Finish, new Vector2(460, 150), 56);
            _close.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(0, -300), new Vector2(460, 150));
        }

        public void Open(bool rewarded, Action<bool> onClosed)
        {
            _rewarded = rewarded;
            _onClosed = onClosed;
            _remaining = rewarded ? 3f : 1.5f;
            _title.text = rewarded ? "TEST REWARDED AD\n(this is where the video plays)" : "TEST INTERSTITIAL AD";
            _close.gameObject.SetActive(false);
            App.Screens.PushModal(this);
        }

        private void Update()
        {
            if (_remaining <= 0f) return;
            _remaining -= Time.unscaledDeltaTime;
            _countdown.text = _rewarded ? $"reward in {Mathf.CeilToInt(_remaining)}..." : $"{Mathf.CeilToInt(_remaining)}...";
            if (_remaining <= 0f)
            {
                _countdown.text = _rewarded ? "Reward earned!" : "";
                _close.gameObject.SetActive(true);
                UIKit.Pop(_close.transform);
            }
        }

        private void Finish()
        {
            App.Screens.PopModal(this);
            var cb = _onClosed;
            _onClosed = null;
            cb?.Invoke(_rewarded);
        }

        public override bool OnBack() => true; // ads can't be skipped with back
    }
}
