#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FBL_FixBoardViewTilePrefab
{
    [MenuItem("Tools/FBL/Fix BoardView TilePrefab")]
    public static void FixBoardViewTilePrefab()
    {
        const string scenePath = "Assets/Scenes/game.unity";
        const string prefabPath = "Assets/Game/Presentation/Prefabs/Tile.prefab";

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("[FBL] Could not open scene: " + scenePath);
            return;
        }

        var boardView = Object.FindFirstObjectByType<Game.Presentation.BoardView>();
        if (boardView == null)
        {
            Debug.LogError("[FBL] Could not find Game.Presentation.BoardView in scene: " + scenePath);
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("[FBL] Could not load prefab: " + prefabPath);
            return;
        }

        Undo.RecordObject(boardView, "Assign BoardView TilePrefab");
        boardView.TilePrefab = prefab;
        EditorUtility.SetDirty(boardView);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[FBL] BoardView.TilePrefab assigned successfully: " + prefabPath);
    }
}
#endif
