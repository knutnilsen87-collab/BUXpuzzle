using UnityEngine;

namespace Game.Presentation.NatureLight
{
    public sealed class FBL_TileVisualBinder : MonoBehaviour
    {
        [SerializeField] private FBL_NatureLightTheme theme;
        [SerializeField] private FBL_TileKind tileKind;

        [Header("Renderers")]
        [SerializeField] private SpriteRenderer baseRenderer;
        [SerializeField] private SpriteRenderer symbolRenderer;

        private void Awake()
        {
            if (baseRenderer == null)
            {
                baseRenderer = GetComponent<SpriteRenderer>();
            }

            if (symbolRenderer == null)
            {
                Transform child = transform.Find("Symbol");
                if (child != null)
                {
                    symbolRenderer = child.GetComponent<SpriteRenderer>();
                }
            }
        }

        private void OnEnable()
        {
            ApplyTheme();
        }

        public void SetTileKind(FBL_TileKind kind)
        {
            tileKind = kind;
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (theme == null) return;

            if (baseRenderer != null)
            {
                baseRenderer.sprite = LoadSprite(GetBaseName(tileKind));
                baseRenderer.color = theme.GetBase(tileKind);
                baseRenderer.sortingOrder = 0;
            }

            if (symbolRenderer != null)
            {
                symbolRenderer.sprite = LoadSprite(GetSymbolName(tileKind));
                symbolRenderer.color = theme.GetHighlight(tileKind);
                symbolRenderer.sortingOrder = 1;
            }
        }

        private static string GetBaseName(FBL_TileKind kind)
        {
            switch (kind)
            {
                case FBL_TileKind.Leaf: return "Leaf_Base";
                case FBL_TileKind.Drop: return "Drop_Base";
                case FBL_TileKind.Sun: return "Sun_Base";
                case FBL_TileKind.Flower: return "Flower_Base";
                case FBL_TileKind.Crystal: return "Crystal_Base";
                case FBL_TileKind.Berry: return "Berry_Base";
                default: return "Leaf_Base";
            }
        }

        private static string GetSymbolName(FBL_TileKind kind)
        {
            switch (kind)
            {
                case FBL_TileKind.Leaf: return "Leaf_Symbol";
                case FBL_TileKind.Drop: return "Drop_Symbol";
                case FBL_TileKind.Sun: return "Sun_Symbol";
                case FBL_TileKind.Flower: return "Flower_Symbol";
                case FBL_TileKind.Crystal: return "Crystal_Symbol";
                case FBL_TileKind.Berry: return "Berry_Symbol";
                default: return "Leaf_Symbol";
            }
        }

        private static Sprite LoadSprite(string name)
        {
            return Resources.Load<Sprite>("Sprites/" + name);
        }
    }
}
