using System;
using GridSystem.GridItems;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Movement.MovingObjects
{
    [Serializable]
    public class SeekerEnemyMovement : MovingObject
    {
        private static readonly MovementManager.Direction[] SteeringPriority =
        {
            MovementManager.Direction.Up, MovementManager.Direction.Left, MovementManager.Direction.Down, MovementManager.Direction.Right
        };
        public GridItem Target { get; private set; }
        public SeekerType type;
        private readonly MovementManager.Direction[] randomDirections = MovementManager.Directions;

        public enum SeekerType
        {
            Chase,
            CutOff,
            Random,
            Scared
        }

        public override void Initialise(MonoBehaviour coroutineHandler)
        {
            base.Initialise(coroutineHandler);
            if (type == SeekerType.Scared)
            {
                LogManager.Log("You are using an enemy type that is dependant on a horizontally symmetrical map.");
            }

            if (targetDebug != null)
            {
                targetDebug.gameObject.SetActive(ProjectSettings.DebugSettings.DebugEnemyTargets);
            }
        }

        protected override void OnDestinationReached()
        {
            base.OnDestinationReached();
            UpdateNext();
        }
        
        private void UpdateNext()
        {

            switch (type)
            {
                case SeekerType.Chase:
                case SeekerType.CutOff:
                case SeekerType.Scared:
                    UpdateTarget();
                    SteerTowardsTarget();
                    break;
                case SeekerType.Random:
                    if (ShouldChangeDirection())
                    {
                        ChangeDirection(GetValidRandomDirection(currentMovementDirection));   
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private bool ShouldChangeDirection()
        {
            return current.GetAdjacentItem(currentMovementDirection) is Wall || ShouldTakeAlternativeRoute();
        }
        
        private bool ShouldTakeAlternativeRoute()
        {
            return Random.Range(0, randomDirections.Length) == 0 && current.MultiplePathWaysAvailable();
        }
        
        private MovementManager.Direction GetValidRandomDirection(MovementManager.Direction currentDirection)
        {
            while (true)
            {
                var randomDirection = randomDirections[Random.Range(0, randomDirections.Length)];

                if (randomDirection == currentDirection) continue;
                
                if (current.GetAdjacentItem(randomDirection) is Wall) continue;

                return randomDirection;
            }
        }

        /// <summary>
        /// Targets the player when further than X tiles away in a straight line, otherwise retreats to the opposite side of the board.
        /// </summary>
        private GridItem GetScaredTarget()
        {
            const int distanceBeforeRetreat = 9;
            var player = MovementManager.Instance.GetPlayersNextPosition();
            var tooClose = SquaredDistance(current, player) <= distanceBeforeRetreat * distanceBeforeRetreat;
            return tooClose ? MovementManager.Instance.GetInversePlayerTransform() : player;
        }
        
        /// <summary>
        /// Predict where the player is moving too and seek out the space X amount of spaces in front of them.
        /// </summary>
        private GridItem GetCutOffTarget()
        {
            var cutoffTarget = MovementManager.Instance.GetPlayersNextPosition();
            var direction = MovementManager.Instance.GetPlayerDirection();
            var previousDirection = direction;
            var spacesAhead = 3;

            for (var i = 0; i < spacesAhead; i++)
            {
                if (cutoffTarget.GetAdjacentItem(direction) is Wall)
                {
                    // Setting direction priorities
                    if (cutoffTarget.GetAdjacentItem(MovementManager.Direction.Up) is not Wall && previousDirection != MovementManager.Direction.Down)
                    {
                        cutoffTarget = cutoffTarget.GetAdjacentItem(MovementManager.Direction.Up);
                        previousDirection = MovementManager.Direction.Up;
                    }
                    else if (cutoffTarget.GetAdjacentItem(MovementManager.Direction.Down) is not Wall && previousDirection != MovementManager.Direction.Up)
                    {
                        cutoffTarget = cutoffTarget.GetAdjacentItem(MovementManager.Direction.Down);
                        previousDirection = MovementManager.Direction.Down;
                    }
                    else if (cutoffTarget.GetAdjacentItem(MovementManager.Direction.Left) is not Wall && previousDirection != MovementManager.Direction.Right)
                    {
                        cutoffTarget = cutoffTarget.GetAdjacentItem(MovementManager.Direction.Left);
                        previousDirection = MovementManager.Direction.Left;
                    }
                    else if (cutoffTarget.GetAdjacentItem(MovementManager.Direction.Right) is not Wall && previousDirection != MovementManager.Direction.Left)
                    {
                        cutoffTarget = cutoffTarget.GetAdjacentItem(MovementManager.Direction.Right);
                        previousDirection = MovementManager.Direction.Right;
                    }
                }
                else
                {
                    cutoffTarget = cutoffTarget.GetAdjacentItem(direction);
                }
            }

            return cutoffTarget;
        }

        /// <summary>
        /// Picks the target tile for this seeker type and moves the target debug marker to it.
        /// </summary>
        private void UpdateTarget()
        {
            switch (type)
            {
                case SeekerType.Chase:
                    Target = MovementManager.Instance.GetPlayersNextPosition();
                    break;
                case SeekerType.CutOff:
                    Target = GetCutOffTarget();
                    break;
                case SeekerType.Scared:
                    Target = GetScaredTarget();
                    break;
            }
            
            if (ProjectSettings.DebugSettings.DebugEnemyTargets && targetDebug != null && Target != null)
            {
                targetDebug.position = Target.transform.position;
            }
        }

        /// <summary>
        /// Steps to the open neighbor closest in a straight line to the target, never reversing unless at a dead end, with ties broken Up, Left, Down, Right.
        /// </summary>
        private void SteerTowardsTarget()
        {
            if (Target == null) return;

            var reverseDirection = MovementManager.GetOppositeDirection(currentMovementDirection);
            var bestDirection = reverseDirection;
            var bestDistance = int.MaxValue;
            foreach (var direction in SteeringPriority)
            {
                if (direction == reverseDirection) continue;
                var candidate = current.GetAdjacentItem(direction);
                if (candidate == null || candidate is Wall) continue;
                var distance = SquaredDistance(candidate, Target);
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                bestDirection = direction;
            }

            var nextItem = current.GetAdjacentItem(bestDirection);
            if (nextItem == null || nextItem is Wall) return;
            currentMovementDirection = bestDirection;
            next = nextItem;
        }

        /// <summary>
        /// Returns the squared straight-line distance between two grid items, measured in tiles.
        /// </summary>
        private static int SquaredDistance(GridItem a, GridItem b)
        {
            var deltaX = a.Indices.Key - b.Indices.Key;
            var deltaY = a.Indices.Value - b.Indices.Value;
            return deltaX * deltaX + deltaY * deltaY;
        }
    }
}
