using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FBL_AssignNatureLightAndExit
{
    public static void Run()
    {
        const string scenePath = "Assets/Scenes/game.unity";
        const string targetPrefabName = "NatureLightTile.prefab";
        const string fieldName = "natureLightTilePrefab";

        var logPath = Path.GetFullPath("Library/FBL_AssignNatureLightAndExit.log");
        void Log(string s)
        {
            Debug.Log(s);
            File.AppendAllText(logPath, DateTime.UtcNow.ToString("o") + " " + s + Environment.NewLine);
        }

        File.WriteAllText(logPath, "");
        Log("START");

        var prefabGuids = AssetDatabase.FindAssets("t:Prefab NatureLightTile");
        var prefabPath = prefabGuids
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p => string.Equals(Path.GetFileName(p), targetPrefabName, StringComparison.OrdinalIgnoreCase))
            ?? prefabGuids.Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(prefabPath))
            throw new Exception("NatureLightTile.prefab not found.");

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
            throw new Exception("Failed to load prefab: " + prefabPath);

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        int boardViews = 0;
        int assigned = 0;

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                if (mb.GetType().FullName != "Game.Presentation.BoardView") continue;

                boardViews++;
                var so = new SerializedObject(mb);
                var prop = so.FindProperty(fieldName);
                if (prop == null)
                    throw new Exception("Serialized field not found: " + fieldName);

                if (prop.objectReferenceValue == null)
                {
                    prop.objectReferenceValue = prefab;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(mb);
                    assigned++;
                    Log("ASSIGNED:" + mb.gameObject.name);
                }
                else
                {
                    Log("ALREADY_ASSIGNED:" + mb.gameObject.name);
                }
            }
        }

        if (boardViews == 0)
            throw new Exception("No Game.Presentation.BoardView found in scene.");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Log("SUCCESS boardViews=" + boardViews + " assigned=" + assigned + " prefab=" + prefabPath);
        EditorApplication.Exit(0);
    }
}
