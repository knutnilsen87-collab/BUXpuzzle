using System;
using System.Collections.Generic;

namespace Game.Telemetry
{
    // P0 guards for BUX integrity
    public sealed class SessionTelemetryGuard
    {
        private readonly ITelemetry _telemetry;
        private readonly DateTime _sessionStartUtc;
        private bool _rewardGranted;
        private bool _firstRewardSent;

        public SessionTelemetryGuard(ITelemetry telemetry)
        {
            _telemetry = telemetry ?? new NullTelemetry();
            _sessionStartUtc = DateTime.UtcNow;

            _telemetry.Track("session_start", new Dictionary<string, object>
            {
                { "utc", _sessionStartUtc.ToString("o") }
            });
        }

        // Call when ANY reward is granted (any layer)
        public void OnRewardGranted()
        {
            _rewardGranted = true;

            if (_firstRewardSent) return;
            _firstRewardSent = true;

            var seconds = (DateTime.UtcNow - _sessionStartUtc).TotalSeconds;
            _telemetry.Track("first_reward_time", new Dictionary<string, object>
            {
                { "seconds", seconds }
            });
        }

        // Call on app background / quit / hard exit
        public void OnSessionEnd()
        {
            var duration = (DateTime.UtcNow - _sessionStartUtc).TotalSeconds;

            // P0: Empty session
            _telemetry.Track("empty_session_detected", new Dictionary<string, object>
            {
                { "true", !_rewardGranted }
            });

            // Rage quit heuristic (< 120 sec)
            if (duration < 120)
            {
                _telemetry.Track("rage_quit", new Dictionary<string, object>
                {
                    { "timeFromStartSeconds", duration }
                });
            }

            _telemetry.Track("session_end", new Dictionary<string, object>
            {
                { "durationSeconds", duration }
            });
        }
    }
}
