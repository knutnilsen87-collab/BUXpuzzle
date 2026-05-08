using UnityEngine;

namespace Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class TileView : MonoBehaviour
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Type { get; private set; }

        private SpriteRenderer _shadowRenderer;
        private SpriteRenderer _baseRenderer;
        private SpriteRenderer _symbolRenderer;
        private Vector3 _baseScale;
        private bool _selected;
        private int _hintLevel;
        private bool _resolveHighlight;
        private bool _clearing;

        void Awake()
        {
            HidePlaceholderMeshes();
            EnsureRenderers();
            _baseScale = transform.localScale;
        }

        void Update()
        {
            if (_hintLevel == 2)
            {
                ApplyVisuals();
            }
        }

        public void Init(int x, int y, int type)
        {
            X = x; Y = y; SetType(type);
        }

        public void SetCoords(int x, int y)
        {
            X = x; Y = y;
        }

        public void SetType(int type)
        {
            Type = Mathf.Clamp(type, 0, 5);
            ApplyVisuals();
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            ApplyVisuals();
        }

        public void SetTutorialHint(bool active, bool strong)
        {
            _hintLevel = active ? (strong ? 2 : 1) : 0;
            ApplyVisuals();
        }

        public void SetResolveHighlight(bool active)
        {
            _resolveHighlight = active;
            ApplyVisuals();
        }

        public void SetClearing(bool active)
        {
            _clearing = active;
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            EnsureRenderers();

            if (_shadowRenderer != null)
            {
                _shadowRenderer.sprite = NatureLightRuntimeArt.TileShadow();
                _shadowRenderer.color = _selected ? new Color(1f, 1f, 1f, 0.92f) : Color.white;
            }

            if (_baseRenderer != null)
            {
                _baseRenderer.sprite = NatureLightRuntimeArt.TileBase(Type, _selected);
                Color color = Color.white;
                if (_hintLevel > 0) color = Color.Lerp(color, new Color(1f, 0.95f, 0.48f, 1f), _hintLevel == 2 ? 0.45f : 0.25f);
                if (_resolveHighlight) color = Color.Lerp(color, new Color(1f, 1f, 0.72f, 1f), 0.35f);
                if (_clearing) color.a = 0.36f;
                _baseRenderer.color = color;
            }

            if (_symbolRenderer != null)
            {
                _symbolRenderer.sprite = NatureLightRuntimeArt.TileSymbol(Type);
                _symbolRenderer.color = _clearing
                    ? new Color(1f, 1f, 1f, 0.22f)
                    : (_selected ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.94f));
            }

            float scale = 1f;
            if (_selected) scale = 1.18f;
            if (_hintLevel == 1) scale = Mathf.Max(scale, 1.10f);
            if (_hintLevel == 2) scale = Mathf.Max(scale, 1.20f + Mathf.Sin(Time.unscaledTime * 5f) * 0.035f);
            if (_resolveHighlight) scale = Mathf.Max(scale, 1.14f);
            if (_clearing) scale = 0.74f;
            transform.localScale = _baseScale * scale;
        }

        private void HidePlaceholderMeshes()
        {
            var meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = false;
            }
        }

        private void EnsureRenderers()
        {
            _shadowRenderer = EnsureSpriteRenderer("TileShadow", -3, new Vector3(0.04f, -0.06f, 0f), _shadowRenderer);
            _baseRenderer = EnsureSpriteRenderer("TileBase", 0, Vector3.zero, _baseRenderer);
            _symbolRenderer = EnsureSpriteRenderer("TileSymbol", 2, new Vector3(0f, 0.01f, 0f), _symbolRenderer);

            if (_shadowRenderer != null) _shadowRenderer.transform.localScale = new Vector3(1.08f, 1.00f, 1f);
            if (_baseRenderer != null) _baseRenderer.transform.localScale = Vector3.one;
            if (_symbolRenderer != null) _symbolRenderer.transform.localScale = new Vector3(0.78f, 0.78f, 1f);
        }

        private SpriteRenderer EnsureSpriteRenderer(string childName, int sortingOrder, Vector3 localPosition, SpriteRenderer current)
        {
            if (current != null) return current;

            var child = transform.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName);
                child = go.transform;
                child.SetParent(transform, false);
            }

            child.localPosition = localPosition;
            child.localRotation = Quaternion.identity;

            var renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = child.gameObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }
    }
}
