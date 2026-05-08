using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FBL_RedesignBoardViewAndEndlessLevels
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
        public string prefab_path;
        public string resources_prefab_path;
        public int boardview_count;
        public int autobinder_added_count;
        public int prefab_assigned_count;
        public bool endless_director_installed;
        public string endless_director_target;
    }

    public static int Run()
    {
        var resultPath = Environment.GetEnvironmentVariable("FBL_REDESIGN_RESULT_PATH");
        var scenePath  = Environment.GetEnvironmentVariable("FBL_REDESIGN_SCENE_PATH");

        var result = new ResultData
        {
            timestamp_utc = DateTime.UtcNow.ToString("o"),
            success = false,
            summary = "Redesign not executed",
            analysis = "",
            top_open_gaps = Array.Empty<string>(),
            actions = Array.Empty<string>(),
            prefab_path = "",
            resources_prefab_path = "",
            boardview_count = 0,
            autobinder_added_count = 0,
            prefab_assigned_count = 0,
            endless_director_installed = false,
            endless_director_target = ""
        };

        var actions = new List<string>();
        var gaps = new List<string>();

        try
        {
            if (string.IsNullOrWhiteSpace(scenePath) || !File.Exists(scenePath))
                throw new Exception("Scene not found: " + scenePath);

            var prefabGuid = AssetDatabase.FindAssets("NatureLightTile t:Prefab").FirstOrDefault();
            if (string.IsNullOrWhiteSpace(prefabGuid))
                throw new Exception("NatureLightTile prefab not found.");

            var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                throw new Exception("Failed to load NatureLightTile prefab.");

            result.prefab_path = prefabPath;

            var resourcesDir = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesDir))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                actions.Add("CREATE_RESOURCES_FOLDER");
            }

            var resourcesPrefabPath = resourcesDir + "/NatureLightTile.prefab";
            if (prefabPath != resourcesPrefabPath)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(resourcesPrefabPath) == null)
                {
                    if (!AssetDatabase.CopyAsset(prefabPath, resourcesPrefabPath))
                        throw new Exception("Failed to copy NatureLightTile prefab into Resources.");
                    actions.Add("COPY_PREFAB_TO_RESOURCES");
                }
            }
            result.resources_prefab_path = resourcesPrefabPath;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
                throw new Exception("Failed to open scene.");

            var boardViewType = Type.GetType("Game.Presentation.BoardView, Assembly-CSharp");
            var autoBinderType = Type.GetType("Game.Presentation.BoardViewAutoBinder, Assembly-CSharp");
            var endlessDirectorType = Type.GetType("Game.Levels.EndlessLevelDirector, Assembly-CSharp");

            if (boardViewType == null) throw new Exception("Game.Presentation.BoardView type not found.");
            if (autoBinderType == null) throw new Exception("Game.Presentation.BoardViewAutoBinder type not found.");
            if (endlessDirectorType == null) throw new Exception("Game.Levels.EndlessLevelDirector type not found.");

            var rootObjects = scene.GetRootGameObjects();
            bool dirty = false;

            foreach (var root in rootObjects)
            {
                foreach (var component in root.GetComponentsInChildren(boardViewType, true))
                {
                    if (component == null) continue;
                    result.boardview_count++;

                    var mb = component as MonoBehaviour;
                    if (mb == null) continue;

                    var binder = mb.GetComponent(autoBinderType) as MonoBehaviour;
                    if (binder == null)
                    {
                        binder = mb.gameObject.AddComponent(autoBinderType) as MonoBehaviour;
                        result.autobinder_added_count++;
                        dirty = true;
                    }

                    var soBoardView = new SerializedObject(mb);
                    var boardPrefabProp = soBoardView.FindProperty("natureLightTilePrefab");
                    if (boardPrefabProp != null && boardPrefabProp.objectReferenceValue == null)
                    {
                        boardPrefabProp.objectReferenceValue = prefab;
                        soBoardView.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(mb);
                        result.prefab_assigned_count++;
                        dirty = true;
                    }

                    var soBinder = new SerializedObject(binder);
                    var fallbackProp = soBinder.FindProperty("fallbackNatureLightTilePrefab");
                    if (fallbackProp != null && fallbackProp.objectReferenceValue == null)
                    {
                        fallbackProp.objectReferenceValue = prefab;
                        soBinder.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(binder);
                        dirty = true;
                    }
                }
            }

            GameObject targetGo =
                GameObject.Find("GameRoot") ??
                GameObject.Find("game") ??
                rootObjects.FirstOrDefault();

            if (targetGo == null)
                throw new Exception("No suitable scene root found for EndlessLevelDirector.");

            var existingDirector = targetGo.GetComponent(endlessDirectorType) as MonoBehaviour;
            if (existingDirector == null)
            {
                targetGo.AddComponent(endlessDirectorType);
                result.endless_director_installed = true;
                result.endless_director_target = targetGo.name;
                dirty = true;
            }
            else
            {
                result.endless_director_installed = true;
                result.endless_director_target = targetGo.name;
            }

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new Exception("Failed to save scene.");
                actions.Add("SAVE_SCENE");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            actions.Add("REFRESH_ASSETS");

            result.actions = actions.ToArray();
            result.success = true;
            result.summary = "BoardView hardening and endless level scaffold installed";
            result.analysis =
                $"Installed BoardViewAutoBinder and EndlessLevelDirector. boardViews={result.boardview_count}, autobindersAdded={result.autobinder_added_count}, prefabAssigned={result.prefab_assigned_count}, resourcesPrefab={resourcesPrefabPath}, directorTarget={result.endless_director_target}.";
            result.top_open_gaps = new[]
            {
                "Run runtime verification.",
                "Verify EndlessLevelDirector bridges into existing engine hooks as expected."
            };
        }
        catch (Exception ex)
        {
            result.success = false;
            result.summary = "Redesign pass failed";
            result.analysis = ex.ToString();
            result.top_open_gaps = new[]
            {
                "Inspect unity_redesign.log and redesign_result.json."
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
            Debug.LogError("Failed to write redesign_result.json: " + exWrite);
            return 71;
        }

        return result.success ? 0 : 70;
    }
}
