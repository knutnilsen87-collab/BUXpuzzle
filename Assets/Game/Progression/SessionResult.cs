using Game.Levels;

namespace Game.Progression
{
    public enum Medal
    {
        None = 0,
        BronzeLeaf = 1,
        SilverBloom = 2,
        GoldenSun = 3,
        NordicPerfect = 4
    }

    public sealed class SessionResult
    {
        public bool Win;
        public int LevelId;
        public int Seed;
        public string RulesetId;
        public int Score;
        public int Stars;
        public Medal Medal;
        public int MovesUsed;
        public int MovesLeft;
        public int BestCascade;
        public bool UsedHint;
        public bool UsedRetry;
        public bool FirstTry;
        public int GoalProgress;
        public int GoalTarget;
        public int GoalsRemaining;
        public RewardGrant[] Rewards;
        public string ShareCode;

        public static SessionResult Create(
            LevelSpec level,
            bool win,
            int score,
            int stars,
            Medal medal,
            int movesUsed,
            int movesLeft,
            int bestCascade,
            bool usedHint,
            bool usedRetry,
            bool firstTry,
            int goalProgress,
            int goalTarget)
        {
            int remaining = goalTarget - goalProgress;
            if (remaining < 0) remaining = 0;

            var result = new SessionResult
            {
                Win = win,
                LevelId = level.LevelNumber,
                Seed = level.Seed,
                RulesetId = "main_path_v1",
                Score = score,
                Stars = stars,
                Medal = medal,
                MovesUsed = movesUsed,
                MovesLeft = movesLeft,
                BestCascade = bestCascade,
                UsedHint = usedHint,
                UsedRetry = usedRetry,
                FirstTry = firstTry,
                GoalProgress = goalProgress,
                GoalTarget = goalTarget,
                GoalsRemaining = remaining
            };

            result.ShareCode = ShareCodeService.Build(result);
            return result;
        }
    }
}
