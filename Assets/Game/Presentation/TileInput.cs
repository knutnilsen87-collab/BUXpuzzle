using UnityEngine;

namespace Game.Presentation
{
    public sealed class TileInput : MonoBehaviour
    {
        public Camera CameraOverride;
        public LayerMask RaycastMask = ~0;

        private TileView _selected;
        private BoardView _boardView;
        private bool _busy;

        void Awake()
        {
            Debug.Log("[TileInput] Awake");
            _boardView = FindFirstObjectByType<BoardView>();
        }

        void Update()
        {
            if (_busy) return;

            if (Input.GetMouseButtonDown(0))
            {
                TryClick();
            }
        }

        private void TryClick()
        {
            var cam = CameraOverride != null ? CameraOverride : Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[TileInput] No camera");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 50f, Color.white, 0.5f);

            if (!Physics.Raycast(ray, out RaycastHit hit, 200f, RaycastMask))
            {
                Debug.Log("[TileInput] Click: no hit");
                return;
            }

            var tile = hit.collider.GetComponentInParent<TileView>();
            if (tile == null)
            {
                Debug.Log("[TileInput] Hit non-tile: " + hit.collider.name);
                return;
            }

            Debug.Log("[TileInput] Hit tile: " + tile.name);

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
                if (_boardView == null)
                {
                    _boardView = FindFirstObjectByType<BoardView>();
                }

                if (_boardView != null)
                {
                    _busy = true;

                    Game.Core.BoardEngine.ResolveSummary summary;
                    bool ok = _boardView.RequestSwap(_selected, tile, out summary);

                    Debug.Log(
                        "[TileInput] Swap result: " +
                        (ok ? "ACCEPT" : "REJECT") +
                        " clearedTiles=" + summary.clearedTiles +
                        " iterations=" + summary.iterations
                    );

                    _busy = false;
                }
                else
                {
                    Debug.LogError("[TileInput] No BoardView found; cannot swap");
                }

                Deselect();
            }
            else
            {
                Select(tile);
            }
        }

        private void Select(TileView tile)
        {
            if (_selected != null) _selected.SetSelected(false);
            _selected = tile;
            _selected.SetSelected(true);
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
