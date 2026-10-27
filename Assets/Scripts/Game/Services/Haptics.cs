using UnityEngine;

namespace JellyDrop.Services
{
    /// <summary>
    /// Vibration feedback. Unity's built-in vibrate is a single long buzz, so it is only used for
    /// big moments. For fine taps, add a native haptics plugin later (e.g. "Nice Vibrations").
    /// </summary>
    public static class Haptics
    {
        public static bool Enabled = true;
        private static float _lastTime = -10f;

        public static void Big()
        {
            if (!Enabled) return;
            if (Time.realtimeSinceStartup - _lastTime < 0.5f) return;
            _lastTime = Time.realtimeSinceStartup;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
