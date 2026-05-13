using System.Collections.Generic;
using Game.Logging;
using UnityEngine;

namespace Game.Telemetry
{
    public static class GameTelemetry
    {
        public static bool Enabled = true;
        private static readonly Dictionary<string, object> Empty = new Dictionary<string, object>();
        private static ITelemetry _client = new NullTelemetry();

        public static void SetClient(ITelemetry client)
        {
            _client = client ?? new NullTelemetry();
        }

        public static void Track(string eventName, Dictionary<string, object> data = null)
        {
            if (!Enabled || string.IsNullOrEmpty(eventName)) return;

            var props = data ?? Empty;
            try
            {
                _client.Track(eventName, props);
                UlfUnityLogger.Info(eventName, eventName, SerializeProps(props));
            }
            catch
            {
                Debug.LogWarning("[GameTelemetry] Failed to track event: " + eventName);
            }
        }

        public static Dictionary<string, object> Props(params object[] values)
        {
            var result = new Dictionary<string, object>();
            if (values == null) return result;

            for (int i = 0; i + 1 < values.Length; i += 2)
            {
                var key = values[i] as string;
                if (string.IsNullOrEmpty(key)) continue;
                result[key] = values[i + 1];
            }

            return result;
        }

        private static string SerializeProps(Dictionary<string, object> props)
        {
            if (props == null || props.Count == 0) return null;
            var parts = new List<string>();
            foreach (var pair in props)
            {
                parts.Add(pair.Key + "=" + (pair.Value ?? "null"));
            }

            return string.Join(" ", parts);
        }
    }
}
