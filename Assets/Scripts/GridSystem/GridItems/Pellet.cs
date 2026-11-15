using System;
using GameStates;
using UnityEngine;

namespace GridSystem.GridItems
{
    /// <summary>
    /// A pellet the player eats for a point; eating every pellet wins the game.
    /// </summary>
    public class Pellet : MonoBehaviour
    {
        /// <summary>
        /// Raised with the pellet's world position when it is eaten.
        /// </summary>
        public static event Action<Vector3> Eaten;

        /// <summary>
        /// Eats the pellet when the player's trigger touches it.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            PelletHit();
        }

        /// <summary>
        /// Scores the pellet, announces it for effects and removes it.
        /// </summary>
        private void PelletHit()
        {
            LogManager.Log("Pellet Hit");
            ScoreManager.Instance.IncrementPelletsHit();
            Eaten?.Invoke(transform.position);
            Destroy(gameObject);
        }
    }
}
