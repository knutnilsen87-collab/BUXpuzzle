using System.Text;
using UnityEngine;

namespace Game.Levels
{
    public readonly struct LevelValidationResult
    {
        public readonly bool IsValid;
        public readonly string Message;

        public LevelValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }
    }

    public static class LevelValidator
    {
        public static LevelValidationResult Validate(LevelSpecV2 spec)
        {
            if (spec == null) return Fail("LevelSpecV2 is null.");

            var errors = new StringBuilder();
            if (spec.LevelId <= 0) errors.Append("levelId must be positive. ");
            if (spec.Width < 3) errors.Append("width must be at least 3. ");
            if (spec.Height < 3) errors.Append("height must be at least 3. ");
            if (spec.MoveLimit <= 0) errors.Append("moveLimit must be positive. ");
            if (spec.Objectives == null || spec.Objectives.Length == 0) errors.Append("at least one objective is required. ");
            if (spec.Seed == 0) spec.Seed = DeterministicEndlessLevelGenerator.SeedForLevel(Mathf.Max(1, spec.LevelId));

            if (spec.Objectives != null)
            {
                foreach (var objective in spec.Objectives)
                {
                    if (objective == null || objective.Target <= 0)
                    {
                        errors.Append("objective target must be positive. ");
                    }
                }
            }

            ValidateRows(spec, errors);

            if (errors.Length > 0)
            {
                return Fail(errors.ToString().Trim());
            }

            try
            {
                var board = new Game.Core.BoardEngine(spec.Width, spec.Height, spec.Seed, spec.BoardRows);
                if (!board.HasAnyValidMove())
                {
                    return Fail("generated board has no legal move.");
                }
            }
            catch (System.Exception ex)
            {
                return Fail("board generation failed: " + ex.Message);
            }

            return new LevelValidationResult(true, "ok");
        }

        private static void ValidateRows(LevelSpecV2 spec, StringBuilder errors)
        {
            if (spec.BoardRows == null || spec.BoardRows.Length == 0) return;
            if (spec.BoardRows.Length != spec.Height)
            {
                errors.Append("boardRows height mismatch. ");
                return;
            }

            for (int y = 0; y < spec.BoardRows.Length; y++)
            {
                string row = spec.BoardRows[y] ?? string.Empty;
                if (row.Length != spec.Width)
                {
                    errors.Append("boardRows width mismatch at row " + y + ". ");
                    continue;
                }

                for (int x = 0; x < row.Length; x++)
                {
                    char c = row[x];
                    if (c != '.' && c != '#' && c != 'M' && c != 'V' && c != 'P' && c != 'I' && c != 'D' && c != 'S' && c != 'O')
                    {
                        errors.Append("unknown board row symbol '" + c + "'. ");
                    }
                }
            }
        }

        private static LevelValidationResult Fail(string message)
        {
            return new LevelValidationResult(false, message);
        }
    }
}
