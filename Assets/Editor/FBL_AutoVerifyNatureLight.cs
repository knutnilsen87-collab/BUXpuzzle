using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FBL_AutoVerifyNatureLight
{
    [Serializable]
    public class VerifyResult
    {
        public string timestamp_utc;
        public string scene_path;
        public bool success;
        public string result;
        public string summary;
        public string analysis;
        public string[] top_open_gaps;
        public int spriteRendererCount;
        public int meshRendererCount;
        public int tileLikeObjects;
        public int missingSpriteCount;
        public int suspiciousDebugNameCount;
        public int suspiciousDefaultMaterialCount;
        public int suspiciousSquareSpriteCount;
        public string screenshot_path;
        public string[] suspiciousObjects;
        public string[] errors;
    }

    public static int Run()
    {
        var outPath = Environment.GetEnvironmentVariable("FBL_VERIFY_RESULT_PATH");
        var shotPath = Environment.GetEnvironmentVariable("FBL_VERIFY_SCREENSHOT_PATH");
        var scenePath = Environment.GetEnvironmentVariable("FBL_VERIFY_SCENE_PATH");

        var result = new VerifyResult
        {
            timestamp_utc = DateTime.UtcNow.ToString("o"),
            scene_path = scenePath,
            success = false,
            result = "FAIL",
            summary = "Verification not executed",
            analysis = "",
            top_open_gaps = Array.Empty<string>(),
            suspiciousObjects = Array.Empty<string>(),
            errors = Array.Empty<string>(),
            screenshot_path = shotPath ?? ""
        };

        var errors = new List<string>();
        var suspicious = new List<string>();

        try
        {
            if (string.IsNullOrWhiteSpace(scenePath))
                throw new Exception("FBL_VERIFY_SCENE_PATH missing");
            if (!File.Exists(scenePath))
                throw new Exception("Scene not found: " + scenePath);

            var openScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!openScene.IsValid())
                throw new Exception("Failed to open scene: " + scenePath);

            var allGos = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.scene.IsValid() && go.scene.path == scenePath)
                .ToArray();

            var spriteRenderers = allGos.SelectMany(go => go.GetComponents<SpriteRenderer>()).ToArray();
            var meshRenderers   = allGos.SelectMany(go => go.GetComponents<MeshRenderer>()).ToArray();

            var tileLike = allGos.Where(go =>
            {
                var n = (go.name ?? "").ToLowerInvariant();
                return n.Contains("tile") || n.Contains("cell") || n.Contains("gem") || n.Contains("piece");
            }).ToArray();

            int missingSprite = 0;
            int suspiciousDebugName = 0;
            int suspiciousDefaultMaterial = 0;
            int suspiciousSquareSprite = 0;

            foreach (var sr in spriteRenderers)
            {
                var goName = sr.gameObject.name ?? "";
                var lowName = goName.ToLowerInvariant();
                var spriteName = sr.sprite != null ? sr.sprite.name : "";
                var lowSprite = spriteName.ToLowerInvariant();
                var materialName = sr.sharedMaterial != null ? sr.sharedMaterial.name : "";
                var lowMat = materialName.ToLowerInvariant();

                if (sr.sprite == null)
                {
                    missingSprite++;
                    suspicious.Add("MissingSprite:" + goName);
                }

                if (lowName.Contains("debug") || lowName.Contains("square") || lowName.Contains("placeholder"))
                {
                    suspiciousDebugName++;
                    suspicious.Add("DebugLikeName:" + goName);
                }

                if (lowSprite.Contains("debug") || lowSprite.Contains("square") || lowSprite.Contains("placeholder") || lowSprite == "white" || lowSprite == "builtin sprites")
                {
                    suspiciousSquareSprite++;
                    suspicious.Add("DebugLikeSprite:" + goName + ":" + spriteName);
                }

                if (lowMat.Contains("default") || lowMat.Contains("sprite-default"))
                {
                    suspiciousDefaultMaterial++;
                    suspicious.Add("DefaultMaterial:" + goName + ":" + materialName);
                }
            }

            foreach (var mr in meshRenderers)
            {
                var goName = mr.gameObject.name ?? "";
                var lowName = goName.ToLowerInvariant();
                var materialName = mr.sharedMaterial != null ? mr.sharedMaterial.name : "";
                var lowMat = materialName.ToLowerInvariant();

                if (lowName.Contains("debug") || lowName.Contains("square") || lowName.Contains("placeholder"))
                {
                    suspiciousDebugName++;
                    suspicious.Add("DebugLikeMeshName:" + goName);
                }

                if (lowMat.Contains("default") || lowMat.Contains("default-material"))
                {
                    suspiciousDefaultMaterial++;
                    suspicious.Add("DefaultMeshMaterial:" + goName + ":" + materialName);
                }
            }

            // Best-effort screenshot via main camera -> RenderTexture.
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
                        var rt = new RenderTexture(w, h, 24);
                        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);

                        cam.targetTexture = rt;
                        var prevActive = RenderTexture.active;
                        RenderTexture.active = rt;
                        cam.Render();
                        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                        tex.Apply();
                        cam.targetTexture = prevTarget;
                        RenderTexture.active = prevActive;

                        var bytes = tex.EncodeToPNG();
                        File.WriteAllBytes(shotPath, bytes);

                        UnityEngine.Object.DestroyImmediate(rt);
                        UnityEngine.Object.DestroyImmediate(tex);
                    }
                    else
                    {
                        errors.Add("No camera found for screenshot");
                    }
                }
            }
            catch (Exception exShot)
            {
                errors.Add("Screenshot failed: " + exShot.Message);
            }

            bool hasAnyRenderable = spriteRenderers.Length > 0 || meshRenderers.Length > 0;
            bool tooSuspicious =
                missingSprite > 0 ||
                suspiciousSquareSprite > 0 ||
                suspiciousDebugName > 0 ||
                (!hasAnyRenderable);

            result.spriteRendererCount = spriteRenderers.Length;
            result.meshRendererCount = meshRenderers.Length;
            result.tileLikeObjects = tileLike.Length;
            result.missingSpriteCount = missingSprite;
            result.suspiciousDebugNameCount = suspiciousDebugName;
            result.suspiciousDefaultMaterialCount = suspiciousDefaultMaterial;
            result.suspiciousSquareSpriteCount = suspiciousSquareSprite;
            result.suspiciousObjects = suspicious.Distinct().Take(100).ToArray();
            result.errors = errors.ToArray();

            if (hasAnyRenderable && !tooSuspicious)
            {
                result.success = true;
                result.result = "PASS";
                result.summary = "Nature Light runtime verification passed";
                result.analysis =
                    $"Scene opened, renderers discovered, and no strong debug-square indicators were detected. " +
                    $"spriteRenderers={spriteRenderers.Length}, meshRenderers={meshRenderers.Length}, tileLikeObjects={tileLike.Length}.";
                result.top_open_gaps = Array.Empty<string>();
            }
            else
            {
                result.success = false;
                result.result = "FAIL";
                result.summary = "Nature Light runtime verification failed";
                result.analysis =
                    $"Heuristic verification detected likely placeholder/debug rendering or missing visuals. " +
                    $"spriteRenderers={spriteRenderers.Length}, meshRenderers={meshRenderers.Length}, tileLikeObjects={tileLike.Length}, " +
                    $"missingSprite={missingSprite}, suspiciousDebugName={suspiciousDebugName}, suspiciousSquareSprite={suspiciousSquareSprite}, suspiciousDefaultMaterial={suspiciousDefaultMaterial}.";
                var gaps = new List<string>();
                if (!hasAnyRenderable) gaps.Add("No renderable tile visuals detected in scene.");
                if (missingSprite > 0) gaps.Add("One or more SpriteRenderers are missing sprites.");
                if (suspiciousSquareSprite > 0) gaps.Add("One or more sprites look like debug/placeholder/square visuals.");
                if (suspiciousDebugName > 0) gaps.Add("One or more tile objects still carry debug/placeholder-like naming.");
                if (errors.Count > 0) gaps.Add("Review verifier errors and screenshot artifact.");
                result.top_open_gaps = gaps.ToArray();
            }
        }
        catch (Exception ex)
        {
            result.success = false;
            result.result = "FAIL";
            result.summary = "Nature Light runtime verification crashed";
            result.analysis = ex.ToString();
            result.top_open_gaps = new[] { "Fix Unity verification crash and rerun." };
            result.errors = new[] { ex.Message };
        }

        try
        {
            var json = JsonUtility.ToJson(result, true);
            var dir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(outPath, json);
        }
        catch (Exception exWrite)
        {
            Debug.LogError("Failed to write verifier result: " + exWrite);
            return 71;
        }

        return result.success ? 0 : 70;
    }
}
