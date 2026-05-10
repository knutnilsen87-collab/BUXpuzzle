using UnityEngine;
using Game.Core;
using Game.Content;

namespace Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class TileView : MonoBehaviour
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Type { get; private set; }
        public TileState State { get; private set; }
        public CellBlockerType Blocker { get; private set; }

        private Transform _visualRoot;
        private SpriteRenderer _shadowRenderer;
        private SpriteRenderer _baseRenderer;
        private SpriteRenderer _symbolRenderer;
        private SpriteRenderer _specialOverlayRenderer;
        private SpriteRenderer _blockerFrameRenderer;
        private SpriteRenderer _blockerOverlayRenderer;
        private TileSetConfig _tileSet;
        private Vector3 _baseScale;
        private float _globalVisualScale = 1f;
        private const float AuthoredTileFitSize = 0.89f;
        private bool _selected;
        private int _hintLevel;
        private bool _resolveHighlight;
        private bool _clearing;
        private bool _landing;

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
            State = TileState.Normal;
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

        public void SetTileSet(TileSetConfig tileSet)
        {
            if (_tileSet == tileSet) return;
            _tileSet = tileSet;
            ApplyVisuals();
        }

        public void SetState(TileState state)
        {
            State = state;
            ApplyVisuals();
        }

        public void SetBlocker(CellBlockerType blocker)
        {
            Blocker = blocker;
            ApplyVisuals();
        }

        public void SetVisualScale(float scale)
        {
            _globalVisualScale = Mathf.Clamp(scale <= 0f ? 1f : scale, 0.72f, 1.18f);
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

        public void SetLanding(bool active)
        {
            _landing = active;
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            EnsureRenderers();

            if (_shadowRenderer != null)
            {
                _shadowRenderer.sprite = _tileSet != null && _tileSet.TileShadowSprite != null ? _tileSet.TileShadowSprite : NatureLightRuntimeArt.TileShadow();
                _shadowRenderer.color = _selected ? new Color(1f, 1f, 1f, 0.92f) : Color.white;
            }

            var visual = _tileSet != null ? _tileSet.Find((TileType)Type) : null;
            bool usesAuthoredTile = visual != null && visual.BaseSprite != null;
            float perTileScale = visual != null && visual.VisualScale > 0f ? visual.VisualScale : 1f;
            float visualFit = AuthoredTileFitSize * perTileScale;

            if (_baseRenderer != null)
            {
                _baseRenderer.sprite = usesAuthoredTile
                    ? (_selected && visual.HighlightSprite != null ? visual.HighlightSprite : visual.BaseSprite)
                    : NatureLightRuntimeArt.TileBase(Type, _selected);
                Color color = Color.white;
                if (_hintLevel > 0) color = Color.Lerp(color, new Color(1f, 0.95f, 0.48f, 1f), _hintLevel == 2 ? 0.45f : 0.25f);
                if (_resolveHighlight) color = Color.Lerp(color, new Color(1f, 1f, 0.72f, 1f), 0.35f);
                if (_landing) color = Color.Lerp(color, new Color(0.85f, 1f, 0.82f, 1f), 0.20f);
                if (State == TileState.Line) color = Color.Lerp(color, new Color(1f, 0.88f, 0.34f, 1f), 0.34f);
                if (State == TileState.Burst) color = Color.Lerp(color, new Color(1f, 0.58f, 0.78f, 1f), 0.34f);
                if (State == TileState.ColorBomb) color = Color.Lerp(color, new Color(0.86f, 0.78f, 1f, 1f), 0.42f);
                if (State == TileState.Frozen) color = Color.Lerp(color, new Color(0.55f, 0.84f, 0.62f, 1f), 0.46f);
                if (State == TileState.Locked) color = Color.Lerp(color, new Color(0.48f, 0.70f, 0.42f, 1f), 0.52f);
                if (State == TileState.Pebble) color = Color.Lerp(color, new Color(0.70f, 0.68f, 0.62f, 1f), 0.62f);
                if (State == TileState.Ice) color = Color.Lerp(color, new Color(0.66f, 0.92f, 1f, 1f), 0.50f);
                if (_clearing) color.a = 0.36f;
                _baseRenderer.color = color;
                FitRendererToCell(_baseRenderer, usesAuthoredTile ? visualFit : 1f);
            }

            if (_symbolRenderer != null)
            {
                _symbolRenderer.sprite = usesAuthoredTile ? null : NatureLightRuntimeArt.TileSymbol(Type);
                Color symbol = _selected ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.94f);
                if (State == TileState.Line) symbol = new Color(1f, 0.98f, 0.72f, 1f);
                if (State == TileState.Burst) symbol = new Color(1f, 0.86f, 0.94f, 1f);
                if (State == TileState.ColorBomb) symbol = new Color(0.92f, 0.86f, 1f, 1f);
                if (State == TileState.Frozen) symbol = new Color(0.78f, 1f, 0.82f, 1f);
                if (State == TileState.Locked) symbol = new Color(0.68f, 0.92f, 0.62f, 1f);
                if (State == TileState.Pebble) symbol = new Color(0.92f, 0.88f, 0.78f, 1f);
                if (State == TileState.Ice) symbol = new Color(0.86f, 0.98f, 1f, 1f);
                _symbolRenderer.color = _clearing ? new Color(1f, 1f, 1f, 0.22f) : symbol;
                _symbolRenderer.transform.localScale = usesAuthoredTile ? Vector3.one : new Vector3(0.78f, 0.78f, 1f);
            }

            if (_specialOverlayRenderer != null)
            {
                _specialOverlayRenderer.sprite = _tileSet != null ? _tileSet.SpecialOverlay(State) : null;
                _specialOverlayRenderer.color = _clearing ? new Color(1f, 1f, 1f, 0.22f) : Color.white;
                FitRendererToCell(_specialOverlayRenderer, visualFit);
            }

            if (_blockerFrameRenderer != null)
            {
                _blockerFrameRenderer.sprite = Blocker == CellBlockerType.None ? null : NatureLightRuntimeArt.BlockerFrame(Blocker);
                _blockerFrameRenderer.color = _clearing ? new Color(1f, 1f, 1f, 0.28f) : Color.white;
                FitRendererToCell(_blockerFrameRenderer, 1.04f);
            }

            if (_blockerOverlayRenderer != null)
            {
                _blockerOverlayRenderer.sprite = _tileSet != null ? _tileSet.BlockerSprite(Blocker) : null;
                _blockerOverlayRenderer.color = _clearing ? new Color(1f, 1f, 1f, 0.26f) : new Color(1f, 1f, 1f, 0.78f);
                FitRendererToCell(_blockerOverlayRenderer, 0.98f);
            }

            float scale = 1f;
            if (_selected) scale = 1.07f;
            if (_hintLevel == 1) scale = Mathf.Max(scale, 1.08f);
            if (_hintLevel == 2) scale = Mathf.Max(scale, 1.12f + Mathf.Sin(Time.unscaledTime * 5f) * 0.025f);
            if (_resolveHighlight) scale = Mathf.Max(scale, 1.12f);
            if (_landing) scale = 0.94f;
            if (_clearing) scale = 0.74f;
            var finalScale = Vector3.one * (_globalVisualScale * scale);
            if (_landing) finalScale = new Vector3(finalScale.x * 1.05f, finalScale.y * 0.92f, finalScale.z);
            if (_visualRoot != null) _visualRoot.localScale = finalScale;
            transform.localScale = _baseScale;
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
            _visualRoot = EnsureVisualRoot();
            _shadowRenderer = EnsureSpriteRenderer("TileShadow", -3, new Vector3(0.045f, -0.075f, 0f), _shadowRenderer);
            _baseRenderer = EnsureSpriteRenderer("TileBase", 0, Vector3.zero, _baseRenderer);
            _symbolRenderer = EnsureSpriteRenderer("TileSymbol", 2, new Vector3(0f, 0.01f, 0f), _symbolRenderer);
            _specialOverlayRenderer = EnsureSpriteRenderer("SpecialOverlay", 3, Vector3.zero, _specialOverlayRenderer);
            _blockerFrameRenderer = EnsureSpriteRenderer("BlockerFrame", 4, Vector3.zero, _blockerFrameRenderer);
            _blockerOverlayRenderer = EnsureSpriteRenderer("BlockerOverlay", 5, Vector3.zero, _blockerOverlayRenderer);

            if (_shadowRenderer != null) _shadowRenderer.transform.localScale = new Vector3(1.02f, 0.86f, 1f);
            if (_baseRenderer != null && _baseRenderer.sprite == null) _baseRenderer.transform.localScale = Vector3.one;
            if (_symbolRenderer != null) _symbolRenderer.transform.localScale = new Vector3(0.78f, 0.78f, 1f);
            if (_specialOverlayRenderer != null && _specialOverlayRenderer.sprite == null) _specialOverlayRenderer.transform.localScale = new Vector3(0.94f, 0.94f, 1f);
            if (_blockerFrameRenderer != null && _blockerFrameRenderer.sprite == null) _blockerFrameRenderer.transform.localScale = Vector3.one;
            if (_blockerOverlayRenderer != null && _blockerOverlayRenderer.sprite == null) _blockerOverlayRenderer.transform.localScale = new Vector3(0.94f, 0.94f, 1f);
        }

        private Transform EnsureVisualRoot()
        {
            var root = transform.Find("VisualRoot");
            if (root == null)
            {
                var go = new GameObject("VisualRoot");
                root = go.transform;
                root.SetParent(transform, false);
            }

            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;
            return root;
        }

        private static void FitRendererToCell(SpriteRenderer renderer, float targetSize)
        {
            if (renderer == null || renderer.sprite == null) return;

            var size = renderer.sprite.bounds.size;
            float largest = Mathf.Max(size.x, size.y);
            if (largest <= 0.0001f) return;

            float scale = targetSize / largest;
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private SpriteRenderer EnsureSpriteRenderer(string childName, int sortingOrder, Vector3 localPosition, SpriteRenderer current)
        {
            if (current != null) return current;

            var child = _visualRoot != null ? _visualRoot.Find(childName) : null;
            if (child == null) child = transform.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName);
                child = go.transform;
                child.SetParent(_visualRoot != null ? _visualRoot : transform, false);
            }
            else if (_visualRoot != null && child.parent != _visualRoot)
            {
                child.SetParent(_visualRoot, false);
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
