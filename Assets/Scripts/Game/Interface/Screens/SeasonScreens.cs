using System.Collections.Generic;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.City;
using JellyDrop.Core.Economy;
using JellyDrop.Core.Loyalty;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>Icons and text for <see cref="Reward"/> bundles.</summary>
    public static class RewardUi
    {
        public static Sprite ItemIcon(ShopItem item)
        {
            if (item == null) return SpriteFactory.Star();
            switch (item.Category)
            {
                case ItemCategory.Skin: return SpriteFactory.Block(item.BlockStyle, ColorUtil.Hex(item.Palette[2]));
                case ItemCategory.Avatar: return SpriteFactory.Avatar(item, AvatarMood.Happy);
                default: return SpriteFactory.Gradient(ColorUtil.Hex(item.BackgroundTop), ColorUtil.Hex(item.Accent));
            }
        }

        /// <summary>The parts of a reward as (icon, amount text), biggest first; items come first.</summary>
        public static List<(Sprite icon, string text)> Parts(Reward r, Catalog catalog)
        {
            var parts = new List<(Sprite, string)>();
            if (r == null) return parts;
            if (r.ItemId != null)
            {
                var item = catalog.Get(r.ItemId);
                parts.Add((ItemIcon(item), Loc.ItemName(item)));
            }
            if (r.Coins > 0) parts.Add((SpriteFactory.Coin(), Loc.Number(r.Coins)));
            if (r.Shards > 0) parts.Add((SpriteFactory.Shard(), r.Shards.ToString()));
            if (r.PowerUps > 0) parts.Add((SpriteFactory.PowerUpIcon(r.PowerUp), "×" + r.PowerUps));
            if (r.Wood > 0) parts.Add((CitySprites.SupplyIcon(Supply.Wood), r.Wood.ToString()));
            if (r.Brick > 0) parts.Add((CitySprites.SupplyIcon(Supply.Brick), r.Brick.ToString()));
            if (r.Glass > 0) parts.Add((CitySprites.SupplyIcon(Supply.Glass), r.Glass.ToString()));
            return parts;
        }

        /// <summary>One line for popups: "+150 coins, +3 shards, Bubble Pop".</summary>
        public static string Describe(Reward r, Catalog catalog)
        {
            if (r == null) return "";
            var bits = new List<string>();
            if (r.Coins > 0) bits.Add("+" + Loc.Coins(r.Coins));
            if (r.Shards > 0) bits.Add("+" + Loc.T("reward.shards", r.Shards));
            if (r.PowerUps > 0) bits.Add("+" + r.PowerUps + " " + Loc.PowerUpName(r.PowerUp));
            if (r.Wood > 0) bits.Add("+" + r.Wood + " " + Loc.SupplyName(Supply.Wood));
            if (r.Brick > 0) bits.Add("+" + r.Brick + " " + Loc.SupplyName(Supply.Brick));
            if (r.Glass > 0) bits.Add("+" + r.Glass + " " + Loc.SupplyName(Supply.Glass));
            if (r.ItemId != null) bits.Add(Loc.ItemName(catalog.Get(r.ItemId)));
            return string.Join(", ", bits);
        }

        /// <summary>Draws up to three reward parts side by side inside <paramref name="parent"/>.</summary>
        public static void Draw(RectTransform parent, Reward r, Catalog catalog, float width, float height)
        {
            var parts = Parts(r, catalog);
            if (parts.Count == 0) return;
            if (parts[0].icon != null && r.ItemId != null)
            {
                // Items get the whole cell: a big preview and the name.
                var img = UIKit.Image(parent, parts[0].icon, Color.white, false, "Item");
                img.rectTransform.At(UIKit.Center, new Vector2(0, height * 0.12f), new Vector2(height * 0.55f, height * 0.55f));
                var name = UIKit.Text(parent, parts[0].text, Mathf.RoundToInt(height * 0.17f), ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(14);
                name.rectTransform.At(UIKit.Center, new Vector2(0, -height * 0.3f), new Vector2(width - 20, height * 0.24f));
                return;
            }
            int n = Mathf.Min(3, parts.Count);
            float slot = width / n;
            float icon = Mathf.Min(slot * 0.55f, height * 0.5f);
            for (int i = 0; i < n; i++)
            {
                float x = -width / 2f + slot * (i + 0.5f);
                var img = UIKit.Image(parent, parts[i].icon, Color.white, false, "Icon");
                img.rectTransform.At(UIKit.Center, new Vector2(x, height * 0.1f), new Vector2(icon, icon));
                var t = UIKit.Text(parent, parts[i].text, Mathf.RoundToInt(height * 0.2f), ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(14);
                t.rectTransform.At(UIKit.Center, new Vector2(x, -height * 0.28f), new Vector2(slot, height * 0.28f));
            }
        }
    }

    /// <summary>Round player-level badge for the home screen: level number, XP ring-bar and coin bonus.</summary>
    public sealed class LevelBadge : MonoBehaviour
    {
        private Text _level;
        private Image _bar;
        private Text _bonus;

        public static LevelBadge Create(Transform parent, System.Action onClick)
        {
            var b = UIKit.Button(parent, "", ColorUtil.Purple, onClick, new Vector2(210, 210), 40, null, "LevelBadge");
            var badge = b.gameObject.AddComponent<LevelBadge>();
            var face = b.Face.transform;
            var lv = UIKit.Text(face, Loc.T("player.lv"), 30, Color.white, TextAnchor.MiddleCenter, false);
            lv.rectTransform.At(UIKit.Top, new Vector2(0, -36), new Vector2(180, 40));
            badge._level = UIKit.Text(face, "1", 84, Color.white, TextAnchor.MiddleCenter, false);
            badge._level.rectTransform.At(UIKit.Center, new Vector2(0, 2), new Vector2(190, 100));
            var bar = UIKit.ProgressBar(face, ColorUtil.Yellow, new Vector2(150, 22));
            ((RectTransform)bar.transform.parent).At(UIKit.Bottom, new Vector2(0, 40), new Vector2(150, 22));
            badge._bar = bar;
            badge._bonus = UIKit.Text(parent, "", 28, ColorUtil.Purple, TextAnchor.MiddleCenter, false).Fit(16);
            badge._bonus.rectTransform.sizeDelta = new Vector2(240, 40);
            return badge;
        }

        public void Place(Vector2 anchor, Vector2 pos)
        {
            ((RectTransform)transform).At(anchor, pos, new Vector2(210, 210));
            _bonus.rectTransform.At(anchor, pos + new Vector2(0, -135), new Vector2(240, 40));
        }

        public void Refresh(LoyaltyService loyalty)
        {
            _level.text = loyalty.Level.ToString();
            _bar.fillAmount = loyalty.LevelProgress;
            int bonus = loyalty.CoinBonusPercent;
            _bonus.text = bonus > 0 ? Loc.T("city.bonus_short", bonus) : "";
        }
    }

    /// <summary>
    /// The season pass: 30 tiers, a free track on the left and the pass track on the right. Points come
    /// from games, won levels and missions.
    /// </summary>
    public sealed class SeasonScreen : UIScreen
    {
        private const float RowH = 190f;
        private const float CellW = 360f;

        private Text _title, _subtitle, _ends, _tier, _points;
        private Image _bar;
        private CandyButton _buy, _claimAll;
        private ScrollRect _scroll;
        private RectTransform _list;
        private float _clock;

        protected override void Build()
        {
            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 110);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(80, -80), new Vector2(110, 110));
            _title = UIKit.Text(Root, "", 76, ColorUtil.Orange).Fit(36);
            _title.rectTransform.At(UIKit.Top, new Vector2(30, -78), new Vector2(720, 110));
            _subtitle = UIKit.Text(Root, "", 36, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(20);
            _subtitle.rectTransform.At(UIKit.Top, new Vector2(30, -152), new Vector2(720, 50));
            _ends = UIKit.Text(Root, "", 30, ColorUtil.InkDim, TextAnchor.MiddleCenter, false).Fit(18);
            _ends.rectTransform.At(UIKit.Top, new Vector2(30, -198), new Vector2(720, 44));

            // Tier progress card
            var card = UIKit.NeonCard(Root, ColorUtil.Orange, ColorUtil.Panel, 1f, false, "TierCard", 0.25f);
            card.rectTransform.At(UIKit.Top, new Vector2(0, -330), new Vector2(1020, 180));
            var circle = UIKit.Image(card.transform, SpriteFactory.Star(), ColorUtil.Gold, false, "TierStar");
            circle.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(110, 0), new Vector2(160, 160));
            _tier = UIKit.Text(circle.transform, "0", 52, Color.white, TextAnchor.MiddleCenter, true);
            _tier.rectTransform.Fill(0, 6, 0, 0);
            var label = UIKit.Text(card.transform, Loc.T("season.how"), 26, ColorUtil.InkDim, TextAnchor.MiddleLeft, false).Fit(16);
            label.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(530, 45), new Vector2(620, 70));
            _bar = UIKit.ProgressBar(card.transform, ColorUtil.Orange, new Vector2(620, 44));
            ((RectTransform)_bar.transform.parent).At(new Vector2(0f, 0.5f), new Vector2(530, -30), new Vector2(620, 44));
            _points = UIKit.Text(_bar.transform.parent, "", 28, ColorUtil.Ink, TextAnchor.MiddleCenter, false);
            _points.rectTransform.Fill();

            // Track headers
            var free = UIKit.Text(Root, Loc.T("season.free"), 38, ColorUtil.Mint, TextAnchor.MiddleCenter, true).Fit(20);
            free.rectTransform.At(UIKit.Top, new Vector2(-(CellW / 2f + 90), -462), new Vector2(CellW, 56));
            var pass = UIKit.Text(Root, Loc.T("season.pass"), 38, ColorUtil.Orange, TextAnchor.MiddleCenter, true).Fit(20);
            pass.rectTransform.At(UIKit.Top, new Vector2(CellW / 2f + 90, -462), new Vector2(CellW, 56));

            var viewport = UIKit.Panel(Root, new Color(1f, 1f, 1f, 0.35f), 0.8f, true, "Viewport");
            viewport.rectTransform.Fill(20, 250, 20, 500);
            viewport.gameObject.AddComponent<RectMask2D>();
            _list = UIKit.Node("Tiers", viewport.transform);
            _list.anchorMin = new Vector2(0, 1);
            _list.anchorMax = new Vector2(1, 1);
            _list.pivot = new Vector2(0.5f, 1f);
            _list.offsetMin = _list.offsetMax = Vector2.zero;
            _list.sizeDelta = new Vector2(0, SeasonPass.Tiers * RowH + 20);
            _scroll = viewport.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = viewport.rectTransform;
            _scroll.content = _list;
            _scroll.horizontal = false;

            _buy = UIKit.Button(Root, "", ColorUtil.Gold, OnBuy, new Vector2(560, 150), 46, SpriteFactory.Star());
            _buy.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-230, 110), new Vector2(560, 150));
            _claimAll = UIKit.Button(Root, Loc.T("season.claim_all"), ColorUtil.Mint, ClaimAll, new Vector2(400, 150), 44);
            _claimAll.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(290, 110), new Vector2(400, 150));
        }

        protected override void OnShow()
        {
            Refresh();
            ScrollToTier();
        }

        public override void Refresh()
        {
            var l = App.Loyalty;
            int season = l.Season;
            _title.text = Loc.T("season.title", season);
            _subtitle.text = Loc.T("season.name." + SeasonPass.SetFor(season));
            UpdateClock();
            _tier.text = l.Tier.ToString();
            _bar.fillAmount = l.TierProgress;
            _points.text = l.Tier >= SeasonPass.Tiers ? Loc.T("season.tier", SeasonPass.Tiers)
                : (l.Points % SeasonPass.PointsPerTier) + " / " + SeasonPass.PointsPerTier;
            _buy.SetLabel(l.HasPass ? Loc.T("season.active") : Loc.T("season.buy", App.Iap.SeasonPassPrice));
            _buy.Interactable = !l.HasPass;
            _buy.SetColor(l.HasPass ? ColorUtil.Mint : ColorUtil.Gold);
            _claimAll.Interactable = l.ClaimableCount > 0;
            _claimAll.SetBadge(l.ClaimableCount);
            BuildRows();
        }

        private void Update()
        {
            _clock += Time.unscaledDeltaTime;
            if (_clock < 30f) return;
            _clock = 0f;
            UpdateClock();
        }

        private void UpdateClock()
        {
            long left = App.Loyalty.SecondsLeft;
            _ends.text = Loc.T("season.ends", left / 86400, left % 86400 / 3600);
        }

        private void BuildRows()
        {
            UIKit.ClearChildren(_list);
            var l = App.Loyalty;
            int season = l.Season;
            for (int tier = 1; tier <= SeasonPass.Tiers; tier++)
            {
                float y = -10 - (tier - 1) * RowH - RowH / 2f;
                bool reached = tier <= l.Tier;

                // Track line and tier marker in the middle
                var line = UIKit.Panel(_list, reached ? ColorUtil.Orange : new Color(0.17f, 0.18f, 0.36f, 0.12f), 3f, false, "Line");
                line.rectTransform.At(UIKit.Top, new Vector2(0, y), new Vector2(16, RowH));
                var marker = UIKit.Image(_list, SpriteFactory.Circle(), reached ? ColorUtil.Orange : ColorUtil.Hex("#D6DAEC"), false, "Tier");
                marker.rectTransform.At(UIKit.Top, new Vector2(0, y), new Vector2(100, 100));
                var num = UIKit.Text(marker.transform, tier.ToString(), 42, reached ? Color.white : ColorUtil.InkDim, TextAnchor.MiddleCenter, false);
                num.rectTransform.Fill();

                Cell(tier, false, SeasonPass.FreeReward(season, tier), new Vector2(-(CellW / 2f + 90), y));
                Cell(tier, true, SeasonPass.PassReward(season, tier), new Vector2(CellW / 2f + 90, y));
            }
        }

        private void Cell(int tier, bool pass, Reward reward, Vector2 pos)
        {
            var l = App.Loyalty;
            bool claimed = l.IsClaimed(tier, pass);
            bool canClaim = l.CanClaim(tier, pass);
            bool locked = pass && !l.HasPass;
            Color color = claimed ? ColorUtil.Hex("#E3E6F3") : canClaim ? (pass ? ColorUtil.Gold : ColorUtil.Mint) : ColorUtil.PanelLight;
            int t = tier;
            var b = UIKit.Button(_list, "", color, () => OnCell(t, pass), new Vector2(CellW, RowH - 26), 30, null, "Cell");
            b.GetComponent<RectTransform>().At(UIKit.Top, pos, new Vector2(CellW, RowH - 26));
            var face = (RectTransform)b.Face.transform;
            RewardUi.Draw(face, reward, App.Economy.Catalog, CellW - 30, RowH - 50);

            if (claimed)
            {
                var check = UIKit.Image(face, SpriteFactory.CheckIcon(), ColorUtil.Mint, false, "Claimed");
                check.rectTransform.At(UIKit.TopRight, new Vector2(-36, -34), new Vector2(56, 56));
                foreach (var g in face.GetComponentsInChildren<Graphic>())
                    if (g != face.GetComponent<Graphic>() && g != check) g.color = new Color(g.color.r, g.color.g, g.color.b, g.color.a * 0.45f);
            }
            else if (locked)
            {
                var lockIcon = UIKit.Image(face, SpriteFactory.LockIcon(), PixelCanvas.WithAlpha(ColorUtil.Ink, 0.55f), false, "Lock");
                lockIcon.rectTransform.At(UIKit.TopRight, new Vector2(-34, -34), new Vector2(46, 46));
            }
            if (canClaim)
            {
                var bob = b.gameObject.AddComponent<Bobber>();
                bob.Amplitude = 4f;
                bob.Speed = 4f;
                bob.Phase = tier;
            }
        }

        private void OnCell(int tier, bool pass)
        {
            var l = App.Loyalty;
            if (l.CanClaim(tier, pass))
            {
                var r = l.Claim(tier, pass);
                App.Audio.Play(r.ItemId != null ? Sfx.Win : Sfx.Purchase);
                if (r.ItemId != null)
                    App.Popup.ShowReward(Loc.ItemName(App.Economy.Catalog.Get(r.ItemId)), RewardUi.Describe(r, App.Economy.Catalog), RewardUi.ItemIcon(App.Economy.Catalog.Get(r.ItemId)));
                return;
            }
            if (l.IsClaimed(tier, pass)) return;
            if (pass && !l.HasPass)
            {
                OnBuy();
                return;
            }
            App.Audio.Play(Sfx.Error);
            App.Popup.ShowMessage(Loc.T("season.tier", tier), Loc.T("season.reach", tier), SpriteFactory.Star());
        }

        private void ClaimAll()
        {
            var r = App.Loyalty.ClaimAll();
            if (r.IsEmpty) return;
            App.Audio.Play(Sfx.Win);
            App.Popup.ShowReward(Loc.T("season.claimed"), RewardUi.Describe(r, App.Economy.Catalog), SpriteFactory.Star());
        }

        private void OnBuy()
        {
            if (App.Loyalty.HasPass) return;
            App.Popup.ShowConfirm(Loc.T("season.buy_title"), Loc.T("season.buy_body"), Loc.T("season.buy", App.Iap.SeasonPassPrice), App.BuySeasonPass);
        }

        private void ScrollToTier()
        {
            Canvas.ForceUpdateCanvases();
            float viewH = ((RectTransform)_scroll.viewport).rect.height;
            float contentH = _list.sizeDelta.y;
            if (contentH <= viewH) return;
            // First tier that still has something to do.
            int target = Mathf.Clamp(App.Loyalty.Tier, 1, SeasonPass.Tiers);
            for (int t = 1; t <= App.Loyalty.Tier; t++)
                if (App.Loyalty.CanClaim(t, false) || App.Loyalty.CanClaim(t, true)) { target = t; break; }
            float y = (target - 1) * RowH;
            _scroll.verticalNormalizedPosition = 1f - Mathf.Clamp01(y / (contentH - viewH));
        }

        public override bool OnBack()
        {
            App.GoHome();
            return true;
        }
    }
}
