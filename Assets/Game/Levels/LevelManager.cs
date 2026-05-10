namespace Game.Levels
{
    public static class LevelManager
    {
        public static LevelSpec GetLevel(int levelNumber)
        {
            int level = UnityEngine.Mathf.Max(1, levelNumber);
            var spec = GetRoadmapLevel(level);
            spec.LevelNumber = level;
            spec.DisplayName = string.IsNullOrEmpty(spec.DisplayName) ? "Level " + level : spec.DisplayName;
            spec.GoalMatches = UnityEngine.Mathf.Max(1, spec.ObjectiveTarget);
            spec.BoardWidth = spec.BoardWidth <= 0 ? 8 : spec.BoardWidth;
            spec.BoardHeight = spec.BoardHeight <= 0 ? 8 : spec.BoardHeight;
            spec.MoveLimit = UnityEngine.Mathf.Max(5, spec.MoveLimit);
            spec.ParMoves = spec.ParMoves <= 0 ? UnityEngine.Mathf.Max(1, spec.MoveLimit - 4) : spec.ParMoves;
            spec.Seed = DeterministicEndlessLevelGenerator.SeedForLevel(level);
            return spec;
        }

        public static LevelSpec GetDaily(System.DateTime dateUtc)
        {
            int dailyIndex = dateUtc.Year * 10000 + dateUtc.Month * 100 + dateUtc.Day;
            var spec = GetLevel(5 + dailyIndex % 25);
            spec.LevelNumber = dailyIndex;
            spec.DisplayName = "Daily Grove " + dateUtc.ToString("yyyy-MM-dd");
            spec.Seed = DeterministicEndlessLevelGenerator.SeedForLevel(dailyIndex);
            spec.IsDailyEligible = true;
            spec.RewardLabel = "Daily badge";
            return spec;
        }

        private static LevelSpec GetRoadmapLevel(int level)
        {
            switch (level)
            {
                case 1: return Spec("Swap intro", LevelObjectiveType.MakeMatches, 5, 14, LevelMechanic.None, "First Leaf", "Teach basic swap and valid match");
                case 2: return Spec("Cascade intro", LevelObjectiveType.TriggerCascades, 1, 15, LevelMechanic.None, "XP + fragment", "Seed board for guaranteed cascade opportunity");
                case 3: return Spec("Match-4", LevelObjectiveType.CreateSpecials, 1, 16, LevelMechanic.Sunbeam, "Unlock Sunbeam", "First special tile creation");
                case 4: return Spec("Score and moves", LevelObjectiveType.ReachScore, 900, 18, LevelMechanic.None, "Medal intro", "Introduce score and moves left bonus");
                case 5: return Spec("Mini milestone", LevelObjectiveType.MakeMatches, 10, 18, LevelMechanic.None, "Unlock Daily Grove", "First reason to return tomorrow", true);
                case 6: return Spec("Basic collection", LevelObjectiveType.ClearTilesOfType, 20, 18, LevelMechanic.None, "Fragment", "Introduce tile-specific collect goal", true, 0);
                case 7: return Spec("Special activation", LevelObjectiveType.CreateSpecials, 1, 18, LevelMechanic.Sunbeam, "Token", "Player activates first line clear", true);
                case 8: return Spec("Harder board", LevelObjectiveType.MakeMatches, 9, 18, LevelMechanic.None, "Badge", "Slight difficulty increase", true);
                case 9: return Spec("Replay value", LevelObjectiveType.ReachScore, 1600, 19, LevelMechanic.None, "Replay prompt", "Encourage mastery replay", true);
                case 10: return Spec("Milestone", LevelObjectiveType.MakeMatches, 12, 20, LevelMechanic.None, "Tile skin", "First visible cosmetic unlock", true);
                case 11: return Spec("Moss intro", LevelObjectiveType.ClearBlockers, 8, 20, LevelMechanic.Moss, "Unlock Moss", "First blocker", true);
                case 12: return Spec("Moss progression", LevelObjectiveType.ClearBlockers, 12, 21, LevelMechanic.Moss, "XP", "Blocker objective repetition", true);
                case 13: return Spec("Moss + Sunbeam", LevelObjectiveType.ClearBlockers, 10, 21, LevelMechanic.Sunbeam, "Fragment", "Teach special/blocker interaction", true);
                case 14: return Spec("Efficiency", LevelObjectiveType.MakeMatches, 12, 20, LevelMechanic.None, "Medal", "Teach par/moves mastery", true);
                case 15: return Spec("Daily reminder", LevelObjectiveType.MakeMatches, 13, 21, LevelMechanic.None, "Daily badge", "Reinforce Daily Grove", true);
                case 16: return Spec("Bloom Bomb intro", LevelObjectiveType.CreateSpecials, 1, 21, LevelMechanic.BloomBomb, "Unlock Bloom Bomb", "Introduce area clear", true);
                case 17: return Spec("Bloom Bomb use", LevelObjectiveType.CreateSpecials, 2, 22, LevelMechanic.BloomBomb, "Token", "Activate area clear", true);
                case 18: return Spec("Drop intro", LevelObjectiveType.DropObjectsToExit, 1, 22, LevelMechanic.DewDrop, "Unlock Dew Drop", "Introduce drop objective", true);
                case 19: return Spec("Drop challenge", LevelObjectiveType.DropObjectsToExit, 2, 23, LevelMechanic.DewDrop, "XP", "Reinforce vertical planning", true);
                case 20: return Spec("Milestone", LevelObjectiveType.ClearBlockers, 14, 24, LevelMechanic.Moss, "Board frame", "First mixed milestone", true);
                case 21: return Spec("Sun Orb intro", LevelObjectiveType.CreateSpecials, 1, 22, LevelMechanic.SunOrb, "Unlock Sun Orb", "Introduce color clear", true);
                case 22: return Spec("Color target", LevelObjectiveType.ClearTilesOfType, 35, 24, LevelMechanic.None, "Fragment", "Tile-type target", true, 1);
                case 23: return Spec("Special challenge", LevelObjectiveType.CreateSpecials, 2, 24, LevelMechanic.Sunbeam, "Mastery badge", "Encourage intentional special creation", true);
                case 24: return Spec("Cascade target", LevelObjectiveType.TriggerCascades, 3, 24, LevelMechanic.None, "XP", "Reward board reading", true);
                case 25: return Spec("First hard level", LevelObjectiveType.MakeMatches, 15, 22, LevelMechanic.None, "Golden Sun chance", "High near-miss potential", true);
                case 26: return Spec("Vine intro", LevelObjectiveType.ClearBlockers, 8, 24, LevelMechanic.Vine, "Unlock Vine", "Second blocker", true);
                case 27: return Spec("Vine + drop", LevelObjectiveType.DropObjectsToExit, 2, 25, LevelMechanic.Vine, "Token", "Combine blocker/drop", true);
                case 28: return Spec("Mixed objective", LevelObjectiveType.ReachScore, 2400, 25, LevelMechanic.Moss, "Fragment", "Two-objective level", true);
                case 29: return Spec("Daily-style board", LevelObjectiveType.MakeMatches, 16, 25, LevelMechanic.None, "Share prompt", "Prepare share loop", true);
                case 30: return Spec("Chapter finale", LevelObjectiveType.CreateSpecials, 3, 26, LevelMechanic.SunOrb, "Garden upgrade", "Strong completion reward", true);
                default:
                    int target = 16 + ((level - 31) % 8);
                    int moves = 24 + ((level - 31) % 6);
                    return Spec("Garden path " + level, LevelObjectiveType.MakeMatches, target, moves, LevelMechanic.None, "Fragment", "Generated post-roadmap level", true);
            }
        }

        private static LevelSpec Spec(
            string name,
            LevelObjectiveType objective,
            int target,
            int moves,
            LevelMechanic mechanic,
            string reward,
            string note,
            bool dailyEligible = false,
            int targetTileType = -1)
        {
            return new LevelSpec
            {
                DisplayName = name,
                ObjectiveType = objective,
                ObjectiveTarget = target,
                MoveLimit = moves,
                ParMoves = UnityEngine.Mathf.Max(1, moves - 4),
                BoardWidth = 8,
                BoardHeight = 8,
                NewMechanic = mechanic,
                IsDailyEligible = dailyEligible,
                TargetTileType = targetTileType,
                RewardLabel = reward,
                DesignerNote = note
            };
        }
    }
}
