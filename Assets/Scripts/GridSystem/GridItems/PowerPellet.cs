using System;
using UnityEngine;

namespace GridSystem.GridItems
{
    /// <summary>
    /// Marks a pellet as a power pellet: eating it starts power mode. The Pellet beside it still scores it, counts it towards the win and removes it.
    /// </summary>
    [RequireComponent(typeof(Pellet))]
    public class PowerPellet : MonoBehaviour
    {
        /// <summary>
        /// Raised when a power pellet is eaten.
        /// </summary>
        public static event Action Eaten;

        /// <summary>
        /// Announces the power pellet when the player's trigger touches it.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            Eaten?.Invoke();
        }
    }
}
