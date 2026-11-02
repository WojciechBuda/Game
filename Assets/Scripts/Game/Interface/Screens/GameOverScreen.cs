using System.Collections.Generic;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Loyalty;
using JellyDrop.Util;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>Results: score, best, coin breakdown, completed missions and the free lucky wheel.</summary>
    public sealed class GameOverScreen : UIScreen
    {
        private Text _title;
        private Text _score;
        private Text _best;
        private Image _newBest;
        private Text _breakdown;
        private Text _total;
        private Text _missions;
        private Text _loyalty;
        private LoyaltyGain _gain;
        private WheelPanel _wheel;
        private Image _avatar;

        private GameResult _result;
        private GameRewards _rewards;
        private bool _spun;

        public GameOverScreen With(GameResult result, GameRewards rewards, LoyaltyGain gain = null)
        {
            _result = result;
            _rewards = rewards;
            _gain = gain;
            _spun = false;
            return this;
        }

        protected override void Build()
        {
            var card = UIKit.NeonCard(Root, ColorUtil.Pink, PixelCanvas.WithAlpha(ColorUtil.Panel, 0.95f), 0.8f, true, "Card");
            card.rectTransform.Fill(40, 40, 40, 40);
            var c = card.rectTransform;

            _title = UIKit.Text(c, Loc.T("go.game_over"), 92, ColorUtil.Pink).Fit(44);
            _title.rectTransform.At(UIKit.Top, new Vector2(0, -90), new Vector2(900, 130));

            _avatar = UIKit.Image(c, null, Color.white, false, "Avatar");
            _avatar.rectTransform.At(UIKit.Top, new Vector2(-330, -250), new Vector2(190, 190));
            _avatar.gameObject.AddComponent<Bobber>().Amplitude = 8f;

            _score = UIKit.Text(c, "0", 110, ColorUtil.Purple);
            _score.rectTransform.At(UIKit.Top, new Vector2(60, -240), new Vector2(700, 140));
            _best = UIKit.Text(c, "", 40, ColorUtil.Ink, TextAnchor.MiddleCenter, false);
            _best.rectTransform.At(UIKit.Top, new Vector2(60, -330), new Vector2(700, 60));

            _newBest = UIKit.Panel(c, ColorUtil.Red, 2f, false, "NewBest");
            _newBest.rectTransform.At(UIKit.Top, new Vector2(330, -170), new Vector2(260, 76));
            _newBest.rectTransform.localRotation = Quaternion.Euler(0, 0, -10);
            UIKit.Text(_newBest.transform, Loc.T("go.new_best"), 40, Color.white).Fit(20).rectTransform.Fill(8, 0, 8, 0);

            _breakdown = UIKit.Text(c, "", 38, ColorUtil.Ink, TextAnchor.UpperLeft, false).Fit(24);
            _breakdown.rectTransform.At(UIKit.Top, new Vector2(-150, -520), new Vector2(560, 300));
            _total = UIKit.Text(c, "", 64, ColorUtil.Orange);
            _total.rectTransform.At(UIKit.Top, new Vector2(270, -480), new Vector2(360, 100));
            var coin = UIKit.Image(c, SpriteFactory.Coin(), Color.white);
            coin.rectTransform.At(UIKit.Top, new Vector2(270, -580), new Vector2(90, 90));
            _missions = UIKit.Text(c, "", 34, ColorUtil.Mint, TextAnchor.MiddleCenter, true).Fit(20);
            _missions.rectTransform.At(UIKit.Top, new Vector2(0, -700), new Vector2(900, 90));
            _loyalty = UIKit.Text(c, "", 32, ColorUtil.Orange, TextAnchor.MiddleCenter, false).Fit(18);
            _loyalty.rectTransform.At(UIKit.Top, new Vector2(0, -775), new Vector2(900, 50));

            // Lucky wheel: one free spin per game, then spins for coins
            var wheelArea = UIKit.Node("WheelArea", c);
            wheelArea.Fill();
            _fx = UIKit.Node("Fx", Root);
            _fx.Fill();
            _wheel = WheelPanel.Create(App, wheelArea, _fx, new Vector2(-180, -230), 500, new Vector2(270, -150));
            _wheel.HasFreeSpin = () => !_spun;
            _wheel.UseFreeSpin = () => _spun = true;

            var home = UIKit.Button(c, Loc.T("go.home"), ColorUtil.Purple, App.GoHome, new Vector2(360, 160), 56, SpriteFactory.HomeIcon());
            home.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-215, 120), new Vector2(360, 160));
            var again = UIKit.Button(c, Loc.T("go.again"), ColorUtil.Pink, App.RestartGame, new Vector2(400, 160), 60, SpriteFactory.PlayIcon());
            again.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(215, 120), new Vector2(400, 160));
            _fx.SetAsLastSibling();
        }

        private RectTransform _fx;

        protected override void OnShow()
        {
            if (_result == null) return;
            var e = App.Economy;
            bool sprint = _result.Mode == GameMode.Sprint;

            _title.text = Loc.T(sprint && _result.Completed ? "go.sprint_clear" : _result.Mode == GameMode.Daily ? "go.daily_done" : "go.game_over");
            _avatar.sprite = SpriteFactory.Avatar(e.EquippedAvatar, _rewards.IsPersonalBest || _result.Completed ? AvatarMood.Excited : AvatarMood.Happy);

            if (sprint && _result.Completed) _score.text = UIKit.FormatTime(_result.TimeSeconds);
            else UIKit.CountUp(_score, 0, _result.Score, 1.0f);

            var best = e.BestFor(_result.Mode, _result.Difficulty);
            var difficulty = DifficultySettings.For(_result.Difficulty);
            string tag = $"<color={difficulty.Color}>{Loc.DifficultyName(_result.Difficulty)}</color>";
            _best.text = sprint
                ? (best.bestTime > 0 ? Loc.T("go.best_time", tag, UIKit.FormatTime(best.bestTime), _result.Lines) : Loc.T("go.lines_only", tag, _result.Lines))
                : Loc.T("go.summary", tag, Loc.Number(best.bestScore), _result.Lines, _result.Level);

            _newBest.gameObject.SetActive(_rewards.IsPersonalBest);
            if (_rewards.IsPersonalBest)
            {
                UIKit.Pop(_newBest.transform, 0f, 0.6f);
                App.Audio.Play(Sfx.Win, 0.8f);
            }

            var lines = new List<string> { $"{Loc.T("go.score_bonus")}  +{Loc.Number(_rewards.ScoreCoins)}" };
            if (_rewards.CollectedCoins > 0)
            {
                string mult = difficulty.CoinMultiplier != 1f ? $"  (x{Loc.Decimal(difficulty.CoinMultiplier)})" : "";
                lines.Add($"{Loc.T("go.collected")}  +{Loc.Number(_rewards.CollectedCoins)}{mult}");
            }
            if (_rewards.PersonalBestBonus > 0) lines.Add($"{Loc.T("go.new_best_bonus")}  +{_rewards.PersonalBestBonus}");
            if (_rewards.SprintBonus > 0) lines.Add($"{Loc.T("go.sprint_bonus")}  +{_rewards.SprintBonus}");
            if (_rewards.PremiumBonus > 0) lines.Add($"{Loc.T("go.premium_bonus", Loc.Decimal(e.Config.PremiumCoinMultiplier))}  +{Loc.Number(_rewards.PremiumBonus)}");
            if (_rewards.CityBonus > 0) lines.Add($"{Loc.T("go.bonus", _rewards.BonusPercent)}  +{Loc.Number(_rewards.CityBonus)}");
            if (_rewards.BoosterBonus > 0) lines.Add($"{Loc.T("wheel.booster_line")}  +{Loc.Number(_rewards.BoosterBonus)}");
            _breakdown.text = string.Join("\n", lines);
            _loyalty.text = _gain != null && _gain.Points > 0 ? Loc.T("player.gain", _gain.Points) : "";
            UIKit.CountUp(_total, 0, _rewards.Total, 1.2f, "+");

            var done = new List<string>();
            foreach (var m in _rewards.MissionsCompleted) done.Add("• " + Loc.MissionTitle(m));
            _missions.text = done.Count > 0 ? Loc.T("go.mission_done", string.Join("   ", done)) : "";

            _wheel.ResetWheel();
            JellyDrop.Util.Tweener.Delay(0.4f, () => { if (IsVisible && _rewards.Total > 0) App.Audio.Play(Sfx.Coin); }, this);
        }

        public override void Refresh() => _wheel?.Refresh();

        public override bool OnBack()
        {
            App.GoHome();
            return true;
        }
    }
}
