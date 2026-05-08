using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Game.Presentation;

[InitializeOnLoad]
public static class AutoSetupTileInputEditor
{
    private const string DoneKey = "FBL_AutoSetupTileInput_Done_v1";

    static AutoSetupTileInputEditor()
    {
        EditorApplication.delayCall += TrySetup;
    }

    private static void TrySetup()
    {
        // NEVER run during play mode
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (Application.isPlaying) return;

        if (EditorPrefs.GetBool(DoneKey, false)) return;
        if (!SceneManager.GetActiveScene().IsValid()) return;

        var input = Object.FindFirstObjectByType<TileInput>();
        if (input == null)
        {
            var go = new GameObject("TileInput");
            input = go.AddComponent<TileInput>();
        }

        input.CameraOverride = null;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("FBL AutoSetup: TileInput added. Click tiles to select and swap.");
        EditorPrefs.SetBool(DoneKey, true);
    }
}
