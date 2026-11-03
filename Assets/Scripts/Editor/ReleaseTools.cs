#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace JellyDrop.EditorTools
{
    /// <summary>
    /// Google Play release helpers (menu Stackville > Release):
    /// check the build settings, bump the version code, build the signed .aab, and capture store screenshots.
    /// </summary>
    public static class ReleaseTools
    {
        public const string VersionName = "1.0.0";
        public const int TargetApi = 36;
        private const string BuildDir = "Builds";
        private const string ScreenshotDir = "Store/screenshots/raw";

        [MenuItem("Stackville/Release/1. Check release settings", priority = 100)]
        public static void Check()
        {
            var problems = new System.Collections.Generic.List<string>();
            var ok = new System.Collections.Generic.List<string>();
            string id = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            ok.Add("Package: " + id);
            ok.Add($"Version: {PlayerSettings.bundleVersion} (code {PlayerSettings.Android.bundleVersionCode})");
            if ((int)PlayerSettings.Android.targetSdkVersion != TargetApi && PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevelAuto)
                problems.Add($"Target API is {(int)PlayerSettings.Android.targetSdkVersion}, Google Play needs {TargetApi}.");
            if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP) problems.Add("Scripting backend must be IL2CPP.");
            if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0) problems.Add("ARM64 must be ticked (64-bit is required).");
            if (!EditorUserBuildSettings.buildAppBundle) problems.Add("Build App Bundle (.aab) is off.");
            if (!PlayerSettings.Android.useCustomKeystore || string.IsNullOrEmpty(PlayerSettings.Android.keystoreName))
                problems.Add("No upload keystore: Player Settings > Android > Publishing Settings > Keystore Manager.");
            var defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
            if (!defines.Contains("ADMOB")) problems.Add("ADMOB is off: the game would ship with fake ads (no income).");
            if (!defines.Contains("UNITY_PURCHASING")) problems.Add("UNITY_PURCHASING is off: purchases would use the fake store.");
            if (JellyDrop.Monetization.MonetizationIds.RewardedAdUnit.Contains("3940256099942544"))
                problems.Add("Ad unit IDs are Google's TEST IDs: put your own in MonetizationIds.cs.");
            if (EditorUserBuildSettings.development) problems.Add("Development Build is ticked.");

            string msg = string.Join("\n", ok) + "\n\n" + (problems.Count == 0 ? "All good - ready to build!" : "To fix:\n- " + string.Join("\n- ", problems));
            Debug.Log("[Stackville] Release check:\n" + msg);
            if (!Application.isBatchMode) EditorUtility.DisplayDialog("Stackville release check", msg, "OK");
        }

        [MenuItem("Stackville/Release/2. Bump version code (+1)", priority = 101)]
        public static void BumpVersionCode()
        {
            PlayerSettings.Android.bundleVersionCode++;
            PlayerSettings.bundleVersion = VersionName;
            AssetDatabase.SaveAssets();
            Debug.Log($"[Stackville] Version code is now {PlayerSettings.Android.bundleVersionCode} (version {VersionName}).");
        }

        [MenuItem("Stackville/Release/3. Build release .aab", priority = 102)]
        public static void BuildAab()
        {
            JellyDropSetup.ApplySettings();
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.development = false;
            Directory.CreateDirectory(BuildDir);
            string path = Path.Combine(BuildDir, $"Stackville-{VersionName}-{PlayerSettings.Android.bundleVersionCode}.aab");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { JellyDropSetup.ScenePath },
                locationPathName = path,
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Stackville] Built {path} ({report.summary.totalSize / (1024 * 1024)} MB). Upload it in Play Console > Test and release.");
                EditorUtility.RevealInFinder(path);
            }
            else Debug.LogError("[Stackville] Build failed: " + report.summary.result);
        }

        /// <summary>
        /// Saves a 1080 x 1920 screenshot of the Game view while playing. Set the Game view to 1080x1920 first
        /// (Game tab > resolution dropdown > + > 1080 x 1920). Files go to Store/screenshots/raw/01.png, 02.png...
        /// </summary>
        [MenuItem("Stackville/Release/Capture store screenshot (Play mode)  %#k", priority = 110)]
        public static void CaptureScreenshot()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Stackville", "Press Play first, open the screen you want, then capture (Ctrl/Cmd+Shift+K).", "OK");
                return;
            }
            Directory.CreateDirectory(ScreenshotDir);
            int n = 1;
            while (File.Exists(Path.Combine(ScreenshotDir, $"{n:00}.png"))) n++;
            string file = Path.Combine(ScreenshotDir, $"{n:00}.png");
            ScreenCapture.CaptureScreenshot(file);
            Debug.Log($"[Stackville] Screenshot saved to {file} (Game view size: {Screen.width}x{Screen.height}). Then run Tools/StoreAssets/make_store_assets.py to add captions.");
        }
    }
}
#endif
