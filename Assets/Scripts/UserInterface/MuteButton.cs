using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserInterface
{
    /// <summary>
    /// The HUD's sound toggle: mutes and unmutes all game audio from a click, a tap or the M key, and remembers the choice between visits.
    /// </summary>
    public class MuteButton : MonoBehaviour
    {
        private const string MutedPreferenceKey = "SoundMuted";
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite soundOnSprite;
        [SerializeField] private Sprite mutedSprite;
        private bool muted;
        private bool initialised;
        private GameObject selectionBeforePress;

        /// <summary>
        /// Restores the saved mute choice and connects the button.
        /// </summary>
        public void Initialise()
        {
            muted = PlayerPrefs.GetInt(MutedPreferenceKey, 0) == 1;
            button.onClick.AddListener(OnPressed);
            Apply();
            initialised = true;
        }

        /// <summary>
        /// Toggles on M, and remembers what was selected before the button took the selection so a press can hand it back.
        /// </summary>
        private void Update()
        {
            if (!initialised) return;
            if (Input.GetKeyDown(KeyCode.M)) Toggle();
            var eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.currentSelectedGameObject != button.gameObject) selectionBeforePress = eventSystem.currentSelectedGameObject;
        }

        /// <summary>
        /// Toggles, then gives the selection back (such as the game-complete dialog's restart button) so Enter and Space keep working.
        /// </summary>
        private void OnPressed()
        {
            Toggle();
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return;
            var restore = selectionBeforePress != null && selectionBeforePress.activeInHierarchy ? selectionBeforePress : null;
            eventSystem.SetSelectedGameObject(restore);
        }

        /// <summary>
        /// Flips and saves the mute choice and applies it.
        /// </summary>
        private void Toggle()
        {
            muted = !muted;
            PlayerPrefs.SetInt(MutedPreferenceKey, muted ? 1 : 0);
            PlayerPrefs.Save();
            Apply();
        }

        /// <summary>
        /// Silences or restores every sound through the listener and shows the matching icon.
        /// </summary>
        private void Apply()
        {
            AudioListener.volume = muted ? 0f : 1f;
            icon.sprite = muted ? mutedSprite : soundOnSprite;
        }
    }
}
