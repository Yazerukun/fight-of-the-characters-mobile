/* === 08. INPUT CONTROLLER, EVENT LISTENERS & BOOTSTRAP === */
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class UnityAndroidBuildScript
{
    public static void PerformAndroidBuild()
    {
        Debug.Log("[AndroidBuildPipeline] Starting Fight of Characters 9.1a Asia ARM64 Build...");

        string projectDir = Directory.GetCurrentDirectory();
        string buildDir = Path.Combine(projectDir, "AndroidBuild");
        Directory.CreateDirectory(buildDir);

        string apkPath = Path.Combine(buildDir, "FOC_9.1a_Asia_Release.apk");

        // Player & Engine Settings
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.foc.reborn.asia");
        PlayerSettings.productName = "Fight of Characters 9.1a Asia";
        PlayerSettings.companyName = "FOC Reborn Team";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;

        // Target Architectures
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingBackend.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26; // Android 8.0 Oreo
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34; // Android 14

        // Scenes to build
        string[] scenes = new string[]
        {
            "Assets/Scenes/FOC_LoadingScreen.unity",
            "Assets/Scenes/FOC_HeroPickingPhase.unity",
            "Assets/Scenes/FOC_BattleArena.unity"
        };

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[AndroidBuildPipeline] BUILD SUCCESSFUL! Output: {apkPath} ({summary.totalSize} bytes)");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"[AndroidBuildPipeline] BUILD FAILED! Errors: {summary.totalErrors}");
        }
    }
}
#endif
