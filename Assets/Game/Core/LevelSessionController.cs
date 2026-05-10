using Game.Core;
using Game.Levels;
using Game.Progression;

namespace Game.Core
{
    public sealed class LevelSessionController
    {
        private readonly ScoreService _score = new ScoreService();
        private readonly ObjectiveTracker _objectives = new ObjectiveTracker();
        private LevelSpec _level;
        private bool _ended;
        private bool _usedRetry;
        private int _invalidSwaps;

        public int MovesUsed { get; private set; }
        public int MovesLeft { get; private set; }
        public bool IsEnded => _ended;
        public int Score => _score.Score;
        public ObjectiveTracker Objectives => _objectives;

        public void StartLevel(LevelSpec level, bool retry)
        {
            _level = level;
            _usedRetry = retry;
            _ended = false;
            _invalidSwaps = 0;
            MovesUsed = 0;
            MovesLeft = UnityEngine.Mathf.Max(0, level.MoveLimit);
            _score.Reset();
            _objectives.Initialize(level);
        }

        public void OnRejectedSwap()
        {
            if (!_ended) _invalidSwaps++;
        }

        public SessionResult OnResolvedSwap(ResolveTrace trace)
        {
            if (_ended || trace == null || !trace.Summary.anyCleared)
            {
                return null;
            }

            MovesUsed++;
            MovesLeft = UnityEngine.Mathf.Max(0, MovesLeft - 1);
            _score.ApplySwapResult(trace);
            int liveScore = _score.FinalizeScore(false, MovesLeft, _invalidSwaps == 0, true, !_usedRetry);
            _objectives.Apply(trace, liveScore, MovesUsed);

            if (_objectives.IsComplete)
            {
                return End(true);
            }

            if (MovesLeft <= 0)
            {
                return End(false);
            }

            return null;
        }

        public SessionResult End(bool win)
        {
            if (_ended) return null;
            _ended = true;
            bool noInvalidSwaps = _invalidSwaps == 0;
            int finalScore = _score.FinalizeScore(win, MovesLeft, noInvalidSwaps, true, !_usedRetry);
            var medal = _score.ComputeMedal(_level, win, finalScore, MovesLeft, noInvalidSwaps, true, !_usedRetry);
            return SessionResult.Create(
                _level,
                win,
                finalScore,
                _score.ComputeStars(medal),
                medal,
                MovesUsed,
                MovesLeft,
                _score.BestCascade,
                false,
                _usedRetry,
                !_usedRetry,
                _objectives.TotalProgress,
                _objectives.TotalTarget);
        }
    }
}
