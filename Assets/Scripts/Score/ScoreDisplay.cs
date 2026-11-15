using System.Text;
using TMPro;
using UnityEngine;

namespace GameStates
{
    /// <summary>
    /// Shows the score arcade-style: a coloured label followed by a zero-padded number, with an optional drop-shadow copy.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] private string label = "SCORE";
        [SerializeField] private Color labelColor = Color.white;
        [SerializeField] private Color digitsColor = Color.white;
        [SerializeField] [Range(1, 9)] private int digits = 6;
        [Header("Drop shadow")]
        [SerializeField] private TMP_Text shadow;
        [SerializeField] private Color labelShadowColor = Color.black;
        [SerializeField] private Color digitsShadowColor = Color.black;
        private readonly StringBuilder builder = new(64);
        private TMP_Text display;
        private string labelMarkup;
        private string digitsMarkup;
        private string labelShadowMarkup;
        private string digitsShadowMarkup;

        /// <summary>
        /// Caches the colour markup and shows a zero score.
        /// </summary>
        private void Awake()
        {
            display = GetComponent<TMP_Text>();
            labelMarkup = $"<color=#{ColorUtility.ToHtmlStringRGBA(labelColor)}>{label}</color> ";
            digitsMarkup = $"<color=#{ColorUtility.ToHtmlStringRGBA(digitsColor)}>";
            labelShadowMarkup = $"<color=#{ColorUtility.ToHtmlStringRGBA(labelShadowColor)}>{label}</color> ";
            digitsShadowMarkup = $"<color=#{ColorUtility.ToHtmlStringRGBA(digitsShadowColor)}>";
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
        /// Rebuilds the readout and its shadow without allocating new strings.
        /// </summary>
        private void OnScoreChange(int score)
        {
            Write(display, labelMarkup, digitsMarkup, score);
            if (shadow != null) Write(shadow, labelShadowMarkup, digitsShadowMarkup, score);
        }

        /// <summary>
        /// Writes the label markup and the zero-padded score in the given colours into a text.
        /// </summary>
        private void Write(TMP_Text target, string labelPart, string digitsPart, int score)
        {
            builder.Clear().Append(labelPart).Append(digitsPart);
            for (var padding = digits - CountDigits(score); padding > 0; padding--) builder.Append('0');
            builder.Append(score).Append("</color>");
            target.SetText(builder);
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
