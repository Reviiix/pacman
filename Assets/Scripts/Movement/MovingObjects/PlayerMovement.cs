using System;
using UnityEngine;

namespace Movement.MovingObjects
{
    /// <summary>
    /// The player's character, turned by the arrow keys or WASD.
    /// </summary>
    [Serializable]
    public class PlayerMovement : MovingObject
    {
        /// <summary>
        /// Queues a turn for each direction key pressed this frame.
        /// </summary>
        public void CheckForInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) DetermineDirectionChange(MovementManager.Direction.Up);
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) DetermineDirectionChange(MovementManager.Direction.Left);
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) DetermineDirectionChange(MovementManager.Direction.Down);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) DetermineDirectionChange(MovementManager.Direction.Right);
        }
    }
}
