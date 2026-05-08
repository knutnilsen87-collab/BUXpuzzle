using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FBL_FullAutopilotNatureLight
{
    [Serializable]
    public class ResultData
    {
        public string timestamp_utc;
        public bool success;
        public string phase;
        public string summary;
        public string analysis;
        public string[] top_open_gaps;
        public string[] actions;
        public string[] suspicious_objects;
        public string screenshot_path;
        public int sprite_renderer_count;
        public int mesh_renderer_count;
        public int missing_sprite_count;
        public int suspicious_name_count;
        public int suspicious_sprite_count;
        public int suspicious_material_count;
        public string theme_asset_path;
        public string prefab_path;
        public string binder_script_path;
    }

    public static int Run()
    {
        var resultPath = Environment.GetEnvironmentVariable("FBL_AUTOPILOT_RESULT_PATH");
        var shotPath = Environment.GetEnvironmentVariable("FBL_AUTOPILOT_SCREENSHOT_PATH");
        var scenePath = Environment.GetEnvironmentVariable("FBL_AUTOPILOT_SCENE_PATH");

        var data = new ResultData
        {
            timestamp_utc = DateTime.UtcNow.ToString("o"),
            success = false,
            phase = "VERIFY",
            summary = "Autopilot not executed",
            analysis = "",
            top_open_gaps = Array.Empty<string>(),
            actions = Array.Empty<string>(),
            suspicious_objects = Array.Empty<string>(),
            screenshot_path = shotPath ?? "",
            theme_asset_path = "",
            prefab_path = "",
            binder_script_path = ""
        };

        var actions = new List<string>();
        var suspicious = new List<string>();
        var gaps = new List<string>();

        try
        {
            if (string.IsNullOrWhiteSpace(scenePath) || !File.Exists(scenePath))
                throw new Exception("Scene not found: " + scenePath);

            // Discover likely project assets.
            var themeGuids = AssetDatabase.FindAssets("t:ScriptableObject NatureLight");
            var prefabGuids = AssetDatabase.FindAssets("NatureLightTile t:Prefab");
            var binderGuids = AssetDatabase.FindAssets("FBL_TileVisualBinder t:Script");

            var themeAssetPath = themeGuids.Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
            var prefabPath = prefabGuids.Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
            var binderPath = binderGuids.Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();

            data.theme_asset_path = themeAssetPath ?? "";
            data.prefab_path = prefabPath ?? "";
            data.binder_script_path = binderPath ?? "";

            if (!string.IsNullOrWhiteSpace(themeAssetPath))
            {
                AssetDatabase.ImportAsset(themeAssetPath, ImportAssetOptions.ForceUpdate);
                actions.Add("REIMPORT_THEME_ASSET");
            }
            else
            {
                gaps.Add("Nature Light theme asset not found.");
            }

            if (!string.IsNullOrWhiteSpace(prefabPath))
            {
                AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);
                actions.Add("REIMPORT_PREFAB");
            }
            else
            {
                gaps.Add("Nature Light tile prefab not found.");
            }

            if (!string.IsNullOrWhiteSpace(binderPath))
            {
                AssetDatabase.ImportAsset(binderPath, ImportAssetOptions.ForceUpdate);
                actions.Add("REIMPORT_BINDER_SCRIPT");
            }
            else
            {
                gaps.Add("FBL_TileVisualBinder script not found.");
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            actions.Add("ASSET_REFRESH");

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
                throw new Exception("Failed to open scene.");

            // Rebind likely binder components to theme/prefab when fields exist.
            var rootObjects = scene.GetRootGameObjects();
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.scene.IsValid() && go.scene.path == scenePath)
                .ToArray();

            var themeAsset = !string.IsNullOrWhiteSpace(themeAssetPath) ? AssetDatabase.LoadMainAssetAtPath(themeAssetPath) : null;
            var prefabAsset = !string.IsNullOrWhiteSpace(prefabPath) ? AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) : null;

            int reboundAssignments = 0;

            foreach (var go in allObjects)
            {
                var comps = go.GetComponents<Component>();
                foreach (var comp in comps)
                {
                    if (comp == null) continue;
                    var type = comp.GetType();
                    var typeName = type.Name ?? "";

                    bool looksLikeBinder = typeName.IndexOf("TileVisualBinder", StringComparison.OrdinalIgnoreCase) >= 0
                                        || typeName.IndexOf("DeterministicBoardRenderer", StringComparison.OrdinalIgnoreCase) >= 0
                                        || typeName.IndexOf("ThemeBootstrap", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (!looksLikeBinder) continue;

                    var so = new SerializedObject(comp);
                    bool changed = false;

                    var iterator = so.GetIterator();
                    bool enterChildren = true;
                    while (iterator.NextVisible(enterChildren))
                    {
                        enterChildren = false;

                        if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            var propName = iterator.name.ToLowerInvariant();

                            if (themeAsset != null &&
                                (propName.Contains("theme") || propName.Contains("style") || propName.Contains("skin")) &&
                                iterator.objectReferenceValue == null)
                            {
                                iterator.objectReferenceValue = themeAsset;
                                changed = true;
                                reboundAssignments++;
                            }

                            if (prefabAsset != null &&
                                (propName.Contains("tileprefab") || propName.Contains("prefab") || propName.Contains("tileview")) &&
                                iterator.objectReferenceValue == null)
                            {
                                iterator.objectReferenceValue = prefabAsset;
                                changed = true;
                                reboundAssignments++;
                            }
                        }
                    }

                    if (changed)
                    {
                        so.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(comp);
                    }
                }
            }

            if (reboundAssignments > 0)
                actions.Add("REBIND_RENDERER_FIELDS");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            actions.Add("SAVE_SCENE");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            // Verify visuals heuristically.
            var spriteRenderers = allObjects.SelectMany(go => go.GetComponents<SpriteRenderer>()).ToArray();
            var meshRenderers = allObjects.SelectMany(go => go.GetComponents<MeshRenderer>()).ToArray();

            int missingSprite = 0;
            int suspiciousName = 0;
            int suspiciousSprite = 0;
            int suspiciousMaterial = 0;

            foreach (var sr in spriteRenderers)
            {
                var goName = sr.gameObject.name ?? "";
                var lowName = goName.ToLowerInvariant();
                var spriteName = sr.sprite != null ? sr.sprite.name : "";
                var lowSprite = spriteName.ToLowerInvariant();
                var matName = sr.sharedMaterial != null ? sr.sharedMaterial.name : "";
                var lowMat = matName.ToLowerInvariant();

                if (sr.sprite == null)
                {
                    missingSprite++;
                    suspicious.Add("MissingSprite:" + goName);
                }

                if (lowName.Contains("debug") || lowName.Contains("square") || lowName.Contains("placeholder"))
                {
                    suspiciousName++;
                    suspicious.Add("DebugLikeName:" + goName);
                }

                if (lowSprite.Contains("debug") || lowSprite.Contains("square") || lowSprite.Contains("placeholder") || lowSprite == "white")
                {
                    suspiciousSprite++;
                    suspicious.Add("DebugLikeSprite:" + goName + ":" + spriteName);
                }

                if (lowMat.Contains("default") || lowMat.Contains("sprite-default"))
                {
                    suspiciousMaterial++;
                    suspicious.Add("DefaultMaterial:" + goName + ":" + matName);
                }
            }

            foreach (var mr in meshRenderers)
            {
                var goName = mr.gameObject.name ?? "";
                var lowName = goName.ToLowerInvariant();
                var matName = mr.sharedMaterial != null ? mr.sharedMaterial.name : "";
                var lowMat = matName.ToLowerInvariant();

                if (lowName.Contains("debug") || lowName.Contains("square") || lowName.Contains("placeholder"))
                {
                    suspiciousName++;
                    suspicious.Add("DebugLikeMeshName:" + goName);
                }

                if (lowMat.Contains("default") || lowMat.Contains("default-material"))
                {
                    suspiciousMaterial++;
                    suspicious.Add("DefaultMeshMaterial:" + goName + ":" + matName);
                }
            }

            data.sprite_renderer_count = spriteRenderers.Length;
            data.mesh_renderer_count = meshRenderers.Length;
            data.missing_sprite_count = missingSprite;
            data.suspicious_name_count = suspiciousName;
            data.suspicious_sprite_count = suspiciousSprite;
            data.suspicious_material_count = suspiciousMaterial;

            // Best-effort screenshot.
            try
            {
                if (!string.IsNullOrWhiteSpace(shotPath))
                {
                    var cam = Camera.main ?? UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault();
                    if (cam != null)
                    {
                        var dir = Path.GetDirectoryName(shotPath);
                        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        int w = 1280, h = 720;
                        var prevTarget = cam.targetTexture;
                        var prevActive = RenderTexture.active;
                        var rt = new RenderTexture(w, h, 24);
                        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);

                        cam.targetTexture = rt;
                        RenderTexture.active = rt;
                        cam.Render();
                        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                        tex.Apply();

                        var bytes = tex.EncodeToPNG();
                        File.WriteAllBytes(shotPath, bytes);

                        cam.targetTexture = prevTarget;
                        RenderTexture.active = prevActive;
                        UnityEngine.Object.DestroyImmediate(rt);
                        UnityEngine.Object.DestroyImmediate(tex);

                        actions.Add("SCREENSHOT_CAPTURED");
                    }
                    else
                    {
                        gaps.Add("No camera found for screenshot.");
                    }
                }
            }
            catch (Exception exShot)
            {
                gaps.Add("Screenshot failed: " + exShot.Message);
            }

            bool hasRenderable = spriteRenderers.Length > 0 || meshRenderers.Length > 0;
            bool pass =
                hasRenderable &&
                missingSprite == 0 &&
                suspiciousSprite == 0 &&
                suspiciousName == 0;

            data.actions = actions.ToArray();
            data.suspicious_objects = suspicious.Distinct().Take(100).ToArray();

            if (pass)
            {
                data.success = true;
                data.phase = "RUNNING";
                data.summary = "Nature Light visuals repaired and auto-verified";
                data.analysis =
                    $"Autopilot completed repair and verification successfully. " +
                    $"spriteRenderers={spriteRenderers.Length}, meshRenderers={meshRenderers.Length}, reboundAssignments={reboundAssignments}.";
                data.top_open_gaps = new[]
                {
                    "Next pass: animation polish",
                    "Next pass: final authored Nature Light art",
                    "Next pass: audio and HUD polish"
                };
            }
            else
            {
                data.success = false;
                data.phase = "VERIFY";
                data.summary = "Nature Light visuals still failed auto-verification after repair";
                data.analysis =
                    $"Autopilot performed repair actions but verification still found issues. " +
                    $"spriteRenderers={spriteRenderers.Length}, meshRenderers={meshRenderers.Length}, " +
                    $"missingSprite={missingSprite}, suspiciousName={suspiciousName}, suspiciousSprite={suspiciousSprite}, suspiciousMaterial={suspiciousMaterial}, reboundAssignments={reboundAssignments}.";
                if (!hasRenderable) gaps.Add("No renderable tile visuals detected.");
                if (missingSprite > 0) gaps.Add("SpriteRenderer instances still missing sprites.");
                if (suspiciousSprite > 0) gaps.Add("One or more sprites still look like debug or placeholder squares.");
                if (suspiciousName > 0) gaps.Add("One or more tile objects still have debug-like naming.");
                data.top_open_gaps = gaps.Distinct().ToArray();
            }
        }
        catch (Exception ex)
        {
            data.success = false;
            data.phase = "VERIFY";
            data.summary = "Autopilot crashed during repair or verification";
            data.analysis = ex.ToString();
            data.top_open_gaps = new[] { "Fix Unity autopilot crash and rerun." };
        }

        try
        {
            var dir = Path.GetDirectoryName(resultPath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(resultPath, JsonUtility.ToJson(data, true));
        }
        catch (Exception exWrite)
        {
            Debug.LogError("Failed to write result json: " + exWrite);
            return 71;
        }

        return data.success ? 0 : 70;
    }
}
