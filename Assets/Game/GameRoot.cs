using UnityEngine;
using Game.Core;
using Game.Presentation;
using Game.Presentation.Juice;
using Game.Levels;
using Game.Progression;
using Game.Telemetry;
using Game.Onboarding;
using Game.UI;

public sealed class GameRoot : MonoBehaviour
{
    public int Width = 8;
    public int Height = 8;
    public int Seed = 12345;

    private BoardEngine _board;

    public BoardEngine Board => _board;
    private FeedbackSystem _feedback;
    private RewardPipeline _rewards;
    private SessionTelemetryGuard _guard;
    private PlayerSave _save;
    private ProgressionService _progression;
    private LevelSpec _level;
    private float _sessionStartedAt;
    private bool _levelCompleteSent;
    private bool _completionPending;
    private Coroutine _completionRoutine;

    void Awake() {
        Debug.Log("[AUTOPILOT_TRACE] GameRoot Awake start");

        ITelemetry telemetry = new NullTelemetry();
        GameTelemetry.SetClient(telemetry);
        _guard = new SessionTelemetryGuard(telemetry);
        _sessionStartedAt = Time.unscaledTime;
        _save = PlayerSave.Load();
        _progression = new ProgressionService(_save);
        _level = _progression.CurrentLevel();
        ApplyLevelSpec(_level);

        _board = new BoardEngine(Width, Height, Seed);
        _feedback = gameObject.AddComponent<FeedbackSystem>();
        _rewards = new RewardPipeline();
        GameTelemetry.Track("session.start", GameTelemetry.Props(
            "level_id", _level.LevelNumber,
            "board_width", Width,
            "board_height", Height
        ));

        Debug.Log("[GameRoot] Initialized");
Debug.Log("[AUTOPILOT_TRACE] BoardEngine initialized");
        TryInitSceneWiring(true);
    }

    
    private void TryInitSceneWiring(bool resetBoardVisible = false)
    {
        // Wire presentation to engine deterministically at runtime.
        var view = FindFirstObjectByType<BoardView>();
        if (view != null)
        {
            view.Bind(this);
            view.SwapAccepted -= OnSwapAccepted;
            view.SwapAccepted += OnSwapAccepted;
            if (resetBoardVisible) view.ResetBoardVisibleTelemetry();
            view.DrawOrRedrawFromEngine();
        }

        // Ensure HUD exists
        var hud = FindFirstObjectByType<SimpleHud>();
        if (hud == null)
        {
            var go = new GameObject("SimpleHud");
            hud = go.AddComponent<SimpleHud>();
        }

        if (hud != null)
        {
            hud.GoalCompleted -= OnHudGoalCompleted;
            hud.Configure(_level.LevelNumber, _level.GoalMatches, _level.MoveLimit);
            hud.GoalCompleted += OnHudGoalCompleted;
        }

        var tutorial = FindFirstObjectByType<FirstMoveTutorialController>();
        if (tutorial == null)
        {
            gameObject.AddComponent<FirstMoveTutorialController>();
        }
    }

    private void OnHudGoalCompleted()
    {
        if (_levelCompleteSent) return;
        _levelCompleteSent = true;
        _completionPending = true;
        GameTelemetry.Track("level.complete", GameTelemetry.Props(
            "level_id", _level.LevelNumber,
            "seconds_since_session_start", Mathf.RoundToInt(Time.unscaledTime - _sessionStartedAt)
        ));
    }

    private void OnSwapAccepted(BoardMove move, BoardEngine.ResolveSummary summary, ResolveTrace trace, string inputType)
    {
        if (!_completionPending || _completionRoutine != null) return;
        _completionRoutine = StartCoroutine(CompletionSequence());
    }

    private System.Collections.IEnumerator CompletionSequence()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        var hud = FindFirstObjectByType<SimpleHud>();
        BoardJuiceController.Ensure().LevelComplete();
        MissionAccomplishedOverlay.Ensure().Show(
            _level.LevelNumber,
            hud != null ? hud.Matches : _level.GoalMatches,
            hud != null ? hud.targetMatches : _level.GoalMatches,
            StartNextLevel,
            ReplayLevel
        );
    }

    private void StartNextLevel()
    {
        MissionAccomplishedOverlay.Ensure().Hide();
        _level = _progression.AdvanceLevel();
        LoadLevel(_level);
    }

    private void ReplayLevel()
    {
        MissionAccomplishedOverlay.Ensure().Hide();
        LoadLevel(_level);
    }

    private void LoadLevel(LevelSpec level)
    {
        if (_completionRoutine != null)
        {
            StopCoroutine(_completionRoutine);
            _completionRoutine = null;
        }

        _completionPending = false;
        _levelCompleteSent = false;
        ApplyLevelSpec(level);
        _board = new BoardEngine(Width, Height, Seed);
        TryInitSceneWiring(true);
    }

    private void ApplyLevelSpec(LevelSpec level)
    {
        _level = level;
        Width = Mathf.Max(3, level.BoardWidth);
        Height = Mathf.Max(3, level.BoardHeight);
        Seed = level.Seed;
    }

    public void EndLevel(bool win, int secondsPlayed, int movesUsed, int goalsRemaining, bool smartMove)
    {
        ResultsData results = _rewards.Compute(win, secondsPlayed, movesUsed, goalsRemaining, smartMove);

        if (results != null && results.Rewards != null && results.Rewards.Count > 0)
        {
            foreach (var reward in results.Rewards)
            {
                _save.ApplyReward(reward);
            }

            if (win)
            {
                _save.AdvanceLevel();
            }

            _save.Save();
            _guard.OnRewardGranted();
        }

        _feedback.Play(new FeedbackEvent
        {
            Type = FeedbackEventType.Reward,
            Intensity = win ? 2 : 1
        });
    }

    void OnApplicationQuit()
    {
        GameTelemetry.Track("session.abandon", GameTelemetry.Props(
            "level_id", _level.LevelNumber,
            "seconds_since_session_start", Mathf.RoundToInt(Time.unscaledTime - _sessionStartedAt)
        ));
        if (_guard != null) _guard.OnSessionEnd();
    }

    public void ConfigureRuntimeSeed(int seed, int? levelIndex = null)
    {
        if (levelIndex.HasValue)
        {
            _level = LevelManager.GetLevel(levelIndex.Value);
            ApplyLevelSpec(_level);
        }

        Seed = seed;
        _board = new BoardEngine(Width, Height, Seed);
        _completionPending = false;
        _levelCompleteSent = false;

        Debug.Log($"[GameRoot] Runtime seed configured. seed={seed} level={(levelIndex.HasValue ? levelIndex.Value.ToString() : "n/a")}");

        TryInitSceneWiring(true);
    }
}
