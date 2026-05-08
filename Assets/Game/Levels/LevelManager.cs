namespace Game.Levels
{
    public static class LevelManager
    {
        public static LevelSpec GetLevel(int levelNumber)
        {
            int level = UnityEngine.Mathf.Max(1, levelNumber);
            int goal = level == 1 ? 10 : (level == 2 ? 12 : 15 + (level - 3) * 2);
            int moves = level == 1 ? 13 : (level == 2 ? 15 : 18 + (level - 3));

            return new LevelSpec
            {
                LevelNumber = level,
                GoalMatches = goal,
                MoveLimit = moves,
                BoardWidth = 8,
                BoardHeight = 8,
                Seed = DeterministicEndlessLevelGenerator.SeedForLevel(level)
            };
        }
    }
}
