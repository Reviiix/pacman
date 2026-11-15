using System;
using System.Collections;
using GridSystem.GridItems;
using UnityEngine;
using UnityEngine.Serialization;

namespace Movement.MovingObjects
{
    /// <summary>
    /// Moves a character tile by tile across the grid.
    /// </summary>
    [Serializable]
    public abstract class MovingObject
    {
        private const float NewPositionDistanceTolerance = 0.01f;
        private const float TurnToleranceInTiles = 0.225f;
        private const float DefaultTilesPerSecond = 2.25f;
        public Transform transform;
        [SerializeField] [Min(0f)] protected float tilesPerSecond = DefaultTilesPerSecond;
        [SerializeField] protected GridItem current;
        [SerializeField] protected GridItem next;
        private WaitUntil waitUntilReachedCurrentTarget;
        protected bool Moving;
        private Coroutine directionChange;
        public MovementManager.Direction currentMovementDirection;
        public event Action<MovementManager.Direction> DirectionChanged;
        private MonoBehaviour coroutiner;
        [SerializeField] protected Transform targetDebug;

        /// <summary>
        /// Whether the character is heading for another tile, rather than held in place by a wall.
        /// </summary>
        public bool IsMoving => next != null && next != current;

        public virtual void Initialise(MonoBehaviour coroutineHandler)
        {
            coroutiner = coroutineHandler;
            waitUntilReachedCurrentTarget = new WaitUntil(() => !Moving);
            SetNextPosition(GetAdjacentItem(currentMovementDirection));
        }
        
        public virtual void CheckPosition()
        {
            if (next is Wall) return;
            Moving = true;
            if (NewPositionReached())
            {
                OnDestinationReached();
            }
        }
        
        protected virtual void OnDestinationReached()
        {
            SetCurrentPosition(next);
            SnapToExactCurrentGridItem();
            SetNextPosition(GetAdjacentItem(currentMovementDirection));
            Moving = false;
        }

        private void SnapToExactCurrentGridItem()
        {
            transform.position = current.transform.position;
        }

        public void DetermineDirectionChange(KeyCode key)
        {
            if (directionChange != null)
            {
                coroutiner.StopCoroutine(directionChange);
            }
            
            if (CloseToCurrentGridItem())
            {
                ChangeDirection(GetDirectionFromKeyCode(key));
            }
            else
            {
                directionChange = coroutiner.StartCoroutine(WaitUntilTargetReached(()=>ChangeDirection(GetDirectionFromKeyCode(key))));
            }
        }

        protected void ChangeDirection(MovementManager.Direction direction)
        {
            if (current.GetAdjacentItem(direction) is Wall) return;
            SetMovementDirection(direction);
            SetCurrentPosition(current);
            SetNextPosition(current.GetAdjacentItem(currentMovementDirection));
        }
        
        /// <summary>
        /// Sets the movement direction and notifies listeners such as the character's sprite animator when it changes.
        /// </summary>
        protected void SetMovementDirection(MovementManager.Direction direction)
        {
            if (currentMovementDirection == direction) return;
            currentMovementDirection = direction;
            DirectionChanged?.Invoke(direction);
        }

        /// <summary>
        /// Returns this frame's position towards the next tile, moving a fixed number of tiles per second at any screen size.
        /// </summary>
        public Vector3 GetDestination()
        {
            return Vector3.MoveTowards(transform.position, next.transform.position, tilesPerSecond * GetTileWorldSize() * Time.deltaTime);
        }

        /// <summary>
        /// Returns the width of one grid tile in world units, which changes with the canvas scale.
        /// </summary>
        private float GetTileWorldSize()
        {
            var tile = (RectTransform)next.transform;
            return tile.rect.width * tile.lossyScale.x;
        }

        private MovementManager.Direction GetDirectionFromKeyCode(KeyCode key)
        {
            return key switch
            {
                KeyCode.W => MovementManager.Direction.Up,
                KeyCode.A => MovementManager.Direction.Left,
                KeyCode.S => MovementManager.Direction.Down,
                KeyCode.D => MovementManager.Direction.Right,
                _ => currentMovementDirection
            };
        }

        /// <summary>
        /// Whether the character is close enough to its current tile to turn immediately.
        /// </summary>
        private bool CloseToCurrentGridItem()
        {
            return Vector3.Distance(transform.position, current.transform.position) <= TurnToleranceInTiles * GetTileWorldSize();
        }

        protected virtual bool NewPositionReached()
        {
            return Vector3.Distance(transform.position, next.transform.position) <= NewPositionDistanceTolerance;
        }

        private void SetCurrentPosition(GridItem position)
        {
            current = position;
        }
        
        private void SetNextPosition(GridItem position)
        {
            next = position;
        }

        private GridItem GetAdjacentItem(MovementManager.Direction direction)
        {
            var gridItem = next.GetAdjacentItem(direction);
            return gridItem is Wall ? current : gridItem;
        }
        
        private IEnumerator WaitUntilTargetReached(Action callBack)
        {
            yield return waitUntilReachedCurrentTarget;
            callBack();
        }

        public GridItem GetNextPosition()
        {
            return next;
        }
    }
}