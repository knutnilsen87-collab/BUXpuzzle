#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class FBL_SceneRecovery
{
    private const string PrefEnabled = "FBL.SceneRecovery.Enabled";
    private static bool _ranThisLaunch = false;

    static FBL_SceneRecovery()
    {
        EditorApplication.delayCall += RunOnce;
    }

    [MenuItem("FBL/Scene Recovery (Enabled)", priority = 0)]
    private static void Toggle()
    {
        bool enabled = IsEnabled();
        EditorPrefs.SetBool(PrefEnabled, !enabled);
        Debug.Log($"[FBL] SceneRecovery now {(!enabled ? "ENABLED" : "DISABLED")}");
        Log($"toggle => {(!enabled ? "ENABLED" : "DISABLED")}");
    }

    [MenuItem("FBL/Scene Recovery (Enabled)", true)]
    private static bool ToggleValidate()
    {
        Menu.SetChecked("FBL/Scene Recovery (Enabled)", IsEnabled());
        return true;
    }

    private static bool IsEnabled() => EditorPrefs.GetBool(PrefEnabled, true);

    private static string LatestLogDir()
    {
        // Application.dataPath = ...\\candycrush\\BUXPuzzle\\Assets
        var projectRoot = Directory.GetParent(Application.dataPath)?.FullName; // ...\\BUXPuzzle
        var outerRoot   = Directory.GetParent(projectRoot ?? "")?.FullName;   // ...\\candycrush
        return Path.Combine(outerRoot ?? projectRoot ?? "", "logs", "latest");
    }

    private static string SingleLogPath() => Path.Combine(LatestLogDir(), "scene_recovery.log");

    private static void AppendLineSafe(string file, string line)
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                {
                    fs.Seek(0, SeekOrigin.End);
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(line);
                    }
                }
                return;
            }
            catch
            {
                System.Threading.Thread.Sleep(60);
            }
        }
    }

    private static void Log(string msg)
    {
        try
        {
            var file = SingleLogPath();

            // rotate if > 5MB
            try
            {
                if (File.Exists(file))
                {
                    var fi = new FileInfo(file);
                    if (fi.Length > 5 * 1024 * 1024)
                    {
                        var rotated = Path.Combine(LatestLogDir(), "scene_recovery.prev.log");
                        try { File.Copy(file, rotated, true); } catch { }
                        try { File.WriteAllText(file, ""); } catch { }
                    }
                }
            }
            catch { }

            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}";
            AppendLineSafe(file, line);

            Debug.Log("[FBL_SCENE_RECOVERY] " + msg);
        }
        catch { }
    }

    private static void RunOnce()
    {
        try
        {
            if (_ranThisLaunch) return;
            _ranThisLaunch = true;

            if (!IsEnabled()) { Log("disabled via EditorPrefs"); return; }
            if (Application.isBatchMode) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            var active = EditorSceneManager.GetActiveScene();
            Log($"startup activeScene.name={active.name} path={(string.IsNullOrEmpty(active.path) ? "<empty>" : active.path)}");

            // 1) Ensure correct scene
            if (string.IsNullOrEmpty(active.path))
            {
                var scenePath = FindBestScenePath();
                if (!string.IsNullOrEmpty(scenePath))
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    Log("opened scene: " + scenePath);
                }
                else
                {
                    Log("no scene found to open (searched scenes by keywords).");
                    return;
                }
            }

            // 2) Repair (no prefab mode)
            RepairScene_NoPrefab();
        }
        catch (Exception ex)
        {
            Log("FAILED: " + ex);
        }
    }

    private static string FindBestScenePath()
    {
        // Priority keywords
        string[] priority = new[] { "game", "main", "play", "level", "match" };

        var guids = AssetDatabase.FindAssets("t:Scene");
        var paths = guids.Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (paths.Length == 0) return null;

        // Prefer Assets/Game/Scenes
        var gameScenes = paths.Where(p => p.IndexOf("Assets/Game/Scenes", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
        var pool = gameScenes.Length > 0 ? gameScenes : paths;

        foreach (var key in priority)
        {
            var hit = pool.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p).IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrEmpty(hit)) return hit;
        }

        return pool[0];
    }

    // --- Reflection helpers
    private static Type FindTypeByName(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetTypes().FirstOrDefault(x => x.Name == typeName);
                if (t != null) return t;
            }
            catch { }
        }
        return null;
    }

    private static Component EnsureComponent(string goName, string typeName, ref bool changed)
    {
        var go = GameObject.Find(goName);
        if (go == null)
        {
            go = new GameObject(goName);
            changed = true;
            Log($"created GO: {goName}");
        }

        var t = FindTypeByName(typeName);
        if (t == null)
        {
            Log($"MISSING type: {typeName} (class name differs or script not compiled)");
            return null;
        }

        var existing = go.GetComponent(t);
        if (existing != null)
        {
            Log($"exists: {goName} has {typeName}");
            return (Component)existing;
        }

        var c = go.AddComponent(t);
        changed = true;
        Log($"added component: {typeName} on {goName}");
        return c;
    }

    private static bool AnyOfTheseExists(params string[] names)
    {
        foreach (var n in names)
            if (GameObject.Find(n) != null) return true;
        return false;
    }

    private static void RepairScene_NoPrefab()
    {
        bool changed = false;

        // --- GameRoot
        EnsureComponent("GameRoot", "GameRoot", ref changed);

        // --- Board root + BoardView component
        if (!AnyOfTheseExists("Board", "BoardView"))
        {
            var board = new GameObject("Board");
            changed = true;
            Log("created GO: Board");
        }
        EnsureComponent("Board", "BoardView", ref changed);

        // --- Input GO + TileInput component
        EnsureComponent("TileInput", "TileInput", ref changed);

        if (changed)
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            Log("scene marked dirty (repaired). Save with Ctrl+S.");
        }
        else
        {
            Log("scene OK (no changes needed).");
        }
    }
}
#endif
