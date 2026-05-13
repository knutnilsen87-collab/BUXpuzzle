using Game.Core;
using Game.Levels;

namespace Game.Progression
{
    public sealed class ScoreService
    {
        public int Score { get; private set; }
        public int BestCascade { get; private set; }
        public bool SmartMoveTriggered { get; private set; }

        public void Reset()
        {
            Score = 0;
            BestCascade = 0;
            SmartMoveTriggered = false;
        }

        public void ApplySwapResult(ResolveTrace trace)
        {
            if (trace == null)
            {
                return;
            }

            int iterations = trace.IterationCount;
            if (iterations > BestCascade)
            {
                BestCascade = iterations;
            }

            if (iterations >= 2 || trace.Summary.clearedTiles >= 4)
            {
                SmartMoveTriggered = true;
            }

            if (trace.Summary.clearedTiles >= 4) Score += 100;
            if (trace.Summary.clearedTiles >= 5) Score += 250;
            if (trace.Summary.clearedTiles >= 7) Score += 300;

            if (trace.Steps == null || trace.Steps.Count == 0)
            {
                Score += trace.Summary.clearedTiles * 10;
                return;
            }

            for (int i = 0; i < trace.Steps.Count; i++)
            {
                int cleared = trace.Steps[i].Cleared != null ? trace.Steps[i].Cleared.Count : 0;
                Score += UnityEngine.Mathf.RoundToInt(cleared * 10 * CascadeMultiplier(i + 1));
            }
        }

        public int FinalizeScore(bool win, int movesLeft, bool noInvalidSwaps, bool noHintUsed, bool firstTry)
        {
            int finalScore = Score;
            if (win)
            {
                finalScore += UnityEngine.Mathf.Max(0, movesLeft) * 75;
                if (noInvalidSwaps) finalScore += 250;
                if (noHintUsed) finalScore += 250;
                if (firstTry) finalScore += 500;
            }

            return finalScore;
        }

        public Medal ComputeMedal(LevelSpec level, bool win, int finalScore, int movesLeft, bool noInvalidSwaps, bool noHintUsed, bool firstTry)
        {
            if (!win) return Medal.None;
            if (noInvalidSwaps && noHintUsed && firstTry) return Medal.NordicPerfect;

            int silver = UnityEngine.Mathf.Max(600, level.GoalMatches * 45);
            int gold = UnityEngine.Mathf.Max(1000, level.GoalMatches * 70 + level.MoveLimit * 25);

            if (finalScore >= gold && movesLeft > 0) return Medal.GoldenSun;
            if (finalScore >= silver) return Medal.SilverBloom;
            return Medal.BronzeLeaf;
        }

        public int ComputeStars(Medal medal)
        {
            switch (medal)
            {
                case Medal.NordicPerfect:
                case Medal.GoldenSun:
                    return 3;
                case Medal.SilverBloom:
                    return 2;
                case Medal.BronzeLeaf:
                    return 1;
                default:
                    return 0;
            }
        }

        private static float CascadeMultiplier(int cascadeIndex)
        {
            if (cascadeIndex <= 1) return 1f;
            if (cascadeIndex == 2) return 1.5f;
            if (cascadeIndex == 3) return 2f;
            return 3f;
        }
    }
}
