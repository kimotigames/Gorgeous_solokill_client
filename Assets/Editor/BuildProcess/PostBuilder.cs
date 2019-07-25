#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.iOS.Xcode;
using UnityEngine;
class PostBuilder : IPostprocessBuild
{
    public int callbackOrder { get { return 0; } }

    static string ProjectPath
    {
        get { return Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')); }
    }
    public void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        // Stop processing if targe is NOT iOS 
        if (buildTarget != BuildTarget.iOS) return;
        // Initialize PbxProject 
        var projectPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
        PBXProject pbxProject = new PBXProject();
        pbxProject.ReadFromFile(projectPath);
        string targetGuid = pbxProject.TargetGuidByName("Unity-iPhone");
        // Sample of adding build property 
        // pbxProject.AddBuildProperty (targetGuid, "OTHER_LDFLAGS", "-all_load");
        // Sample of setting build property 
        // pbxProject.SetBuildProperty (targetGuid, "ENABLE_BITCODE", "NO");
        // Sample of update build property 
        // pbxProject.UpdateBuildProperty (targetGuid, "OTHER_LDFLAGS", new string[] { "-ObjC" }, new string[] { "-weak_framework" });
        // Sample of adding REQUIRED framwrok 
        // pbxProject.AddFrameworkToProject (targetGuid, " Security.framework ", false);
        // Sample of adding OPTIONAL framework
        // pbxProject.AddFrameworkToProject (targetGuid, " SafariServices.framework ", true);
        // Sample of setting compile flags 
        pbxProject.AddCapability(targetGuid, PBXCapabilityType.InAppPurchase);
        pbxProject.AddCapability(targetGuid, PBXCapabilityType.iCloud);
        pbxProject.AddCapability(targetGuid, PBXCapabilityType.GameCenter);
        pbxProject.AddCapability(targetGuid, PBXCapabilityType.PushNotifications);
        // pbxProject.SetBuildProperty(targetGuid, "DEVELOPMENT_TEAM", "<set developement key here>");
        var guid = pbxProject.FindFileGuidByProjectPath("Classes/UI/Keyboard.mm");
        var flags = pbxProject.GetCompileFlagsForFile(targetGuid, guid);
        flags.Add("-fno-objc-arc");
        pbxProject.SetCompileFlagsForFile(targetGuid, guid, flags);
        pbxProject.AddFrameworkToProject(targetGuid, "CloudKit.framework", false);

        File.WriteAllText(projectPath, pbxProject.WriteToString());
        // Samlpe of editing Info.plist 
        var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        // Add string setting 
        plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        // Add URL Scheme\
        // Apply editing settings to Info.plist
        var cap = plist.root.CreateArray("UIRequiredDeviceCapabilities");
        cap.AddString("gamekit");
        plist.WriteToFile(plistPath);


        //-----

        var file_name = "unity.entitlements";
        var proj_path = projectPath;
        var proj = new PBXProject();
        proj.ReadFromFile(proj_path);


        // target_name = "Unity-iPhone"
        var target_name = PBXProject.GetUnityTargetName();
        var target_guid = proj.TargetGuidByName(target_name);
        var dst = pathToBuiltProject + "/" + target_name + "/" + file_name;
        try
        {
            File.WriteAllText(dst, entitlements);
            proj.AddFile(target_name + "/" + file_name, file_name);
            proj.AddBuildProperty(target_guid, "CODE_SIGN_ENTITLEMENTS", target_name + "/" + file_name);
            proj.WriteToFile(proj_path);
        }
        catch (IOException e)
        {
            Debug.Log("Could not copy entitlements. Probably already exists. " + e);
        }
        UnityEditor.iOS.Xcode.ProjectCapabilityManager pcm = new UnityEditor.iOS.Xcode.ProjectCapabilityManager(proj_path, dst, target_name);
        pcm.AddPushNotifications(false);
    }

    private const string entitlements = @"
     <?xml version=""1.0"" encoding=""UTF-8\""?>
     <!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
     <plist version=""1.0"">
         <dict>
             <key>aps-environment</key>
             <string>production</string>
         </dict>
     </plist>";
}
#endif