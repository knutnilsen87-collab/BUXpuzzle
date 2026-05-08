using Game.Core;
using Game.Presentation;
using Game.Telemetry;
using Game.UI;
using UnityEngine;

namespace Game.Onboarding
{
    public sealed class FirstMoveTutorialController : MonoBehaviour
    {
        [SerializeField] private bool disableTutorial;
        [SerializeField] private bool resetTutorialForQa;
        [SerializeField] private float strongerHintDelaySeconds = 2.5f;

        private GameRoot _root;
        private BoardView _boardView;
        private BoardMove _move;
        private bool _active;
        private bool _strongHintShown;
        private float _startedAt;

        public bool IsActive => _active;

        private void Awake()
        {
            _root = FindFirstObjectByType<GameRoot>();
            if (resetTutorialForQa)
            {
                TutorialStateStore.ResetForQa();
            }
        }

        private void OnEnable()
        {
            BindBoard();
        }

        private void OnDisable()
        {
            if (_boardView != null)
            {
                _boardView.BoardVisible -= OnBoardVisible;
                _boardView.SwapAccepted -= OnSwapAccepted;
                _boardView.SwapRejected -= OnSwapRejected;
            }
        }

        private void Update()
        {
            if (!_active) return;

            if (!_strongHintShown && Time.unscaledTime - _startedAt >= strongerHintDelaySeconds)
            {
                _strongHintShown = true;
                _boardView.SetTutorialMove(_move, true);
                ToastUI.Show(UXCopy.TutorialFirstMove, 2.3f);
                GameTelemetry.Track("tutorial.hint_shown", GameTelemetry.Props(
                    "level_id", LevelId(),
                    "from", _move.A.ToString(),
                    "to", _move.B.ToString()
                ));
            }
        }

        private void OnGUI()
        {
            if (!_active) return;

            var width = Mathf.Min(520f, Screen.width - 32f);
            var height = 58f;
            var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - height - 24f, width, height);
            GUI.Box(rect, GUIContent.none);
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Screen.width / 34, 14, 20),
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.98f, 0.96f, 0.84f, 1f) }
            };
            GUI.Label(new Rect(rect.x + 14f, rect.y + 8f, rect.width - 28f, rect.height - 16f), UXCopy.TutorialFirstMove, style);
        }

        private void BindBoard()
        {
            _boardView = FindFirstObjectByType<BoardView>();
            if (_boardView == null) return;

            _boardView.BoardVisible -= OnBoardVisible;
            _boardView.SwapAccepted -= OnSwapAccepted;
            _boardView.SwapRejected -= OnSwapRejected;
            _boardView.BoardVisible += OnBoardVisible;
            _boardView.SwapAccepted += OnSwapAccepted;
            _boardView.SwapRejected += OnSwapRejected;

            if (_root == null) _root = FindFirstObjectByType<GameRoot>();
            TryStart();
        }

        private void OnBoardVisible(BoardView board)
        {
            _boardView = board;
            TryStart();
        }

        private void TryStart()
        {
            if (disableTutorial || _active || TutorialStateStore.IsCompleted()) return;
            if (_root == null || _root.Board == null || _boardView == null) return;
            if (!ValidMoveFinder.TryFind(_root.Board, out _move)) return;

            _active = true;
            _strongHintShown = false;
            _startedAt = Time.unscaledTime;
            _boardView.SetTutorialMove(_move, false);
            ToastUI.Show(UXCopy.TutorialFirstMove, 2.5f);
            GameTelemetry.Track("tutorial.start", GameTelemetry.Props(
                "level_id", LevelId(),
                "from", _move.A.ToString(),
                "to", _move.B.ToString()
            ));
        }

        private void OnSwapAccepted(BoardMove move, BoardEngine.ResolveSummary summary, ResolveTrace trace, string inputType)
        {
            if (!_active) return;

            if (_boardView.IsTutorialMove(move))
            {
                Complete(summary, inputType);
                return;
            }

            if (summary.anyCleared)
            {
                Complete(summary, inputType);
            }
        }

        private void OnSwapRejected(BoardMove move, string inputType)
        {
            if (!_active) return;
            _boardView.SetTutorialMove(_move, true);
        }

        private void Complete(BoardEngine.ResolveSummary summary, string inputType)
        {
            _active = false;
            _boardView.ClearTutorialMove();
            TutorialStateStore.MarkCompleted();
            ToastUI.Show(UXCopy.TutorialAfterSuccess, 2.4f);
            GameTelemetry.Track("tutorial.complete", GameTelemetry.Props(
                "level_id", LevelId(),
                "input_type", inputType,
                "cleared_tile_count", summary.clearedTiles,
                "cascade_iteration_count", summary.iterations
            ));
        }

        private int LevelId()
        {
            return 1;
        }
    }
}
