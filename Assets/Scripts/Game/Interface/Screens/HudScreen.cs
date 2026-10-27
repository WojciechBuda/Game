using System.Collections.Generic;
using JellyDrop.Core;
using JellyDrop.Util;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>In-game overlay: score, level, pause, power-up buttons, HOLD/NEXT labels, floating texts.</summary>
    public sealed class HudScreen : UIScreen
    {
        private Text _score;
        private Text _sub;
        private Text _coins;
        private Text _holdLabel;
        private Text _nextLabel;
        private Text _slowMo;
        private Text _junk;
        private Text _powerUpsLeft;
        private RectTransform _floatingLayer;
        private readonly Dictionary<PowerUpType, CandyButton> _powerButtons = new Dictionary<PowerUpType, CandyButton>();
        private readonly Dictionary<PowerUpType, Text> _powerCounts = new Dictionary<PowerUpType, Text>();
        private int _shownScore;

        protected override void Build()
        {
            // The HUD must not block touches on the board, so the root has no raycast target.
            var pause = UIKit.IconButton(Root, SpriteFactory.PauseIcon(), ColorUtil.Purple, App.PauseGame, 110);
            pause.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(80, -70), new Vector2(110, 110));

            _score = UIKit.Text(Root, "0", 88, Color.white);
            _score.rectTransform.At(UIKit.Top, new Vector2(0, -60), new Vector2(600, 110));
            _sub = UIKit.Text(Root, "", 36, Color.white).Fit(20);
            _sub.rectTransform.At(UIKit.Top, new Vector2(0, -132), new Vector2(700, 50));

            _coins = UIKit.CurrencyPill(Root, SpriteFactory.Coin(), ColorUtil.Ink, out var pill, 230);
            pill.At(UIKit.TopRight, new Vector2(-135, -70), pill.sizeDelta);

            _holdLabel = UIKit.Text(Root, Loc.T("hud.hold"), 34, Color.white).Fit(20);
            _holdLabel.rectTransform.sizeDelta = new Vector2(200, 50);
            _nextLabel = UIKit.Text(Root, Loc.T("hud.next"), 34, Color.white).Fit(20);
            _nextLabel.rectTransform.sizeDelta = new Vector2(200, 50);

            _slowMo = UIKit.Text(Root, "", 46, ColorUtil.Blue);
            _slowMo.rectTransform.At(UIKit.Center, new Vector2(0, 260), new Vector2(600, 80));
            _junk = UIKit.Text(Root, "", 46, ColorUtil.Red);
            _junk.rectTransform.At(UIKit.Center, new Vector2(0, 180), new Vector2(600, 80));

            // Power-up bar
            var bar = UIKit.Node("PowerUps", Root).At(UIKit.Bottom, new Vector2(0, 110), new Vector2(1000, 190));
            var types = new[] { PowerUpType.Bomb, PowerUpType.LineZap, PowerUpType.SlowMo, PowerUpType.Swap };
            var colors = new[] { ColorUtil.Purple, ColorUtil.Yellow, ColorUtil.Blue, ColorUtil.Mint };
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                var b = UIKit.Button(bar, null, colors[i], () => UsePowerUp(type), new Vector2(170, 170), 40, SpriteFactory.PowerUpIcon(type));
                b.GetComponent<RectTransform>().At(UIKit.Center, new Vector2(-300 + i * 200, 0), new Vector2(170, 170));
                var countBg = UIKit.Image(b.transform, SpriteFactory.Circle(), ColorUtil.Night, false, "Count");
                countBg.rectTransform.At(new Vector2(1f, 0f), new Vector2(-14, 24), new Vector2(66, 66));
                var count = UIKit.Text(countBg.transform, "0", 38, ColorUtil.Ink, TextAnchor.MiddleCenter, false);
                count.rectTransform.Fill();
                _powerButtons[type] = b;
                _powerCounts[type] = count;
            }
            _powerUpsLeft = UIKit.Text(Root, "", 30, Color.white).Fit(18);
            _powerUpsLeft.rectTransform.At(UIKit.Bottom, new Vector2(0, 222), new Vector2(700, 44));

            _floatingLayer = UIKit.Node("Floating", Root).Fill();
        }

        public void OnGameStarted()
        {
            _shownScore = 0;
            _score.text = "0";
            UIKit.ClearChildren(_floatingLayer);
            Refresh();
        }

        private void UsePowerUp(PowerUpType type)
        {
            if (!App.Game.TryUsePowerUp(type, App.Economy))
            {
                if (App.Economy.PowerUpCount(type) <= 0)
                {
                    App.Game.Pause();
                    App.Popup.ShowMessage(Loc.T("hud.out_title"),
                        Loc.T("hud.out_body"),
                        SpriteFactory.PowerUpIcon(type), App.Game.Resume);
                }
                else
                {
                    App.Audio.Play(Audio.Sfx.Error);
                    UIKit.Punch(_powerButtons[type].transform, 0.1f);
                }
                return;
            }
            UIKit.Punch(_powerButtons[type].transform, 0.2f);
            Refresh();
        }

        public override void Refresh()
        {
            var game = App.Game;
            var s = game.Session;
            if (s == null) return;

            var difficulty = DifficultySettings.For(s.Difficulty);
            string tag = $"<color={difficulty.Color}>{Loc.DifficultyName(s.Difficulty)}</color>   ";
            if (s.Mode == GameMode.Sprint)
            {
                int left = Mathf.Max(0, s.Config.SprintLines - s.Lines);
                _sub.text = tag + Loc.T("hud.lines_left", left);
            }
            else
            {
                _sub.text = tag + Loc.T("hud.level_lines", s.Level, s.Lines);
            }
            _coins.text = s.CoinsCollected.ToString();

            foreach (var kv in _powerButtons)
            {
                int owned = App.Economy.PowerUpCount(kv.Key);
                _powerCounts[kv.Key].text = owned.ToString();
                kv.Value.Interactable = owned > 0 && s.CanUsePowerUp(kv.Key);
            }
            if (s.Config.MaxPowerUpsPerGame <= 0) _powerUpsLeft.text = Loc.T("hud.no_powerups_on", Loc.DifficultyName(s.Difficulty));
            else _powerUpsLeft.text = s.PowerUpsLeft > 0 ? Loc.T("hud.powerups_left", s.PowerUpsLeft) : Loc.T("hud.powerups_none");
        }

        private void Update()
        {
            var game = App.Game;
            var s = game.Session;
            if (s == null) return;

            // Score ticks up smoothly; sprint shows a timer instead.
            if (s.Mode == GameMode.Sprint)
            {
                _score.text = UIKit.FormatTime(s.ElapsedSeconds);
            }
            else if (_shownScore != s.Score)
            {
                int diff = s.Score - _shownScore;
                _shownScore += Mathf.Max(1, Mathf.CeilToInt(diff * Mathf.Min(1f, Time.unscaledDeltaTime * 10f)));
                if (_shownScore > s.Score) _shownScore = s.Score;
                _score.text = Loc.Number(_shownScore);
            }

            _slowMo.text = s.SlowMoRemaining > 0f ? Loc.T("hud.slowmo", Mathf.CeilToInt(s.SlowMoRemaining)) : "";
            float junk = s.GarbageCountdown;
            _junk.text = junk >= 0f && junk <= 3f && !s.IsOver ? Loc.T("hud.junk_in", Mathf.CeilToInt(junk)) : "";

            // Keep labels glued above the world-space preview boxes.
            PlaceAtWorld(_holdLabel.rectTransform, game.HoldSlot.transform.position + new Vector3(0, 1.55f, 0));
            PlaceAtWorld(_nextLabel.rectTransform, game.NextSlots[0].transform.position + new Vector3(0.9f, 1.55f, 0));
        }

        private void PlaceAtWorld(RectTransform rt, Vector3 world)
        {
            var screen = App.Camera.WorldToScreenPoint(world);
            var parent = (RectTransform)rt.parent;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, null, out var local))
            {
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = local;
            }
        }

        /// <summary>Pops a bouncy text at a world position that rises and fades.</summary>
        public void SpawnFloatingText(string text, Vector3 world, Color color, float scale)
        {
            if (!IsVisible) return;
            var t = UIKit.Text(_floatingLayer, text, Mathf.RoundToInt(64 * scale), color);
            t.rectTransform.sizeDelta = new Vector2(1000, 300);
            PlaceAtWorld(t.rectTransform, world);
            var rt = t.rectTransform;
            var start = rt.anchoredPosition;
            rt.localScale = Vector3.zero;
            Tweener.Run(0.35f, k => { if (rt != null) rt.localScale = Vector3.one * k; }, Ease.OutBack, owner: t);
            Tweener.Run(1.1f, k =>
            {
                if (rt == null) return;
                rt.anchoredPosition = start + new Vector2(0, 160f * k);
                var c = t.color;
                c.a = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
                t.color = c;
            }, Ease.OutQuad, () => { if (t != null) Destroy(t.gameObject); }, 0.15f, t);
        }

        public override bool OnBack()
        {
            App.PauseGame();
            return true;
        }
    }
}
