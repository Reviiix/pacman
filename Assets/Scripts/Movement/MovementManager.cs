using System;
using System.Collections.Generic;
using Abstractions;
using GridSystem;
using GridSystem.GridItems;
using Movement.MovingObjects;
using UnityEngine;

namespace Movement
{
    /// <summary>
    /// Moves the player and enemies every frame and answers questions about the player for the enemies.
    /// </summary>
    public class MovementManager : Singleton<MovementManager>
    {
        public static readonly Direction[] Directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        [SerializeField] private PlayerMovement player;
        [SerializeField] private SeekerEnemyMovement[] seekerEnemy;
        private bool canMove;
        private readonly Dictionary<GridItem, GridItem> mirroredWalkableItems = new();

        /// <summary>
        /// The player followed by every enemy.
        /// </summary>
        public IEnumerable<MovingObject> MovingObjects
        {
            get
            {
                yield return player;
                foreach (var seeker in seekerEnemy) yield return seeker;
            }
        }

        /// <summary>
        /// Caches the mirrored targets, places every character on its start tile and starts movement.
        /// </summary>
        public void Initialise()
        {
            CacheMirroredWalkableItems();
            player.Initialise(this);
            foreach (var seeker in seekerEnemy)
            {
                seeker.Initialise(this);
            }

            canMove = true;
        }

        /// <summary>
        /// Stops every character and the player's input, as when the game ends.
        /// </summary>
        public void StopMoving()
        {
            canMove = false;
        }

        /// <summary>
        /// Lets the characters and the player's input carry on, as after a pause.
        /// </summary>
        public void ResumeMoving()
        {
            canMove = true;
        }

        private void Update()
        {
            if (!canMove) return;
            player.CheckForInput();
            MoveObjects();
        }

        /// <summary>
        /// Advances every character's tile logic, then moves each one towards its next tile.
        /// </summary>
        private void MoveObjects()
        {
            player.CheckPosition();
            foreach (var seeker in seekerEnemy)
            {
                seeker.CheckPosition();
            }
            player.ApplyPosition(player.GetDestination());
            foreach (var seeker in seekerEnemy)
            {
                seeker.ApplyPosition(seeker.GetDestination());
            }
        }
        
        public GridItem GetPlayersNextPosition()
        {
            return player.GetNextPosition();
        }
        
        public GridItem GetInversePlayerTransform()
        {
            var playerPosition = GetPlayersNextPosition();
            return mirroredWalkableItems.TryGetValue(playerPosition, out var mirrored) ? mirrored : playerPosition;
        }

        /// <summary>
        /// Precomputes the walkable mirror of every walkable item so the Scared ghost does no searching at runtime.
        /// </summary>
        private void CacheMirroredWalkableItems()
        {
            mirroredWalkableItems.Clear();
            foreach (var gridItem in GridManager.Instance.gridItems)
            {
                if (gridItem is Wall) continue;
                mirroredWalkableItems[gridItem] = GetInverseGridItem(gridItem);
            }
        }

        /// <summary>
        /// Mirrors the item horizontally across the grid, falling back to the nearest walkable item when the mirror is a wall.
        /// </summary>
        private static GridItem GetInverseGridItem(GridItem original)
        {
            var inverseX = GridManager.Instance.amountOfColumns - 1 - original.Indices.Key;
            var mirrored = GridManager.Instance.GetItem(new KeyValuePair<int, int>(inverseX, original.Indices.Value));
            if (mirrored == null) return original;
            return mirrored is Wall ? GetNearestWalkableItem(mirrored) ?? original : mirrored;
        }

        /// <summary>
        /// Breadth-first search outward from the given item until a non-wall item is found.
        /// </summary>
        private static GridItem GetNearestWalkableItem(GridItem start)
        {
            var visited = new HashSet<GridItem> { start };
            var queue = new Queue<GridItem>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                if (item is not Wall) return item;
                foreach (var neighbor in item.Neighbors)
                {
                    if (visited.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the direction the player is moving.
        /// </summary>
        public Direction GetPlayerDirection()
        {
            return player.CurrentMovementDirection;
        }

        public static Direction GetOppositeDirection(Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        public enum Direction
        {
            Up, Down, Left, Right
        }
    }
}
