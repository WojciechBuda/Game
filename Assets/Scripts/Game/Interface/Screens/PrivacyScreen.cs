using JellyDrop.Core;
using JellyDrop.Monetization;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>
    /// The privacy policy, readable inside the game. On first launch it opens once with an OK button;
    /// from Settings it can be read any time (with a link to the online version).
    /// </summary>
    public sealed class PrivacyScreen : UIScreen
    {
        public static int PolicyVersion => PrivacyPolicyText.Version;

        /// <summary>Opened automatically on first launch (shows the OK button and the short intro).</summary>
        public bool FirstRun;

        private RectTransform _content;
        private ScrollRect _scroll;
        private Text _intro;
        private CandyButton _ok;
        private CandyButton _close;

        protected override void Build()
        {
            var card = BuildModalCard(new Vector2(960, 1720), ColorUtil.Panel, ColorUtil.Mint);
            var title = UIKit.Text(card, Loc.T("settings.privacy_policy"), 60, ColorUtil.Mint).Fit(30);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -80), new Vector2(820, 90));
            _intro = UIKit.Text(card, Loc.T("privacy.intro"), 32, ColorUtil.Ink, TextAnchor.MiddleCenter, true).Fit(18);
            _intro.rectTransform.At(UIKit.Top, new Vector2(0, -175), new Vector2(840, 100));

            var viewport = UIKit.Panel(card, new Color(0, 0, 0, 0.18f), 0.6f, true, "Viewport");
            viewport.rectTransform.Fill(40, 250, 40, 240);
            viewport.gameObject.AddComponent<RectMask2D>();
            _content = UIKit.Node("Content", viewport.transform);
            _content.anchorMin = new Vector2(0, 1);
            _content.anchorMax = new Vector2(1, 1);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.offsetMin = _content.offsetMax = Vector2.zero;
            var layout = _content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 24, 40);
            layout.spacing = 14;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            _scroll = viewport.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = viewport.rectTransform;
            _scroll.content = _content;
            _scroll.horizontal = false;

            var online = UIKit.Button(card, Loc.T("privacy.online"), ColorUtil.Blue, () => Application.OpenURL(MonetizationIds.PrivacyPolicyUrl), new Vector2(400, 110), 34);
            online.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(-220, 110), new Vector2(400, 110));
            _ok = UIKit.Button(card, Loc.T("privacy.accept"), ColorUtil.Mint, Accept, new Vector2(400, 110), 40, SpriteFactory.CheckIcon());
            _ok.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(220, 110), new Vector2(400, 110));
            _close = UIKit.Button(card, Loc.T("common.close"), ColorUtil.Purple, Accept, new Vector2(400, 110), 40);
            _close.GetComponent<RectTransform>().At(UIKit.Bottom, new Vector2(220, 110), new Vector2(400, 110));
        }

        protected override void OnShow()
        {
            _intro.gameObject.SetActive(FirstRun);
            _ok.gameObject.SetActive(FirstRun);
            _close.gameObject.SetActive(!FirstRun);
            Fill();
        }

        private void Fill()
        {
            UIKit.ClearChildren(_content);
            bool polish = Loc.Current == Language.Polish;
            var sections = polish ? PrivacyPolicyText.Polish : PrivacyPolicyText.English;
            Paragraph((polish ? "Obowiązuje od: " + PrivacyPolicyText.DatePl : "Effective: " + PrivacyPolicyText.DateEn), 28, ColorUtil.InkDim);
            if (!polish && Loc.Current != Language.English) Paragraph(Loc.T("privacy.english_only"), 28, ColorUtil.InkDim);
            foreach (var s in sections)
            {
                Paragraph(s[0], 38, ColorUtil.Mint);
                for (int i = 1; i < s.Length; i++) Paragraph(s[i], 29, ColorUtil.Ink);
            }
            _content.anchoredPosition = Vector2.zero;
        }

        private void Paragraph(string text, int size, Color color)
        {
            var t = UIKit.Text(_content, text, size, color, TextAnchor.UpperLeft, false);
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void Accept()
        {
            if (App.Economy.Data.privacyVersionSeen < PolicyVersion)
            {
                App.Economy.Data.privacyVersionSeen = PolicyVersion;
                App.Save.Save(App.Economy.Data);
            }
            App.Screens.PopModal(this);
        }

        public override bool OnBack()
        {
            Accept();
            return true;
        }
    }
}
