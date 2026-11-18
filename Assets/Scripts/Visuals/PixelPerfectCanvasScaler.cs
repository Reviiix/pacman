using UnityEngine;
using UnityEngine.UI;

namespace Visuals
{
    /// <summary>
    /// Scales a canvas by the largest whole number that fits its reference size on screen, so pixel art stays crisp, shrinking to fit exactly when the screen is smaller than the reference. A strip can be kept clear at the bottom, with the content raised to sit centred above it.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class PixelPerfectCanvasScaler : MonoBehaviour
    {
        [SerializeField] private Vector2Int referenceSize = new(288, 392);
        [SerializeField] [Min(1)] private int minimumScale = 1;
        [SerializeField] private RectTransform content;
        private CanvasScaler canvasScaler;
        private int bottomReservePixels;

        /// <summary>
        /// The size, in canvas units, that the canvas fits on screen.
        /// </summary>
        public Vector2Int ReferenceSize => referenceSize;

        /// <summary>
        /// Returns the scale the reference size would get on a screen of the given size in pixels.
        /// </summary>
        public float ScaleFor(int screenWidth, int screenHeight)
        {
            return CalculateScale(screenWidth, screenHeight, referenceSize, minimumScale);
        }

        /// <summary>
        /// Keeps a strip this many screen pixels high clear at the bottom, fitting the reference size above it.
        /// </summary>
        public void SetBottomReserve(int pixels)
        {
            bottomReservePixels = Mathf.Max(0, pixels);
            if (canvasScaler != null) ApplyScale();
        }

        /// <summary>
        /// Returns the largest whole-number scale at which the reference size fits the screen, never below the minimum; on a screen too small for even 1x, returns the exact fit so the whole layout stays visible.
        /// </summary>
        private static float CalculateScale(int screenWidth, int screenHeight, Vector2Int reference, int minimum)
        {
            if (reference.x <= 0 || reference.y <= 0 || screenWidth <= 0 || screenHeight <= 0) return Mathf.Max(1, minimum);
            var exactFit = Mathf.Min((float)screenWidth / reference.x, (float)screenHeight / reference.y);
            if (exactFit < 1f) return exactFit;
            return Mathf.Max(Mathf.Max(1, minimum), Mathf.FloorToInt(exactFit));
        }

        /// <summary>
        /// Switches the canvas scaler to constant pixel size and applies the scale for the current screen.
        /// </summary>
        private void OnEnable()
        {
            canvasScaler = GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            ApplyScale();
        }

        /// <summary>
        /// Recalculates the scale when the screen, and so the canvas, changes size.
        /// </summary>
        private void OnRectTransformDimensionsChange()
        {
            if (canvasScaler == null) return;
            ApplyScale();
        }

        /// <summary>
        /// Applies the scale for the screen above any reserved strip, and raises the content by half the strip, in whole canvas units, so it sits centred in the space left.
        /// </summary>
        private void ApplyScale()
        {
            var scale = CalculateScale(Screen.width, Screen.height - bottomReservePixels, referenceSize, minimumScale);
            if (!Mathf.Approximately(canvasScaler.scaleFactor, scale)) canvasScaler.scaleFactor = scale;
            if (content != null) content.anchoredPosition = new Vector2(0f, Mathf.Round(bottomReservePixels / 2f / scale));
        }
    }
}
