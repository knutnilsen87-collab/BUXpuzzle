using UnityEngine;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Candy Crush–style onboarding:
    /// First move is forced, highlighted, and demonstrated.
    /// After first successful swap, normal play unlocks.
    /// </summary>
    public class ForcedFirstMove : MonoBehaviour
    {
        private bool _done = false;
        private SpriteRenderer _a, _b;
        private float _t;

        void Start()
        {
            FindForcedMove();
        }

        void Update()
        {
            if (_done || _a == null || _b == null) return;

            _t += Time.deltaTime * 4f;
            float s = Mathf.Sin(_t) * 0.1f;

            _a.transform.localPosition += Vector3.right * s;
            _b.transform.localPosition += Vector3.left * s;
        }

        public bool IsInputAllowed(SpriteRenderer a, SpriteRenderer b)
        {
            if (_done) return true;
            return (a == _a && b == _b) || (a == _b && b == _a);
        }

        public void OnSuccessfulSwap()
        {
            _done = true;
            ResetPositions();
        }

        private void FindForcedMove()
        {
            var tiles = FindObjectsOfType<SpriteRenderer>();
            foreach (var a in tiles)
            {
                foreach (var b in tiles)
                {
                    if (a == b) continue;
                    if (Vector2.Distance(a.transform.position, b.transform.position) > 1.1f) continue;
                    if (a.color == b.color)
                    {
                        _a = a;
                        _b = b;
                        return;
                    }
                }
            }
        }

        private void ResetPositions()
        {
            if (_a != null) _a.transform.localPosition = Vector3.zero;
            if (_b != null) _b.transform.localPosition = Vector3.zero;
        }
    }
}

