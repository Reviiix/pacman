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

        /// <summary>
        /// Greys the level tools out in Play mode, where the pickups have moved to the pickup layer and edits are lost on exit.
        /// </summary>
        [ContextMenu(nameof(AddPellet), true)]
        [ContextMenu(nameof(RemovePellet), true)]
        [ContextMenu(nameof(AddBonusItem), true)]
        [ContextMenu(nameof(RemoveBonusItem), true)]
        [ContextMenu(nameof(ChangeToWall), true)]
        private bool CanUseLevelTools() => !Application.isPlaying;

        #region Pellet
        /// <summary>
        /// Adds a linked pellet to this tile.
        /// </summary>
        [ContextMenu(nameof(AddPellet))]
        private void AddPellet()
        {
            if (hasPellet)
            {
                Debug.LogWarning($"Attempting to add a duplicate {nameof(pelletPrefab)} to {nameof(GridItem)}: {Indices.Key},{Indices.Value}. Skipped creation to avoid duplicate.");
                return;
            }
            hasPellet = true;
            pellet = AddPickup(pelletPrefab);
        }

        /// <summary>
        /// Adds a linked prefab instance centred on this tile, so later prefab edits reach it.
        /// </summary>
        private GameObject AddPickup(GameObject prefab)
        {
            var instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform);
            instance.transform.position = transform.position;
            UnityEditor.Undo.RegisterCreatedObjectUndo(instance, nameof(AddPickup));
            return instance;
        }
        
        /// <summary>
        /// Removes this tile's pellet.
        /// </summary>
        [ContextMenu(nameof(RemovePellet))]
        private void RemovePellet()
        {
            // if (!hasPellet) return;
            hasPellet = false;
            DestroyPelletObject();
        }

        /// <summary>
        /// Destroys this tile's pellet, finding it by component if it was not added in this session.
        /// </summary>
        private void DestroyPelletObject()
        {
            if (pellet == null)
            {
                var found = GetComponentInChildren<Pellet>(true);
                if (found == null)
                {
                    Debug.LogWarning($"No pellet under {nameof(GridItem)} {Indices.Key},{Indices.Value}.", this);
                    return;
                }
                pellet = found.gameObject;
            }
            UnityEditor.EditorApplication.delayCall+=()=>
            {
                DestroyImmediate(pellet);
            };
        }
        #endregion Pellet

        #region BonusItem
        /// <summary>
        /// Adds a linked bonus pickup to this tile.
        /// </summary>
        [ContextMenu(nameof(AddBonusItem))]
        private void AddBonusItem()
        {
            if (hasBonusItem)
            {
                Debug.LogWarning($"Attempting to add a duplicate {nameof(bonusItemPrefab)} to {nameof(GridItem)}: {Indices.Key},{Indices.Value}. Skipped creation to avoid duplicate.");
                return;
            }
            hasBonusItem = true;
            bonusItem = AddPickup(bonusItemPrefab);
        }
        
        /// <summary>
        /// Removes this tile's bonus pickup.
        /// </summary>
        [ContextMenu(nameof(RemoveBonusItem))]
        private void RemoveBonusItem()
        {
            //if (!hasBonusItem) return;
            hasBonusItem = false;
            DestroyBonusItemObject();
        }

        /// <summary>
        /// Destroys this tile's bonus pickup, finding it by component if it was not added in this session.
        /// </summary>
        private void DestroyBonusItemObject()
        {
            if (bonusItem == null)
            {
                var found = GetComponentInChildren<BonusItem>(true);
                if (found == null)
                {
                    Debug.LogWarning($"No bonus pickup under {nameof(GridItem)} {Indices.Key},{Indices.Value}.", this);
                    return;
                }
                bonusItem = found.gameObject;
            }
            UnityEditor.EditorApplication.delayCall+=()=>
            {
                DestroyImmediate(bonusItem);
            };
        }
        #endregion BonusItem

        [ContextMenu(nameof(ChangeToWall))]
        private void ChangeToWall()
        {
            gameObject.AddComponent<Wall>().InitialiseWall(pelletPrefab, bonusItemPrefab);
            RemovePellet();
            RemoveSelf();
        }
        #endif
    }
}
