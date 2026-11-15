using System.Globalization;
using TMPro;
using UnityEngine;

namespace GameStates
{
    /// <summary>
    /// Shows the score as zero-padded, monospaced digits (and an optional drop-shadow copy) without allocating on changes.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] [Range(1, 9)] private int digits = 6;
        [SerializeField] [Min(0f)] private float digitAdvanceEm = 0.7556f;
        [SerializeField] private TMP_Text shadow;
        private TMP_Text display;
        private char[] buffer;
        private int prefixLength;

        /// <summary>
        /// Prepares the reusable character buffer, with the monospace tag up front, and shows a zero score.
        /// </summary>
        private void Awake()
        {
            display = GetComponent<TMP_Text>();
            var prefix = digitAdvanceEm > 0f ? $"<mspace={digitAdvanceEm.ToString(CultureInfo.InvariantCulture)}em>" : string.Empty;
            prefixLength = prefix.Length;
            buffer = new char[prefixLength + 10];
            prefix.CopyTo(0, buffer, 0, prefixLength);
            OnScoreChange(0);
        }

        /// <summary>
        /// Listens for score changes.
        /// </summary>
        private void OnEnable()
        {
            ScoreManager.OnScoreChange += OnScoreChange;
        }

        /// <summary>
        /// Stops listening for score changes.
        /// </summary>
        private void OnDisable()
        {
            ScoreManager.OnScoreChange -= OnScoreChange;
        }

        /// <summary>
        /// Writes the zero-padded score into the buffer and shows it on the text and its shadow.
        /// </summary>
        private void OnScoreChange(int score)
        {
            var count = Mathf.Max(digits, CountDigits(score));
            var remaining = score;
            for (var i = count - 1; i >= 0; i--)
            {
                buffer[prefixLength + i] = (char)('0' + remaining % 10);
                remaining /= 10;
            }
            display.SetText(buffer, 0, prefixLength + count);
            if (shadow != null) shadow.SetText(buffer, 0, prefixLength + count);
        }

        /// <summary>
        /// Returns how many decimal digits a non-negative number has.
        /// </summary>
        private static int CountDigits(int value)
        {
            var count = 1;
            for (var remaining = value; remaining >= 10; remaining /= 10) count++;
            return count;
        }
    }
}
