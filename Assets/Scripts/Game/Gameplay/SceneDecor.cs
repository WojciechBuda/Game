using System.Collections.Generic;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.Gameplay
{
    /// <summary>Shows one small piece inside a rounded box (used for HOLD and NEXT).</summary>
    public sealed class PieceSlot : MonoBehaviour
    {
        private readonly List<SpriteRenderer> _cells = new List<SpriteRenderer>();
        private SpriteRenderer _box;
        private SpriteRenderer _ring;
        private float _cellScale;
        private float _pop;
        private PieceType? _shown;
        private bool _shownGolden;

        public void Build(float boxSize, float cellScale)
        {
            _cellScale = cellScale;
            _box = SpriteMaterial.Add(gameObject);
            _box.sprite = SpriteFactory.RoundedRect();
            _box.drawMode = SpriteDrawMode.Sliced;
            _box.size = new Vector2(boxSize, boxSize);
            _box.color = PixelCanvas.WithAlpha(ColorUtil.Night, 0.7f);
            _box.sortingOrder = 5;
            var ringGo = new GameObject("ring");
            ringGo.transform.SetParent(transform, false);
            _ring = SpriteMaterial.Add(ringGo);
            _ring.sprite = SpriteFactory.RoundedRing();
            _ring.drawMode = SpriteDrawMode.Sliced;
            _ring.size = new Vector2(boxSize, boxSize);
            _ring.sortingOrder = 5;
            SetAccent(ColorUtil.Blue);
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject("cell");
                go.transform.SetParent(transform, false);
                var r = SpriteMaterial.Add(go);
                r.sortingOrder = 6;
                _cells.Add(r);
            }
        }

        public void SetAccent(Color accent)
        {
            if (_ring != null) _ring.color = PixelCanvas.WithAlpha(accent, 0.8f);
        }

        public void Show(PieceType? type, bool golden, BoardView look, bool dimmed = false)
        {
            if (type != _shown || golden != _shownGolden) _pop = 1f;
            _shown = type;
            _shownGolden = golden;

            if (!type.HasValue)
            {
                foreach (var c in _cells) c.enabled = false;
                return;
            }

            var cells = PieceData.GetCells(type.Value, 0);
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            foreach (var c in cells)
            {
                minX = Mathf.Min(minX, c.x); maxX = Mathf.Max(maxX, c.x);
                minY = Mathf.Min(minY, c.y); maxY = Mathf.Max(maxY, c.y);
            }
            var center = new Vector2((minX + maxX) / 2f + 0.5f, (minY + maxY) / 2f + 0.5f);
            var sprite = look.SpriteFor(type.Value, golden);

            for (int i = 0; i < _cells.Count; i++)
            {
                var r = _cells[i];
                if (i >= cells.Length) { r.enabled = false; continue; }
                r.enabled = true;
                r.sprite = sprite;
                r.color = dimmed ? new Color(1f, 1f, 1f, 0.45f) : Color.white;
                r.transform.localPosition = new Vector3(cells[i].x + 0.5f - center.x, cells[i].y + 0.5f - center.y, 0f) * _cellScale;
                r.transform.localScale = Vector3.one * _cellScale;
            }
        }

        private void Update()
        {
            if (_pop <= 0f) return;
            _pop = Mathf.Max(0f, _pop - Time.unscaledDeltaTime * 4f);
            float s = 1f + Mathf.Sin(_pop * Mathf.PI) * 0.15f;
            transform.localScale = Vector3.one * s;
        }
    }

    /// <summary>The cute mascot above the board. Reacts to what happens in the game.</summary>
    public sealed class AvatarView : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private ShopItem _avatar;
        private AvatarMood _baseMood = AvatarMood.Calm;
        private AvatarMood _mood = AvatarMood.Calm;
        private float _moodTimer;
        private float _jump;
        private Vector3 _basePos;
        private float _size = 1f;

        public void Build(float size)
        {
            _size = size;
            _renderer = SpriteMaterial.Add(gameObject);
            _renderer.sortingOrder = 30;
            transform.localScale = Vector3.one * size;
            _basePos = transform.localPosition;
        }

        public void SetAvatar(ShopItem avatar)
        {
            _avatar = avatar;
            Apply(_mood);
        }

        public void SetBasePosition(Vector3 localPos)
        {
            _basePos = localPos;
            transform.localPosition = localPos;
        }

        /// <summary>Calm normally, shocked when the stack is high.</summary>
        public void SetBaseMood(AvatarMood mood) => _baseMood = mood;

        public void React(AvatarMood mood, float seconds = 1.2f, bool jump = true)
        {
            _mood = mood;
            _moodTimer = seconds;
            if (jump) _jump = 1f;
            Apply(mood);
        }

        private void Apply(AvatarMood mood)
        {
            if (_avatar != null && _renderer != null) _renderer.sprite = SpriteFactory.Avatar(_avatar, mood);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (_moodTimer > 0f)
            {
                _moodTimer -= dt;
                if (_moodTimer <= 0f) { _mood = _baseMood; Apply(_mood); }
            }
            else if (_mood != _baseMood)
            {
                _mood = _baseMood;
                Apply(_mood);
            }

            float t = Time.unscaledTime;
            _jump = Mathf.Max(0f, _jump - dt * 2.5f);
            float hop = Mathf.Sin(_jump * Mathf.PI) * 0.6f;
            float breathe = Mathf.Sin(t * 2.2f) * 0.04f;
            transform.localPosition = _basePos + new Vector3(0f, hop, 0f);
            transform.localScale = new Vector3(_size * (1f + breathe), _size * (1f - breathe), 1f);
        }
    }

    /// <summary>Gradient background with slowly rising candy bubbles in the theme colours.</summary>
    public sealed class BackgroundView : MonoBehaviour
    {
        private SpriteRenderer _gradient;
        private SpriteRenderer _grid;
        private Color _gridColor;
        private readonly List<SpriteRenderer> _bubbles = new List<SpriteRenderer>();
        private readonly List<Vector3> _bubbleVel = new List<Vector3>();
        private Camera _camera;

        public void Build(Camera cam, int bubbleCount = 22)
        {
            _camera = cam;
            var gradientGo = new GameObject("Gradient");
            gradientGo.transform.SetParent(transform, false);
            _gradient = SpriteMaterial.Add(gradientGo);
            _gradient.sortingOrder = -100;
            var gridGo = new GameObject("Grid");
            gridGo.transform.SetParent(transform, false);
            _grid = SpriteMaterial.Add(gridGo);
            _grid.sortingOrder = -95;
            _grid.enabled = false;
            var dot = SpriteFactory.SoftDot();
            for (int i = 0; i < bubbleCount; i++)
            {
                var go = new GameObject("bubble");
                go.transform.SetParent(transform, false);
                var r = SpriteMaterial.Add(go);
                r.sprite = dot;
                r.sortingOrder = -90;
                _bubbles.Add(r);
                _bubbleVel.Add(Vector3.zero);
            }
            Scatter(true);
        }

        public void SetTheme(ShopItem theme)
        {
            var top = ColorUtil.Hex(theme.BackgroundTop);
            var bottom = ColorUtil.Hex(theme.BackgroundBottom);
            var accent = ColorUtil.Hex(theme.Accent);
            _gradient.sprite = SpriteFactory.Gradient(top, bottom);
            _grid.enabled = theme.Grid;
            _gridColor = accent;
            if (theme.Grid) _grid.sprite = SpriteFactory.NeonGrid(Color.white);
            for (int i = 0; i < _bubbles.Count; i++)
            {
                var c = i % 3 == 0 ? Color.white : i % 3 == 1 ? accent : Color.Lerp(top, Color.white, 0.5f);
                c.a = Random.Range(0.12f, 0.3f);
                _bubbles[i].color = c;
            }
            FitToCamera();
        }

        public void FitToCamera()
        {
            if (_camera == null || _gradient.sprite == null) return;
            float h = _camera.orthographicSize * 2f + 2f;
            float w = h * _camera.aspect + 2f;
            var size = _gradient.sprite.bounds.size;
            transform.position = new Vector3(_camera.transform.position.x, _camera.transform.position.y, 10f);
            _gradient.transform.localScale = new Vector3(w / size.x, h / size.y, 1f);
            if (_grid.sprite != null)
            {
                var gs = _grid.sprite.bounds.size;
                float scale = w / gs.x;
                _grid.transform.localScale = new Vector3(scale, scale, 1f);
                _grid.transform.localPosition = new Vector3(0f, -h / 2f + gs.y * scale / 2f, 0f);
            }
        }

        private Vector2 HalfExtents => _camera == null
            ? new Vector2(8, 14)
            : new Vector2(_camera.orthographicSize * _camera.aspect, _camera.orthographicSize);

        private void Scatter(bool anywhere)
        {
            var half = HalfExtents;
            for (int i = 0; i < _bubbles.Count; i++) Respawn(i, anywhere, half);
        }

        private void Respawn(int i, bool anywhere, Vector2 half)
        {
            var t = _bubbles[i].transform;
            float scale = Random.Range(0.6f, 2.6f);
            t.localScale = Vector3.one * scale;
            float y = anywhere ? Random.Range(-half.y, half.y) : -half.y - 2f;
            t.localPosition = new Vector3(Random.Range(-half.x, half.x), y, 0f);
            _bubbleVel[i] = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.4f, 1.3f) / Mathf.Sqrt(scale), 0f);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            var half = HalfExtents;
            if (_grid != null && _grid.enabled)
                _grid.color = PixelCanvas.WithAlpha(_gridColor, 0.32f + 0.08f * Mathf.Sin(Time.unscaledTime * 1.5f));
            for (int i = 0; i < _bubbles.Count; i++)
            {
                var t = _bubbles[i].transform;
                var p = t.localPosition + _bubbleVel[i] * dt;
                p.x += Mathf.Sin(Time.unscaledTime * 0.7f + i) * 0.15f * dt;
                t.localPosition = p;
                if (p.y > half.y + 2f) Respawn(i, false, half);
            }
        }
    }
}
