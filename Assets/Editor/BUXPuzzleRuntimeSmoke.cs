using System;
using System.Collections.Generic;
using System.IO;
using Game.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BUXPuzzleRuntimeSmoke
{
    private const string MainScene = "Assets/Scenes/game.unity";
    private const string ReportPath = "Logs/codex_runtime_smoke.json";

    private static readonly List<string> Failures = new List<string>();
    private static readonly List<string> Warnings = new List<string>();
    private static readonly List<string> Passed = new List<string>();
    private static double _startTime;
    private static int _playFrames;
    private static bool _completed;
    private static bool _previousEnterPlayModeOptionsEnabled;
    private static EnterPlayModeOptions _previousEnterPlayModeOptions;

    public static void RunAndExit()
    {
        Failures.Clear();
        Warnings.Clear();
        Passed.Clear();
        _completed = false;
        _playFrames = 0;
        _startTime = EditorApplication.timeSinceStartup;

        Application.logMessageReceived += OnLogMessageReceived;
        EditorApplication.update += OnUpdate;

        EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
        _previousEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
        _previousEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.EnterPlaymode();
    }

    private static void OnUpdate()
    {
        if (_completed) return;

        if (EditorApplication.timeSinceStartup - _startTime > 30)
        {
            Failures.Add("Runtime smoke test timed out.");
            Finish();
            return;
        }

        if (!EditorApplication.isPlaying) return;

        _playFrames++;
        if (_playFrames < 20) return;

        RunRuntimeAssertions();
        Finish();
    }

    private static void RunRuntimeAssertions()
    {
        var root = UnityEngine.Object.FindFirstObjectByType<GameRoot>();
        var board = UnityEngine.Object.FindFirstObjectByType<BoardView>();
        var input = UnityEngine.Object.FindFirstObjectByType<TileInput>();
        var tiles = UnityEngine.Object.FindObjectsByType<TileView>(FindObjectsSortMode.None);
        var textSymbols = UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None);

        Require(root != null, "Runtime GameRoot exists", "Runtime GameRoot was not found.");
        Require(board != null, "Runtime BoardView exists", "Runtime BoardView was not found.");
        Require(input != null, "Runtime TileInput exists", "Runtime TileInput was not found.");
        int expectedTiles = root != null && root.Board != null ? CountActiveCells(root.Board) : 64;
        Require(tiles.Length == expectedTiles, "Runtime draws expected tile count", $"Expected {expectedTiles} tiles, found {tiles.Length}.");
        Require(CountTileColliders(tiles) == expectedTiles, "Every runtime tile has a collider", "One or more runtime tiles are missing colliders.");
        Require(CountTileSprites(tiles) == expectedTiles, "Runtime tiles expose configured sprites", "One or more runtime tiles are missing configured sprites.");
        Require(textSymbols.Length == 0, "Runtime does not use placeholder text symbols", $"Found {textSymbols.Length} TextMesh placeholder symbols.");

        if (Camera.main != null && tiles.Length > 0)
        {
            Physics.SyncTransforms();
            var tilePosition = tiles[0].transform.position;
            var ray = new Ray(new Vector3(tilePosition.x, tilePosition.y, Camera.main.transform.position.z), Camera.main.transform.forward);
            Require(Physics.Raycast(ray, out _, 200f), "Camera-aligned ray hits a tile collider", "Camera-aligned ray did not hit a tile collider.");
        }
        else
        {
            Failures.Add("Runtime has no MainCamera.");
        }

        if (board != null && tiles.Length == expectedTiles)
        {
            bool sawAccepted = false;
            bool sawRejected = false;

            for (int i = 0; i < tiles.Length; i++)
            {
                for (int j = 0; j < tiles.Length; j++)
                {
                    if (i == j) continue;
                    if (!AreAdjacent(tiles[i], tiles[j])) continue;

                    Game.Core.BoardEngine.ResolveSummary summary;
                    bool ok = board.RequestSwap(tiles[i], tiles[j], out summary);
                    sawAccepted |= ok;
                    sawRejected |= !ok;

                    if (sawAccepted && sawRejected) break;
                }

                if (sawAccepted && sawRejected) break;
            }

            Require(sawAccepted, "Runtime accepts at least one board swap", "No accepted runtime swap was observed.");
            Require(sawRejected, "Runtime rejects at least one board swap", "No rejected runtime swap was observed.");
        }
    }

    private static int CountTileColliders(TileView[] tiles)
    {
        int count = 0;
        foreach (var tile in tiles)
        {
            if (tile != null && tile.GetComponentInChildren<Collider>() != null) count++;
        }
        return count;
    }

    private static int CountActiveCells(Game.Core.BoardEngine engine)
    {
        if (engine == null) return 0;
        int count = 0;
        for (int y = 0; y < engine.Height; y++)
        {
            for (int x = 0; x < engine.Width; x++)
            {
                if (engine.IsCellActive(x, y)) count++;
            }
        }

        return count;
    }

    private static int CountTileSprites(TileView[] tiles)
    {
        int count = 0;
        foreach (var tile in tiles)
        {
            if (tile == null) continue;
            if (HasSprite(tile.transform.Find("TileBase")) || HasSprite(tile.transform.Find("TileSymbol"))) count++;
        }

        return count;
    }

    private static bool HasSprite(Transform child)
    {
        if (child == null) return false;
        var renderer = child.GetComponent<SpriteRenderer>();
        return renderer != null && renderer.sprite != null;
    }

    private static bool AreAdjacent(TileView a, TileView b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y) == 1;
    }

    private static void Require(bool condition, string pass, string fail)
    {
        if (condition) Passed.Add(pass);
        else Failures.Add(fail);
    }

    private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if ((condition != null && condition.Contains("UnityEditor.Search.SearchDatabase")) ||
            (stackTrace != null && stackTrace.Contains("UnityEditor.Search.SearchDatabase")))
        {
            Warnings.Add("Ignored UnityEditor.Search startup exception during batchmode smoke.");
            return;
        }

        if (condition != null &&
            condition.Contains("Sharing violation") &&
            condition.Contains("CandyCrushLab") &&
            condition.Contains("main.log"))
        {
            Warnings.Add("Ignored LocalLow log sharing violation during batchmode smoke.");
            return;
        }

        if (type == LogType.Exception)
        {
            Failures.Add(condition);
        }
        else if (type == LogType.Error || type == LogType.Assert)
        {
            Warnings.Add(condition);
        }
    }

    private static void Finish()
    {
        _completed = true;
        EditorApplication.update -= OnUpdate;
        Application.logMessageReceived -= OnLogMessageReceived;
        EditorSettings.enterPlayModeOptionsEnabled = _previousEnterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptions = _previousEnterPlayModeOptions;

        WriteReport();

        foreach (var pass in Passed) Debug.Log("[BUX_RUNTIME_PASS] " + pass);
        foreach (var warning in Warnings) Debug.LogWarning("[BUX_RUNTIME_WARN] " + warning);
        foreach (var failure in Failures) Debug.LogError("[BUX_RUNTIME_FAIL] " + failure);

        EditorApplication.ExitPlaymode();
        EditorApplication.Exit(Failures.Count == 0 ? 0 : 1);
    }

    private static void WriteReport()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Logs");
        var json = JsonUtility.ToJson(new RuntimeSmokeReport
        {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            failures = Failures.ToArray(),
            warnings = Warnings.ToArray(),
            passed = Passed.ToArray()
        }, true);
        File.WriteAllText(ReportPath, json);
    }

    [Serializable]
    private sealed class RuntimeSmokeReport
    {
        public string generatedUtc;
        public string[] failures;
        public string[] warnings;
        public string[] passed;
    }
}
