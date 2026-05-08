using UnityEngine;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Candy Crush-level affordance:
    /// - highlight ONLY valid neighbor swaps (from current selection)
    /// - auto-clears on: empty click, click elsewhere, timeout
    /// This prevents "stuck" indicators even if input code forgets to call Clear().
    /// </summary>
    public class CandyCrushMoveGuide : MonoBehaviour
    {
        [Header("Visual")]
        public Color validColor = new Color(1f, 1f, 0.2f, 1f);
        public Color selectedColor = new Color(1f, 1f, 1f, 1f);

        [Header("Behavior")]
        [Tooltip("Auto-clear highlight after this many seconds with no re-selection.")]
        public float autoClearSeconds = 1.25f;

        private SpriteRenderer _selected;
        private readonly List<SpriteRenderer> _valid = new();
        private readonly Dictionary<SpriteRenderer, Color> _base = new();
        private float _lastSelectTime;

        void Update()
        {
            // Timeout safety
            if (_selected != null && autoClearSeconds > 0f)
            {
                if (Time.time - _lastSelectTime >= autoClearSeconds)
                    Clear();
            }

            // Click-safety: if user clicks "nothing" or something else, clear.
            if (_selected != null && Input.GetMouseButtonDown(0))
            {
                var hit = RaycastSpriteUnderMouse();
                if (hit == null)
                {
                    Clear();
                    return;
                }

                // If click is neither the selected tile nor one of the valid neighbors => clear.
                if (hit != _selected && !_valid.Contains(hit))
                {
                    Clear();
                    return;
                }

                // If click IS a valid neighbor, we still clear now.
                // The swap system will handle the swap; we just remove guidance overlay immediately.
                if (_valid.Contains(hit))
                {
                    Clear();
                    return;
                }
            }
        }

        public void OnTileSelected(SpriteRenderer tile)
        {
            Clear();

            if (tile == null) return;
            _selected = tile;
            _lastSelectTime = Time.time;

            // store and tint selected lightly (optional)
            if (!_base.ContainsKey(tile)) _base[tile] = tile.color;
            tile.color = selectedColor;

            foreach (var n in FindNeighbors(tile))
            {
                var r = n.GetComponent<SpriteRenderer>();
                if (r == null) continue;

                if (WouldMatch(tile, r))
                {
                    _valid.Add(r);
                    if (!_base.ContainsKey(r)) _base[r] = r.color;
                    r.color = validColor;
                }
            }
        }

        public bool IsSwapAllowed(SpriteRenderer a, SpriteRenderer b)
        {
            if (_selected == null) return false;
            return (a == _selected && _valid.Contains(b)) ||
                   (b == _selected && _valid.Contains(a));
        }

        public void Clear()
        {
            foreach (var kv in _base)
            {
                if (kv.Key != null) kv.Key.color = kv.Value;
            }
            _valid.Clear();
            _base.Clear();
            _selected = null;
        }

        // --- helpers ---

        private SpriteRenderer RaycastSpriteUnderMouse()
        {
            var cam = Camera.main;
            if (cam == null) return null;

            var mp = Input.mousePosition;
            var wp = cam.ScreenToWorldPoint(mp);
            var p2 = new Vector2(wp.x, wp.y);

            // Use Physics2D if sprites have colliders; fallback: brute scan (safe).
            var hit2d = Physics2D.Raycast(p2, Vector2.zero);
            if (hit2d.collider != null)
            {
                return hit2d.collider.GetComponent<SpriteRenderer>();
            }

            // Fallback scan
            float best = float.MaxValue;
            SpriteRenderer bestR = null;
            foreach (var r in FindObjectsOfType<SpriteRenderer>())
            {
                var d = Vector2.Distance(p2, r.transform.position);
                if (d < 0.6f && d < best) { best = d; bestR = r; }
            }
            return bestR;
        }

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

        // IMPORTANT:
        // This should ideally call the SAME "would form match" logic as your board system.
        // For now we keep a safe MVP heuristic:
        // If you have a tile type id or sprite index, compare that instead of color.
        private bool WouldMatch(SpriteRenderer a, SpriteRenderer b)
        {
            return a.color == b.color;
        }
    }
}
