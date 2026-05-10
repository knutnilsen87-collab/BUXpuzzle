using Game.Core;
using Game.Levels;

namespace Game.Progression
{
    public sealed class ObjectiveTracker
    {
        public LevelObjectiveType Type { get; private set; }
        public int Target { get; private set; }
        public int Progress { get; private set; }
        public string Label { get; private set; }
        public bool IsComplete => Progress >= Target;

        private LevelSpec _level;

        public void Initialize(LevelSpec level)
        {
            _level = level;
            Type = level.ObjectiveType;
            Target = UnityEngine.Mathf.Max(1, level.ObjectiveTarget > 0 ? level.ObjectiveTarget : level.GoalMatches);
            Progress = 0;
            Label = BuildLabel(level);
        }

        public void Apply(ResolveTrace trace, int score, int movesUsed)
        {
            if (IsComplete) return;

            switch (Type)
            {
                case LevelObjectiveType.MakeMatches:
                    if (trace != null && trace.Summary.anyCleared) Progress += 1;
                    break;
                case LevelObjectiveType.ClearTilesOfType:
                    Progress += CountCleared(trace);
                    break;
                case LevelObjectiveType.ReachScore:
                    Progress = score;
                    break;
                case LevelObjectiveType.ClearBlockers:
                    Progress += EstimateBlockerClears(trace);
                    break;
                case LevelObjectiveType.DropObjectsToExit:
                    Progress += EstimateDropProgress(trace);
                    break;
                case LevelObjectiveType.CreateSpecials:
                    Progress += EstimateSpecialCreates(trace);
                    break;
                case LevelObjectiveType.TriggerCascades:
                    if (trace != null && trace.IterationCount >= 2) Progress += trace.IterationCount - 1;
                    break;
                case LevelObjectiveType.FinishUnderPar:
                    Progress = movesUsed <= _level.ParMoves ? 1 : 0;
                    break;
            }

            if (Progress > Target)
            {
                Progress = Target;
            }
        }

        public int GoalsRemaining()
        {
            return UnityEngine.Mathf.Max(0, Target - Progress);
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

        private static int EstimateBlockerClears(ResolveTrace trace)
        {
            if (trace == null) return 0;
            int cleared = trace.Summary.clearedTiles;
            int cascades = UnityEngine.Mathf.Max(0, trace.IterationCount - 1);
            return UnityEngine.Mathf.Max(0, cleared / 3 + cascades);
        }

        private static int EstimateDropProgress(ResolveTrace trace)
        {
            if (trace == null || trace.Steps == null) return 0;
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
