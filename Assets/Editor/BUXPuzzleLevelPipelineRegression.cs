using System;
using System.Collections.Generic;
using System.IO;
using Game.Core;
using Game.Levels;
using UnityEditor;
using UnityEngine;

public static class BUXPuzzleLevelPipelineRegression
{
    private const string ReportPath = "Logs/codex_level_pipeline_regression.json";

    public static void RunAndExit()
    {
        var failures = new List<string>();
        var passed = new List<string>();

        CheckRepository(failures, passed);
        CheckValidator(failures, passed);
        CheckBoardDeterminism(failures, passed);
        CheckSpecialOrientation(failures, passed);

        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Logs");
        File.WriteAllText(ReportPath, JsonUtility.ToJson(new Report
        {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            failures = failures.ToArray(),
            passed = passed.ToArray()
        }, true));

        foreach (var pass in passed) Debug.Log("[BUX_PIPELINE_PASS] " + pass);
        foreach (var failure in failures) Debug.LogError("[BUX_PIPELINE_FAIL] " + failure);
        EditorApplication.Exit(failures.Count == 0 ? 0 : 1);
    }

    private static void CheckRepository(List<string> failures, List<string> passed)
    {
        var repo = new LevelRepository();
        var level1 = repo.GetLevel(1);
        Require(level1 != null && level1.Width == 6 && level1.Height == 6, passed, failures, "LevelRepository loads Level_1.json as 6x6.");
        Require(level1 != null && level1.Objectives != null && level1.Objectives.Length > 0 && level1.Objectives[0].Type == LevelObjectiveType.ReachScore, passed, failures, "Level_1 objective maps to score.");

        var fallback = repo.GetLevel(99);
        Require(fallback != null && fallback.Width == 8 && fallback.Height == 8, passed, failures, "LevelRepository falls back to generated legacy levels.");
    }

    private static void CheckValidator(List<string> failures, List<string> passed)
    {
        var valid = new LevelSpecV2
        {
            LevelId = 501,
            Width = 6,
            Height = 6,
            MoveLimit = 8,
            Seed = 501,
            Objectives = new[] { new ObjectiveSpec { Type = LevelObjectiveType.MakeMatches, Target = 2 } }
        };
        Require(LevelValidator.Validate(valid).IsValid, passed, failures, "Valid generated LevelSpecV2 passes validation.");

        var invalid = new LevelSpecV2
        {
            LevelId = 502,
            Width = 2,
            Height = 6,
            MoveLimit = 0,
            Objectives = Array.Empty<ObjectiveSpec>()
        };
        Require(!LevelValidator.Validate(invalid).IsValid, passed, failures, "Invalid LevelSpecV2 fails validation.");
    }

    private static void CheckBoardDeterminism(List<string> failures, List<string> passed)
    {
        var a = new BoardEngine(6, 6, 1234);
        var b = new BoardEngine(6, 6, 1234);
        Require(Snapshot(a) == Snapshot(b), passed, failures, "Same seed generates same board snapshot.");

        var masked = new BoardEngine(5, 5, 1234, new[] { ".....", ".###.", ".M.V.", ".###.", "....." });
        Require(!masked.IsCellActive(1, 1) && masked.GetCell(1, 2).Blocker == CellBlockerType.Moss, passed, failures, "Board mask and cell blockers initialize.");
    }

    private static void CheckSpecialOrientation(List<string> failures, List<string> passed)
    {
        var horizontal = new BoardEngine(5, 5, 1001);
        horizontal.Set(2, 2, new Tile { Type = TileType.A, State = TileState.Line, Special = TileSpecial.LineHorizontal });
        horizontal.Set(2, 3, new Tile { Type = TileType.B, State = TileState.Normal });
        Require(horizontal.TrySwapAndResolveWithTrace(2, 2, 2, 3, out var horizontalTrace), passed, failures, "Horizontal line special activates on swap.");
        Require(TraceClearsRow(horizontalTrace, 2) && !TraceClearsColumn(horizontalTrace, 2), passed, failures, "Horizontal line special clears its row only.");

        var vertical = new BoardEngine(5, 5, 1002);
        vertical.Set(2, 2, new Tile { Type = TileType.A, State = TileState.Line, Special = TileSpecial.LineVertical });
        vertical.Set(3, 2, new Tile { Type = TileType.B, State = TileState.Normal });
        Require(vertical.TrySwapAndResolveWithTrace(2, 2, 3, 2, out var verticalTrace), passed, failures, "Vertical line special activates on swap.");
        Require(TraceClearsColumn(verticalTrace, 2) && !TraceClearsRow(verticalTrace, 2), passed, failures, "Vertical line special clears its column only.");
    }

    private static bool TraceClearsRow(ResolveTrace trace, int y)
    {
        if (trace == null || trace.Steps.Count == 0) return false;
        int count = 0;
        foreach (var coord in trace.Steps[0].Cleared)
        {
            if (coord.Y == y) count++;
        }

        return count >= 5;
    }

    private static bool TraceClearsColumn(ResolveTrace trace, int x)
    {
        if (trace == null || trace.Steps.Count == 0) return false;
        int count = 0;
        foreach (var coord in trace.Steps[0].Cleared)
        {
            if (coord.X == x) count++;
        }

        return count >= 5;
    }

    private static string Snapshot(BoardEngine board)
    {
        var text = new System.Text.StringBuilder();
        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width; x++)
            {
                var tile = board.Get(x, y);
                text.Append((int)tile.Type).Append(':').Append((int)tile.State).Append('|');
            }
        }

        return text.ToString();
    }

    private static void Require(bool condition, List<string> passed, List<string> failures, string message)
    {
        if (condition) passed.Add(message);
        else failures.Add(message);
    }

    [Serializable]
    private sealed class Report
    {
        public string generatedUtc;
        public string[] failures;
        public string[] passed;
    }
}
