using UnityEngine;

namespace Game.Presentation
{
    [DefaultExecutionOrder(-1000)]
    public sealed class RuntimeBootstrap : MonoBehaviour
    {
        void Awake()
        {
            // Ensure core runtime singletons exist
            Game.UI.ToastUI.Ensure();
            Game.Logging.UlfUnityLogger.Start();

            var root = FindFirstObjectByType<GameRoot>();
            if (root == null)
            {
                var goRoot = new GameObject("GameRoot");
                root = goRoot.AddComponent<GameRoot>();
                Debug.Log("[RuntimeBootstrap] Created GameRoot");
            }

            // Ensure TileInput exists at runtime
            var input = FindFirstObjectByType<TileInput>();
            if (input == null)
            {
                var go = new GameObject("TileInput");
                input = go.AddComponent<TileInput>();
                Debug.Log("[RuntimeBootstrap] Created TileInput");
            }

            // Ensure camera is orthographic (safe)
            var cam = Camera.main;
            if (cam != null) cam.orthographic = true;
        }
    }
}
