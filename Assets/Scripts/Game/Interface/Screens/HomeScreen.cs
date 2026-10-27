using JellyDrop.Core;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    public sealed class HomeScreen : UIScreen
    {
        private Text _coins;
        private Text _shards;
        private Image _avatar;
        private Text _best;
        private CandyButton _adButton;
        private CandyButton _missionsButton;
        private CandyButton _premiumButton;
        private Text _dailyInfo;
        private Text _difficultyInfo;
        private readonly System.Collections.Generic.Dictionary<Difficulty, CandyButton> _difficultyButtons =
            new System.Collections.Generic.Dictionary<Difficulty, CandyButton>();

        protected override void Build()
        {
            // Top bar: currencies + settings
            _coins = UIKit.CurrencyPill(Root, SpriteFactory.Coin(), ColorUtil.Ink, out var coinPill);
            coinPill.At(UIKit.TopLeft, new Vector2(190, -80), coinPill.sizeDelta);
            _shards = UIKit.CurrencyPill(Root, SpriteFactory.Shard(), ColorUtil.Ink, out var shardPill, 200);
            shardPill.At(UIKit.TopLeft, new Vector2(460, -80), shardPill.sizeDelta);
            var gear = UIKit.IconButton(Root, SpriteFactory.GearIcon(), ColorUtil.Purple, App.OpenSettings, 120);
            gear.GetComponent<RectTransform>().At(UIKit.TopRight, new Vector2(-90, -84), new Vector2(120, 120));

            // Title: each letter its own colour and bob
            const string title = "JELLY DROP";
            var titleRow = UIKit.Node("Title", Root).At(UIKit.Top, new Vector2(0, -320), new Vector2(1000, 220));
            float letterW = 96f;
            float startX = -(title.Length - 1) * letterW / 2f;
            for (int i = 0; i < title.Length; i++)
            {
                if (title[i] == ' ') continue;
                var letter = UIKit.Text(titleRow, title[i].ToString(), 150, ColorUtil.Rainbow[i % ColorUtil.Rainbow.Length]);
                letter.rectTransform.At(UIKit.Center, new Vector2(startX + i * letterW, i < 5 ? 40 : -40), new Vector2(130, 180));
                var outline = letter.GetComponent<Outline>();
                outline.effectColor = PixelCanvas.WithAlpha(ColorUtil.Rainbow[i % ColorUtil.Rainbow.Length], 0.45f);
                outline.effectDistance = new Vector2(7, 7);
                var bob = letter.gameObject.AddComponent<Bobber>();
                bob.Amplitude = 10f;
                bob.Speed = 2.4f;
                bob.Phase = i * 0.6f;
                bob.Rotation = 6f;
            }

            // Mascot
            _avatar = UIKit.Image(Root, null, Color.white, false, "Mascot");
            _avatar.rectTransform.At(UIKit.Center, new Vector2(0, 350), new Vector2(300, 300));
            var mascotBob = _avatar.gameObject.AddComponent<Bobber>();
            mascotBob.Amplitude = 18f;
            mascotBob.Speed = 2f;

            _best = UIKit.Text(Root, "", 42, Color.white).Fit();
            _best.rectTransform.At(UIKit.Center, new Vector2(0, 170), new Vector2(900, 64));

            // Difficulty picker
            float dx = -360f;
            foreach (var d in DifficultySettings.All)
            {
                var settings = DifficultySettings.For(d);
                var level = d;
                var b = UIKit.Button(Root, Loc.DifficultyName(d), ColorUtil.Hex(settings.Color), () => SelectDifficulty(level), new Vector2(225, 104), 36);
                b.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(dx, 70), new Vector2(225, 104));
                _difficultyButtons[d] = b;
                dx += 240f;
            }
            _difficultyInfo = UIKit.Text(Root, "", 30, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(18);
            _difficultyInfo.rectTransform.At(UIKit.Center, new Vector2(0, -12), new Vector2(980, 50));

            // Main play button
            var play = UIKit.Button(Root, Loc.T("home.play"), ColorUtil.Pink, () => App.StartGame(GameMode.Classic), new Vector2(620, 180), 96, SpriteFactory.PlayIcon());
            play.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(0, -150), new Vector2(620, 180));
            var pulse = play.gameObject.AddComponent<Bobber>();
            pulse.Amplitude = 6f;
            pulse.Speed = 3f;

            var sprint = UIKit.Button(Root, Loc.T("home.sprint"), ColorUtil.Blue, () => App.StartGame(GameMode.Sprint), new Vector2(380, 140), 50);
            sprint.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(-200, -335), new Vector2(380, 130));
            var daily = UIKit.Button(Root, Loc.T("home.daily"), ColorUtil.Orange, () => App.StartGame(GameMode.Daily), new Vector2(380, 140), 50);
            daily.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(200, -335), new Vector2(380, 130));
            _dailyInfo = UIKit.Text(Root, "", 28, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(16);
            _dailyInfo.rectTransform.At(UIKit.Center, new Vector2(200, -428), new Vector2(420, 50));

            // Bottom row
            var shop = UIKit.Button(Root, Loc.T("home.shop"), ColorUtil.Mint, () => App.OpenShop(), new Vector2(300, 150), 46);
            shop.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-330, 330), new Vector2(300, 150));
            _missionsButton = UIKit.Button(Root, Loc.T("home.missions"), ColorUtil.Purple, App.OpenMissions, new Vector2(300, 150), 46);
            _missionsButton.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 330), new Vector2(300, 150));
            _premiumButton = UIKit.Button(Root, Loc.T("home.noads"), ColorUtil.Gold, () => App.OpenShop(ShopTab.Premium), new Vector2(300, 150), 46, SpriteFactory.Star());
            _premiumButton.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(330, 330), new Vector2(300, 150));

            _adButton = UIKit.Button(Root, "", ColorUtil.Yellow, () => App.WatchAdForCoins(), new Vector2(760, 130), 44, SpriteFactory.AdIcon());
            _adButton.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 150), new Vector2(760, 130));
        }

        public override void Refresh()
        {
            var e = App.Economy;
            _coins.text = Loc.Number(e.Coins);
            _shards.text = e.Shards.ToString();
            _avatar.sprite = SpriteFactory.Avatar(e.EquippedAvatar, AvatarMood.Happy);

            var difficulty = e.SelectedDifficulty;
            var settings = DifficultySettings.For(difficulty);
            int best = e.BestFor(GameMode.Classic, difficulty).bestScore;
            _best.text = best > 0 ? Loc.T("home.best", Loc.DifficultyName(difficulty), Loc.Number(best)) : Loc.T("home.pick");
            _difficultyInfo.text = Loc.DifficultyDescription(difficulty);
            foreach (var kv in _difficultyButtons)
            {
                bool selected = kv.Key == difficulty;
                var c = ColorUtil.Hex(DifficultySettings.For(kv.Key).Color);
                kv.Value.SetColor(selected ? c : Color.Lerp(c, ColorUtil.InkDim, 0.6f));
                kv.Value.transform.localScale = Vector3.one * (selected ? 1.08f : 0.92f);
            }

            int dailyBest = e.DailyChallengeBestToday;
            _dailyInfo.text = dailyBest > 0 ? Loc.T("home.daily_today", Loc.Number(dailyBest)) : Loc.T("home.daily_info");

            int left = e.RewardedAdsLeftToday;
            _adButton.SetLabel(left > 0 ? Loc.T("home.free_coins", e.Config.RewardedAdCoins, left, e.Config.MaxRewardedAdsPerDay) : Loc.T("home.free_coins_tomorrow"));
            _adButton.Interactable = left > 0;

            _missionsButton.SetBadge(e.UnclaimedMissionCount + (e.CanClaimStreak ? 1 : 0));
            _premiumButton.gameObject.SetActive(!e.IsPremium);
        }

        private void SelectDifficulty(Difficulty difficulty)
        {
            if (App.Economy.SelectedDifficulty == difficulty) return;
            App.Economy.SelectDifficulty(difficulty);
            App.Audio.Play(Audio.Sfx.Pop);
            Refresh();
        }

        protected override void OnShow()
        {
            App.Audio.PlayThemeMusic(App.Economy.EquippedTheme);
        }

        public override bool OnBack()
        {
            App.Popup.ShowConfirm(Loc.T("home.quit_title"), Loc.T("home.quit_body"), Loc.T("home.quit"), Application.Quit);
            return true;
        }
    }
}
