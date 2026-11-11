#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace FOC.Editor
{
    public static class AndroidBuildScript
    {
        private const string PackageName = "com.focreborn.mobile";
        private const string ProductName = "FOC Reborn Mobile";
        private const string Version = "0.1.0";
        private const int VersionCode = 1;
        private const string BuildOutputDirectory = "Builds/Android";
        private const string ApkFileName = "FOC_Reborn_Mobile.apk";

        [MenuItem("FOC/Build/Build Android APK (ARM64)")]
        public static void PerformBuild()
        {
            Debug.Log("[FOC Build] === STARTING ANDROID BUILD PIPELINE ===");
            var startTime = DateTime.Now;

            // 0. Ensure Android SDK, NDK, and JDK toolchains are explicitly configured
            EditorPrefs.SetString("AndroidSdkRoot", "D:/Android/Sdk");
            EditorPrefs.SetString("AndroidNdkRoot", "D:/Android/Ndk/android-ndk-r23b");
            EditorPrefs.SetString("JdkPath", "C:/Users/Technical/AppData/Local/Programs/Eclipse Adoptium/jdk-17.0.20.101-hotspot");
            EditorPrefs.SetBool("SdkUseEmbedded", false);
            EditorPrefs.SetBool("NdkUseEmbedded", false);
            EditorPrefs.SetBool("JdkUseEmbedded", false);
            Debug.Log("[FOC Build] Toolchains configured: SDK=D:/Android/Sdk, NDK=D:/Android/Ndk/android-ndk-r23b, JDK=Eclipse Adoptium 17");

            // 1. Configure Project Settings
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageName);
            PlayerSettings.productName = ProductName;
            PlayerSettings.companyName = "FOC Studio";
            PlayerSettings.bundleVersion = Version;
            PlayerSettings.Android.bundleVersionCode = VersionCode;

            // 2. Configure Orientation & Splash
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // 3. Configure Android Architecture & APIs
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34; // Android 14/15 target
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // 4. Graphics APIs (Vulkan primary, OpenGLES3 fallback)
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[]
            {
                UnityEngine.Rendering.GraphicsDeviceType.Vulkan,
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3
            });

            // 5. Gather Enabled Scenes
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[FOC Build] FATAL: No enabled scenes found in EditorBuildSettings!");
                return;
            }

            Debug.Log($"[FOC Build] Registered {scenes.Length} scenes for build:");
            for (int i = 0; i < scenes.Length; i++)
            {
                Debug.Log($"  [{i}] {scenes[i]}");
            }

            // 6. Ensure Output Directory on Drive D:
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string outputDir = Path.Combine(projectRoot, BuildOutputDirectory);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            string apkPath = Path.Combine(outputDir, ApkFileName);

            // 7. Execute Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[FOC Build] Building APK to destination: {apkPath}");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            var duration = DateTime.Now - startTime;
            if (summary.result == BuildResult.Succeeded)
            {
                long fileSize = File.Exists(apkPath) ? new FileInfo(apkPath).Length : 0;
                Debug.Log($"[FOC Build] SUCCESS: Android APK compiled in {duration.TotalSeconds:F1}s.");
                Debug.Log($"[FOC Build] Output: {apkPath} ({fileSize:N0} bytes)");
            }
            else
            {
                Debug.LogError($"[FOC Build] FAILED: Android build ended with result '{summary.result}'.");
                Debug.LogError($"[FOC Build] Errors: {summary.totalErrors}, Warnings: {summary.totalWarnings}");
            }
        }
    }
}
#endif
