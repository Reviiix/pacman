using System.Collections;
using Movement;
using Movement.MovingObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Visuals
{
    /// <summary>
    /// Plays a looping pixel-art flipbook on a character's Image and faces it in its movement direction.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CharacterAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] sideFrames;
        [SerializeField] private Sprite[] upFrames;
        [SerializeField] private Sprite[] downFrames;
        [SerializeField] [Min(0.01f)] private float secondsPerFrame = 0.15f;
        private Image image;
        private Sprite[] currentFrames;
        private int frameIndex;
        private WaitForSeconds frameWait;
        private Coroutine playback;

        /// <summary>
        /// Faces the character's current direction and follows every future direction change.
        /// </summary>
        public void Follow(MovingObject movingObject)
        {
            Face(movingObject.currentMovementDirection);
            movingObject.DirectionChanged += Face;
        }

        /// <summary>
        /// Switches to the frames for the given direction, mirroring the side frames when facing left.
        /// </summary>
        public void Face(MovementManager.Direction direction)
        {
            currentFrames = direction switch
            {
                MovementManager.Direction.Up when HasFrames(upFrames) => upFrames,
                MovementManager.Direction.Down when HasFrames(downFrames) => downFrames,
                _ => sideFrames,
            };
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction == MovementManager.Direction.Left ? -1f : 1f);
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
        /// Advances one frame per interval for as long as the character is enabled.
        /// </summary>
        private IEnumerator Play()
        {
            while (true)
            {
                yield return frameWait;
                frameIndex++;
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
