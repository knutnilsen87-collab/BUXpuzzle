using UnityEngine;
using Game.Telemetry;

namespace Game.Onboarding
{
    public static class TutorialStateStore
    {
        private const string CompletedKey = "bux.first_move_tutorial.completed";
        private const string ForceKey = "bux.first_move_tutorial.force";

        public static bool ForceShow
        {
            get => PlayerPrefs.GetInt(ForceKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(ForceKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool IsCompleted()
        {
            return !ForceShow && PlayerPrefs.GetInt(CompletedKey, 0) == 1;
        }

        public static void MarkCompleted()
        {
            PlayerPrefs.SetInt(CompletedKey, 1);
            PlayerPrefs.SetInt(ForceKey, 0);
            PlayerPrefs.Save();
        }

        public static void ResetForQa()
        {
            PlayerPrefs.DeleteKey(CompletedKey);
            PlayerPrefs.SetInt(ForceKey, 1);
            PlayerPrefs.Save();
            GameTelemetry.Track("tutorial.reset");
        }
    }
}
