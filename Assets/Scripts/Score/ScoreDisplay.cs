using System.Text;
using TMPro;
using UnityEngine;

namespace GameStates
{
    /// <summary>
    /// Shows the score arcade-style: a coloured label followed by a zero-padded number.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] private string label = "SCORE";
        [SerializeField] private Color labelColor = Color.white;
        [SerializeField] [Range(1, 9)] private int digits = 6;
        private readonly StringBuilder builder = new(64);
        private TMP_Text display;
        private string labelMarkup;

        /// <summary>
        /// Caches the label markup and shows a zero score.
        /// </summary>
        private void Awake()
        {
            display = GetComponent<TMP_Text>();
            labelMarkup = $"<color=#{ColorUtility.ToHtmlStringRGBA(labelColor)}>{label}</color> ";
            OnScoreChange(0);
        }

        private void OnEnable()
        {
            ScoreManager.OnScoreChange += OnScoreChange;
        }

        private void OnDisable()
        {
            ScoreManager.OnScoreChange -= OnScoreChange;
        }

        /// <summary>
        /// Rebuilds the readout without allocating a new string.
        /// </summary>
        private void OnScoreChange(int score)
        {
            builder.Clear().Append(labelMarkup);
            for (var padding = digits - CountDigits(score); padding > 0; padding--) builder.Append('0');
            builder.Append(score);
            display.SetText(builder);
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
