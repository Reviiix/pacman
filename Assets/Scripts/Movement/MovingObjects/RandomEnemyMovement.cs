using System;
using GridSystem.GridItems;
using Random = UnityEngine.Random;

namespace Movement.MovingObjects
{
    /// <summary>
    /// An enemy that wanders the maze, turning at random when blocked and now and then at a junction.
    /// </summary>
    [Serializable]
    public class RandomEnemyMovement : MovingObject
    {
        private readonly MovementManager.Direction[] directions = MovementManager.Directions;

        /// <summary>
        /// Snaps to the reached tile, then turns at random when blocked or now and then at a junction.
        /// </summary>
        protected override void OnDestinationReached()
        {
            base.OnDestinationReached();
            if (ShouldChangeDirection())
            {
                ChangeDirection(GetValidRandomDirection(CurrentMovementDirection));   
            }
        }

        /// <summary>
        /// Whether a wall ahead, or an occasional random choice at a junction, calls for a turn.
        /// </summary>
        private bool ShouldChangeDirection()
        {
            return current.GetAdjacentItem(CurrentMovementDirection) is Wall || ShouldTakeAlternativeRoute();
        }

        private bool ShouldTakeAlternativeRoute()
        {
            return Random.Range(0, directions.Length) == 0 && current.MultiplePathWaysAvailable();
        }

        private MovementManager.Direction GetValidRandomDirection(MovementManager.Direction currentDirection)
        {
            while (true)
            {
                var randomDirection = directions[Random.Range(0, directions.Length)];

                if (randomDirection == currentDirection) continue;
                
                if (current.GetAdjacentItem(randomDirection) is Wall) continue;

                return randomDirection;
            }
        }
    }
}