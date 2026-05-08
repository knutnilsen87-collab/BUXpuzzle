using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FBL_DeepBindBoardViewNatureLightPrefab
{
    private const string ScenePath = "Assets/Scenes/game.unity";
    private const string FieldName = "natureLightTilePrefab";
    private const string PrefabName = "NatureLightTile.prefab";

    public static void Run()
    {
        var logPath = Path.GetFullPath("Library/FBL_DeepBindBoardViewNatureLightPrefab.log");

        void Log(string msg)
        {
            Debug.Log(msg);
            File.AppendAllText(logPath, DateTime.UtcNow.ToString("o") + " " + msg + Environment.NewLine);
        }

        File.WriteAllText(logPath, "");
        Log("START");

        var prefabGuid = AssetDatabase.FindAssets("t:Prefab NatureLightTile")
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(prefabGuid))
            throw new Exception("NatureLightTile prefab GUID not found.");

        var natureLightPrefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
        if (string.IsNullOrWhiteSpace(natureLightPrefabPath))
            throw new Exception("NatureLightTile prefab path not found.");

        var natureLightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(natureLightPrefabPath);
        if (natureLightPrefab == null)
            throw new Exception("Failed to load NatureLightTile prefab at: " + natureLightPrefabPath);

        int sceneBoardViews = 0;
        int sceneAssigned = 0;
        int prefabAssetsChecked = 0;
        int prefabAssetsAssigned = 0;

        bool TryAssign(UnityEngine.Object target)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(FieldName);
            if (prop == null) return false;

            if (prop.objectReferenceValue == null || prop.objectReferenceValue != natureLightPrefab)
            {
                prop.objectReferenceValue = natureLightPrefab;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
                return true;
            }

            return false;
        }

        // 1) Scene instances
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                if (mb.GetType().FullName != "Game.Presentation.BoardView") continue;

                sceneBoardViews++;
                if (TryAssign(mb))
                {
                    sceneAssigned++;
                    Log("SCENE_ASSIGNED:" + mb.gameObject.name);
                }
                else
                {
                    Log("SCENE_ALREADY_OK:" + mb.gameObject.name);
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // 2) Prefab assets that contain BoardView
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in prefabGuids)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrWhiteSpace(prefabPath)) continue;

            GameObject root = null;
            bool changed = false;

            try
            {
                root = PrefabUtility.LoadPrefabContents(prefabPath);
                if (root == null) continue;

                var components = root.GetComponentsInChildren<MonoBehaviour>(true)
                    .Where(m => m != null && m.GetType().FullName == "Game.Presentation.BoardView")
                    .ToArray();

                if (components.Length == 0)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                    continue;
                }

                prefabAssetsChecked++;

                foreach (var comp in components)
                {
                    if (TryAssign(comp))
                    {
                        changed = true;
                        prefabAssetsAssigned++;
                        Log("PREFAB_ASSIGNED:" + prefabPath + "::" + comp.gameObject.name);
                    }
                    else
                    {
                        Log("PREFAB_ALREADY_OK:" + prefabPath + "::" + comp.gameObject.name);
                    }
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }

                PrefabUtility.UnloadPrefabContents(root);
            }
            catch (Exception ex)
            {
                if (root != null)
                {
                    try { PrefabUtility.UnloadPrefabContents(root); } catch {}
                }
                Log("PREFAB_SCAN_ERROR:" + prefabPath + "::" + ex.Message);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Log("SUCCESS sceneBoardViews=" + sceneBoardViews +
            " sceneAssigned=" + sceneAssigned +
            " prefabAssetsChecked=" + prefabAssetsChecked +
            " prefabAssetsAssigned=" + prefabAssetsAssigned +
            " natureLightPrefabPath=" + natureLightPrefabPath);

        EditorApplication.Exit(0);
    }
}
