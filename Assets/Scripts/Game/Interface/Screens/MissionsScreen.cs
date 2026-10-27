using JellyDrop.Core;
using JellyDrop.Audio;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>Daily gift streak, three daily missions and the free-coins video.</summary>
    public sealed class MissionsScreen : UIScreen
    {
        private RectTransform _list;
        private Text _coins;

        protected override void Build()
        {
            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 120);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(84, -84), new Vector2(120, 120));
            var title = UIKit.Text(Root, Loc.T("home.missions"), 86, ColorUtil.Purple).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -84), new Vector2(600, 120));
            _coins = UIKit.CurrencyPill(Root, SpriteFactory.Coin(), ColorUtil.Ink, out var pill, 240);
            pill.At(UIKit.TopRight, new Vector2(-140, -84), pill.sizeDelta);

            _list = UIKit.Node("List", Root).Fill(30, 40, 30, 190);
        }

        public override void Refresh()
        {
            var e = App.Economy;
            _coins.text = Loc.Number(e.Coins);
            UIKit.ClearChildren(_list);

            float y = -20;

            // --- Daily gift row
            var gift = Row(ref y, 260, ColorUtil.Yellow);
            var gt = UIKit.Text(gift, Loc.T("missions.daily_gift"), 50, ColorUtil.Pink, TextAnchor.UpperLeft);
            gt.rectTransform.Fill(40, 0, 40, 26);
            int today = e.NextStreakDay;
            for (int d = 1; d <= 7; d++)
            {
                bool claimed = d < today || (d == today && !e.CanClaimStreak);
                bool isToday = d == today && e.CanClaimStreak;
                var dot = UIKit.Image(gift, SpriteFactory.Circle(), isToday ? ColorUtil.Yellow : claimed ? ColorUtil.Mint : ColorUtil.PanelLight);
                dot.rectTransform.At(new Vector2(0f, 0f), new Vector2(80 + (d - 1) * 92, 80), new Vector2(80, 80));
                UIKit.Text(dot.transform, d.ToString(), 36, claimed || isToday ? ColorUtil.Night : ColorUtil.Ink, TextAnchor.MiddleCenter, false).rectTransform.Fill();
            }
            var claim = UIKit.Button(gift, e.CanClaimStreak ? Loc.T("common.claim", e.StreakReward(today)) : Loc.T("common.tomorrow"), ColorUtil.Pink,
                () => App.Popup.ShowDailyReward(), new Vector2(300, 130), 40, SpriteFactory.Coin());
            claim.GetComponent<RectTransform>().At(new Vector2(1f, 0f), new Vector2(-180, 90), new Vector2(300, 130));
            claim.Interactable = e.CanClaimStreak;

            // --- Missions
            var header = UIKit.Text(_list, Loc.T("missions.today"), 50, ColorUtil.Ink).Fit(28);
            header.rectTransform.At(UIKit.Top, new Vector2(0, y - 50), new Vector2(900, 80));
            y -= 110;

            foreach (var (def, progress) in e.Missions())
            {
                var row = Row(ref y, 220, ColorUtil.Blue);
                var t = UIKit.Text(row, Loc.MissionTitle(def), 42, ColorUtil.Ink, TextAnchor.UpperLeft, false).Fit(26);
                t.rectTransform.Fill(40, 90, 330, 26);
                var bar = UIKit.ProgressBar(row, ColorUtil.Mint, new Vector2(560, 46));
                bar.transform.parent.GetComponent<RectTransform>().At(new Vector2(0f, 0f), new Vector2(320, 55), new Vector2(560, 46));
                bar.fillAmount = Mathf.Clamp01((float)progress.progress / def.Target);
                var pt = UIKit.Text(bar.transform.parent, $"{Loc.Number(progress.progress)} / {Loc.Number(def.Target)}", 28, ColorUtil.Ink, TextAnchor.MiddleCenter, false);
                pt.rectTransform.Fill();

                bool complete = progress.progress >= def.Target;
                string label = progress.claimed ? Loc.T("common.done") : $"+{def.RewardCoins}";
                var b = UIKit.Button(row, label, complete && !progress.claimed ? ColorUtil.Orange : ColorUtil.Blue, () =>
                {
                    int coins = App.Economy.ClaimMission(def.Id);
                    if (coins > 0)
                    {
                        App.Audio.Play(Sfx.Purchase);
                        App.Popup.ShowReward($"+{coins}", Loc.T("missions.complete"), SpriteFactory.Coin());
                    }
                }, new Vector2(280, 130), 46, SpriteFactory.Coin());
                b.GetComponent<RectTransform>().At(new Vector2(1f, 0.5f), new Vector2(-170, 0), new Vector2(280, 130));
                b.Interactable = complete && !progress.claimed;
                if (complete && !progress.claimed) b.gameObject.AddComponent<Bobber>().Amplitude = 6f;
            }

            // --- Free coins video
            y -= 20;
            var ad = Row(ref y, 220, ColorUtil.Orange);
            var at = UIKit.Text(ad, Loc.T("missions.free_coins"), 48, ColorUtil.Orange, TextAnchor.UpperLeft);
            at.rectTransform.Fill(40, 0, 360, 30);
            int left = e.RewardedAdsLeftToday;
            var info = UIKit.Text(ad, left > 0 ? Loc.T("missions.watch_left", left) : Loc.T("missions.back_tomorrow"), 34, ColorUtil.Ink, TextAnchor.LowerLeft, false).Fit(20);
            info.rectTransform.Fill(40, 30, 330, 95);
            var watch = UIKit.Button(ad, $"+{e.Config.RewardedAdCoins}", ColorUtil.Yellow, () => App.WatchAdForCoins(), new Vector2(280, 130), 46, SpriteFactory.AdIcon());
            watch.GetComponent<RectTransform>().At(new Vector2(1f, 0.5f), new Vector2(-170, 0), new Vector2(280, 130));
            watch.Interactable = left > 0;
        }

        private RectTransform Row(ref float y, float height, Color accent)
        {
            var row = UIKit.NeonCard(_list, accent, ColorUtil.Panel, 1.1f, false, "Row", 0.25f);
            var rt = row.rectTransform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(0, y - height);
            rt.offsetMax = new Vector2(0, y);
            y -= height + 26;
            return rt;
        }

        public override bool OnBack()
        {
            App.GoHome();
            return true;
        }
    }
}
