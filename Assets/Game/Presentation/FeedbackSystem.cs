using UnityEngine;
using BUXPuzzle.Presentation.Audio;

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
                FBL_PresentationAudioRouter.Ensure().PlayEvent(EventName(evt.Type));
            }

            // HAPTICS (mobile later)
            if (HapticsEnabled)
            {
                // later: Handheld.Vibrate() or native haptics
            }
        }

        private static string EventName(FeedbackEventType type)
        {
            switch (type)
            {
                case FeedbackEventType.Match: return "match";
                case FeedbackEventType.Cascade: return "cascade";
                case FeedbackEventType.Special: return "combo";
                case FeedbackEventType.SmartMove: return "combo";
                case FeedbackEventType.FailSoft: return "invalid_swap";
                case FeedbackEventType.Reward: return "combo";
                default: return "settle";
            }
        }
    }
}
