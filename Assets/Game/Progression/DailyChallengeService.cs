using System;
using Game.Levels;

namespace Game.Progression
{
    public sealed class DailyChallengeService
    {
        public const string RulesetId = "daily_grove_v1";
        public const int EngineVersion = 1;
        public const int ScoreFormulaVersion = 1;

        public LevelSpec GetToday(DateTime dateUtc)
        {
            return LevelManager.GetDaily(dateUtc.Date);
        }

        public string BuildShareCode(SessionResult result)
        {
            if (result == null) return string.Empty;
            string ruleset = string.IsNullOrEmpty(result.RulesetId) ? RulesetId : result.RulesetId;
            return "BUX-" + ruleset + "-E" + EngineVersion + "-F" + ScoreFormulaVersion + "-L" + result.LevelId + "-S" + result.Seed;
        }

        public bool TryParseShareCode(string code, out LevelSpec level)
        {
            level = default;
            if (string.IsNullOrEmpty(code)) return false;

            int levelIndex = code.IndexOf("-L", StringComparison.OrdinalIgnoreCase);
            int seedIndex = code.IndexOf("-S", StringComparison.OrdinalIgnoreCase);
            if (levelIndex < 0 || seedIndex < 0 || seedIndex <= levelIndex) return false;

            string levelText = code.Substring(levelIndex + 2, seedIndex - levelIndex - 2);
            string seedText = code.Substring(seedIndex + 2);
            int levelId;
            int seed;
            if (!int.TryParse(levelText, out levelId) || !int.TryParse(seedText, out seed)) return false;

            level = LevelManager.GetLevel(levelId);
            level.Seed = seed;
            level.DisplayName = "Shared Grove";
            return true;
        }
    }
}
