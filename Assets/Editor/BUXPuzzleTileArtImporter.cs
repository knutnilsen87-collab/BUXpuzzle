using UnityEditor;

namespace Game.EditorTools
{
    public sealed class BUXPuzzleTileArtImporter : AssetPostprocessor
    {
        private const string TileArtPath = "Assets/Game/Art/Tiles/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(TileArtPath, System.StringComparison.Ordinal))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.filterMode = UnityEngine.FilterMode.Bilinear;
        }
    }
}
