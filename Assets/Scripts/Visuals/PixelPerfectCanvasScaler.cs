using UnityEngine;
using UnityEngine.UI;

namespace Visuals
{
    /// <summary>
    /// Scales a canvas by the largest whole number that fits its reference size on screen, so pixel art stays crisp.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class PixelPerfectCanvasScaler : MonoBehaviour
    {
        [SerializeField] private Vector2Int referenceSize = new(288, 392);
        [SerializeField] [Min(1)] private int minimumScale = 1;
        private CanvasScaler canvasScaler;

        /// <summary>
        /// Returns the largest whole-number scale at which the reference size fits the screen, never below the minimum.
        /// </summary>
        private static int CalculateScale(int screenWidth, int screenHeight, Vector2Int reference, int minimum)
        {
            if (reference.x <= 0 || reference.y <= 0) return Mathf.Max(1, minimum);
            var fit = Mathf.Min(screenWidth / reference.x, screenHeight / reference.y);
            return Mathf.Max(Mathf.Max(1, minimum), fit);
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
        /// Applies the whole-number scale for the current screen size if it has changed.
        /// </summary>
        private void ApplyScale()
        {
            var scale = CalculateScale(Screen.width, Screen.height, referenceSize, minimumScale);
            if (Mathf.Approximately(canvasScaler.scaleFactor, scale)) return;
            canvasScaler.scaleFactor = scale;
        }
    }
}
