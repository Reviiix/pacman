using UnityEngine;

namespace GridSystem.GridItems
{
    /// <summary>
    /// A wall tile whose editor tool turns it into a corridor.
    /// </summary>
    public class Wall : GridItem
    {
        #if UNITY_EDITOR
        public void InitialiseWall(GameObject newPelletPrefab, GameObject newBonusItemPrefab)
        {
            Initialise(Indices);
            Display.color = Color.black;
            pelletPrefab = newPelletPrefab;
            bonusItemPrefab = newBonusItemPrefab;
        }

        /// <summary>
        /// Turns this tile into a corridor, handing its debug label to the new Blank component.
        /// </summary>
        [ContextMenu(nameof(ChangeToBlank))]
        private void ChangeToBlank()
        {
            var blank = gameObject.AddComponent<Blank>();
            HandOverDebugLabel(blank);
            blank.InitialiseBlank(pelletPrefab, bonusItemPrefab);
            RemoveSelf();
        }
        #endif
    }
}
