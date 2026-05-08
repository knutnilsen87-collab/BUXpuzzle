using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Minimal MVP toast (OnGUI) for clarity feedback.
    /// No dependencies on Unity UI.
    /// </summary>
    public sealed class ToastUI : MonoBehaviour
    {
        private static ToastUI _instance;

        private string _msg;
        private float _until;

        public static void Ensure()
        {
            if (_instance != null) return;
            var existing = FindFirstObjectByType<ToastUI>();
            if (existing != null) { _instance = existing; return; }

            var go = new GameObject("ToastUI");
            _instance = go.AddComponent<ToastUI>();
            DontDestroyOnLoad(go);
        }

        public static void Show(string msg, float seconds = 1.2f)
        {
            Ensure();
            if (_instance == null) return;
            _instance._msg = msg;
            _instance._until = Time.unscaledTime + Mathf.Max(0.2f, seconds);
        }

        void Awake()
        {
            if (_instance == null) _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnGUI()
        {
            if (string.IsNullOrEmpty(_msg)) return;
            if (Time.unscaledTime > _until) { _msg = null; return; }

            var w = 260;
            var h = 44;
            var r = new Rect(Screen.width/2 - w/2, 26, w, h);
            GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x + 12, r.y + 12, r.width - 24, r.height - 24), _msg);
        }
    }
}
