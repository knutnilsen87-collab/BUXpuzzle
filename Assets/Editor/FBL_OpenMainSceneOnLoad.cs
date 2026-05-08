#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class FBL_OpenMainSceneOnLoad
{
    private const string PrefKey = "FBL.AutoOpenMainScene.Enabled";
    private const string MainScenePath = "Assets/Game/Scenes/game.unity";

    static FBL_OpenMainSceneOnLoad()
    {
        EditorApplication.delayCall += TryOpenMainSceneIfNeeded;
    }

    private static bool IsEnabled()
    {
        // Default ON
        return EditorPrefs.GetBool(PrefKey, true);
    }

    [MenuItem("FBL/Auto Open Main Scene (Enabled)", priority = 1)]
    private static void ToggleEnabled()
    {
        bool enabled = IsEnabled();
        EditorPrefs.SetBool(PrefKey, !enabled);
        Debug.Log($"[FBL] AutoOpenMainScene now {(!enabled ? "ENABLED" : "DISABLED")}");
    }

    [MenuItem("FBL/Auto Open Main Scene (Enabled)", true)]
    private static bool ToggleEnabledValidate()
    {
        Menu.SetChecked("FBL/Auto Open Main Scene (Enabled)", IsEnabled());
        return true;
    }

    private static void TryOpenMainSceneIfNeeded()
    {
        try
        {
            if (!IsEnabled())
            {
                Debug.Log("[FBL] AutoOpenMainScene disabled (EditorPrefs).");
                return;
            }

            if (Application.isBatchMode) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            var active = EditorSceneManager.GetActiveScene();

            // Only intervene when Unity starts in an unsaved/untitled scene
            if (!string.IsNullOrEmpty(active.path))
            {
                // Debug.Log($"[FBL] AutoOpenMainScene skip: active scene has path: {active.path}");
                return;
            }

            // Use AssetDatabase (reliable in Editor) instead of File.Exists relative path.
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath);
            if (sceneAsset == null)
            {
                Debug.LogWarning("[FBL] AutoOpenMainScene: main scene not found in AssetDatabase at: " + MainScenePath);
                return;
            }

            // Open the scene
            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            Debug.Log("[FBL] Auto-opened main scene: " + MainScenePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[FBL] AutoOpenMainScene failed: " + ex);
        }
    }
}
#endif
