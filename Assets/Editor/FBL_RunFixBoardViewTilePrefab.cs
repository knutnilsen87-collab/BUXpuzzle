#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FBL_RunFixBoardViewTilePrefab
{
    public static void Execute()
    {
        const string scenePath = "Assets/Scenes/game.unity";
        const string prefabPath = "Assets/Game/Presentation/Prefabs/Tile.prefab";

        try
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[FBL] Could not open scene: " + scenePath);
                EditorApplication.Exit(2);
                return;
            }

            var boardView = Object.FindFirstObjectByType<Game.Presentation.BoardView>();
            if (boardView == null)
            {
                Debug.LogError("[FBL] Could not find Game.Presentation.BoardView in scene");
                EditorApplication.Exit(3);
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("[FBL] Could not load prefab: " + prefabPath);
                EditorApplication.Exit(4);
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
            EditorApplication.Exit(0);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[FBL] Exception in FBL_RunFixBoardViewTilePrefab.Execute: " + ex);
            EditorApplication.Exit(10);
        }
    }
}
#endif
