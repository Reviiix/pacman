using System;
using UnityEngine;

namespace Movement
{
    /// <summary>
    /// Reports when this enemy's trigger touches the player, whose rigidbody is what makes the trigger contact happen.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyContact : MonoBehaviour
    {
        public static event Action PlayerCaught;

        /// <summary>
        /// Announces the catch when the collider that touched this enemy is the player's.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            var movement = MovementManager.Instance;
            if (movement == null || movement.Player == null || other.transform != movement.Player.transform) return;
            PlayerCaught?.Invoke();
        }
    }
}
