using UnityEditor;

namespace Game.EditorTools
{
    public sealed class BUXPuzzleTileArtImporter : AssetPostprocessor
    {
        private const string TileArtPath = "Assets/Game/Art/Tiles/";
        private const string BlockerArtPath = "Assets/Game/Art/Blockers/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(TileArtPath, System.StringComparison.Ordinal) &&
                !assetPath.StartsWith(BlockerArtPath, System.StringComparison.Ordinal))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteMeshType = SpriteMeshType.FullRect;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.filterMode = UnityEngine.FilterMode.Bilinear;
        }
    }
}
