using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Presentation
{
    public sealed class FBL_BoardVisualFallback : MonoBehaviour
    {
        private static string RootPath = @"F:\prosjekter\candycrush";
        private static string LatestDir => Path.Combine(RootPath, "logs", "latest");
        private static string SceneRecovery => Path.Combine(LatestDir, "scene_recovery.log");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            try
            {
                Directory.CreateDirectory(LatestDir);
                Log("BOOTSTRAP", "AfterSceneLoad bootstrap started");

                var go = new GameObject("FBL_BoardVisualFallback");
                DontDestroyOnLoad(go);
                go.hideFlags = HideFlags.DontSave;
                go.AddComponent<FBL_BoardVisualFallback>();
            }
            catch (Exception ex)
            {
                Log("ERROR", "Bootstrap failed: " + ex);
            }
        }

        private void Start()
        {
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                Log("ERROR", "Start failed: " + ex);
            }
        }

        private void Run()
        {
            var scene = SceneManager.GetActiveScene();
            var cam = Camera.main;
            var board = GameObject.Find("Board");
            var boardView = GameObject.Find("BoardView");
            var tileInput = GameObject.Find("TileInput");
            var gameRoot = GameObject.Find("GameRoot");

            Log("INFO", $"Scene={scene.name}, cam={(cam ? cam.name : "NULL")}, Board={(board ? "YES" : "NO")}, BoardView={(boardView ? "YES" : "NO")}, TileInput={(tileInput ? "YES" : "NO")}, GameRoot={(gameRoot ? "YES" : "NO")}");

            int rendererCount = 0;
            if (board != null)
            {
                rendererCount = board.GetComponentsInChildren<Renderer>(true).Length;
            }

            Log("INFO", "Board renderer count=" + rendererCount);

            if (rendererCount > 0)
            {
                Log("INFO", "Fallback skipped because board already has visible renderers");
                return;
            }

            var fallbackRoot = new GameObject("FBL_DebugBoard");
            if (board != null) fallbackRoot.transform.SetParent(board.transform, false);

            const int width = 8;
            const int height = 8;
            const float spacing = 1.05f;
            float originX = -((width - 1) * spacing) * 0.5f;
            float originY = -((height - 1) * spacing) * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    q.name = $"FBL_Tile_{x}_{y}";
                    q.transform.SetParent(fallbackRoot.transform, false);
                    q.transform.localPosition = new Vector3(originX + x * spacing, originY + y * spacing, 0f);
                    q.transform.localScale = new Vector3(0.92f, 0.92f, 1f);

                    var renderer = q.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        var mat = renderer.material;
                        float hue = ((x + y) % 6) / 6f;
                        mat.color = Color.HSVToRGB(hue, 0.55f, 0.95f);
                    }

                    var col = q.GetComponent<Collider>();
                    if (col != null) Destroy(col);
                }
            }

            Log("INFO", "Spawned visual fallback 8x8 board");

            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 5.2f;
                cam.transform.position = new Vector3(0f, 0f, -10f);
                cam.transform.rotation = Quaternion.identity;
                Log("INFO", "Adjusted main camera to orthographic fallback framing");
            }
            else
            {
                Log("WARN", "No main camera found; fallback board spawned but camera not adjusted");
            }
        }

        private static void Log(string level, string msg)
        {
            try
            {
                Directory.CreateDirectory(LatestDir);
                File.AppendAllText(SceneRecovery, $"[{DateTime.Now:O}] [FBL_VISUAL_FALLBACK] [{level}] {msg}{Environment.NewLine}");
            }
            catch { }
        }
    }
}
