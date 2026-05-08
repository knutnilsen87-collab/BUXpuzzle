#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public static class FBL_BuildWindows
{
    [MenuItem("FBL/Build/Windows x64 (RC)")]
    public static void BuildMenu() => Build();

    public static void Build()
    {
        string outDir = Path.GetFullPath("Build/Windows");
        Directory.CreateDirectory(outDir);

        string exe = Path.Combine(outDir, "BUXPuzzle.exe");
        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogError("[FBL] No scenes in Build Settings.");
            return;
        }

        var opts = new BuildPlayerOptions
        {
            scenes = System.Array.ConvertAll(scenes, s => s.path),
            locationPathName = exe,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("[FBL] Build result: " + report.summary.result + " / size=" + report.summary.totalSize);

        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("[FBL] Build failed: " + report.summary.result);
    }
}
#endif