#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace JellyDrop.EditorTools
{
    /// <summary>
    /// Fills in Info.plist entries the App Store needs after Unity writes the Xcode project:
    /// the App Tracking Transparency text (shown by Google's consent flow before personalised ads),
    /// the AdMob app id, and "no custom encryption" so uploads skip the export-compliance question.
    /// </summary>
    public static class IosBuildPostProcess
    {
        [PostProcessBuild(100)]
        public static void OnPostprocessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;
            string plistPath = Path.Combine(path, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            var root = plist.root;

            root.SetString("NSUserTrackingUsageDescription",
                "Your data is used to show you more relevant ads and keep Stackville free. You can play either way.");
            if (root["GADApplicationIdentifier"] == null)
                root.SetString("GADApplicationIdentifier", JellyDrop.Monetization.MonetizationIds.IosAdMobAppId);
            root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            root.SetString("CFBundleDisplayName", "Stackville");

            plist.WriteToFile(plistPath);
        }
    }
}
#endif
