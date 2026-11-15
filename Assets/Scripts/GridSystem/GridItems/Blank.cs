using JetBrains.Annotations;
using UnityEngine;

namespace GridSystem.GridItems
{
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
