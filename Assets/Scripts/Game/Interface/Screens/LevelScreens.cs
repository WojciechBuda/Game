using System.Collections.Generic;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.City;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Levels;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>
    /// The level path: a winding trail of level nodes from the bottom up. Every 10th node is a big gold
    /// Boost level. Cleared levels show their stars; the next one bounces.
    /// </summary>
    public sealed class PathScreen : UIScreen
    {
        private const float Spacing = 210f;
        private const float BottomPad = 170f;

        private ScrollRect _scroll;
        private RectTransform _content;
        private Text _stars;
        private int _builtFor = -1;
        private int _builtStars = -1;

        protected override void Build()
        {
            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 120);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(84, -84), new Vector2(120, 120));
            var title = UIKit.Text(Root, Loc.T("home.levels"), 90, ColorUtil.Pink).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -84), new Vector2(600, 120));

            var starPill = UIKit.CurrencyPill(Root, SpriteFactory.Star(), ColorUtil.Ink, out var pill, 220);
            pill.At(UIKit.TopRight, new Vector2(-130, -84), pill.sizeDelta);
            pill.GetChild(1).GetComponent<Image>().color = ColorUtil.Gold;
            _stars = starPill;

            var viewport = UIKit.Panel(Root, new Color(1f, 1f, 1f, 0.35f), 0.8f, true, "Viewport");
            viewport.rectTransform.Fill(24, 30, 24, 180);
            viewport.gameObject.AddComponent<RectMask2D>();

            _content = UIKit.Node("Content", viewport.transform);
            _content.anchorMin = new Vector2(0, 1);
            _content.anchorMax = new Vector2(1, 1);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.offsetMin = _content.offsetMax = Vector2.zero;

            _scroll = viewport.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = viewport.rectTransform;
            _scroll.content = _content;
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Elastic;
            _scroll.scrollSensitivity = 40f;
        }

        protected override void OnShow()
        {
            Rebuild(force: true);
            ScrollToCurrent();
        }

        public override void Refresh()
        {
            _stars.text = App.Progress.TotalStars.ToString();
            Rebuild(force: false);
        }

        private int VisibleLevels => Mathf.Max(30, Mathf.CeilToInt((App.Progress.CurrentLevel + 10) / 10f) * 10);

        private float NodeX(int n) => Mathf.Sin(n * 0.9f) * 260f;
        private float NodeY(int n) => BottomPad + (n - 1) * Spacing; // from the bottom of the content

        private void Rebuild(bool force)
        {
            var progress = App.Progress;
            if (!force && _builtFor == progress.CurrentLevel && _builtStars == progress.TotalStars) return;
            _builtFor = progress.CurrentLevel;
            _builtStars = progress.TotalStars;

            UIKit.ClearChildren(_content);
            int count = VisibleLevels;
            float height = BottomPad * 2 + (count - 1) * Spacing;
            _content.sizeDelta = new Vector2(0, height);

            // Trail dots first so the nodes draw on top.
            for (int n = 1; n < count; n++)
            {
                bool done = n < progress.CurrentLevel;
                for (int k = 1; k <= 4; k++)
                {
                    float t = k / 5f;
                    var dot = UIKit.Image(_content, SpriteFactory.Circle(), done ? PixelCanvas.WithAlpha(ColorUtil.Pink, 0.55f) : new Color(0.17f, 0.18f, 0.36f, 0.14f), false, "Dot");
                    Place(dot.rectTransform, Mathf.Lerp(NodeX(n), NodeX(n + 1), t), Mathf.Lerp(NodeY(n), NodeY(n + 1), t), 22, height);
                }
            }

            for (int n = 1; n <= count; n++) BuildNode(n, height);
            _stars.text = progress.TotalStars.ToString();
        }

        private void BuildNode(int n, float height)
        {
            var progress = App.Progress;
            bool boost = LevelPath.IsBoost(n);
            bool unlocked = progress.IsUnlocked(n);
            bool current = n == progress.CurrentLevel;
            int stars = progress.StarsOf(n);
            float size = boost ? 190f : 150f;

            Color color = !unlocked ? ColorUtil.Hex("#C9CDE6") : boost ? ColorUtil.Gold : current ? ColorUtil.Pink : ColorUtil.Mint;
            int level = n;
            var node = UIKit.Button(_content, boost ? Loc.T("level.boost") : n.ToString(), color, () => OnNode(level),
                new Vector2(size, size), boost ? 40 : 60, !unlocked ? SpriteFactory.LockIcon() : null, "Level" + n);
            Place((RectTransform)node.transform, NodeX(n), NodeY(n), size, height);
            if (!unlocked)
            {
                node.Interactable = false;
                node.ClickWhenDisabled = true;
                if (node.Icon != null) node.Icon.rectTransform.Fill(40, 40, 40, 40);
            }
            if (boost && unlocked)
            {
                var num = UIKit.Text(node.transform, n.ToString(), 34, ColorUtil.Ink, TextAnchor.MiddleCenter, false);
                num.rectTransform.At(UIKit.Bottom, new Vector2(0, -26), new Vector2(160, 44));
            }
            if (current)
            {
                var bob = node.gameObject.AddComponent<Bobber>();
                bob.Amplitude = 10f;
                bob.Speed = 3f;
            }
            if (stars > 0)
            {
                var row = StarRow.Create(_content, 40, 4);
                row.Set(stars);
                Place((RectTransform)row.transform, NodeX(n), NodeY(n) - size / 2f - 24f, 0, height);
                ((RectTransform)row.transform).sizeDelta = new Vector2(128, 40);
            }
        }

        private static void Place(RectTransform rt, float x, float yFromBottom, float size, float contentHeight)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, -(contentHeight - yFromBottom));
            if (size > 0) rt.sizeDelta = new Vector2(size, size);
        }

        private void ScrollToCurrent()
        {
            Canvas.ForceUpdateCanvases();
            float viewH = ((RectTransform)_scroll.viewport).rect.height;
            float contentH = _content.sizeDelta.y;
            if (contentH <= viewH) { _scroll.verticalNormalizedPosition = 0f; return; }
            float y = NodeY(App.Progress.CurrentLevel);
            _scroll.verticalNormalizedPosition = Mathf.Clamp01((y - viewH / 2f) / (contentH - viewH));
        }

        private void OnNode(int level)
        {
            if (!App.Progress.IsUnlocked(level))
            {
                App.Audio.Play(Sfx.Error);
                App.Popup.ShowMessage(Loc.T("level.title", level), Loc.T("level.locked", App.Progress.CurrentLevel), SpriteFactory.LockIcon());
                return;
            }
            App.LevelIntro.Open(level);
        }

        public override bool OnBack()
        {
            App.GoHome();
            return true;
        }
    }

    /// <summary>Popup before a level: goal, piece limit or timer, star scores and rewards.</summary>
    public sealed class LevelIntro : UIScreen
    {
        private RectTransform _content;
        private int _level;

        protected override void Build()
        {
            var card = BuildModalCard(new Vector2(900, 1180), ColorUtil.Panel, ColorUtil.Pink);
            _content = UIKit.Node("Content", card).Fill(40, 40, 40, 40);
        }

        public void Open(int level)
        {
            _level = level;
            UIKit.ClearChildren(_content);
            var def = LevelPath.Get(level);
            bool firstClear = level > App.Progress.LevelsCleared;

            var close = UIKit.IconButton(_content, SpriteFactory.CloseIcon(), ColorUtil.Purple, () => App.Screens.PopModal(this), 90);
            close.GetComponent<RectTransform>().At(UIKit.TopRight, new Vector2(-30, -30), new Vector2(90, 90));

            var title = UIKit.Text(_content, def.IsBoost ? Loc.T("level.boost_title", level) : Loc.T("level.title", level), 76,
                def.IsBoost ? ColorUtil.Orange : ColorUtil.Pink).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -90), new Vector2(700, 110));

            var stars = StarRow.Create(_content, 90, 14);
            stars.Set(App.Progress.StarsOf(level));
            ((RectTransform)stars.transform).At(UIKit.Top, new Vector2(0, -230), new Vector2(300, 90));

            float y = -340;
            if (def.IsBoost)
            {
                var info = UIKit.Text(_content, Loc.T("level.boost_info", Mathf.RoundToInt(def.TimeLimitSeconds)), 40, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(24);
                info.rectTransform.At(UIKit.Top, new Vector2(0, y - 80), new Vector2(780, 220));
                y -= 230;
            }
            else
            {
                var goal = UIKit.Text(_content, Loc.T("level.goal", Loc.Number(def.TargetScore)), 52, ColorUtil.Ink).Fit(28);
                goal.rectTransform.At(UIKit.Top, new Vector2(0, y), new Vector2(780, 80));
                var pieces = UIKit.Text(_content, Loc.T("level.pieces", def.PieceLimit), 40, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(24);
                pieces.rectTransform.At(UIKit.Top, new Vector2(0, y - 75), new Vector2(780, 60));
                y -= 160;
            }

            // Star scores
            var label = UIKit.Text(_content, Loc.T("level.stars"), 34, ColorUtil.InkDim, TextAnchor.MiddleCenter, false);
            label.rectTransform.At(UIKit.Top, new Vector2(0, y), new Vector2(700, 50));
            for (int i = 0; i < 3; i++)
            {
                var star = UIKit.Image(_content, SpriteFactory.Star(), ColorUtil.Gold, false, "Star");
                star.rectTransform.At(UIKit.Top, new Vector2(-260 + i * 260 - 60, y - 70), new Vector2(50, 50));
                var t = UIKit.Text(_content, Loc.Number(def.StarScores[i]), 38, ColorUtil.Ink, TextAnchor.MiddleLeft, false).Fit(22);
                t.rectTransform.At(UIKit.Top, new Vector2(-260 + i * 260 + 50, y - 70), new Vector2(170, 56));
            }
            y -= 150;

            // Reward preview (what a 1-star finish pays)
            var rlabel = UIKit.Text(_content, Loc.T("level.rewards"), 34, ColorUtil.InkDim, TextAnchor.MiddleCenter, false);
            rlabel.rectTransform.At(UIKit.Top, new Vector2(0, y), new Vector2(700, 50));
            var preview = LevelPath.RewardFor(def, def.StarScores[0], 1, firstClear);
            RewardRow.Create(_content, preview, new Vector2(0, y - 75));

            var play = UIKit.Button(_content, Loc.T("home.play"), def.IsBoost ? ColorUtil.Gold : ColorUtil.Pink, Play, new Vector2(560, 160), 72, SpriteFactory.PlayIcon());
            play.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 100), new Vector2(560, 160));

            App.Screens.PushModal(this);
        }

        private void Play()
        {
            App.Screens.PopModal(this);
            App.StartLevel(_level);
        }

        public override bool OnBack()
        {
            App.Screens.PopModal(this);
            return true;
        }
    }

    /// <summary>A centered row of reward icons with amounts (coins and supplies). Zero amounts are hidden.</summary>
    public static class RewardRow
    {
        public static RectTransform Create(Transform parent, LevelReward r, Vector2 topPosition, float scale = 1f)
            => Create(parent, r.Coins, r.Wood, r.Brick, r.Glass, topPosition, scale);

        public static RectTransform Create(Transform parent, int coins, int wood, int brick, int glass, Vector2 topPosition, float scale = 1f)
        {
            var row = UIKit.Node("Rewards", parent).At(UIKit.Top, topPosition, new Vector2(820, 90 * scale));
            var items = new List<(Sprite, int)>();
            if (coins > 0) items.Add((SpriteFactory.Coin(), coins));
            if (wood > 0) items.Add((CitySprites.SupplyIcon(Supply.Wood), wood));
            if (brick > 0) items.Add((CitySprites.SupplyIcon(Supply.Brick), brick));
            if (glass > 0) items.Add((CitySprites.SupplyIcon(Supply.Glass), glass));
            float w = 200f * scale;
            float x0 = -(items.Count - 1) * w / 2f;
            for (int i = 0; i < items.Count; i++)
            {
                var icon = UIKit.Image(row, items[i].Item1, Color.white, false, "Icon");
                icon.rectTransform.At(UIKit.Center, new Vector2(x0 + i * w - 45 * scale, 0), new Vector2(80 * scale, 80 * scale));
                var t = UIKit.Text(row, "+" + Loc.Number(items[i].Item2), Mathf.RoundToInt(44 * scale), ColorUtil.Ink, TextAnchor.MiddleLeft, false).Fit(20);
                t.rectTransform.At(UIKit.Center, new Vector2(x0 + i * w + 45 * scale, 0), new Vector2(110 * scale, 70 * scale));
            }
            return row;
        }
    }

    /// <summary>After a level: stars, score, rewards and what to do next.</summary>
    public sealed class LevelResultScreen : UIScreen
    {
        private RectTransform _content;
        private GameResult _result;
        private GameRewards _rewards;
        private LevelOutcome _outcome;

        public LevelResultScreen With(GameResult result, GameRewards rewards, LevelOutcome outcome)
        {
            _result = result;
            _rewards = rewards;
            _outcome = outcome;
            return this;
        }

        protected override void Build()
        {
            var card = UIKit.NeonCard(Root, ColorUtil.Pink, ColorUtil.Panel, 0.8f, true, "Card");
            card.rectTransform.Fill(40, 60, 40, 60);
            _content = UIKit.Node("Content", card.transform).Fill(30, 30, 30, 30);
        }

        protected override void OnShow()
        {
            UIKit.ClearChildren(_content);
            if (_outcome == null) return;
            var def = _outcome.Level;
            bool won = _outcome.Won;

            string titleKey = def.IsBoost ? "level.boost_done" : won ? "level.complete" : "level.failed";
            var title = UIKit.Text(_content, Loc.T(titleKey), 84, won ? ColorUtil.Pink : ColorUtil.Purple).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -100), new Vector2(900, 130));
            var sub = UIKit.Text(_content, def.IsBoost ? Loc.T("level.boost_title", def.Number) : Loc.T("level.title", def.Number), 40, ColorUtil.InkDim, TextAnchor.MiddleCenter, false);
            sub.rectTransform.At(UIKit.Top, new Vector2(0, -190), new Vector2(800, 60));

            var avatar = UIKit.Image(_content, SpriteFactory.Avatar(App.Economy.EquippedAvatar, won ? AvatarMood.Excited : AvatarMood.Sad), Color.white, false, "Avatar");
            avatar.rectTransform.At(UIKit.Top, new Vector2(0, -350), new Vector2(220, 220));
            avatar.gameObject.AddComponent<Bobber>().Amplitude = 8f;

            var stars = StarRow.Create(_content, 120, 18);
            ((RectTransform)stars.transform).At(UIKit.Top, new Vector2(0, -530), new Vector2(400, 120));
            if (won) stars.Animate(_outcome.Stars, i => App.Audio.PlayPitched(Sfx.Coin, 1f + i * 0.12f));
            else stars.Set(0);

            var scoreLabel = UIKit.Text(_content, Loc.T("common.score"), 34, ColorUtil.InkDim, TextAnchor.MiddleCenter, false);
            scoreLabel.rectTransform.At(UIKit.Top, new Vector2(0, -640), new Vector2(600, 50));
            var score = UIKit.Text(_content, "0", 100, ColorUtil.Ink);
            score.rectTransform.At(UIKit.Top, new Vector2(0, -720), new Vector2(800, 120));
            UIKit.CountUp(score, 0, _result.Score, 1.0f);

            float y = -830;
            if (!won && _outcome.PointsShort > 0)
            {
                var shortText = UIKit.Text(_content, Loc.T("level.short", Loc.Number(_outcome.PointsShort)), 40, ColorUtil.Red, TextAnchor.MiddleCenter, false).Fit(24);
                shortText.rectTransform.At(UIKit.Top, new Vector2(0, y), new Vector2(800, 60));
                y -= 80;
            }

            // Everything earned: level reward plus the normal coins from the game itself.
            var r = _outcome.Reward;
            int coins = r.Coins + (_rewards?.Total ?? 0);
            if (coins > 0 || !r.IsEmpty)
            {
                var label = UIKit.Text(_content, Loc.T("level.rewards"), 34, ColorUtil.InkDim, TextAnchor.MiddleCenter, false);
                label.rectTransform.At(UIKit.Top, new Vector2(0, y), new Vector2(700, 50));
                RewardRow.Create(_content, coins, r.Wood, r.Brick, r.Glass, new Vector2(0, y - 80));
                y -= 170;
                if (_outcome.FirstClear && !r.IsEmpty)
                {
                    var bonus = UIKit.Text(_content, Loc.T("level.first_clear"), 34, ColorUtil.Mint, TextAnchor.MiddleCenter, true);
                    bonus.rectTransform.At(UIKit.Top, new Vector2(0, y), new Vector2(800, 56));
                }
            }

            // Buttons
            int next = def.Number + 1;
            if (won && App.Progress.IsUnlocked(next))
            {
                var nextBtn = UIKit.Button(_content, Loc.T("level.next"), ColorUtil.Pink, () => App.StartLevel(next), new Vector2(620, 160), 66, SpriteFactory.PlayIcon());
                nextBtn.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 290), new Vector2(620, 160));
            }
            else
            {
                var retry = UIKit.Button(_content, Loc.T("level.retry"), ColorUtil.Pink, () => App.StartLevel(def.Number), new Vector2(620, 160), 66);
                retry.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 290), new Vector2(620, 160));
            }
            var path = UIKit.Button(_content, Loc.T("home.levels"), ColorUtil.Purple, App.OpenPath, new Vector2(400, 140), 48);
            path.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-215, 100), new Vector2(400, 140));
            var city = UIKit.Button(_content, Loc.T("home.city"), ColorUtil.Mint, App.OpenCity, new Vector2(400, 140), 48, CitySprites.Building("house"));
            city.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(215, 100), new Vector2(400, 140));

            App.Audio.Play(won ? Sfx.Win : Sfx.GameOver);
        }

        public override bool OnBack()
        {
            App.OpenPath();
            return true;
        }
    }
}
