using UnityEngine;

namespace Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class TileView : MonoBehaviour
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Type { get; private set; }

        private Renderer _renderer;
        private Vector3 _baseScale;
        private bool _selected;
        private Color _baseColor;

        void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _baseScale = transform.localScale;
            _baseColor = (_renderer != null) ? _renderer.material.color : Color.white;
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

        private void ApplyVisuals()
        {
            if (_renderer != null)
            {
                Color c = Type switch
                {
                    0 => new Color(0.25f, 0.55f, 1.00f),
                    1 => new Color(1.00f, 0.85f, 0.20f),
                    2 => new Color(1.00f, 0.30f, 0.30f),
                    3 => new Color(0.25f, 0.85f, 0.35f),
                    4 => new Color(0.70f, 0.35f, 1.00f),
                    _ => new Color(1.00f, 0.55f, 0.20f),
                };

                _baseColor = c;
                var mat = _renderer.material;
                mat.color = _selected ? (c * 1.25f) : c;
            }

            transform.localScale = _selected ? (_baseScale * 1.18f) : _baseScale;
        }
    }
}
