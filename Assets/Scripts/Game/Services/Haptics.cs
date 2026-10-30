using System;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace JellyDrop.Services
{
    public enum HapticStrength { Light = 0, Medium = 1, Heavy = 2 }

    /// <summary>
    /// Short, crisp haptic taps like modern phones use for UI and game feel.
    /// Android: VibrationEffect (predefined tick/click on Android 10+, short one-shots on 8–9).
    /// iOS: UIImpactFeedbackGenerator through Plugins/iOS/JellyDropHaptics.mm.
    /// The editor does nothing.
    /// </summary>
    public static class Haptics
    {
        public static bool Enabled = true;
        private static float _lastTap = -10f;
        private static float _lastBig = -10f;

        /// <summary>Tiny tick: button presses, piece rotation and locking.</summary>
        public static void Tap() => Impact(HapticStrength.Light, 0.04f);

        /// <summary>Line clears, purchases.</summary>
        public static void Medium() => Impact(HapticStrength.Medium, 0.06f);

        /// <summary>Quads, jackpots, level complete.</summary>
        public static void Big()
        {
            if (!Enabled || Time.realtimeSinceStartup - _lastBig < 0.25f) return;
            _lastBig = Time.realtimeSinceStartup;
            Impact(HapticStrength.Heavy, 0f);
        }

        private static void Impact(HapticStrength strength, float minInterval)
        {
            if (!Enabled) return;
            float now = Time.realtimeSinceStartup;
            if (minInterval > 0f && now - _lastTap < minInterval) return;
            _lastTap = now;
            try { Native(strength); }
            catch (Exception e) { Debug.LogWarning("[Haptics] " + e.Message); Enabled = false; }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject _vibrator;
        private static AndroidJavaClass _effect;
        private static int _sdk;

        private static void Native(HapticStrength strength)
        {
            if (_vibrator == null)
            {
                using (var version = new AndroidJavaClass("android.os.Build$VERSION")) _sdk = version.GetStatic<int>("SDK_INT");
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (_sdk >= 26) _effect = new AndroidJavaClass("android.os.VibrationEffect");
            }
            if (_vibrator == null || !_vibrator.Call<bool>("hasVibrator")) return;

            if (_sdk >= 29)
            {
                // EFFECT_TICK = 2, EFFECT_CLICK = 0, EFFECT_HEAVY_CLICK = 5
                int id = strength == HapticStrength.Light ? 2 : strength == HapticStrength.Medium ? 0 : 5;
                using (var e = _effect.CallStatic<AndroidJavaObject>("createPredefined", id)) _vibrator.Call("vibrate", e);
            }
            else if (_sdk >= 26)
            {
                long ms = strength == HapticStrength.Light ? 12 : strength == HapticStrength.Medium ? 25 : 45;
                int amp = strength == HapticStrength.Light ? 60 : strength == HapticStrength.Medium ? 140 : 255;
                using (var e = _effect.CallStatic<AndroidJavaObject>("createOneShot", ms, amp)) _vibrator.Call("vibrate", e);
            }
            else if (strength == HapticStrength.Heavy)
            {
                Handheld.Vibrate();
            }
        }
#elif UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void JD_HapticImpact(int style);

        private static void Native(HapticStrength strength) => JD_HapticImpact((int)strength);
#else
        private static void Native(HapticStrength strength) { }
#endif
    }
}
