using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FBL_FixBoardViewPrefab_Attempt2
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
        public string[] boardview_object_names;
    }

    public static int Run()
    {
        var resultPath = Environment.GetEnvironmentVariable("FBL_FIX_RESULT_PATH");
        var scenePath  = Environment.GetEnvironmentVariable("FBL_FIX_SCENE_PATH");

        var result = new ResultData
        {
            timestamp_utc = DateTime.UtcNow.ToString("o"),
            success = false,
            summary = "Repair not executed",
            analysis = "",
            top_open_gaps = Array.Empty<string>(),
            actions = Array.Empty<string>(),
            scene_path = scenePath ?? "",
            prefab_path = "",
            boardview_count = 0,
            assigned_count = 0,
            already_assigned_count = 0,
            remaining_null_count = 0,
            boardview_object_names = Array.Empty<string>()
        };

        var actions = new List<string>();
        var gaps = new List<string>();
        var boardViewNames = new List<string>();

        try
        {
            if (string.IsNullOrWhiteSpace(scenePath) || !File.Exists(scenePath))
                throw new Exception("Scene not found: " + scenePath);

            var prefabGuids = AssetDatabase.FindAssets("NatureLightTile t:Prefab");
            var prefabGuid = prefabGuids.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(prefabGuid))
                throw new Exception("NatureLightTile prefab not found.");

            var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                throw new Exception("Failed to load prefab: " + prefabPath);

            result.prefab_path = prefabPath;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
                throw new Exception("Failed to open scene.");

            var boardViews = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(mb => mb != null && mb.GetType().FullName == "Game.Presentation.BoardView")
                .ToArray();

            if (boardViews.Length == 0)
                throw new Exception("No Game.Presentation.BoardView components found in opened scene.");

            int assigned = 0;
            int alreadyAssigned = 0;
            bool dirty = false;

            foreach (var boardView in boardViews)
            {
                boardViewNames.Add(boardView.gameObject.name);

                var so = new SerializedObject(boardView);
                var prop = so.FindProperty("natureLightTilePrefab");
                if (prop == null)
                {
                    gaps.Add("Field 'natureLightTilePrefab' not found on BoardView object: " + boardView.gameObject.name);
                    continue;
                }

                if (prop.objectReferenceValue == null)
                {
                    prop.objectReferenceValue = prefab;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(boardView);
                    assigned++;
                    dirty = true;
                }
                else
                {
                    alreadyAssigned++;
                }
            }

            result.boardview_count = boardViews.Length;
            result.assigned_count = assigned;
            result.already_assigned_count = alreadyAssigned;
            result.boardview_object_names = boardViewNames.ToArray();

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new Exception("Failed to save scene after prefab assignment.");

                actions.Add("ASSIGN_NATURELIGHT_PREFAB");
                actions.Add("SAVE_SCENE");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            actions.Add("REFRESH_ASSET_DATABASE");

            // Verify by reopening scene and re-checking.
            var verifyScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!verifyScene.IsValid())
                throw new Exception("Failed to reopen scene for verification.");

            int remainingNull = 0;
            var verifyBoardViews = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(mb => mb != null && mb.GetType().FullName == "Game.Presentation.BoardView")
                .ToArray();

            foreach (var boardView in verifyBoardViews)
            {
                var so = new SerializedObject(boardView);
                var prop = so.FindProperty("natureLightTilePrefab");
                if (prop == null || prop.objectReferenceValue == null)
                    remainingNull++;
            }

            result.remaining_null_count = remainingNull;
            result.actions = actions.ToArray();

            if (remainingNull == 0)
            {
                result.success = true;
                result.summary = "BoardView Nature Light prefab binding repaired";
                result.analysis =
                    $"Repair successful. boardViews={boardViews.Length}, assigned={assigned}, alreadyAssigned={alreadyAssigned}, remainingNull={remainingNull}, prefabPath={prefabPath}.";
                result.top_open_gaps = new[] { "Run visual verification." };
            }
            else
            {
                result.success = false;
                result.summary = "BoardView Nature Light prefab binding still incomplete";
                result.analysis =
                    $"Repair attempted but verification still found null prefab references. boardViews={boardViews.Length}, assigned={assigned}, alreadyAssigned={alreadyAssigned}, remainingNull={remainingNull}.";
                gaps.Add("One or more BoardView instances still have null natureLightTilePrefab after save/reopen.");
                result.top_open_gaps = gaps.Distinct().ToArray();
            }
        }
        catch (Exception ex)
        {
            result.success = false;
            result.summary = "BoardView Nature Light prefab repair crashed";
            result.analysis = ex.ToString();
            result.top_open_gaps = new[] { "Inspect unity_fix.log and fix_result.json." };
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
