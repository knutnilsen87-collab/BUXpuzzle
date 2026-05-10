using Game.Audio;
using Game.Telemetry;
using UnityEngine;

namespace Game.UI
{
    public sealed class SimpleHud : MonoBehaviour
    {
        public event System.Action GoalCompleted;
        public const float HudHeight = 62f;
        public const float HudTopMargin = 8f;

        public int levelId = 1;
        public int targetMatches = 10;
        public int movesLeft = 20;
        public int score;
        public string objectiveLabel = "Matcher";
        public bool autoCompleteFromMatch = true;

        private int _matches;
        private float _displayMatches;
        private float _progressGlowStart = -100f;
        private bool _goalSent;
        private GUIStyle _titleStyle;
        private GUIStyle _statStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _centerStyle;
        private Texture2D _panelTexture;
        private Texture2D _trackTexture;
        private Texture2D _fillTexture;
        private Texture2D _glowTexture;

        public int Matches => _matches;
        public static float ReservedTopPixels => (Screen.height - Screen.safeArea.yMax) + HudTopMargin + HudHeight + 22f;

        private void Update()
        {
            _displayMatches = Mathf.MoveTowards(_displayMatches, _matches, Time.unscaledDeltaTime * 5f);
        }

        private void OnGUI()
        {
            EnsureStyles();

            float safeTop = Screen.height - Screen.safeArea.yMax;
            float top = safeTop + HudTopMargin;
            float margin = 12f;
            float available = Mathf.Max(280f, Screen.width - margin * 2f);
            float leftW = Mathf.Clamp(available * 0.22f, 88f, 132f);
            float scoreW = Mathf.Clamp(available * 0.22f, 92f, 132f);
            float rightW = Mathf.Clamp(available * 0.22f, 96f, 132f);
            float centerW = Mathf.Max(116f, available - leftW - scoreW - rightW - 24f);

            var left = new Rect(margin, top, leftW, HudHeight);
            var center = new Rect(left.xMax + 8f, top, centerW, HudHeight);
            var scoreRect = new Rect(center.xMax + 8f, top, scoreW, HudHeight);
            var right = new Rect(scoreRect.xMax + 8f, top, rightW, HudHeight);

            DrawPill(left);
            DrawPill(center);
            DrawPill(scoreRect);
            DrawPill(right);

            GUI.Label(new Rect(left.x + 12f, left.y + 9f, left.width - 24f, 20f), $"Level {levelId}", _titleStyle);
            GUI.Label(new Rect(left.x + 12f, left.y + 33f, left.width - 24f, 18f), "Rolig spill", _smallStyle);

            GUI.Label(new Rect(center.x + 12f, center.y + 8f, center.width - 24f, 19f), $"Mål: {objectiveLabel}", _centerStyle);
            GUI.Label(new Rect(center.x + 12f, center.y + 28f, center.width - 24f, 17f), $"{_matches} av {targetMatches}", _centerStyle);

            float p = Mathf.Clamp01(targetMatches <= 0 ? 0f : _displayMatches / targetMatches);
            var track = new Rect(center.x + 14f, center.y + 49f, center.width - 28f, 6f);
            GUI.DrawTexture(track, _trackTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(track.x, track.y, track.width * p, track.height), _fillTexture, ScaleMode.StretchToFill);

            if (Time.unscaledTime - _progressGlowStart < 0.5f && p > 0f)
            {
                float k = Mathf.Clamp01((Time.unscaledTime - _progressGlowStart) / 0.5f);
                float glowX = track.x + Mathf.Lerp(0f, track.width * p, k) - 18f;
                GUI.DrawTexture(new Rect(glowX, track.y - 3f, 36f, track.height + 6f), _glowTexture, ScaleMode.StretchToFill);
            }

            GUI.Label(new Rect(scoreRect.x + 10f, scoreRect.y + 8f, scoreRect.width - 20f, 28f), score.ToString(), _statStyle);
            GUI.Label(new Rect(scoreRect.x + 10f, scoreRect.y + 36f, scoreRect.width - 20f, 18f), "score", _centerStyle);

            GUI.Label(new Rect(right.x + 10f, right.y + 8f, right.width - 20f, 28f), movesLeft.ToString(), _statStyle);
            GUI.Label(new Rect(right.x + 10f, right.y + 36f, right.width - 20f, 18f), "trekk igjen", _centerStyle);
        }

        public void Configure(int level, int goalMatches, int startingMoves)
        {
            Configure(level, "Matcher", goalMatches, startingMoves);
        }

        public void Configure(int level, string objective, int goalTarget, int startingMoves)
        {
            levelId = Mathf.Max(1, level);
            objectiveLabel = string.IsNullOrEmpty(objective) ? "Matcher" : objective;
            targetMatches = Mathf.Max(1, goalTarget);
            movesLeft = Mathf.Max(0, startingMoves);
            score = 0;
            _matches = 0;
            _displayMatches = 0f;
            _goalSent = false;
        }

        public void SetScore(int value)
        {
            score = Mathf.Max(0, value);
        }

        public void SetObjectiveProgress(int progress, int target)
        {
            _matches = Mathf.Clamp(progress, 0, Mathf.Max(1, target));
            targetMatches = Mathf.Max(1, target);
            _progressGlowStart = Time.unscaledTime;

            if (!_goalSent && _matches >= targetMatches)
            {
                _goalSent = true;
                GoalCompleted?.Invoke();
            }
        }

        public void OnMatch()
        {
            OnMatch(3, 1);
        }

        public void OnMatch(int clearedTiles, int cascadeIterations)
        {
            if (_goalSent) return;

            _matches++;
            movesLeft = Mathf.Max(0, movesLeft - 1);
            _progressGlowStart = Time.unscaledTime;
            GameAudioController.Ensure().Play(AudioEvent.GoalProgress);

            GameTelemetry.Track("level.goal_progress", GameTelemetry.Props(
                "level_id", levelId,
                "matches", _matches,
                "target_matches", targetMatches,
                "moves_left", movesLeft,
                "cleared_tile_count", clearedTiles,
                "cascade_iteration_count", cascadeIterations
            ));

            if (autoCompleteFromMatch && _matches >= targetMatches)
            {
                _goalSent = true;
                GoalCompleted?.Invoke();
            }
        }

        private void DrawPill(Rect rect)
        {
            GUI.DrawTexture(rect, _panelTexture, ScaleMode.StretchToFill);
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.98f, 0.96f, 0.83f, 1f) }
            };

            _smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.88f, 0.96f, 0.86f, 1f) }
            };

            _centerStyle = new GUIStyle(_smallStyle)
            {
                alignment = TextAnchor.MiddleCenter
            };

            _statStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.91f, 0.54f, 1f) }
            };

            _panelTexture = MakeTexture(new Color(0.10f, 0.22f, 0.23f, 0.70f));
            _trackTexture = MakeTexture(new Color(0.04f, 0.11f, 0.12f, 0.68f));
            _fillTexture = MakeTexture(new Color(0.92f, 0.76f, 0.28f, 1f));
            _glowTexture = MakeTexture(new Color(1f, 0.98f, 0.66f, 0.55f));
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
