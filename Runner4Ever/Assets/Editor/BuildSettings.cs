using UnityEditor;
using UnityEngine;
using System.Collections;
 
public class BuildScript: MonoBehaviour
{
     static void Start()
     {
         string[] scenes = {"Assets/Project.unity" };
         BuildPipeline.BuildPlayer(scenes, "StandaloneWindows", BuildTarget.StandaloneWindows, BuildOptions.None);
     }

     public static void BuildAndroid()
    {
        string[] scenes = {"Assets/Project.unity" };

		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = @"c:\temp\test.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}

/*
C:\Program Files\Unity\Editor>Unity.exe -projectPath C:\Boulot\Runner4Ever\Runner4Ever -buildTarget android -executeMethod BuildScript.BuildAndroid -logFile c:\temp\build.log -batchmode -quit c:\temp
*/