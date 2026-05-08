using UnityEngine;
using Game.Core;
using Game.Telemetry;

namespace Game.Presentation
{
    public sealed class TileInput : MonoBehaviour
    {
        public Camera CameraOverride;
        public LayerMask RaycastMask = ~0;
        public float DragThresholdPixels = 42f;

        private TileView _selected;
        private BoardView _boardView;
        private TileView _pointerStartTile;
        private Vector2 _pointerStartScreen;
        private bool _pointerDown;
        private bool _gestureConsumed;
        private bool _firstInputSent;

        void Awake()
        {
            Debug.Log("[TileInput] Awake");
            _boardView = FindFirstObjectByType<BoardView>();
        }

        void Update()
        {
            if (_boardView == null)
            {
                _boardView = FindFirstObjectByType<BoardView>();
            }

            if (_boardView != null && _boardView.IsResolving) return;

            if (Input.touchCount > 0)
            {
                HandleTouch(Input.GetTouch(0));
                return;
            }

            HandleMouse();
        }

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                BeginPointer(Input.mousePosition);
            }

            if (_pointerDown && Input.GetMouseButton(0))
            {
                TryDrag(Input.mousePosition, "drag");
            }

            if (_pointerDown && Input.GetMouseButtonUp(0))
            {
                EndPointer(Input.mousePosition, "tap");
            }
        }

        private void HandleTouch(Touch touch)
        {
            if (touch.phase == TouchPhase.Began)
            {
                BeginPointer(touch.position);
            }

            if (_pointerDown && (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
            {
                TryDrag(touch.position, "drag");
            }

            if (_pointerDown && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
            {
                EndPointer(touch.position, "tap");
            }
        }

        private void BeginPointer(Vector2 screenPosition)
        {
            if (_boardView != null && _boardView.IsResolving) return;

            _pointerStartTile = RaycastTile(screenPosition);
            _pointerStartScreen = screenPosition;
            _pointerDown = _pointerStartTile != null;
            _gestureConsumed = false;

            if (_pointerStartTile != null)
            {
                TrackFirstInput("pointer_down");
            }
        }

        private void TryDrag(Vector2 screenPosition, string inputType)
        {
            if (!_pointerDown || _gestureConsumed || _pointerStartTile == null) return;

            Vector2 delta = screenPosition - _pointerStartScreen;
            if (delta.magnitude < DragThresholdPixels) return;

            int dx = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y) ? (delta.x > 0f ? 1 : -1) : 0;
            int dy = Mathf.Abs(delta.y) > Mathf.Abs(delta.x) ? (delta.y > 0f ? 1 : -1) : 0;
            if (dx == 0 && dy == 0) return;

            var target = _boardView != null ? _boardView.GetTileView(_pointerStartTile.X + dx, _pointerStartTile.Y + dy) : null;
            if (target != null)
            {
                _gestureConsumed = true;
                RequestSwap(_pointerStartTile, target, inputType);
                Deselect();
            }
        }

        private void EndPointer(Vector2 screenPosition, string inputType)
        {
            if (!_pointerDown)
            {
                return;
            }

            if (!_gestureConsumed)
            {
                var tile = RaycastTile(screenPosition);
                if (tile != null)
                {
                    HandleTap(tile, inputType);
                }
            }

            _pointerDown = false;
            _pointerStartTile = null;
            _gestureConsumed = false;
        }

        private void HandleTap(TileView tile, string inputType)
        {
            TrackFirstInput(inputType);
            if (_selected == null)
            {
                Select(tile);
                return;
            }

            if (tile == _selected)
            {
                Deselect();
                return;
            }

            if (IsAdjacent(_selected, tile))
            {
                RequestSwap(_selected, tile, inputType);
                Deselect();
            }
            else
            {
                Select(tile);
            }
        }

        private void RequestSwap(TileView a, TileView b, string inputType)
        {
            if (_boardView == null)
            {
                _boardView = FindFirstObjectByType<BoardView>();
            }

            if (_boardView == null)
            {
                Debug.LogError("[TileInput] No BoardView found; cannot swap");
                return;
            }

            BoardEngine.ResolveSummary summary;
            bool ok = _boardView.RequestSwap(a, b, out summary, inputType);

            Debug.Log(
                "[TileInput] Swap result: " +
                (ok ? "ACCEPT" : "REJECT") +
                " clearedTiles=" + summary.clearedTiles +
                " iterations=" + summary.iterations
            );
        }

        private void Select(TileView tile)
        {
            if (_selected != null) _selected.SetSelected(false);
            _selected = tile;
            _selected.SetSelected(true);
            GameTelemetry.Track("game.tile_selected", GameTelemetry.Props(
                "input_type", "tap",
                "x", tile.X,
                "y", tile.Y,
                "tutorial_active", _boardView != null && _boardView.TutorialActive
            ));
        }

        private void Deselect()
        {
            if (_selected != null) _selected.SetSelected(false);
            _selected = null;
        }

        private static bool IsAdjacent(TileView a, TileView b)
        {
            int dx = Mathf.Abs(a.X - b.X);
            int dy = Mathf.Abs(a.Y - b.Y);
            return (dx + dy) == 1;
        }

        private TileView RaycastTile(Vector2 screenPosition)
        {
            var cam = CameraOverride != null ? CameraOverride : Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[TileInput] No camera");
                return null;
            }

            Ray ray = cam.ScreenPointToRay(screenPosition);
            Debug.DrawRay(ray.origin, ray.direction * 50f, Color.white, 0.5f);

            if (!Physics.Raycast(ray, out RaycastHit hit, 200f, RaycastMask))
            {
                return null;
            }

            return hit.collider.GetComponentInParent<TileView>();
        }

        private void TrackFirstInput(string inputType)
        {
            if (_firstInputSent) return;
            _firstInputSent = true;
            GameTelemetry.Track("game.first_input", GameTelemetry.Props(
                "input_type", inputType,
                "tutorial_active", _boardView != null && _boardView.TutorialActive
            ));
        }

        [System.Obsolete("Visual-only swap is forbidden. Use BoardView.RequestSwap.")]
        private static void Swap(TileView a, TileView b)
        {
            Vector3 posA = a.transform.position;
            a.transform.position = b.transform.position;
            b.transform.position = posA;

            int ax = a.X, ay = a.Y;
            a.SetCoords(b.X, b.Y);
            b.SetCoords(ax, ay);

            Debug.Log("[TileInput] Swapped: " + a.name + " <-> " + b.name);
        }
    }
}
