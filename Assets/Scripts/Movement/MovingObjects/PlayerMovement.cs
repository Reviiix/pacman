using System;
using System.Collections.Generic;
using GridSystem;
using GridSystem.Transport.Logic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Movement.MovingObjects
{
    /// <summary>
    /// The player's character, turned by the arrow keys, WASD or a swipe on a touch screen, and carried between linked transport tiles.
    /// </summary>
    [Serializable]
    public class PlayerMovement : MovingObject
    {
        [SerializeField] [Range(0.01f, 0.25f)] private float swipeScreenFraction = 0.04f;
        private Vector2 swipeStart;
        private bool tracking;
        private bool touchOnControls;
        private TransportLinks transportLinks;
        private GridManager grid;

        /// <summary>
        /// Lets the player use the level's transport tiles, looking their exits up on the given grid.
        /// </summary>
        public void UseTransportLinks(TransportLinks links, GridManager gridManager)
        {
            transportLinks = links;
            grid = gridManager;
        }

        /// <summary>
        /// Arrives on the next tile and, if it is a transport tile reached from another tile, carries on from its partner.
        /// </summary>
        protected override void OnDestinationReached()
        {
            var arrivedFrom = current;
            base.OnDestinationReached();
            if (transportLinks == null || grid == null || arrivedFrom == null) return;
            if (!transportLinks.TryGetExitOnArrival(arrivedFrom.Cell, current.Cell, out var exit)) return;
            var exitTile = grid.GetItem(new KeyValuePair<int, int>(exit.x, exit.y));
            if (exitTile != null) TransportTo(exitTile);
        }

        /// <summary>
        /// Queues a turn for each direction key pressed this frame and for each swipe.
        /// </summary>
        public void CheckForInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) DetermineDirectionChange(MovementManager.Direction.Up);
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) DetermineDirectionChange(MovementManager.Direction.Left);
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) DetermineDirectionChange(MovementManager.Direction.Down);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) DetermineDirectionChange(MovementManager.Direction.Right);
            CheckForSwipe();
        }

        /// <summary>
        /// Turns once the first finger has travelled a small share of the screen, then measures again from there so one drag can steer through several corners; a touch that starts on on-screen controls, such as the D-pad, is left to them.
        /// </summary>
        private void CheckForSwipe()
        {
            if (Input.touchCount == 0)
            {
                tracking = false;
                touchOnControls = false;
                return;
            }

            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) touchOnControls = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            if (touchOnControls) return;
            if (touch.phase == TouchPhase.Began || !tracking)
            {
                swipeStart = touch.position;
                tracking = true;
                return;
            }

            var travel = touch.position - swipeStart;
            var threshold = Mathf.Min(Screen.width, Screen.height) * swipeScreenFraction;
            if (travel.sqrMagnitude < threshold * threshold) return;

            var direction = Mathf.Abs(travel.x) > Mathf.Abs(travel.y)
                ? travel.x > 0f ? MovementManager.Direction.Right : MovementManager.Direction.Left
                : travel.y > 0f ? MovementManager.Direction.Up : MovementManager.Direction.Down;
            DetermineDirectionChange(direction);
            swipeStart = touch.position;
        }
    }
}
