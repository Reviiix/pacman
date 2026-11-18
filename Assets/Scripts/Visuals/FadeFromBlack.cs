using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Visuals
{
    /// <summary>
    /// Covers the screen in black as the level starts and fades it away in even steps, like an old console's palette fade, in real time so a paused game still fades in (save its Image disabled so the scene stays visible in edit mode).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class FadeFromBlack : MonoBehaviour
    {
        [SerializeField] [Min(0f)] private float holdSeconds = 0.15f;
        [SerializeField] [Min(0.01f)] private float fadeSeconds = 0.8f;
        [SerializeField] [Min(0)] private int steps = 8;
        private Image image;

        /// <summary>
        /// Shows the screen fully black and starts fading it away.
        /// </summary>
        public void Play()
        {
            image = GetComponent<Image>();
            image.raycastTarget = false;
            SetOpacity(1f);
            image.enabled = true;
            gameObject.SetActive(true);
            StartCoroutine(Fade());
        }

        /// <summary>
        /// Holds the black briefly, steps the opacity down to nothing, then switches the overlay off.
        /// </summary>
        private IEnumerator Fade()
        {
            if (holdSeconds > 0f) yield return new WaitForSecondsRealtime(holdSeconds);
            for (var elapsed = 0f; elapsed < fadeSeconds; elapsed += Time.unscaledDeltaTime)
            {
                SetOpacity(1f - Quantise(elapsed / fadeSeconds));
                yield return null;
            }
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Rounds fade progress down to the nearest step, or leaves it smooth when steps is 0.
        /// </summary>
        private float Quantise(float progress)
        {
            return steps > 0 ? Mathf.Floor(progress * steps) / steps : progress;
        }

        /// <summary>
        /// Sets the overlay's opacity, keeping its colour.
        /// </summary>
        private void SetOpacity(float opacity)
        {
            var colour = image.color;
            colour.a = opacity;
            image.color = colour;
        }
    }
}
