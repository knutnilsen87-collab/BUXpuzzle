using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FBL_BoardRenderInspector
{
    private static readonly string RootPath = @"F:\prosjekter\candycrush";
    private static readonly string LatestDir = Path.Combine(RootPath, "logs", "latest");
    private static readonly string SceneRecovery = Path.Combine(LatestDir, "scene_recovery.log");

    private static void Log(string level, string msg)
    {
        try
        {
            Directory.CreateDirectory(LatestDir);
            File.AppendAllText(SceneRecovery, $"[{DateTime.Now:O}] [FBL_BOARD_INSPECT] [{level}] {msg}{Environment.NewLine}");
        }
        catch { }
        Debug.Log($"[FBL_BOARD_INSPECT] {msg}");
    }

    [MenuItem("FBL/Inspect/Board Render State")]
    public static void Inspect()
    {
        var scene = SceneManager.GetActiveScene();
        var board = GameObject.Find("Board");
        var boardViewGo = GameObject.Find("BoardView") ?? board;
        var fallbackRoot = GameObject.Find("FBL_DebugBoard");
        var ddol = GameObject.Find("FBL_BoardVisualFallback");

        Log("INFO", $"Scene={scene.name}");

        if (board == null)
        {
            Log("ERROR", "Board object not found.");
            return;
        }

        var renderers = board.GetComponentsInChildren<Renderer>(true);
        var colliders = board.GetComponentsInChildren<Collider>(true);
        var allChildren = board.GetComponentsInChildren<Transform>(true);

        int fallbackTiles = allChildren.Count(t => t.name.StartsWith("FBL_Tile_"));
        int realTiles = allChildren.Count(t => t.name.StartsWith("Tile_"));

        Log("INFO", $"Board renderers={renderers.Length}, colliders={colliders.Length}, fallbackTiles={fallbackTiles}, realTiles={realTiles}, fallbackRoot={(fallbackRoot ? "YES" : "NO")}, fallbackBootstrap={(ddol ? "YES" : "NO")}");

        foreach (var t in allChildren.Take(25))
        {
            Log("INFO", $"Child: {t.name}");
        }

        if (boardViewGo != null)
        {
            var comp = boardViewGo.GetComponents<MonoBehaviour>().FirstOrDefault(c => c != null && (c.GetType().FullName == "Game.Presentation.BoardView" || c.GetType().Name == "BoardView"));
            if (comp != null)
            {
                var type = comp.GetType();
                var field = type.GetField("TilePrefab", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? type.GetField("_tilePrefab", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                  .FirstOrDefault(f => typeof(GameObject).IsAssignableFrom(f.FieldType) && f.Name.ToLower().Contains("tileprefab"));

                if (field != null)
                {
                    var value = field.GetValue(comp) as GameObject;
                    Log("INFO", $"BoardView TilePrefab field={field.Name} assigned={(value ? value.name : "NULL")}");
                }
                else
                {
                    Log("WARN", "BoardView found but TilePrefab field not found via reflection.");
                }
            }
            else
            {
                Log("WARN", "BoardView MonoBehaviour not found on Board/BoardView object.");
            }
        }
    }

    [MenuItem("FBL/Repair/Purge Fallback And Rebind BoardView")]
    public static void Repair()
    {
        var scene = SceneManager.GetActiveScene();
        var board = GameObject.Find("Board");
        var boardViewGo = GameObject.Find("BoardView") ?? board;

        int purged = 0;

        var fallbackRoot = GameObject.Find("FBL_DebugBoard");
        if (fallbackRoot != null)
        {
            UnityEngine.Object.DestroyImmediate(fallbackRoot);
            purged++;
            Log("INFO", "Destroyed FBL_DebugBoard");
        }

        var ddol = GameObject.Find("FBL_BoardVisualFallback");
        if (ddol != null)
        {
            UnityEngine.Object.DestroyImmediate(ddol);
            purged++;
            Log("INFO", "Destroyed FBL_BoardVisualFallback bootstrap object");
        }

        if (board != null)
        {
            var children = board.GetComponentsInChildren<Transform>(true).ToList();
            foreach (var t in children)
            {
                if (t == null) continue;
                if (t.name.StartsWith("FBL_Tile_"))
                {
                    UnityEngine.Object.DestroyImmediate(t.gameObject);
                    purged++;
                }
            }
            Log("INFO", $"Purged fallback/debug children count={purged}");
        }

        var prefabGuids = AssetDatabase.FindAssets("t:Prefab Tile");
        string prefabPath = prefabGuids
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p => p.Replace("\\", "/").EndsWith("Assets/Game/Presentation/Prefabs/Tile.prefab"));

        if (string.IsNullOrEmpty(prefabPath))
        {
            prefabPath = prefabGuids.Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
        }

        if (string.IsNullOrEmpty(prefabPath))
        {
            Log("ERROR", "Tile.prefab not found.");
            return;
        }

        var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (tilePrefab == null)
        {
            Log("ERROR", "Failed to load Tile.prefab at: " + prefabPath);
            return;
        }

        int assigned = 0;

        if (boardViewGo != null)
        {
            var comp = boardViewGo.GetComponents<MonoBehaviour>().FirstOrDefault(c => c != null && (c.GetType().FullName == "Game.Presentation.BoardView" || c.GetType().Name == "BoardView"));
            if (comp != null)
            {
                var t = comp.GetType();
                var field = t.GetField("TilePrefab", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? t.GetField("_tilePrefab", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                               .FirstOrDefault(f => typeof(GameObject).IsAssignableFrom(f.FieldType) && f.Name.ToLower().Contains("tileprefab"));

                if (field != null)
                {
                    field.SetValue(comp, tilePrefab);
                    EditorUtility.SetDirty(comp);
                    assigned++;
                    Log("INFO", $"Assigned TilePrefab using field {field.Name} -> {prefabPath}");
                }
                else
                {
                    Log("WARN", "BoardView found but TilePrefab field not found.");
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();

        Log("INFO", $"Repair complete. assigned={assigned}, purged={purged}, scene={scene.name}");
    }
}
