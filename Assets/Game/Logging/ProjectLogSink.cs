using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game.Logging
{
    /// <summary>
    /// Writes Unity logs to a fixed external folder (DevOps-style):
    ///   F:\prosjekter\candycrush\logs\latest\unity.log
    /// and per-run:
    ///   F:\prosjekter\candycrush\logs\yyyyMmdDD\run-<id>\unity.log
    /// </summary>
    public static class ProjectLogSink
    {
        // IMPORTANT: fixed log root (requested)
        private const string FixedLogRoot = @"F:\prosjekter\candycrush\logs";

        private static StreamWriter _runWriter;
        private static string _latestPath;
        private static string _runPath;
        private static string _runId;

        public static string LatestPath => _latestPath;
        public static string RunPath => _runPath;
        public static string RunId => _runId;

        public static void Start()
        {
            if (_runWriter != null) return;

            try
            {
                var latestDir = Path.Combine(FixedLogRoot, "latest");
                Directory.CreateDirectory(latestDir);

                // Match your existing convention: 2026M1d11
                var day = DateTime.UtcNow.ToString("yyyy'M'Md'd'dd");
                _runId = Guid.NewGuid().ToString("N").Substring(0, 12);

                var runDir = Path.Combine(FixedLogRoot, day, "run-" + _runId);
                Directory.CreateDirectory(runDir);

                _latestPath = Path.Combine(latestDir, "unity.log");
                _runPath    = Path.Combine(runDir, "unity.log");

                // fresh run file; latest is append mirror
                _runWriter = new StreamWriter(_runPath, append: false, encoding: new UTF8Encoding(false));
                _runWriter.AutoFlush = true;

                Application.logMessageReceived += OnLog;

                var header = $"=== UNITY LOG START === {DateTime.UtcNow:o} runId={_runId}";
                WriteLine(header);
                MirrorLatest(header);

                Debug.Log("[ProjectLogSink] Writing run log to: " + _runPath);
                Debug.Log("[ProjectLogSink] Mirroring latest to: " + _latestPath);
            }
            catch (Exception e)
            {
                Debug.LogError("[ProjectLogSink] Failed to start: " + e);
            }
        }

        public static void Stop()
        {
            if (_runWriter == null) return;

            Application.logMessageReceived -= OnLog;

            var footer = $"=== UNITY LOG STOP === {DateTime.UtcNow:o} runId={_runId}";
            WriteLine(footer);
            MirrorLatest(footer);

            try { _runWriter.Dispose(); } catch { }
            _runWriter = null;
        }

        private static void OnLog(string condition, string stacktrace, LogType type)
        {
            var line = $"[{DateTime.UtcNow:o}] {type}: {condition}";
            WriteLine(line);
            MirrorLatest(line);

            if ((type == LogType.Error || type == LogType.Exception) && !string.IsNullOrEmpty(stacktrace))
            {
                WriteLine(stacktrace);
                MirrorLatest(stacktrace);
            }
        }

        private static void WriteLine(string line)
        {
            try { _runWriter?.WriteLine(line); } catch { /* never crash due to logging */ }
        }

        private static void MirrorLatest(string line)
        {
            try
            {
                if (string.IsNullOrEmpty(_latestPath)) return;
                File.AppendAllText(_latestPath, line + Environment.NewLine, new UTF8Encoding(false));
            }
            catch { /* ignore */ }
        }
    }
}
