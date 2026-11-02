using System.Collections.Generic;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Services;
using JellyDrop.Util;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>Icons for the lucky wheel (drawn in code, cached).</summary>
    public static class WheelArt
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        private static Sprite Cached(string key, System.Action<PixelCanvas> draw, int size = 128)
        {
            if (Cache.TryGetValue(key, out var s) && s != null) return s;
            var c = new PixelCanvas(size, size);
            draw(c);
            s = c.ToSprite(size);
            s.name = "wheel_" + key;
            Cache[key] = s;
            return s;
        }

        public static Sprite Icon(WheelRewardKind kind)
        {
            switch (kind)
            {
                case WheelRewardKind.Coins: return SpriteFactory.Coin();
                case WheelRewardKind.PowerUp: return SpriteFactory.PowerUpIcon(PowerUpType.LineZap);
                case WheelRewardKind.Shard: return SpriteFactory.Shard();
                case WheelRewardKind.Supplies: return CitySprites.SupplyIcon(JellyDrop.Core.City.Supply.Brick);
                case WheelRewardKind.RunTicket: return Ticket();
                case WheelRewardKind.PetEnergy: return PetSprites.Bolt();
                case WheelRewardKind.CoinBooster: return Booster();
                default: return Chest();
            }
        }

        public static Sprite Ticket() => Cached("ticket", c =>
        {
            c.Paint((x, y) => Mathf.Max(Mathf.Max(PixelCanvas.RoundedBox(x, y, 64, 64, 54, 34, 8), -PixelCanvas.Circle(x, y, 10, 64, 12)), -PixelCanvas.Circle(x, y, 118, 64, 12)),
                (x, y, d) => Color.Lerp(ColorUtil.Hex("#FF4FA3"), ColorUtil.Hex("#FF9AD0"), y / 128f));
            for (int i = 0; i < 6; i++) { float yy = 38 + i * 10; c.Paint((x, y) => PixelCanvas.Circle(x, y, 86, yy, 2.2f), Color.white); }
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, PixelCanvas.StarPoints(50, 64, 20, 9, 5)), Color.white);
        });

        public static Sprite Booster() => Cached("booster", c =>
        {
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 64, 56), (x, y, d) => Color.Lerp(ColorUtil.Hex("#2A6BFF"), ColorUtil.Hex("#3DE0FF"), y / 128f));
            c.Paint((x, y) => Mathf.Abs(PixelCanvas.Circle(x, y, 64, 64, 52)) - 3f, new Color(1, 1, 1, 0.8f));
            // "x2": a cross and a 2 made of strokes
            c.Paint((x, y) => Mathf.Min(PixelCanvas.Segment(x, y, 30, 48, 54, 76, 6), PixelCanvas.Segment(x, y, 30, 76, 54, 48, 6)), Color.white);
            c.Paint((x, y) => Mathf.Min(Mathf.Min(
                Mathf.Max(Mathf.Abs(PixelCanvas.Circle(x, y, 82, 80, 13)) - 5f, 78 - y),
                PixelCanvas.Segment(x, y, 94, 78, 68, 44, 5.5f)),
                PixelCanvas.Segment(x, y, 68, 44, 98, 44, 5.5f)), Color.white);
        });

        public static Sprite Chest() => Cached("chest", c =>
        {
            var wood = ColorUtil.Hex("#B5662E");
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 44, 50, 30, 8), wood);
            c.Paint((x, y) => Mathf.Max(PixelCanvas.Ellipse(x, y, 64, 74, 50, 30), 74 - y), PixelCanvas.Lighten(wood, 0.15f));
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 60, 52, 5, 2), ColorUtil.Gold);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 56, 9, 12, 3), ColorUtil.Gold);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 54, 3.5f), ColorUtil.Hex("#5A2E10"));
            foreach (var p in new[] { new Vector2(40, 100), new Vector2(64, 108), new Vector2(88, 98) })
                c.Paint((x, y) => PixelCanvas.Polygon(x, y, PixelCanvas.StarPoints(p.x, p.y, 11, 4, 4)), ColorUtil.Hex("#FFF0A0"));
        });

        /// <summary>Small wheel for the home screen button.</summary>
        public static Sprite WheelIcon() => Cached("wheelicon", c =>
        {
            Color[] cols = { ColorUtil.Pink, ColorUtil.Yellow, ColorUtil.Blue, ColorUtil.Mint, ColorUtil.Purple, ColorUtil.Orange };
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 60, 52), ColorUtil.Gold);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 60, 44), (x, y, d) =>
            {
                float a = Mathf.Atan2(y - 60, x - 64) / (Mathf.PI * 2f) + 0.5f;
                return cols[Mathf.Clamp((int)(a * 6), 0, 5)];
            });
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 60, 10), Color.white);
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, new List<Vector2> { new Vector2(52, 124), new Vector2(76, 124), new Vector2(64, 102) }), ColorUtil.Red);
        });
    }

    /// <summary>Little celebration effects on the UI layer: confetti, flying icons, a flash.</summary>
    public static class UiFx
    {
        private static readonly Color[] Party = { ColorUtil.Pink, ColorUtil.Yellow, ColorUtil.Blue, ColorUtil.Mint, ColorUtil.Purple, ColorUtil.Orange };

        public static void Confetti(RectTransform layer, int count, MonoBehaviour owner)
        {
            var size = layer.rect.size;
            for (int i = 0; i < count; i++)
            {
                var img = UIKit.Image(layer, SpriteFactory.Confetti(), Party[Random.Range(0, Party.Length)], false, "Confetti");
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(26, 26) * Random.Range(0.8f, 1.5f);
                var from = new Vector2(Random.Range(-size.x * 0.5f, size.x * 0.5f), size.y * 0.5f + Random.Range(20f, 300f));
                float fall = size.y + Random.Range(100f, 400f);
                float drift = Random.Range(-160f, 160f), spin = Random.Range(-720f, 720f), delay = Random.Range(0f, 0.6f);
                rt.anchoredPosition = from;
                Tweener.Run(Random.Range(1.8f, 2.8f), k =>
                {
                    if (rt == null) return;
                    rt.anchoredPosition = from + new Vector2(drift * k + Mathf.Sin(k * 9f) * 30f, -fall * k);
                    rt.localRotation = Quaternion.Euler(0, 0, spin * k);
                }, Ease.Linear, () => { if (img != null) Object.Destroy(img.gameObject); }, delay, img);
            }
        }

        /// <summary>Icons bursting out of a point and flying up (coins after a win).</summary>
        public static void Burst(RectTransform layer, Vector2 at, Sprite sprite, int count, MonoBehaviour owner)
        {
            for (int i = 0; i < count; i++)
            {
                var img = UIKit.Image(layer, sprite, Color.white, false, "Burst");
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(70, 70);
                float ang = Random.Range(0f, Mathf.PI * 2f);
                var dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * Random.Range(160f, 380f);
                Tweener.Run(Random.Range(0.8f, 1.2f), k =>
                {
                    if (rt == null) return;
                    rt.anchoredPosition = at + dir * k + new Vector2(0, -260f * k * k + 200f * k);
                    rt.localScale = Vector3.one * (1f - 0.6f * k);
                    img.color = new Color(1, 1, 1, 1f - k * k);
                }, Ease.OutQuad, () => { if (img != null) Object.Destroy(img.gameObject); }, i * 0.02f, img);
            }
        }

        public static void Flash(RectTransform layer, Color color)
        {
            var img = UIKit.Image(layer, null, PixelCanvas.WithAlpha(color, 0.6f), false, "Flash");
            img.rectTransform.Fill();
            Tweener.Run(0.5f, k => { if (img != null) img.color = PixelCanvas.WithAlpha(color, 0.6f * (1f - k)); }, Ease.OutQuad,
                () => { if (img != null) Object.Destroy(img.gameObject); }, 0f, img);
        }
    }

    /// <summary>
    /// The lucky wheel: coloured slices with icons, a golden rim with chasing lights, a pointer that flicks on
    /// every slice, and a glowing highlight on the winning slice.
    /// </summary>
    public sealed class WheelView : MonoBehaviour
    {
        private RectTransform _disc;
        private RectTransform _pointer;
        private Image _win;
        private readonly List<Image> _bulbs = new List<Image>();
        private int _count;
        private float _angle;
        private bool _spinning;
        private float _celebrate;
        private float _pointerKick;

        public void Build(LuckyWheel wheel)
        {
            var rt = (RectTransform)transform;
            _count = wheel.Segments.Count;
            float size = rt.sizeDelta.x;

            var glow = UIKit.Image(rt, SpriteFactory.SoftDot(), PixelCanvas.WithAlpha(ColorUtil.Gold, 0.45f), false, "Glow");
            glow.rectTransform.Fill(-70, -70, -70, -70);
            var rimDark = UIKit.Image(rt, SpriteFactory.Circle(), ColorUtil.Hex("#8A5A00"), false, "RimDark");
            rimDark.rectTransform.Fill(-30, -30, -30, -30);
            var rim = UIKit.Image(rt, SpriteFactory.Circle(), ColorUtil.Gold, false, "Rim");
            rim.rectTransform.Fill(-24, -24, -24, -24);
            var rimInner = UIKit.Image(rt, SpriteFactory.Circle(), ColorUtil.Hex("#B37A00"), false, "RimInner");
            rimInner.rectTransform.Fill(-6, -6, -6, -6);

            _disc = UIKit.Node("Disc", rt).Fill();
            float seg = 360f / _count;
            for (int i = 0; i < _count; i++)
            {
                var s = wheel.Segments[i];
                var col = ColorUtil.Hex(s.Color);
                var slice = UIKit.Image(_disc, SpriteFactory.Circle(), col, false, "Slice");
                slice.rectTransform.Fill();
                slice.type = Image.Type.Filled;
                slice.fillMethod = Image.FillMethod.Radial360;
                slice.fillOrigin = (int)Image.Origin360.Top;
                slice.fillClockwise = true;
                slice.fillAmount = 1f / _count;
                slice.rectTransform.localRotation = Quaternion.Euler(0, 0, -i * seg);
                // Lighter inner half for depth
                var inner = UIKit.Image(_disc, SpriteFactory.Circle(), PixelCanvas.Lighten(col, 0.25f), false, "Inner");
                inner.rectTransform.Fill(size * 0.18f, size * 0.18f, size * 0.18f, size * 0.18f);
                inner.type = Image.Type.Filled;
                inner.fillMethod = Image.FillMethod.Radial360;
                inner.fillOrigin = (int)Image.Origin360.Top;
                inner.fillAmount = 1f / _count;
                inner.rectTransform.localRotation = Quaternion.Euler(0, 0, -i * seg);
            }
            // Thin separators between slices
            for (int i = 0; i < _count; i++)
            {
                var line = UIKit.Image(_disc, null, new Color(1f, 1f, 1f, 0.85f), false, "Sep");
                line.rectTransform.anchorMin = line.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                line.rectTransform.pivot = new Vector2(0.5f, 0f);
                line.rectTransform.sizeDelta = new Vector2(4f, size * 0.5f);
                line.rectTransform.anchoredPosition = Vector2.zero;
                line.rectTransform.localRotation = Quaternion.Euler(0, 0, -i * seg);
            }
            // Icons and amounts, reading outward
            for (int i = 0; i < _count; i++)
            {
                var s = wheel.Segments[i];
                float mid = (i + 0.5f) * seg;
                var holder = UIKit.Node("Item", _disc);
                holder.anchorMin = holder.anchorMax = new Vector2(0.5f, 0.5f);
                holder.sizeDelta = Vector2.zero;
                holder.localRotation = Quaternion.Euler(0, 0, -mid);
                var icon = UIKit.Image(holder, WheelArt.Icon(s.Kind), Color.white, false, "Icon");
                icon.rectTransform.At(UIKit.Center, new Vector2(0, size * 0.36f), new Vector2(size * 0.12f, size * 0.12f));
                var label = UIKit.Text(holder, s.Label, s.Label.Length > 3 ? 26 : 32, Color.white, TextAnchor.MiddleCenter, true).Fit(16);
                label.rectTransform.At(UIKit.Center, new Vector2(0, size * 0.25f), new Vector2(110, 44));
            }

            // Winner highlight (rotates with the disc)
            _win = UIKit.Image(_disc, SpriteFactory.Circle(), new Color(1f, 1f, 1f, 0f), false, "Win");
            _win.rectTransform.Fill();
            _win.type = Image.Type.Filled;
            _win.fillMethod = Image.FillMethod.Radial360;
            _win.fillOrigin = (int)Image.Origin360.Top;
            _win.fillAmount = 1f / _count;

            // Light bulbs around the rim
            const int bulbs = 24;
            for (int i = 0; i < bulbs; i++)
            {
                float a = i * Mathf.PI * 2f / bulbs;
                var b = UIKit.Image(rt, SpriteFactory.Circle(), Color.white, false, "Bulb");
                b.rectTransform.At(UIKit.Center, new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * (size * 0.5f + 15f), new Vector2(16, 16));
                _bulbs.Add(b);
            }

            var hub = UIKit.Image(rt, SpriteFactory.Circle(), ColorUtil.Gold, false, "Hub");
            hub.rectTransform.At(UIKit.Center, Vector2.zero, new Vector2(size * 0.2f, size * 0.2f));
            var hubIn = UIKit.Image(hub.transform, SpriteFactory.Circle(), ColorUtil.Night, false, "HubIn");
            hubIn.rectTransform.Fill(8, 8, 8, 8);
            var star = UIKit.Image(hubIn.transform, SpriteFactory.Star(), ColorUtil.Yellow, false, "Star");
            star.rectTransform.Fill(10, 10, 10, 10);

            var pointerShadow = UIKit.Image(rt, SpriteFactory.Triangle(), new Color(0, 0, 0, 0.35f), false, "PointerShadow");
            pointerShadow.rectTransform.At(UIKit.Top, new Vector2(4, 4), new Vector2(84, 84));
            var pointer = UIKit.Image(rt, SpriteFactory.Triangle(), ColorUtil.Red, false, "Pointer");
            _pointer = pointer.rectTransform;
            _pointer.At(UIKit.Top, new Vector2(0, 12), new Vector2(84, 84));
            _pointer.pivot = new Vector2(0.5f, 0.85f);
        }

        public bool IsSpinning => _spinning;

        public void ResetRotation()
        {
            _angle = 0f;
            _disc.localRotation = Quaternion.identity;
            _win.color = new Color(1f, 1f, 1f, 0f);
            _celebrate = 0f;
        }

        public void SpinTo(int index, System.Action onTick, System.Action onDone)
        {
            float seg = 360f / _count;
            float jitter = Random.Range(-0.35f, 0.35f) * seg;
            float target = (index + 0.5f) * seg + jitter;
            float start = _angle % 360f;
            float end = 360f * 6f + target;
            int lastSlice = Mathf.FloorToInt(start / seg);
            _spinning = true;
            _celebrate = 0f;
            _win.color = new Color(1f, 1f, 1f, 0f);
            _win.rectTransform.localRotation = Quaternion.Euler(0, 0, -index * seg);
            Tweener.Run(4.6f, k =>
            {
                if (_disc == null) return;
                _angle = Mathf.LerpUnclamped(start, end, k);
                _disc.localRotation = Quaternion.Euler(0, 0, _angle);
                int slice = Mathf.FloorToInt(_angle / seg);
                if (slice != lastSlice) { lastSlice = slice; _pointerKick = 1f; onTick?.Invoke(); }
            }, Ease.OutQuint, () =>
            {
                _spinning = false;
                _celebrate = 3f;
                onDone?.Invoke();
            }, owner: this);
        }

        private void Update()
        {
            float t = Time.unscaledTime;
            // Bulbs: slow twinkle when idle, fast chase while spinning, all flashing when someone wins.
            for (int i = 0; i < _bulbs.Count; i++)
            {
                bool on;
                if (_celebrate > 0f) on = Mathf.Repeat(t * 6f, 1f) < 0.5f;
                else if (_spinning) on = (i + (int)(t * 18f)) % 4 == 0;
                else on = (i + (int)(t * 3f)) % 2 == 0;
                _bulbs[i].color = on ? new Color(1f, 0.97f, 0.75f, 1f) : new Color(1f, 0.7f, 0.2f, 0.45f);
                _bulbs[i].rectTransform.localScale = Vector3.one * (on ? 1.15f : 0.9f);
            }
            if (_celebrate > 0f)
            {
                _celebrate -= Time.unscaledDeltaTime;
                _win.color = new Color(1f, 1f, 1f, 0.25f + 0.25f * Mathf.Sin(t * 10f));
                if (_celebrate <= 0f) _win.color = new Color(1f, 1f, 1f, 0.2f);
            }
            if (_pointer != null)
            {
                _pointerKick = Mathf.Max(0f, _pointerKick - Time.unscaledDeltaTime * 8f);
                _pointer.localRotation = Quaternion.Euler(0, 0, _pointerKick * 18f);
            }
        }
    }

    /// <summary>
    /// Wheel + spin button + info + odds, used on the results screen and on the wheel screen.
    /// Free spins come first; after that a spin costs coins (a few per day).
    /// </summary>
    public sealed class WheelPanel : MonoBehaviour
    {
        private App _app;
        private WheelView _wheel;
        private CandyButton _spin;
        private Text _info;
        private RectTransform _fx;
        private readonly SystemRandom _rng = new SystemRandom();

        /// <summary>How many free spins are waiting; the owner sets it and the panel uses them first.</summary>
        public System.Func<bool> HasFreeSpin;
        public System.Action UseFreeSpin;
        public System.Action Spun;

        /// <param name="wheelRect">Where the wheel goes (in <paramref name="parent"/>).</param>
        /// <param name="buttonPos">Centre of the spin button; info and odds go below it.</param>
        public static WheelPanel Create(App app, RectTransform parent, RectTransform fxLayer, Vector2 wheelPos, float wheelSize, Vector2 buttonPos)
        {
            var p = parent.gameObject.AddComponent<WheelPanel>();
            p._app = app;
            p._fx = fxLayer;
            p._wheel = UIKit.Node("Wheel", parent).gameObject.AddComponent<WheelView>();
            ((RectTransform)p._wheel.transform).At(UIKit.Center, wheelPos, new Vector2(wheelSize, wheelSize));
            p._wheel.Build(app.Economy.Wheel);
            p._spin = UIKit.Button(parent, Loc.T("go.spin"), ColorUtil.Orange, p.Spin, new Vector2(340, 150), 54);
            p._spin.GetComponent<RectTransform>().At(UIKit.Center, buttonPos, new Vector2(340, 150));
            p._spin.ClickWhenDisabled = true;
            p._spin.gameObject.AddComponent<Bobber>().Amplitude = 4f;
            p._info = UIKit.Text(parent, "", 30, ColorUtil.Ink, TextAnchor.MiddleCenter, false).Fit(18);
            p._info.rectTransform.At(UIKit.Center, buttonPos + new Vector2(0, -130), new Vector2(380, 100));
            var odds = UIKit.Button(parent, Loc.T("go.odds"), ColorUtil.Purple, p.ShowOdds, new Vector2(170, 76), 30);
            odds.GetComponent<RectTransform>().At(UIKit.Center, buttonPos + new Vector2(0, -230), new Vector2(170, 76));
            return p;
        }

        public void ResetWheel()
        {
            _wheel.ResetRotation();
            Refresh();
        }

        public void Refresh()
        {
            var e = _app.Economy;
            bool free = HasFreeSpin != null && HasFreeSpin();
            if (free)
            {
                _spin.SetLabel(Loc.T("go.spin"));
                _spin.SetColor(ColorUtil.Orange);
                _info.text = Loc.T("wheel.free_now");
            }
            else
            {
                int left = e.PaidSpinsLeft;
                _spin.SetLabel(Loc.T("wheel.spin_for", EconomyService.PaidSpinCost));
                _spin.SetColor(left > 0 && e.Coins >= EconomyService.PaidSpinCost ? ColorUtil.Gold : ColorUtil.Muted);
                _info.text = left > 0 ? Loc.T("wheel.paid_left", left) : Loc.T("wheel.paid_none");
            }
            if (e.CoinBoostGames > 0) _info.text += "\n" + Loc.T("wheel.booster_active", e.CoinBoostGames);
            _spin.Interactable = !_wheel.IsSpinning;
        }

        private void Spin()
        {
            if (_wheel.IsSpinning) return;
            var e = _app.Economy;
            if (HasFreeSpin != null && HasFreeSpin()) UseFreeSpin?.Invoke();
            else if (!e.TryBuySpin())
            {
                _app.Audio.Play(Sfx.Error);
                _app.Popup.ShowMessage(Loc.T("wheel.title"),
                    e.PaidSpinsLeft <= 0 ? Loc.T("wheel.paid_none") : Loc.T("city.not_enough_coins"), WheelArt.WheelIcon());
                return;
            }
            var result = e.SpinWheel(_rng);
            if (result.Segment.Kind == WheelRewardKind.PetEnergy) _app.Pet.AddEnergy(result.Segment.Amount);
            _app.Save.Save(e.Data);
            Refresh();
            _spin.Interactable = false;
            _app.Audio.Play(Sfx.Button);
            _wheel.SpinTo(result.SegmentIndex, () => _app.Audio.Play(Sfx.WheelTick, 0.55f), () => Celebrate(result));
            Spun?.Invoke();
        }

        private void Celebrate(WheelResult result)
        {
            var seg = result.Segment;
            bool big = seg.Kind == WheelRewardKind.Jackpot || (seg.Kind == WheelRewardKind.Coins && seg.Amount >= 250) || seg.Kind == WheelRewardKind.CoinBooster;
            Vector2 at = _fx.InverseTransformPoint(_wheel.transform.position);
            _app.Audio.Play(big ? Sfx.Jackpot : Sfx.Win);
            Haptics.Medium();
            if (big)
            {
                Haptics.Big();
                UiFx.Flash(_fx, ColorUtil.Gold);
            }
            UiFx.Confetti(_fx, big ? 90 : 40, this);
            UiFx.Burst(_fx, at, WheelArt.Icon(seg.Kind), big ? 22 : 10, this);
            Refresh();

            string big1, body;
            Sprite icon = WheelArt.Icon(seg.Kind);
            switch (seg.Kind)
            {
                case WheelRewardKind.Coins: big1 = "+" + Loc.Number(seg.Amount); body = Loc.T("go.lucky_spin"); break;
                case WheelRewardKind.Jackpot: big1 = "+" + Loc.Number(seg.Amount); body = Loc.T("wheel.jackpot"); break;
                case WheelRewardKind.Shard: big1 = "+" + seg.Amount; body = Loc.T("go.shard_body"); break;
                case WheelRewardKind.PowerUp:
                    big1 = "+" + seg.Amount + " " + Loc.PowerUpShortName(result.PowerUp); body = Loc.T("go.free_powerup");
                    icon = SpriteFactory.PowerUpIcon(result.PowerUp);
                    break;
                case WheelRewardKind.Supplies: big1 = Loc.T("wheel.supplies_big", seg.Amount * 2, seg.Amount, System.Math.Max(1, seg.Amount / 2)); body = Loc.T("wheel.supplies"); break;
                case WheelRewardKind.RunTicket: big1 = "+" + seg.Amount; body = Loc.T("run.ticket_won"); break;
                case WheelRewardKind.PetEnergy: big1 = "+" + seg.Amount; body = Loc.T("wheel.pet_energy"); break;
                default: big1 = "x2"; body = Loc.T("wheel.booster", seg.Amount); break;
            }
            Tweener.Delay(big ? 0.9f : 0.5f, () => _app.Popup.ShowReward(big1, body, icon), this);
        }

        /// <summary>Shows the exact chances of every slice.</summary>
        private void ShowOdds()
        {
            var w = _app.Economy.Wheel;
            var lines = new List<string>();
            for (int i = 0; i < w.Segments.Count; i++)
            {
                var s = w.Segments[i];
                lines.Add($"{Loc.T("wheel.kind." + s.Kind, s.Amount)}: {Loc.Decimal((float)System.Math.Round(w.ProbabilityOf(i) * 100.0, 1))}%");
            }
            lines.Add("");
            lines.Add(Loc.T("wheel.odds_note", EconomyService.PaidSpinCost, EconomyService.MaxPaidSpinsPerDay));
            _app.Popup.ShowMessage(Loc.T("go.odds_title"), string.Join("\n", lines), null);
        }
    }

    /// <summary>The lucky wheel on its own (from the home screen): one free spin a day, then spins for coins.</summary>
    public sealed class WheelScreen : UIScreen
    {
        private WheelPanel _panel;
        private Text _coins;

        protected override void Build()
        {
            var back = UIKit.IconButton(Root, SpriteFactory.HomeIcon(), ColorUtil.Purple, App.GoHome, 120);
            back.GetComponent<RectTransform>().At(UIKit.TopLeft, new Vector2(84, -84), new Vector2(120, 120));
            var title = UIKit.Text(Root, Loc.T("wheel.title"), 84, ColorUtil.Gold).Fit(40);
            title.rectTransform.At(UIKit.Top, new Vector2(-20, -84), new Vector2(560, 120));
            _coins = UIKit.CurrencyPill(Root, SpriteFactory.Coin(), ColorUtil.Ink, out var pill, 260);
            pill.At(UIKit.TopRight, new Vector2(-150, -84), pill.sizeDelta);

            var area = UIKit.Node("Area", Root);
            area.Fill();
            var fx = UIKit.Node("Fx", Root);
            fx.Fill();
            _panel = WheelPanel.Create(App, area, fx, new Vector2(0, 230), 760, new Vector2(0, -400));
            _panel.HasFreeSpin = () => App.Economy.DailyFreeSpinReady;
            _panel.UseFreeSpin = () => App.Economy.UseDailyFreeSpin();
            fx.SetAsLastSibling();
        }

        protected override void OnShow() => _panel.ResetWheel();

        public override void Refresh()
        {
            _coins.text = Loc.Number(App.Economy.Coins);
            _panel.Refresh();
        }

        public override bool OnBack()
        {
            App.GoHome();
            return true;
        }
    }
}
