using GameStates;
using GridSystem;
using GridSystem.GridItems;
using Movement;
using UnityEngine;
using Visuals;

namespace Core
{
    /// <summary>
    /// Using a project initializer can help reduce the amount of race conditions by offering more granular control of the initialisation sequence.
    /// Also offers an exy way to implement dependency injections instead of always relying on the singleton pattern.
    /// </summary>
    public class ProjectInitializer : MonoBehaviour
    {
        [SerializeField] private PelletTwinkle pelletTwinkle;

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
            ConnectCharacterAnimators();
            if (pelletTwinkle != null) pelletTwinkle.Initialise(FindObjectsByType<Pellet>(FindObjectsSortMode.None));
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
