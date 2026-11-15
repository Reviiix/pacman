using UnityEngine;
using UnityEngine.UI;

namespace Visuals
{
    /// <summary>
    /// Darkens the corners of its rect with a soft radial gradient generated at runtime, so no point-filtered asset bands it (save its RawImage transparent; it turns itself on).
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class VignetteOverlay : MonoBehaviour
    {
        [SerializeField] private Color color = new(0.078f, 0.043f, 0.149f, 1f);
        [SerializeField] [Range(0f, 1f)] private float cornerAlpha = 0.25f;
        [SerializeField] [Range(0f, 1f)] private float fadeStart = 0.45f;
        [SerializeField] private Vector2Int textureSize = new(68, 88);
        private Texture2D vignetteTexture;

        /// <summary>
        /// Builds the gradient texture and shows it on the RawImage.
        /// </summary>
        private void OnEnable()
        {
            var rawImage = GetComponent<RawImage>();
            rawImage.raycastTarget = false;
            if (vignetteTexture == null) vignetteTexture = BuildTexture();
            rawImage.texture = vignetteTexture;
            rawImage.color = Color.white;
        }

        /// <summary>
        /// Releases the generated texture.
        /// </summary>
        private void OnDestroy()
        {
            if (vignetteTexture != null) Destroy(vignetteTexture);
        }

        /// <summary>
        /// Fills a small texture whose alpha eases from clear at the fade start to the corner alpha at the corners.
        /// </summary>
        private Texture2D BuildTexture()
        {
            var width = Mathf.Max(2, textureSize.x);
            var height = Mathf.Max(2, textureSize.y);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "Vignette (generated)",
            };
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var dx = (x + 0.5f) / width * 2f - 1f;
                    var dy = (y + 0.5f) / height * 2f - 1f;
                    var distance = Mathf.Sqrt(dx * dx + dy * dy) / Mathf.Sqrt(2f);
                    var t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(fadeStart, 1f, distance));
                    var pixel = color;
                    pixel.a = cornerAlpha * t;
                    pixels[y * width + x] = pixel;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
