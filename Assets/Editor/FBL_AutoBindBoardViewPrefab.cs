using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;

public class FBL_AutoBindBoardViewPrefab
{
    public static void Run()
    {
        var prefabGuid = AssetDatabase.FindAssets("NatureLightTile t:Prefab").FirstOrDefault();
        if (prefabGuid == null)
        {
            Debug.LogError("NatureLightTile prefab not found");
            EditorApplication.Exit(1);
            return;
        }

        var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/game.unity");

        var boardViews = GameObject.FindObjectsOfType<MonoBehaviour>()
            .Where(m => m.GetType().FullName == "Game.Presentation.BoardView");

        int assigned = 0;

        foreach (var bv in boardViews)
        {
            var so = new SerializedObject(bv);
            var prop = so.FindProperty("natureLightTilePrefab");

            if (prop != null && prop.objectReferenceValue == null)
            {
                prop.objectReferenceValue = prefab;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(bv);
                assigned++;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("BoardView prefab assignments: " + assigned);

        EditorApplication.Exit(0);
    }
}
