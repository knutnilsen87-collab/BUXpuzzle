using System;
using System.IO;
using UnityEngine;

namespace Game.Core
{
    public sealed class PlayPingLogger : MonoBehaviour
    {
        [SerializeField] private string externalDir = @"F:\prosjekter\candycrush\logs\latest";

        private void Awake()
        {
            try
            {
                Directory.CreateDirectory(externalDir);
                var path = Path.Combine(externalDir, "play_ping.log");
                File.AppendAllText(path, DateTime.Now.ToString("s") + " | PlayPingLogger Awake\n");
                Debug.Log("[FBL] play_ping.log written: " + path);
            }
            catch (Exception e)
            {
                Debug.LogError("[FBL] PlayPingLogger failed: " + e);
            }
        }
    }
}
