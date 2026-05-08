using UnityEngine;

namespace Game.Logging
{
    public class AutoLogFileBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            FileLogSink.Start();
        }
    }
}
