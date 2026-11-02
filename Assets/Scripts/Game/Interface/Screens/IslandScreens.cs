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

namespace JellyDrop.Interface
{
    /// <summary>
    /// The garden island screen: the 3D garden behind a transparent UI, garden level and goods on top,
    /// kitchen and stall buttons, the crows banner, and the islands switcher.
    /// </summary>
    public sealed class GardenScreen : UIScreen
    {
        private Text _title, _xp, _goods, _hint;
        private Image _xpBar;
        private CandyButton _crows;
        private RectTransform _crowBanner;
        private Text _crowText;
        private int _crowHits;
        private float _refresh;

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
            bottom.rectTransform.offsetMax = new Vector2(-12, 430);

            var home = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 110);
            home.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(80, -80), new Vector2(110, 110));
            _title = UIKit.Text(Root, "", 64, ColorUtil.Orange).Fit(30);
            _title.rectTransform.At(UIKit.Top, new Vector2(40, -78), new Vector2(760, 100));
            _xpBar = UIKit.ProgressBar(Root, ColorUtil.Orange, new Vector2(600, 30));
            _xpBar.transform.parent.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(40, -150), new Vector2(600, 30));
            _xp = UIKit.Text(Root, "", 28, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(16);
            _xp.rectTransform.At(UIKit.Top, new Vector2(40, -190), new Vector2(800, 40));
            _goods = UIKit.Text(Root, "", 32, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(16);
            _goods.rectTransform.At(UIKit.Top, new Vector2(0, -262), new Vector2(1020, 70));

            var kitchen = UIKit.Button(Root, Loc.T("garden.kitchen"), ColorUtil.Pink, OpenKitchen, new Vector2(470, 120), 40, IslandSprites.Dish(1));
            kitchen.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-255, 340), new Vector2(470, 120));
            var stall = UIKit.Button(Root, Loc.T("garden.sell"), ColorUtil.Gold, SellCrops, new Vector2(470, 120), 40, SpriteFactory.Coin());
            stall.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(255, 340), new Vector2(470, 120));
            _hint = UIKit.Text(Root, "", 30, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(16);
            _hint.rectTransform.At(UIKit.Bottom, new Vector2(0, 75), new Vector2(1000, 60));
            IslandNav.Add(Root, App, IslandNav.Island.Garden, 190f);

            _crows = UIKit.Button(Root, Loc.T("garden.crows_chip"), ColorUtil.Purple, StartCrows, new Vector2(700, 110), 38, null);
            _crows.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 500), new Vector2(700, 110));
            _crows.gameObject.AddComponent<Bobber>().Amplitude = 6f;
            var banner = UIKit.Panel(Root, ColorUtil.Glass, 0.85f, true, "CrowBanner");
            banner.rectTransform.At(UIKit.Top, new Vector2(0, -420), new Vector2(820, 120));
            _crowBanner = banner.rectTransform;
            _crowText = UIKit.Text(_crowBanner, "", 42, ColorUtil.Orange).Fit(22);
            _crowText.rectTransform.Fill(20, 10, 20, 10);
            _crowBanner.gameObject.SetActive(false);
        }

        protected override void OnShow()
        {
            var w = App.GardenWorld;
            w.BedTapped += OnBed;
            w.BuildingTapped += OnBuilding;
            w.CrowProgress += OnCrowProgress;
            w.CrowsEnded += OnCrowsEnded;
            App.Garden.LeveledUp += OnLevelUp;
            w.Show();
            _crowBanner.gameObject.SetActive(false);
            if (App.Economy.Data.gardenXp == 0 && App.Garden.IsEmpty(0) && !App.Screens.HasModal)
                App.Popup.ShowMessage(Loc.T("island.garden"), Loc.T("garden.intro"), IslandSprites.Crop(0));
        }

        protected override void OnHide()
        {
            var w = App.GardenWorld;
            w.BedTapped -= OnBed;
            w.BuildingTapped -= OnBuilding;
            w.CrowProgress -= OnCrowProgress;
            w.CrowsEnded -= OnCrowsEnded;
            App.Garden.LeveledUp -= OnLevelUp;
            w.Hide();
        }

        public override void Refresh()
        {
            var g = App.Garden;
            var d = App.Economy.Data;
            _title.text = Loc.T("garden.title", g.Level);
            _xpBar.fillAmount = g.LevelProgress;
            _xp.text = g.NextLevelXp > 0 ? Loc.T("forest.xp", g.Xp, g.NextLevelXp) : Loc.T("forest.max");
            _goods.text = Loc.T("garden.goods", d.crops[0], d.crops[1], d.crops[2], d.crops[3], d.crops[4], d.eggs, d.compost);
            int ripe = 0, empty = 0;
            for (int i = 0; i < g.OpenBeds; i++) { if (g.IsRipe(i)) ripe++; else if (g.IsEmpty(i)) empty++; }
            _hint.text = ripe > 0 ? Loc.T("garden.ripe", ripe) : empty > 0 ? Loc.T("garden.empty", empty) : Loc.T("garden.growing");
            _crows.gameObject.SetActive(g.CrowsReady && !App.GardenWorld.CrowsActive);
        }

        private void Update()
        {
            if (App.GardenWorld.CrowsActive)
                _crowText.text = Loc.T("garden.crows_do", _crowHits, JellyDrop.City3D.GardenWorld.CrowTaps) + "  ·  " + Loc.T("mission.time", Mathf.CeilToInt(App.GardenWorld.CrowTimeLeft));
            _refresh += Time.unscaledDeltaTime;
            if (_refresh > 1f) { _refresh = 0f; if (!App.Screens.HasModal) Refresh(); }
        }

        // ------------------------------------------------------------------ beds

        private void OnBed(int bed)
        {
            var g = App.Garden;
            if (g.IsRipe(bed))
            {
                var crop = g.CropIn(bed).Value;
                int n = g.Harvest(bed);
                App.Save.Save(App.Economy.Data);
                App.Audio.Play(Sfx.Coin);
                Haptics.Tap();
                FloatText.Show(Root, App.GardenWorld.BedScreen(bed), "+" + n, IslandSprites.Crop((int)crop), this);
                App.GardenWorld.Sync();
                Refresh();
                return;
            }
            if (g.IsEmpty(bed))
            {
                var options = new List<ActionPopup.Option>();
                foreach (var def in GardenCatalog.Crops)
                {
                    var t = def.Type;
                    bool unlocked = g.IsCropUnlocked(t);
                    string label = unlocked
                        ? Loc.T("garden.plant", Loc.T("garden.crop." + t), def.SeedCost, CityUi.Duration(g.GrowSeconds(t)))
                        : Loc.T("garden.crop_locked", Loc.T("garden.crop." + t), def.UnlockLevel);
                    options.Add(new ActionPopup.Option(label, ColorUtil.Hex("#3E9A44"), () => Plant(bed, t), IslandSprites.Crop((int)t),
                        unlocked && App.Economy.Coins >= def.SeedCost));
                }
                string body = Loc.T("garden.choose_seed") + (g.Compost > 0 ? "\n" + Loc.T("garden.has_compost", g.Compost) : "");
                App.Actions.Show(Loc.T("garden.bed"), body, IslandSprites.Seedling(), options.ToArray());
                return;
            }
            // Growing
            var c = g.CropIn(bed).Value;
            var opts = new List<ActionPopup.Option>();
            opts.Add(new ActionPopup.Option(g.IsWatered(bed) ? Loc.T("garden.watered") : Loc.T("garden.water"), ColorUtil.Blue, () =>
            {
                if (g.Water(bed) == GardenResult.Ok) { App.Audio.Play(Sfx.Pop); App.Save.Save(App.Economy.Data); App.GardenWorld.Sync(); }
            }, IslandSprites.WateringCan(), !g.IsWatered(bed)));
            if (g.BuildingLevel(GardenBuilding.Well) > 0)
                opts.Add(new ActionPopup.Option(Loc.T("garden.water_all"), ColorUtil.Blue, WaterAll, IslandSprites.WateringCan()));
            string info = Loc.T("garden.growing_info", Loc.T("garden.crop." + c), CityUi.Duration(g.SecondsLeft(bed))) +
                          (g.HasCompost(bed) ? "\n" + Loc.T("garden.composted") : "");
            App.Actions.Show(Loc.T("garden.crop." + c), info, IslandSprites.Crop((int)c), opts.ToArray());
        }

        private void Plant(int bed, CropType t)
        {
            var r = App.Garden.Plant(bed, t);
            if (r != GardenResult.Ok) { App.Audio.Play(Sfx.Error); return; }
            App.Save.Save(App.Economy.Data);
            App.Audio.Play(Sfx.Pop);
            App.GardenWorld.Sync();
            App.GardenWorld.PopBed(bed);
            Refresh();
        }

        private void WaterAll()
        {
            int n = App.Garden.WaterAll();
            App.Audio.Play(n > 0 ? Sfx.Pop : Sfx.Error);
            App.Save.Save(App.Economy.Data);
            App.GardenWorld.Sync();
        }

        // ------------------------------------------------------------------ buildings

        private void OnBuilding(GardenBuilding b)
        {
            var g = App.Garden;
            var def = GardenCatalog.Get(b);
            int level = g.BuildingLevel(b);
            var body = new List<string> { Loc.T("garden.b." + b + ".desc") };
            if (level > 0) body.Add(Loc.T("city.level_of", level, def.MaxLevel));
            var options = new List<ActionPopup.Option>();
            if (level > 0)
            {
                switch (b)
                {
                    case GardenBuilding.Coop:
                        int eggs = g.EggsPending;
                        options.Add(new ActionPopup.Option(Loc.T("garden.collect_eggs", eggs), ColorUtil.Gold, () =>
                        {
                            if (g.CollectEggs() > 0) { App.Audio.Play(Sfx.Coin); App.Save.Save(App.Economy.Data); }
                        }, IslandSprites.Egg(), eggs > 0));
                        break;
                    case GardenBuilding.Compost:
                        options.Add(new ActionPopup.Option(Loc.T("garden.make_compost", g.CompostCost), ColorUtil.Hex("#6B8A3A"), () =>
                        {
                            if (g.MakeCompost() == GardenResult.Ok) { App.Audio.Play(Sfx.Pop); App.Save.Save(App.Economy.Data); }
                        }, IslandSprites.Mushroom(), App.Economy.Mushrooms >= g.CompostCost));
                        break;
                    case GardenBuilding.Well:
                        options.Add(new ActionPopup.Option(Loc.T("garden.water_all"), ColorUtil.Blue, WaterAll, IslandSprites.WateringCan()));
                        break;
                }
            }
            if (level < def.MaxLevel)
            {
                var r = g.CanUpgrade(b);
                if (r == GardenResult.Locked) body.Add(Loc.T("garden.unlock_at", def.UnlockLevel));
                else body.Add(Loc.T("garden.cost", def.Coins[level], def.Wood[level], def.Brick[level]));
                options.Add(new ActionPopup.Option(Loc.T(level == 0 ? "forest.build" : "forest.upgrade"), ColorUtil.Mint, () =>
                {
                    if (g.Upgrade(b) == GardenResult.Ok)
                    {
                        App.Save.Save(App.Economy.Data);
                        App.Audio.Play(Sfx.Win);
                        Haptics.Medium();
                        App.GardenWorld.Sync();
                        Refresh();
                    }
                    else App.Audio.Play(Sfx.Error);
                }, SpriteFactory.Coin(), r == GardenResult.Ok));
            }
            else body.Add(Loc.T("forest.maxed"));
            App.Actions.Show(Loc.T("garden.b." + b), string.Join("\n", body), IslandSprites.Crop(b == GardenBuilding.Greenhouse ? 1 : 4), options.ToArray());
        }

        // ------------------------------------------------------------------ kitchen and stall

        private void OpenKitchen()
        {
            var g = App.Garden;
            var options = new List<ActionPopup.Option>();
            foreach (var r in GardenCatalog.Recipes)
            {
                var recipe = r;
                options.Add(new ActionPopup.Option(Loc.T("garden.dish." + r.Id) + "  ·  " + Ingredients(r), ColorUtil.Pink, () => Cook(recipe),
                    IslandSprites.Dish(r.Index), g.CanCook(r)));
            }
            App.Actions.Show(Loc.T("garden.kitchen"), Loc.T("garden.kitchen_info"), IslandSprites.Dish(0), options.ToArray());
        }

        private static string Ingredients(Recipe r)
        {
            var parts = new List<string>();
            for (int i = 0; i < 5; i++) if (r.Crops[i] > 0) parts.Add(r.Crops[i] + " " + Loc.T("garden.crop_short." + (CropType)i));
            if (r.Mushrooms > 0) parts.Add(r.Mushrooms + " " + Loc.T("garden.ing.mushroom"));
            if (r.Eggs > 0) parts.Add(r.Eggs + " " + Loc.T("garden.ing.egg"));
            if (r.Honey > 0) parts.Add(r.Honey + " " + Loc.T("garden.ing.honey"));
            return string.Join(", ", parts);
        }

        private void Cook(Recipe r)
        {
            if (!App.Garden.Cook(r)) { App.Audio.Play(Sfx.Error); return; }
            // Dishes that are for the pet
            if (r.Id == "veggie_soup") { App.Pet.AddFood(100f); App.Pet.AddFun(20f); }
            if (r.Id == "omelette") App.Pet.AddEnergy(100f);
            App.Save.Save(App.Economy.Data);
            App.Audio.Play(Sfx.Win);
            App.Popup.ShowReward(Loc.T("garden.dish." + r.Id), Loc.T("garden.dish." + r.Id + ".bonus"), IslandSprites.Dish(r.Index));
            Refresh();
        }

        private void SellCrops()
        {
            int coins = App.Garden.SellAllCrops();
            if (coins <= 0)
            {
                App.Audio.Play(Sfx.Error);
                App.Popup.ShowMessage(Loc.T("garden.sell"), Loc.T("garden.nothing_to_sell"), IslandSprites.Crop(0));
                return;
            }
            App.Save.Save(App.Economy.Data);
            App.Audio.Play(Sfx.Coin);
            App.Popup.ShowReward("+" + Loc.Number(coins), Loc.T("garden.sold"), SpriteFactory.Coin());
            Refresh();
        }

        private void OnLevelUp(int level)
        {
            App.Audio.Play(Sfx.LevelUp);
            Tweener.Delay(0.4f, () => App.Popup.ShowReward(Loc.T("garden.title", level), Loc.T("garden.level_up"), IslandSprites.Crop(4)), this);
        }

        // ------------------------------------------------------------------ crows

        private void StartCrows()
        {
            _crowHits = 0;
            _crows.gameObject.SetActive(false);
            _crowBanner.gameObject.SetActive(true);
            UIKit.Pop(_crowBanner, 0.8f, 0.3f);
            App.GardenWorld.BeginCrows();
        }

        private void OnCrowProgress(int hits, int needed)
        {
            _crowHits = hits;
            if (hits > 0) { App.Audio.Play(Sfx.Pop); Haptics.Tap(); UIKit.Punch(_crowBanner, 0.08f); }
        }

        private void OnCrowsEnded(bool chased)
        {
            _crowBanner.gameObject.SetActive(false);
            App.Garden.FinishCrows(chased);
            App.Save.Save(App.Economy.Data);
            if (chased)
            {
                App.Audio.Play(Sfx.Win);
                App.Popup.ShowReward("+" + GardenService.CrowCoins, Loc.T("garden.crows_chased", GardenService.CrowEggs), SpriteFactory.Coin());
            }
            else if (IsVisible)
            {
                App.Audio.Play(Sfx.Error);
                App.Popup.ShowMessage(Loc.T("garden.crows_title"), Loc.T("garden.crows_won"), IslandSprites.Crop(0));
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
