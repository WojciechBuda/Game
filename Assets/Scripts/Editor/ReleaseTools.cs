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
            if (defines.Contains("ADMOB") && !ApplyAdMobAppId())
                problems.Add("AdMob App ID not set: Assets > Google Mobile Ads > Settings > Android App ID = " + JellyDrop.Monetization.MonetizationIds.AndroidAdMobAppId);
            if (EditorUserBuildSettings.development) problems.Add("Development Build is ticked.");

            string msg = string.Join("\n", ok) + "\n\n" + (problems.Count == 0 ? "All good - ready to build!" : "To fix:\n- " + string.Join("\n- ", problems));
            Debug.Log("[Stackville] Release check:\n" + msg);
            if (!Application.isBatchMode) EditorUtility.DisplayDialog("Stackville release check", msg, "OK");
        }

        private const string AdMobSettingsPath = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";

        /// <summary>
        /// Writes Stackville's AdMob app id into the Google Mobile Ads settings asset (created by the plugin:
        /// open Assets > Google Mobile Ads > Settings once). Returns false if the asset isn't there yet.
        /// </summary>
        [MenuItem("Stackville/Release/Apply AdMob App ID", priority = 105)]
        public static bool ApplyAdMobAppId()
        {
            string id = JellyDrop.Monetization.MonetizationIds.AndroidAdMobAppId;
            if (!File.Exists(AdMobSettingsPath))
            {
                Debug.LogWarning("[Stackville] Google Mobile Ads settings not found. Import the plugin, open Assets > Google Mobile Ads > Settings once, then run this again (or paste " + id + " into 'Android App ID').");
                return false;
            }
            var lines = File.ReadAllLines(AdMobSettingsPath);
            bool changed = false, found = false;
            for (int i = 0; i < lines.Length; i++)
            {
                var t = lines[i].TrimStart();
                if (!t.StartsWith("adMobAndroidAppId:")) continue;
                found = true;
                string indent = lines[i].Substring(0, lines[i].Length - t.Length);
                string wanted = indent + "adMobAndroidAppId: " + id;
                if (lines[i] != wanted) { lines[i] = wanted; changed = true; }
            }
            if (!found)
            {
                Debug.LogWarning("[Stackville] Couldn't find the Android App ID field in " + AdMobSettingsPath + ". Paste " + id + " into Assets > Google Mobile Ads > Settings > Android App ID.");
                return false;
            }
            if (changed)
            {
                File.WriteAllLines(AdMobSettingsPath, lines);
                AssetDatabase.ImportAsset(AdMobSettingsPath);
                Debug.Log("[Stackville] AdMob Android App ID set to " + id);
            }
            return true;
        }

        /// <summary>
        /// Rewrites the .meta files of imported plugins (Google Mobile Ads etc.) in the current Unity format.
        /// Fixes "contains a PluginImporter object at version 1, below the supported minimum (2)".
        /// </summary>
        [MenuItem("Stackville/Release/Upgrade plugin files (.meta)", priority = 106)]
        public static void UpgradePluginMetas()
        {
            var paths = new System.Collections.Generic.List<string>();
            foreach (var root in new[] { "Assets/Plugins", "Assets/GoogleMobileAds", "Assets/ExternalDependencyManager" })
            {
                if (!Directory.Exists(root)) continue;
                foreach (var f in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                {
                    if (f.EndsWith(".meta")) continue;
                    paths.Add(f.Replace('\\', '/'));
                }
            }
            AssetDatabase.ForceReserializeAssets(paths, ForceReserializeAssetsOptions.ReserializeMetadata);
            AssetDatabase.Refresh();
            Debug.Log($"[Stackville] Re-saved {paths.Count} plugin .meta files in the current format.");
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
            ApplyAdMobAppId();
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
