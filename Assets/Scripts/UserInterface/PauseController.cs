using Movement;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface
{
    /// <summary>
    /// Pauses and resumes the game with P (never Esc, which the portfolio site uses to leave the game), showing an overlay that a tap also dismisses.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private GameObject overlay;
        [SerializeField] private Button resumeButton;
        private bool paused;
        private bool locked = true;

        /// <summary>
        /// Starts the game unpaused with the overlay hidden and allows pausing.
        /// </summary>
        public void Initialise()
        {
            Time.timeScale = 1f;
            overlay.SetActive(false);
            resumeButton.onClick.AddListener(Resume);
            locked = false;
        }

        /// <summary>
        /// Resumes if paused and stops further pausing, as when the game is over.
        /// </summary>
        public void Lock()
        {
            if (paused) Resume();
            locked = true;
        }

        /// <summary>
        /// Toggles the pause when P is pressed.
        /// </summary>
        private void Update()
        {
            if (locked || !Input.GetKeyDown(KeyCode.P)) return;
            if (paused) Resume();
            else Pause();
        }

        /// <summary>
        /// Pauses a built game when it loses focus, such as when the site moves focus to its Stop button, so the visitor returns to the pause overlay rather than a running game.
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && !Application.isEditor && !locked && !paused) Pause();
        }

        /// <summary>
        /// Freezes time, the characters and their input, and shows the overlay.
        /// </summary>
        private void Pause()
        {
            paused = true;
            Time.timeScale = 0f;
            MovementManager.Instance.StopMoving();
            overlay.SetActive(true);
        }

        /// <summary>
        /// Restores time, the characters and their input, and hides the overlay.
        /// </summary>
        private void Resume()
        {
            paused = false;
            Time.timeScale = 1f;
            MovementManager.Instance.ResumeMoving();
            overlay.SetActive(false);
        }

        /// <summary>
        /// Restores normal time so a reloaded level never starts frozen.
        /// </summary>
        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
