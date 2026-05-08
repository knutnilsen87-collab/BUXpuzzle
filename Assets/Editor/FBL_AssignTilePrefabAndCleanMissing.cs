using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FBL_AssignTilePrefabAndCleanMissing
{
    [MenuItem("FBL/Repair/Assign TilePrefab And Clean Missing Scripts")]
    public static void Run()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[FBL] No loaded scene.");
            return;
        }

        var prefabGuids = AssetDatabase.FindAssets("t:Prefab Tile");
        string prefabPath = prefabGuids
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p => p.Replace("\\", "/").EndsWith("Assets/Game/Presentation/Prefabs/Tile.prefab"));

        if (string.IsNullOrEmpty(prefabPath))
        {
            // fallback: any prefab named Tile
            prefabPath = prefabGuids.Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
        }

        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("[FBL] Tile.prefab not found in project.");
            return;
        }

        var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (tilePrefab == null)
        {
            Debug.LogError("[FBL] Failed to load Tile.prefab at: " + prefabPath);
            return;
        }

        int assigned = 0;
        int cleaned = 0;

        foreach (var root in scene.GetRootGameObjects())
        {
            cleaned += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);

            foreach (var comp in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (comp == null) continue;

                var t = comp.GetType();
                if (t.FullName == "Game.Presentation.BoardView" || t.Name == "BoardView")
                {
                    var field = t.GetField("TilePrefab", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                               ?? t.GetField("_tilePrefab", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                               ?? t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                   .FirstOrDefault(f => typeof(GameObject).IsAssignableFrom(f.FieldType) && f.Name.ToLower().Contains("tileprefab"));

                    if (field != null && typeof(GameObject).IsAssignableFrom(field.FieldType))
                    {
                        field.SetValue(comp, tilePrefab);
                        EditorUtility.SetDirty(comp);
                        assigned++;
                        Debug.Log("[FBL] Assigned TilePrefab on " + comp.gameObject.name + " using field " + field.Name + " -> " + prefabPath);
                    }
                    else
                    {
                        Debug.LogWarning("[FBL] BoardView found on " + comp.gameObject.name + " but no assignable TilePrefab field/property was found.");
                    }
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[FBL] Assign TilePrefab complete. assigned=" + assigned + " cleanedMissingScripts=" + cleaned + " prefab=" + prefabPath);
    }
}
