using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class FBL_BoardViewPrefabPermanentFix
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
        public bool autoRepairInstalled;
    }

    static FBL_BoardViewPrefabPermanentFix()
    {
        EditorApplication.delayCall += AutoRepairOnLoad;
    }

    private static void AutoRepairOnLoad()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        try
        {
            EnsureBoardViewPrefabAssignment(false, null);
        }
        catch
        {
            // Silent on automatic editor-load repair; explicit run will surface details.
        }
    }

    [MenuItem("FBL/Fix/Assign BoardView NatureLight Prefab")]
    public static void RunFromMenu()
    {
        EnsureBoardViewPrefabAssignment(true, null);
    }

    public static void Run()
    {
        string resultPath = Environment.GetEnvironmentVariable("FBL_RESULT_JSON");
        EnsureBoardViewPrefabAssignment(true, resultPath);
    }

    private static void EnsureBoardViewPrefabAssignment(bool verbose, string resultPath)
    {
        const string scenePath = "Assets/Scenes/game.unity";
        const string resourcesFolder = "Assets/Resources";
        const string prefabPath = "Assets/Resources/NatureLightTile.prefab";

        var result = new ResultData
        {
            success = false,
            summary = "Not executed",
            analysis = "",
            boardViewCount = 0,
            assignedCount = 0,
            remainingNullCount = 0,
            prefabPath = prefabPath,
            autoRepairInstalled = true
        };

        try
        {
            if (!AssetDatabase.IsValidFolder(resourcesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = "NatureLightTile";
                go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

                var collider = go.GetComponent<Collider>();
                if (collider != null)
                    UnityEngine.Object.DestroyImmediate(collider);

                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var shader = Shader.Find("Sprites/Default");
                    if (shader == null)
                        shader = Shader.Find("Unlit/Color");

                    if (shader != null)
                    {
                        var mat = new Material(shader);
                        mat.color = new Color(0.35f, 0.78f, 0.42f, 1f);
                        renderer.sharedMaterial = mat;
                    }
                }

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
                throw new Exception("Failed to save scene.");

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
                result.summary = "Permanent BoardView prefab autofix installed and verified";
                result.analysis =
                    $"Prefab ensured and assigned. boardViews={result.boardViewCount}, assigned={result.assignedCount}, remainingNull={result.remainingNullCount}, prefabPath={prefabPath}.";
            }
            else
            {
                result.success = false;
                result.summary = "BoardView prefab autofix incomplete";
                result.analysis =
                    $"Assignment attempted, but some BoardView instances still have null natureLightTilePrefab. boardViews={result.boardViewCount}, assigned={result.assignedCount}, remainingNull={result.remainingNullCount}.";
            }

            if (verbose)
                Debug.Log("[FBL] " + result.summary + " " + result.analysis);
        }
        catch (Exception ex)
        {
            result.success = false;
            result.summary = "BoardView prefab autofix failed";
            result.analysis = ex.ToString();
            if (verbose)
                Debug.LogError("[FBL] " + result.analysis);
        }

        if (!string.IsNullOrWhiteSpace(resultPath))
        {
            var dir = Path.GetDirectoryName(resultPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(resultPath, JsonUtility.ToJson(result, true));
        }

        if (!result.success)
            throw new Exception(result.analysis);
    }
}
