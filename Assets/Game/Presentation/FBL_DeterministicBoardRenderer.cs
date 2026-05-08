using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Minimal deterministic board renderer.
    ///
    /// Contract:
    /// - Engine remains source of truth.
    /// - Presentation only mirrors snapshots.
    /// - Existing tiles move to new rows when possible.
    /// - New visuals spawn only above top row.
    ///
    /// Usage:
    /// - Add to Board / BoardView / GameRoot.
    /// - Call ApplySnapshot(width, height, flatCells) whenever board state changes.
    /// - Cell value < 0 means empty.
    /// </summary>
    public sealed class FBL_DeterministicBoardRenderer : MonoBehaviour
    {
        public bool FBL_ThemePrefabLooksConfigured()
        {
            return !string.IsNullOrWhiteSpace(natureLightPrefabPath);
        }

        public string FBL_GetThemePrefabPath()
        {
            return natureLightPrefabPath;
        }

        [Header("Phase B Nature Light")]
        [SerializeField] private string natureLightPrefabPath = "Assets/Game/Presentation/NatureLight/NatureLightTile.prefab";

        public void FBL_PhaseBPlayableBuildMarker()
        {
            Debug.Log("[FBL_DeterministicBoardRenderer] Phase B playable build marker active.");
        }

        [Header("Scene references")]
        [SerializeField] private Transform boardRoot;
        [SerializeField] private GameObject tilePrefab;

        [Header("Layout")]
        [SerializeField] private float tileSpacing = 1.0f;
        [SerializeField] private Vector2 origin = new Vector2(-3.5f, -3.5f);
        [SerializeField] private int spawnOffsetRows = 2;

        [Header("Animation")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private bool animateMovement = true;

        [Header("Fallback visuals")]
        [SerializeField] private Vector3 tileScale = new Vector3(0.92f, 0.92f, 1f);

        private readonly Dictionary<Vector2Int, TileVisual> _visualsByCell = new Dictionary<Vector2Int, TileVisual>();
        private readonly Dictionary<int, Color> _colorCache = new Dictionary<int, Color>();

        private int _width;
        private int _height;
        private int[] _lastSnapshot = Array.Empty<int>();

        private sealed class TileVisual
        {
            public GameObject GameObject;
            public int TileId;
            public Vector2Int Cell;
            public Coroutine ActiveMove;
        }

        private void Awake()
        {
            if (boardRoot == null)
            {
                boardRoot = transform;
            }
        }

        public void ApplySnapshot(int width, int height, int[] flatCells)
        {
            if (flatCells == null) throw new ArgumentNullException(nameof(flatCells));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (flatCells.Length != width * height)
            {
                throw new ArgumentException("flatCells length must equal width * height");
            }

            _width = width;
            _height = height;

            var nextOccupied = BuildOccupiedByColumn(width, height, flatCells);
            var prevOccupied = BuildPreviousOccupiedByColumn(width, height);

            var nextMap = new Dictionary<Vector2Int, TileVisual>();
            var reused = new HashSet<TileVisual>();

            for (int x = 0; x < width; x++)
            {
                var prevColumn = prevOccupied[x];
                var nextColumn = nextOccupied[x];

                // Match bottom-up so falls stay visually coherent.
                for (int nextIndex = 0; nextIndex < nextColumn.Count; nextIndex++)
                {
                    var nextEntry = nextColumn[nextIndex];
                    var targetCell = new Vector2Int(x, nextEntry.Row);

                    TileVisual matched = TryMatchVisual(prevColumn, nextEntry.TileId, reused);
                    bool isNewSpawn = matched == null;

                    if (matched == null)
                    {
                        matched = CreateVisual(nextEntry.TileId, x, height - 1 + spawnOffsetRows);
                    }

                    reused.Add(matched);
                    matched.TileId = nextEntry.TileId;
                    matched.Cell = targetCell;
                    ApplyVisualStyle(matched, nextEntry.TileId);

                    Vector3 targetLocalPosition = CellToLocalPosition(targetCell.x, targetCell.y);

                    if (isNewSpawn)
                    {
                        Vector3 spawnLocalPosition = CellToLocalPosition(targetCell.x, height - 1 + spawnOffsetRows);
                        matched.GameObject.transform.localPosition = spawnLocalPosition;
                    }

                    MoveVisual(matched, targetLocalPosition);
                    nextMap[targetCell] = matched;
                }
            }

            // Destroy visuals not reused in the new snapshot.
            foreach (var kvp in _visualsByCell)
            {
                if (!reused.Contains(kvp.Value))
                {
                    if (kvp.Value.ActiveMove != null)
                    {
                        StopCoroutine(kvp.Value.ActiveMove);
                    }

                    if (kvp.Value.GameObject != null)
                    {
                        Destroy(kvp.Value.GameObject);
                    }
                }
            }

            _visualsByCell.Clear();
            foreach (var kvp in nextMap)
            {
                _visualsByCell[kvp.Key] = kvp.Value;
            }

            _lastSnapshot = (int[])flatCells.Clone();
        }

        public void ApplySnapshot(int width, int height, IReadOnlyList<int> flatCells)
        {
            if (flatCells == null) throw new ArgumentNullException(nameof(flatCells));

            int[] copy = new int[flatCells.Count];
            for (int i = 0; i < flatCells.Count; i++)
            {
                copy[i] = flatCells[i];
            }

            ApplySnapshot(width, height, copy);
        }

        private List<ColumnEntry>[] BuildOccupiedByColumn(int width, int height, int[] cells)
        {
            var result = new List<ColumnEntry>[width];
            for (int x = 0; x < width; x++)
            {
                result[x] = new List<ColumnEntry>();
                for (int y = 0; y < height; y++)
                {
                    int tileId = cells[y * width + x];
                    if (tileId >= 0)
                    {
                        result[x].Add(new ColumnEntry(y, tileId));
                    }
                }
            }
            return result;
        }

        private List<ColumnEntry>[] BuildPreviousOccupiedByColumn(int width, int height)
        {
            var result = new List<ColumnEntry>[width];
            for (int x = 0; x < width; x++)
            {
                result[x] = new List<ColumnEntry>();
            }

            foreach (var kvp in _visualsByCell)
            {
                Vector2Int cell = kvp.Key;
                TileVisual visual = kvp.Value;
                if (cell.x >= 0 && cell.x < width)
                {
                    result[cell.x].Add(new ColumnEntry(cell.y, visual.TileId, visual));
                }
            }

            for (int x = 0; x < width; x++)
            {
                result[x].Sort((a, b) => a.Row.CompareTo(b.Row));
            }

            return result;
        }

        private TileVisual TryMatchVisual(List<ColumnEntry> prevColumn, int tileId, HashSet<TileVisual> reused)
        {
            // Prefer same tile id first, then any non-reused visual in the column.
            for (int i = 0; i < prevColumn.Count; i++)
            {
                if (prevColumn[i].TileId == tileId &&
                    prevColumn[i].Visual != null &&
                    !reused.Contains(prevColumn[i].Visual))
                {
                    return prevColumn[i].Visual;
                }
            }

            for (int i = 0; i < prevColumn.Count; i++)
            {
                if (prevColumn[i].Visual != null && !reused.Contains(prevColumn[i].Visual))
                {
                    return prevColumn[i].Visual;
                }
            }

            return null;
        }

        private TileVisual CreateVisual(int tileId, int x, int y)
        {
            GameObject go;

            if (tilePrefab != null)
            {
                go = Instantiate(tilePrefab, boardRoot != null ? boardRoot : transform);
                go.name = $"Tile_{x}_{y}";
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = $"Tile_{x}_{y}";
                go.transform.SetParent(boardRoot != null ? boardRoot : transform, false);
                go.transform.localScale = tileScale;

                Collider col = go.GetComponent<Collider>();
                if (col != null)
                {
                    Destroy(col);
                }
            }

            var visual = new TileVisual
            {
                GameObject = go,
                TileId = tileId,
                Cell = new Vector2Int(x, y)
            };

            ApplyVisualStyle(visual, tileId);
            return visual;
        }

        private void ApplyVisualStyle(TileVisual visual, int tileId)
        {
            if (visual == null || visual.GameObject == null) return;

            Renderer renderer = visual.GameObject.GetComponent<Renderer>();
            if (renderer == null) return;

            if (!_colorCache.TryGetValue(tileId, out Color color))
            {
                float hue = Mathf.Abs((tileId * 0.137f) % 1f);
                color = Color.HSVToRGB(hue, 0.60f, 0.95f);
                _colorCache[tileId] = color;
            }

            Material material = renderer.material;
            material.color = color;
        }

        private void MoveVisual(TileVisual visual, Vector3 targetLocalPosition)
        {
            if (visual == null || visual.GameObject == null) return;

            if (visual.ActiveMove != null)
            {
                StopCoroutine(visual.ActiveMove);
                visual.ActiveMove = null;
            }

            if (!animateMovement)
            {
                visual.GameObject.transform.localPosition = targetLocalPosition;
                return;
            }

            visual.ActiveMove = StartCoroutine(AnimateMove(visual, targetLocalPosition));
        }

        private IEnumerator AnimateMove(TileVisual visual, Vector3 targetLocalPosition)
        {
            Transform t = visual.GameObject.transform;
            Vector3 start = t.localPosition;

            if ((start - targetLocalPosition).sqrMagnitude < 0.0001f)
            {
                t.localPosition = targetLocalPosition;
                visual.ActiveMove = null;
                yield break;
            }

            float duration = Mathf.Max(0.04f, Vector3.Distance(start, targetLocalPosition) / Mathf.Max(0.01f, moveSpeed));
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);
                t.localPosition = Vector3.Lerp(start, targetLocalPosition, alpha);
                yield return null;
            }

            t.localPosition = targetLocalPosition;
            visual.ActiveMove = null;
        }

        private Vector3 CellToLocalPosition(int x, int y)
        {
            return new Vector3(origin.x + x * tileSpacing, origin.y + y * tileSpacing, 0f);
        }

        private readonly struct ColumnEntry
        {
            public readonly int Row;
            public readonly int TileId;
            public readonly TileVisual Visual;

            public ColumnEntry(int row, int tileId, TileVisual visual = null)
            {
                Row = row;
                TileId = tileId;
                Visual = visual;
            }
        }
    }
}


