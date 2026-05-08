using UnityEngine;
using Game.Telemetry;

namespace Game.UI
{
    /// <summary>
    /// Always-visible top HUD: Objective + Moves + Progress.
    /// MVP: counts 'matches' via public method OnMatch(); we can wire later.
    /// </summary>
    public sealed class SimpleHud : MonoBehaviour
    {
        public event System.Action GoalCompleted;
        public int levelId = 1;
        public int targetMatches = 10;
        public int movesLeft = 20;

        private int _matches = 0;
        private bool _goalSent;
        private GUIStyle _titleStyle;
        private GUIStyle _statStyle;
        private GUIStyle _smallStyle;
        private Texture2D _panelTexture;
        private Texture2D _trackTexture;
        private Texture2D _fillTexture;

        void OnGUI()
        {
            EnsureStyles();

            float width = Mathf.Min(560f, Screen.width - 32f);
            float height = 102f;
            var panel = new Rect((Screen.width - width) * 0.5f, 14f, width, height);
            GUI.DrawTexture(panel, _panelTexture, ScaleMode.StretchToFill);

            var titleRect = new Rect(panel.x + 22f, panel.y + 13f, panel.width - 44f, 26f);
            GUI.Label(titleRect, $"Level {levelId}", _titleStyle);

            var objectiveRect = new Rect(panel.x + 22f, panel.y + 43f, panel.width * 0.64f, 22f);
            GUI.Label(objectiveRect, $"Lag {targetMatches} matcher", _smallStyle);

            var progressRect = new Rect(panel.x + 22f, panel.y + 62f, panel.width * 0.64f, 20f);
            GUI.Label(progressRect, $"{_matches}/{targetMatches} fullført", _smallStyle);

            var movesRect = new Rect(panel.x + panel.width - 146f, panel.y + 36f, 120f, 34f);
            GUI.Label(movesRect, movesLeft.ToString(), _statStyle);
            GUI.Label(new Rect(movesRect.x, movesRect.y + 34f, movesRect.width, 18f), "trekk", _smallStyle);

            float p = Mathf.Clamp01(targetMatches <= 0 ? 0f : (float)_matches / targetMatches);
            var track = new Rect(panel.x + 22f, panel.y + 86f, panel.width - 44f, 8f);
            GUI.DrawTexture(track, _trackTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(track.x, track.y, track.width * p, track.height), _fillTexture, ScaleMode.StretchToFill);
        }

        public void Configure(int level, int goalMatches, int startingMoves)
        {
            levelId = Mathf.Max(1, level);
            targetMatches = Mathf.Max(1, goalMatches);
            movesLeft = Mathf.Max(0, startingMoves);
            _matches = 0;
            _goalSent = false;
        }

        public void OnMatch()
        {
            OnMatch(3, 1);
        }

        public void OnMatch(int clearedTiles, int cascadeIterations)
        {
            _matches++;
            movesLeft = Mathf.Max(0, movesLeft - 1);
            GameTelemetry.Track("level.goal_progress", GameTelemetry.Props(
                "level_id", levelId,
                "matches", _matches,
                "target_matches", targetMatches,
                "moves_left", movesLeft,
                "cleared_tile_count", clearedTiles,
                "cascade_iteration_count", cascadeIterations
            ));

            if (!_goalSent && _matches >= targetMatches)
            {
                _goalSent = true;
                GoalCompleted?.Invoke();
            }
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.98f, 0.96f, 0.83f, 1f) }
            };

            _smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.88f, 0.96f, 0.86f, 1f) }
            };

            _statStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.91f, 0.54f, 1f) }
            };

            _panelTexture = MakeTexture(new Color(0.10f, 0.22f, 0.23f, 0.86f));
            _trackTexture = MakeTexture(new Color(0.04f, 0.11f, 0.12f, 0.68f));
            _fillTexture = MakeTexture(new Color(0.92f, 0.76f, 0.28f, 1f));
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
