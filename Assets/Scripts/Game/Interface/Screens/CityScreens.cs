using System;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.City;
using JellyDrop.Core.Economy;
using JellyDrop.Services;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    internal static class CityUi
    {
        public static Sprite ProductIcon(Product p)
        {
            switch (p)
            {
                case Product.Wood: return CitySprites.SupplyIcon(Supply.Wood);
                case Product.Brick: return CitySprites.SupplyIcon(Supply.Brick);
                case Product.Glass: return CitySprites.SupplyIcon(Supply.Glass);
                default: return SpriteFactory.Coin();
            }
        }

        /// <summary>Row of cost icons; amounts the player can't afford turn red.</summary>
        public static RectTransform CostRow(Transform parent, BuildingCost cost, CityService city, int coins, float iconSize = 56f)
        {
            var row = UIKit.Node("Cost", parent);
            float x = 0f;
            void Item(Sprite icon, int need, int have)
            {
                if (need <= 0) return;
                var img = UIKit.Image(row, icon, Color.white, false, "Icon");
                img.rectTransform.anchorMin = img.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                img.rectTransform.pivot = new Vector2(0f, 0.5f);
                img.rectTransform.anchoredPosition = new Vector2(x, 0);
                img.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
                var t = UIKit.Text(row, Loc.Number(need), Mathf.RoundToInt(iconSize * 0.62f), have >= need ? ColorUtil.Ink : ColorUtil.Red, TextAnchor.MiddleLeft, false);
                t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                t.rectTransform.pivot = new Vector2(0f, 0.5f);
                t.rectTransform.anchoredPosition = new Vector2(x + iconSize + 4, 0);
                float w = 24f + t.fontSize * 0.62f * Loc.Number(need).Length;
                t.rectTransform.sizeDelta = new Vector2(w, iconSize);
                x += iconSize + 10 + w;
            }
            Item(SpriteFactory.Coin(), cost.Coins, coins);
            Item(CitySprites.SupplyIcon(Supply.Wood), cost.Wood, city.Amount(Supply.Wood));
            Item(CitySprites.SupplyIcon(Supply.Brick), cost.Brick, city.Amount(Supply.Brick));
            Item(CitySprites.SupplyIcon(Supply.Glass), cost.Glass, city.Amount(Supply.Glass));
            row.sizeDelta = new Vector2(x, iconSize);
            return row;
        }

        /// <summary>One line describing what a building does at a level (1-based).</summary>
        public static string Describe(BuildingDef def, int level)
        {
            int lv = Mathf.Clamp(level, 1, def.MaxLevel);
            var parts = new System.Collections.Generic.List<string>();
            if (def.Produces != Product.None)
            {
                string what = def.Produces == Product.Coins ? Loc.Coins(def.PerHourAt(lv))
                    : def.PerHourAt(lv) + " " + Loc.SupplyName((Supply)((int)def.Produces - 2));
                parts.Add(Loc.T("city.produces", what));
            }
            if (def.PopulationAt(lv) > 0) parts.Add(Loc.T("city.people", def.PopulationAt(lv)));
            if (def.CoinBonusAt(lv) > 0) parts.Add(Loc.T("city.bonus_short", def.CoinBonusAt(lv)));
            return string.Join("  ·  ", parts);
        }

        public static string ResultMessage(BuildResult r) =>
            r == BuildResult.NotEnoughCoins ? Loc.T("city.not_enough_coins")
            : r == BuildResult.NotEnoughSupplies ? Loc.T("city.not_enough_supplies")
            : r == BuildResult.NoFreeBuilder ? Loc.T("city.no_builder", CityCatalog.Builders)
            : r == BuildResult.UnderConstruction ? Loc.T("city.busy")
            : r.ToString();

        /// <summary>"2h 05m", "4m 30s", "45s".</summary>
        public static string Duration(long seconds)
        {
            if (seconds >= 3600) return $"{seconds / 3600}h {seconds % 3600 / 60:00}m";
            if (seconds >= 60) return $"{seconds / 60}m {seconds % 60:00}s";
            return $"{Math.Max(0, seconds)}s";
        }
    }

    /// <summary>
    /// The city: 12 plots on a grassy field. Empty plots open the build menu, built ones collect what they
    /// made (or open the upgrade menu when there is nothing to collect). Locked plots open with city level.
    /// </summary>
    public sealed class CityScreen : UIScreen
    {
        private ResourceBar _resources;
        private Text _title;
        private Text _population;
        private Image _populationBar;
        private Text _bonus;
        private Text _builders;
        private CandyButton _collect;
        private float _refreshTimer;

        protected override void Build()
        {
            // The middle of the screen is see-through: the 3D village (CityWorld) renders behind the UI.
            // Glass bars keep the header and buttons readable over it.
            var topBar = UIKit.Panel(Root, ColorUtil.Glass, 0.6f, true, "TopBar");
            topBar.rectTransform.anchorMin = new Vector2(0, 1);
            topBar.rectTransform.anchorMax = new Vector2(1, 1);
            topBar.rectTransform.pivot = new Vector2(0.5f, 1f);
            topBar.rectTransform.offsetMin = new Vector2(12, -380);
            topBar.rectTransform.offsetMax = new Vector2(-12, -8);
            var bottomBar = UIKit.Panel(Root, ColorUtil.Glass, 0.6f, true, "BottomBar");
            bottomBar.rectTransform.anchorMin = new Vector2(0, 0);
            bottomBar.rectTransform.anchorMax = new Vector2(1, 0);
            bottomBar.rectTransform.pivot = new Vector2(0.5f, 0f);
            bottomBar.rectTransform.offsetMin = new Vector2(12, 8);
            bottomBar.rectTransform.offsetMax = new Vector2(-12, 210);

            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 110);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(80, -80), new Vector2(110, 110));
            _title = UIKit.Text(Root, "", 70, ColorUtil.Mint).Fit(34);
            _title.rectTransform.At(UIKit.Top, new Vector2(40, -80), new Vector2(760, 110));

            _resources = ResourceBar.Create(Root, UIKit.Top, new Vector2(0, -200));

            _population = UIKit.Text(Root, "", 34, ColorUtil.Ink, TextAnchor.MiddleLeft, false).Fit(20);
            _population.rectTransform.At(UIKit.Top, new Vector2(-170, -282), new Vector2(560, 50));
            _populationBar = UIKit.ProgressBar(Root, ColorUtil.Mint, new Vector2(560, 34));
            _populationBar.transform.parent.GetComponent<RectTransform>().At(UIKit.Top, new Vector2(-170, -330), new Vector2(560, 34));
            _bonus = UIKit.Text(Root, "", 34, ColorUtil.Orange, TextAnchor.MiddleRight, false).Fit(20);
            _bonus.rectTransform.At(UIKit.Top, new Vector2(300, -290), new Vector2(360, 50));
            _builders = UIKit.Text(Root, "", 30, ColorUtil.InkDim, TextAnchor.MiddleRight, false).Fit(18);
            _builders.rectTransform.At(UIKit.Top, new Vector2(300, -338), new Vector2(360, 44));

            _collect = UIKit.Button(Root, Loc.T("city.collect"), ColorUtil.Gold, CollectAll, new Vector2(470, 150), 54, SpriteFactory.Coin());
            _collect.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-255, 105), new Vector2(470, 150));
            _collect.ClickWhenDisabled = true;
            var market = UIKit.Button(Root, Loc.T("city.market"), ColorUtil.Blue, () => App.Market.Open(), new Vector2(470, 150), 54, CitySprites.SupplyIcon(Supply.Brick));
            market.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(255, 105), new Vector2(470, 150));
        }

        protected override void OnShow()
        {
            _refreshTimer = 0f;
            App.CityWorld.PlotTapped += OnPlot;
            App.CityWorld.Show();
            if (App.City.Population == 0 && !App.Screens.HasModal)
                App.Popup.ShowMessage(Loc.T("home.city"), Loc.T("city.intro"), CitySprites.Building("house"));
        }

        public override void Refresh()
        {
            var city = App.City;
            _resources.Refresh(App);
            _title.text = Loc.T("city.level", city.CityLevel);
            int next = city.NextLevelPopulation;
            if (next > 0)
            {
                int prev = CityCatalog.PopulationForLevel(city.CityLevel);
                _population.text = Loc.T("city.population", city.Population, next);
                _populationBar.fillAmount = Mathf.Clamp01((city.Population - prev) / (float)Mathf.Max(1, next - prev));
            }
            else
            {
                _population.text = Loc.T("city.population_max", city.Population);
                _populationBar.fillAmount = 1f;
            }
            int bonus = city.CoinBonusPercent;
            _bonus.text = bonus > 0 ? Loc.T("city.bonus_short", bonus) : "";
            _builders.text = Loc.T("city.builders", city.BusyBuilders, CityCatalog.Builders);
            _collect.Interactable = !city.PendingTotal().IsEmpty;
            if (IsVisible) App.CityWorld.Sync();
        }

        protected override void OnHide()
        {
            App.CityWorld.PlotTapped -= OnPlot;
            App.CityWorld.Hide();
        }

        private void Update()
        {
            // Production ticks in real time; refresh the storage bars now and then.
            _refreshTimer += Time.unscaledDeltaTime;
            if (_refreshTimer < 5f) return;
            _refreshTimer = 0f;
            if (!App.Screens.HasModal) Refresh();
        }

        private void OnPlot(int index)
        {
            var city = App.City;
            if (!city.IsPlotUnlocked(index))
            {
                App.Audio.Play(Sfx.Error);
                App.Popup.ShowMessage(Loc.T("city.empty_plot"), Loc.T("city.locked_plot", city.PlotUnlockLevel(index)), SpriteFactory.LockIcon());
                return;
            }
            if (city.BuildingAt(index) == null)
            {
                App.BuildMenu.OpenBuild(index);
                return;
            }
            if (city.Pending(index) > 0 && !city.IsUnderConstruction(index))
            {
                var h = city.CollectPlot(index);
                App.CityWorld.PopPlot(index);
                PlayCollect(h);
                return;
            }
            App.BuildMenu.OpenUpgrade(index);
        }

        private void CollectAll()
        {
            var h = App.City.CollectAll();
            if (h.IsEmpty)
            {
                App.Audio.Play(Sfx.Error);
                App.Popup.ShowMessage(Loc.T("city.collect"), Loc.T("city.nothing"), SpriteFactory.Coin());
                return;
            }
            PlayCollect(h);
            UIKit.Punch(_resources.transform);
        }

        private void PlayCollect(Harvest h)
        {
            App.Audio.Play(h.Coins > 0 ? Sfx.Coin : Sfx.Pop);
            UIKit.Punch(_resources.transform, 0.08f);
        }

        public override bool OnBack()
        {
            App.GoHome();
            return true;
        }
    }

    /// <summary>Modal for building on an empty plot (list of buildings) or upgrading a built one.</summary>
    public sealed class BuildMenu : UIScreen
    {
        private RectTransform _content;
        private int _plot;

        protected override void Build()
        {
            var card = BuildModalCard(new Vector2(960, 1500), ColorUtil.Panel, ColorUtil.Mint);
            _content = UIKit.Node("Content", card).Fill(30, 30, 30, 30);
        }

        private void Begin(string title)
        {
            UIKit.ClearChildren(_content);
            var close = UIKit.IconButton(_content, SpriteFactory.CloseIcon(), ColorUtil.Purple, () => App.Screens.PopModal(this), 90);
            close.GetComponent<RectTransform>().At(UIKit.TopRight, new Vector2(-30, -30), new Vector2(90, 90));
            var t = UIKit.Text(_content, title, 60, ColorUtil.Mint).Fit(30);
            t.rectTransform.At(UIKit.Top, new Vector2(-30, -60), new Vector2(720, 90));
        }

        public void OpenBuild(int plot)
        {
            _plot = plot;
            ShowBuildList();
            if (App.Screens.TopModal != this) App.Screens.PushModal(this);
        }

        public void OpenUpgrade(int plot)
        {
            _plot = plot;
            ShowUpgrade();
            if (App.Screens.TopModal != this) App.Screens.PushModal(this);
        }

        public override void Refresh()
        {
            if (!IsVisible) return;
            if (App.City.BuildingAt(_plot) == null) ShowBuildList();
            else ShowUpgrade();
        }

        private void ShowBuildList()
        {
            Begin(Loc.T("city.choose"));
            var city = App.City;

            var viewport = UIKit.Panel(_content, PixelCanvas.WithAlpha(ColorUtil.PanelLight, 1f), 0.8f, true, "Viewport");
            viewport.rectTransform.Fill(0, 0, 0, 130);
            viewport.gameObject.AddComponent<RectMask2D>();
            var list = UIKit.Node("List", viewport.transform);
            list.anchorMin = new Vector2(0, 1);
            list.anchorMax = new Vector2(1, 1);
            list.pivot = new Vector2(0.5f, 1f);
            list.offsetMin = list.offsetMax = Vector2.zero;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport.rectTransform;
            scroll.content = list;
            scroll.horizontal = false;

            const float rowH = 250f;
            float y = -14f;
            foreach (var def in city.BuildOptions())
            {
                BuildRow(list, def, y, rowH);
                y -= rowH + 14f;
            }
            list.sizeDelta = new Vector2(0, -y + 10);
        }

        private void BuildRow(RectTransform list, BuildingDef def, float y, float h)
        {
            var city = App.City;
            bool levelOk = city.CityLevel >= def.UnlockCityLevel;
            var row = UIKit.NeonCard(list, levelOk ? ColorUtil.Mint : ColorUtil.Muted, ColorUtil.Panel, 1.2f, false, "Row", 0.2f);
            row.rectTransform.anchorMin = new Vector2(0, 1);
            row.rectTransform.anchorMax = new Vector2(1, 1);
            row.rectTransform.pivot = new Vector2(0.5f, 1f);
            row.rectTransform.offsetMin = new Vector2(14, y - h);
            row.rectTransform.offsetMax = new Vector2(-14, y);

            var img = UIKit.Image(row.transform, CitySprites.Building(def.Id), levelOk ? Color.white : new Color(1, 1, 1, 0.45f));
            img.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(115, 0), new Vector2(200, 200));

            var name = UIKit.Text(row.transform, Loc.BuildingName(def.Id), 44, ColorUtil.Ink, TextAnchor.MiddleLeft, false).Fit(24);
            name.rectTransform.At(UIKit.TopLeft, new Vector2(420, -45), new Vector2(400, 60));
            var desc = UIKit.Text(row.transform, CityUi.Describe(def, 1), 28, ColorUtil.InkDim, TextAnchor.MiddleLeft, false).Fit(16);
            desc.rectTransform.At(UIKit.TopLeft, new Vector2(420, -110), new Vector2(400, 70));

            var cost = CityUi.CostRow(row.transform, def.CostFor(1), city, App.Economy.Coins, 44);
            cost.anchorMin = cost.anchorMax = new Vector2(0f, 0f);
            cost.pivot = new Vector2(0f, 0.5f);
            cost.anchoredPosition = new Vector2(220, 45);

            string id = def.Id;
            if (levelOk)
            {
                var b = UIKit.Button(row.transform, Loc.T("city.build"), ColorUtil.Mint, () => DoBuild(id), new Vector2(210, 110), 36);
                b.GetComponent<RectTransform>().At(new Vector2(1f, 0.5f), new Vector2(-120, 20), new Vector2(210, 110));
                b.Interactable = city.CanAfford(def.CostFor(1));
                b.ClickWhenDisabled = true;
            }
            else
            {
                var need = UIKit.Text(row.transform, Loc.T("city.needs_level", def.UnlockCityLevel), 30, ColorUtil.Purple, TextAnchor.MiddleCenter, false).Fit(18);
                need.rectTransform.At(new Vector2(1f, 0.5f), new Vector2(-120, 20), new Vector2(220, 100));
            }
        }

        private void DoBuild(string id)
        {
            var city = App.City;
            int before = city.CityLevel;
            var r = city.Build(_plot, id);
            if (r != BuildResult.Success)
            {
                Fail(r);
                return;
            }
            App.Audio.Play(Sfx.Purchase);
            Haptics.Big();
            App.Screens.PopModal(this);
            App.Popup.ShowReward(Loc.T("city.started", Loc.BuildingName(id)),
                Loc.T("city.ready_in", CityUi.Duration(city.ConstructionSecondsLeft(_plot))), CitySprites.Building(id));
        }

        private void ShowUpgrade()
        {
            var city = App.City;
            var def = city.BuildingAt(_plot);
            var p = city.Plot(_plot);
            Begin(Loc.BuildingName(def.Id));

            var img = UIKit.Image(_content, CitySprites.Building(def.Id), Color.white, false, "Building");
            img.rectTransform.At(UIKit.Top, new Vector2(0, -330), new Vector2(380, 380));
            img.gameObject.AddComponent<Bobber>().Amplitude = 6f;

            var lvText = UIKit.Text(_content, Loc.T("city.level_of", p.level, def.MaxLevel), 44, ColorUtil.Gold);
            lvText.rectTransform.At(UIKit.Top, new Vector2(0, -560), new Vector2(500, 70));

            if (city.IsUnderConstruction(_plot))
            {
                ShowConstruction(def, p);
                return;
            }

            var now = UIKit.Text(_content, Loc.T("city.level_short", p.level) + ":  " + CityUi.Describe(def, p.level), 34, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(20);
            now.rectTransform.At(UIKit.Top, new Vector2(0, -650), new Vector2(860, 90));

            var cost = city.UpgradeCost(_plot);
            if (cost == null)
            {
                var max = UIKit.Text(_content, Loc.T("city.max"), 60, ColorUtil.Gold);
                max.rectTransform.At(UIKit.Bottom, new Vector2(0, 160), new Vector2(700, 100));
                return;
            }

            var arrow = UIKit.Image(_content, SpriteFactory.Triangle(), ColorUtil.Mint, false, "Arrow");
            arrow.rectTransform.At(UIKit.Top, new Vector2(0, -730), new Vector2(56, 44));
            var next = UIKit.Text(_content, Loc.T("city.level_short", p.level + 1) + ":  " + CityUi.Describe(def, p.level + 1), 36, ColorUtil.Mint, TextAnchor.MiddleCenter, false).Fit(20);
            next.rectTransform.At(UIKit.Top, new Vector2(0, -810), new Vector2(860, 90));

            var costRow = CityUi.CostRow(_content, cost, city, App.Economy.Coins, 60);
            costRow.anchorMin = costRow.anchorMax = UIKit.Bottom;
            costRow.pivot = new Vector2(0.5f, 0.5f);
            costRow.anchoredPosition = new Vector2(0, 330);

            var time = UIKit.Text(_content, Loc.T("city.build_time", CityUi.Duration(def.BuildSeconds(p.level + 1))), 32, ColorUtil.InkDim, TextAnchor.MiddleCenter, false);
            time.rectTransform.At(UIKit.Bottom, new Vector2(0, 255), new Vector2(800, 50));

            var b = UIKit.Button(_content, Loc.T("city.upgrade"), ColorUtil.Mint, DoUpgrade, new Vector2(560, 160), 60);
            b.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 110), new Vector2(560, 160));
            b.Interactable = city.CanAfford(cost);
            b.ClickWhenDisabled = true;
        }

        /// <summary>Plot being built or upgraded: time left, progress and a coin speed-up.</summary>
        private void ShowConstruction(BuildingDef def, PlotData p)
        {
            var city = App.City;
            var label = UIKit.Text(_content, Loc.T("city.building_to", p.targetLevel), 40, ColorUtil.Mint, TextAnchor.MiddleCenter, false).Fit(22);
            label.rectTransform.At(UIKit.Top, new Vector2(0, -660), new Vector2(860, 70));
            var bar = UIKit.ProgressBar(_content, ColorUtil.Orange, new Vector2(700, 50));
            ((RectTransform)bar.transform.parent).At(UIKit.Top, new Vector2(0, -760), new Vector2(700, 50));
            bar.fillAmount = city.ConstructionProgress(_plot);
            var left = UIKit.Text(_content, Loc.T("city.ready_in", CityUi.Duration(city.ConstructionSecondsLeft(_plot))), 38, ColorUtil.Ink, TextAnchor.MiddleCenter, false);
            left.rectTransform.At(UIKit.Top, new Vector2(0, -840), new Vector2(800, 60));
            int cost = city.SpeedUpCost(_plot);
            var b = UIKit.Button(_content, Loc.T("city.speed_up", cost), ColorUtil.Gold, DoSpeedUp, new Vector2(620, 160), 52, SpriteFactory.Coin());
            b.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 110), new Vector2(620, 160));
            b.Interactable = App.Economy.Coins >= cost;
            b.ClickWhenDisabled = true;
        }

        private void DoSpeedUp()
        {
            var city = App.City;
            int before = city.CityLevel;
            var def = city.BuildingAt(_plot);
            if (!city.SpeedUp(_plot))
            {
                Fail(BuildResult.NotEnoughCoins);
                return;
            }
            App.Audio.Play(Sfx.LevelUp);
            Haptics.Big();
            App.Screens.PopModal(this);
            App.Popup.ShowReward(Loc.T("city.upgraded", Loc.BuildingName(def.Id), city.Plot(_plot).level), CityUi.Describe(def, city.Plot(_plot).level),
                CitySprites.Building(def.Id), () => CheckLevelUp(before));
        }

        private void DoUpgrade()
        {
            var city = App.City;
            int before = city.CityLevel;
            var def = city.BuildingAt(_plot);
            var r = city.Upgrade(_plot);
            if (r != BuildResult.Success)
            {
                Fail(r);
                return;
            }
            App.Audio.Play(Sfx.Purchase);
            Haptics.Big();
            App.Screens.PopModal(this);
            App.Popup.ShowReward(Loc.T("city.started", Loc.BuildingName(def.Id)),
                Loc.T("city.ready_in", CityUi.Duration(city.ConstructionSecondsLeft(_plot))), CitySprites.Building(def.Id));
        }

        private void Fail(BuildResult r)
        {
            App.Audio.Play(Sfx.Error);
            if (r == BuildResult.NotEnoughSupplies)
                App.Popup.ShowConfirm(Loc.T("city.market"), CityUi.ResultMessage(r), Loc.T("city.market"), () => App.Market.Open());
            else
                App.Popup.ShowMessage(Loc.T("city.build"), CityUi.ResultMessage(r), SpriteFactory.Coin());
        }

        private void CheckLevelUp(int before)
        {
            int now = App.City.CityLevel;
            if (now <= before) return;
            App.Audio.Play(Sfx.Win);
            App.Popup.ShowReward(Loc.T("city.level_up", now), Loc.T("city.level_up_body"), CitySprites.Building("tower"));
        }

        public override bool OnBack()
        {
            App.Screens.PopModal(this);
            return true;
        }
    }

    /// <summary>The market: turn coins into building supplies.</summary>
    public sealed class MarketPopup : UIScreen
    {
        private RectTransform _content;
        private readonly Text[] _owned = new Text[3];
        private Text _coins;

        protected override void Build()
        {
            var card = BuildModalCard(new Vector2(920, 1180), ColorUtil.Panel, ColorUtil.Blue);
            _content = UIKit.Node("Content", card).Fill(30, 30, 30, 30);

            var close = UIKit.IconButton(_content, SpriteFactory.CloseIcon(), ColorUtil.Purple, () => App.Screens.PopModal(this), 90);
            close.GetComponent<RectTransform>().At(UIKit.TopRight, new Vector2(-30, -30), new Vector2(90, 90));
            var title = UIKit.Text(_content, Loc.T("city.market"), 70, ColorUtil.Blue).Fit(34);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -70), new Vector2(600, 100));
            var info = UIKit.Text(_content, Loc.T("market.info"), 34, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(20);
            info.rectTransform.At(UIKit.Top, new Vector2(0, -160), new Vector2(800, 60));

            _coins = UIKit.CurrencyPill(_content, SpriteFactory.Coin(), ColorUtil.Ink, out var pill, 300);
            pill.At(UIKit.Top, new Vector2(0, -250), pill.sizeDelta);

            float y = -380;
            foreach (Supply s in Enum.GetValues(typeof(Supply)))
            {
                BuildRow(s, y);
                y -= 250;
            }
        }

        private void BuildRow(Supply s, float y)
        {
            var row = UIKit.NeonCard(_content, ColorUtil.Blue, ColorUtil.PanelLight, 1.2f, false, "Row", 0.15f);
            row.rectTransform.At(UIKit.Top, new Vector2(0, y - 110), new Vector2(840, 220));

            var icon = UIKit.Image(row.transform, CitySprites.SupplyIcon(s), Color.white);
            icon.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(90, 0), new Vector2(130, 130));
            var name = UIKit.Text(row.transform, Loc.SupplyName(s), 42, ColorUtil.Ink, TextAnchor.MiddleLeft, false).Fit(24);
            name.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(290, 35), new Vector2(240, 60));
            var owned = UIKit.Text(row.transform, "", 34, ColorUtil.InkDim, TextAnchor.MiddleLeft, false);
            owned.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(290, -30), new Vector2(240, 50));
            _owned[(int)s] = owned;

            int price = CityCatalog.Price(s);
            var one = UIKit.Button(row.transform, "+1  " + price, ColorUtil.Mint, () => Buy(s, 1), new Vector2(180, 120), 34, SpriteFactory.Coin());
            one.GetComponent<RectTransform>().At(new Vector2(1f, 0.5f), new Vector2(-320, 0), new Vector2(180, 120));
            var five = UIKit.Button(row.transform, "+5  " + price * 5, ColorUtil.Orange, () => Buy(s, 5), new Vector2(200, 120), 34, SpriteFactory.Coin());
            five.GetComponent<RectTransform>().At(new Vector2(1f, 0.5f), new Vector2(-115, 0), new Vector2(200, 120));
        }

        public void Open()
        {
            Refresh();
            if (App.Screens.TopModal != this) App.Screens.PushModal(this);
        }

        public override void Refresh()
        {
            _coins.text = Loc.Number(App.Economy.Coins);
            foreach (Supply s in Enum.GetValues(typeof(Supply)))
                _owned[(int)s].text = "× " + Loc.Number(App.City.Amount(s));
        }

        private void Buy(Supply s, int amount)
        {
            if (!App.City.BuySupply(s, amount))
            {
                App.Audio.Play(Sfx.Error);
                App.Popup.ShowMessage(Loc.T("city.market"), Loc.T("city.not_enough_coins"), SpriteFactory.Coin());
                return;
            }
            App.Audio.Play(Sfx.Purchase);
            UIKit.Punch(_owned[(int)s].transform);
            Refresh();
        }

        public override bool OnBack()
        {
            App.Screens.PopModal(this);
            return true;
        }
    }
}
