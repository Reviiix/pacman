using JetBrains.Annotations;
using UnityEngine;

namespace GridSystem.GridItems
{
    /// <summary>
    /// A walkable tile whose edit-mode tools add or remove a pellet or bonus pickup.
    /// </summary>
    public class Blank : GridItem
    {
        #if UNITY_EDITOR
        private bool hasPellet;
        [CanBeNull] private GameObject pellet;
        private bool hasBonusItem;
        [CanBeNull] private GameObject bonusItem;

        public void InitialiseBlank(GameObject newPelletPrefab, GameObject newBonusItemPrefab)
        {
            Initialise(Indices);
            pelletPrefab = newPelletPrefab;
            bonusItemPrefab = newBonusItemPrefab;
            Display.color = Color.white;
            UnityEditorInternal.ComponentUtility.MoveComponentUp(GetComponent<Blank>());
        }

        #region Pellet
        [ContextMenu(nameof(AddPellet))]
        private void AddPellet()
        {
            if (hasPellet)
            {
                Debug.LogWarning($"Attempting to add a duplicate {nameof(pelletPrefab)} to {nameof(GridItem)}: {Indices.Key},{Indices.Value}. Skipped creation to avoid duplicate.");
                return;
            }
            hasPellet = true;
            var parent = transform;
            pellet = Instantiate(pelletPrefab, parent.position, Quaternion.identity, parent);
            pellet.GetComponent<Pellet>().Initialise(this);
            PelletManager.Instance.AddPelletToList(this);
        }
        
        [ContextMenu(nameof(RemovePellet))]
        private void RemovePellet()
        {
            // if (!hasPellet) return;
            hasPellet = false;
            DestroyPelletObject();
            PelletManager.Instance.RemovePelletFromList(this);
        }

        private void DestroyPelletObject()
        {
            if (pellet == null)
            {
                pellet = transform.GetChild(0).gameObject;
                if (pellet == null) return;
            }
            if (Application.isPlaying)
            {
                Destroy(pellet);
            }
            else
            {
                UnityEditor.EditorApplication.delayCall+=()=>
                {
                    DestroyImmediate(pellet);
                };
            }
        }
        #endregion Pellet

        #region BonusItem
        [ContextMenu(nameof(AddBonusItem))]
        private void AddBonusItem()
        {
            if (hasBonusItem)
            {
                Debug.LogWarning($"Attempting to add a duplicate {nameof(bonusItemPrefab)} to {nameof(GridItem)}: {Indices.Key},{Indices.Value}. Skipped creation to avoid duplicate.");
                return;
            }
            hasBonusItem = true;
            var parent = transform;
            bonusItem = Instantiate(bonusItemPrefab, parent.position, Quaternion.identity, parent);
            bonusItem.GetComponent<BonusItem>().Initialise(this);
        }
        
        [ContextMenu(nameof(RemoveBonusItem))]
        private void RemoveBonusItem()
        {
            //if (!hasBonusItem) return;
            hasBonusItem = false;
            DestroyBonusItemObject();
            //PelletManager.Instance.RemovePelletFromList(this);
        }

        private void DestroyBonusItemObject()
        {
            if (bonusItem == null)
            {
                bonusItem = transform.GetChild(0).gameObject;
                if (bonusItem == null) return;
            }
            if (Application.isPlaying)
            {
                Destroy(bonusItem);
            }
            else
            {
                UnityEditor.EditorApplication.delayCall+=()=>
                {
                    DestroyImmediate(bonusItem);
                };
            }
        }
        #endregion BonusItem

        /// <summary>
        /// Turns this tile into a wall, handing its debug label to the new Wall component and removing any pellet or bonus pickup it holds.
        /// </summary>
        [ContextMenu(nameof(ChangeToWall))]
        private void ChangeToWall()
        {
            var wall = gameObject.AddComponent<Wall>();
            HandOverDebugLabel(wall);
            wall.InitialiseWall(pelletPrefab, bonusItemPrefab);
            pellet = FindChildPickup<Pellet>();
            if (pellet != null) RemovePellet();
            bonusItem = FindChildPickup<BonusItem>();
            if (bonusItem != null) RemoveBonusItem();
            RemoveSelf();
        }

        /// <summary>
        /// Finds the GameObject of this tile's pickup of the given type, or null when it has none.
        /// </summary>
        private GameObject FindChildPickup<T>() where T : Component
        {
            var pickup = GetComponentInChildren<T>(true);
            return pickup != null ? pickup.gameObject : null;
        }
        #endif
    }
}
