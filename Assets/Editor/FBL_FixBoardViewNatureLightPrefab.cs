using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FBL_FixBoardViewNatureLightPrefab
{
    [Serializable]
    public class ResultData
    {
        public string timestamp_utc;
        public bool success;
        public string summary;
        public string analysis;
        public string[] top_open_gaps;
        public string[] actions;
        public string scene_path;
        public string prefab_path;
        public int boardview_count;
        public int assigned_count;
        public int already_assigned_count;
        public int remaining_null_count;
    }

    public static int Run()
    {
        var resultPath = Environment.GetEnvironmentVariable("FBL_FIX_RESULT_PATH");
        var scenePath  = Environment.GetEnvironmentVariable("FBL_FIX_SCENE_PATH");

        var result = new ResultData
        {
            timestamp_utc = DateTime.UtcNow.ToString("o"),
            success = false,
            summary = "BoardView prefab repair not executed",
            analysis = "",
            top_open_gaps = Array.Empty<string>(),
            actions = Array.Empty<string>(),
            scene_path = scenePath ?? "",
            prefab_path = "",
            boardview_count = 0,
            assigned_count = 0,
            already_assigned_count = 0,
            remaining_null_count = 0
        };

        var actions = new List<string>();
        var gaps = new List<string>();

        try
        {
            if (string.IsNullOrWhiteSpace(scenePath) || !File.Exists(scenePath))
                throw new Exception("Scene not found: " + scenePath);

            var prefabGuid = AssetDatabase.FindAssets("NatureLightTile t:Prefab")
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(prefabGuid))
                throw new Exception("NatureLightTile prefab not found.");

            var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                throw new Exception("Failed to load prefab at: " + prefabPath);

            result.prefab_path = prefabPath;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
                throw new Exception("Failed to open scene: " + scenePath);

            var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.scene.IsValid() && go.scene.path == scenePath)
                .ToArray();

            int boardViews = 0;
            int assigned = 0;
            int alreadyAssigned = 0;
            bool sceneDirty = false;

            foreach (var go in allGameObjects)
            {
                var comps = go.GetComponents<Component>();
                foreach (var comp in comps)
                {
                    if (comp == null) continue;
                    var type = comp.GetType();
                    if (type.FullName != "Game.Presentation.BoardView") continue;

                    boardViews++;

                    var so = new SerializedObject(comp);
                    var prop = so.FindProperty("natureLightTilePrefab");
                    if (prop == null)
                    {
                        gaps.Add("BoardView field 'natureLightTilePrefab' not found on " + go.name);
                        continue;
                    }

                    if (prop.objectReferenceValue == null)
                    {
                        prop.objectReferenceValue = prefab;
                        so.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(comp);
                        assigned++;
                        sceneDirty = true;
                    }
                    else
                    {
                        alreadyAssigned++;
                    }
                }
            }

            result.boardview_count = boardViews;
            result.assigned_count = assigned;
            result.already_assigned_count = alreadyAssigned;

            if (boardViews == 0)
                throw new Exception("No Game.Presentation.BoardView components found in scene.");

            if (sceneDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new Exception("Failed to save scene after assignment.");
                actions.Add("ASSIGN_NATURELIGHT_PREFAB");
                actions.Add("SAVE_SCENE");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            actions.Add("REFRESH_ASSET_DATABASE");

            // Reopen and verify serialized value is no longer null.
            var verifyScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!verifyScene.IsValid())
                throw new Exception("Failed to reopen scene for verification.");

            int remainingNull = 0;
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.scene.IsValid() && g.scene.path == scenePath))
            {
                foreach (var comp in go.GetComponents<Component>())
                {
                    if (comp == null) continue;
                    var type = comp.GetType();
                    if (type.FullName != "Game.Presentation.BoardView") continue;

                    var so = new SerializedObject(comp);
                    var prop = so.FindProperty("natureLightTilePrefab");
                    if (prop == null || prop.objectReferenceValue == null)
                        remainingNull++;
                }
            }

            result.remaining_null_count = remainingNull;
            result.actions = actions.ToArray();

            if (remainingNull == 0)
            {
                result.success = true;
                result.summary = "BoardView Nature Light prefab binding repaired";
                result.analysis =
                    $"Scene repaired successfully. boardViews={boardViews}, assigned={assigned}, alreadyAssigned={alreadyAssigned}, remainingNull={remainingNull}, prefabPath={prefabPath}.";
                result.top_open_gaps = new[]
                {
                    "Run gameplay verification script to confirm debug squares are gone."
                };
            }
            else
            {
                result.success = false;
                result.summary = "BoardView Nature Light prefab binding still incomplete";
                result.analysis =
                    $"Repair attempted but some BoardView instances still have null natureLightTilePrefab. boardViews={boardViews}, assigned={assigned}, alreadyAssigned={alreadyAssigned}, remainingNull={remainingNull}.";
                gaps.Add("One or more BoardView instances still have null natureLightTilePrefab.");
                result.top_open_gaps = gaps.Distinct().ToArray();
            }
        }
        catch (Exception ex)
        {
            result.success = false;
            result.summary = "BoardView Nature Light prefab repair crashed";
            result.analysis = ex.ToString();
            result.top_open_gaps = new[]
            {
                "Inspect unity_fix.log and fix_result.json."
            };
        }

        try
        {
            var dir = Path.GetDirectoryName(resultPath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(resultPath, JsonUtility.ToJson(result, true));
        }
        catch (Exception exWrite)
        {
            Debug.LogError("Failed to write result json: " + exWrite);
            return 71;
        }

        return result.success ? 0 : 70;
    }
}
