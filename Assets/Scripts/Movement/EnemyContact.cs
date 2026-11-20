using System;
using PowerUps;
using UnityEngine;

namespace Movement
{
    /// <summary>
    /// Reports when this enemy's trigger touches the player, whose rigidbody is what makes the trigger contact happen: in power mode the enemy is defeated, otherwise the player is caught.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyContact : MonoBehaviour
    {
        public static event Action PlayerCaught;

        /// <summary>
        /// When the collider that touched this enemy is the player's, defeats the enemy in power mode or announces the catch otherwise.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            var movement = MovementManager.Instance;
            if (movement == null || movement.Player == null || other.transform != movement.Player.transform) return;
            if (PowerMode.Instance != null && PowerMode.Instance.IsActive) movement.DefeatEnemy(transform);
            else PlayerCaught?.Invoke();
        }
    }
}
