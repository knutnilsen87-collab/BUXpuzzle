using UnityEngine;

namespace Game.Settings
{
    public static class GameSettings
    {
        private const string SfxKey = "bux.settings.sfx";
        private const string MusicKey = "bux.settings.music";
        private const string AmbienceKey = "bux.settings.ambience";
        private const string HapticsKey = "bux.settings.haptics";
        private const string SfxVolumeKey = "bux.settings.sfx.volume";
        private const string MusicVolumeKey = "bux.settings.music.volume";
        private const string AmbienceVolumeKey = "bux.settings.ambience.volume";

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

        public static bool AmbienceEnabled
        {
            get => PlayerPrefs.GetInt(AmbienceKey, 1) == 1;
            set => SetBool(AmbienceKey, value);
        }

        public static bool HapticsEnabled
        {
            get => PlayerPrefs.GetInt(HapticsKey, 1) == 1;
            set => SetBool(HapticsKey, value);
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            set => SetFloat(SfxVolumeKey, value);
        }

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            set => SetFloat(MusicVolumeKey, value);
        }

        public static float AmbienceVolume
        {
            get => PlayerPrefs.GetFloat(AmbienceVolumeKey, 1f);
            set => SetFloat(AmbienceVolumeKey, value);
        }

        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        private static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }
}
