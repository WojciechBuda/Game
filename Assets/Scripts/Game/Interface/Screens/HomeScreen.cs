using System.Collections.Generic;
using JellyDrop.Core;
using JellyDrop.Core.Levels;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>
    /// The hub: the next level on the path is the big button; the city and arcade modes sit right under it.
    /// </summary>
    public sealed class HomeScreen : UIScreen
    {
        private ResourceBar _resources;
        private Image _avatar;
        private Text _cityInfo;
        private CandyButton _play;
        private CandyButton _city;
        private CandyButton _adButton;
        private CandyButton _missionsButton;
        private CandyButton _premiumButton;

        protected override void Build()
        {
            _resources = ResourceBar.Create(Root, UIKit.TopLeft, new Vector2(440, -80));
            var gear = UIKit.IconButton(Root, SpriteFactory.GearIcon(), ColorUtil.Purple, App.OpenSettings, 110);
            gear.GetComponent<RectTransform>().At(UIKit.TopRight, new Vector2(-80, -80), new Vector2(110, 110));

            // Title: each letter its own colour and bob
            const string title = "JELLY DROP";
            var titleRow = UIKit.Node("Title", Root).At(UIKit.Top, new Vector2(0, -300), new Vector2(1000, 220));
            float letterW = 96f;
            float startX = -(title.Length - 1) * letterW / 2f;
            for (int i = 0; i < title.Length; i++)
            {
                if (title[i] == ' ') continue;
                var letter = UIKit.Text(titleRow, title[i].ToString(), 150, ColorUtil.Rainbow[i % ColorUtil.Rainbow.Length]);
                letter.rectTransform.At(UIKit.Center, new Vector2(startX + i * letterW, i < 5 ? 40 : -40), new Vector2(130, 180));
                var outline = letter.GetComponent<Outline>();
                outline.effectDistance = new Vector2(6, 6);
                var bob = letter.gameObject.AddComponent<Bobber>();
                bob.Amplitude = 10f;
                bob.Speed = 2.4f;
                bob.Phase = i * 0.6f;
                bob.Rotation = 6f;
            }

            _avatar = UIKit.Image(Root, null, Color.white, false, "Mascot");
            _avatar.rectTransform.At(UIKit.Center, new Vector2(0, 330), new Vector2(250, 250));
            var mascotBob = _avatar.gameObject.AddComponent<Bobber>();
            mascotBob.Amplitude = 16f;
            mascotBob.Speed = 2f;

            _cityInfo = UIKit.Text(Root, "", 34, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(20);
            _cityInfo.rectTransform.At(UIKit.Center, new Vector2(0, 175), new Vector2(900, 56));

            // The next level is the main call to action.
            _play = UIKit.Button(Root, "", ColorUtil.Pink, () => App.OpenPath(), new Vector2(720, 200), 84, SpriteFactory.PlayIcon());
            _play.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(0, 30), new Vector2(720, 200));
            var pulse = _play.gameObject.AddComponent<Bobber>();
            pulse.Amplitude = 6f;
            pulse.Speed = 3f;

            _city = UIKit.Button(Root, Loc.T("home.city"), ColorUtil.Mint, App.OpenCity, new Vector2(350, 170), 54, CitySprites.Building("house"));
            _city.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(-185, -180), new Vector2(350, 170));
            var arcade = UIKit.Button(Root, Loc.T("home.arcade"), ColorUtil.Blue, App.OpenArcade, new Vector2(350, 170), 54, SpriteFactory.Block(Core.Economy.BlockStyle.Jelly, ColorUtil.Yellow));
            arcade.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(185, -180), new Vector2(350, 170));

            var shop = UIKit.Button(Root, Loc.T("home.shop"), ColorUtil.Orange, () => App.OpenShop(), new Vector2(300, 140), 44);
            shop.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-330, 320), new Vector2(300, 140));
            _missionsButton = UIKit.Button(Root, Loc.T("home.missions"), ColorUtil.Purple, App.OpenMissions, new Vector2(300, 140), 44);
            _missionsButton.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 320), new Vector2(300, 140));
            _premiumButton = UIKit.Button(Root, Loc.T("home.noads"), ColorUtil.Gold, () => App.OpenShop(ShopTab.Premium), new Vector2(300, 140), 40, SpriteFactory.Star());
            _premiumButton.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(330, 320), new Vector2(300, 140));

            _adButton = UIKit.Button(Root, "", ColorUtil.Yellow, () => App.WatchAdForCoins(), new Vector2(760, 120), 42, SpriteFactory.AdIcon());
            _adButton.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 150), new Vector2(760, 120));
        }

        public override void Refresh()
        {
            var e = App.Economy;
            _resources.Refresh(App);
            _avatar.sprite = SpriteFactory.Avatar(e.EquippedAvatar, AvatarMood.Happy);

            int level = App.Progress.CurrentLevel;
            _play.SetLabel(LevelPath.IsBoost(level) ? Loc.T("level.boost_title", level) : Loc.T("home.play_level", level));
            _play.SetColor(LevelPath.IsBoost(level) ? ColorUtil.Gold : ColorUtil.Pink);

            var city = App.City;
            _cityInfo.text = Loc.T("city.level", city.CityLevel) + "  ·  " + Loc.T("city.population_max", city.Population);
            _city.SetBadge(city.PendingTotal().IsEmpty ? 0 : 1);

            int left = e.RewardedAdsLeftToday;
            _adButton.SetLabel(left > 0 ? Loc.T("home.free_coins", e.Config.RewardedAdCoins, left, e.Config.MaxRewardedAdsPerDay) : Loc.T("home.free_coins_tomorrow"));
            _adButton.Interactable = left > 0;

            _missionsButton.SetBadge(e.UnclaimedMissionCount + (e.CanClaimStreak ? 1 : 0));
            _premiumButton.gameObject.SetActive(!e.IsPremium);
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

    /// <summary>Endless modes: Classic with a difficulty, Sprint 40 and the Daily Challenge.</summary>
    public sealed class ArcadeScreen : UIScreen
    {
        private Text _best;
        private Text _difficultyInfo;
        private Text _dailyInfo;
        private readonly Dictionary<Difficulty, CandyButton> _difficultyButtons = new Dictionary<Difficulty, CandyButton>();

        protected override void Build()
        {
            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 120);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(84, -84), new Vector2(120, 120));
            var title = UIKit.Text(Root, Loc.T("home.arcade"), 96, ColorUtil.Blue).Fit(44);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -90), new Vector2(700, 130));

            var info = UIKit.Text(Root, Loc.T("arcade.info"), 36, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(22);
            info.rectTransform.At(UIKit.Top, new Vector2(0, -220), new Vector2(900, 110));

            var preview = UIKit.Image(Root, SpriteFactory.Avatar(App.Economy.EquippedAvatar, AvatarMood.Excited), Color.white, false, "Mascot");
            preview.rectTransform.At(UIKit.Center, new Vector2(0, 330), new Vector2(240, 240));
            preview.gameObject.AddComponent<Bobber>().Amplitude = 12f;

            _best = UIKit.Text(Root, "", 42, ColorUtil.Ink).Fit(24);
            _best.rectTransform.At(UIKit.Center, new Vector2(0, 170), new Vector2(900, 64));

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

            var play = UIKit.Button(Root, Loc.T("home.play"), ColorUtil.Pink, () => App.StartGame(GameMode.Classic), new Vector2(620, 180), 96, SpriteFactory.PlayIcon());
            play.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(0, -150), new Vector2(620, 180));

            var sprint = UIKit.Button(Root, Loc.T("home.sprint"), ColorUtil.Blue, () => App.StartGame(GameMode.Sprint), new Vector2(380, 130), 50);
            sprint.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(-200, -335), new Vector2(380, 130));
            var daily = UIKit.Button(Root, Loc.T("home.daily"), ColorUtil.Orange, () => App.StartGame(GameMode.Daily), new Vector2(380, 130), 50);
            daily.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(200, -335), new Vector2(380, 130));
            _dailyInfo = UIKit.Text(Root, "", 28, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(16);
            _dailyInfo.rectTransform.At(UIKit.Center, new Vector2(200, -428), new Vector2(420, 50));
        }

        public override void Refresh()
        {
            var e = App.Economy;
            var difficulty = e.SelectedDifficulty;
            int best = e.BestFor(GameMode.Classic, difficulty).bestScore;
            _best.text = best > 0 ? Loc.T("home.best", Loc.DifficultyName(difficulty), Loc.Number(best)) : Loc.T("home.pick");
            _difficultyInfo.text = Loc.DifficultyDescription(difficulty);
            foreach (var kv in _difficultyButtons)
            {
                bool selected = kv.Key == difficulty;
                var c = ColorUtil.Hex(DifficultySettings.For(kv.Key).Color);
                kv.Value.SetColor(selected ? c : Color.Lerp(c, Color.white, 0.55f));
                kv.Value.transform.localScale = Vector3.one * (selected ? 1.08f : 0.92f);
            }
            int dailyBest = e.DailyChallengeBestToday;
            _dailyInfo.text = dailyBest > 0 ? Loc.T("home.daily_today", Loc.Number(dailyBest)) : Loc.T("home.daily_info");
        }

        private void SelectDifficulty(Difficulty difficulty)
        {
            if (App.Economy.SelectedDifficulty == difficulty) return;
            App.Economy.SelectDifficulty(difficulty);
            App.Audio.Play(Audio.Sfx.Pop);
            Refresh();
        }

        public override bool OnBack()
        {
            App.GoHome();
            return true;
        }
    }
}
