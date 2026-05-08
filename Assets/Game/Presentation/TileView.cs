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
        private TextMesh _symbol;
        private Vector3 _baseScale;
        private bool _selected;
        private Color _baseColor;

        void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _symbol = GetComponentInChildren<TextMesh>();
            if (_symbol == null)
            {
                var symbolGo = new GameObject("TileSymbol");
                symbolGo.transform.SetParent(transform, false);
                symbolGo.transform.localPosition = new Vector3(0f, 0f, -0.56f);
                symbolGo.transform.localRotation = Quaternion.identity;
                _symbol = symbolGo.AddComponent<TextMesh>();
                _symbol.anchor = TextAnchor.MiddleCenter;
                _symbol.alignment = TextAlignment.Center;
                _symbol.characterSize = 0.34f;
                _symbol.fontSize = 48;
                _symbol.color = new Color(0.08f, 0.12f, 0.16f, 1f);
            }
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
                    0 => new Color(0.36f, 0.80f, 0.47f),
                    1 => new Color(0.39f, 0.66f, 0.96f),
                    2 => new Color(0.95f, 0.87f, 0.39f),
                    3 => new Color(0.95f, 0.65f, 0.35f),
                    4 => new Color(0.71f, 0.48f, 0.91f),
                    _ => new Color(0.96f, 0.43f, 0.47f),
                };

                _baseColor = c;
                var mat = _renderer.material;
                mat.color = _selected ? (c * 1.25f) : c;
            }

            if (_symbol != null)
            {
                _symbol.text = Type switch
                {
                    0 => "L",
                    1 => "D",
                    2 => "S",
                    3 => "F",
                    4 => "C",
                    _ => "B",
                };
            }

            transform.localScale = _selected ? (_baseScale * 1.18f) : _baseScale;
        }
    }
}
