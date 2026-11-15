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
        [SerializeField] private MovementManager.Direction currentMovementDirection;
        public event Action<MovementManager.Direction> DirectionChanged;
        private MonoBehaviour coroutiner;
        private Vector3 logicalLocalPosition;
        [SerializeField] protected Transform targetDebug;

        /// <summary>
        /// The direction the character is moving; change it through SetMovementDirection so DirectionChanged fires.
        /// </summary>
        public MovementManager.Direction CurrentMovementDirection => currentMovementDirection;

        /// <summary>
        /// Whether the character is heading for another tile, rather than held in place by a wall.
        /// </summary>
        public bool IsMoving => next != null && next != current;

        /// <summary>
        /// Places the character exactly on its starting tile (its next tile if no current one is set) and picks the first tile to head for.
        /// </summary>
        public virtual void Initialise(MonoBehaviour coroutineHandler)
        {
            coroutiner = coroutineHandler;
            waitUntilReachedCurrentTarget = new WaitUntil(() => !Moving);
            if (current == null) SetCurrentPosition(next);
            SnapToExactCurrentGridItem();
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

        /// <summary>
        /// Places the character exactly on its current tile.
        /// </summary>
        private void SnapToExactCurrentGridItem()
        {
            ApplyPosition(ToLocal(current));
        }

        /// <summary>
        /// Turns now if the character is close enough to its tile, otherwise queues the turn for when it arrives, replacing any queued turn.
        /// </summary>
        public void DetermineDirectionChange(MovementManager.Direction direction)
        {
            if (directionChange != null)
            {
                coroutiner.StopCoroutine(directionChange);
            }

            if (CloseToCurrentGridItem())
            {
                ChangeDirection(direction);
            }
            else
            {
                directionChange = coroutiner.StartCoroutine(WaitUntilTargetReached(()=>ChangeDirection(direction)));
            }
        }

        /// <summary>
        /// Turns to face a direction and heads for the tile that way, unless a wall is in the way.
        /// </summary>
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
        /// Returns this frame's logical position towards the next tile, in the parent canvas's units, moving a fixed number of tiles per second.
        /// </summary>
        public Vector3 GetDestination()
        {
            return Vector3.MoveTowards(logicalLocalPosition, ToLocal(next), tilesPerSecond * GetTileSize() * Time.deltaTime);
        }

        /// <summary>
        /// Stores the logical position and draws the character on the nearest whole art pixel (one canvas unit), so it lines up with the maze while moving.
        /// </summary>
        public void ApplyPosition(Vector3 localPosition)
        {
            logicalLocalPosition = localPosition;
            transform.localPosition = new Vector3(Mathf.Round(localPosition.x), Mathf.Round(localPosition.y), localPosition.z);
        }

        /// <summary>
        /// Returns a grid item's position in the character's parent canvas space, which stays the same when the canvas rescales.
        /// </summary>
        private Vector3 ToLocal(GridItem item)
        {
            return transform.parent.InverseTransformPoint(item.transform.position);
        }

        /// <summary>
        /// Returns the width of one grid tile in the parent canvas's units.
        /// </summary>
        private float GetTileSize()
        {
            var tile = (RectTransform)next.transform;
            return tile.rect.width * tile.lossyScale.x / transform.parent.lossyScale.x;
        }

        /// <summary>
        /// Whether the character is close enough to its current tile to turn immediately.
        /// </summary>
        private bool CloseToCurrentGridItem()
        {
            return Vector3.Distance(logicalLocalPosition, ToLocal(current)) <= TurnToleranceInTiles * GetTileSize();
        }

        /// <summary>
        /// Whether the character's logical position has arrived at the next tile.
        /// </summary>
        protected virtual bool NewPositionReached()
        {
            return Vector3.Distance(logicalLocalPosition, ToLocal(next)) <= NewPositionDistanceTolerance;
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