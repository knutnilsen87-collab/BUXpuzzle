using UnityEngine;
using Game.Audio;
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
    private ScoreService _score;
    private ObjectiveTracker _objectives;
    private GardenState _garden;
    private DailyChallengeService _daily;
    private SessionTelemetryGuard _guard;
    private PlayerSave _save;
    private ProgressionService _progression;
    private LevelSpec _level;
    private float _sessionStartedAt;
    private bool _levelCompleteSent;
    private bool _completionPending;
    private bool _levelEnded;
    private bool _usedRetry;
    private int _invalidSwapCount;
    private string _lastShareCode;
    public bool IsLevelEnded => _levelEnded;

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

        _board = new BoardEngine(Width, Height, Seed, _level.BoardRows);
        ApplyLevelMechanics();
        _feedback = gameObject.AddComponent<FeedbackSystem>();
        _rewards = new RewardPipeline();
        _score = new ScoreService();
        _objectives = new ObjectiveTracker();
        _objectives.Initialize(_level);
        _garden = GardenState.Load();
        _daily = new DailyChallengeService();
        _score.Reset();
        StartAudioForLevel();
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
            view.SwapRejected -= OnSwapRejected;
            view.SwapRejected += OnSwapRejected;
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
            if (_objectives == null)
            {
                _objectives = new ObjectiveTracker();
                _objectives.Initialize(_level);
            }

            hud.Configure(_level.LevelNumber, _objectives.Label, _objectives.Target, _level.MoveLimit);
            hud.autoCompleteFromMatch = _level.ObjectiveType == LevelObjectiveType.MakeMatches;
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
        if (_levelEnded || _levelCompleteSent) return;
        _levelCompleteSent = true;
        _completionPending = true;
    }

    private void OnSwapAccepted(BoardMove move, BoardEngine.ResolveSummary summary, ResolveTrace trace, string inputType)
    {
        if (_levelEnded) return;

        _score.ApplySwapResult(trace);

        var hud = FindFirstObjectByType<SimpleHud>();
        int movesLeft = hud != null ? hud.movesLeft : 0;
        int movesUsed = Mathf.Max(0, _level.MoveLimit - movesLeft);
        int liveScore = _score.FinalizeScore(false, movesLeft, _invalidSwapCount == 0, true, !_usedRetry);

        if (_objectives != null)
        {
            _objectives.Apply(trace, liveScore, movesUsed);
        }

        if (hud != null)
        {
            hud.SetScore(_score.Score);
            if (_objectives != null)
            {
                hud.SetObjectiveProgress(_objectives.Progress, _objectives.Target);
            }
        }

        if (_completionPending || (_objectives != null && _objectives.IsComplete))
        {
            EndCurrentLevel(true);
            return;
        }

        if (hud != null && hud.movesLeft <= 0)
        {
            EndCurrentLevel(false);
        }
    }

    private void OnSwapRejected(BoardMove move, string inputType)
    {
        if (_levelEnded) return;
        _invalidSwapCount++;
    }

    private void EndCurrentLevel(bool win)
    {
        if (_levelEnded) return;
        _levelEnded = true;

        var hud = FindFirstObjectByType<SimpleHud>();
        int movesLeft = hud != null ? hud.movesLeft : 0;
        int movesUsed = Mathf.Max(0, _level.MoveLimit - movesLeft);
        int goalProgress = _objectives != null ? _objectives.TotalProgress : (hud != null ? hud.Matches : 0);
        int goalTarget = _objectives != null ? _objectives.TotalTarget : (hud != null ? hud.targetMatches : _level.GoalMatches);
        bool noInvalidSwaps = _invalidSwapCount == 0;
        bool noHintUsed = true;
        bool firstTry = !_usedRetry;

        int finalScore = _score.FinalizeScore(win, movesLeft, noInvalidSwaps, noHintUsed, firstTry);
        Medal medal = _score.ComputeMedal(_level, win, finalScore, movesLeft, noInvalidSwaps, noHintUsed, firstTry);
        int stars = _score.ComputeStars(medal);

        var session = SessionResult.Create(
            _level,
            win,
            finalScore,
            stars,
            medal,
            movesUsed,
            movesLeft,
            _score.BestCascade,
            false,
            _usedRetry,
            firstTry,
            goalProgress,
            goalTarget);

        ResultsData results = _rewards.Compute(session);
        _lastShareCode = session.ShareCode;
        ApplyRewards(results);
        ApplyCompletionSave(session);
        ApplyGardenProgress(results);

        if (hud != null)
        {
            hud.SetScore(finalScore);
        }

        GameTelemetry.Track(win ? "level.complete" : "level.fail", GameTelemetry.Props(
            "level_id", _level.LevelNumber,
            "score", finalScore,
            "stars", stars,
            "medal", medal.ToString(),
            "moves_used", movesUsed,
            "moves_left", movesLeft,
            "best_cascade", _score.BestCascade,
            "goals_remaining", session.GoalsRemaining,
            "seconds_since_session_start", Mathf.RoundToInt(Time.unscaledTime - _sessionStartedAt)
        ));

        GameTelemetry.Track("result.screen_shown", GameTelemetry.Props(
            "level_id", _level.LevelNumber,
            "win", win,
            "score", finalScore,
            "share_code", session.ShareCode
        ));

        if (win)
        {
            BoardJuiceController.Ensure().LevelComplete();
        }
        else
        {
            GameAudioController.Ensure().Play(AudioEvent.SessionFail);
        }

        ResultScreenOverlay.Ensure().Show(session, results, StartNextLevel, ReplayLevel, ShareResult);
    }

    private void StartNextLevel()
    {
        ResultScreenOverlay.Ensure().Hide();
        MissionAccomplishedOverlay.Ensure().Hide();
        _level = _progression.AdvanceLevel();
        LoadLevel(_level, false);
    }

    public void StartDailyGrove()
    {
        if (_daily == null) _daily = new DailyChallengeService();
        ResultScreenOverlay.Ensure().Hide();
        MissionAccomplishedOverlay.Ensure().Hide();
        _level = _daily.GetToday(System.DateTime.UtcNow);
        LoadLevel(_level, false);
        GameTelemetry.Track("daily.started", GameTelemetry.Props(
            "level_id", _level.LevelNumber,
            "seed", _level.Seed
        ));
    }

    public bool LoadSharedChallenge(string shareCode)
    {
        if (_daily == null) _daily = new DailyChallengeService();
        LevelSpec shared;
        if (!_daily.TryParseShareCode(shareCode, out shared))
        {
            return false;
        }

        ResultScreenOverlay.Ensure().Hide();
        MissionAccomplishedOverlay.Ensure().Hide();
        _level = shared;
        LoadLevel(_level, false);
        GameTelemetry.Track("share.challenge_loaded", GameTelemetry.Props(
            "level_id", _level.LevelNumber,
            "seed", _level.Seed
        ));
        return true;
    }

    private void ReplayLevel()
    {
        ResultScreenOverlay.Ensure().Hide();
        MissionAccomplishedOverlay.Ensure().Hide();
        LoadLevel(_level, true);
    }

    private void LoadLevel(LevelSpec level, bool retry)
    {
        _completionPending = false;
        _levelCompleteSent = false;
        _levelEnded = false;
        _usedRetry = retry;
        _invalidSwapCount = 0;
        if (_score == null) _score = new ScoreService();
        _score.Reset();
        ApplyLevelSpec(level);
        if (_objectives == null) _objectives = new ObjectiveTracker();
        _objectives.Initialize(_level);
        _board = new BoardEngine(Width, Height, Seed, _level.BoardRows);
        ApplyLevelMechanics();
        StartAudioForLevel();
        TryInitSceneWiring(true);
    }

    private void ApplyLevelSpec(LevelSpec level)
    {
        _level = level;
        Width = Mathf.Max(3, level.BoardWidth);
        Height = Mathf.Max(3, level.BoardHeight);
        Seed = level.Seed;
    }

    private void StartAudioForLevel()
    {
        var audio = GameAudioController.Ensure();
        audio.PlayAmbience(AmbienceTrack.NatureLightMorning);
        audio.PlayMusic(MusicTrackForLevel(_level.LevelNumber));
    }

    private static MusicTrack MusicTrackForLevel(int levelNumber)
    {
        if (levelNumber <= 3) return MusicTrack.RelaxedMenusEasyLevels;
        if (levelNumber >= 10) return MusicTrack.DeeperFocusLaterLevels;
        return MusicTrack.MainGameplay;
    }

    public void EndLevel(bool win, int secondsPlayed, int movesUsed, int goalsRemaining, bool smartMove)
    {
        if (_levelEnded) return;
        _levelEnded = true;
        ResultsData results = _rewards.Compute(win, secondsPlayed, movesUsed, goalsRemaining, smartMove);
        ApplyRewards(results);

        _feedback.Play(new FeedbackEvent
        {
            Type = FeedbackEventType.Reward,
            Intensity = win ? 2 : 1
        });
    }

    private void ApplyRewards(ResultsData results)
    {
        if (results != null && results.Rewards != null && results.Rewards.Count > 0)
        {
            foreach (var reward in results.Rewards)
            {
                _save.ApplyReward(reward);
                GameTelemetry.Track("reward.granted", GameTelemetry.Props(
                    "currency", reward.Currency,
                    "amount", reward.Amount,
                    "reason", reward.Reason,
                    "layer", reward.Layer.ToString()
                ));
            }

            _save.Save();
            _guard.OnRewardGranted();
        }
    }

    private void ApplyCompletionSave(SessionResult session)
    {
        if (session == null || _save == null) return;

        if (session.Win)
        {
            _save.MarkLevelCompleted(session.LevelId, session.Score);
            if (session.RulesetId == DailyChallengeService.RulesetId)
            {
                _save.MarkDailyCompleted(System.DateTime.UtcNow.ToString("yyyyMMdd"), session.Score);
            }

            _save.Save();
        }
    }

    private void ApplyGardenProgress(ResultsData results)
    {
        if (_garden == null) _garden = GardenState.Load();
        if (_save == null) _save = PlayerSave.Load();

        string gardenText = _garden.ApplyFragments(_save.Fragments);
        if (results != null)
        {
            results.GardenProgress = gardenText;
            results.NextUnlock = NextUnlockForLevel(_level.LevelNumber + 1);
        }
    }

    private static string NextUnlockForLevel(int nextLevel)
    {
        switch (nextLevel)
        {
            case 3: return "Sunbeam";
            case 5: return "Daily Grove";
            case 10: return "Tile skin";
            case 11: return "Moss";
            case 16: return "Bloom Bomb";
            case 18: return "Dew Drop";
            case 21: return "Sun Orb";
            case 26: return "Vine";
            case 30: return "Garden upgrade";
            default: return string.Empty;
        }
    }

    private void ShareResult()
    {
        string shareCode = _lastShareCode;
        if (string.IsNullOrEmpty(shareCode))
        {
            shareCode = "BUX-main_path_v1-L" + _level.LevelNumber + "-S" + Seed + "-V1";
        }

        GUIUtility.systemCopyBuffer = "Spill samme BUXPuzzle: " + shareCode;
        GameTelemetry.Track("share.completed_or_copied", GameTelemetry.Props(
            "level_id", _level.LevelNumber,
            "seed", Seed,
            "share_code", shareCode
        ));
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
        _board = new BoardEngine(Width, Height, Seed, _level.BoardRows);
        ApplyLevelMechanics();
        _completionPending = false;
        _levelCompleteSent = false;
        _levelEnded = false;
        _invalidSwapCount = 0;
        if (_score == null) _score = new ScoreService();
        _score.Reset();
        if (_objectives == null) _objectives = new ObjectiveTracker();
        _objectives.Initialize(_level);

        Debug.Log($"[GameRoot] Runtime seed configured. seed={seed} level={(levelIndex.HasValue ? levelIndex.Value.ToString() : "n/a")}");

        TryInitSceneWiring(true);
    }

    private void ApplyLevelMechanics()
    {
        if (_board == null) return;

        if (_level.NewMechanic == LevelMechanic.Moss)
        {
            _board.SeedBlockers(TileState.Frozen, Mathf.Clamp(_level.ObjectiveTarget, 4, 16));
        }
        else if (_level.NewMechanic == LevelMechanic.Vine)
        {
            _board.SeedBlockers(TileState.Locked, Mathf.Clamp(_level.ObjectiveTarget, 4, 16));
        }
    }
}
