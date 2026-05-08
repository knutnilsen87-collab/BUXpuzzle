using System.Collections.Generic;

namespace Game.Telemetry
{
    public interface ITelemetryClient
    {
        void Track(string eventName, Dictionary<string, object> props = null);
    }

    // MVP default: no-op client (swap later for Unity Analytics / custom pipe)
    public sealed class NullTelemetryClient : ITelemetryClient
    {
        public void Track(string eventName, Dictionary<string, object> props = null) { }
    }
}
