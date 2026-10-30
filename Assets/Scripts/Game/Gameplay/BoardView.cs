using System.Collections.Generic;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.Gameplay
{
    /// <summary>
    /// Draws the board, active piece and ghost with SpriteRenderers. One cell = one world unit.
    /// The board's bottom-left corner sits at <see cref="Origin"/>.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        private const int OrderBackground = 0;
        private const int OrderCells = 10;
        private const int OrderGhost = 9;
        private const int OrderActive = 20;
        private const int OrderOverlay = 25;

        private int _width;
        private int _height;
        private SpriteRenderer[,] _cells;
        private SpriteRenderer[,] _cellOverlays;
        private float[,] _squish;
        private readonly List<SpriteRenderer> _active = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> _activeOverlays = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> _ghost = new List<SpriteRenderer>();
        private SpriteRenderer _frame;
        private SpriteRenderer _frameGlow;
        private readonly List<SpriteRenderer> _dots = new List<SpriteRenderer>();
        private Color _accent = ColorUtil.Pink;
        private SpriteRenderer _dangerGlow;
        private SpriteRenderer _flashOverlay;
        private ShopItem _skin;
        private Color[] _palette;
        private Vector3 _activeVisualOffset;
        private int _lastRotation;
        private float _rotatePop;
        private ActivePiece _lastPiece;
        private Int2 _lastOrigin;
        private float _flash;

        public Vector3 Origin => transform.position;
        public int Width => _width;
        public int VisibleHeight => _height;

        public void Build(int width, int visibleHeight)
        {
            _width = width;
            _height = visibleHeight;
            _cells = new SpriteRenderer[width, visibleHeight];
            _cellOverlays = new SpriteRenderer[width, visibleHeight];
            _squish = new float[width, visibleHeight];

            // Frame & well
            // Neon frame: soft glow behind, a dark well, and a bright ring on top of the well.
            var center = new Vector3(width / 2f, visibleHeight / 2f, 0f);
            _frameGlow = NewRenderer("FrameGlow", SpriteFactory.Glow(), OrderBackground);
            _frameGlow.drawMode = SpriteDrawMode.Sliced;
            _frameGlow.size = new Vector2(width + 0.75f, visibleHeight + 0.75f); // hollow glow starts 0.3 inside its edge
            _frameGlow.transform.localPosition = center;

            var well = NewRenderer("Well", SpriteFactory.RoundedRect(), OrderBackground + 1);
            well.drawMode = SpriteDrawMode.Sliced;
            well.size = new Vector2(width + 0.15f, visibleHeight + 0.15f);
            well.transform.localPosition = center;
            well.color = new Color(1f, 1f, 1f, 0.72f);

            _frame = NewRenderer("Frame", SpriteFactory.RoundedRing(), OrderBackground + 4);
            _frame.drawMode = SpriteDrawMode.Sliced;
            _frame.size = new Vector2(width + 0.45f, visibleHeight + 0.45f);
            _frame.transform.localPosition = center;

            _dangerGlow = NewRenderer("Danger", SpriteFactory.RoundedRect(), OrderBackground + 2);
            _dangerGlow.drawMode = SpriteDrawMode.Sliced;
            _dangerGlow.size = well.size;
            _dangerGlow.transform.localPosition = well.transform.localPosition;
            _dangerGlow.color = new Color(1f, 0.2f, 0.3f, 0f);

            _flashOverlay = NewRenderer("Flash", SpriteFactory.RoundedRect(), OrderOverlay + 1);
            _flashOverlay.drawMode = SpriteDrawMode.Sliced;
            _flashOverlay.size = well.size;
            _flashOverlay.transform.localPosition = well.transform.localPosition;
            _flashOverlay.color = new Color(1f, 1f, 1f, 0f);

            // Faint grid dots
            var dot = SpriteFactory.SoftDot();
            for (int x = 1; x < width; x++)
            {
                for (int y = 1; y < visibleHeight; y++)
                {
                    var d = NewRenderer("dot", dot, OrderBackground + 3);
                    d.transform.localPosition = new Vector3(x, y, 0f);
                    d.transform.localScale = Vector3.one * 0.12f;
                    _dots.Add(d);
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < visibleHeight; y++)
                {
                    _cells[x, y] = NewRenderer($"c{x}_{y}", null, OrderCells);
                    _cells[x, y].transform.localPosition = CellLocal(x, y);
                    _cellOverlays[x, y] = NewRenderer("o", null, OrderOverlay);
                    _cellOverlays[x, y].transform.SetParent(_cells[x, y].transform, false);
                    _cellOverlays[x, y].transform.localScale = Vector3.one * 0.55f;
                }
            }

            SetAccent(_accent);

            for (int i = 0; i < 4; i++)
            {
                var a = NewRenderer("active", null, OrderActive);
                var o = NewRenderer("activeOverlay", null, OrderOverlay);
                o.transform.SetParent(a.transform, false);
                o.transform.localScale = Vector3.one * 0.55f;
                _active.Add(a);
                _activeOverlays.Add(o);
                _ghost.Add(NewRenderer("ghost", null, OrderGhost));
            }
        }

        public void SetSkin(ShopItem skin)
        {
            _skin = skin;
            _palette = new Color[skin.Palette.Length];
            for (int i = 0; i < _palette.Length; i++) _palette[i] = ColorUtil.Hex(skin.Palette[i]);
        }

        /// <summary>Theme accent used for the glowing frame and grid dots.</summary>
        public void SetAccent(Color accent)
        {
            _accent = accent;
            if (_frame == null) return;
            _frame.color = accent;
            _frameGlow.color = PixelCanvas.WithAlpha(accent, 0.3f);
            foreach (var d in _dots) d.color = PixelCanvas.WithAlpha(ColorUtil.Ink, 0.1f);
        }

        public Color ColorOf(PieceType type)
        {
            if (type == PieceType.Garbage) return ColorUtil.Junk;
            return _palette == null || (int)type >= _palette.Length ? ColorUtil.Pink : _palette[(int)type];
        }

        public Sprite SpriteFor(PieceType type, bool golden)
        {
            if (type == PieceType.Bomb) return SpriteFactory.PowerUpIcon(PowerUpType.Bomb);
            if (type == PieceType.Garbage) return SpriteFactory.JunkBlock();
            if (golden) return SpriteFactory.GoldenBlock();
            return SpriteFactory.Block(_skin?.BlockStyle ?? BlockStyle.Jelly, ColorOf(type));
        }

        public Vector3 CellWorld(int x, int y) => transform.TransformPoint(CellLocal(x, y));

        public Rect WorldRect => new Rect(Origin.x, Origin.y, _width, _height);

        /// <summary>Makes the cells of a freshly locked piece squish down a little.</summary>
        public void Squish(ActivePiece piece)
        {
            foreach (var c in piece.BoardCells())
                if (c.x >= 0 && c.x < _width && c.y >= 0 && c.y < _height) _squish[c.x, c.y] = 1f;
            _activeVisualOffset = Vector3.zero;
        }

        /// <summary>Short white flash over the board (used on quads / jackpots).</summary>
        public void Flash() => _flash = 1f;

        public void Render(GameSession session, float deltaTime)
        {
            var board = session.Board;
            float t = Time.unscaledTime;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var cell = board[x, y];
                    var r = _cells[x, y];
                    var overlay = _cellOverlays[x, y];
                    if (cell.IsEmpty)
                    {
                        if (r.enabled) r.enabled = false;
                        if (overlay.enabled) overlay.enabled = false;
                        _squish[x, y] = 0f;
                        continue;
                    }

                    bool golden = (cell.Flags & CellFlags.Golden) != 0;
                    r.enabled = true;
                    r.sprite = SpriteFor(cell.PieceType, golden);
                    r.color = Color.white;

                    // Squish animation: scale y down, x up, then settle.
                    float s = _squish[x, y];
                    if (s > 0f)
                    {
                        _squish[x, y] = Mathf.Max(0f, s - deltaTime * 5f);
                        float k = Mathf.Sin(s * Mathf.PI) * 0.18f;
                        r.transform.localScale = new Vector3(1f + k, 1f - k, 1f);
                        r.transform.localPosition = CellLocal(x, y) - new Vector3(0f, k * 0.5f, 0f);
                    }
                    else
                    {
                        // Idle jelly wobble, very subtle.
                        float wob = Mathf.Sin(t * 3f + x * 0.7f + y * 0.45f) * 0.015f;
                        r.transform.localScale = new Vector3(1f + wob, 1f - wob, 1f);
                        r.transform.localPosition = CellLocal(x, y);
                    }

                    bool coin = (cell.Flags & CellFlags.Coin) != 0;
                    overlay.enabled = coin || golden;
                    if (coin)
                    {
                        overlay.sprite = SpriteFactory.Coin();
                        overlay.transform.localScale = Vector3.one * (0.6f + Mathf.Sin(t * 5f) * 0.05f);
                        overlay.transform.localRotation = Quaternion.identity;
                        overlay.color = Color.white;
                    }
                    else if (golden)
                    {
                        overlay.sprite = SpriteFactory.Sparkle();
                        float tw = Mathf.Max(0f, Mathf.Sin(t * 4f + x * 1.3f + y * 0.7f));
                        overlay.transform.localScale = Vector3.one * (0.5f * tw);
                        overlay.transform.localRotation = Quaternion.Euler(0f, 0f, t * 90f);
                        overlay.color = new Color(1f, 1f, 1f, tw);
                    }
                }
            }

            RenderActive(session, deltaTime, t);

            // Danger glow when the stack gets close to the top.
            float danger = Mathf.InverseLerp(_height - 6, _height - 2, board.StackHeight());
            float pulse = 0.5f + 0.5f * Mathf.Sin(t * 6f);
            _dangerGlow.color = new Color(1f, 0.25f, 0.35f, danger * (0.15f + 0.15f * pulse));

            _frameGlow.color = PixelCanvas.WithAlpha(_accent, 0.22f + 0.08f * Mathf.Sin(t * 2.5f));
            _flash = Mathf.Max(0f, _flash - deltaTime * 4f);
            _flashOverlay.color = PixelCanvas.WithAlpha(ColorUtil.Yellow, _flash * 0.35f);
        }

        private void RenderActive(GameSession session, float deltaTime, float t)
        {
            var piece = session.Current;
            if (piece == null || session.IsOver)
            {
                foreach (var r in _active) r.enabled = false;
                foreach (var r in _activeOverlays) r.enabled = false;
                foreach (var r in _ghost) r.enabled = false;
                return;
            }

            // Smooth visual movement: when the piece moves, keep a small offset that eases to zero.
            if (piece == _lastPiece)
            {
                var delta = _lastOrigin - piece.Origin;
                if (delta.x != 0 || delta.y != 0)
                {
                    // One row down is gravity or soft drop: the continuous fall below already glides there.
                    bool fallStep = delta.x == 0 && delta.y == 1;
                    if (fallStep) { }
                    else if (Mathf.Abs(delta.y) <= 2 && Mathf.Abs(delta.x) <= 2)
                        _activeVisualOffset += new Vector3(delta.x, delta.y, 0f);
                    else
                        _activeVisualOffset = Vector3.zero;
                }
                if (piece.Rotation != _lastRotation) _rotatePop = 1f;
            }
            else
            {
                _activeVisualOffset = Vector3.zero;
                _rotatePop = 0f;
            }
            _lastPiece = piece;
            _lastOrigin = piece.Origin;
            _lastRotation = piece.Rotation;
            _activeVisualOffset = Vector3.Lerp(_activeVisualOffset, Vector3.zero, 1f - Mathf.Exp(-deltaTime * 26f));
            _rotatePop = Mathf.Max(0f, _rotatePop - deltaTime * 7f);
            // Smooth gravity: the piece slides continuously between rows instead of jumping.
            var fallOffset = new Vector3(0f, -session.FallProgress, 0f);

            var cells = piece.LocalCells;
            var ghostOrigin = session.GhostOrigin();
            var sprite = SpriteFor(piece.Type, piece.Golden);
            bool showGhost = session.Config.ShowGhost && ghostOrigin.y != piece.Origin.y;

            for (int i = 0; i < 4; i++)
            {
                var r = _active[i];
                var o = _activeOverlays[i];
                var g = _ghost[i];
                if (i >= cells.Length)
                {
                    r.enabled = o.enabled = g.enabled = false;
                    continue;
                }

                var p = piece.Origin + cells[i];
                bool visible = p.y < _height;
                r.enabled = visible;
                r.sprite = sprite;
                r.transform.localPosition = CellLocal(p.x, p.y) + _activeVisualOffset + fallOffset;
                float pulse = piece.Type == PieceType.Bomb ? 1f + Mathf.Sin(t * 12f) * 0.08f : 1f;
                pulse *= 1f + Mathf.Sin(_rotatePop * Mathf.PI) * 0.08f;
                r.transform.localScale = Vector3.one * pulse;
                r.color = Color.white;

                bool coin = i == piece.CoinCellIndex;
                o.enabled = visible && (coin || piece.Golden);
                if (coin)
                {
                    o.sprite = SpriteFactory.Coin();
                    o.color = Color.white;
                    o.transform.localScale = Vector3.one * 0.6f;
                    o.transform.localRotation = Quaternion.identity;
                }
                else if (piece.Golden)
                {
                    o.sprite = SpriteFactory.Sparkle();
                    float tw = Mathf.Max(0f, Mathf.Sin(t * 6f + i * 1.7f));
                    o.transform.localScale = Vector3.one * (0.55f * tw);
                    o.transform.localRotation = Quaternion.Euler(0f, 0f, t * 120f);
                    o.color = Color.white;
                }

                var gp = ghostOrigin + cells[i];
                g.enabled = showGhost && gp.y < _height;
                g.sprite = sprite;
                g.transform.localPosition = CellLocal(gp.x, gp.y);
                g.color = new Color(1f, 1f, 1f, 0.22f);
            }
        }

        private static Vector3 CellLocal(int x, int y) => new Vector3(x + 0.5f, y + 0.5f, 0f);

        private SpriteRenderer NewRenderer(string name, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var r = SpriteMaterial.Add(go);
            r.sprite = sprite;
            r.sortingOrder = order;
            return r;
        }
    }
}
