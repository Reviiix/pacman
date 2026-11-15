using UnityEngine;
using UnityEngine.UI;

namespace Visuals
{
    /// <summary>
    /// Darkens the bottom screen row of every art pixel to fake a CRT scanline, using a tiny texture rebuilt when the canvas scale changes (save its RawImage transparent; it turns itself on).
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class ScanlineOverlay : MonoBehaviour
    {
        [SerializeField] [Range(0f, 1f)] private float darkness = 0.25f;
        private RawImage rawImage;
        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private Texture2D scanlineTexture;
        private int textureScale;

        /// <summary>
        /// Caches components and builds the overlay for the current scale.
        /// </summary>
        private void OnEnable()
        {
            rawImage = GetComponent<RawImage>();
            rectTransform = (RectTransform)transform;
            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
            rawImage.raycastTarget = false;
            Refresh();
        }

        /// <summary>
        /// Rebuilds the overlay when the canvas is resized or rescaled.
        /// </summary>
        private void OnRectTransformDimensionsChange()
        {
            if (rawImage == null) return;
            Refresh();
        }

        /// <summary>
        /// Releases the generated texture.
        /// </summary>
        private void OnDestroy()
        {
            if (scanlineTexture != null) Destroy(scanlineTexture);
        }

        /// <summary>
        /// Tiles the scanline texture once per art pixel, hiding the overlay at 1x where there is no room for a scanline.
        /// </summary>
        private void Refresh()
        {
            var scale = Mathf.Max(1, Mathf.RoundToInt(rootCanvas.scaleFactor));
            rawImage.enabled = scale > 1 && darkness > 0f;
            if (!rawImage.enabled) return;
            if (scale != textureScale) BuildTexture(scale);
            var size = rectTransform.rect.size;
            rawImage.uvRect = new Rect(0f, 0f, size.x, size.y);
        }

        /// <summary>
        /// Creates a 1 x scale texture whose bottom row is dark, so each art pixel gets one dark screen row.
        /// </summary>
        private void BuildTexture(int scale)
        {
            if (scanlineTexture != null) Destroy(scanlineTexture);
            scanlineTexture = new Texture2D(1, scale, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                name = "Scanlines (generated)",
            };
            var pixels = new Color32[scale];
            for (var row = 0; row < scale; row++)
            {
                pixels[row] = row == 0 ? new Color32(0, 0, 0, (byte)Mathf.RoundToInt(darkness * 255f)) : new Color32(0, 0, 0, 0);
            }
            scanlineTexture.SetPixels32(pixels);
            scanlineTexture.Apply(false, true);
            rawImage.texture = scanlineTexture;
            rawImage.color = Color.white;
            textureScale = scale;
        }
    }
}
