using UnityEngine;

namespace Game.Logging
{
    public class AutoProjectLogBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            ProjectLogSink.Start();
        }

        private void OnApplicationQuit()
        {
            ProjectLogSink.Stop();
        }
    }
}
