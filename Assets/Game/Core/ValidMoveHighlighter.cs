using UnityEngine;
using System.Collections.Generic;
using Game.Presentation;

namespace Game.Core
{
    /// <summary>
    /// Absolute MVP affordance:
    /// Highlights ONLY swaps that will result in a match.
    /// This answers the player's core question:
    /// 'Which tiles can I actually swap?'
    /// </summary>
    public class ValidMoveHighlighter : MonoBehaviour
    {
        public Color highlight = new Color(1f, 1f, 0.3f, 1f);

        private readonly List<SpriteRenderer> _active = new();
        private SpriteRenderer _selected;
        private Color _selectedBase;
        private readonly Dictionary<SpriteRenderer, Color> _baseColors = new();

        public void OnTileSelected(SpriteRenderer tile)
        {
            Clear();

            if (tile == null) return;

            _selected = tile;
            _selectedBase = tile.color;

            foreach (var n in FindNeighbors(tile))
            {
                if (WouldMatch(tile, n))
                {
                    var r = n.GetComponent<SpriteRenderer>();
                    if (r == null) continue;

                    _active.Add(r);
                    _baseColors[r] = r.color;
                    r.color = highlight;
                }
            }
        }

        public void Clear()
        {
            foreach (var r in _active)
            {
                if (r != null && _baseColors.TryGetValue(r, out var baseColor)) r.color = baseColor;
            }
            _active.Clear();
            _baseColors.Clear();

            if (_selected != null)
                _selected.color = _selectedBase;

            _selected = null;
        }

        // --- Helpers ---

        private IEnumerable<GameObject> FindNeighbors(SpriteRenderer tile)
        {
            var pos = tile.transform.position;
            foreach (var other in FindObjectsOfType<SpriteRenderer>())
            {
                if (other == tile) continue;
                if (Vector2.Distance(pos, other.transform.position) < 1.1f)
                    yield return other.gameObject;
            }
        }

        private bool WouldMatch(SpriteRenderer a, GameObject bObj)
        {
            var aView = a.GetComponentInParent<TileView>();
            var bView = bObj.GetComponentInParent<TileView>();
            if (aView == null || bView == null) return false;

            var root = FindFirstObjectByType<global::GameRoot>();
            var board = root != null ? root.Board : null;
            if (board == null) return false;

            return board.WouldSwapCreateMatch(aView.X, aView.Y, bView.X, bView.Y);
        }
    }
}

