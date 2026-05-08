using UnityEngine;

namespace Game.Presentation.NatureLight
{
    [CreateAssetMenu(
        fileName = "FBL_NatureLightTheme",
        menuName = "FBL/Presentation/Nature Light Theme")]
    public sealed class FBL_NatureLightTheme : ScriptableObject
    {
        [Header("Tile base colors")]
        public Color leafBase = Hex("5BCB77");
        public Color dropBase = Hex("63A9F6");
        public Color sunBase = Hex("F2DF63");
        public Color flowerBase = Hex("F2A65A");
        public Color crystalBase = Hex("B57AE8");
        public Color berryBase = Hex("F46D78");

        [Header("Tile shadow colors")]
        public Color leafShadow = Hex("3C9B55");
        public Color dropShadow = Hex("3E78C4");
        public Color sunShadow = Hex("C7AE35");
        public Color flowerShadow = Hex("C97A2E");
        public Color crystalShadow = Hex("7E4FB8");
        public Color berryShadow = Hex("C74955");

        [Header("Tile highlight colors")]
        public Color leafHighlight = Hex("A6F0B7");
        public Color dropHighlight = Hex("BFE1FF");
        public Color sunHighlight = Hex("FFF3A8");
        public Color flowerHighlight = Hex("FFD1A1");
        public Color crystalHighlight = Hex("E4C6FF");
        public Color berryHighlight = Hex("FFB7C0");

        [Header("Board / background")]
        public Color topSky = Hex("DDEBFF");
        public Color midHaze = Hex("EEF7F2");
        public Color bottomGround = Hex("D9D1C5");
        public Color boardTint = new Color(1f, 1f, 1f, 0.10f);
        public Color boardBorder = new Color(1f, 1f, 1f, 0.20f);
        public Color boardInnerShadow = new Color(36f / 255f, 48f / 255f, 74f / 255f, 0.12f);

        [Header("Shape / layout")]
        [Range(0.16f, 0.24f)] public float tileCornerRadius = 0.20f;
        [Range(0.08f, 0.14f)] public float outlineOpacity = 0.10f;
        [Range(0.12f, 0.18f)] public float contactShadowOpacity = 0.15f;
        [Range(0.14f, 0.20f)] public float highlightOpacity = 0.16f;
        [Range(0.10f, 0.10f)] public float boardPanelAlpha = 0.10f;

        [Header("Motion")]
        [Range(0.08f, 0.12f)] public float selectInSeconds = 0.09f;
        [Range(0.10f, 0.14f)] public float selectOutSeconds = 0.12f;
        [Range(0.14f, 0.18f)] public float swapSeconds = 0.16f;
        [Range(0.12f, 0.18f)] public float invalidSwapSeconds = 0.14f;
        [Range(0.14f, 0.22f)] public float clearSeconds = 0.18f;
        [Range(0.04f, 0.08f)] public float emptyBeatSeconds = 0.05f;
        [Range(0.12f, 0.22f)] public float fallSeconds = 0.16f;
        [Range(0.10f, 0.18f)] public float spawnSeconds = 0.12f;
        [Range(0.04f, 0.08f)] public float landingBounce = 0.06f;

        [Header("Intensity budget")]
        [Range(0.05f, 0.10f)] public float idleGlow = 0.06f;
        [Range(0.15f, 0.25f)] public float selectGlow = 0.18f;
        [Range(0.30f, 0.45f)] public float matchGlow = 0.36f;
        [Range(0.35f, 0.60f)] public float specialGlow = 0.42f;

        public Color GetBase(FBL_TileKind kind)
        {
            switch (kind)
            {
                case FBL_TileKind.Leaf: return leafBase;
                case FBL_TileKind.Drop: return dropBase;
                case FBL_TileKind.Sun: return sunBase;
                case FBL_TileKind.Flower: return flowerBase;
                case FBL_TileKind.Crystal: return crystalBase;
                case FBL_TileKind.Berry: return berryBase;
                default: return Color.white;
            }
        }

        public Color GetShadow(FBL_TileKind kind)
        {
            switch (kind)
            {
                case FBL_TileKind.Leaf: return leafShadow;
                case FBL_TileKind.Drop: return dropShadow;
                case FBL_TileKind.Sun: return sunShadow;
                case FBL_TileKind.Flower: return flowerShadow;
                case FBL_TileKind.Crystal: return crystalShadow;
                case FBL_TileKind.Berry: return berryShadow;
                default: return Color.gray;
            }
        }

        public Color GetHighlight(FBL_TileKind kind)
        {
            switch (kind)
            {
                case FBL_TileKind.Leaf: return leafHighlight;
                case FBL_TileKind.Drop: return dropHighlight;
                case FBL_TileKind.Sun: return sunHighlight;
                case FBL_TileKind.Flower: return flowerHighlight;
                case FBL_TileKind.Crystal: return crystalHighlight;
                case FBL_TileKind.Berry: return berryHighlight;
                default: return Color.white;
            }
        }

        private static Color Hex(string value)
        {
            if (ColorUtility.TryParseHtmlString("#" + value, out var color))
            {
                return color;
            }

            return Color.magenta;
        }
    }
}
