using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FBL_AssignBoardViewPrefabAndVerify
{
    [Serializable]
    private class ResultData
    {
        public bool success;
        public string summary;
        public string analysis;
        public int boardViewCount;
        public int assignedCount;
        public int remainingNullCount;
        public string prefabPath;
    }

    public static int Run()
    {
        string resultPath = Environment.GetEnvironmentVariable("FBL_RESULT_JSON");
        string scenePath = "Assets/Scenes/game.unity";
        string prefabPath = "Assets/Resources/NatureLightTile.prefab";

        var result = new ResultData
        {
            success = false,
            summary = "Not executed",
            analysis = "",
            boardViewCount = 0,
            assignedCount = 0,
            remainingNullCount = 0,
            prefabPath = prefabPath
        };

        try
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = "NatureLightTile";
                go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
                prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                UnityEngine.Object.DestroyImmediate(go);

                if (prefab == null)
                    throw new Exception("Failed to create NatureLightTile prefab.");
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
                throw new Exception("Failed to open scene: " + scenePath);

            var boardViews = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(mb => mb != null && mb.GetType().FullName == "Game.Presentation.BoardView")
                .ToArray();

            if (boardViews.Length == 0)
                throw new Exception("No Game.Presentation.BoardView components found in scene.");

            foreach (var boardView in boardViews)
            {
                result.boardViewCount++;

                var so = new SerializedObject(boardView);
                var prop = so.FindProperty("natureLightTilePrefab");
                if (prop == null)
                    throw new Exception("BoardView serialized field 'natureLightTilePrefab' not found.");

                if (prop.objectReferenceValue == null)
                {
                    prop.objectReferenceValue = prefab;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(boardView);
                    result.assignedCount++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new Exception("Failed to save scene after assignment.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            var verifyScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!verifyScene.IsValid())
                throw new Exception("Failed to reopen scene for verification.");

            var verifyBoardViews = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(mb => mb != null && mb.GetType().FullName == "Game.Presentation.BoardView")
                .ToArray();

            foreach (var boardView in verifyBoardViews)
            {
                var so = new SerializedObject(boardView);
                var prop = so.FindProperty("natureLightTilePrefab");
                if (prop == null || prop.objectReferenceValue == null)
                    result.remainingNullCount++;
            }

            if (result.remainingNullCount == 0)
            {
                result.success = true;
                result.summary = "BoardView prefab assignment verified";
                result.analysis =
                    $"Assigned prefab successfully. boardViews={result.boardViewCount}, assigned={result.assignedCount}, remainingNull={result.remainingNullCount}, prefabPath={prefabPath}.";
            }
            else
            {
                result.success = false;
                result.summary = "BoardView prefab assignment incomplete";
                result.analysis =
                    $"Assignment attempted, but some BoardView instances still have null natureLightTilePrefab. boardViews={result.boardViewCount}, assigned={result.assignedCount}, remainingNull={result.remainingNullCount}.";
            }
        }
        catch (Exception ex)
        {
            result.success = false;
            result.summary = "BoardView prefab assignment failed";
            result.analysis = ex.ToString();
        }

        try
        {
            var dir = Path.GetDirectoryName(resultPath);
            if (!Directory.Exists(dir))
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
