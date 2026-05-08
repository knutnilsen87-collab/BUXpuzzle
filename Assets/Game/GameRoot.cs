using UnityEngine;
using Game.Core;
using Game.Presentation;
using Game.Progression;
using Game.Telemetry;
using Game.Onboarding;

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
    private float _sessionStartedAt;
    private bool _levelCompleteSent;

    void Awake() { 
        Debug.Log("[AUTOPILOT_TRACE] GameRoot Awake start");

        ITelemetry telemetry = new NullTelemetry();
        GameTelemetry.SetClient(telemetry);
        _guard = new SessionTelemetryGuard(telemetry);
        _sessionStartedAt = Time.unscaledTime;

        _board = new BoardEngine(Width, Height, Seed);
        _feedback = gameObject.AddComponent<FeedbackSystem>();
        _rewards = new RewardPipeline();
        _save = PlayerSave.Load();
        GameTelemetry.Track("session.start", GameTelemetry.Props(
            "level_id", _save != null ? _save.CurrentLevel : 1,
            "board_width", Width,
            "board_height", Height
        ));

        Debug.Log("[GameRoot] Initialized");
Debug.Log("[AUTOPILOT_TRACE] BoardEngine initialized");
        TryInitSceneWiring();
    }

    
    private void TryInitSceneWiring()
    {
        // Wire presentation to engine deterministically at runtime.
        var view = FindFirstObjectByType<BoardView>();
        if (view != null)
        {
            view.Bind(this);
            view.DrawOrRedrawFromEngine();
        }

        // Ensure HUD exists
        var hud = FindFirstObjectByType<Game.UI.SimpleHud>();
        if (hud == null)
        {
            var go = new GameObject("SimpleHud");
            hud = go.AddComponent<Game.UI.SimpleHud>();
        }

        if (hud != null)
        {
            hud.Configure(_save != null ? _save.CurrentLevel : 1, 10, 20);
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
        GameTelemetry.Track("level.complete", GameTelemetry.Props(
            "level_id", _save != null ? _save.CurrentLevel : 1,
            "seconds_since_session_start", Mathf.RoundToInt(Time.unscaledTime - _sessionStartedAt)
        ));
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
            "level_id", _save != null ? _save.CurrentLevel : 1,
            "seconds_since_session_start", Mathf.RoundToInt(Time.unscaledTime - _sessionStartedAt)
        ));
        if (_guard != null) _guard.OnSessionEnd();
    }

    public void ConfigureRuntimeSeed(int seed, int? levelIndex = null)
    {
        Seed = seed;
        _board = new BoardEngine(Width, Height, Seed);

        Debug.Log($"[GameRoot] Runtime seed configured. seed={seed} level={(levelIndex.HasValue ? levelIndex.Value.ToString() : "n/a")}");

        TryInitSceneWiring();
    }
}
