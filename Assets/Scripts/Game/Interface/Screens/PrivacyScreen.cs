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
            var card = BuildModalCard(new Vector2(980, 1760), ColorUtil.Panel, ColorUtil.Mint);
            // Solid backdrop: the policy must be easy to read, nothing should show through.
            var solid = Root.Find("Dim").GetComponent<Image>();
            solid.color = new Color(0.03f, 0.035f, 0.09f, 0.96f);
            var title = UIKit.Text(card, Loc.T("settings.privacy_policy"), 60, ColorUtil.Mint).Fit(30);
            title.rectTransform.At(UIKit.Top, new Vector2(0, -80), new Vector2(820, 90));
            _intro = UIKit.Text(card, Loc.T("privacy.intro"), 32, ColorUtil.Ink, TextAnchor.MiddleCenter, true).Fit(18);
            _intro.rectTransform.At(UIKit.Top, new Vector2(0, -175), new Vector2(840, 100));

            var viewport = UIKit.Panel(card, ColorUtil.Hex("#14163A"), 0.6f, true, "Viewport");
            viewport.rectTransform.Fill(36, 240, 36, 240);
            viewport.gameObject.AddComponent<RectMask2D>();
            _content = UIKit.Node("Content", viewport.transform);
            _content.anchorMin = new Vector2(0, 1);
            _content.anchorMax = new Vector2(1, 1);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.offsetMin = _content.offsetMax = Vector2.zero;
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

        private float _y;
        private const float Pad = 34f;

        private void Fill()
        {
            UIKit.ClearChildren(_content);
            _y = -28f;
            bool polish = Loc.Current == Language.Polish;
            var sections = polish ? PrivacyPolicyText.Polish : PrivacyPolicyText.English;
            Paragraph((polish ? "Obowiązuje od: " + PrivacyPolicyText.DatePl : "Effective: " + PrivacyPolicyText.DateEn), 28, ColorUtil.InkDim);
            if (!polish && Loc.Current != Language.English) Paragraph(Loc.T("privacy.english_only"), 28, ColorUtil.InkDim);
            foreach (var s in sections)
            {
                _y -= 18f;
                Paragraph(s[0], 40, ColorUtil.Mint);
                for (int i = 1; i < s.Length; i++) Paragraph(s[i], 31, ColorUtil.Ink);
            }
            _content.sizeDelta = new Vector2(0, -_y + 40f);
            _content.anchoredPosition = Vector2.zero;
        }

        private void Paragraph(string text, int size, Color color)
        {
            // Laid out by hand: each paragraph gets the full width and exactly the height its text needs.
            var t = UIKit.Text(_content, text, size, color, TextAnchor.UpperLeft, false);
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.resizeTextForBestFit = false;
            t.lineSpacing = 1.12f;
            var rt = t.rectTransform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            float width = ((RectTransform)_content.parent).rect.width - 2 * Pad;
            var settings = t.GetGenerationSettings(new Vector2(Mathf.Max(100f, width), 0f));
            float h = t.cachedTextGeneratorForLayout.GetPreferredHeight(text, settings) / t.pixelsPerUnit;
            rt.anchoredPosition = new Vector2(0, _y);
            rt.sizeDelta = new Vector2(-2 * Pad, h + 4f);
            _y -= h + 16f;
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
