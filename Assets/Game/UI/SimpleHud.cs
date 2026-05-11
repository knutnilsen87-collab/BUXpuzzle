using System;
using Game.Audio;
using Game.Telemetry;
using UnityEngine;
using UnityEngine.UI;

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
        private Canvas _canvas;
        private RectTransform _root;
        private Text _levelText;
        private Text _modeText;
        private Text _objectiveText;
        private Text _progressText;
        private Text _scoreText;
        private Text _scoreLabelText;
        private Text _movesText;
        private Text _movesLabelText;
        private Image _objectiveIcon;
        private Image _progressFill;
        private Image _progressGlow;
        private Sprite _cardSprite;
        private Sprite _trackSprite;
        private Sprite _fillSprite;
        private Sprite _mossIconSprite;
        private Sprite _vineIconSprite;
        private Font _font;

        public int Matches => _matches;
        public static float ReservedTopPixels => (Screen.height - Screen.safeArea.yMax) + HudTopMargin + HudHeight + 22f;

        private void Awake()
        {
            EnsureUi();
            RefreshImmediate();
        }

        private void OnEnable()
        {
            EnsureUi();
            RefreshImmediate();
        }

        private void Update()
        {
            _displayMatches = Mathf.MoveTowards(_displayMatches, _matches, Time.unscaledDeltaTime * 5f);
            EnsureUi();
            RefreshUi();
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
            RefreshImmediate();
        }

        public void SetScore(int value)
        {
            score = Mathf.Max(0, value);
            RefreshUi();
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

            RefreshUi();
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

            RefreshUi();
        }

        private void RefreshImmediate()
        {
            _displayMatches = _matches;
            EnsureUi();
            RefreshUi();
        }

        private void RefreshUi()
        {
            if (_root == null) return;

            ApplySafeArea();
            SetText(_levelText, "Level " + levelId);
            SetText(_modeText, "Rolig spill");
            SetText(_objectiveText, objectiveLabel);
            SetText(_progressText, _matches + " av " + targetMatches);
            SetText(_scoreText, score.ToString());
            SetText(_scoreLabelText, "score");
            SetText(_movesText, movesLeft.ToString());
            SetText(_movesLabelText, "trekk igjen");

            bool showIcon = IsMossObjective() || IsVineObjective();
            if (_objectiveIcon != null)
            {
                _objectiveIcon.gameObject.SetActive(showIcon);
                _objectiveIcon.sprite = IsVineObjective() ? _vineIconSprite : _mossIconSprite;
            }

            float progress = Mathf.Clamp01(targetMatches <= 0 ? 0f : _displayMatches / targetMatches);
            if (_progressFill != null)
            {
                _progressFill.fillAmount = progress;
            }

            if (_progressGlow != null)
            {
                bool glow = Time.unscaledTime - _progressGlowStart < 0.5f && progress > 0f;
                _progressGlow.enabled = glow;
                if (glow)
                {
                    float k = Mathf.Clamp01((Time.unscaledTime - _progressGlowStart) / 0.5f);
                    var rect = (RectTransform)_progressGlow.transform;
                    rect.anchorMin = new Vector2(Mathf.Lerp(0f, progress, k), 0.5f);
                    rect.anchorMax = rect.anchorMin;
                }
            }
        }

        private void EnsureUi()
        {
            if (_root != null) return;

            _font = LoadRuntimeFont();
            _cardSprite = MakeRoundedSprite("hud-card", new Color(0.11f, 0.24f, 0.23f, 0.82f), new Color(0.24f, 0.42f, 0.34f, 0.90f), 18);
            _trackSprite = MakeRoundedSprite("hud-track", new Color(0.04f, 0.12f, 0.12f, 0.74f), new Color(0.07f, 0.16f, 0.15f, 0.78f), 5);
            _fillSprite = MakeRoundedSprite("hud-fill", new Color(0.86f, 0.63f, 0.20f, 1f), new Color(1f, 0.88f, 0.42f, 1f), 5);
            _mossIconSprite = MakeObjectiveIcon("hud-moss-icon", new Color(0.34f, 0.82f, 0.24f, 1f), new Color(0.12f, 0.36f, 0.12f, 1f));
            _vineIconSprite = MakeObjectiveIcon("hud-vine-icon", new Color(0.20f, 0.64f, 0.18f, 1f), new Color(0.10f, 0.28f, 0.10f, 1f));

            var canvasGo = new GameObject("SimpleHudCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            canvasGo.AddComponent<GraphicRaycaster>();
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);
            scaler.matchWidthOrHeight = 0.5f;

            _root = new GameObject("HudRoot").AddComponent<RectTransform>();
            _root.SetParent(canvasGo.transform, false);
            _root.anchorMin = new Vector2(0f, 1f);
            _root.anchorMax = new Vector2(1f, 1f);
            _root.pivot = new Vector2(0.5f, 1f);

            var left = CreateCard("LevelCard", _root);
            var center = CreateCard("ObjectiveCard", _root);
            var scoreCard = CreateCard("ScoreCard", _root);
            var movesCard = CreateCard("MovesCard", _root);

            _levelText = CreateText("LevelText", left, 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.98f, 0.96f, 0.83f, 1f));
            SetAnchors((RectTransform)_levelText.transform, 12f, -6f, -12f, -28f);
            _modeText = CreateText("ModeText", left, 13, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.88f, 0.96f, 0.86f, 1f));
            SetAnchors((RectTransform)_modeText.transform, 12f, -32f, -12f, -50f);

            _objectiveIcon = CreateImage("ObjectiveIcon", center, _mossIconSprite);
            var iconRect = (RectTransform)_objectiveIcon.transform;
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(28f, 7f);
            iconRect.sizeDelta = new Vector2(30f, 30f);

            _objectiveText = CreateText("ObjectiveText", center, 15, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.96f, 1f, 0.86f, 1f));
            SetAnchors((RectTransform)_objectiveText.transform, 44f, -5f, -14f, -24f);
            _progressText = CreateText("ProgressText", center, 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.88f, 0.96f, 0.86f, 1f));
            SetAnchors((RectTransform)_progressText.transform, 44f, -25f, -14f, -42f);

            var track = CreateImage("ProgressTrack", center, _trackSprite);
            SetAnchors((RectTransform)track.transform, 16f, -50f, -16f, -57f);
            _progressFill = CreateImage("ProgressFill", track.rectTransform, _fillSprite);
            _progressFill.type = Image.Type.Filled;
            _progressFill.fillMethod = Image.FillMethod.Horizontal;
            SetStretch((RectTransform)_progressFill.transform);
            _progressGlow = CreateImage("ProgressGlow", track.rectTransform, _fillSprite);
            _progressGlow.color = new Color(1f, 0.98f, 0.66f, 0.55f);
            var glowRect = (RectTransform)_progressGlow.transform;
            glowRect.anchorMin = new Vector2(0f, 0.5f);
            glowRect.anchorMax = new Vector2(0f, 0.5f);
            glowRect.pivot = new Vector2(0.5f, 0.5f);
            glowRect.sizeDelta = new Vector2(28f, 12f);
            _progressGlow.enabled = false;

            _scoreText = CreateText("ScoreText", scoreCard, 26, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.91f, 0.54f, 1f));
            SetAnchors((RectTransform)_scoreText.transform, 8f, -4f, -8f, -34f);
            _scoreLabelText = CreateText("ScoreLabel", scoreCard, 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.88f, 0.96f, 0.86f, 1f));
            SetAnchors((RectTransform)_scoreLabelText.transform, 8f, -36f, -8f, -54f);

            _movesText = CreateText("MovesText", movesCard, 26, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.91f, 0.54f, 1f));
            SetAnchors((RectTransform)_movesText.transform, 8f, -4f, -8f, -34f);
            _movesLabelText = CreateText("MovesLabel", movesCard, 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.88f, 0.96f, 0.86f, 1f));
            SetAnchors((RectTransform)_movesLabelText.transform, 8f, -36f, -8f, -54f);

            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (_root == null) return;

            float safeTop = Screen.height - Screen.safeArea.yMax;
            float margin = 12f;
            float top = safeTop + HudTopMargin;
            _root.offsetMin = new Vector2(margin, 0f);
            _root.offsetMax = new Vector2(-margin, -top);
            _root.sizeDelta = new Vector2(_root.sizeDelta.x, HudHeight);

            float available = Mathf.Max(280f, Screen.width - margin * 2f);
            float leftW = Mathf.Clamp(available * 0.22f, 88f, 132f);
            float scoreW = Mathf.Clamp(available * 0.22f, 92f, 132f);
            float rightW = Mathf.Clamp(available * 0.22f, 96f, 132f);
            float centerW = Mathf.Max(140f, available - leftW - scoreW - rightW - 24f);

            LayoutCard("LevelCard", 0f, leftW);
            LayoutCard("ObjectiveCard", leftW + 8f, centerW);
            LayoutCard("ScoreCard", leftW + centerW + 16f, scoreW);
            LayoutCard("MovesCard", leftW + centerW + scoreW + 24f, rightW);
        }

        private RectTransform CreateCard(string name, RectTransform parent)
        {
            var image = CreateImage(name, parent, _cardSprite);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            return image.rectTransform;
        }

        private void LayoutCard(string name, float x, float width)
        {
            var card = _root.Find(name) as RectTransform;
            if (card == null) return;

            card.anchorMin = new Vector2(0f, 1f);
            card.anchorMax = new Vector2(0f, 1f);
            card.pivot = new Vector2(0f, 1f);
            card.anchoredPosition = new Vector2(x, 0f);
            card.sizeDelta = new Vector2(width, HudHeight);
        }

        private Text CreateText(string name, RectTransform parent, int size, FontStyle style, TextAnchor anchor, Color color)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = _font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static Font LoadRuntimeFont()
        {
            try
            {
                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font != null) return font;
            }
            catch (ArgumentException)
            {
                // Older Unity versions used a different built-in font name.
            }

            return Font.CreateDynamicFontFromOSFont("Arial", 14);
        }

        private static Image CreateImage(string name, RectTransform parent, Sprite sprite)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value ?? string.Empty;
        }

        private static void SetAnchors(RectTransform rect, float left, float top, float right, float bottom)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }

        private static void SetStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private bool IsMossObjective()
        {
            return !string.IsNullOrEmpty(objectiveLabel) && objectiveLabel.IndexOf("Moss", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsVineObjective()
        {
            return !string.IsNullOrEmpty(objectiveLabel) && objectiveLabel.IndexOf("Vine", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Sprite MakeRoundedSprite(string name, Color bottom, Color top, int radius)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = (x + 0.5f) / size;
                    float py = (y + 0.5f) / size;
                    float edge = RoundedRectDistance(px, py, 0.48f, 0.38f, radius / 64f);
                    if (edge > 0f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    Color c = Color.Lerp(bottom, top, py);
                    if (edge > -0.05f) c = Color.Lerp(c, new Color(0.95f, 1f, 0.78f, c.a), 0.18f);
                    texture.SetPixel(x, y, c);
                }
            }

            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            sprite.name = name;
            return sprite;
        }

        private static Sprite MakeObjectiveIcon(string name, Color top, Color bottom)
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = ((x + 0.5f) / size - 0.5f) * 2f;
                    float py = ((y + 0.5f) / size - 0.5f) * 2f;
                    float d = Mathf.Sqrt(px * px + py * py);
                    if (d > 0.92f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    Color c = Color.Lerp(bottom, top, (py + 1f) * 0.5f);
                    float rim = Mathf.Clamp01((d - 0.68f) / 0.24f);
                    c = Color.Lerp(c, new Color(0.90f, 1f, 0.72f, 1f), rim * 0.28f);
                    c.a = Mathf.Clamp01((0.92f - d) / 0.08f);
                    texture.SetPixel(x, y, c);
                }
            }

            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = name;
            return sprite;
        }

        private static float RoundedRectDistance(float px, float py, float halfW, float halfH, float radius)
        {
            float qx = Mathf.Abs(px - 0.5f) - (halfW - radius);
            float qy = Mathf.Abs(py - 0.5f) - (halfH - radius);
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
            return outside + inside - radius;
        }
    }
}
