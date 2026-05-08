using System;
using System.Collections.Generic;
using System.IO;
using Game.Core;
using Game.Levels;
using Game.Onboarding;
using Game.Presentation;
using Game.Progression;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class BUXPuzzleReleaseValidation
{
    private const string MainScene = "Assets/Scenes/game.unity";
    private const string ReportPath = "Logs/codex_release_validation.json";

    private static readonly List<string> Failures = new List<string>();
    private static readonly List<string> Warnings = new List<string>();
    private static readonly List<string> Passed = new List<string>();

    public static void RunAllAndExit()
    {
        RunAll();
        EditorApplication.Exit(Failures.Count == 0 ? 0 : 1);
    }

    public static void RunAll()
    {
        Failures.Clear();
        Warnings.Clear();
        Passed.Clear();

        ValidateProjectSettings();
        ValidateGameplayCore();
        ValidateEndlessGenerator();
        ValidateProgressionSave();
        ValidateScene();
        ValidatePresentationAssets();
        ValidateAudioAssets();
        ValidateStoreReadinessScaffold();

        WriteReport();

        foreach (var pass in Passed) Debug.Log("[BUX_RELEASE_PASS] " + pass);
        foreach (var warning in Warnings) Debug.LogWarning("[BUX_RELEASE_WARN] " + warning);
        foreach (var failure in Failures) Debug.LogError("[BUX_RELEASE_FAIL] " + failure);
    }

    private static void ValidateProjectSettings()
    {
        Require(PlayerSettings.productName == "BUXPuzzle", "Product name is BUXPuzzle", "Product name must be BUXPuzzle.");
        Require(!string.IsNullOrWhiteSpace(PlayerSettings.companyName), "Company name is configured", "Company name is missing.");

        var androidId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
        var iosId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);

        Require(IsValidBundleId(androidId), "Android bundle identifier is configured", "Android bundle identifier is missing or invalid.");
        Require(IsValidBundleId(iosId), "iOS bundle identifier is configured", "iOS bundle identifier is missing or invalid.");

        if (androidId != iosId)
        {
            Warnings.Add("Android and iOS bundle identifiers differ. This can be intentional, but store metadata must match.");
        }
    }

    private static void ValidateGameplayCore()
    {
        var board = new BoardEngine(8, 8, 12345);
        Require(board.Width == 8 && board.Height == 8, "BoardEngine creates an 8x8 board", "BoardEngine dimensions are wrong.");
        Require(board.HasAnyValidMove(), "BoardEngine stable start has a valid move", "BoardEngine generated a dead starting board.");
        Require(ValidMoveFinder.TryFind(board, out var tutorialMove), "ValidMoveFinder returns a tutorial move", "ValidMoveFinder could not find a first move.");
        Require(board.WouldSwapCreateMatch(tutorialMove.A.X, tutorialMove.A.Y, tutorialMove.B.X, tutorialMove.B.Y), "ValidMoveFinder move creates a match", "ValidMoveFinder returned a move that does not create a match.");

        bool accepted = false;
        BoardEngine.ResolveSummary acceptedSummary = BoardEngine.ResolveSummary.New();
        ResolveTrace acceptedTrace = null;
        for (int y = 0; y < board.Height && !accepted; y++)
        {
            for (int x = 0; x < board.Width && !accepted; x++)
            {
                if (x + 1 < board.Width)
                {
                    accepted = board.TrySwapAndResolveWithTrace(x, y, x + 1, y, out acceptedTrace);
                }

                if (!accepted && y + 1 < board.Height)
                {
                    accepted = board.TrySwapAndResolveWithTrace(x, y, x, y + 1, out acceptedTrace);
                }
            }
        }

        if (acceptedTrace != null) acceptedSummary = acceptedTrace.Summary;
        Require(accepted, "BoardEngine accepts at least one legal swap", "No legal swap could be executed from a stable board.");
        Require(acceptedSummary.clearedTiles >= 3, "Accepted swap clears at least three tiles", "Accepted swap did not clear enough tiles.");
        Require(acceptedTrace != null && acceptedTrace.Steps.Count == acceptedSummary.iterations, "Accepted swap records resolve trace", "Resolve trace is missing or does not match iteration count.");
        Require(board.HasAnyValidMove(), "BoardEngine keeps a legal move after accepted resolve", "BoardEngine ended in a dead-board state after resolve.");

        var invalidBoard = new BoardEngine(8, 8, 54321);
        string beforeInvalid = Snapshot(invalidBoard);
        BoardEngine.ResolveSummary invalidSummary;
        bool invalidAccepted = invalidBoard.TrySwapAndResolve(0, 0, 2, 0, out invalidSummary);
        Require(!invalidAccepted, "BoardEngine rejects non-adjacent swaps", "BoardEngine accepted a non-adjacent swap.");
        Require(Snapshot(invalidBoard) == beforeInvalid, "Invalid swap does not mutate board", "Invalid swap changed board state.");

        var adjacentInvalidBoard = new BoardEngine(8, 8, 98765);
        Require(TryFindAdjacentInvalidMove(adjacentInvalidBoard, out var invalidMove), "Validation found an adjacent invalid swap", "Could not find an adjacent invalid swap to test.");
        string beforeAdjacentInvalid = Snapshot(adjacentInvalidBoard);
        bool adjacentInvalidAccepted = adjacentInvalidBoard.TrySwapAndResolve(invalidMove.A.X, invalidMove.A.Y, invalidMove.B.X, invalidMove.B.Y, out invalidSummary);
        Require(!adjacentInvalidAccepted, "BoardEngine rejects adjacent swaps that make no match", "BoardEngine accepted an adjacent non-matching swap.");
        Require(Snapshot(adjacentInvalidBoard) == beforeAdjacentInvalid, "Adjacent invalid swap does not mutate board", "Adjacent invalid swap changed board state.");
    }

    private static void ValidateProgressionSave()
    {
        PlayerSave.Clear();
        var save = PlayerSave.Load();
        Require(save.CurrentLevel == 1, "PlayerSave starts at level 1", "PlayerSave did not start at level 1.");

        var reward = new RewardGrant { Currency = "xp", Amount = 25, Reason = "validation" };
        save.ApplyReward(reward);
        save.ApplyReward(new RewardGrant { Currency = "token", Amount = 2, Reason = "validation" });
        save.ApplyReward(new RewardGrant { Currency = "fragment", Amount = 3, Reason = "validation" });
        save.AdvanceLevel();
        save.Save();

        var loaded = PlayerSave.Load();
        Require(loaded.XP == 25, "PlayerSave persists XP", "PlayerSave did not persist XP.");
        Require(loaded.Tokens == 2, "PlayerSave persists tokens", "PlayerSave did not persist tokens.");
        Require(loaded.Fragments == 3, "PlayerSave persists fragments", "PlayerSave did not persist fragments.");
        Require(loaded.CurrentLevel == 2, "PlayerSave persists current level", "PlayerSave did not persist current level.");
        PlayerSave.Clear();

        var rewards = new RewardPipeline().Compute(true, 30, 12, 0, true);
        Require(rewards.Rewards.Count >= 3, "RewardPipeline grants progress rewards", "RewardPipeline did not grant baseline progress rewards.");
    }

    private static void ValidateEndlessGenerator()
    {
        for (int level = 1; level <= 25; level++)
        {
            var generated = DeterministicEndlessLevelGenerator.GenerateBoard(8, 8, 6, level);
            Require(!HasAnyMatch(generated, 8, 8), $"Level {level} starts without immediate matches", $"Level {level} starts with an immediate match.");
            Require(HasLegalMove(generated, 8, 8), $"Level {level} has at least one legal move", $"Level {level} has no legal moves.");
        }
    }

    private static void ValidateScene()
    {
        var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
        Require(scene.IsValid() && scene.isLoaded, "Main scene loads", "Main scene could not be loaded.");

        var roots = scene.GetRootGameObjects();
        var gameRoots = FindAll<GameRoot>(roots);
        var boardViews = FindAll<BoardView>(roots);
        var tileInputs = FindAll<TileInput>(roots);

        Require(gameRoots.Count == 1, "Scene has one GameRoot", $"Scene must have exactly one GameRoot, found {gameRoots.Count}.");
        Require(boardViews.Count == 1, "Scene has one BoardView", $"Scene must have exactly one BoardView, found {boardViews.Count}.");
        Require(tileInputs.Count == 1, "Scene has one TileInput", $"Scene must have exactly one TileInput, found {tileInputs.Count}.");
        Require(Camera.main != null, "Scene has a MainCamera", "Scene is missing a camera tagged MainCamera.");

        if (boardViews.Count > 0)
        {
            var serialized = new SerializedObject(boardViews[0]);
            var naturePrefab = serialized.FindProperty("natureLightTilePrefab").objectReferenceValue as GameObject;
            var fallbackPrefab = serialized.FindProperty("TilePrefab").objectReferenceValue as GameObject;

            Require(naturePrefab != null, "BoardView has Nature Light tile prefab", "BoardView natureLightTilePrefab is missing.");
            Require(fallbackPrefab != null, "BoardView has fallback tile prefab", "BoardView TilePrefab is missing.");

            if (naturePrefab != null)
            {
                Require(naturePrefab.GetComponentInChildren<Collider>() != null, "Nature Light tile prefab has collider for input", "Nature Light tile prefab needs a Collider for TileInput raycasts.");
                Require(naturePrefab.GetComponentInChildren<Renderer>() != null, "Nature Light tile prefab has renderer", "Nature Light tile prefab needs a Renderer.");
            }
        }
    }

    private static void ValidatePresentationAssets()
    {
        Require(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/NatureLightTile.prefab") != null, "Nature Light tile prefab exists", "NatureLightTile prefab is missing.");
        Require(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Game/Presentation/NatureLight/NatureLightArtManifest.json") != null, "Nature Light art manifest exists", "Nature Light art manifest is missing.");
        Require(File.Exists("docs/style-guide/Nature_Light_Production_Style_Guide_v2.md"), "Nature Light style guide exists", "Nature Light style guide is missing.");
        Require(File.Exists("docs/implementation-spec/Nature_Light_Unity_Implementation_Spec.md"), "Nature Light implementation spec exists", "Nature Light implementation spec is missing.");
    }

    private static void ValidateAudioAssets()
    {
        string[] clips =
        {
            "board_settle.wav",
            "cascade_roll.wav",
            "combo_bloom.wav",
            "match_pop.wav",
            "spawn_soft.wav",
            "swap_invalid.wav",
            "swap_soft.wav"
        };

        foreach (var clip in clips)
        {
            string path = "Assets/Resources/Audio/" + clip;
            Require(File.Exists(path), "Audio clip exists: " + clip, "Missing audio clip: " + clip);
        }

        Require(File.Exists("Assets/Game/Presentation/Audio/FBL_AudioEventMap.json"), "Audio event map exists", "Audio event map is missing.");
    }

    private static void ValidateStoreReadinessScaffold()
    {
        Require(File.Exists("README.md"), "README exists", "README.md is missing.");
        Require(File.Exists("RELEASE_DOD.md"), "Release DoD exists", "RELEASE_DOD.md is missing.");
        Require(File.Exists("STORE_READINESS.md"), "Store readiness tracker exists", "STORE_READINESS.md is missing.");
        Require(File.Exists("STORE_METADATA_DRAFT.md"), "Store metadata draft exists", "STORE_METADATA_DRAFT.md is missing.");
        Require(File.Exists("PRIVACY_POLICY_DRAFT.md"), "Privacy policy draft exists", "PRIVACY_POLICY_DRAFT.md is missing.");
        Require(File.Exists("QA_DEVICE_MATRIX.md"), "QA device matrix exists", "QA_DEVICE_MATRIX.md is missing.");
        Require(File.Exists("docs/qa/UX_FIRST_SESSION_QA.md"), "First-session UX QA checklist exists", "First-session UX QA checklist is missing.");

        if (string.IsNullOrWhiteSpace(PlayerSettings.Android.keystoreName))
        {
            Warnings.Add("Android release signing is not configured. This blocks Play Store upload until a real keystore is supplied.");
        }

        if (PlayerSettings.GetIcons(NamedBuildTarget.Android, IconKind.Application).Length == 0)
        {
            Warnings.Add("Android app icons are not configured.");
        }

        if (PlayerSettings.GetIcons(NamedBuildTarget.iOS, IconKind.Application).Length == 0)
        {
            Warnings.Add("iOS app icons are not configured.");
        }
    }

    private static void Require(bool condition, string pass, string fail)
    {
        if (condition) Passed.Add(pass);
        else Failures.Add(fail);
    }

    private static bool IsValidBundleId(string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Contains(".") &&
               !value.Contains(" ") &&
               !value.EndsWith(".", StringComparison.Ordinal);
    }

    private static List<T> FindAll<T>(IEnumerable<GameObject> roots) where T : Object
    {
        var result = new List<T>();
        foreach (var root in roots)
        {
            result.AddRange(root.GetComponentsInChildren<T>(true));
        }
        return result;
    }

    private static bool HasAnyMatch(int[,] board, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            int streak = 1;
            for (int x = 1; x < width; x++)
            {
                streak = board[x, y] == board[x - 1, y] ? streak + 1 : 1;
                if (streak >= 3) return true;
            }
        }

        for (int x = 0; x < width; x++)
        {
            int streak = 1;
            for (int y = 1; y < height; y++)
            {
                streak = board[x, y] == board[x, y - 1] ? streak + 1 : 1;
                if (streak >= 3) return true;
            }
        }

        return false;
    }

    private static bool HasLegalMove(int[,] board, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x + 1 < width)
                {
                    Swap(board, x, y, x + 1, y);
                    bool ok = HasAnyMatch(board, width, height);
                    Swap(board, x, y, x + 1, y);
                    if (ok) return true;
                }

                if (y + 1 < height)
                {
                    Swap(board, x, y, x, y + 1);
                    bool ok = HasAnyMatch(board, width, height);
                    Swap(board, x, y, x, y + 1);
                    if (ok) return true;
                }
            }
        }

        return false;
    }

    private static void Swap(int[,] board, int x1, int y1, int x2, int y2)
    {
        int temp = board[x1, y1];
        board[x1, y1] = board[x2, y2];
        board[x2, y2] = temp;
    }

    private static string Snapshot(BoardEngine board)
    {
        var parts = new List<string>();
        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width; x++)
            {
                var tile = board.Get(x, y);
                parts.Add(((int)tile.Type) + ":" + ((int)tile.State));
            }
        }

        return string.Join(",", parts);
    }

    private static bool TryFindAdjacentInvalidMove(BoardEngine board, out BoardMove move)
    {
        move = default(BoardMove);
        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width; x++)
            {
                if (x + 1 < board.Width && !board.WouldSwapCreateMatch(x, y, x + 1, y))
                {
                    move = new BoardMove(new BoardCoord(x, y), new BoardCoord(x + 1, y));
                    return true;
                }

                if (y + 1 < board.Height && !board.WouldSwapCreateMatch(x, y, x, y + 1))
                {
                    move = new BoardMove(new BoardCoord(x, y), new BoardCoord(x, y + 1));
                    return true;
                }
            }
        }

        return false;
    }

    private static void WriteReport()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Logs");
        var json = JsonUtility.ToJson(new ValidationReport
        {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            failures = Failures.ToArray(),
            warnings = Warnings.ToArray(),
            passed = Passed.ToArray()
        }, true);
        File.WriteAllText(ReportPath, json);
    }

    [Serializable]
    private sealed class ValidationReport
    {
        public string generatedUtc;
        public string[] failures;
        public string[] warnings;
        public string[] passed;
    }
}
