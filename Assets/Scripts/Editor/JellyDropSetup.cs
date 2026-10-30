#if UNITY_EDITOR
using System.IO;
using JellyDrop.Core.Economy;
using JellyDrop.Visuals;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace JellyDrop.EditorTools
{
    /// <summary>
    /// One-click project setup: Stackville > Setup Project.
    /// Sets app name/IDs/orientation, Android IL2CPP + ARM64, creates the Main scene and the app icon.
    /// </summary>
    public static class JellyDropSetup
    {
        // Change these before publishing. The package name can't be changed after the first upload.
        private const string Company = "Wojciech Buda";
        private const string Product = "Stackville";
        private const string BundleId = "com.wojciechbuda.stackville";
        public const string ScenePath = "Assets/Scenes/Main.unity";
        private const string IconPath = "Assets/Art/AppIcon.png";

        [MenuItem("Stackville/Setup Project", priority = 0)]
        public static void SetupProject()
        {
            ApplySettings();
            Debug.Log("[Stackville] Project set up. Press Play in Assets/Scenes/Main.unity.");
            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("Stackville", "Project set up!\n\nOpen Assets/Scenes/Main.unity and press Play.", "OK");
        }

        /// <summary>Applies all player settings, creates the scene and icon. No dialogs, so CI builds can call it.</summary>
        public static void ApplySettings()
        {
            PlayerSettings.companyName = Company;
            PlayerSettings.productName = Product;
            PlayerSettings.bundleVersion = "0.1.0";

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);

            // Portrait only
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // Android: Google Play requires 64-bit (ARM64) and an AAB upload.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            // Android 8.0+ (API 26); newer Unity versions dropped the older levels.
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)26;
            PlayerSettings.Android.bundleVersionCode = 1;
            EditorUserBuildSettings.buildAppBundle = true;

            // iOS
            PlayerSettings.iOS.buildNumber = "1";
            PlayerSettings.iOS.targetOSVersionString = "13.0";

            PlayerSettings.SplashScreen.backgroundColor = ColorUtil.Night;

            CreateMainScene();
            GenerateIcon();

            AssetDatabase.SaveAssets();
        }

        private static void CreateMainScene()
        {
            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Stackville/Generate App Icon", priority = 10)]
        public static void GenerateIcon()
        {
            const int size = 1024;
            PixelCanvas.KeepReadable = true;
            SpriteFactory.ClearCache();
            try
            {
                var canvas = new PixelCanvas(size, size);
                var top = ColorUtil.Hex("#2A0B4F");
                var bottom = ColorUtil.Hex("#0B1033");
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        canvas.Set(x, y, Color.Lerp(bottom, top, y / (float)size));

                // Synthwave grid and a few neon blocks
                var catalog = Catalog.CreateDefault();
                Stamp(canvas, SpriteFactory.NeonGrid(ColorUtil.Pink).texture, 0, 0, size, size / 2);
                var skin = catalog.Get("skin_neon");
                Stamp(canvas, SpriteFactory.Block(BlockStyle.Neon, ColorUtil.Hex(skin.Palette[0])).texture, 60, 60, 260);
                Stamp(canvas, SpriteFactory.Block(BlockStyle.Neon, ColorUtil.Hex(skin.Palette[1])).texture, 320, 60, 260);
                Stamp(canvas, SpriteFactory.Block(BlockStyle.Neon, ColorUtil.Hex(skin.Palette[3])).texture, 704, 60, 260);
                Stamp(canvas, SpriteFactory.Block(BlockStyle.Neon, ColorUtil.Hex(skin.Palette[2])).texture, 704, 320, 260);

                // Mascot
                var avatar = SpriteFactory.Avatar(catalog.Get("avatar_bloop"), AvatarMood.Happy).texture;
                Stamp(canvas, avatar, 150, 260, 720, 720);

                var tex = canvas.ToTexture();
                Directory.CreateDirectory(Path.GetDirectoryName(IconPath));
                File.WriteAllBytes(IconPath, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(IconPath);
                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
                PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
                Debug.Log("[Stackville] App icon written to " + IconPath);
            }
            finally
            {
                PixelCanvas.KeepReadable = false;
                SpriteFactory.ClearCache();
            }
        }

        private static void Stamp(PixelCanvas canvas, Texture2D src, int x0, int y0, int size) => Stamp(canvas, src, x0, y0, size, size);

        private static void Stamp(PixelCanvas canvas, Texture2D src, int x0, int y0, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var c = src.GetPixelBilinear((x + 0.5f) / width, (y + 0.5f) / height);
                    canvas.Blend(x0 + x, y0 + y, c);
                }
            }
        }

        // ------------------------------------------------------------------ SDK switches

        [MenuItem("Stackville/Ads: enable AdMob (after importing Google Mobile Ads)", priority = 30)]
        public static void EnableAdMob() => SetDefine("ADMOB", true);

        [MenuItem("Stackville/Ads: use test ads only", priority = 31)]
        public static void DisableAdMob() => SetDefine("ADMOB", false);

        [MenuItem("Stackville/IAP: enable Unity IAP (after installing In-App Purchasing)", priority = 32)]
        public static void EnableIap() => SetDefine("UNITY_PURCHASING", true);

        [MenuItem("Stackville/IAP: use fake store", priority = 33)]
        public static void DisableIap() => SetDefine("UNITY_PURCHASING", false);

        [MenuItem("Stackville/Reset saved progress", priority = 50)]
        public static void ResetProgress()
        {
            if (!EditorUtility.DisplayDialog("Stackville", "Delete all saved coins, items and settings?", "Delete", "Cancel")) return;
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[Stackville] Progress reset.");
        }

        private static void SetDefine(string define, bool enabled)
        {
            foreach (var target in new[] { NamedBuildTarget.Android, NamedBuildTarget.iOS })
            {
                var defines = PlayerSettings.GetScriptingDefineSymbols(target);
                var list = new System.Collections.Generic.List<string>(defines.Split(';'));
                list.RemoveAll(string.IsNullOrWhiteSpace);
                if (enabled && !list.Contains(define)) list.Add(define);
                if (!enabled) list.Remove(define);
                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", list));
            }
            Debug.Log($"[Stackville] {define} {(enabled ? "enabled" : "disabled")} for Android and iOS.");
        }
    }
}
#endif
