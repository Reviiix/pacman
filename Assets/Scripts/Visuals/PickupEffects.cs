using System.Collections;
using GridSystem.GridItems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Visuals
{
    /// <summary>
    /// Plays a pooled sugar-burst flash where pellets and pickups are eaten, plus a rising "+NN" popup for bonus points.
    /// </summary>
    public class PickupEffects : MonoBehaviour
    {
        private const float PopupRiseArtPixels = 8f;
        [SerializeField] private Sprite[] burstFrames;
        [SerializeField] [Min(0.01f)] private float firstFrameSeconds = 0.06f;
        [SerializeField] [Min(0.01f)] private float secondFrameSeconds = 0.09f;
        [SerializeField] [Min(1)] private int burstPoolSize = 6;
        [SerializeField] private RectTransform popupTemplate;
        [SerializeField] [Min(1)] private int popupPoolSize = 3;
        [SerializeField] [Min(0.01f)] private float popupSeconds = 0.6f;
        [SerializeField] [Min(0f)] private float popupFadeSeconds = 0.2f;
        private Image[] bursts;
        private Coroutine[] burstRoutines;
        private int nextBurst;
        private RectTransform[] popups;
        private TMP_Text[][] popupTexts;
        private Coroutine[] popupRoutines;
        private int nextPopup;

        /// <summary>
        /// Builds the burst and popup pools up front so nothing is instantiated during play.
        /// </summary>
        private void Awake()
        {
            bursts = new Image[burstPoolSize];
            burstRoutines = new Coroutine[burstPoolSize];
            for (var i = 0; i < burstPoolSize; i++)
            {
                var burst = new GameObject($"Burst {i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                burst.transform.SetParent(transform, false);
                ((RectTransform)burst.transform).sizeDelta = new Vector2(16f, 16f);
                bursts[i] = burst.GetComponent<Image>();
                bursts[i].raycastTarget = false;
                burst.SetActive(false);
            }

            popups = new RectTransform[popupTemplate != null ? popupPoolSize : 0];
            popupTexts = new TMP_Text[popups.Length][];
            popupRoutines = new Coroutine[popups.Length];
            for (var i = 0; i < popups.Length; i++)
            {
                popups[i] = Instantiate(popupTemplate, transform);
                popups[i].name = $"Popup {i}";
                popupTexts[i] = popups[i].GetComponentsInChildren<TMP_Text>(true);
                popups[i].gameObject.SetActive(false);
            }
            if (popupTemplate != null) popupTemplate.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Pellet.Eaten += OnPelletEaten;
            BonusItem.Collected += OnBonusCollected;
        }

        private void OnDisable()
        {
            Pellet.Eaten -= OnPelletEaten;
            BonusItem.Collected -= OnBonusCollected;
        }

        /// <summary>
        /// Flashes a burst where a pellet was eaten.
        /// </summary>
        private void OnPelletEaten(Vector3 position)
        {
            PlayBurst(position);
        }

        /// <summary>
        /// Flashes a burst and shows the points where a bonus pickup was collected.
        /// </summary>
        private void OnBonusCollected(Vector3 position, int points)
        {
            PlayBurst(position);
            PlayPopup(position, points);
        }

        /// <summary>
        /// Starts the next pooled burst at a world position, restarting it if it was still playing.
        /// </summary>
        private void PlayBurst(Vector3 position)
        {
            if (burstFrames == null || burstFrames.Length < 2) return;
            var index = nextBurst;
            nextBurst = (nextBurst + 1) % bursts.Length;
            if (burstRoutines[index] != null) StopCoroutine(burstRoutines[index]);
            burstRoutines[index] = StartCoroutine(Burst(bursts[index], position));
        }

        /// <summary>
        /// Shows the first frame then the second, fading from opaque to clear over both.
        /// </summary>
        private IEnumerator Burst(Image burst, Vector3 position)
        {
            burst.transform.position = position;
            burst.gameObject.SetActive(true);
            var total = firstFrameSeconds + secondFrameSeconds;
            for (var elapsed = 0f; elapsed < total; elapsed += Time.deltaTime)
            {
                burst.sprite = elapsed < firstFrameSeconds ? burstFrames[0] : burstFrames[1];
                burst.color = new Color(1f, 1f, 1f, 1f - elapsed / total);
                yield return null;
            }
            burst.gameObject.SetActive(false);
        }

        /// <summary>
        /// Starts the next pooled points popup at a world position.
        /// </summary>
        private void PlayPopup(Vector3 position, int points)
        {
            if (popups.Length == 0) return;
            var index = nextPopup;
            nextPopup = (nextPopup + 1) % popups.Length;
            if (popupRoutines[index] != null) StopCoroutine(popupRoutines[index]);
            popupRoutines[index] = StartCoroutine(Popup(index, position, points));
        }

        /// <summary>
        /// Rises a whole art pixel at a time and fades out over its last moments.
        /// </summary>
        private IEnumerator Popup(int index, Vector3 position, int points)
        {
            var popup = popups[index];
            var texts = popupTexts[index];
            foreach (var text in texts) text.SetText("+{0}", points);
            popup.gameObject.SetActive(true);
            var artPixel = popup.lossyScale.y;
            for (var elapsed = 0f; elapsed < popupSeconds; elapsed += Time.deltaTime)
            {
                var rise = Mathf.Floor(PopupRiseArtPixels * elapsed / popupSeconds);
                popup.position = position + Vector3.up * (rise * artPixel);
                var fade = popupFadeSeconds > 0f ? Mathf.Clamp01((popupSeconds - elapsed) / popupFadeSeconds) : 1f;
                foreach (var text in texts) text.alpha = fade;
                yield return null;
            }
            popup.gameObject.SetActive(false);
        }
    }
}
