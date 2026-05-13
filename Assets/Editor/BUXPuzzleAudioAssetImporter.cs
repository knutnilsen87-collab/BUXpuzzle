using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public sealed class BUXPuzzleAudioAssetImporter : AssetPostprocessor
    {
        private const string AudioPath = "Assets/Audio/";

        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(AudioPath, System.StringComparison.Ordinal))
            {
                return;
            }

            var importer = (AudioImporter)assetImporter;
            bool isLongLoop = assetPath.Contains("/mus_") || assetPath.Contains("/amb_");
            importer.forceToMono = !isLongLoop;
            importer.loadInBackground = isLongLoop;

            var settings = importer.defaultSampleSettings;
            settings.loadType = isLongLoop ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = isLongLoop ? 0.72f : 0.82f;
            settings.preloadAudioData = !isLongLoop;
            importer.defaultSampleSettings = settings;
        }
    }
}
