using GameAudio;
using GameStates;
using GridSystem;
using GridSystem.GridItems;
using Movement;
using UnityEngine;
using UserInterface;
using Visuals;

namespace Core
{
    /// <summary>
    /// Runs the start-up sequence: grid, movement, pellets and score, then wires up the presentation layer.
    /// </summary>
    public class ProjectInitializer : MonoBehaviour
    {
        [SerializeField] private PelletTwinkle pelletTwinkle;
        [SerializeField] private Transform pickupLayer;
        [SerializeField] private GameCompleteDialog gameCompleteDialog;
        [SerializeField] private PauseController pauseController;
        [SerializeField] private SoundEffects soundEffects;

        /// <summary>
        /// Initialises the game systems in dependency order (laying out the grid before characters snap onto it), then starts the presentation layer.
        /// </summary>
        private void Start()
        {
            var pellets = FindObjectsByType<Pellet>(FindObjectsSortMode.None);
            GridManager.Instance.Initialise();
            Canvas.ForceUpdateCanvases();
            MovementManager.Instance.Initialise();
            PelletManager.Instance.Initialise(pellets);
            ScoreManager.Instance.Initialise();
            MovePickupsToTheirLayer(pellets);
            ConnectCharacterAnimators();
            if (pelletTwinkle != null) pelletTwinkle.Initialise(pellets);
            if (gameCompleteDialog != null) gameCompleteDialog.Initialise();
            if (pauseController != null) pauseController.Initialise();
            ConnectSoundEffects();
            ScoreManager.OnGameWon += EndGame;
        }

        /// <summary>
        /// Hooks the sound effects up to the pause and plays the level-start jingle.
        /// </summary>
        private void ConnectSoundEffects()
        {
            if (soundEffects == null) return;
            if (pauseController != null) pauseController.PausedChanged += soundEffects.PlayPauseChange;
            soundEffects.Initialise();
        }

        /// <summary>
        /// Stops listening for the win, so a reloaded level does not call into this destroyed initialiser.
        /// </summary>
        private void OnDestroy()
        {
            ScoreManager.OnGameWon -= EndGame;
        }

        /// <summary>
        /// Ends pausing, freezes the characters and shows the game-complete dialog once the last pellet is eaten.
        /// </summary>
        private void EndGame()
        {
            if (pauseController != null) pauseController.Lock();
            MovementManager.Instance.StopMoving();
            if (gameCompleteDialog != null) gameCompleteDialog.Show();
        }

        /// <summary>
        /// Moves pellets and bonus items from their maze cells onto their own canvas layer, keeping their positions, so changes to them don't rebuild the static maze.
        /// </summary>
        private void MovePickupsToTheirLayer(Pellet[] pellets)
        {
            if (pickupLayer == null) return;
            foreach (var pellet in pellets) pellet.transform.SetParent(pickupLayer, true);
            foreach (var bonusItem in FindObjectsByType<BonusItem>(FindObjectsSortMode.None)) bonusItem.transform.SetParent(pickupLayer, true);
        }

        /// <summary>
        /// Makes each character's animator face the way that character moves.
        /// </summary>
        private static void ConnectCharacterAnimators()
        {
            foreach (var movingObject in MovementManager.Instance.MovingObjects)
            {
                if (movingObject.transform.TryGetComponent(out CharacterAnimator characterAnimator))
                {
                    characterAnimator.Follow(movingObject);
                }
            }
        }
    }
}
