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
    
        [ContextMenu(nameof(ChangeToBlank))]
        private void ChangeToBlank()
        {
            var blank = gameObject.AddComponent<Blank>();
            blank.InitialiseBlank(pelletPrefab, bonusItemPrefab);
            RemoveSelf();
        }

        /// <summary>
        /// Greys the conversion tool out in Play mode, as Blank's tools are, since the grid is already built and edits are lost on exit.
        /// </summary>
        [ContextMenu(nameof(ChangeToBlank), true)]
        private bool CanUseLevelTools() => !Application.isPlaying;
        #endif
    }
}
