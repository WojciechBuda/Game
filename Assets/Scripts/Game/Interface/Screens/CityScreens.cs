using System;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.City;
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
            int i = Mathf.Clamp(level, 1, def.MaxLevel) - 1;
            var parts = new System.Collections.Generic.List<string>();
            if (def.Produces != Product.None)
            {
                string what = def.Produces == Product.Coins ? Loc.Coins(def.PerHour[i])
                    : def.PerHour[i] + " " + Loc.SupplyName((Supply)((int)def.Produces - 2));
                parts.Add(Loc.T("city.produces", what));
            }
            if (def.Population[i] > 0) parts.Add(Loc.T("city.people", def.Population[i]));
            if (def.CoinBonusPercent[i] > 0) parts.Add(Loc.T("city.bonus_short", def.CoinBonusPercent[i]));
            return string.Join("  ·  ", parts);
        }

        public static string ResultMessage(BuildResult r) =>
            r == BuildResult.NotEnoughCoins ? Loc.T("city.not_enough_coins")
            : r == BuildResult.NotEnoughSupplies ? Loc.T("city.not_enough_supplies")
            : r.ToString();
    }

    /// <summary>
    /// The city: 12 plots on a grassy field. Empty plots open the build menu, built ones collect what they
    /// made (or open the upgrade menu when there is nothing to collect). Locked plots open with city level.
    /// </summary>
    public sealed class CityScreen : UIScreen
    {
        private const int Columns = 3;
        private const float Cell = 310f;
        private const float Gap = 18f;

        private ResourceBar _resources;
        private Text _title;
        private Text _population;
        private Image _populationBar;
        private Text _bonus;
        private CandyButton _collect;
        private RectTransform _grid;
        private float _refreshTimer;

        protected override void Build()
        {
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
            _bonus.rectTransform.At(UIKit.Top, new Vector2(300, -305), new Vector2(360, 60));

            // Grassy field holding the plots; scrolls on short screens.
            var field = UIKit.Panel(Root, PixelCanvas.WithAlpha(ColorUtil.Grass, 0.55f), 0.8f, true, "Field");
            field.rectTransform.Fill(24, 230, 24, 380);
            field.gameObject.AddComponent<RectMask2D>();
            _grid = UIKit.Node("Plots", field.transform);
            _grid.anchorMin = new Vector2(0, 1);
            _grid.anchorMax = new Vector2(1, 1);
            _grid.pivot = new Vector2(0.5f, 1f);
            int rows = Mathf.CeilToInt(CityCatalog.MaxPlots / (float)Columns);
            _grid.offsetMin = _grid.offsetMax = Vector2.zero;
            _grid.sizeDelta = new Vector2(0, rows * (Cell + Gap) + Gap);
            var scroll = field.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = field.rectTransform;
            scroll.content = _grid;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;

            _collect = UIKit.Button(Root, Loc.T("city.collect"), ColorUtil.Gold, CollectAll, new Vector2(470, 150), 54, SpriteFactory.Coin());
            _collect.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-255, 105), new Vector2(470, 150));
            _collect.ClickWhenDisabled = true;
            var market = UIKit.Button(Root, Loc.T("city.market"), ColorUtil.Blue, () => App.Market.Open(), new Vector2(470, 150), 54, CitySprites.SupplyIcon(Supply.Brick));
            market.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(255, 105), new Vector2(470, 150));
        }

        protected override void OnShow()
        {
            _refreshTimer = 0f;
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
                int prev = CityCatalog.LevelPopulation[city.CityLevel - 1];
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
            _collect.Interactable = !city.PendingTotal().IsEmpty;
            BuildPlots();
        }

        private void Update()
        {
            // Production ticks in real time; refresh the storage bars now and then.
            _refreshTimer += Time.unscaledDeltaTime;
            if (_refreshTimer < 5f) return;
            _refreshTimer = 0f;
            if (!App.Screens.HasModal) Refresh();
        }

        private void BuildPlots()
        {
            UIKit.ClearChildren(_grid);
            var city = App.City;
            float width = Columns * Cell + (Columns - 1) * Gap;
            for (int i = 0; i < CityCatalog.MaxPlots; i++)
            {
                int col = i % Columns, row = i / Columns;
                var pos = new Vector2(-width / 2f + Cell / 2f + col * (Cell + Gap), -(Gap + Cell / 2f + row * (Cell + Gap)));
                BuildPlot(i, pos, city);
            }
        }

        private void BuildPlot(int index, Vector2 pos, CityService city)
        {
            bool unlocked = city.IsPlotUnlocked(index);
            var def = city.BuildingAt(index);
            Color color = !unlocked ? ColorUtil.Hex("#D6DAEC") : def == null ? ColorUtil.Hex("#C8EFC0") : ColorUtil.Hex("#E9F8E4");
            int plot = index;
            var tile = UIKit.Button(_grid, "", color, () => OnPlot(plot), new Vector2(Cell, Cell), 40, null, "Plot" + index);
            tile.GetComponent<RectTransform>().At(UIKit.Top, pos, new Vector2(Cell, Cell));
            var face = tile.Face.transform;

            if (!unlocked)
            {
                var lockIcon = UIKit.Image(face, SpriteFactory.LockIcon(), PixelCanvas.WithAlpha(ColorUtil.Ink, 0.45f));
                lockIcon.rectTransform.At(UIKit.Center, new Vector2(0, 30), new Vector2(100, 100));
                var t = UIKit.Text(face, Loc.T("city.locked_plot", city.PlotUnlockLevel(index)), 28, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(18);
                t.rectTransform.At(UIKit.Center, new Vector2(0, -70), new Vector2(Cell - 30, 80));
                tile.ClickWhenDisabled = true;
                tile.Interactable = false;
                return;
            }

            if (def == null)
            {
                var plus = UIKit.Text(face, "+", 140, ColorUtil.Mint);
                plus.rectTransform.At(UIKit.Center, new Vector2(0, 30), new Vector2(160, 160));
                var t = UIKit.Text(face, Loc.T("city.build"), 36, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(20);
                t.rectTransform.At(UIKit.Center, new Vector2(0, -90), new Vector2(Cell - 30, 56));
                return;
            }

            var p = city.Plot(index);
            var img = UIKit.Image(face, CitySprites.Building(def.Id), Color.white, false, "Building");
            img.rectTransform.At(UIKit.Center, new Vector2(0, 22), new Vector2(Cell - 50, Cell - 50));

            var lv = UIKit.Text(face, Loc.T("city.level_short", p.level), 30, ColorUtil.Purple, TextAnchor.MiddleLeft, true);
            lv.rectTransform.At(UIKit.TopLeft, new Vector2(80, -34), new Vector2(140, 48));

            if (def.Produces != Product.None)
            {
                int pending = city.Pending(index);
                float fill = city.StorageFill(index);
                var bar = UIKit.ProgressBar(face, fill >= 1f ? ColorUtil.Orange : ColorUtil.Mint, new Vector2(Cell - 70, 26));
                bar.fillAmount = fill;
                bar.transform.parent.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 32), new Vector2(Cell - 70, 26));
                if (pending > 0)
                {
                    // Bubble with what's ready, like most city games.
                    var bubble = UIKit.NeonCard(face, fill >= 1f ? ColorUtil.Orange : ColorUtil.Mint, ColorUtil.Panel, 2f, false, "Ready", 0.3f);
                    bubble.rectTransform.At(UIKit.TopRight, new Vector2(-70, -40), new Vector2(130, 64));
                    var icon = UIKit.Image(bubble.transform, CityUi.ProductIcon(def.Produces), Color.white);
                    icon.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(30, 0), new Vector2(46, 46));
                    var amount = UIKit.Text(bubble.transform, fill >= 1f ? Loc.T("city.full") : Loc.Number(pending), 28, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(14);
                    amount.rectTransform.Fill(52, 0, 8, 0);
                    var bob = bubble.gameObject.AddComponent<Bobber>();
                    bob.Amplitude = 5f;
                    bob.Speed = 3f;
                    bob.Phase = index;
                }
            }
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
            if (city.Pending(index) > 0)
            {
                var h = city.CollectPlot(index);
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
            var row = UIKit.NeonCard(list, levelOk ? ColorUtil.Mint : ColorUtil.Hex("#C9CDE6"), ColorUtil.Panel, 1.2f, false, "Row", 0.2f);
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

            var cost = CityUi.CostRow(row.transform, def.Costs[0], city, App.Economy.Coins, 44);
            cost.anchorMin = cost.anchorMax = new Vector2(0f, 0f);
            cost.pivot = new Vector2(0f, 0.5f);
            cost.anchoredPosition = new Vector2(220, 45);

            string id = def.Id;
            if (levelOk)
            {
                var b = UIKit.Button(row.transform, Loc.T("city.build"), ColorUtil.Mint, () => DoBuild(id), new Vector2(210, 110), 36);
                b.GetComponent<RectTransform>().At(new Vector2(1f, 0.5f), new Vector2(-120, 20), new Vector2(210, 110));
                b.Interactable = city.CanAfford(def.Costs[0]);
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
            App.Popup.ShowReward(Loc.T("city.built", Loc.BuildingName(id)), CityUi.Describe(CityCatalog.Get(id), 1), CitySprites.Building(id),
                () => CheckLevelUp(before));
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

            var stars = StarRow.Create(_content, 70, 10);
            stars.Set(p.level);
            ((RectTransform)stars.transform).At(UIKit.Top, new Vector2(0, -560), new Vector2(240, 70));

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

            var b = UIKit.Button(_content, Loc.T("city.upgrade"), ColorUtil.Mint, DoUpgrade, new Vector2(560, 160), 60);
            b.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(0, 150), new Vector2(560, 160));
            b.Interactable = city.CanAfford(cost);
            b.ClickWhenDisabled = true;
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
            App.Audio.Play(Sfx.LevelUp);
            Haptics.Big();
            App.Screens.PopModal(this);
            App.Popup.ShowReward(Loc.T("city.upgraded", Loc.BuildingName(def.Id), city.Plot(_plot).level), CityUi.Describe(def, city.Plot(_plot).level),
                CitySprites.Building(def.Id), () => CheckLevelUp(before));
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
