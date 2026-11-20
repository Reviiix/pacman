using System;
using System.Collections.Generic;
using System.Linq;
using Movement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GridSystem.GridItems
{
    /// <summary>
    /// One maze tile: knows its grid position and neighbours and shows its wall or corridor look.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public abstract class GridItem : MonoBehaviour
    {
        public KeyValuePair<int, int> Indices { get; private set; } //Key = X, Value = Y

        /// <summary>
        /// The tile's column and row as a cell, as the plain C# grid logic uses them.
        /// </summary>
        public Vector2Int Cell => new(Indices.Key, Indices.Value);
        protected Image Display;
        public GridItem[] Neighbors { get; private set; }
        #if UNITY_EDITOR
        [SerializeField] protected GameObject pelletPrefab; //These references are only needed while building the Grid, no point in storing them in builds
        [SerializeField] protected GameObject bonusItemPrefab;
        #endif
        [SerializeField] private TMP_Text debugIndices;

        /// <summary>
        /// Stores the tile's grid position and Image, then shows its debug label or, in Play mode, removes it; a tile without a label skips that step.
        /// </summary>
        public void Initialise(KeyValuePair<int, int> indices)
        {
            Display = GetComponent<Image>();
            Indices = indices;

            if (debugIndices == null) return;
            if (ProjectSettings.DebugSettings.DebugGridIndices)
            {
                debugIndices.text = $"{Indices.Key},{Indices.Value}";
            }
            else if (Application.isPlaying)
            {
                Destroy(debugIndices.gameObject);
            }
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Gives the component replacing this tile its debug label, which a component added by script starts without.
        /// </summary>
        protected void HandOverDebugLabel(GridItem replacement)
        {
            replacement.debugIndices = debugIndices;
        }
        #endif

        public void SetNeighbors()
        {
            Neighbors = new[]
            {
                //Row Above
                GridManager.Instance.GetItem(new KeyValuePair<int, int>(Indices.Key-1,Indices.Value)),
                //Same Row
                GridManager.Instance.GetItem(new KeyValuePair<int, int>(Indices.Key,Indices.Value-1)),
                GridManager.Instance.GetItem(new KeyValuePair<int, int>(Indices.Key,Indices.Value+1)),
                //Row Below
                GridManager.Instance.GetItem(new KeyValuePair<int, int>(Indices.Key+1,Indices.Value)),
            };
            Neighbors = Neighbors.Where(x => x != null).ToArray(); //Remove edge piece neighbors
        }

        public GridItem GetAdjacentItem(MovementManager.Direction direction)
        {
            return direction switch
            {
                MovementManager.Direction.Up => GetUpperNeighbor(),
                MovementManager.Direction.Down => GetLowerNeighbor(),
                MovementManager.Direction.Left => GetLeftNeighbor(),
                MovementManager.Direction.Right => GetRightNeighbor(),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, $"{nameof(direction)} is not accounted for in the switch statement")
            };
        }

        private GridItem GetUpperNeighbor()
        {
            return Neighbors.FirstOrDefault(x => x.Indices.Value == Indices.Value - 1);
        }
        
        private GridItem GetLowerNeighbor()
        {
            return Neighbors.FirstOrDefault(x => x.Indices.Value == Indices.Value + 1);
        }
        
        private GridItem GetLeftNeighbor()
        {
            return Neighbors.FirstOrDefault(x => x.Indices.Key == Indices.Key - 1);
        }
        
        private GridItem GetRightNeighbor()
        {
            return Neighbors.FirstOrDefault(x => x.Indices.Key == Indices.Key + 1);
        }

        public bool MultiplePathWaysAvailable()
        {
            return Neighbors.Count(x => x is Blank) > 2;
        }

        /// <summary>
        /// Sets the tile's sprite and colour, finding the Image itself when the item has not been initialised (edit mode).
        /// </summary>
        public void SetAppearance(Sprite sprite, Color color)
        {
            if (Display == null) Display = GetComponent<Image>();
            Display.sprite = sprite;
            Display.color = color;
        }

        /// <summary>
        /// Removes this tile component, deferring the removal in edit mode.
        /// </summary>
        protected void RemoveSelf()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
            }
            #if UNITY_EDITOR
            else
            {
                UnityEditor.EditorApplication.delayCall+=()=>
                {
                    DestroyImmediate(this);
                };
            }
            #endif
        }
    }
}
