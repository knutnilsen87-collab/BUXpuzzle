using Game.Core;
using UnityEngine;

namespace Game.Content
{
    [CreateAssetMenu(menuName = "BUXPuzzle/Tiles/Tile Set")]
    public sealed class TileSetConfig : ScriptableObject
    {
        public TileVisualConfig[] Tiles;
        public Sprite LineHorizontalOverlay;
        public Sprite LineVerticalOverlay;
        public Sprite BloomBombOverlay;
        public Sprite ColorClearOverlay;
        public BlockerVisualConfig[] Blockers;
        public Sprite TileShadowSprite;
        public Sprite CellSlotSprite;

        public TileVisualConfig Find(TileType type)
        {
            if (Tiles == null) return null;

            for (int i = 0; i < Tiles.Length; i++)
            {
                var tile = Tiles[i];
                if (tile != null && tile.Type == type)
                {
                    return tile;
                }
            }

            return null;
        }

        public Sprite SpecialOverlay(TileState state)
        {
            switch (state)
            {
                case TileState.Line:
                    return LineHorizontalOverlay;
                case TileState.Burst:
                    return BloomBombOverlay;
                case TileState.ColorBomb:
                    return ColorClearOverlay;
                default:
                    return null;
            }
        }

        public Sprite BlockerSprite(CellBlockerType blocker)
        {
            if (Blockers == null) return null;

            for (int i = 0; i < Blockers.Length; i++)
            {
                var entry = Blockers[i];
                if (entry != null && entry.Type == blocker)
                {
                    return entry.Sprite;
                }
            }

            return null;
        }
    }

    [System.Serializable]
    public sealed class TileVisualConfig
    {
        public TileType Type;
        public string DisplayName;
        public Sprite BaseSprite;
        public float VisualScale = 1f;
        public Sprite HighlightSprite;
        public Sprite MatchedSprite;
        public Sprite DisabledSprite;
        public AudioClip TapSound;
        public AudioClip MatchSound;
        public Color AccentColor = Color.white;
        public string AccessibilityLabel;
    }

    [System.Serializable]
    public sealed class BlockerVisualConfig
    {
        public CellBlockerType Type;
        public string DisplayName;
        public Sprite Sprite;
        public Color AccentColor = Color.white;
    }
}
