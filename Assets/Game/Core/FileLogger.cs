using System;
using System.IO;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Writes a simple runtime log into the project's external logs folder.
    /// This avoids "Unity log is somewhere else" confusion.
    /// </summary>
    public sealed class FileLogger : MonoBehaviour
    {
        private static StreamWriter _w;
        private static string _path;

        [SerializeField] private string externalLogDir = @"F:\prosjekter\candycrush\logs\latest";

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            try
            {
                Directory.CreateDirectory(externalLogDir);
                _path = Path.Combine(externalLogDir, "latest.log");
                _w = new StreamWriter(_path, append: true);
                _w.AutoFlush = true;
                Write("=== RUN START " + DateTime.Now.ToString("s") + " ===");
                Application.logMessageReceived += OnUnityLog;
                Write("FileLogger attached. Writing to: " + _path);
            }
            catch (Exception e)
            {
                Debug.LogError("[FileLogger] Failed: " + e.Message);
            }
        }

        void OnDestroy()
        {
            try
            {
                Application.logMessageReceived -= OnUnityLog;
                Write("=== RUN END " + DateTime.Now.ToString("s") + " ===");
                _w?.Dispose();
            }
            catch {}
        }

        private void OnUnityLog(string condition, string stackTrace, LogType type)
        {
            Write($"[{type}] {condition}");
            if (type == LogType.Exception || type == LogType.Error)
                Write(stackTrace);
        }

        public static void Write(string msg)
        {
            try { _w?.WriteLine(msg); } catch {}
        }
    }
}
