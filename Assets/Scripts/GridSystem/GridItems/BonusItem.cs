using System;
using GameStates;
using UnityEngine;
using UnityEngine.UI;

namespace GridSystem.GridItems
{
    /// <summary>
    /// A collectible bonus pickup that awards points by type and shows that type's sprite.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class BonusItem : MonoBehaviour
    {
        /// <summary>
        /// Raised with the pickup's world position and the points it awarded when it is collected.
        /// </summary>
        public static event Action<Vector3, int> Collected;
        [SerializeField] private BonusItemType type = BonusItemType.Cherry;

        /// <summary>
        /// Shows the sprite for this pickup's type once the score manager is available.
        /// </summary>
        private void Start()
        {
            SetGraphic();
        }

        /// <summary>
        /// Sets the Image to this pickup type's sprite.
        /// </summary>
        private void SetGraphic()
        {
            if (ScoreManager.Instance == null) return;
            GetComponent<Image>().sprite = ScoreManager.Instance.GetBonusPickUpGraphic(type);
        }

        /// <summary>
        /// Collects the pickup when the player's trigger touches it.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            BonusItemHit();
        }

        /// <summary>
        /// Scores the pickup, announces it for effects and removes it.
        /// </summary>
        private void BonusItemHit()
        {
            var points = ScoreManager.Instance.BonusItemHit(type);
            Collected?.Invoke(transform.position, points);
            Destroy(gameObject);
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Changes the pickup type in the editor and refreshes its sprite so the level shows the right item.
        /// </summary>
        private void ChangeType(BonusItemType newType)
        {
            UnityEditor.Undo.RecordObjects(new UnityEngine.Object[] { this, GetComponent<Image>() }, nameof(ChangeType));
            type = newType;
            SetGraphic();
        }

        /// <summary>
        /// Makes this pickup a cherry.
        /// </summary>
        [ContextMenu(nameof(ChangeToCherry))]
        private void ChangeToCherry() => ChangeType(BonusItemType.Cherry);

        /// <summary>
        /// Makes this pickup a strawberry.
        /// </summary>
        [ContextMenu(nameof(ChangeToStrawberry))]
        private void ChangeToStrawberry() => ChangeType(BonusItemType.Strawberry);

        /// <summary>
        /// Makes this pickup an orange.
        /// </summary>
        [ContextMenu(nameof(ChangeToOrange))]
        private void ChangeToOrange() => ChangeType(BonusItemType.Orange);

        /// <summary>
        /// Makes this pickup an apple.
        /// </summary>
        [ContextMenu(nameof(ChangeToApple))]
        private void ChangeToApple() => ChangeType(BonusItemType.Apple);

        /// <summary>
        /// Makes this pickup a melon.
        /// </summary>
        [ContextMenu(nameof(ChangeToMelon))]
        private void ChangeToMelon() => ChangeType(BonusItemType.Melon);

        /// <summary>
        /// Makes this pickup a potion.
        /// </summary>
        [ContextMenu(nameof(ChangeToPotion))]
        private void ChangeToPotion() => ChangeType(BonusItemType.Potion);

        /// <summary>
        /// Makes this pickup a bell.
        /// </summary>
        [ContextMenu(nameof(ChangeToBell))]
        private void ChangeToBell() => ChangeType(BonusItemType.Bell);

        /// <summary>
        /// Makes this pickup a key.
        /// </summary>
        [ContextMenu(nameof(ChangeToKey))]
        private void ChangeToKey() => ChangeType(BonusItemType.Key);
        #endif

        /// <summary>
        /// The pickup types in ascending point value (10 to 80); the order matches ScoreManager's pickup sprites.
        /// </summary>
        public enum BonusItemType
        {
            Cherry, Strawberry, Orange, Apple, Melon, Potion, Bell, Key
        }
    }
}
