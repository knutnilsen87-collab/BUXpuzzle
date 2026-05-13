using Game.Core;
using Game.Levels;

namespace Game.Progression
{
    public sealed class ObjectiveTracker
    {
        public LevelObjectiveType Type { get; private set; }
        public int Target { get; private set; }
        public int Progress { get; private set; }
        public int TotalTarget
        {
            get
            {
                if (_states == null || _states.Length == 0) return Target;
                int total = 0;
                for (int i = 0; i < _states.Length; i++) total += _states[i].Target;
                return total;
            }
        }

        public int TotalProgress
        {
            get
            {
                if (_states == null || _states.Length == 0) return Progress;
                int total = 0;
                for (int i = 0; i < _states.Length; i++) total += UnityEngine.Mathf.Min(_states[i].Progress, _states[i].Target);
                return total;
            }
        }

        public string Label { get; private set; }
        public bool IsComplete
        {
            get
            {
                if (_states == null || _states.Length == 0) return Progress >= Target;
                for (int i = 0; i < _states.Length; i++)
                {
                    if (_states[i].Progress < _states[i].Target) return false;
                }

                return true;
            }
        }

        private LevelSpec _level;
        private ObjectiveRuntime[] _states;

        public void Initialize(LevelSpec level)
        {
            _level = level;
            var specs = level.Objectives != null && level.Objectives.Length > 0
                ? level.Objectives
                : new[] { new ObjectiveSpec { Type = level.ObjectiveType, Target = level.ObjectiveTarget > 0 ? level.ObjectiveTarget : level.GoalMatches, TargetTileType = level.TargetTileType } };

            _states = new ObjectiveRuntime[specs.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                _states[i] = new ObjectiveRuntime
                {
                    Type = specs[i].Type,
                    Target = UnityEngine.Mathf.Max(1, specs[i].Target),
                    TargetTileType = specs[i].TargetTileType,
                    Progress = 0
                };
            }

            Type = _states[0].Type;
            Target = _states[0].Target;
            Progress = 0;
            Label = BuildLabel(level);
        }

        public void Apply(ResolveTrace trace, int score, int movesUsed)
        {
            if (_states == null || _states.Length == 0) return;

            for (int i = 0; i < _states.Length; i++)
            {
                if (_states[i].Progress >= _states[i].Target) continue;
                _states[i].Progress = ApplyToState(_states[i], trace, score, movesUsed);
                if (_states[i].Progress > _states[i].Target)
                {
                    _states[i].Progress = _states[i].Target;
                }
            }

            Progress = _states[0].Progress;
            Target = _states[0].Target;
            Type = _states[0].Type;
        }

        private int ApplyToState(ObjectiveRuntime state, ResolveTrace trace, int score, int movesUsed)
        {
            int progress = state.Progress;
            switch (state.Type)
            {
                case LevelObjectiveType.MakeMatches:
                    if (trace != null && trace.Summary.anyCleared) progress += 1;
                    break;
                case LevelObjectiveType.ClearTilesOfType:
                    progress += CountCleared(trace);
                    break;
                case LevelObjectiveType.ReachScore:
                    progress = score;
                    break;
                case LevelObjectiveType.ClearBlockers:
                    progress += CountBlockersCleared(trace);
                    break;
                case LevelObjectiveType.DropObjectsToExit:
                    progress += CountDropObjects(trace);
                    break;
                case LevelObjectiveType.CreateSpecials:
                    progress += CountSpecialCreates(trace);
                    break;
                case LevelObjectiveType.TriggerCascades:
                    if (trace != null && trace.IterationCount >= 2) progress += trace.IterationCount - 1;
                    break;
                case LevelObjectiveType.FinishUnderPar:
                    progress = movesUsed <= _level.ParMoves ? 1 : 0;
                    break;
            }

            return progress;
        }

        public int GoalsRemaining()
        {
            if (_states == null || _states.Length == 0) return UnityEngine.Mathf.Max(0, Target - Progress);
            int remaining = 0;
            for (int i = 0; i < _states.Length; i++)
            {
                remaining += UnityEngine.Mathf.Max(0, _states[i].Target - _states[i].Progress);
            }

            return remaining;
        }

        private struct ObjectiveRuntime
        {
            public LevelObjectiveType Type;
            public int Target;
            public int TargetTileType;
            public int Progress;
        }

        private static int CountCleared(ResolveTrace trace)
        {
            return trace != null ? UnityEngine.Mathf.Max(0, trace.Summary.clearedTiles) : 0;
        }

        private static int EstimateSpecialCreates(ResolveTrace trace)
        {
            if (trace == null) return 0;
            int count = 0;
            if (trace.Summary.clearedTiles >= 4) count++;
            if (trace.Summary.clearedTiles >= 6) count++;
            if (trace.IterationCount >= 3) count++;
            return count;
        }

        private static int CountSpecialCreates(ResolveTrace trace)
        {
            if (trace == null) return 0;
            if (trace.SpecialsCreated != null && trace.SpecialsCreated.Count > 0)
            {
                return trace.SpecialsCreated.Count;
            }

            return EstimateSpecialCreates(trace);
        }

        private static int CountBlockersCleared(ResolveTrace trace)
        {
            if (trace == null) return 0;
            if (trace.BlockersCleared != null && trace.BlockersCleared.Count > 0)
            {
                return trace.BlockersCleared.Count;
            }

            int cleared = trace.Summary.clearedTiles;
            int cascades = UnityEngine.Mathf.Max(0, trace.IterationCount - 1);
            return UnityEngine.Mathf.Max(0, cleared / 3 + cascades);
        }

        private static int CountDropObjects(ResolveTrace trace)
        {
            if (trace == null) return 0;
            if (trace.DropObjectsCollected != null && trace.DropObjectsCollected.Count > 0)
            {
                return trace.DropObjectsCollected.Count;
            }

            if (trace.Steps == null) return 0;
            int progress = 0;
            foreach (var step in trace.Steps)
            {
                if (step.Drops != null && step.Drops.Count >= 6)
                {
                    progress++;
                }
            }

            return progress;
        }

        private static string BuildLabel(LevelSpec level)
        {
            switch (level.ObjectiveType)
            {
                case LevelObjectiveType.MakeMatches: return "Matcher";
                case LevelObjectiveType.ClearTilesOfType: return "Samle brikker";
                case LevelObjectiveType.ReachScore: return "Score";
                case LevelObjectiveType.ClearBlockers: return level.NewMechanic == LevelMechanic.Vine ? "Rydd Vine" : "Rydd Moss";
                case LevelObjectiveType.DropObjectsToExit: return "Slipp Dew";
                case LevelObjectiveType.CreateSpecials: return SpecialName(level.NewMechanic);
                case LevelObjectiveType.TriggerCascades: return "Cascades";
                case LevelObjectiveType.FinishUnderPar: return "Under par";
                default: return "Mål";
            }
        }

        private static string SpecialName(LevelMechanic mechanic)
        {
            switch (mechanic)
            {
                case LevelMechanic.BloomBomb: return "Lag Bloom Bomb";
                case LevelMechanic.SunOrb: return "Lag Sun Orb";
                default: return "Lag Sunbeam";
            }
        }
    }
}
