using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    /// <summary>
    /// Imports WAVs under Assets/Audio/SFX as small, instantly playable mono effects for the web build.
    /// </summary>
    public class SoundEffectImporter : AssetPostprocessor
    {
        private const string SoundEffectsFolder = "Assets/Audio/SFX/";

        /// <summary>
        /// On first import only, so later hand-tuning sticks: forces mono, preloads and decompresses on load so a clip plays without delay, and compresses to Vorbis.
        /// </summary>
        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(SoundEffectsFolder)) return;
            var importer = (AudioImporter)assetImporter;
            if (!importer.importSettingsMissing) return;
            importer.forceToMono = true;
            importer.loadInBackground = false;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.7f;
            settings.preloadAudioData = true;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings = settings;
        }
    }
}
