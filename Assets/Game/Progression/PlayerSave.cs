using UnityEngine;

namespace Game.Progression
{
    public sealed class PlayerSave
    {
        private const string XpKey = "BUX_Save_XP";
        private const string TokensKey = "BUX_Save_Tokens";
        private const string FragmentsKey = "BUX_Save_Fragments";
        private const string LevelKey = "BUX_Save_CurrentLevel";
        private const string CompletedPrefix = "BUX_Save_LevelCompleted_";
        private const string BestScorePrefix = "BUX_Save_BestScore_";
        private const string DailyPrefix = "BUX_Save_DailyCompleted_";

        public int XP;
        public int Tokens;
        public int Fragments;
        public int CurrentLevel = 1;

        public static PlayerSave Load()
        {
            return new PlayerSave
            {
                XP = Mathf.Max(0, PlayerPrefs.GetInt(XpKey, 0)),
                Tokens = Mathf.Max(0, PlayerPrefs.GetInt(TokensKey, 0)),
                Fragments = Mathf.Max(0, PlayerPrefs.GetInt(FragmentsKey, 0)),
                CurrentLevel = Mathf.Max(1, PlayerPrefs.GetInt(LevelKey, 1))
            };
        }

        public void ApplyReward(RewardGrant reward)
        {
            if (reward.Amount <= 0 || string.IsNullOrWhiteSpace(reward.Currency))
            {
                return;
            }

            switch (reward.Currency.ToLowerInvariant())
            {
                case "xp":
                    XP += reward.Amount;
                    break;
                case "token":
                case "tokens":
                    Tokens += reward.Amount;
                    break;
                case "fragment":
                case "fragments":
                    Fragments += reward.Amount;
                    break;
            }
        }

        public void AdvanceLevel()
        {
            CurrentLevel = Mathf.Max(1, CurrentLevel + 1);
        }

        public void MarkLevelCompleted(int levelId, int score)
        {
            if (levelId <= 0) return;
            PlayerPrefs.SetInt(CompletedPrefix + levelId, 1);
            int best = PlayerPrefs.GetInt(BestScorePrefix + levelId, 0);
            if (score > best)
            {
                PlayerPrefs.SetInt(BestScorePrefix + levelId, score);
            }
        }

        public bool IsLevelCompleted(int levelId)
        {
            return PlayerPrefs.GetInt(CompletedPrefix + levelId, 0) == 1;
        }

        public int BestScoreForLevel(int levelId)
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(BestScorePrefix + levelId, 0));
        }

        public void MarkDailyCompleted(string dailyKey, int score)
        {
            if (string.IsNullOrEmpty(dailyKey)) return;
            PlayerPrefs.SetInt(DailyPrefix + dailyKey, Mathf.Max(1, score));
        }

        public int DailyScore(string dailyKey)
        {
            if (string.IsNullOrEmpty(dailyKey)) return 0;
            return Mathf.Max(0, PlayerPrefs.GetInt(DailyPrefix + dailyKey, 0));
        }

        public void Save()
        {
            PlayerPrefs.SetInt(XpKey, Mathf.Max(0, XP));
            PlayerPrefs.SetInt(TokensKey, Mathf.Max(0, Tokens));
            PlayerPrefs.SetInt(FragmentsKey, Mathf.Max(0, Fragments));
            PlayerPrefs.SetInt(LevelKey, Mathf.Max(1, CurrentLevel));
            PlayerPrefs.Save();
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(XpKey);
            PlayerPrefs.DeleteKey(TokensKey);
            PlayerPrefs.DeleteKey(FragmentsKey);
            PlayerPrefs.DeleteKey(LevelKey);
            PlayerPrefs.Save();
        }
    }
}
