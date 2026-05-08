using System;
using System.Collections.Generic;

namespace Game.Presentation
{
    public sealed class FeedbackThrottler
    {
        private readonly Dictionary<FeedbackEventType, float> _lastTime = new();
        private readonly float _cooldownSeconds;

        public FeedbackThrottler(float cooldownSeconds = 0.05f)
        {
            _cooldownSeconds = cooldownSeconds;
        }

        public bool CanPlay(FeedbackEventType type, float now)
        {
            if (!_lastTime.TryGetValue(type, out var t))
            {
                _lastTime[type] = now;
                return true;
            }

            if (now - t >= _cooldownSeconds)
            {
                _lastTime[type] = now;
                return true;
            }
            return false;
        }
    }
}
