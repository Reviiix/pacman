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
        private GridItem parent;

        public void Initialise(GridItem p)
        {
            parent = p;
        }

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

        private void OnTriggerEnter2D(Collider2D other)
        {
            BonusItemHit();
        }

        private void OnTriggerStay(Collider other)
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

        [ContextMenu(nameof(ChangeToCherry))]
        private void ChangeToCherry() => ChangeType(BonusItemType.Cherry);

        [ContextMenu(nameof(ChangeToStrawberry))]
        private void ChangeToStrawberry() => ChangeType(BonusItemType.Strawberry);

        [ContextMenu(nameof(ChangeToOrange))]
        private void ChangeToOrange() => ChangeType(BonusItemType.Orange);

        [ContextMenu(nameof(ChangeToApple))]
        private void ChangeToApple() => ChangeType(BonusItemType.Apple);

        [ContextMenu(nameof(ChangeToMelon))]
        private void ChangeToMelon() => ChangeType(BonusItemType.Melon);

        [ContextMenu(nameof(ChangeToPotion))]
        private void ChangeToPotion() => ChangeType(BonusItemType.Potion);

        [ContextMenu(nameof(ChangeToBell))]
        private void ChangeToBell() => ChangeType(BonusItemType.Bell);

        [ContextMenu(nameof(ChangeToKey))]
        private void ChangeToKey() => ChangeType(BonusItemType.Key);
        #endif

        public enum BonusItemType
        {
            Cherry, Strawberry, Orange, Apple, Melon, Potion, Bell, Key
        }
    }
}
