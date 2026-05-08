using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Game.Presentation;

[InitializeOnLoad]
public static class AutoSetupBoardViewEditor
{
    private const string DoneKey = "FBL_AutoSetupBoardView_Done_v1";

    static AutoSetupBoardViewEditor()
    {
        EditorApplication.delayCall += TrySetup;
    }

    private static void TrySetup()
    {
        // NEVER run editor setup during play mode (fixes InvalidOperationException spam)
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (Application.isPlaying) return;

        if (EditorPrefs.GetBool(DoneKey, false)) return;
        if (SceneManager.GetActiveScene().IsValid() == false) return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Presentation/Prefabs/Tile.prefab");
        if (prefab == null) prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Presentation/Prefabs/Tile");
        if (prefab == null)
        {
            Debug.LogWarning("FBL AutoSetup: Tile prefab not found at Assets/Game/Presentation/Prefabs/Tile.prefab");
            return;
        }

        BoardView view = Object.FindFirstObjectByType<BoardView>();
        if (view == null)
        {
            var go = new GameObject("BoardView");
            view = go.AddComponent<BoardView>();
        }

        view.Width = 8;
        view.Height = 8;
        view.CellSize = 1.1f;
        view.TilePrefab = prefab;
        view.AutoDrawOnStart = true;

        var cam = Camera.main;
        if (cam == null)
        {
            var camGO = GameObject.Find("Main Camera");
            if (camGO != null) cam = camGO.GetComponent<Camera>();
        }

        if (cam != null)
        {
            cam.orthographic = true;
            float halfW = (view.Width - 1) * view.CellSize * 0.5f;
            float halfH = (view.Height - 1) * view.CellSize * 0.5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographicSize = Mathf.Max(halfH + 1.5f, (halfW + 1.5f) / cam.aspect);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("FBL AutoSetup: BoardView created/wired. Press Play to see 8x8 grid.");
        EditorPrefs.SetBool(DoneKey, true);
    }
}
