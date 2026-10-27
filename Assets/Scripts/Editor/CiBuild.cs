#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace JellyDrop.EditorTools
{
    /// <summary>
    /// Command-line builds, used by the GitHub Actions workflow (.github/workflows/android.yml).
    /// Locally: Unity -batchmode -quit -projectPath . -executeMethod JellyDrop.EditorTools.CiBuild.BuildAndroid
    /// Produces an installable APK signed with Unity's debug key (for testing, not for the Play Store).
    /// </summary>
    public static class CiBuild
    {
        public static void BuildAndroid()
        {
            JellyDropSetup.ApplySettings();

            // An APK installs directly on a phone; the Play Store needs an .aab instead.
            EditorUserBuildSettings.buildAppBundle = false;

            string runNumber = Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER");
            if (int.TryParse(runNumber, out int code) && code > 0)
            {
                PlayerSettings.Android.bundleVersionCode = code;
                PlayerSettings.bundleVersion = "0.1." + code;
            }

            string path = Arg("-customBuildPath") ?? "build/Android/JellyDrop.apk";
            if (!path.EndsWith(".apk", StringComparison.OrdinalIgnoreCase)) path = Path.Combine(path, "JellyDrop.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));

            var options = new BuildPlayerOptions
            {
                scenes = new[] { JellyDropSetup.ScenePath },
                locationPathName = path,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log($"[Jelly Drop] Android build {summary.result}: {path} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalErrors} errors)");
            if (summary.result != BuildResult.Succeeded && Application.isBatchMode) EditorApplication.Exit(1);
        }

        private static string Arg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }
    }
}
#endif
