using Movement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserInterface
{
    /// <summary>
    /// An on-screen D-pad for touch screens, drawn faintly over the game: press or slide a thumb over it to steer. It shows only where touch input exists and scales in whole pixel-art steps to at most 45% of the screen's shorter side, so each arm suits a thumb.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class TouchDPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const float ArtSize = 48f;
        [SerializeField] private Image upButton;
        [SerializeField] private Image downButton;
        [SerializeField] private Image leftButton;
        [SerializeField] private Image rightButton;
        [SerializeField] private Sprite buttonSprite;
        [SerializeField] private Sprite pressedSprite;
        [SerializeField] [Range(0.1f, 0.6f)] private float screenFraction = 0.45f;
        [SerializeField] [Range(0f, 0.9f)] private float deadZoneFraction = 0.25f;
        [SerializeField] private bool showWithoutTouch;
        [SerializeField] [Range(0f, 1f)] private float idleOpacity = 0.3f;
        [SerializeField] [Range(0f, 1f)] private float heldOpacity = 0.6f;
        private CanvasGroup group;
        private RectTransform rect;
        private Canvas rootCanvas;
        private Image pressedButton;
        private MovementManager.Direction? heldDirection;
        private Vector2Int sizedForScreen;
        private float sizedForCanvasScale;

        /// <summary>
        /// Shows the D-pad only on touch devices (or when forced for testing), faint so the game shows through, and sizes it for the screen.
        /// </summary>
        public void Initialise()
        {
            rect = (RectTransform)transform;
            group = GetComponent<CanvasGroup>();
            group.alpha = idleOpacity;
            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
            gameObject.SetActive(showWithoutTouch || TouchScreen.IsAvailable);
            ApplyScale();
        }

        /// <summary>
        /// Resizes the D-pad when the screen or the canvas scale changes, such as on rotation or entering full screen.
        /// </summary>
        private void Update()
        {
            if (rootCanvas == null) return;
            if (sizedForScreen.x != Screen.width || sizedForScreen.y != Screen.height || !Mathf.Approximately(sizedForCanvasScale, rootCanvas.scaleFactor)) ApplyScale();
        }

        /// <summary>
        /// Scales the 48 px D-pad by the whole number that brings it closest to its share of the screen without exceeding it, at least 1x.
        /// </summary>
        private void ApplyScale()
        {
            sizedForScreen = new Vector2Int(Screen.width, Screen.height);
            sizedForCanvasScale = rootCanvas.scaleFactor;
            var target = screenFraction * Mathf.Min(Screen.width, Screen.height);
            var steps = Mathf.Max(1, Mathf.FloorToInt(target / (ArtSize * Mathf.Max(0.01f, sizedForCanvasScale))));
            rect.localScale = new Vector3(steps, steps, 1f);
        }

        /// <summary>
        /// Brightens the D-pad while it is held and steers towards the pressed arm.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            group.alpha = heldOpacity;
            Steer(eventData);
        }

        /// <summary>
        /// Steers again when the thumb slides onto another arm.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            Steer(eventData);
        }

        /// <summary>
        /// Releases the pressed arm and fades the D-pad back.
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            heldDirection = null;
            Highlight(null);
            group.alpha = idleOpacity;
        }

        /// <summary>
        /// Turns the player towards the arm under the pointer, ignoring the centre, and only when the arm changes.
        /// </summary>
        private void Steer(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out var local)) return;
            var offset = local - rect.rect.center;
            if (offset.magnitude < deadZoneFraction * ArtSize / 2f) return;
            var direction = Mathf.Abs(offset.x) > Mathf.Abs(offset.y)
                ? offset.x > 0f ? MovementManager.Direction.Right : MovementManager.Direction.Left
                : offset.y > 0f ? MovementManager.Direction.Up : MovementManager.Direction.Down;
            if (heldDirection == direction) return;
            heldDirection = direction;
            Highlight(ButtonFor(direction));
            if (MovementManager.Instance != null) MovementManager.Instance.TurnPlayer(direction);
        }

        /// <summary>
        /// Shows one arm as pressed and the rest as released.
        /// </summary>
        private void Highlight(Image button)
        {
            if (pressedButton != null) pressedButton.sprite = buttonSprite;
            pressedButton = button;
            if (pressedButton != null) pressedButton.sprite = pressedSprite;
        }

        /// <summary>
        /// Returns the arm for a direction.
        /// </summary>
        private Image ButtonFor(MovementManager.Direction direction)
        {
            return direction switch
            {
                MovementManager.Direction.Up => upButton,
                MovementManager.Direction.Down => downButton,
                MovementManager.Direction.Left => leftButton,
                _ => rightButton,
            };
        }
    }
}
