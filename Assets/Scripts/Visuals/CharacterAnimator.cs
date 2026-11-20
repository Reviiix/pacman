using System.Collections;
using Movement;
using Movement.MovingObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Visuals
{
    /// <summary>
    /// Plays a looping pixel-art flipbook on a character's Image and faces it in its movement direction, with an optional second set of frames for its powered look.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CharacterAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] sideFrames;
        [SerializeField] private Sprite[] upFrames;
        [SerializeField] private Sprite[] downFrames;
        [SerializeField] private Sprite[] poweredSideFrames;
        [SerializeField] private Sprite[] poweredUpFrames;
        [SerializeField] private Sprite[] poweredDownFrames;
        [SerializeField] [Min(0.01f)] private float secondsPerFrame = 0.15f;
        [SerializeField] private bool animateOnlyWhileMoving;
        [SerializeField] private bool mirrorWhenFacingLeft = true;
        private Image image;
        private Sprite[] currentFrames;
        private int frameIndex;
        private WaitForSeconds frameWait;
        private Coroutine playback;
        private MovingObject followed;
        private MovementManager.Direction facing = MovementManager.Direction.Right;
        private bool powered;

        /// <summary>
        /// Faces the character's current direction and follows every future direction change.
        /// </summary>
        public void Follow(MovingObject movingObject)
        {
            followed = movingObject;
            Face(movingObject.CurrentMovementDirection);
            movingObject.DirectionChanged += Face;
        }

        /// <summary>
        /// Switches between the normal and powered frames (when it has powered frames), keeping the direction it faces.
        /// </summary>
        public void ShowPoweredLook(bool showPowered)
        {
            powered = showPowered && HasFrames(poweredSideFrames);
            Face(facing);
        }

        /// <summary>
        /// Switches to the frames for the given direction in the current look, mirroring the side frames when facing left unless that is turned off for symmetric, top-left-lit art.
        /// </summary>
        private void Face(MovementManager.Direction direction)
        {
            facing = direction;
            var side = powered ? poweredSideFrames : sideFrames;
            var up = powered ? poweredUpFrames : upFrames;
            var down = powered ? poweredDownFrames : downFrames;
            currentFrames = direction switch
            {
                MovementManager.Direction.Up when HasFrames(up) => up,
                MovementManager.Direction.Down when HasFrames(down) => down,
                _ => side,
            };
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (mirrorWhenFacingLeft && direction == MovementManager.Direction.Left ? -1f : 1f);
            transform.localScale = scale;
            ShowFrame();
        }

        /// <summary>
        /// Starts the flipbook, defaulting to the side frames.
        /// </summary>
        private void OnEnable()
        {
            image = GetComponent<Image>();
            frameWait = new WaitForSeconds(secondsPerFrame);
            currentFrames ??= sideFrames;
            ShowFrame();
            playback = StartCoroutine(Play());
        }

        /// <summary>
        /// Stops the flipbook.
        /// </summary>
        private void OnDisable()
        {
            if (playback != null) StopCoroutine(playback);
        }

        /// <summary>
        /// Advances one frame per interval, holding the first frame while a followed character is blocked if so configured.
        /// </summary>
        private IEnumerator Play()
        {
            while (true)
            {
                yield return frameWait;
                var holding = animateOnlyWhileMoving && followed != null && !followed.IsMoving;
                frameIndex = holding ? 0 : frameIndex + 1;
                ShowFrame();
            }
        }

        /// <summary>
        /// Shows the current frame of the current direction's frames.
        /// </summary>
        private void ShowFrame()
        {
            if (!HasFrames(currentFrames) || image == null) return;
            image.sprite = currentFrames[frameIndex % currentFrames.Length];
        }

        /// <summary>
        /// Whether a frame list has at least one frame.
        /// </summary>
        private static bool HasFrames(Sprite[] frames)
        {
            return frames != null && frames.Length > 0;
        }
    }
}
