using System;
using System.Collections.Generic;
using UnityEngine;

namespace JellyDrop.Util
{
    public static class Ease
    {
        public static float Linear(float t) => t;
        public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);
        public static float InQuad(float t) => t * t;
        public static float OutCubic(float t) { float u = 1f - t; return 1f - u * u * u; }
        public static float InOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

        public static float OutBack(float t)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }

        public static float OutElastic(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            const float c4 = 2f * Mathf.PI / 3f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }

        /// <summary>Strong ease-out used for the lucky wheel spin.</summary>
        public static float OutQuint(float t) { float u = 1f - t; return 1f - u * u * u * u * u; }
    }

    /// <summary>
    /// Minimal tween runner (no package needed). Uses unscaled time so UI animates while the game is paused.
    /// </summary>
    public sealed class Tweener : MonoBehaviour
    {
        private sealed class Tween
        {
            public float Duration;
            public float Elapsed;
            public float Delay;
            public Func<float, float> Ease;
            public Action<float> Step;
            public Action Done;
            public UnityEngine.Object Owner;
            public bool HasOwner;
            public bool Cancelled;
        }

        public sealed class Handle
        {
            internal object Tween;
            public void Cancel()
            {
                if (Tween is Tween t) t.Cancelled = true;
            }
        }

        private static Tweener _instance;
        private readonly List<Tween> _tweens = new List<Tween>();
        private readonly List<Tween> _adding = new List<Tween>();

        private static Tweener Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("Tweener");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<Tweener>();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Runs <paramref name="step"/> with an eased 0..1 value over <paramref name="duration"/> seconds.
        /// If <paramref name="owner"/> is destroyed the tween stops silently.
        /// </summary>
        public static Handle Run(float duration, Action<float> step, Func<float, float> ease = null,
            Action done = null, float delay = 0f, UnityEngine.Object owner = null)
        {
            var tween = new Tween
            {
                Duration = Mathf.Max(0.0001f, duration),
                Step = step,
                Ease = ease ?? JellyDrop.Util.Ease.OutQuad,
                Done = done,
                Delay = delay,
                Owner = owner,
                HasOwner = owner != null
            };
            Instance._adding.Add(tween);
            return new Handle { Tween = tween };
        }

        /// <summary>Calls <paramref name="action"/> after a delay (unscaled seconds).</summary>
        public static Handle Delay(float seconds, Action action, UnityEngine.Object owner = null) =>
            Run(0.0001f, null, null, action, seconds, owner);

        private void Update()
        {
            if (_adding.Count > 0)
            {
                _tweens.AddRange(_adding);
                _adding.Clear();
            }

            float dt = Time.unscaledDeltaTime;
            for (int i = _tweens.Count - 1; i >= 0; i--)
            {
                var t = _tweens[i];
                if (t.Cancelled || (t.HasOwner && t.Owner == null))
                {
                    _tweens.RemoveAt(i);
                    continue;
                }
                if (t.Delay > 0f)
                {
                    t.Delay -= dt;
                    continue;
                }
                t.Elapsed += dt;
                float p = Mathf.Clamp01(t.Elapsed / t.Duration);
                try
                {
                    t.Step?.Invoke(t.Ease(p));
                    if (p >= 1f)
                    {
                        _tweens.RemoveAt(i);
                        t.Done?.Invoke();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    _tweens.Remove(t);
                }
            }
        }
    }
}
