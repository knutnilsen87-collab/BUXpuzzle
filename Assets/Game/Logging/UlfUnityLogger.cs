using System;
using System.IO;
using UnityEngine;

namespace Game.Logging
{
    /// <summary>
    /// ULF v1 JSONL logger for Unity runtime.
    /// - Immutable Run Archive: persistentDataPath/logs/runs/unity_<ts>_<run8>/scene_recovery.jsonl
    /// - LATEST view:         persistentDataPath/logs/LATEST/scene_recovery.jsonl
    /// - Optional mirror:     F:\prosjekter\candycrush\logs\latest\scene_recovery.log (if exists)
    ///
    /// Never throws; logging must not crash gameplay.
    /// No PII.
    /// </summary>
    public static class UlfUnityLogger
    {
        private static bool _started;
        private static string _runId;
        private static string _runPath;
        private static string _latestPath;
        private static string _mirrorPath;
        private static readonly object _lock = new object();

        public static string RunId => _runId;
        public static string RunPath => _runPath;

        [Serializable]
        private sealed class UlfLine
        {
            public string v = "ulf-1.0";
            public string ts;
            public string level;
            public string msg;
            public UlfEvent @event;
            public UlfIds ids;
            public string extra; // keep as short string (no PII)
            public UlfError error;
        }

        [Serializable] private sealed class UlfEvent { public string type; public string name; public string stage; }
        [Serializable] private sealed class UlfIds { public string run_id; }
        [Serializable] private sealed class UlfError { public string stack; }

        public static void Start(string serviceName = "candycrush", string envName = "dev")
        {
            if (_started) return;
            _started = true;

            try
            {
                _runId = Guid.NewGuid().ToString("N");
                var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                var runDir = Path.Combine(Application.persistentDataPath, "logs", "runs", "unity_" + ts + "_" + _runId.Substring(0, 8));
                Directory.CreateDirectory(runDir);

                _runPath = Path.Combine(runDir, "scene_recovery.jsonl");

                var latestDir = Path.Combine(Application.persistentDataPath, "logs", "LATEST");
                Directory.CreateDirectory(latestDir);
                _latestPath = Path.Combine(latestDir, "scene_recovery.jsonl");

                var externalDir = @"F:\prosjekter\candycrush\logs\latest";
                if (Directory.Exists(externalDir))
                    _mirrorPath = Path.Combine(externalDir, "scene_recovery.log");

                Info("runtime.start", "Unity runtime started",
                    extra: $"service={serviceName} env={envName} unity={Application.unityVersion}");
            }
            catch (Exception e)
            {
                Debug.LogError("[ULF] Start failed: " + e.Message);
            }

            Application.logMessageReceived += OnUnityLog;
        }

        public static void Stop()
        {
            if (!_started) return;
            try { Info("runtime.stop", "Unity runtime stopping"); } catch { }
            try { Application.logMessageReceived -= OnUnityLog; } catch { }
        }

        private static void OnUnityLog(string condition, string stackTrace, LogType type)
        {
            // Don't reflect normal Debug.Log spam into ULF by default (keeps JSONL readable)
            if (type == LogType.Log) return;

            if (type == LogType.Warning) Warn("unity.warning", condition);
            if (type == LogType.Error) Error("unity.error", condition, stackTrace);
            if (type == LogType.Exception) Error("unity.exception", condition, stackTrace);
        }

        public static void Info(string name, string msg, string extra = null) => Write("INFO", name, msg, null, extra);
        public static void Warn(string name, string msg, string extra = null) => Write("WARN", name, msg, null, extra);
        public static void Error(string name, string msg, string stack = null, string extra = null) => Write("ERROR", name, msg, stack, extra);

        private static void Write(string level, string name, string msg, string stack, string extra)
        {
            try
            {
                var lineObj = new UlfLine
                {
                    ts = DateTime.UtcNow.ToString("o"),
                    level = level,
                    msg = msg,
                    @event = new UlfEvent { type = "game", name = name, stage = "runtime" },
                    ids = new UlfIds { run_id = _runId },
                    extra = extra,
                    error = string.IsNullOrEmpty(stack) ? null : new UlfError { stack = stack }
                };

                var line = JsonUtility.ToJson(lineObj, false);

                lock (_lock)
                {
                    AppendLine(_runPath, line);
                    AppendLine(_latestPath, line);
                    if (!string.IsNullOrEmpty(_mirrorPath)) AppendLine(_mirrorPath, line);
                    RefreshBundle();
                }
            }
            catch { /* never crash due to logging */ }
        }

        private static void AppendLine(string path, string line)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return;
                File.AppendAllText(path, line + Environment.NewLine);
            }
            catch { }
        }

        private static void RefreshBundle()
        {
            // Minimal ULF bundle file (fault-tolerant)
            try
            {
                var bundlePath = Path.Combine(Application.persistentDataPath, "logs", "FBL_STATUS_BUNDLE.txt");
                var txt =
                    "FBL_STATUS_BUNDLE\n" +
                    "ts=" + DateTime.UtcNow.ToString("o") + "\n" +
                    "run_id=" + _runId + "\n" +
                    "run_path=" + _runPath + "\n" +
                    "latest_path=" + _latestPath + "\n" +
                    "mirror_path=" + (_mirrorPath ?? "") + "\n";
                File.WriteAllText(bundlePath, txt);
            }
            catch { }
        }
    }
}
