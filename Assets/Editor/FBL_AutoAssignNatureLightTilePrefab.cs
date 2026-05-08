using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FBL_AutoAssignNatureLightTilePrefab
{
    public static void Run()
    {
        const string scenePath = "Assets/Scenes/game.unity";
        const string fieldName = "natureLightTilePrefab";
        const string targetPrefabName = "NatureLightTile.prefab";

        var logPath = Path.GetFullPath("Library/FBL_AutoAssignNatureLightTilePrefab.log");
        void Log(string msg)
        {
            Debug.Log(msg);
            File.AppendAllText(logPath, DateTime.UtcNow.ToString("o") + " " + msg + Environment.NewLine);
        }

        File.WriteAllText(logPath, "");
        Log("START prefab auto-assign");

        var prefabGuids = AssetDatabase.FindAssets("t:Prefab NatureLightTile");
        var prefabPath = prefabGuids
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p => string.Equals(Path.GetFileName(p), targetPrefabName, StringComparison.OrdinalIgnoreCase))
            ?? prefabGuids.Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(prefabPath))
        {
            Log("FAIL prefab not found");
            throw new Exception("NatureLightTile.prefab not found in project.");
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Log("FAIL prefab load returned null: " + prefabPath);
            throw new Exception("Failed to load prefab at: " + prefabPath);
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();
        int boardViewCount = 0;
        int assignedCount = 0;

        foreach (var root in roots)
        {
            var monoBehaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in monoBehaviours)
            {
                if (mb == null) continue;
                var type = mb.GetType();
                if (type.FullName != "Game.Presentation.BoardView") continue;

                boardViewCount++;
                var so = new SerializedObject(mb);
                var prop = so.FindProperty(fieldName);

                if (prop == null)
                {
                    Log("FAIL serialized field not found on BoardView: " + fieldName);
                    throw new Exception("Serialized field not found on BoardView: " + fieldName);
                }

                if (prop.objectReferenceValue == null)
                {
                    prop.objectReferenceValue = prefab;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(mb);
                    assignedCount++;
                    Log("ASSIGNED prefab to BoardView on GameObject: " + mb.gameObject.name);
                }
                else
                {
                    Log("SKIP already assigned on GameObject: " + mb.gameObject.name);
                }
            }
        }

        if (boardViewCount == 0)
        {
            Log("FAIL no BoardView components found in scene");
            throw new Exception("No Game.Presentation.BoardView components found in scene.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Log("SUCCESS boardViews=" + boardViewCount + " assigned=" + assignedCount + " prefabPath=" + prefabPath);
        EditorApplication.Exit(0);
    }
}
