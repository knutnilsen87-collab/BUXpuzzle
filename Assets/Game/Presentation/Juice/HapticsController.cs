using Game.Settings;
using UnityEngine;

namespace Game.Presentation.Juice
{
    public static class HapticsController
    {
        private static float _lastAt = -10f;

        public static void Light()
        {
            VibrateThrottled(0.18f);
        }

        public static void Warning()
        {
            VibrateThrottled(0.28f);
        }

        public static void Success()
        {
            VibrateThrottled(0.45f);
        }

        private static void VibrateThrottled(float minInterval)
        {
            if (!GameSettings.HapticsEnabled) return;
            if (Time.unscaledTime - _lastAt < minInterval) return;
            _lastAt = Time.unscaledTime;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
