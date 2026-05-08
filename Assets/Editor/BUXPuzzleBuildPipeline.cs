using System.IO;
using UnityEditor;
using UnityEngine;

public static class BUXPuzzleBuildPipeline
{
    private static readonly string[] Scenes = { "Assets/Scenes/game.unity" };

    public static void BuildWindows()
    {
        Directory.CreateDirectory("Builds/Windows");
        Build(
            BuildTarget.StandaloneWindows64,
            "Builds/Windows/BUXPuzzle.exe",
            BuildOptions.None);
    }

    public static void BuildAndroidAab()
    {
        Directory.CreateDirectory("Builds/Android");
        EditorUserBuildSettings.buildAppBundle = true;
        Build(
            BuildTarget.Android,
            "Builds/Android/BUXPuzzle.aab",
            BuildOptions.None);
    }

    public static void BuildIosXcode()
    {
        Directory.CreateDirectory("Builds/iOS");
        Build(
            BuildTarget.iOS,
            "Builds/iOS",
            BuildOptions.None);
    }

    private static void Build(BuildTarget target, string location, BuildOptions options)
    {
        var report = BuildPipeline.BuildPlayer(Scenes, location, target, options);
        var summary = report.summary;

        Debug.Log($"[BUX_BUILD] target={target} result={summary.result} errors={summary.totalErrors} warnings={summary.totalWarnings} size={summary.totalSize} path={location}");

        if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
            return;
        }

        EditorApplication.Exit(0);
    }
}
