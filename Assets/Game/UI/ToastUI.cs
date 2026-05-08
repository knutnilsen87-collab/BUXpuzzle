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

            var w = Mathf.Min(420, Screen.width - 32);
            var h = 48;
            var r = new Rect(Screen.width/2 - w/2, 128, w, h);
            GUI.Box(r, GUIContent.none);
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.98f, 0.96f, 0.84f, 1f) }
            };
            GUI.Label(new Rect(r.x + 12, r.y + 8, r.width - 24, r.height - 16), _msg, style);
        }
    }
}
