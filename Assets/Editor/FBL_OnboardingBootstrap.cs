#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

using Game.Core;
using Game.UI;

public static class FBL_OnboardingBootstrap
{
    [MenuItem("FBL/Onboarding/Apply Bootstrap")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
        {
            Debug.Log("[FBL] OnboardingBootstrap: STOP Play Mode first.");
            return;
        }

        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0)
        {
            // pick any scene if build settings empty
            var any = AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
            if (!string.IsNullOrEmpty(any))
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(any, true) };
            scenes = EditorBuildSettings.scenes;
        }

        var target = scenes[0].path;
        EditorSceneManager.OpenScene(target, OpenSceneMode.Single);

        var root = GameObject.Find("FBL_BOOTSTRAP") ?? new GameObject("FBL_BOOTSTRAP");

        Ensure<FileLogger>(root);
        Ensure<FirstRunDemo>(root);
        Ensure<SimpleHud>(root);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[FBL] OnboardingBootstrap applied to scene: " + target);
    }

    public static void ApplyForCLI() => Apply();

    private static void Ensure<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() == null)
        {
            go.AddComponent<T>();
            Debug.Log("[FBL] Added: " + typeof(T).Name);
        }
    }
}
#endif

