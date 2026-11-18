using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UserInterface
{
    /// <summary>
    /// Shows an end-of-game message (level complete or game over) over the maze and reloads the level when the player presses it.
    /// </summary>
    public class EndGameDialog : MonoBehaviour
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private GameObject restartPrompt;
        [SerializeField] [Min(0.05f)] private float promptBlinkSeconds = 0.5f;
        private WaitForSeconds blinkWait;

        /// <summary>
        /// Hides the dialog and connects its restart button.
        /// </summary>
        public void Initialise()
        {
            blinkWait = new WaitForSeconds(promptBlinkSeconds);
            restartButton.onClick.AddListener(Restart);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows the dialog, selecting its button so Enter, Space or a pad's submit button restart as well as a click or tap.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
            StartCoroutine(BlinkPrompt());
        }

        /// <summary>
        /// Flashes the restart prompt on and off like an arcade attract screen.
        /// </summary>
        private IEnumerator BlinkPrompt()
        {
            while (true)
            {
                yield return blinkWait;
                restartPrompt.SetActive(!restartPrompt.activeSelf);
            }
        }

        /// <summary>
        /// Reloads the current level from scratch.
        /// </summary>
        private static void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
