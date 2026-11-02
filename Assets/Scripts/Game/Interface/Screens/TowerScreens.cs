using System.Collections.Generic;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.Tower;
using JellyDrop.GameInput;
using JellyDrop.Services;
using JellyDrop.Util;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>Colours for each tower look.</summary>
    internal static class TowerLook
    {
        public struct Palette
        {
            public Color Top, Bottom, Wall, Platform, Decor;
        }

        public static Palette Get(TowerScenery s)
        {
            switch (s)
            {
                case TowerScenery.Candy: return new Palette { Top = ColorUtil.Hex("#4A1F5E"), Bottom = ColorUtil.Hex("#E86FA8"), Wall = ColorUtil.Hex("#FFB3CF"), Platform = ColorUtil.Hex("#FF6F91"), Decor = ColorUtil.Hex("#FFE1EC") };
                case TowerScenery.Jungle: return new Palette { Top = ColorUtil.Hex("#0B2E22"), Bottom = ColorUtil.Hex("#2F8F55"), Wall = ColorUtil.Hex("#6B4A2A"), Platform = ColorUtil.Hex("#8BD15C"), Decor = ColorUtil.Hex("#C8F59A") };
                case TowerScenery.Space: return new Palette { Top = ColorUtil.Hex("#05061A"), Bottom = ColorUtil.Hex("#2A1A5E"), Wall = ColorUtil.Hex("#3B3F7A"), Platform = ColorUtil.Hex("#8E7CFF"), Decor = Color.white };
                case TowerScenery.Lava: return new Palette { Top = ColorUtil.Hex("#220808"), Bottom = ColorUtil.Hex("#A8361A"), Wall = ColorUtil.Hex("#4A2626"), Platform = ColorUtil.Hex("#FF9F43"), Decor = ColorUtil.Hex("#FFC93C") };
                default: return new Palette { Top = ColorUtil.Hex("#14305E"), Bottom = ColorUtil.Hex("#5FA8DE"), Wall = ColorUtil.Hex("#BFE6FF"), Platform = ColorUtil.Hex("#E8F6FF"), Decor = Color.white };
            }
        }

        public static Color KindColor(PlatformKind kind, Palette pal)
        {
            switch (kind)
            {
                case PlatformKind.Ice: return ColorUtil.Hex("#9FE8FF");
                case PlatformKind.Moving: return ColorUtil.Blue;
                case PlatformKind.Crumble: return ColorUtil.Hex("#B07A4A");
                case PlatformKind.Spring: return ColorUtil.Yellow;
                case PlatformKind.Goal: return ColorUtil.Gold;
                default: return pal.Platform;
            }
        }
    }

    /// <summary>A button that reports while it is held down (for running left / right and jumping).</summary>
    public sealed class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
    {
        public bool Held { get; private set; }
        private bool _down;
        private Image _image;
        private Color _color;

        private void Awake()
        {
            _image = GetComponent<Image>();
            if (_image != null) _color = _image.color;
        }

        public void OnPointerDown(PointerEventData e) { _down = true; Set(true); }
        public void OnPointerUp(PointerEventData e) { _down = false; Set(false); }
        public void OnPointerExit(PointerEventData e) => Set(false);
        public void OnPointerEnter(PointerEventData e) { if (_down) Set(true); }
        private void OnDisable() { _down = false; Held = false; }

        private void Set(bool held)
        {
            Held = held;
            if (_image != null) _image.color = held ? Color.Lerp(_color, Color.white, 0.35f) : _color;
            transform.localScale = Vector3.one * (held ? 0.94f : 1f);
        }
    }

    /// <summary>
    /// Pick a tower climb: the endless tower in three modes (with a choice of scenery) or one of the towers
    /// with a floor limit and obstacles, unlocked one after another.
    /// </summary>
    public sealed class TowerMenuScreen : UIScreen
    {
        private Text _coins;
        private readonly List<CandyButton> _sceneryChips = new List<CandyButton>();
        private readonly Text[] _modeBest = new Text[3];
        private RectTransform _list;

        protected override void Build()
        {
            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.OpenArcade, 120);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(84, -84), new Vector2(120, 120));
            var title = UIKit.Text(Root, Loc.T("tower.title"), 88, ColorUtil.Blue).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(-30, -84), new Vector2(520, 120));
            _coins = UIKit.CurrencyPill(Root, SpriteFactory.Coin(), ColorUtil.Ink, out var pill, 260);
            pill.At(UIKit.TopRight, new Vector2(-150, -84), pill.sizeDelta);

            // Endless
            var endless = UIKit.Text(Root, Loc.T("tower.endless"), 50, ColorUtil.Ink).Fit(26);
            endless.rectTransform.At(UIKit.Top, new Vector2(0, -200), new Vector2(900, 70));
            for (int i = 0; i < 5; i++)
            {
                var sc = (TowerScenery)i;
                var pal = TowerLook.Get(sc);
                var chip = UIKit.Button(Root, Loc.T("tower.scenery." + sc), pal.Bottom, () => PickScenery(sc), new Vector2(196, 80), 26);
                chip.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(-412 + i * 206, -290), new Vector2(196, 80));
                _sceneryChips.Add(chip);
            }
            Color[] modeColors = { ColorUtil.Pink, ColorUtil.Orange, ColorUtil.Blue };
            for (int i = 0; i < 3; i++)
            {
                var mode = (TowerMode)i;
                var b = UIKit.Button(Root, Loc.T("tower.mode." + mode), modeColors[i], () => App.StartTowerEndless(mode), new Vector2(320, 140), 40, SpriteFactory.PlayIcon());
                b.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(-340 + i * 340, -420), new Vector2(320, 140));
                _modeBest[i] = UIKit.Text(Root, "", 28, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(16);
                _modeBest[i].rectTransform.At(UIKit.Top, new Vector2(-340 + i * 340, -515), new Vector2(320, 44));
            }
            var modeInfo = UIKit.Text(Root, Loc.T("tower.modes_info"), 28, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(16);
            modeInfo.rectTransform.At(UIKit.Top, new Vector2(0, -570), new Vector2(980, 60));

            // Towers with a floor limit
            var towers = UIKit.Text(Root, Loc.T("tower.towers"), 50, ColorUtil.Ink).Fit(26);
            towers.rectTransform.At(UIKit.Top, new Vector2(0, -660), new Vector2(900, 70));
            var viewport = UIKit.Panel(Root, ColorUtil.Glass, 0.8f, true, "Viewport");
            viewport.rectTransform.Fill(30, 60, 30, 720);
            viewport.gameObject.AddComponent<RectMask2D>();
            _list = UIKit.Node("Content", viewport.transform);
            _list.anchorMin = new Vector2(0, 1);
            _list.anchorMax = new Vector2(1, 1);
            _list.pivot = new Vector2(0.5f, 1f);
            _list.offsetMin = _list.offsetMax = Vector2.zero;
            var grid = _list.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(480, 300);
            grid.spacing = new Vector2(20, 20);
            grid.padding = new RectOffset(15, 15, 20, 30);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            _list.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport.rectTransform;
            scroll.content = _list;
            scroll.horizontal = false;
        }

        protected override void OnShow() => Rebuild();

        public override void Refresh()
        {
            _coins.text = Loc.Number(App.Economy.Coins);
            var t = App.Tower;
            for (int i = 0; i < 5; i++)
                _sceneryChips[i].transform.localScale = Vector3.one * ((int)t.Scenery == i ? 1.08f : 0.9f);
            for (int i = 0; i < 3; i++) _modeBest[i].text = Loc.T("tower.best", t.Best((TowerMode)i));
        }

        private void PickScenery(TowerScenery s)
        {
            App.Tower.Scenery = s;
            App.Audio.Play(Sfx.Pop);
            Refresh();
        }

        private void Rebuild()
        {
            UIKit.ClearChildren(_list);
            var t = App.Tower;
            for (int i = 0; i < TowerCatalog.Count; i++) TowerCard(TowerCatalog.Get(i), t.IsUnlocked(i), t.IsCleared(i));
            _list.anchoredPosition = Vector2.zero;
            Refresh();
        }

        private void TowerCard(TowerDef def, bool unlocked, bool cleared)
        {
            var pal = TowerLook.Get(def.Scenery);
            var card = UIKit.NeonCard(_list, cleared ? ColorUtil.Mint : unlocked ? pal.Bottom : ColorUtil.Muted, ColorUtil.PanelLight, 1.2f, false, "Tower", 0.18f);
            var rt = card.rectTransform;
            var name = UIKit.Text(rt, Loc.T("tower.name", def.Index + 1, Loc.T("tower.scenery." + def.Scenery)), 34, unlocked ? ColorUtil.Ink : ColorUtil.InkDim).Fit(18);
            name.rectTransform.At(UIKit.Top, new Vector2(0, -40), new Vector2(440, 50));
            var floors = UIKit.Text(rt, Loc.T("tower.floors", def.Floors), 30, ColorUtil.Gold).Fit(16);
            floors.rectTransform.At(UIKit.Top, new Vector2(0, -88), new Vector2(440, 44));
            var hz = UIKit.Text(rt, HazardText(def), 24, ColorUtil.InkDim, TextAnchor.MiddleCenter, true).Fit(14);
            hz.rectTransform.At(UIKit.Top, new Vector2(0, -140), new Vector2(440, 60));
            CandyButton b;
            if (!unlocked)
            {
                b = UIKit.Button(rt, Loc.T("tower.locked"), ColorUtil.Muted, null, new Vector2(400, 90), 30, SpriteFactory.LockIcon());
                b.Interactable = false;
            }
            else
            {
                int idx = def.Index;
                b = UIKit.Button(rt, cleared ? Loc.T("tower.replay", def.ReplayReward) : Loc.T("tower.climb", def.Reward),
                    cleared ? ColorUtil.Mint : ColorUtil.Orange, () => App.StartTower(idx), new Vector2(400, 90), 30, SpriteFactory.Coin());
            }
            b.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 62), new Vector2(400, 90));
        }

        private static string HazardText(TowerDef def)
        {
            if (string.IsNullOrEmpty(def.Hazards)) return Loc.T("tower.hz.none");
            var parts = new List<string>();
            foreach (var h in def.Hazards.Split(' ')) parts.Add(Loc.T("tower.hz." + h));
            return string.Join(" · ", parts);
        }

        public override bool OnBack()
        {
            App.OpenArcade();
            return true;
        }
    }

    /// <summary>
    /// The tower climb itself, drawn with UI images: walls, platforms, the avatar, a combo meter and the
    /// controls (hold left / right, hold jump). Arrow keys and space work in the editor.
    /// </summary>
    public sealed class TowerScreen : UIScreen
    {
        private const float Wall = 40f;
        private const float BottomUi = 260f;

        private RectTransform _field;
        private Image _bgTop, _bgBottom, _wallL, _wallR;
        private RectTransform _avatar;
        private Image _avatarImage;
        private readonly Dictionary<TowerPlatform, Image> _views = new Dictionary<TowerPlatform, Image>();
        private readonly Stack<Image> _pool = new Stack<Image>();
        private readonly List<RectTransform> _decor = new List<RectTransform>();
        private readonly List<Vector2> _decorPos = new List<Vector2>();
        private Text _floor, _score, _comboText, _hint;
        private Image _comboBar;
        private RectTransform _results;
        private Text _resultTitle, _resultCoins, _resultInfo;
        private HoldButton _left, _right, _jump;
        private CandyButton _pause;

        private TowerSession _session;
        private TowerMode _mode;
        private int _towerIndex = -1;
        private bool _paused, _finished;
        private TowerScenery _shownScenery = (TowerScenery)(-1);
        private float _spin, _squash, _hintTime, _comboHold;
        private readonly List<TowerPlatform> _remove = new List<TowerPlatform>();

        private float Scale => (_field.rect.width - 2 * Wall) / TowerSession.Width;

        protected override void Build()
        {
            _field = UIKit.Node("Field", Root);
            _field.Fill();
            _bgBottom = UIKit.Image(_field, null, Color.white, false, "Bg");
            _bgBottom.rectTransform.Fill();
            _bgTop = UIKit.Image(_field, SpriteFactory.Gradient(Color.white, new Color(1, 1, 1, 0)), Color.white, false, "BgTop");
            _bgTop.rectTransform.Fill();
            for (int i = 0; i < 18; i++)
            {
                var d = UIKit.Image(_field, i % 3 == 0 ? SpriteFactory.Sparkle() : SpriteFactory.SoftDot(), Color.white, false, "Decor");
                float size = 18 + (i * 37) % 30;
                d.rectTransform.anchorMin = d.rectTransform.anchorMax = Vector2.zero;
                d.rectTransform.sizeDelta = new Vector2(size, size);
                _decor.Add(d.rectTransform);
                _decorPos.Add(new Vector2((i * 0.379f) % 1f, (i * 0.617f) % 1f));
            }
            _wallL = UIKit.Image(_field, SpriteFactory.RoundedRect(), Color.white, false, "WallL");
            _wallL.rectTransform.anchorMin = new Vector2(0, 0);
            _wallL.rectTransform.anchorMax = new Vector2(0, 1);
            _wallL.rectTransform.pivot = new Vector2(0, 0.5f);
            _wallL.rectTransform.offsetMin = new Vector2(-20, -40);
            _wallL.rectTransform.offsetMax = new Vector2(Wall, 40);
            _wallR = UIKit.Image(_field, SpriteFactory.RoundedRect(), Color.white, false, "WallR");
            _wallR.rectTransform.anchorMin = new Vector2(1, 0);
            _wallR.rectTransform.anchorMax = new Vector2(1, 1);
            _wallR.rectTransform.pivot = new Vector2(1, 0.5f);
            _wallR.rectTransform.offsetMin = new Vector2(-Wall, -40);
            _wallR.rectTransform.offsetMax = new Vector2(20, 40);

            _avatarImage = UIKit.Image(_field, null, Color.white, false, "Avatar");
            _avatar = _avatarImage.rectTransform;
            _avatar.anchorMin = _avatar.anchorMax = Vector2.zero;
            _avatar.pivot = new Vector2(0.5f, 0.1f);
            _avatar.sizeDelta = new Vector2(120, 120);

            // HUD
            _pause = UIKit.IconButton(Root, SpriteFactory.PauseIcon(), ColorUtil.Purple, TogglePause, 110);
            _pause.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(80, -80), new Vector2(110, 110));
            _floor = UIKit.Text(Root, "", 72, Color.white).Fit(30);
            _floor.rectTransform.At(UIKit.Top, new Vector2(0, -72), new Vector2(520, 100));
            _score = UIKit.Text(Root, "", 36, ColorUtil.Gold, TextAnchor.MiddleRight, false).Fit(20);
            _score.rectTransform.At(UIKit.TopRight, new Vector2(-170, -80), new Vector2(300, 60));
            _comboText = UIKit.Text(Root, "", 54, ColorUtil.Yellow).Fit(26);
            _comboText.rectTransform.At(UIKit.Top, new Vector2(0, -170), new Vector2(700, 80));
            _comboBar = UIKit.ProgressBar(Root, ColorUtil.Yellow, new Vector2(420, 26));
            _comboBar.transform.parent.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(0, -225), new Vector2(420, 26));
            _hint = UIKit.Text(Root, Loc.T("tower.hint"), 40, Color.white, TextAnchor.MiddleCenter, true).Fit(22);
            _hint.rectTransform.At(UIKit.Center, new Vector2(0, 200), new Vector2(940, 220));

            // Controls
            _left = Control(SpriteFactory.Triangle(), new Vector2(130, 130), 90f);
            _right = Control(SpriteFactory.Triangle(), new Vector2(330, 130), -90f);
            _jump = Control(SpriteFactory.PlayIcon(), new Vector2(-180, 130), 90f, true);

            // Results card
            var card = UIKit.NeonCard(Root, ColorUtil.Gold, ColorUtil.Panel, 0.8f, true, "Results");
            card.rectTransform.At(UIKit.Center, new Vector2(0, 40), new Vector2(880, 900));
            _results = card.rectTransform;
            _resultTitle = UIKit.Text(_results, "", 76, ColorUtil.Gold).Fit(36);
            _resultTitle.rectTransform.At(UIKit.Top, new Vector2(0, -100), new Vector2(780, 120));
            var coin = UIKit.Image(_results, SpriteFactory.Coin(), Color.white);
            coin.rectTransform.At(UIKit.Top, new Vector2(-120, -280), new Vector2(130, 130));
            _resultCoins = UIKit.Text(_results, "", 96, ColorUtil.Ink, TextAnchor.MiddleLeft);
            _resultCoins.rectTransform.At(UIKit.Top, new Vector2(110, -280), new Vector2(320, 140));
            _resultInfo = UIKit.Text(_results, "", 38, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(22);
            _resultInfo.rectTransform.At(UIKit.Top, new Vector2(0, -450), new Vector2(780, 180));
            var again = UIKit.Button(_results, Loc.T("tower.again"), ColorUtil.Pink, Again, new Vector2(620, 150), 52, SpriteFactory.PlayIcon());
            again.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 250), new Vector2(620, 150));
            var back = UIKit.Button(_results, Loc.T("run.back"), ColorUtil.Purple, App.OpenTowerMenu, new Vector2(620, 130), 46);
            back.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 90), new Vector2(620, 130));
        }

        private HoldButton Control(Sprite icon, Vector2 pos, float rotation, bool right = false)
        {
            var bg = UIKit.Image(Root, SpriteFactory.Circle(), new Color(1f, 1f, 1f, 0.18f), true, "Control");
            bg.rectTransform.At(right ? new Vector2(1, 0) : new Vector2(0, 0), pos, new Vector2(right ? 230 : 180, right ? 230 : 180));
            var ic = UIKit.Image(bg.rectTransform, icon, new Color(1f, 1f, 1f, 0.9f), false, "Icon");
            ic.rectTransform.At(UIKit.Center, Vector2.zero, new Vector2(80, 80));
            ic.rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);
            return bg.gameObject.AddComponent<HoldButton>();
        }

        // ------------------------------------------------------------------ start / end

        public void BeginEndless(TowerMode mode, TowerScenery scenery)
        {
            _mode = mode;
            _towerIndex = -1;
            Begin(new TowerSession(TowerConfig.Endless(mode, scenery), System.Environment.TickCount));
        }

        public void BeginTower(int index)
        {
            _towerIndex = index;
            Begin(new TowerSession(TowerCatalog.Get(index).Config(), System.Environment.TickCount));
        }

        private void Begin(TowerSession session)
        {
            _session = session;
            _session.Landed += OnLanded;
            _session.ComboEnded += OnComboEnded;
            _session.Bounced += () => Haptics.Tap();
            _paused = false;
            _finished = false;
            _spin = 0f;
            _squash = 0f;
            _hintTime = 4f;
            _comboHold = 0f;
            _comboText.text = "";
            _hint.text = Loc.T("tower.hint");
            _results.gameObject.SetActive(false);
            _pause.gameObject.SetActive(true);
            foreach (var v in _views.Values) { v.gameObject.SetActive(false); _pool.Push(v); }
            _views.Clear();
            _shownScenery = (TowerScenery)(-1);
            _avatarImage.sprite = SpriteFactory.Avatar(App.Economy.EquippedAvatar, AvatarMood.Excited);
            Render();
        }

        private void Again()
        {
            if (_towerIndex >= 0) BeginTower(_towerIndex);
            else BeginEndless(_mode, App.Tower.Scenery);
        }

        private void Finish()
        {
            if (_finished) return;
            _finished = true;
            var s = _session;
            int coins;
            string info;
            if (_towerIndex >= 0)
            {
                coins = App.Tower.FinishTower(_towerIndex, s.IsWon, out bool first);
                _resultTitle.text = s.IsWon ? Loc.T("tower.won") : Loc.T("tower.fell");
                info = Loc.T("tower.result_tower", s.HighestFloor, s.Config.Floors) + "\n" +
                       (s.IsWon ? (first ? Loc.T("tower.unlocked_next") : "") : Loc.T("tower.try_again"));
            }
            else
            {
                coins = App.Tower.FinishEndless(_mode, s.HighestFloor, s.ComboScore, out bool record);
                _resultTitle.text = record ? Loc.T("tower.record") : Loc.T("tower.fell");
                info = Loc.T("tower.result", s.HighestFloor, s.BestCombo, s.Score) + "\n" + Loc.T("tower.best", App.Tower.Best(_mode));
            }
            App.Pet.OnGamePlayed();
            App.Save.Save(App.Economy.Data);
            _resultCoins.text = "+" + Loc.Number(coins);
            _resultInfo.text = info;
            _pause.gameObject.SetActive(false);
            _results.gameObject.SetActive(true);
            UIKit.Pop(_results, 0.8f, 0.4f);
            App.Audio.Play(s.IsWon ? Sfx.Win : Sfx.GameOver);
            Haptics.Medium();
        }

        private void OnLanded(int floor, int jumped)
        {
            _squash = 1f;
            if (jumped >= 2) App.Audio.Play(Sfx.Combo);
            else if (jumped >= 1) App.Audio.Play(Sfx.Pop);
            if (_session.IsMilestone(floor) && jumped > 0) App.Audio.Play(Sfx.LevelUp);
        }

        private void OnComboEnded(int combo)
        {
            if (combo < 4) return;
            _comboText.text = Loc.T("tower.combo_end", combo, combo * combo);
            _comboHold = 1.6f;
            UIKit.Punch(_comboText.transform, 0.2f);
            App.Audio.Play(Sfx.Clear3);
        }

        // ------------------------------------------------------------------ update

        private void Update()
        {
            if (_session == null) return;
            if (!_paused && !_finished)
            {
                float move = 0f;
                if (_left.Held || PointerInput.KeyHeld(GameKey.Left)) move -= 1f;
                if (_right.Held || PointerInput.KeyHeld(GameKey.Right)) move += 1f;
                bool jump = _jump.Held || PointerInput.KeyHeld(GameKey.HardDrop);
                int jumps = _session.Jumps;
                _session.Step(Time.deltaTime, move, jump);
                if (_session.Jumps != jumps) { App.Audio.Play(Sfx.Move); if (Mathf.Abs(_session.Vx) > 9f) _spin = 1f; }
                if (_session.IsOver || _session.IsWon) Finish();
            }
            Render();
        }

        private void Render()
        {
            var s = _session;
            float k = Scale;
            float cam = s.CameraY;

            var scenery = s.SceneryAt(s.HighestFloor);
            if (scenery != _shownScenery)
            {
                _shownScenery = scenery;
                var pal = TowerLook.Get(scenery);
                _bgBottom.color = pal.Bottom;
                _bgTop.color = pal.Top;
                _wallL.color = _wallR.color = pal.Wall;
                foreach (var d in _decor) d.GetComponent<Image>().color = PixelCanvas.WithAlpha(pal.Decor, 0.55f);
                foreach (var kv in _views) Paint(kv.Value, kv.Key, pal);
            }

            // Decorations drift with parallax.
            float w = _field.rect.width, h = _field.rect.height;
            for (int i = 0; i < _decor.Count; i++)
            {
                float y = Mathf.Repeat(_decorPos[i].y * h - cam * k * 0.3f, h);
                _decor[i].anchoredPosition = new Vector2(Wall + _decorPos[i].x * (w - 2 * Wall), y);
            }

            // Platforms
            var palette = TowerLook.Get(_shownScenery);
            _remove.Clear();
            foreach (var kv in _views) _remove.Add(kv.Key);
            foreach (var p in s.Platforms)
            {
                _remove.Remove(p);
                float y = (p.Floor * TowerSession.Gap - cam) * k + BottomUi;
                if (!_views.TryGetValue(p, out var view))
                {
                    view = _pool.Count > 0 ? _pool.Pop() : NewView();
                    view.gameObject.SetActive(true);
                    _views[p] = view;
                    Paint(view, p, palette);
                }
                var rt = view.rectTransform;
                rt.anchoredPosition = new Vector2(Wall + p.Left * k, y);
                rt.sizeDelta = new Vector2(p.Width * k, 34f);
                float alpha = p.Broken ? 0f : p.CrumbleTime >= 0f ? 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(Time.time * 30f)) : 1f;
                view.color = PixelCanvas.WithAlpha(view.color, alpha);
            }
            foreach (var p in _remove)
            {
                var v = _views[p];
                v.gameObject.SetActive(false);
                _pool.Push(v);
                _views.Remove(p);
            }

            // Avatar: squash on landing, spin on fast jumps.
            _avatar.anchoredPosition = new Vector2(Wall + s.X * k, (s.Y - cam) * k + BottomUi + 30f);
            _squash = Mathf.Max(0f, _squash - Time.deltaTime * 5f);
            float sq = 1f + Mathf.Sin(_squash * Mathf.PI) * 0.25f;
            _avatar.localScale = new Vector3(Mathf.Sign(s.Vx == 0 ? 1 : s.Vx) * sq, 1f / sq, 1f);
            if (_spin > 0f && !s.Grounded) { _spin -= Time.deltaTime * 1.4f; _avatar.localRotation = Quaternion.Euler(0, 0, -Mathf.Sign(s.Vx) * (1f - Mathf.Max(0f, _spin)) * 360f); }
            else { _spin = 0f; _avatar.localRotation = Quaternion.identity; }

            // HUD
            _floor.text = s.Config.Floors > 0 ? Loc.T("tower.floor_of", s.HighestFloor, s.Config.Floors) : Loc.T("tower.floor", s.HighestFloor);
            _score.text = Loc.Number(s.Score);
            if (s.Combo > 0)
            {
                _comboText.text = Loc.T("tower.combo", s.Combo);
                _comboBar.fillAmount = s.ComboTimer / TowerSession.ComboTime;
                _comboBar.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                _comboBar.transform.parent.gameObject.SetActive(false);
                _comboHold -= Time.deltaTime;
                if (_comboHold <= 0f) _comboText.text = "";
            }
            if (_hintTime > 0f)
            {
                _hintTime -= Time.unscaledDeltaTime;
                _hint.color = new Color(1f, 1f, 1f, Mathf.Clamp01(_hintTime));
            }
            else if (!_paused) _hint.color = new Color(1f, 1f, 1f, 0f);
        }

        private Image NewView()
        {
            var img = UIKit.Image(_field, SpriteFactory.RoundedRect(), Color.white, false, "Platform");
            img.type = Image.Type.Sliced;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0f, 1f);
            var label = UIKit.Text(rt, "", 26, ColorUtil.FaceInk);
            label.rectTransform.Fill();
            _avatar.SetAsLastSibling();
            return img;
        }

        private static void Paint(Image view, TowerPlatform p, TowerLook.Palette pal)
        {
            view.color = TowerLook.KindColor(p.Kind, pal);
            var label = view.GetComponentInChildren<Text>();
            if (label != null)
                label.text = p.Kind == PlatformKind.Goal ? Loc.T("tower.top") : p.Floor > 0 && p.Floor % 10 == 0 ? p.Floor.ToString() : "";
        }

        private void TogglePause()
        {
            if (_finished) return;
            _paused = !_paused;
            _hint.text = _paused ? Loc.T("run.paused") : Loc.T("tower.hint");
            _hint.color = Color.white;
            _hintTime = _paused ? 0f : 1f;
        }

        protected override void OnHide()
        {
            _session = null;
        }

        public override bool OnBack()
        {
            if (_finished) App.OpenTowerMenu();
            else TogglePause();
            return true;
        }
    }
}
