using UnityEngine;

namespace Game.Rules.Unity
{
    /// <summary>
    /// FINAL minimal affordance:
    /// Pulses ALL tiles (SpriteRenderers) while ANY rule is active.
    /// This matches match-3 mental model and is impossible to miss.
    /// </summary>
    public class RuleVisualIndicator : MonoBehaviour
    {
        [SerializeField] private Color ruleTint = new Color(1f, 0.9f, 0.4f, 1f);
        [SerializeField] private float pulseSpeed = 2.2f;
        [SerializeField] private float pulseStrength = 0.25f;

        private RuleRuntime _runtime;
        private SpriteRenderer[] _tiles;
        private Color[] _baseColors;

        void Awake()
        {
            _runtime = FindFirstObjectByType<RuleRuntime>();

            _tiles = FindObjectsOfType<SpriteRenderer>();
            _baseColors = new Color[_tiles.Length];

            for (int i = 0; i < _tiles.Length; i++)
                _baseColors[i] = _tiles[i].color;
        }

        void Update()
        {
            if (_runtime == null || _tiles == null) return;

            bool anyRules = GetActiveRuleCount(_runtime) > 0;

            if (!anyRules)
            {
                Restore();
                return;
            }

            float t = (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) * pulseStrength;

            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i] == null) continue;
                _tiles[i].color = Color.Lerp(_baseColors[i], ruleTint, t);
            }
        }

        private void Restore()
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i] == null) continue;
                _tiles[i].color = _baseColors[i];
            }
        }

        private static int GetActiveRuleCount(RuleRuntime rt)
        {
            if (rt == null) return 0;

            var t = rt.GetType();
            const System.Reflection.BindingFlags F =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic;

            var fManager = t.GetField("_manager", F);
            var manager = fManager != null ? fManager.GetValue(rt) : null;
            if (manager == null) return 0;

            var mt = manager.GetType();

            var pActive = mt.GetProperty("ActiveCount", F);
            if (pActive != null)
            {
                try { return (int)pActive.GetValue(manager); } catch {}
            }

            var fActive = mt.GetField("_active", F);
            if (fActive != null)
            {
                var list = fActive.GetValue(manager);
                if (list != null)
                {
                    var pCount = list.GetType().GetProperty("Count");
                    if (pCount != null)
                        try { return (int)pCount.GetValue(list); } catch {}
                }
            }

            return 0;
        }
    }
}