using System;

namespace Game.Levels
{
    public static class LevelSpecV2Adapter
    {
        public static LevelSpecV2 FromLegacy(LevelSpec legacy)
        {
            int target = legacy.ObjectiveTarget > 0 ? legacy.ObjectiveTarget : legacy.GoalMatches;
            if (target <= 0) target = 1;

            return new LevelSpecV2
            {
                LevelId = Math.Max(1, legacy.LevelNumber),
                WorldId = legacy.WorldId <= 0 ? 1 : legacy.WorldId,
                DisplayName = string.IsNullOrEmpty(legacy.DisplayName) ? "Level " + legacy.LevelNumber : legacy.DisplayName,
                Width = legacy.BoardWidth <= 0 ? 8 : legacy.BoardWidth,
                Height = legacy.BoardHeight <= 0 ? 8 : legacy.BoardHeight,
                MoveLimit = Math.Max(1, legacy.MoveLimit),
                ParMoves = legacy.ParMoves > 0 ? legacy.ParMoves : Math.Max(1, legacy.MoveLimit - 4),
                Seed = legacy.Seed,
                RulesetId = string.IsNullOrEmpty(legacy.RulesetId) ? "main_path_v1" : legacy.RulesetId,
                AllowLose = legacy.AllowLose,
                ForceWinBias = legacy.ForceWinBias,
                DesignerNote = legacy.DesignerNote,
                NewMechanic = legacy.NewMechanic,
                BoardRows = legacy.BoardRows ?? Array.Empty<string>(),
                Objectives = new[]
                {
                    new ObjectiveSpec
                    {
                        Type = legacy.ObjectiveType,
                        Target = target,
                        TargetTileType = legacy.TargetTileType
                    }
                }
            };
        }

        public static LevelSpec ToLegacy(LevelSpecV2 spec)
        {
            if (spec == null)
            {
                return LevelManager.GetLevel(1);
            }

            var objective = FirstObjective(spec);
            var objectives = spec.Objectives != null && spec.Objectives.Length > 0
                ? spec.Objectives
                : new[] { new ObjectiveSpec { Type = LevelObjectiveType.MakeMatches, Target = 1, TargetTileType = -1 } };
            int target = objective != null ? Math.Max(1, objective.Target) : 1;
            var level = new LevelSpec
            {
                LevelNumber = Math.Max(1, spec.LevelId),
                WorldId = spec.WorldId <= 0 ? 1 : spec.WorldId,
                DisplayName = string.IsNullOrEmpty(spec.DisplayName) ? "Level " + spec.LevelId : spec.DisplayName,
                GoalMatches = target,
                ObjectiveTarget = target,
                Objectives = objectives,
                ObjectiveType = objective != null ? objective.Type : LevelObjectiveType.MakeMatches,
                TargetTileType = objective != null ? objective.TargetTileType : -1,
                MoveLimit = Math.Max(1, spec.MoveLimit),
                ParMoves = spec.ParMoves > 0 ? spec.ParMoves : Math.Max(1, spec.MoveLimit - 4),
                BoardWidth = Math.Max(3, spec.Width),
                BoardHeight = Math.Max(3, spec.Height),
                Seed = spec.Seed != 0 ? spec.Seed : DeterministicEndlessLevelGenerator.SeedForLevel(Math.Max(1, spec.LevelId)),
                RulesetId = string.IsNullOrEmpty(spec.RulesetId) ? "main_path_v1" : spec.RulesetId,
                NewMechanic = spec.NewMechanic,
                AllowLose = spec.AllowLose,
                ForceWinBias = spec.ForceWinBias,
                DesignerNote = spec.DesignerNote,
                BoardRows = spec.BoardRows ?? Array.Empty<string>()
            };

            return level;
        }

        private static ObjectiveSpec FirstObjective(LevelSpecV2 spec)
        {
            return spec.Objectives != null && spec.Objectives.Length > 0 ? spec.Objectives[0] : null;
        }
    }
}
