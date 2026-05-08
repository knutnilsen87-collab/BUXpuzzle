using System.Collections.Generic;

namespace Game.Telemetry
{
    // Guard: empty_session_detected should never be true
    public sealed class SessionGuards
    {
        private int _rewardGrantedCount;

        public void OnRewardGranted() => _rewardGrantedCount++;

        public void OnSessionEnd(ITelemetryClient telemetry)
        {
            var empty = _rewardGrantedCount == 0;
            telemetry.Track("empty_session_detected", new Dictionary<string, object> { { "true", empty } });
        }
    }
}
