using JellyDrop.Core;
using System;
using JellyDrop.Util;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>
    /// Builds UGUI elements from code in a consistent candy style.
    /// Reference resolution is 1080 x 1920 (portrait).
    /// </summary>
    public static class UIKit
    {
        public static readonly Vector2 Reference = new Vector2(1080, 1920);

        /// <summary>Plays the button sound. Set by the App.</summary>
        public static Action ClickSound;

        private static Font _font;

        /// <summary>
        /// Uses Assets/Resources/Fonts/MainFont.ttf if present (drop in a rounded font such as
        /// Fredoka or Baloo 2 from Google Fonts, OFL licence), otherwise Unity's built-in font.
        /// </summary>
        public static Font Font
        {
            get
            {
                if (_font != null) return _font;
                _font = Resources.Load<Font>("Fonts/MainFont");
                if (_font == null)
                {
                    try { _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { /* older Unity */ }
                }
                if (_font == null)
                {
                    try { _font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { /* newer Unity */ }
                }
                return _font;
            }
        }

        // ------------------------------------------------------------------ layout helpers

        public static RectTransform Node(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>Stretches to fill the parent with optional insets (left, bottom, right, top).</summary>
        public static RectTransform Fill(this RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        /// <summary>Anchors at a normalised point of the parent, positioned relative to it, with a fixed size.</summary>
        public static RectTransform At(this RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        public static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        public static readonly Vector2 Top = new Vector2(0.5f, 1f);
        public static readonly Vector2 Bottom = new Vector2(0.5f, 0f);
        public static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        public static readonly Vector2 TopRight = new Vector2(1f, 1f);

        public static void ClearChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }

        // ------------------------------------------------------------------ primitives

        public static Image Image(Transform parent, Sprite sprite, Color color, bool raycast = false, string name = "Image")
        {
            var rt = Node(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            img.preserveAspect = sprite != null && sprite.border == Vector4.zero;
            return img;
        }

        /// <summary>Rounded rectangle panel. <paramref name="roundness"/> &gt; 1 makes corners smaller.</summary>
        public static Image Panel(Transform parent, Color color, float roundness = 1f, bool raycast = true, string name = "Panel")
        {
            var img = Image(parent, SpriteFactory.RoundedRect(), color, raycast, name);
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = roundness;
            img.preserveAspect = false;
            return img;
        }

        /// <summary>Dark card with a glowing neon border. Returns the card (add content to it).</summary>
        public static Image NeonCard(Transform parent, Color accent, Color? fill = null, float roundness = 1f,
            bool raycast = true, string name = "Card", float glow = 0.35f)
        {
            var card = Panel(parent, fill ?? ColorUtil.Panel, roundness, raycast, name);
            var halo = Image(card.transform, SpriteFactory.Glow(), PixelCanvas.WithAlpha(accent, glow), false, "Glow");
            halo.type = UnityEngine.UI.Image.Type.Sliced;
            halo.pixelsPerUnitMultiplier = roundness;
            halo.preserveAspect = false;
            float pad = 30f / roundness; // glow sprite's inset, in canvas units at this roundness
            halo.rectTransform.Fill(-pad, -pad, -pad, -pad);
            var ring = Image(card.transform, SpriteFactory.RoundedRing(), accent, false, "Ring");
            ring.type = UnityEngine.UI.Image.Type.Sliced;
            ring.pixelsPerUnitMultiplier = roundness;
            ring.preserveAspect = false;
            ring.rectTransform.Fill();
            return card;
        }

        public static Text Text(Transform parent, string text, int size, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter, bool outline = true, string name = "Text")
        {
            var rt = Node(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = FontStyle.Bold;
            t.color = color;
            t.alignment = anchor;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            if (outline)
            {
                // Neon glow: a dark shadow for contrast, then a soft halo in the text's own colour.
                var s = rt.gameObject.AddComponent<Shadow>();
                s.effectColor = PixelCanvas.WithAlpha(ColorUtil.Night, 0.7f);
                s.effectDistance = new Vector2(0f, -5f);
                var o = rt.gameObject.AddComponent<Outline>();
                o.effectColor = PixelCanvas.WithAlpha(color, 0.35f);
                o.effectDistance = new Vector2(3f, 3f);
            }
            return t;
        }

        /// <summary>
        /// Shrinks the text to fit its rect (down to <paramref name="minSize"/>), so longer translations
        /// don't overflow. Use on labels with a fixed size.
        /// </summary>
        public static Text Fit(this Text text, int minSize = 0)
        {
            text.resizeTextForBestFit = true;
            text.resizeTextMaxSize = text.fontSize;
            text.resizeTextMinSize = minSize > 0 ? minSize : Mathf.Max(14, Mathf.RoundToInt(text.fontSize * 0.55f));
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        public static CandyButton Button(Transform parent, string label, Color color, Action onClick, Vector2 size,
            int fontSize = 56, Sprite icon = null, string name = "Button")
        {
            var rt = Node(name, parent);
            rt.sizeDelta = size;
            var b = rt.gameObject.AddComponent<CandyButton>();
            b.Build(label, color, fontSize, icon);
            b.OnClick = onClick;
            return b;
        }

        /// <summary>Small round icon button (pause, settings, close).</summary>
        public static CandyButton IconButton(Transform parent, Sprite icon, Color color, Action onClick, float size = 130f)
        {
            var b = Button(parent, null, color, onClick, new Vector2(size, size), 40, icon, "IconButton");
            return b;
        }

        /// <summary>Pill with coin icon and number; returns the number text.</summary>
        public static Text CurrencyPill(Transform parent, Sprite icon, Color tint, out RectTransform pill, float width = 300f)
        {
            pill = Node("Pill", parent);
            pill.sizeDelta = new Vector2(width, 96);
            var bg = NeonCard(pill, ColorUtil.Blue, PixelCanvas.WithAlpha(ColorUtil.Panel, 0.9f), 2f, false, "Pill", 0.2f);
            bg.rectTransform.Fill();
            var ic = Image(pill, icon, Color.white);
            ic.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(44, 2), new Vector2(84, 84));
            var t = Text(pill, "0", 52, tint, TextAnchor.MiddleCenter, false);
            t.rectTransform.Fill(80, 0, 16, 0);
            return t;
        }

        /// <summary>Horizontal progress bar; returns the fill image (set fillAmount).</summary>
        public static Image ProgressBar(Transform parent, Color fill, Vector2 size)
        {
            var bg = Panel(parent, new Color(1f, 1f, 1f, 0.08f), 2.5f, false, "Bar");
            bg.rectTransform.sizeDelta = size;
            var f = Panel(bg.transform, fill, 2.5f, false, "Fill");
            f.rectTransform.Fill(4, 4, 4, 4);
            f.type = UnityEngine.UI.Image.Type.Filled;
            f.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            f.fillAmount = 0f;
            return f;
        }

        public static Color RarityColor(JellyDrop.Core.Economy.Rarity rarity)
        {
            switch (rarity)
            {
                case JellyDrop.Core.Economy.Rarity.Rare: return ColorUtil.Blue;
                case JellyDrop.Core.Economy.Rarity.Epic: return ColorUtil.Purple;
                case JellyDrop.Core.Economy.Rarity.Premium: return ColorUtil.Gold;
                default: return ColorUtil.Mint;
            }
        }

        // ------------------------------------------------------------------ animation helpers

        public static void Pop(Transform t, float from = 0.6f, float duration = 0.35f)
        {
            t.localScale = Vector3.one * from;
            Tweener.Run(duration, k => { if (t != null) t.localScale = Vector3.one * Mathf.LerpUnclamped(from, 1f, k); }, Ease.OutBack, owner: t);
        }

        public static void Punch(Transform t, float amount = 0.15f, float duration = 0.3f)
        {
            Tweener.Run(duration, k => { if (t != null) t.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI) * amount); }, Ease.Linear, owner: t);
        }

        /// <summary>Animates a number text counting up.</summary>
        public static void CountUp(Text text, int from, int to, float duration = 0.8f, string prefix = "", string suffix = "")
        {
            Tweener.Run(duration, k =>
            {
                if (text != null) text.text = prefix + Loc.Number(Mathf.RoundToInt(Mathf.Lerp(from, to, k))) + suffix;
            }, Ease.OutCubic, owner: text);
        }

        public static string FormatTime(float seconds)
        {
            int m = (int)(seconds / 60f);
            float s = seconds - m * 60;
            return $"{m}:{s:00.0}";
        }
    }

    /// <summary>
    /// Neon arcade button: dark face with a glowing coloured border and halo, a coloured base for depth.
    /// Squishes when pressed and plays a click.
    /// </summary>
    public sealed class CandyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler
    {
        public Action OnClick;
        /// <summary>Still fires OnClick when disabled (e.g. a dimmed price button that explains what's missing).</summary>
        public bool ClickWhenDisabled;
        public Image Face { get; private set; }
        public Image Base { get; private set; }
        public Text Label { get; private set; }
        public Image Icon { get; private set; }
        private Image _ring;
        private Image _glow;
        private Image _shine;
        private RectTransform _faceRt;
        private Image _badge;
        private Text _badgeText;
        private bool _interactable = true;
        private bool _pressed;
        private Color _color;
        private const float Depth = 14f;

        public void Build(string label, Color color, int fontSize, Sprite icon)
        {
            var rt = (RectTransform)transform;

            _glow = UIKit.Image(rt, SpriteFactory.Glow(), color, false, "Glow");
            _glow.type = Image.Type.Sliced;
            _glow.pixelsPerUnitMultiplier = 1.4f;
            _glow.preserveAspect = false;
            _glow.rectTransform.Fill(-21.4f, -21.4f, -21.4f, -21.4f); // 30 px inset / 1.4 roundness

            Base = UIKit.Panel(rt, color, 1.4f, true, "Base");
            Base.rectTransform.Fill();

            Face = UIKit.Panel(rt, color, 1.4f, false, "Face");
            _faceRt = Face.rectTransform;
            _faceRt.Fill(0, Depth, 0, 0);

            _shine = UIKit.Panel(_faceRt, Color.white, 2.2f, false, "Shine");
            _shine.rectTransform.anchorMin = new Vector2(0.06f, 0.6f);
            _shine.rectTransform.anchorMax = new Vector2(0.94f, 0.9f);
            _shine.rectTransform.offsetMin = _shine.rectTransform.offsetMax = Vector2.zero;

            _ring = UIKit.Image(_faceRt, SpriteFactory.RoundedRing(), color, false, "Ring");
            _ring.type = Image.Type.Sliced;
            _ring.pixelsPerUnitMultiplier = 1.4f;
            _ring.preserveAspect = false;
            _ring.rectTransform.Fill();

            if (icon != null)
            {
                Icon = UIKit.Image(_faceRt, icon, Color.white, false, "Icon");
                if (string.IsNullOrEmpty(label))
                {
                    Icon.rectTransform.Fill(18, 18, 18, 18);
                }
                else
                {
                    float h = rt.sizeDelta.y - Depth;
                    Icon.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(h * 0.5f + 12, 0), new Vector2(h * 0.62f, h * 0.62f));
                }
            }

            if (!string.IsNullOrEmpty(label))
            {
                Label = UIKit.Text(_faceRt, label, fontSize, Color.white);
                Label.rectTransform.Fill(icon != null ? rt.sizeDelta.y * 0.75f : 16, 4, 16, 0);
                Label.resizeTextForBestFit = true;
                Label.resizeTextMinSize = 20;
                Label.resizeTextMaxSize = fontSize;
                Label.horizontalOverflow = HorizontalWrapMode.Wrap;
                Label.verticalOverflow = VerticalWrapMode.Truncate;
            }
            _color = color;
            ApplyColors();
        }

        private void ApplyColors()
        {
            var c = _interactable ? _color : Color.Lerp(_color, ColorUtil.InkDim, 0.75f);
            float alpha = _interactable ? 1f : 0.55f;
            Face.color = Color.Lerp(ColorUtil.Panel, c, 0.32f);
            Base.color = PixelCanvas.Darken(c, 0.35f);
            _ring.color = PixelCanvas.WithAlpha(PixelCanvas.Lighten(c, 0.15f), alpha);
            _glow.color = PixelCanvas.WithAlpha(c, _interactable ? 0.45f : 0f);
            _shine.color = new Color(1f, 1f, 1f, _interactable ? 0.08f : 0.03f);
            if (Icon != null) Icon.color = new Color(1f, 1f, 1f, alpha);
            if (Label != null)
            {
                Label.color = new Color(1f, 1f, 1f, _interactable ? 1f : 0.6f);
                var halo = Label.GetComponent<Outline>();
                if (halo != null) halo.effectColor = PixelCanvas.WithAlpha(c, _interactable ? 0.55f : 0.15f);
            }
        }

        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                ApplyColors();
            }
        }

        public void SetColor(Color color)
        {
            _color = color;
            Interactable = _interactable;
        }

        public void SetLabel(string text)
        {
            if (Label != null) Label.text = text;
        }

        /// <summary>Red counter bubble in the corner (e.g. unclaimed missions). 0 hides it.</summary>
        public void SetBadge(int count)
        {
            if (_badge == null)
            {
                _badge = UIKit.Image(transform, SpriteFactory.Circle(), ColorUtil.Red, false, "Badge");
                _badge.rectTransform.At(new Vector2(1f, 1f), new Vector2(-10, -10), new Vector2(64, 64));
                _badgeText = UIKit.Text(_badge.transform, "", 38, Color.white, TextAnchor.MiddleCenter, false);
                _badgeText.rectTransform.Fill();
            }
            _badge.gameObject.SetActive(count > 0);
            _badgeText.text = count > 9 ? "9+" : count.ToString();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            _faceRt.offsetMin = new Vector2(0, Depth * 0.3f);
            _faceRt.offsetMax = new Vector2(0, -Depth * 0.7f);
            transform.localScale = Vector3.one * 0.96f;
        }

        public void OnPointerUp(PointerEventData eventData) => Release();
        public void OnPointerExit(PointerEventData eventData) { if (_pressed) Release(); }

        private void Release()
        {
            _pressed = false;
            _faceRt.offsetMin = new Vector2(0, Depth);
            _faceRt.offsetMax = Vector2.zero;
            var t = transform;
            Tweener.Run(0.25f, k => { if (t != null) t.localScale = Vector3.one * Mathf.LerpUnclamped(0.96f, 1f, k); }, Ease.OutBack, owner: this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            UIKit.ClickSound?.Invoke();
            if (!_interactable && !ClickWhenDisabled) return;
            OnClick?.Invoke();
        }
    }

    /// <summary>Keeps its RectTransform inside the device safe area (notches, rounded corners, home bar).</summary>
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private Rect _applied;
        private Vector2Int _screen;

        private void Awake() => Apply();
        private void Update()
        {
            if (Screen.safeArea != _applied || _screen.x != Screen.width || _screen.y != Screen.height) Apply();
        }

        private void Apply()
        {
            var rt = (RectTransform)transform;
            var safe = Screen.safeArea;
            _applied = safe;
            _screen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;
            rt.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rt.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }

    /// <summary>Gentle up-down bob for decorative UI (title letters, mascot).</summary>
    public sealed class Bobber : MonoBehaviour
    {
        public float Amplitude = 12f;
        public float Speed = 2f;
        public float Phase;
        public float Rotation;
        private Vector2 _base;
        private bool _init;

        private void Update()
        {
            var rt = (RectTransform)transform;
            if (!_init) { _base = rt.anchoredPosition; _init = true; }
            float t = Time.unscaledTime * Speed + Phase;
            rt.anchoredPosition = _base + new Vector2(0f, Mathf.Sin(t) * Amplitude);
            if (Rotation != 0f) rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.8f) * Rotation);
        }
    }
}
