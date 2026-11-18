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
        [SerializeField] private EndGameDialog gameCompleteDialog;
        [SerializeField] private EndGameDialog gameOverDialog;
        [SerializeField] private PauseController pauseController;
        [SerializeField] private SoundEffects soundEffects;
        [SerializeField] private MuteButton muteButton;
        [SerializeField] private TouchDPad touchDPad;
        private bool gameOver;

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
            if (gameOverDialog != null) gameOverDialog.Initialise();
            if (pauseController != null) pauseController.Initialise();
            if (muteButton != null) muteButton.Initialise();
            if (touchDPad != null) touchDPad.Initialise();
            ConnectSoundEffects();
            ScoreManager.OnGameWon += WinGame;
            EnemyContact.PlayerCaught += LoseGame;
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
        /// Stops listening for the win and the catch, so a reloaded level does not call into this destroyed initialiser.
        /// </summary>
        private void OnDestroy()
        {
            ScoreManager.OnGameWon -= WinGame;
            EnemyContact.PlayerCaught -= LoseGame;
        }

        /// <summary>
        /// Shows the game-complete dialog once the last pellet is eaten.
        /// </summary>
        private void WinGame()
        {
            FinishGame(gameCompleteDialog);
        }

        /// <summary>
        /// Shows the game-over dialog when an enemy catches the player.
        /// </summary>
        private void LoseGame()
        {
            FinishGame(gameOverDialog);
        }

        /// <summary>
        /// Ends the game once only: ends pausing, freezes the characters and shows the given dialog.
        /// </summary>
        private void FinishGame(EndGameDialog dialog)
        {
            if (gameOver) return;
            gameOver = true;
            if (pauseController != null) pauseController.Lock();
            MovementManager.Instance.StopMoving();
            if (dialog != null) dialog.Show();
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
