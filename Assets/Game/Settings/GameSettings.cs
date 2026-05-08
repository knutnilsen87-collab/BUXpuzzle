using UnityEngine;

namespace Game.Settings
{
    public static class GameSettings
    {
        private const string SfxKey = "bux.settings.sfx";
        private const string MusicKey = "bux.settings.music";
        private const string HapticsKey = "bux.settings.haptics";

        public static bool SfxEnabled
        {
            get => PlayerPrefs.GetInt(SfxKey, 1) == 1;
            set => SetBool(SfxKey, value);
        }

        public static bool MusicEnabled
        {
            get => PlayerPrefs.GetInt(MusicKey, 1) == 1;
            set => SetBool(MusicKey, value);
        }

        public static bool HapticsEnabled
        {
            get => PlayerPrefs.GetInt(HapticsKey, 1) == 1;
            set => SetBool(HapticsKey, value);
        }

        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
