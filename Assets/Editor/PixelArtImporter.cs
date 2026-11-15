using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace EditorTools
{
    /// <summary>
    /// Imports PNGs under Assets/Art/Sprites as crisp pixel-art sprites and slices horizontal strips into square frames.
    /// </summary>
    public class PixelArtImporter : AssetPostprocessor
    {
        private const string PixelArtFolder = "Assets/Art/Sprites/";
        private const int PixelsPerUnit = 16;

        /// <summary>
        /// Applies pixel-art settings on first import and keeps strip slicing in step with the image's width.
        /// </summary>
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(PixelArtFolder)) return;
            var importer = (TextureImporter)assetImporter;
            importer.GetSourceTextureWidthAndHeight(out var width, out var height);
            if (importer.importSettingsMissing) ApplyPixelArtSettings(importer, Mathf.Max(width, height));
            var isStrip = height > 0 && width > height && width % height == 0;
            importer.spriteImportMode = isStrip ? SpriteImportMode.Multiple : SpriteImportMode.Single;
            if (isStrip) SliceStrip(importer, width / height, height);
        }

        /// <summary>
        /// Point filtering, no mipmaps, no compression, a 16 pixels-per-unit sprite and the smallest max size that keeps every pixel.
        /// </summary>
        private static void ApplyPixelArtSettings(TextureImporter importer, int largestSide)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.maxTextureSize = Mathf.Clamp(Mathf.NextPowerOfTwo(largestSide), 32, 8192);
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
        }

        /// <summary>
        /// Cuts a horizontal strip into square frames named "file_0", "file_1"..., keeping existing sprite IDs so references survive re-slicing.
        /// </summary>
        private static void SliceStrip(TextureImporter importer, int frameCount, int frameSize)
        {
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();
            var existing = dataProvider.GetSpriteRects();
            if (IsSlicedAsStrip(existing, frameCount, frameSize)) return;

            var baseName = System.IO.Path.GetFileNameWithoutExtension(importer.assetPath);
            var spriteRects = new SpriteRect[frameCount];
            var nameFileIdPairs = new List<SpriteNameFileIdPair>(frameCount);
            for (var i = 0; i < frameCount; i++)
            {
                var spriteRect = i < existing.Length ? existing[i] : new SpriteRect();
                spriteRect.name = $"{baseName}_{i}";
                spriteRect.rect = new Rect(i * frameSize, 0, frameSize, frameSize);
                spriteRect.alignment = SpriteAlignment.Center;
                spriteRect.pivot = new Vector2(0.5f, 0.5f);
                spriteRects[i] = spriteRect;
                nameFileIdPairs.Add(new SpriteNameFileIdPair(spriteRect.name, spriteRect.spriteID));
            }
            dataProvider.SetSpriteRects(spriteRects);
            dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(nameFileIdPairs);
            dataProvider.Apply();
        }

        /// <summary>
        /// Whether the existing sprite rects already match the strip's frame count and frame size.
        /// </summary>
        private static bool IsSlicedAsStrip(SpriteRect[] existing, int frameCount, int frameSize)
        {
            if (existing == null || existing.Length != frameCount) return false;
            for (var i = 0; i < frameCount; i++)
            {
                if (existing[i].rect != new Rect(i * frameSize, 0, frameSize, frameSize)) return false;
            }
            return true;
        }
    }
}
