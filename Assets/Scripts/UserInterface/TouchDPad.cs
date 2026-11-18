using Movement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Visuals;

namespace UserInterface
{
    /// <summary>
    /// An on-screen D-pad for touch screens: press or slide a thumb over it to steer. It shows only where touch input exists, scales in whole screen pixels per art pixel to at most 45% of the screen's shorter side so each arm suits a thumb, and keeps a strip clear for itself below the game when there is no room beside it.
    /// </summary>
    public class TouchDPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const float ArtSize = 48f;
        private const float MarginArtPixels = 6f;
        [SerializeField] private Image upButton;
        [SerializeField] private Image downButton;
        [SerializeField] private Image leftButton;
        [SerializeField] private Image rightButton;
        [SerializeField] private Sprite buttonSprite;
        [SerializeField] private Sprite pressedSprite;
        [SerializeField] [Range(0.1f, 0.6f)] private float screenFraction = 0.45f;
        [SerializeField] [Range(0f, 0.9f)] private float deadZoneFraction = 0.25f;
        [SerializeField] private bool showWithoutTouch;
        [SerializeField] private PixelPerfectCanvasScaler[] layoutCanvases;
        private RectTransform rect;
        private PixelPerfectCanvasScaler ownCanvas;
        private Image pressedButton;
        private MovementManager.Direction? heldDirection;
        private Vector2Int sizedForScreen;

        /// <summary>
        /// Shows the D-pad only on touch devices (or when forced for testing), then sizes it and makes room for it.
        /// </summary>
        public void Initialise()
        {
            rect = (RectTransform)transform;
            ownCanvas = GetComponentInParent<PixelPerfectCanvasScaler>();
            gameObject.SetActive(showWithoutTouch || TouchScreen.IsAvailable);
            if (gameObject.activeSelf) ApplyLayout();
        }

        /// <summary>
        /// Lays the D-pad and the game out again when the screen changes size, such as on rotation or entering full screen.
        /// </summary>
        private void Update()
        {
            if (ownCanvas != null && (sizedForScreen.x != Screen.width || sizedForScreen.y != Screen.height)) ApplyLayout();
        }

        /// <summary>
        /// Picks the D-pad's size, keeps a strip clear for it below the game unless it fits beside the game, and places it in the bottom-right corner on whole screen pixels.
        /// </summary>
        private void ApplyLayout()
        {
            sizedForScreen = new Vector2Int(Screen.width, Screen.height);
            var target = Mathf.Max(1, Mathf.FloorToInt(screenFraction * Mathf.Min(Screen.width, Screen.height) / ArtSize));
            var steps = ChooseSteps(target);
            var footprint = Footprint(steps);
            var reserve = HasRoomBeside(footprint) ? 0 : footprint;
            foreach (var canvas in layoutCanvases) if (canvas != null) canvas.SetBottomReserve(reserve);
            var canvasScale = ownCanvas.ScaleFor(Screen.width, Screen.height - reserve);
            var margin = MarginArtPixels * steps / canvasScale;
            rect.localScale = new Vector3(steps / canvasScale, steps / canvasScale, 1f);
            rect.anchoredPosition = new Vector2(-margin, margin);
        }

        /// <summary>
        /// Returns the most screen pixels per art pixel, up to the target, that shrink the game no further than half the target would, so the D-pad stays big without costing the game a scale step it need not lose.
        /// </summary>
        private int ChooseSteps(int target)
        {
            if (HasRoomBeside(Footprint(target))) return target;
            var minimum = Mathf.Max(1, target / 2);
            var scaleAtMinimum = GameScaleWith(minimum);
            for (var steps = target; steps > minimum; steps--)
            {
                if (GameScaleWith(steps) >= scaleAtMinimum) return steps;
            }
            return minimum;
        }

        /// <summary>
        /// Returns the square the D-pad and its margins take, in screen pixels.
        /// </summary>
        private static int Footprint(int steps)
        {
            return Mathf.RoundToInt((ArtSize + 2f * MarginArtPixels) * steps);
        }

        /// <summary>
        /// Whether the D-pad fits in the space beside the game at full screen height, as in landscape.
        /// </summary>
        private bool HasRoomBeside(int footprint)
        {
            var gameWidth = ownCanvas.ReferenceSize.x * ownCanvas.ScaleFor(Screen.width, Screen.height);
            return (Screen.width - gameWidth) / 2f >= footprint;
        }

        /// <summary>
        /// Returns the game's scale with a strip kept clear for a D-pad of this size.
        /// </summary>
        private float GameScaleWith(int steps)
        {
            return ownCanvas.ScaleFor(Screen.width, Screen.height - Footprint(steps));
        }

        /// <summary>
        /// Steers towards the pressed arm.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
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
        /// Releases the pressed arm.
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            heldDirection = null;
            Highlight(null);
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
