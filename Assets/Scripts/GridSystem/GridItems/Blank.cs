using JetBrains.Annotations;
using UnityEngine;

namespace GridSystem.GridItems
{
    /// <summary>
    /// A walkable tile whose editor tools add or remove a pellet or bonus pickup.
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
        /// <summary>
        /// Adds a linked pellet to this tile and registers the tile with the PelletManager.
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
            PelletManager.Instance.AddPelletToList(this);
        }

        /// <summary>
        /// Adds a pickup centred on this tile: a linked prefab instance in edit mode (so prefab edits reach it), a plain copy in Play mode.
        /// </summary>
        private GameObject AddPickup(GameObject prefab)
        {
            if (Application.isPlaying) return Instantiate(prefab, transform.position, Quaternion.identity, transform);
            var instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform);
            instance.transform.position = transform.position;
            UnityEditor.Undo.RegisterCreatedObjectUndo(instance, nameof(AddPickup));
            return instance;
        }
        
        [ContextMenu(nameof(RemovePellet))]
        private void RemovePellet()
        {
            // if (!hasPellet) return;
            hasPellet = false;
            DestroyPelletObject();
            PelletManager.Instance.RemovePelletFromList(this);
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
                    Debug.LogWarning($"No pellet under {nameof(GridItem)} {Indices.Key},{Indices.Value} (in Play mode pellets move to the pickup layer).", this);
                    return;
                }
                pellet = found.gameObject;
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
        
        [ContextMenu(nameof(RemoveBonusItem))]
        private void RemoveBonusItem()
        {
            //if (!hasBonusItem) return;
            hasBonusItem = false;
            DestroyBonusItemObject();
            //PelletManager.Instance.RemovePelletFromList(this);
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
                    Debug.LogWarning($"No bonus pickup under {nameof(GridItem)} {Indices.Key},{Indices.Value} (in Play mode pickups move to the pickup layer).", this);
                    return;
                }
                bonusItem = found.gameObject;
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
