using UnityEngine;
using Game.Core;
using Game.Presentation;
using Game.Progression;
using Game.Telemetry;

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

    void Awake() { 
        Debug.Log("[AUTOPILOT_TRACE] GameRoot Awake start");

        ITelemetry telemetry = new NullTelemetry();
        _guard = new SessionTelemetryGuard(telemetry);

        _board = new BoardEngine(Width, Height, Seed);
        _feedback = gameObject.AddComponent<FeedbackSystem>();
        _rewards = new RewardPipeline();

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
            go.AddComponent<Game.UI.SimpleHud>();
        }
    }

    public void EndLevel(bool win, int secondsPlayed, int movesUsed, int goalsRemaining, bool smartMove)
    {
        ResultsData results = _rewards.Compute(win, secondsPlayed, movesUsed, goalsRemaining, smartMove);

        if (results != null && results.Rewards != null && results.Rewards.Count > 0)
        {
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
