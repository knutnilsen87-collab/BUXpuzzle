using System;
using Game.Progression;
using UnityEngine;

namespace Game.UI
{
    public sealed class ResultScreenOverlay : MonoBehaviour
    {
        private static ResultScreenOverlay _instance;

        private SessionResult _session;
        private ResultsData _results;
        private Action _next;
        private Action _replay;
        private Action _share;
        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _small;
        private GUIStyle _button;
        private Texture2D _backdrop;
        private Texture2D _panel;
        private Texture2D _card;

        public static ResultScreenOverlay Ensure()
        {
            var existing = FindFirstObjectByType<ResultScreenOverlay>();
            if (existing != null)
            {
                _instance = existing;
                return _instance;
            }

            var go = new GameObject("ResultScreenOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ResultScreenOverlay>();
            _instance.gameObject.SetActive(false);
            return _instance;
        }

        public void Show(SessionResult session, ResultsData results, Action next, Action replay, Action share)
        {
            _session = session;
            _results = results;
            _next = next;
            _replay = replay;
            _share = share;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnGUI()
        {
            if (_session == null || _results == null) return;
            EnsureStyles();

            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _backdrop, ScaleMode.StretchToFill);

            float width = Mathf.Min(560f, Screen.width - 28f);
            float height = Mathf.Min(620f, Screen.height - 36f);
            var panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.DrawTexture(panel, _panel, ScaleMode.StretchToFill);

            float x = panel.x + 22f;
            float y = panel.y + 18f;
            float inner = panel.width - 44f;

            GUI.Label(new Rect(x, y, inner, 38f), _results.Headline ?? string.Empty, _title);
            y += 40f;
            GUI.Label(new Rect(x, y, inner, 26f), _results.Explanation ?? string.Empty, _body);
            y += 34f;

            DrawCard(new Rect(x, y, inner, 86f), "Level " + _session.LevelId + " score", _session.Score.ToString(), MedalText(_session.Medal) + "  " + Stars(_session.Stars));
            y += 96f;
            DrawCard(new Rect(x, y, inner, 68f), "Trekk", _session.MovesLeft + " igjen", "Beste cascade: " + _session.BestCascade);
            y += 78f;

            float cardW = (inner - 10f) * 0.5f;
            DrawRewardCard(new Rect(x, y, cardW, 76f), "Token", "token");
            DrawRewardCard(new Rect(x + cardW + 10f, y, cardW, 76f), "XP", "xp");
            y += 86f;
            DrawRewardCard(new Rect(x, y, cardW, 76f), "Fragment", "fragment");
            DrawCard(new Rect(x + cardW + 10f, y, cardW, 76f), "Skill", _results.SkillHighlight ?? "Tips", string.Empty);
            y += 90f;

            string garden = string.IsNullOrEmpty(_results.GardenProgress) ? string.Empty : _results.GardenProgress;
            string unlock = string.IsNullOrEmpty(_results.NextUnlock) ? string.Empty : "Neste: " + _results.NextUnlock;
            GUI.Label(new Rect(x, y, inner, 20f), garden, _small);
            GUI.Label(new Rect(x, y + 20f, inner, 20f), unlock, _small);
            y += 42f;
            GUI.Label(new Rect(x, y, inner, 28f), "Share code: " + (_session.ShareCode ?? string.Empty), _small);
            y = panel.yMax - 70f;

            float buttonW = (inner - 20f) / 3f;
            if (GUI.Button(new Rect(x, y, buttonW, 44f), _session.Win ? "Neste level" : "Prøv igjen", _button))
            {
                if (_session.Win) _next?.Invoke(); else _replay?.Invoke();
            }

            if (GUI.Button(new Rect(x + buttonW + 10f, y, buttonW, 44f), "Spill igjen", _button))
            {
                _replay?.Invoke();
            }

            if (GUI.Button(new Rect(x + (buttonW + 10f) * 2f, y, buttonW, 44f), "Del", _button))
            {
                _share?.Invoke();
            }
        }

        private void DrawRewardCard(Rect rect, string title, string currency)
        {
            int amount = 0;
            if (_results.Rewards != null)
            {
                foreach (var reward in _results.Rewards)
                {
                    if (string.Equals(reward.Currency, currency, StringComparison.OrdinalIgnoreCase))
                    {
                        amount += reward.Amount;
                    }
                }
            }

            DrawCard(rect, title, amount > 0 ? "+" + amount : "+0", "alltid progresjon");
        }

        private void DrawCard(Rect rect, string title, string value, string sub)
        {
            GUI.DrawTexture(rect, _card, ScaleMode.StretchToFill);
            GUI.Label(new Rect(rect.x + 14f, rect.y + 10f, rect.width - 28f, 22f), title, _small);
            GUI.Label(new Rect(rect.x + 14f, rect.y + 31f, rect.width - 28f, 28f), value, _body);
            if (!string.IsNullOrEmpty(sub))
            {
                GUI.Label(new Rect(rect.x + 14f, rect.yMax - 24f, rect.width - 28f, 18f), sub, _small);
            }
        }

        private void EnsureStyles()
        {
            if (_title != null) return;

            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.98f, 0.96f, 0.84f, 1f) }
            };

            _body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.94f, 0.98f, 0.91f, 1f) }
            };

            _small = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.82f, 0.92f, 0.84f, 1f) }
            };

            _button = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            _backdrop = MakeTexture(new Color(0.02f, 0.06f, 0.07f, 0.68f));
            _panel = MakeTexture(new Color(0.09f, 0.22f, 0.22f, 0.96f));
            _card = MakeTexture(new Color(0.16f, 0.34f, 0.31f, 0.86f));
        }

        private static string MedalText(Medal medal)
        {
            switch (medal)
            {
                case Medal.NordicPerfect: return "Nordic Perfect";
                case Medal.GoldenSun: return "Golden Sun";
                case Medal.SilverBloom: return "Silver Bloom";
                case Medal.BronzeLeaf: return "Bronze Leaf";
                default: return "Near miss";
            }
        }

        private static string Stars(int count)
        {
            if (count <= 0) return "0/3";
            if (count == 1) return "1/3";
            if (count == 2) return "2/3";
            return "3/3";
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
