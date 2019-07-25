using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
public class AutoBuilder : ScriptableObject
{
    static string[] SCENES = FindEnabledEditorScenes();
    static string APP_NAME = "gsk_"; // 2019_07_12 lhamed  edit typo. 
    static string TARGET_DIR;
    static string ProjectPath
    {
        get { return Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')); }
    }
    [MenuItem("AutoBuild/Debug")]
    public static void BuildIL2CPP_Debug()
    {
        SetBuildInfo();
        SetAndroidInfo();
        APP_NAME = "app_cpp_" + PlayerSettings.Android.bundleVersionCode + ".apk";
        TARGET_DIR = ProjectPath + "/" + "Output";
        Directory.CreateDirectory(TARGET_DIR);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        EditorUserBuildSettings.androidBuildType = AndroidBuildType.Debug;
        BuildAndroid(SCENES, TARGET_DIR + "/" + APP_NAME, BuildTargetGroup.Android, BuildTarget.Android);
    }

    [MenuItem("AutoBuild/IL2CPP")]
    public static void BuildIL2CPP()
    {
        SetBuildInfo();
        SetAndroidInfo();
        APP_NAME = "app_cpp_" + PlayerSettings.Android.bundleVersionCode + ".apk";
        TARGET_DIR = ProjectPath + "/" + "Output";
        Directory.CreateDirectory(TARGET_DIR);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;
        BuildAndroid(SCENES, TARGET_DIR + "/" + APP_NAME, BuildTargetGroup.Android, BuildTarget.Android);
    }

    [MenuItem("AutoBuild/Mono")]
    public static void BuildMono()
    {
        SetBuildInfo();
        SetAndroidInfo();
        APP_NAME = "app_mono_" + PlayerSettings.Android.bundleVersionCode + ".apk";
        TARGET_DIR = ProjectPath + "/" + "Output";
        Directory.CreateDirectory(TARGET_DIR);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;
        BuildAndroid(SCENES, TARGET_DIR + "/" + APP_NAME, BuildTargetGroup.Android, BuildTarget.Android);
    }

    [MenuItem("AutoBuild/iOS")]
    public static void BuildIOS()
    {
        //ios 유니티 버전 옮기면서 꼬임 ㅎㅁㅎ;; 
        // PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        // SetBuildInfo();
        // SetiOSInfo();
        // char sep = Path.DirectorySeparatorChar;
        // string BUILD_TARGET_PATH = ProjectPath + "/ios";
        // Directory.CreateDirectory(BUILD_TARGET_PATH);
        // PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        // try
        // {
        //     EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        //     BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        // buildPlayerOptions.scenes = scenes;
        // buildPlayerOptions.locationPathName = app_target;
        // buildPlayerOptions.target = BuildTarget.iOS;
        // buildPlayerOptions.options = BuildOptions.None; 
        //     string res = BuildPipeline.BuildPlayer(BuildOptions.None);
        //     if (res.Length > 0) { throw new Exception("BuildPlayer failure: " + res); }
        // }
        // catch (System.Exception e)
        // {
        //     Debug.Log(e.Message);
        // }
    }

    private static void BuildAndroid(string[] scenes, string app_target, BuildTargetGroup build_target_group, BuildTarget build_target)
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        PlayerSettings.Android.androidIsGame = true;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = app_target;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log(report);
    }

    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }

    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    private static void SetArg(string key, string value)
    {
        System.Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.User);
    }

    static void SetBuildInfo()
    {
        string tempBundleVersion = GetArg("-BUNDLE_VERSION");
        PlayerSettings.bundleVersion = tempBundleVersion;
    }

    static void SetAndroidInfo()
    {
        string tempVersionCodeString = GetArg("-VERSION_CODE");
        PlayerSettings.Android.bundleVersionCode = int.Parse(tempVersionCodeString);
        // PlayerSettings.Android.keyaliasName = "<key>";
        // PlayerSettings.Android.keystoreName = "keystorefile";
        // PlayerSettings.Android.keyaliasPass = GetArg("-pass");
        // PlayerSettings.Android.keystorePass = GetArg("-pass");
    }

    static void SetiOSInfo()
    {
        PlayerSettings.iOS.buildNumber = (GetArg("-VERSION_CODE"));
    }
}