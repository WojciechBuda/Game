using System.Collections.Generic;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.Gameplay
{
    /// <summary>
    /// Pooled sprite particles (bursts, confetti, coin rain). Kept self-contained so it needs no
    /// particle-system materials or shaders, which keeps the project asset-free.
    /// </summary>
    public sealed class ParticleField : MonoBehaviour
    {
        private sealed class P
        {
            public SpriteRenderer R;
            public Vector3 Velocity;
            public float Life, MaxLife, Gravity, Spin, Size, Drag;
            public Color Color;
            public bool Shrink;
        }

        private readonly List<P> _active = new List<P>();
        private readonly Stack<P> _pool = new Stack<P>();
        private const int SortingOrder = 50;

        private P Get()
        {
            if (_pool.Count > 0)
            {
                var p = _pool.Pop();
                p.R.gameObject.SetActive(true);
                return p;
            }
            var go = new GameObject("p");
            go.transform.SetParent(transform, false);
            var r = SpriteMaterial.Add(go);
            r.sortingOrder = SortingOrder;
            return new P { R = r };
        }

        public void Burst(Vector3 pos, int count, Color[] colors, float speed = 6f, float life = 0.8f,
            float size = 0.35f, Sprite sprite = null, float gravity = -14f, bool shrink = true)
        {
            sprite = sprite != null ? sprite : SpriteFactory.SoftDot();
            for (int i = 0; i < count; i++)
            {
                var p = Get();
                float a = Random.Range(0f, Mathf.PI * 2f);
                float s = speed * Random.Range(0.35f, 1f);
                p.Velocity = new Vector3(Mathf.Cos(a) * s, Mathf.Sin(a) * s + speed * 0.3f, 0f);
                p.MaxLife = p.Life = life * Random.Range(0.7f, 1.2f);
                p.Gravity = gravity;
                p.Spin = Random.Range(-540f, 540f);
                p.Size = size * Random.Range(0.6f, 1.3f);
                p.Drag = 1.5f;
                p.Shrink = shrink;
                p.Color = colors[Random.Range(0, colors.Length)];
                p.R.sprite = sprite;
                p.R.color = p.Color;
                p.R.transform.position = pos + (Vector3)(Random.insideUnitCircle * 0.2f);
                p.R.transform.localScale = Vector3.one * p.Size;
                _active.Add(p);
            }
        }

        /// <summary>Colourful confetti falling from above the given rect (world space).</summary>
        public void Confetti(Rect area, int count)
        {
            var sprite = SpriteFactory.Confetti();
            for (int i = 0; i < count; i++)
            {
                var p = Get();
                p.Velocity = new Vector3(Random.Range(-2f, 2f), Random.Range(-4f, 2f), 0f);
                p.MaxLife = p.Life = Random.Range(1.8f, 3f);
                p.Gravity = -6f;
                p.Spin = Random.Range(-720f, 720f);
                p.Size = Random.Range(0.35f, 0.6f);
                p.Drag = 0.8f;
                p.Shrink = false;
                p.Color = ColorUtil.Rainbow[Random.Range(0, ColorUtil.Rainbow.Length)];
                p.R.sprite = sprite;
                p.R.color = p.Color;
                p.R.transform.position = new Vector3(Random.Range(area.xMin, area.xMax), area.yMax + Random.Range(0f, 3f), 0f);
                p.R.transform.localScale = Vector3.one * p.Size;
                _active.Add(p);
            }
        }

        /// <summary>Coins raining down over the area (jackpot!).</summary>
        public void CoinRain(Rect area, int count)
        {
            var sprite = SpriteFactory.Coin();
            for (int i = 0; i < count; i++)
            {
                var p = Get();
                p.Velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-6f, -1f), 0f);
                p.MaxLife = p.Life = Random.Range(1.5f, 2.6f);
                p.Gravity = -9f;
                p.Spin = Random.Range(-200f, 200f);
                p.Size = Random.Range(0.6f, 0.9f);
                p.Drag = 0.2f;
                p.Shrink = false;
                p.Color = Color.white;
                p.R.sprite = sprite;
                p.R.color = p.Color;
                p.R.transform.position = new Vector3(Random.Range(area.xMin, area.xMax), area.yMax + Random.Range(0f, 4f), 0f);
                p.R.transform.localScale = Vector3.one * p.Size;
                _active.Add(p);
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var p = _active[i];
                p.Life -= dt;
                if (p.Life <= 0f)
                {
                    p.R.gameObject.SetActive(false);
                    _active.RemoveAt(i);
                    _pool.Push(p);
                    continue;
                }
                p.Velocity.y += p.Gravity * dt;
                p.Velocity *= 1f - Mathf.Clamp01(p.Drag * dt);
                var t = p.R.transform;
                t.position += p.Velocity * dt;
                t.Rotate(0f, 0f, p.Spin * dt);
                float k = p.Life / p.MaxLife;
                if (p.Shrink) t.localScale = Vector3.one * (p.Size * k);
                var c = p.Color;
                c.a = Mathf.Clamp01(k * 2.5f) * p.Color.a;
                p.R.color = c;
            }
        }

        public void ClearAll()
        {
            foreach (var p in _active)
            {
                p.R.gameObject.SetActive(false);
                _pool.Push(p);
            }
            _active.Clear();
        }
    }

    /// <summary>Screen shake on the camera (unscaled time).</summary>
    public sealed class CameraShake : MonoBehaviour
    {
        private Vector3 _basePosition;
        private float _strength;
        private float _remaining;
        private float _duration;

        public void SetBasePosition(Vector3 position)
        {
            _basePosition = position;
            transform.position = position;
        }

        public void Shake(float strength, float duration)
        {
            if (strength < _strength * (_remaining / Mathf.Max(0.001f, _duration))) return;
            _strength = strength;
            _duration = _remaining = duration;
        }

        private void LateUpdate()
        {
            if (_remaining <= 0f)
            {
                transform.position = _basePosition;
                return;
            }
            _remaining -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(_remaining / _duration);
            var offset = (Vector3)(Random.insideUnitCircle * _strength * k * k);
            transform.position = _basePosition + offset;
        }
    }
}
