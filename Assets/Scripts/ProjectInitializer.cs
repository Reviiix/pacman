using GameStates;
using GridSystem;
using GridSystem.GridItems;
using Movement;
using UnityEngine;
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

        /// <summary>
        /// Initialises the game systems in dependency order (laying out the grid before characters snap onto it), then starts the presentation layer.
        /// </summary>
        private void Start()
        {
            GridManager.Instance.Initialise();
            Canvas.ForceUpdateCanvases();
            MovementManager.Instance.Initialise();
            PelletManager.Instance.Initialise();
            ScoreManager.Instance.Initialise();
            var pellets = FindObjectsByType<Pellet>(FindObjectsSortMode.None);
            MovePickupsToTheirLayer(pellets);
            ConnectCharacterAnimators();
            if (pelletTwinkle != null) pelletTwinkle.Initialise(pellets);
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
