using System;
using System.IO;
using UnityEngine;

namespace Game.Core
{
    public sealed class LatestLogWriter : MonoBehaviour
    {
        private StreamWriter _w;
        private string _primaryPath;
        private string _mirrorPath = @"F:\prosjekter\candycrush\logs\latest\latest.log";

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            try
            {
                var dir = Path.Combine(Application.persistentDataPath, "logs");
                Directory.CreateDirectory(dir);
                _primaryPath = Path.Combine(dir, "latest.log");
                _w = new StreamWriter(_primaryPath, append: true) { AutoFlush = true };

                Log("=== PLAY RUN START " + DateTime.Now.ToString("s") + " ===");
                Log("Primary: " + _primaryPath);

                // disable mirror if folder missing
                try
                {
                    var d = Path.GetDirectoryName(_mirrorPath);
                    if (string.IsNullOrEmpty(d) || !Directory.Exists(d)) _mirrorPath = null;
                } catch { _mirrorPath = null; }

                Application.logMessageReceived += OnUnityLog;
                Debug.Log("[FBL] LatestLogWriter active: " + _primaryPath);
            }
            catch (Exception e)
            {
                Debug.LogError("[FBL] LatestLogWriter failed: " + e);
            }
        }

        void OnDestroy()
        {
            try { Application.logMessageReceived -= OnUnityLog; } catch {}
            try { Log("=== PLAY RUN END " + DateTime.Now.ToString("s") + " ==="); } catch {}
            try { _w?.Dispose(); } catch {}
        }

        void OnUnityLog(string condition, string stackTrace, LogType type)
        {
            Log("[" + type + "] " + condition);
            if (type == LogType.Error || type == LogType.Exception) Log(stackTrace);
        }

        void Log(string s)
        {
            try { _w?.WriteLine(s); } catch {}
            try { if (!string.IsNullOrEmpty(_mirrorPath)) File.AppendAllText(_mirrorPath, s + Environment.NewLine); } catch {}
        }
    }
}
