using UnityEngine;

namespace Game.Presentation
{
    public sealed class FeedbackSystem : MonoBehaviour
    {
        public bool SoundEnabled = true;
        public bool HapticsEnabled = true;
        public bool ReducedMotion = false;

        private FeedbackThrottler _throttler;

        void Awake()
        {
            _throttler = new FeedbackThrottler(0.05f);
        }

        public void Play(FeedbackEvent evt)
        {
            var now = Time.unscaledTime;
            if (!_throttler.CanPlay(evt.Type, now))
                return;

            // VISUAL (placeholder)
            if (!ReducedMotion)
            {
                // later: particles / shake
            }

            // AUDIO (placeholder)
            if (SoundEnabled)
            {
                // later: AudioSource.PlayOneShot(...)
            }

            // HAPTICS (mobile later)
            if (HapticsEnabled)
            {
                // later: Handheld.Vibrate() or native haptics
            }
        }
    }
}
