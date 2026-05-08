using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game.Logging
{
    public static class FileLogSink
    {
        private static StreamWriter _writer;
        private static string _logPath;

        public static string LogPath => _logPath;

        public static void Start()
        {
            if (_writer != null) return;

            var dir = Path.Combine(Application.persistentDataPath, "logs", "latest");
            Directory.CreateDirectory(dir);

            _logPath = Path.Combine(dir, "main.log");

            // Append so we don't lose previous session logs.
            _writer = new StreamWriter(_logPath, append: true, encoding: new UTF8Encoding(false));
            _writer.AutoFlush = true;

            Application.logMessageReceived += OnLog;

            // Write header
            WriteLine("=== LOG START === " + DateTime.UtcNow.ToString("o"));
            WriteLine("persistentDataPath=" + Application.persistentDataPath);
            Debug.Log("[FileLogSink] Writing logs to: " + _logPath);
        }

        public static void Stop()
        {
            if (_writer == null) return;
            Application.logMessageReceived -= OnLog;
            WriteLine("=== LOG STOP === " + DateTime.UtcNow.ToString("o"));
            _writer.Dispose();
            _writer = null;
        }

        private static void OnLog(string condition, string stacktrace, LogType type)
        {
            // One line per event (simple, readable). Can be upgraded to ULF JSONL later.
            WriteLine($"[{DateTime.UtcNow:o}] {type}: {condition}");
            if (type == LogType.Error || type == LogType.Exception)
            {
                if (!string.IsNullOrEmpty(stacktrace))
                    WriteLine(stacktrace);
            }
        }

        private static void WriteLine(string line)
        {
            try { _writer?.WriteLine(line); }
            catch { /* never crash game due to logging */ }
        }
    }
}
