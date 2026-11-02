using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>
    /// HUD for the Block Run bonus: coins and distance while running, a short how-to at the start,
    /// and a results card with the coins earned when the run ends.
    /// </summary>
    public sealed class RunnerScreen : UIScreen
    {
        private Text _coins;
        private Text _distance;
        private Text _hint;
        private readonly Image[] _hearts = new Image[JellyDrop.Core.Runner.RunnerSession.StartLives];
        private RectTransform _results;
        private Text _resultCoins;
        private Text _resultInfo;
        private CandyButton _again;
        private CandyButton _pause;
        private float _hintTime;
        private bool _finished;

        protected override void Build()
        {
            _pause = UIKit.IconButton(Root, SpriteFactory.PauseIcon(), ColorUtil.Purple, TogglePause, 110);
            _pause.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(80, -80), new Vector2(110, 110));
            AddHomeButton(Root, UIKit.TopLeft, new Vector2(205, -80), () => !_finished, 110f, () => { if (!_pausedState) TogglePause(); }, () => { if (_pausedState) TogglePause(); });

            _coins = UIKit.CurrencyPill(Root, SpriteFactory.Coin(), ColorUtil.Ink, out var pill, 260);
            pill.At(UIKit.TopRight, new Vector2(-150, -80), pill.sizeDelta);
            _distance = UIKit.Text(Root, "0 m", 76, Color.white).Fit(36);
            _distance.rectTransform.At(UIKit.Top, new Vector2(0, -80), new Vector2(420, 110));
            for (int i = 0; i < _hearts.Length; i++)
            {
                _hearts[i] = UIKit.Image(Root, PetSprites.Heart(), Color.white, false, "Life");
                _hearts[i].rectTransform.At(UIKit.Top, new Vector2(-80 + i * 80, -180), new Vector2(70, 70));
            }

            _hint = UIKit.Text(Root, Loc.T("run.hint"), 40, Color.white, TextAnchor.MiddleCenter, true).Fit(22);
            _hint.rectTransform.At(UIKit.Center, new Vector2(0, 260), new Vector2(940, 200));

            // Results card (hidden until the run ends)
            var card = UIKit.NeonCard(Root, ColorUtil.Gold, ColorUtil.Panel, 0.8f, true, "Results");
            card.rectTransform.At(UIKit.Center, new Vector2(0, 40), new Vector2(880, 900));
            _results = card.rectTransform;
            var title = UIKit.Text(_results, Loc.T("run.over"), 80, ColorUtil.Gold).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -100), new Vector2(760, 120));
            var coin = UIKit.Image(_results, SpriteFactory.Coin(), Color.white);
            coin.rectTransform.At(UIKit.Top, new Vector2(-120, -280), new Vector2(130, 130));
            _resultCoins = UIKit.Text(_results, "", 96, ColorUtil.Ink, TextAnchor.MiddleLeft);
            _resultCoins.rectTransform.At(UIKit.Top, new Vector2(110, -280), new Vector2(320, 140));
            _resultInfo = UIKit.Text(_results, "", 38, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(22);
            _resultInfo.rectTransform.At(UIKit.Top, new Vector2(0, -440), new Vector2(780, 160));
            _again = UIKit.Button(_results, "", ColorUtil.Pink, () => App.StartRun(), new Vector2(620, 150), 52, SpriteFactory.PlayIcon());
            _again.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 250), new Vector2(620, 150));
            AddHomeButton(_results, UIKit.TopLeft, new Vector2(80, -80), null, 100);
            var back = UIKit.Button(_results, Loc.T("run.back"), ColorUtil.Purple, App.OpenArcade, new Vector2(620, 130), 46);
            back.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 90), new Vector2(620, 130));
        }

        protected override void OnShow() => PrepareRun();

        /// <summary>Resets the HUD for a new run (also used for "run again" while the screen stays open).</summary>
        public void PrepareRun()
        {
            _finished = false;
            _pausedState = false;
            _hint.text = Loc.T("run.hint");
            _hintTime = 3.5f;
            _results.gameObject.SetActive(false);
            _pause.gameObject.SetActive(true);
            App.RunnerWorld.RunEnded -= OnRunEnded;
            App.RunnerWorld.RunEnded += OnRunEnded;
            App.RunnerWorld.LivesChanged -= OnLives;
            App.RunnerWorld.LivesChanged += OnLives;
            ShowLives(JellyDrop.Core.Runner.RunnerSession.StartLives);
        }

        private void OnLives(int left)
        {
            ShowLives(left);
            if (left >= 0 && left < _hearts.Length) UIKit.Punch(_hearts[left].transform, 0.4f);
            _hint.text = Loc.T("run.life_lost", left);
            _hintTime = 1.4f;
        }

        private void ShowLives(int left)
        {
            for (int i = 0; i < _hearts.Length; i++)
                _hearts[i].color = i < left ? Color.white : new Color(1f, 1f, 1f, 0.18f);
        }

        protected override void OnHide()
        {
            App.RunnerWorld.RunEnded -= OnRunEnded;
            App.RunnerWorld.LivesChanged -= OnLives;
            App.RunnerWorld.End();
        }

        private void Update()
        {
            var s = App.RunnerWorld.Session;
            if (s == null) return;
            _coins.text = s.Coins.ToString();
            _distance.text = Mathf.FloorToInt(s.Distance) + " m";
            if (_hintTime > 0f)
            {
                _hintTime -= Time.unscaledDeltaTime;
                _hint.color = new Color(1f, 1f, 1f, Mathf.Clamp01(_hintTime));
            }
            else _hint.color = new Color(1f, 1f, 1f, 0f);
        }

        private void TogglePause()
        {
            if (_finished) return;
            var w = App.RunnerWorld;
            bool paused = !_pausedState;
            _pausedState = paused;
            w.SetPaused(paused);
            _hint.text = paused ? Loc.T("run.paused") : Loc.T("run.hint");
            _hintTime = paused ? 9999f : 0f;
        }

        private bool _pausedState;

        private void OnRunEnded()
        {
            var s = App.RunnerWorld.Session;
            if (s == null || _finished) return;
            _finished = true;
            _pausedState = false;
            ShowLives(0);
            int best = App.Economy.Data.runBest;
            int earned = App.Economy.ApplyRunResult(s.Coins, s.Distance, App.RunDoubled);
            App.Pet.OnGamePlayed();
            App.Save.Save(App.Economy.Data);
            bool record = (int)s.Distance > best;
            _resultCoins.text = "+" + Loc.Number(earned);
            _resultInfo.text = Loc.T("run.result", Mathf.FloorToInt(s.Distance), s.Coins) + "\n" +
                (record ? Loc.T("run.record") : Loc.T("run.best", App.Economy.Data.runBest));
            int tickets = App.Economy.RunTickets;
            _again.SetLabel(tickets > 0 ? Loc.T("run.again_bonus", tickets) : Loc.T("run.again_free"));
            _again.Interactable = true;
            if (App.RunDoubled) _resultInfo.text += "\n" + Loc.T("run.doubled");
            _pause.gameObject.SetActive(false);
            _results.gameObject.SetActive(true);
            UIKit.Pop(_results, 0.8f, 0.4f);
            App.Audio.Play(record ? Sfx.Win : Sfx.Coin);
        }

        public override bool OnBack()
        {
            if (_finished) App.OpenArcade();
            else TogglePause();
            return true;
        }
    }
}
