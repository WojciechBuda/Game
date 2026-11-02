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
                case TowerScenery.Forest: return new Palette { Top = ColorUtil.Hex("#0E2018"), Bottom = ColorUtil.Hex("#3E6B3A"), Wall = ColorUtil.Hex("#5A3E28"), Platform = ColorUtil.Hex("#8A5E3A"), Decor = ColorUtil.Hex("#E8F5B0") };
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
            for (int i = 0; i < TowerSession.SceneryCount; i++)
            {
                var sc = (TowerScenery)i;
                var pal = TowerLook.Get(sc);
                var chip = UIKit.Button(Root, Loc.T("tower.scenery." + sc), pal.Bottom, () => PickScenery(sc), new Vector2(160, 80), 24);
                chip.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(-420 + i * 168, -290), new Vector2(160, 80));
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
            for (int i = 0; i < _sceneryChips.Count; i++)
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
    /// The tower climb itself, drawn with UI images: a painted backdrop per scenery (scrolling with parallax),
    /// textured walls, styled platforms, items to grab, animated critters and the avatar.
    /// Touch: slide a finger left / right to run (like a joystick that follows the finger), double-tap to jump. Arrow keys and space work in the editor.
    /// </summary>
    public sealed class TowerScreen : UIScreen
    {
        private const float Wall = 44f;
        private const float BottomUi = 150f;
        private const float WallTileH = 88f;
        private const float PlatformH = 46f;
        private const float DoubleTapTime = 0.32f;

        private RectTransform _field;
        private Image _bgTop, _bgBottom;
        private readonly Image[] _backdrop = new Image[3];
        private readonly List<Image> _wallTiles = new List<Image>();
        private RectTransform _avatar;
        private Image _avatarImage;
        private readonly Dictionary<TowerPlatform, Image> _views = new Dictionary<TowerPlatform, Image>();
        private readonly Stack<Image> _pool = new Stack<Image>();
        private readonly Dictionary<TowerItem, Image> _itemViews = new Dictionary<TowerItem, Image>();
        private readonly Stack<Image> _itemPool = new Stack<Image>();
        private readonly List<Critter> _critters = new List<Critter>();
        private Text _floor, _score, _comboText, _hint, _loot;
        private Image _comboBar;
        private RectTransform _results;
        private Text _resultTitle, _resultCoins, _resultInfo;
        private CandyButton _pause;
        private RectTransform _stick;
        private RectTransform _stickKnob;

        private TowerSession _session;
        private TowerMode _mode;
        private int _towerIndex = -1;
        private bool _paused, _finished;
        private TowerScenery _shownScenery = (TowerScenery)(-1);
        private float _spin, _squash, _hintTime, _comboHold;
        private float _lastCam;
        private readonly List<TowerPlatform> _remove = new List<TowerPlatform>();
        private readonly List<TowerItem> _removeItems = new List<TowerItem>();

        // Touch state
        private bool _touching, _touchIgnored, _usedForJump;
        private float _anchorX, _touchX, _touchStart, _lastTapEnd = -10f, _jumpBuffer;
        private Vector2 _touchStartPos;
        private bool _lastWasTap;

        private sealed class Critter
        {
            public Image Image;
            public TowerArt.CritterKind Kind;
            public Vector2 Pos, Vel;
            public float Phase, Life;
        }

        private float Scale => (_field.rect.width - 2 * Wall) / TowerSession.Width;

        protected override void Build()
        {
            _field = UIKit.Node("Field", Root);
            _field.Fill();
            _bgBottom = UIKit.Image(_field, null, Color.white, false, "Bg");
            _bgBottom.rectTransform.Fill();
            _bgTop = UIKit.Image(_field, SpriteFactory.Gradient(Color.white, new Color(1, 1, 1, 0)), Color.white, false, "BgTop");
            _bgTop.rectTransform.Fill();
            for (int i = 0; i < _backdrop.Length; i++)
            {
                var b = UIKit.Image(_field, null, new Color(0.82f, 0.82f, 0.88f, 0.92f), false, "Backdrop");
                b.rectTransform.anchorMin = b.rectTransform.anchorMax = Vector2.zero;
                b.rectTransform.pivot = Vector2.zero;
                _backdrop[i] = b;
            }
            for (int i = 0; i < 14; i++)
            {
                var img = UIKit.Image(_field, null, Color.white, false, "Critter");
                img.rectTransform.anchorMin = img.rectTransform.anchorMax = Vector2.zero;
                _critters.Add(new Critter { Image = img, Life = -1f });
            }
            for (int side = 0; side < 2; side++)
                for (int i = 0; i < 26; i++)
                {
                    var t = UIKit.Image(_field, null, Color.white, false, "Wall");
                    var rt = t.rectTransform;
                    rt.anchorMin = rt.anchorMax = new Vector2(side, 0);
                    rt.pivot = new Vector2(side, 0);
                    rt.sizeDelta = new Vector2(Wall, WallTileH);
                    _wallTiles.Add(t);
                }

            _avatarImage = UIKit.Image(_field, null, Color.white, false, "Avatar");
            _avatar = _avatarImage.rectTransform;
            _avatar.anchorMin = _avatar.anchorMax = Vector2.zero;
            _avatar.pivot = new Vector2(0.5f, 0.1f);
            _avatar.sizeDelta = new Vector2(120, 120);

            // Floating joystick shown under the finger
            var stick = UIKit.Image(Root, SpriteFactory.Circle(), new Color(1f, 1f, 1f, 0.12f), false, "Stick");
            _stick = stick.rectTransform;
            _stick.anchorMin = _stick.anchorMax = Vector2.zero;
            _stick.sizeDelta = new Vector2(220, 220);
            var knob = UIKit.Image(_stick, SpriteFactory.Circle(), new Color(1f, 1f, 1f, 0.4f), false, "Knob");
            _stickKnob = knob.rectTransform;
            _stickKnob.At(UIKit.Center, Vector2.zero, new Vector2(90, 90));
            stick.gameObject.SetActive(false);

            // HUD
            _pause = UIKit.IconButton(Root, SpriteFactory.PauseIcon(), ColorUtil.Purple, TogglePause, 110);
            _pause.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(90, -80), new Vector2(110, 110));
            _floor = UIKit.Text(Root, "", 72, Color.white).Fit(30);
            _floor.rectTransform.At(UIKit.Top, new Vector2(0, -72), new Vector2(520, 100));
            _score = UIKit.Text(Root, "", 36, ColorUtil.Gold, TextAnchor.MiddleRight, true).Fit(20);
            _score.rectTransform.At(UIKit.TopRight, new Vector2(-180, -60), new Vector2(300, 56));
            _loot = UIKit.Text(Root, "", 30, Color.white, TextAnchor.MiddleRight, true).Fit(16);
            _loot.rectTransform.At(UIKit.TopRight, new Vector2(-180, -112), new Vector2(320, 48));
            _comboText = UIKit.Text(Root, "", 54, ColorUtil.Yellow).Fit(26);
            _comboText.rectTransform.At(UIKit.Top, new Vector2(0, -170), new Vector2(700, 80));
            _comboBar = UIKit.ProgressBar(Root, ColorUtil.Yellow, new Vector2(420, 26));
            _comboBar.transform.parent.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(0, -225), new Vector2(420, 26));
            _hint = UIKit.Text(Root, Loc.T("tower.hint"), 40, Color.white, TextAnchor.MiddleCenter, true).Fit(22);
            _hint.rectTransform.At(UIKit.Center, new Vector2(0, 200), new Vector2(940, 260));

            // Results card
            var card = UIKit.NeonCard(Root, ColorUtil.Gold, ColorUtil.Panel, 0.8f, true, "Results");
            card.rectTransform.At(UIKit.Center, new Vector2(0, 40), new Vector2(880, 960));
            _results = card.rectTransform;
            _resultTitle = UIKit.Text(_results, "", 76, ColorUtil.Gold).Fit(36);
            _resultTitle.rectTransform.At(UIKit.Top, new Vector2(0, -100), new Vector2(780, 120));
            var coin = UIKit.Image(_results, SpriteFactory.Coin(), Color.white);
            coin.rectTransform.At(UIKit.Top, new Vector2(-120, -270), new Vector2(130, 130));
            _resultCoins = UIKit.Text(_results, "", 96, ColorUtil.Ink, TextAnchor.MiddleLeft);
            _resultCoins.rectTransform.At(UIKit.Top, new Vector2(110, -270), new Vector2(320, 140));
            _resultInfo = UIKit.Text(_results, "", 36, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(20);
            _resultInfo.rectTransform.At(UIKit.Top, new Vector2(0, -470), new Vector2(780, 240));
            var again = UIKit.Button(_results, Loc.T("tower.again"), ColorUtil.Pink, Again, new Vector2(620, 150), 52, SpriteFactory.PlayIcon());
            again.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 250), new Vector2(620, 150));
            var back = UIKit.Button(_results, Loc.T("run.back"), ColorUtil.Purple, App.OpenTowerMenu, new Vector2(620, 130), 46);
            back.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 90), new Vector2(620, 130));
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
            _session.ItemCollected += OnItem;
            _paused = false;
            _finished = false;
            _spin = 0f;
            _squash = 0f;
            _hintTime = 5f;
            _comboHold = 0f;
            _comboText.text = "";
            _hint.text = Loc.T("tower.hint");
            _touching = false;
            _usedForJump = false;
            _lastWasTap = false;
            _jumpBuffer = 0f;
            _stick.gameObject.SetActive(false);
            _results.gameObject.SetActive(false);
            _pause.gameObject.SetActive(true);
            foreach (var v in _views.Values) { v.gameObject.SetActive(false); _pool.Push(v); }
            _views.Clear();
            foreach (var v in _itemViews.Values) { v.gameObject.SetActive(false); _itemPool.Push(v); }
            _itemViews.Clear();
            foreach (var c in _critters) { c.Life = -1f; c.Image.enabled = false; }
            _shownScenery = (TowerScenery)(-1);
            _lastCam = 0f;
            _avatarImage.sprite = SpriteFactory.Avatar(App.Economy.EquippedAvatar, AvatarMood.Excited);
            Render(0f);
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
            // Everything picked up on the way is kept, even after a fall.
            var got = s.Collected;
            App.Tower.PayItems(got);
            if (got[(int)TowerItemKind.Energy] > 0) App.Pet.AddEnergy(got[(int)TowerItemKind.Energy] * 15f);
            coins += got[(int)TowerItemKind.Coin];
            info += "\n" + LootText(got, true);
            App.Pet.OnGamePlayed();
            App.Save.Save(App.Economy.Data);
            _resultCoins.text = "+" + Loc.Number(coins);
            _resultInfo.text = info;
            _pause.gameObject.SetActive(false);
            _stick.gameObject.SetActive(false);
            _results.gameObject.SetActive(true);
            UIKit.Pop(_results, 0.8f, 0.4f);
            App.Audio.Play(s.IsWon ? Sfx.Win : Sfx.GameOver);
            Haptics.Medium();
        }

        private static string LootText(int[] got, bool long_)
        {
            var parts = new List<string>();
            if (long_ && got[(int)TowerItemKind.Coin] > 0) parts.Add(Loc.T("tower.loot.Coin", got[(int)TowerItemKind.Coin]));
            if (got[(int)TowerItemKind.Energy] > 0) parts.Add(Loc.T("tower.loot.Energy", got[(int)TowerItemKind.Energy]));
            if (got[(int)TowerItemKind.Wood] > 0) parts.Add(Loc.T("mission.supply", got[(int)TowerItemKind.Wood], Loc.T("supply.Wood")));
            if (got[(int)TowerItemKind.Brick] > 0) parts.Add(Loc.T("mission.supply", got[(int)TowerItemKind.Brick], Loc.T("supply.Brick")));
            if (got[(int)TowerItemKind.Glass] > 0) parts.Add(Loc.T("mission.supply", got[(int)TowerItemKind.Glass], Loc.T("supply.Glass")));
            return string.Join(" · ", parts);
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

        private void OnItem(TowerItem item)
        {
            App.Audio.Play(item.Kind == TowerItemKind.Coin ? Sfx.Coin : Sfx.PowerUp, 0.8f);
            if (_itemViews.TryGetValue(item, out var v))
            {
                // Fly up and fade.
                var img = v;
                _itemViews.Remove(item);
                var rt = img.rectTransform;
                var from = rt.anchoredPosition;
                Tweener.Run(0.35f, t =>
                {
                    rt.anchoredPosition = from + new Vector2(0, 90f * t);
                    rt.localScale = Vector3.one * (1f + 0.6f * t);
                    img.color = new Color(1, 1, 1, 1f - t);
                }, Ease.OutQuad, () => { img.color = Color.white; rt.localScale = Vector3.one; img.gameObject.SetActive(false); _itemPool.Push(img); }, 0f, img);
            }
            UIKit.Punch(_loot.transform, 0.12f);
        }

        // ------------------------------------------------------------------ input

        private void ReadInput(out float move, out bool jump)
        {
            move = 0f;
            float dt = Time.deltaTime;
            if (_jumpBuffer > 0f) _jumpBuffer -= dt;

            bool down = PointerInput.TryGetPointer(out var pos);
            float now = Time.unscaledTime;
            if (down && !_touching)
            {
                _touching = true;
                _touchIgnored = PointerInput.IsOverUi(pos);
                _touchStart = now;
                _touchStartPos = pos;
                _anchorX = pos.x;
                _touchX = pos.x;
                // Second tap soon after a first one: jump (on the press itself, so it feels instant).
                if (!_touchIgnored && _lastWasTap && now - _lastTapEnd < DoubleTapTime)
                {
                    _jumpBuffer = 0.3f;
                    _usedForJump = true;
                }
                if (!_touchIgnored)
                {
                    _stick.gameObject.SetActive(true);
                    _stick.anchoredPosition = ToCanvas(pos);
                }
            }
            else if (!down && _touching)
            {
                _touching = false;
                bool tap = now - _touchStart < 0.25f && (pos - _touchStartPos).magnitude < Screen.height * 0.02f;
                // A tap that was itself the second of a double tap doesn't start a new one.
                // A tap that finished a double tap doesn't count as the first tap of the next one,
                // so tap-tap, tap-tap gives one jump each time.
                _lastWasTap = tap && !_usedForJump;
                _lastTapEnd = now;
                _usedForJump = false;
                _stick.gameObject.SetActive(false);
            }

            if (_touching && !_touchIgnored)
            {
                _touchX = pos.x;
                float range = Screen.width * 0.09f;
                float dx = _touchX - _anchorX;
                // The joystick centre follows the finger when it goes past the edge (easy direction changes).
                if (dx > range) _anchorX = _touchX - range;
                if (dx < -range) _anchorX = _touchX + range;
                dx = _touchX - _anchorX;
                move = Mathf.Abs(dx) < range * 0.12f ? 0f : Mathf.Clamp(dx / range, -1f, 1f);
                _stick.anchoredPosition = ToCanvas(new Vector2(_anchorX, _touchStartPos.y));
                _stickKnob.anchoredPosition = new Vector2(move * 70f, 0f);
            }

            if (PointerInput.KeyHeld(GameKey.Left)) move = -1f;
            if (PointerInput.KeyHeld(GameKey.Right)) move = 1f;
            jump = _jumpBuffer > 0f || PointerInput.KeyHeld(GameKey.HardDrop);
        }

        private Vector2 ToCanvas(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Root, screen, null, out var local);
            var r = ((RectTransform)Root).rect;
            return local - r.min;
        }

        // ------------------------------------------------------------------ update

        private void Update()
        {
            if (_session == null) return;
            float dt = Time.deltaTime;
            if (!_paused && !_finished)
            {
                ReadInput(out float move, out bool jump);
                int jumps = _session.Jumps;
                _session.Step(dt, move, jump);
                if (_session.Jumps != jumps)
                {
                    _jumpBuffer = 0f;
                    App.Audio.Play(Sfx.Move);
                    if (Mathf.Abs(_session.Vx) > 9f) _spin = 1f;
                }
                if (_session.IsOver || _session.IsWon) Finish();
            }
            Render(_paused ? 0f : dt);
        }

        private void Render(float dt)
        {
            var s = _session;
            float k = Scale;
            float cam = s.CameraY;
            float w = _field.rect.width, h = _field.rect.height;

            var scenery = s.SceneryAt(s.HighestFloor);
            if (scenery != _shownScenery) ApplyScenery(scenery);

            // Backdrop (parallax) and walls (scroll with the tower)
            float bh = w * TowerArt.BackH / (float)TowerArt.BackW;
            float off = Mathf.Repeat(cam * k * 0.35f, bh);
            for (int i = 0; i < _backdrop.Length; i++)
            {
                var rt = _backdrop[i].rectTransform;
                rt.sizeDelta = new Vector2(w, bh + 1f);
                rt.anchoredPosition = new Vector2(0, -off + i * bh);
            }
            float woff = Mathf.Repeat(cam * k, WallTileH);
            int perSide = _wallTiles.Count / 2;
            for (int i = 0; i < _wallTiles.Count; i++)
            {
                int row = i % perSide;
                _wallTiles[i].rectTransform.anchoredPosition = new Vector2(0, -woff + row * WallTileH);
            }

            // Platforms
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
                    Style(view, p);
                }
                var rt = view.rectTransform;
                rt.anchoredPosition = new Vector2(Wall + p.Left * k, y + 6f);
                rt.sizeDelta = new Vector2(p.Width * k, PlatformH);
                float alpha = p.Broken ? 0f : p.CrumbleTime >= 0f ? 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(Time.time * 30f)) : 1f;
                view.color = new Color(1f, 1f, 1f, alpha);
            }
            foreach (var p in _remove)
            {
                var v = _views[p];
                v.gameObject.SetActive(false);
                _pool.Push(v);
                _views.Remove(p);
            }

            // Items (bob gently)
            _removeItems.Clear();
            foreach (var kv in _itemViews) _removeItems.Add(kv.Key);
            foreach (var it in s.Items)
            {
                if (it.Taken) continue;
                _removeItems.Remove(it);
                if (!_itemViews.TryGetValue(it, out var img))
                {
                    img = _itemPool.Count > 0 ? _itemPool.Pop() : NewItemView();
                    img.gameObject.SetActive(true);
                    img.sprite = ItemSprite(it.Kind);
                    img.color = Color.white;
                    _itemViews[it] = img;
                }
                float bob = Mathf.Sin(Time.time * 3f + it.X) * 6f;
                img.rectTransform.anchoredPosition = new Vector2(Wall + it.X * k, (it.Y - cam) * k + BottomUi + bob);
                img.rectTransform.localRotation = it.Kind == TowerItemKind.Coin ? Quaternion.identity : Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 2f + it.Y) * 10f);
            }
            foreach (var it in _removeItems)
            {
                var v = _itemViews[it];
                v.gameObject.SetActive(false);
                _itemPool.Push(v);
                _itemViews.Remove(it);
            }

            UpdateCritters(dt, (cam - _lastCam) * k, w, h);
            _lastCam = cam;

            // Avatar: squash on landing, spin on fast jumps.
            _avatar.anchoredPosition = new Vector2(Wall + s.X * k, (s.Y - cam) * k + BottomUi + 6f + PlatformH - 10f);
            _squash = Mathf.Max(0f, _squash - Time.deltaTime * 5f);
            float sq = 1f + Mathf.Sin(_squash * Mathf.PI) * 0.25f;
            _avatar.localScale = new Vector3(Mathf.Sign(s.Vx == 0 ? 1 : s.Vx) * sq, 1f / sq, 1f);
            if (_spin > 0f && !s.Grounded) { _spin -= Time.deltaTime * 1.4f; _avatar.localRotation = Quaternion.Euler(0, 0, -Mathf.Sign(s.Vx) * (1f - Mathf.Max(0f, _spin)) * 360f); }
            else { _spin = 0f; _avatar.localRotation = Quaternion.identity; }

            // HUD
            _floor.text = s.Config.Floors > 0 ? Loc.T("tower.floor_of", s.HighestFloor, s.Config.Floors) : Loc.T("tower.floor", s.HighestFloor);
            _score.text = Loc.Number(s.Score);
            _loot.text = Loc.T("tower.loot.Coin", s.Collected[(int)TowerItemKind.Coin]) + "  " + LootText(s.Collected, false);
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

        private void ApplyScenery(TowerScenery scenery)
        {
            _shownScenery = scenery;
            var pal = TowerLook.Get(scenery);
            _bgBottom.color = pal.Bottom;
            _bgTop.color = pal.Top;
            var back = TowerArt.Backdrop(scenery);
            foreach (var b in _backdrop) b.sprite = back;
            var wall = TowerArt.Wall(scenery);
            foreach (var t in _wallTiles) t.sprite = wall;
            foreach (var kv in _views) Style(kv.Value, kv.Key);
            var kinds = TowerArt.CrittersFor(scenery);
            for (int i = 0; i < _critters.Count; i++)
            {
                _critters[i].Kind = kinds[i % kinds.Length];
                _critters[i].Life = -1f - i * 0.6f; // stagger their first appearance
                _critters[i].Image.enabled = false;
            }
        }

        // ------------------------------------------------------------------ critters

        private void UpdateCritters(float dt, float scrolled, float w, float h)
        {
            foreach (var c in _critters)
            {
                var img = c.Image;
                if (c.Life < 0f)
                {
                    c.Life += dt;
                    if (c.Life >= 0f) Spawn(c, w, h);
                    continue;
                }
                c.Life += dt;
                c.Phase += dt;
                c.Pos += c.Vel * dt;
                c.Pos.y -= scrolled * 0.6f; // drift with the tower a bit
                var rt = img.rectTransform;
                switch (c.Kind)
                {
                    case TowerArt.CritterKind.Bird:
                    case TowerArt.CritterKind.Butterfly:
                    {
                        float flap = c.Kind == TowerArt.CritterKind.Bird ? 0.16f : 0.1f;
                        img.sprite = TowerArt.Critter(c.Kind, (int)(c.Phase / flap) % 2);
                        if (c.Kind == TowerArt.CritterKind.Butterfly) c.Vel.y = Mathf.Sin(c.Phase * 3f) * 60f;
                        else c.Pos.y += Mathf.Sin(c.Phase * 2f) * 20f * dt;
                        rt.localScale = new Vector3(c.Vel.x < 0 ? -1f : 1f, 1f, 1f);
                        break;
                    }
                    case TowerArt.CritterKind.Leaf:
                    case TowerArt.CritterKind.Snow:
                    case TowerArt.CritterKind.Sprinkle:
                        c.Pos.x += Mathf.Sin(c.Phase * 1.7f) * 40f * dt;
                        rt.localRotation = Quaternion.Euler(0, 0, c.Phase * 90f);
                        break;
                    case TowerArt.CritterKind.Firefly:
                        c.Vel += new Vector2(Mathf.Sin(c.Phase * 1.3f), Mathf.Cos(c.Phase * 1.1f)) * 30f * dt;
                        img.color = PixelCanvas.WithAlpha(img.color, 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(c.Phase * 2.5f)));
                        break;
                    case TowerArt.CritterKind.Ember:
                    case TowerArt.CritterKind.Bubble:
                        c.Pos.x += Mathf.Sin(c.Phase * 2f) * 25f * dt;
                        if (c.Kind == TowerArt.CritterKind.Ember) img.color = PixelCanvas.WithAlpha(img.color, Mathf.Clamp01(1.5f - c.Life * 0.25f));
                        break;
                }
                rt.anchoredPosition = c.Pos;
                bool gone = c.Pos.x < -80 || c.Pos.x > w + 80 || c.Pos.y < -80 || c.Pos.y > h + 80 || c.Life > 14f;
                if (gone) { img.enabled = false; c.Life = -Random.Range(0.5f, 4f); }
            }
        }

        private void Spawn(Critter c, float w, float h)
        {
            var img = c.Image;
            img.enabled = true;
            img.sprite = TowerArt.Critter(c.Kind);
            img.color = Color.white;
            img.rectTransform.localRotation = Quaternion.identity;
            img.rectTransform.localScale = Vector3.one;
            float size = 46f;
            switch (c.Kind)
            {
                case TowerArt.CritterKind.Bird:
                {
                    bool left = Random.value < 0.5f;
                    c.Pos = new Vector2(left ? -60 : w + 60, Random.Range(h * 0.35f, h * 0.9f));
                    c.Vel = new Vector2((left ? 1 : -1) * Random.Range(130f, 220f), 0f);
                    size = Random.Range(44f, 64f);
                    break;
                }
                case TowerArt.CritterKind.Butterfly:
                {
                    bool left = Random.value < 0.5f;
                    c.Pos = new Vector2(left ? -40 : w + 40, Random.Range(h * 0.2f, h * 0.8f));
                    c.Vel = new Vector2((left ? 1 : -1) * Random.Range(60f, 100f), 0f);
                    size = 50f;
                    break;
                }
                case TowerArt.CritterKind.Leaf:
                    c.Pos = new Vector2(Random.Range(0, w), h + 40);
                    c.Vel = new Vector2(0, -Random.Range(60f, 110f));
                    img.sprite = TowerArt.Critter(c.Kind, Random.value < 0.5f ? 0 : 1);
                    size = Random.Range(30f, 44f);
                    break;
                case TowerArt.CritterKind.Snow:
                    c.Pos = new Vector2(Random.Range(0, w), h + 30);
                    c.Vel = new Vector2(0, -Random.Range(50f, 100f));
                    size = Random.Range(16f, 30f);
                    img.color = new Color(1, 1, 1, 0.85f);
                    break;
                case TowerArt.CritterKind.Sprinkle:
                    c.Pos = new Vector2(Random.Range(0, w), h + 30);
                    c.Vel = new Vector2(0, -Random.Range(70f, 120f));
                    size = 26f;
                    Color[] sc = { ColorUtil.Pink, ColorUtil.Yellow, ColorUtil.Blue, ColorUtil.Mint };
                    img.color = sc[Random.Range(0, sc.Length)];
                    break;
                case TowerArt.CritterKind.Firefly:
                    c.Pos = new Vector2(Random.Range(60, w - 60), Random.Range(h * 0.1f, h * 0.9f));
                    c.Vel = Random.insideUnitCircle * 25f;
                    size = Random.Range(20f, 34f);
                    img.color = _shownScenery == TowerScenery.Space ? new Color(0.85f, 0.9f, 1f) : new Color(0.85f, 1f, 0.45f);
                    break;
                case TowerArt.CritterKind.Ember:
                    c.Pos = new Vector2(Random.Range(0, w), -30);
                    c.Vel = new Vector2(0, Random.Range(80f, 150f));
                    size = Random.Range(16f, 30f);
                    img.color = new Color(1f, Random.Range(0.45f, 0.75f), 0.2f);
                    break;
                case TowerArt.CritterKind.Bubble:
                    c.Pos = new Vector2(Random.Range(0, w), -40);
                    c.Vel = new Vector2(0, Random.Range(50f, 90f));
                    size = Random.Range(24f, 48f);
                    img.color = _shownScenery == TowerScenery.Lava ? new Color(1f, 0.6f, 0.3f, 0.7f) : new Color(1f, 1f, 1f, 0.8f);
                    break;
                case TowerArt.CritterKind.ShootingStar:
                    c.Pos = new Vector2(Random.Range(w * 0.3f, w + 40), h + 40);
                    c.Vel = new Vector2(-Random.Range(500f, 800f), -Random.Range(400f, 600f));
                    size = 70f;
                    img.rectTransform.localRotation = Quaternion.Euler(0, 0, 180f);
                    break;
            }
            img.rectTransform.sizeDelta = new Vector2(size, size);
            c.Life = 0f;
            c.Phase = Random.Range(0f, 6f);
        }

        // ------------------------------------------------------------------ views

        private Image NewView()
        {
            var img = UIKit.Image(_field, null, Color.white, false, "Platform");
            img.type = Image.Type.Sliced;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0f, 1f);
            var mark = UIKit.Image(rt, null, Color.white, false, "Mark");
            mark.rectTransform.At(UIKit.Top, new Vector2(0, 0), new Vector2(64, 40));
            var label = UIKit.Text(rt, "", 26, Color.white, TextAnchor.MiddleCenter, true);
            label.rectTransform.Fill();
            _avatar.SetAsLastSibling();
            return img;
        }

        private Image NewItemView()
        {
            var img = UIKit.Image(_field, null, Color.white, false, "Item");
            img.rectTransform.anchorMin = img.rectTransform.anchorMax = Vector2.zero;
            img.rectTransform.sizeDelta = new Vector2(62, 62);
            _avatar.SetAsLastSibling();
            return img;
        }

        private static Sprite ItemSprite(TowerItemKind kind)
        {
            switch (kind)
            {
                case TowerItemKind.Energy: return PetSprites.Bolt();
                case TowerItemKind.Wood: return CitySprites.SupplyIcon(JellyDrop.Core.City.Supply.Wood);
                case TowerItemKind.Brick: return CitySprites.SupplyIcon(JellyDrop.Core.City.Supply.Brick);
                case TowerItemKind.Glass: return CitySprites.SupplyIcon(JellyDrop.Core.City.Supply.Glass);
                default: return SpriteFactory.Coin();
            }
        }

        private void Style(Image view, TowerPlatform p)
        {
            view.sprite = TowerArt.Platform(_shownScenery, p.Kind);
            var mark = view.transform.Find("Mark").GetComponent<Image>();
            var mrt = mark.rectTransform;
            switch (p.Kind)
            {
                case PlatformKind.Moving:
                    mark.enabled = true; mark.sprite = TowerArt.Chevrons();
                    mrt.At(UIKit.Center, new Vector2(0, -4), new Vector2(64, 32));
                    break;
                case PlatformKind.Spring:
                    mark.enabled = true; mark.sprite = TowerArt.Spring();
                    mrt.At(UIKit.Top, new Vector2(0, 30), new Vector2(64, 48));
                    break;
                case PlatformKind.Goal:
                    mark.enabled = true; mark.sprite = TowerArt.Flag();
                    mrt.At(UIKit.Top, new Vector2(120, 80), new Vector2(64, 96));
                    break;
                default:
                    mark.enabled = false;
                    break;
            }
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
            _touching = false;
            _stick.gameObject.SetActive(false);
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
