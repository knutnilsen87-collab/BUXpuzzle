using Game.Audio;
using UnityEngine;

namespace Game.UI
{
    public sealed class MissionAccomplishedOverlay : MonoBehaviour
    {
        private static MissionAccomplishedOverlay _instance;
        private System.Action _nextLevel;
        private System.Action _replay;
        private bool _visible;
        private float _shownAt;
        private int _level;
        private int _matches;
        private int _target;
        private Texture2D _dim;
        private Texture2D _panel;
        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _button;

        public static MissionAccomplishedOverlay Ensure()
        {
            if (_instance != null) return _instance;
            var existing = FindFirstObjectByType<MissionAccomplishedOverlay>();
            if (existing != null)
            {
                _instance = existing;
                return _instance;
            }

            var go = new GameObject("MissionAccomplishedOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MissionAccomplishedOverlay>();
            return _instance;
        }

        public void Show(int level, int matches, int target, System.Action nextLevel, System.Action replay)
        {
            _level = Mathf.Max(1, level);
            _matches = Mathf.Max(0, matches);
            _target = Mathf.Max(1, target);
            _nextLevel = nextLevel;
            _replay = replay;
            _visible = true;
            _shownAt = Time.unscaledTime;
        }

        public void Hide()
        {
            _visible = false;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnGUI()
        {
            if (!_visible) return;
            EnsureStyles();

            float age = Time.unscaledTime - _shownAt;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / 0.28f));
            GUI.color = new Color(1f, 1f, 1f, 0.42f * k);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _dim);
            GUI.color = Color.white;

            float width = Mathf.Min(420f, Screen.width - 40f);
            float height = 230f;
            float scale = Mathf.Lerp(0.94f, 1f, k);
            var rect = new Rect((Screen.width - width * scale) * 0.5f, (Screen.height - height * scale) * 0.5f, width * scale, height * scale);
            GUI.DrawTexture(rect, _panel, ScaleMode.StretchToFill);

            GUI.Label(new Rect(rect.x + 22f, rect.y + 22f, rect.width - 44f, 38f), "Mission accomplished", _title);
            GUI.Label(new Rect(rect.x + 22f, rect.y + 70f, rect.width - 44f, 54f), $"Du fullførte Level {_level}\n{_matches} av {_target} matcher", _body);

            bool active = age > 0.35f;
            GUI.enabled = active;
            if (GUI.Button(new Rect(rect.x + 42f, rect.y + 142f, rect.width - 84f, 38f), "Neste level", _button))
            {
                GameAudioController.Ensure().Play(AudioEvent.UIButtonTap);
                _nextLevel?.Invoke();
            }

            if (GUI.Button(new Rect(rect.x + 42f, rect.y + 186f, rect.width - 84f, 30f), "Spill igjen", _button))
            {
                GameAudioController.Ensure().Play(AudioEvent.UIButtonTap, 0.75f);
                _replay?.Invoke();
            }
            GUI.enabled = true;
        }

        private void EnsureStyles()
        {
            if (_title != null) return;
            _dim = MakeTexture(new Color(0.02f, 0.07f, 0.08f, 1f));
            _panel = MakeTexture(new Color(0.12f, 0.27f, 0.25f, 0.96f));

            _title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.95f, 0.70f, 1f) }
            };

            _body = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.90f, 0.98f, 0.88f, 1f) }
            };

            _button = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
        }

        private static Texture2D MakeTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
