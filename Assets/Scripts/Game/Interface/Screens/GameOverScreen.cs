using System.Collections.Generic;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
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
        private WheelView _wheel;
        private CandyButton _spin;
        private Text _spinInfo;
        private Image _avatar;

        private GameResult _result;
        private GameRewards _rewards;
        private bool _spun;
        private bool _spinning;
        private readonly SystemRandom _rng = new SystemRandom();

        public GameOverScreen With(GameResult result, GameRewards rewards)
        {
            _result = result;
            _rewards = rewards;
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

            // Lucky wheel
            _wheel = UIKit.Node("Wheel", c).gameObject.AddComponent<WheelView>();
            ((RectTransform)_wheel.transform).At(UIKit.Center, new Vector2(-180, -230), new Vector2(520, 520));
            _wheel.Build(App.Economy.Wheel);

            _spin = UIKit.Button(c, Loc.T("go.spin"), ColorUtil.Orange, Spin, new Vector2(330, 160), 64);
            _spin.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(270, -170), new Vector2(330, 160));
            _spinInfo = UIKit.Text(c, Loc.T("go.free_spin"), 32, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(20);
            _spinInfo.rectTransform.At(UIKit.Center, new Vector2(270, -330), new Vector2(340, 110));
            var odds = UIKit.Button(c, Loc.T("go.odds"), ColorUtil.Purple, ShowOdds, new Vector2(160, 80), 32);
            odds.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(270, -440), new Vector2(160, 80));

            var home = UIKit.Button(c, Loc.T("go.home"), ColorUtil.Purple, App.GoHome, new Vector2(360, 160), 56, SpriteFactory.HomeIcon());
            home.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-215, 120), new Vector2(360, 160));
            var again = UIKit.Button(c, Loc.T("go.again"), ColorUtil.Pink, App.RestartGame, new Vector2(400, 160), 60, SpriteFactory.PlayIcon());
            again.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(215, 120), new Vector2(400, 160));
        }

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
            _breakdown.text = string.Join("\n", lines);
            UIKit.CountUp(_total, 0, _rewards.Total, 1.2f, "+");

            var done = new List<string>();
            foreach (var m in _rewards.MissionsCompleted) done.Add("• " + Loc.MissionTitle(m));
            _missions.text = done.Count > 0 ? Loc.T("go.mission_done", string.Join("   ", done)) : "";

            _wheel.ResetRotation();
            _spinning = false;
            UpdateSpinButton();
            JellyDrop.Util.Tweener.Delay(0.4f, () => { if (IsVisible && _rewards.Total > 0) App.Audio.Play(Sfx.Coin); }, this);
        }

        private void UpdateSpinButton()
        {
            _spin.Interactable = !_spun && !_spinning;
            _spin.SetLabel(Loc.T(_spun ? "common.done" : "go.spin"));
        }

        private void Spin()
        {
            if (_spun || _spinning) return;
            _spinning = true;
            var result = App.Economy.SpinWheel(_rng);
            _spun = true;
            UpdateSpinButton();
            _wheel.SpinTo(result.SegmentIndex, () => App.Audio.Play(Sfx.WheelTick, 0.6f), () =>
            {
                _spinning = false;
                UpdateSpinButton();
                App.Audio.Play(Sfx.Purchase);
                var seg = result.Segment;
                switch (seg.Kind)
                {
                    case WheelRewardKind.Coins:
                        App.Popup.ShowReward($"+{Loc.Number(seg.Amount)}", Loc.T("go.lucky_spin"), SpriteFactory.Coin());
                        break;
                    case WheelRewardKind.Shard:
                        App.Popup.ShowReward($"+{seg.Amount}", Loc.T("go.shard_body"), SpriteFactory.Shard());
                        break;
                    case WheelRewardKind.PowerUp:
                        App.Popup.ShowReward($"+1 {Loc.PowerUpShortName(result.PowerUp)}", Loc.T("go.free_powerup"), SpriteFactory.PowerUpIcon(result.PowerUp));
                        break;
                }
            });
        }

        /// <summary>Shows the exact chances (good practice for random rewards; required in some regions).</summary>
        private void ShowOdds()
        {
            var w = App.Economy.Wheel;
            var lines = new List<string>();
            for (int i = 0; i < w.Segments.Count; i++)
            {
                var s = w.Segments[i];
                string name = s.Kind == WheelRewardKind.Coins ? Loc.Coins(s.Amount) : Loc.T(s.Kind == WheelRewardKind.Shard ? "go.odds_shard" : "go.odds_powerup");
                lines.Add($"{name}: {Loc.Decimal((float)System.Math.Round(w.ProbabilityOf(i) * 100.0, 1))}%");
            }
            App.Popup.ShowMessage(Loc.T("go.odds_title"), string.Join("\n", lines), null);
        }

        public override bool OnBack()
        {
            if (!_spinning) App.GoHome();
            return true;
        }
    }

    /// <summary>Colourful wheel built from radial-filled images, with an ease-out spin.</summary>
    public sealed class WheelView : MonoBehaviour
    {
        private RectTransform _disc;
        private int _count;
        private float _angle;

        public void Build(LuckyWheel wheel)
        {
            var rt = (RectTransform)transform;
            _count = wheel.Segments.Count;
            float size = rt.sizeDelta.x;

            var glow = UIKit.Image(rt, SpriteFactory.SoftDot(), PixelCanvas.WithAlpha(ColorUtil.Pink, 0.6f), false, "Glow");
            glow.rectTransform.Fill(-60, -60, -60, -60);
            var rim = UIKit.Image(rt, SpriteFactory.Circle(), ColorUtil.Pink, false, "Rim");
            rim.rectTransform.Fill(-14, -14, -14, -14);

            _disc = UIKit.Node("Disc", rt).Fill();
            float seg = 360f / _count;
            for (int i = 0; i < _count; i++)
            {
                var s = wheel.Segments[i];
                var slice = UIKit.Image(_disc, SpriteFactory.Circle(), ColorUtil.Hex(s.Color), false, "Slice");
                slice.rectTransform.Fill();
                slice.type = Image.Type.Filled;
                slice.fillMethod = Image.FillMethod.Radial360;
                slice.fillOrigin = (int)Image.Origin360.Top;
                slice.fillClockwise = true;
                slice.fillAmount = 1f / _count;
                slice.rectTransform.localRotation = Quaternion.Euler(0, 0, -i * seg);

                float mid = (i + 0.5f) * seg * Mathf.Deg2Rad;
                float r = size * 0.33f;
                string text = s.Kind == WheelRewardKind.PowerUp ? Loc.T("wheel.power") : s.Kind == WheelRewardKind.Shard ? Loc.T("wheel.shard") : s.Label;
                var label = UIKit.Text(_disc, text, text.Length > 4 ? 30 : 42, ColorUtil.Night, TextAnchor.MiddleCenter, false).Fit(18);
                label.rectTransform.At(UIKit.Center, new Vector2(Mathf.Sin(mid) * r, Mathf.Cos(mid) * r), new Vector2(150, 60));
                label.rectTransform.localRotation = Quaternion.Euler(0, 0, -(i + 0.5f) * seg);
            }

            var hub = UIKit.Image(rt, SpriteFactory.Circle(), ColorUtil.Night, false, "Hub");
            hub.rectTransform.At(UIKit.Center, Vector2.zero, new Vector2(size * 0.22f, size * 0.22f));
            var star = UIKit.Image(hub.transform, SpriteFactory.Star(), ColorUtil.Yellow, false, "Star");
            star.rectTransform.Fill(14, 14, 14, 14);

            var pointer = UIKit.Image(rt, SpriteFactory.Triangle(), ColorUtil.Red, false, "Pointer");
            pointer.rectTransform.At(UIKit.Top, new Vector2(0, 10), new Vector2(80, 80));
        }

        public void ResetRotation()
        {
            _angle = 0f;
            _disc.localRotation = Quaternion.identity;
        }

        public void SpinTo(int index, System.Action onTick, System.Action onDone)
        {
            float seg = 360f / _count;
            float jitter = Random.Range(-0.35f, 0.35f) * seg;
            // Rotating the disc counter-clockwise by the slice's centre angle brings it under the pointer.
            float target = (index + 0.5f) * seg + jitter;
            float start = _angle % 360f;
            float end = 360f * 5f + target;
            int lastSlice = -1;
            Tweener.Run(4f, k =>
            {
                if (_disc == null) return;
                _angle = Mathf.LerpUnclamped(start, end, k);
                _disc.localRotation = Quaternion.Euler(0, 0, _angle);
                int slice = Mathf.FloorToInt(_angle / seg);
                if (slice != lastSlice) { lastSlice = slice; onTick?.Invoke(); }
            }, Ease.OutQuint, onDone, owner: this);
        }
    }
}
