using System;
using System.Collections.Generic;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.Islands;
using JellyDrop.Services;
using JellyDrop.Util;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>A modal card with an icon, a text and a column of action buttons (building menus on the islands).</summary>
    public sealed class ActionPopup : UIScreen
    {
        public struct Option
        {
            public string Label;
            public Color Color;
            public Sprite Icon;
            public Action OnClick;
            public bool Enabled;

            public Option(string label, Color color, Action onClick, Sprite icon = null, bool enabled = true)
            {
                Label = label; Color = color; OnClick = onClick; Icon = icon; Enabled = enabled;
            }
        }

        private RectTransform _content;

        protected override void Build()
        {
            var card = BuildModalCard(new Vector2(900, 1300), ColorUtil.Panel, ColorUtil.Mint);
            _content = UIKit.Node("Content", card).Fill(40, 40, 40, 40);
        }

        public void Show(string title, string body, Sprite icon, params Option[] options)
        {
            UIKit.ClearChildren(_content);
            var close = UIKit.IconButton(_content, SpriteFactory.CloseIcon(), ColorUtil.Purple, Close, 90);
            close.GetComponent<RectTransform>().At(UIKit.TopRight, new Vector2(-10, -10), new Vector2(90, 90));
            var t = UIKit.Text(_content, title, 62, ColorUtil.Mint).Fit(30);
            t.rectTransform.At(UIKit.Top, new Vector2(0, -60), new Vector2(680, 100));
            if (icon != null)
            {
                var img = UIKit.Image(_content, icon, Color.white, false, "Icon");
                img.rectTransform.At(UIKit.Top, new Vector2(0, -230), new Vector2(200, 200));
                img.gameObject.AddComponent<Bobber>().Amplitude = 6f;
            }
            var b = UIKit.Text(_content, body, 34, ColorUtil.Ink, TextAnchor.UpperCenter, false).Fit(20);
            b.rectTransform.At(UIKit.Top, new Vector2(0, -470), new Vector2(780, 300));
            float y = 90;
            for (int i = options.Length - 1; i >= 0; i--)
            {
                var o = options[i];
                var btn = UIKit.Button(_content, o.Label, o.Color, () => { Close(); o.OnClick?.Invoke(); }, new Vector2(700, 130), 40, o.Icon);
                btn.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, y), new Vector2(700, 130));
                btn.Interactable = o.Enabled;
                btn.ClickWhenDisabled = false;
                y += 150;
            }
            if (App.Screens.TopModal != this) App.Screens.PushModal(this);
            else UIKit.Pop(PopTarget, 0.85f, 0.25f);
        }

        private void Close() => App.Screens.PopModal(this);

        public override bool OnBack()
        {
            Close();
            return true;
        }
    }

    /// <summary>The three islands switcher (village, forest, garden) shown at the bottom of each island screen.</summary>
    public static class IslandNav
    {
        public enum Island { City, Forest, Garden }

        public static void Add(RectTransform parent, App app, Island current, float y = 250f)
        {
            string[] keys = { "island.city", "island.forest", "island.garden" };
            Action[] go = { app.OpenCity, app.OpenForest, app.OpenGarden };
            Sprite[] icons = { CitySprites.Building("house"), IslandSprites.Mushroom(), IslandSprites.Crop(4) };
            Color[] colors = { ColorUtil.Blue, ColorUtil.Hex("#3E9A44"), ColorUtil.Orange };
            for (int i = 0; i < 3; i++)
            {
                bool here = (int)current == i;
                var b = UIKit.Button(parent, Loc.T(keys[i]), here ? colors[i] : Color.Lerp(colors[i], ColorUtil.Muted, 0.55f), go[i], new Vector2(310, 96), 32, icons[i]);
                b.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-330 + i * 330, y), new Vector2(310, 96));
                b.transform.localScale = Vector3.one * (here ? 1.05f : 0.95f);
            }
        }
    }

    /// <summary>Floating "+3" texts over the 3D islands.</summary>
    public static class FloatText
    {
        public static void Show(RectTransform root, Vector3 screenPos, string text, Sprite icon, MonoBehaviour owner)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenPos, null, out var local);
            var holder = UIKit.Node("Float", root);
            holder.anchorMin = holder.anchorMax = new Vector2(0.5f, 0.5f);
            holder.anchoredPosition = local;
            holder.sizeDelta = new Vector2(300, 90);
            var t = UIKit.Text(holder, text, 48, Color.white, TextAnchor.MiddleLeft, true);
            t.rectTransform.At(UIKit.Center, new Vector2(40, 0), new Vector2(220, 80));
            Image img = null;
            if (icon != null)
            {
                img = UIKit.Image(holder, icon, Color.white, false, "Icon");
                img.rectTransform.At(UIKit.Center, new Vector2(-70, 0), new Vector2(76, 76));
            }
            var from = local;
            Tweener.Run(1.1f, k =>
            {
                if (holder == null) return;
                holder.anchoredPosition = from + new Vector2(0, 160f * k);
                float a = 1f - Mathf.Clamp01((k - 0.6f) / 0.4f);
                t.color = new Color(1, 1, 1, a);
                if (img != null) img.color = new Color(1, 1, 1, a);
            }, Ease.OutQuad, () => { if (holder != null) UnityEngine.Object.Destroy(holder.gameObject); }, 0f, holder);
        }
    }

    /// <summary>
    /// The forest island screen: the 3D forest behind a transparent UI, the forest level and goods on top,
    /// the fox chase banner, and the islands switcher at the bottom.
    /// </summary>
    public sealed class ForestScreen : UIScreen
    {
        private Text _title, _xp, _goods, _hint;
        private Image _xpBar;
        private CandyButton _fox;
        private RectTransform _foxBanner;
        private Text _foxText;

        protected override void Build()
        {
            var top = UIKit.Panel(Root, ColorUtil.Glass, 0.6f, true, "TopBar");
            top.rectTransform.anchorMin = new Vector2(0, 1);
            top.rectTransform.anchorMax = new Vector2(1, 1);
            top.rectTransform.pivot = new Vector2(0.5f, 1f);
            top.rectTransform.offsetMin = new Vector2(12, -330);
            top.rectTransform.offsetMax = new Vector2(-12, -8);
            var bottom = UIKit.Panel(Root, ColorUtil.Glass, 0.6f, true, "BottomBar");
            bottom.rectTransform.anchorMin = new Vector2(0, 0);
            bottom.rectTransform.anchorMax = new Vector2(1, 0);
            bottom.rectTransform.pivot = new Vector2(0.5f, 0f);
            bottom.rectTransform.offsetMin = new Vector2(12, 8);
            bottom.rectTransform.offsetMax = new Vector2(-12, 320);

            var home = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 110);
            home.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(80, -80), new Vector2(110, 110));
            _title = UIKit.Text(Root, "", 64, ColorUtil.Hex("#7CE38A")).Fit(30);
            _title.rectTransform.At(UIKit.Top, new Vector2(40, -78), new Vector2(760, 100));
            _xpBar = UIKit.ProgressBar(Root, ColorUtil.Hex("#7CE38A"), new Vector2(600, 30));
            _xpBar.transform.parent.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(40, -150), new Vector2(600, 30));
            _xp = UIKit.Text(Root, "", 28, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(16);
            _xp.rectTransform.At(UIKit.Top, new Vector2(40, -190), new Vector2(800, 40));
            _goods = UIKit.Text(Root, "", 34, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(18);
            _goods.rectTransform.At(UIKit.Top, new Vector2(0, -260), new Vector2(1000, 60));

            _hint = UIKit.Text(Root, "", 30, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(18);
            _hint.rectTransform.At(UIKit.Bottom, new Vector2(0, 80), new Vector2(1000, 60));
            IslandNav.Add(Root, App, IslandNav.Island.Forest, 190f);

            _fox = UIKit.Button(Root, Loc.T("forest.fox_chip"), ColorUtil.Orange, StartFox, new Vector2(700, 110), 38, IslandSprites.Mushroom());
            _fox.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 400), new Vector2(700, 110));
            _fox.gameObject.AddComponent<Bobber>().Amplitude = 6f;
            var banner = UIKit.Panel(Root, ColorUtil.Glass, 0.85f, true, "FoxBanner");
            banner.rectTransform.At(UIKit.Top, new Vector2(0, -420), new Vector2(820, 120));
            _foxBanner = banner.rectTransform;
            _foxText = UIKit.Text(_foxBanner, "", 42, ColorUtil.Orange).Fit(22);
            _foxText.rectTransform.Fill(20, 10, 20, 10);
            _foxBanner.gameObject.SetActive(false);
        }

        protected override void OnShow()
        {
            var w = App.ForestWorld;
            w.MushroomPicked += OnMushroom;
            w.TreeChopped += OnTree;
            w.BerriesPicked += OnBerries;
            w.BuildingTapped += OnBuilding;
            w.FoxProgress += OnFoxProgress;
            w.FoxEnded += OnFoxEnded;
            App.Forest.LeveledUp += OnLevelUp;
            w.Show();
            _foxBanner.gameObject.SetActive(false);
            if (App.Economy.Data.forestXp == 0 && !App.Screens.HasModal)
                App.Popup.ShowMessage(Loc.T("island.forest"), Loc.T("forest.intro"), IslandSprites.Mushroom());
        }

        protected override void OnHide()
        {
            var w = App.ForestWorld;
            w.MushroomPicked -= OnMushroom;
            w.TreeChopped -= OnTree;
            w.BerriesPicked -= OnBerries;
            w.BuildingTapped -= OnBuilding;
            w.FoxProgress -= OnFoxProgress;
            w.FoxEnded -= OnFoxEnded;
            App.Forest.LeveledUp -= OnLevelUp;
            w.Hide();
        }

        public override void Refresh()
        {
            var f = App.Forest;
            var d = App.Economy.Data;
            _title.text = Loc.T("forest.title", f.Level);
            _xpBar.fillAmount = f.LevelProgress;
            _xp.text = f.NextLevelXp > 0 ? Loc.T("forest.xp", f.Xp, f.NextLevelXp) : Loc.T("forest.max");
            _goods.text = Loc.T("forest.goods", d.mushrooms, d.goldMushrooms, d.berries, d.honey, d.wood);
            int ready = f.ReadyMushrooms;
            _hint.text = ready > 0 ? Loc.T("forest.ready", ready) : Loc.T("forest.wait");
            _fox.gameObject.SetActive(f.FoxReady && !App.ForestWorld.FoxActive);
        }

        private float _refresh;

        private void Update()
        {
            if (App.ForestWorld.FoxActive)
                _foxText.text = Loc.T("forest.fox_do", _foxHits, JellyDrop.City3D.ForestWorld.FoxTaps) + "  ·  " + Loc.T("mission.time", Mathf.CeilToInt(App.ForestWorld.FoxTimeLeft));
            _refresh += Time.unscaledDeltaTime;
            if (_refresh > 1f) { _refresh = 0f; if (!App.Screens.HasModal) Refresh(); }
        }

        private int _foxHits;

        // ------------------------------------------------------------------ events from the 3D forest

        private void OnMushroom(ForestPick kind, Vector3 screen)
        {
            bool gold = kind == ForestPick.GoldMushroom;
            App.Audio.Play(gold ? Sfx.Golden : Sfx.Pop);
            Haptics.Tap();
            FloatText.Show(Root, screen, "+1", IslandSprites.Mushroom(gold), this);
            Refresh();
        }

        private void OnTree(int wood, Vector3 screen)
        {
            App.Audio.Play(Sfx.Lock);
            Haptics.Medium();
            FloatText.Show(Root, screen, "+" + wood, CitySprites.SupplyIcon(JellyDrop.Core.City.Supply.Wood), this);
            Refresh();
        }

        private void OnBerries(int berries, Vector3 screen)
        {
            App.Audio.Play(Sfx.Pop);
            Haptics.Tap();
            FloatText.Show(Root, screen, "+" + berries, IslandSprites.Berries(), this);
            Refresh();
        }

        private void OnLevelUp(int level)
        {
            App.Audio.Play(Sfx.LevelUp);
            Tweener.Delay(0.4f, () => App.Popup.ShowReward(Loc.T("forest.title", level), Loc.T("forest.level_up"), IslandSprites.Mushroom(true)), this);
        }

        private void OnBuilding(ForestBuilding b)
        {
            var f = App.Forest;
            var def = ForestCatalog.Get(b);
            int level = f.BuildingLevel(b);
            string name = Loc.T("forest.b." + b);
            var body = new List<string> { Loc.T("forest.b." + b + ".desc") };
            if (level > 0) body.Add(Loc.T("city.level_of", level, def.MaxLevel));
            var options = new List<ActionPopup.Option>();

            if (b == ForestBuilding.Apiary && level > 0)
            {
                int honey = f.HoneyPending;
                options.Add(new ActionPopup.Option(Loc.T("forest.collect_honey", honey), ColorUtil.Gold, () =>
                {
                    int got = f.CollectHoney();
                    if (got > 0) { App.Audio.Play(Sfx.Coin); App.Save.Save(App.Economy.Data); }
                }, IslandSprites.Honey(), honey > 0));
            }
            if (b == ForestBuilding.Herbalist && level > 0)
            {
                options.Add(new ActionPopup.Option(Loc.T("forest.brew", f.BrewCost), ColorUtil.Purple, () =>
                {
                    var p = f.Brew();
                    if (p.HasValue)
                    {
                        App.Save.Save(App.Economy.Data);
                        App.Audio.Play(Sfx.PowerUp);
                        App.Popup.ShowReward("+1 " + Loc.PowerUpShortName(p.Value), Loc.T("forest.brewed"), SpriteFactory.PowerUpIcon(p.Value));
                    }
                }, IslandSprites.Mushroom(), App.Economy.Data.mushrooms >= f.BrewCost));
            }

            if (level < def.MaxLevel)
            {
                var r = f.CanUpgrade(b);
                string cost = Loc.T("forest.cost", def.Coins[level], def.Wood[level], def.Mushrooms[level]);
                if (r == ForestBuildResult.Locked) body.Add(Loc.T("forest.unlock_at", def.UnlockLevel));
                else body.Add(cost);
                options.Add(new ActionPopup.Option(Loc.T(level == 0 ? "forest.build" : "forest.upgrade"), ColorUtil.Mint, () => DoUpgrade(b),
                    SpriteFactory.Coin(), r == ForestBuildResult.Ok));
                if (r != ForestBuildResult.Ok && r != ForestBuildResult.Locked) body.Add(Loc.T("forest.missing." + r));
            }
            else body.Add(Loc.T("forest.maxed"));

            App.Actions.Show(name, string.Join("\n", body), IslandSprites.Mushroom(b == ForestBuilding.Watchtower), options.ToArray());
        }

        private void DoUpgrade(ForestBuilding b)
        {
            var r = App.Forest.Upgrade(b);
            if (r != ForestBuildResult.Ok) { App.Audio.Play(Sfx.Error); return; }
            App.Save.Save(App.Economy.Data);
            App.Audio.Play(Sfx.Win);
            Haptics.Medium();
            App.ForestWorld.Sync();
            Refresh();
        }

        // ------------------------------------------------------------------ fox chase

        private void StartFox()
        {
            _foxHits = 0;
            _fox.gameObject.SetActive(false);
            _foxBanner.gameObject.SetActive(true);
            UIKit.Pop(_foxBanner, 0.8f, 0.3f);
            App.ForestWorld.BeginFox();
            App.Audio.Play(Sfx.Pop);
        }

        private void OnFoxProgress(int hits, int needed)
        {
            _foxHits = hits;
            if (hits > 0) { App.Audio.Play(Sfx.Pop); Haptics.Tap(); UIKit.Punch(_foxBanner, 0.08f); }
        }

        private void OnFoxEnded(bool caught)
        {
            _foxBanner.gameObject.SetActive(false);
            int won = App.Forest.FinishFox(caught);
            App.Save.Save(App.Economy.Data);
            if (caught)
            {
                App.Audio.Play(Sfx.Win);
                App.Popup.ShowReward("+" + won, Loc.T("forest.fox_caught"), IslandSprites.Mushroom());
            }
            else if (IsVisible)
            {
                App.Audio.Play(Sfx.Error);
                App.Popup.ShowMessage(Loc.T("forest.fox_title"), Loc.T("forest.fox_escaped"), IslandSprites.Mushroom());
            }
            Refresh();
        }

        public override bool OnBack()
        {
            App.OpenCity();
            return true;
        }
    }
}
