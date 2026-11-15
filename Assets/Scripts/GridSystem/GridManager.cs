using System;
using System.Collections.Generic;
using Abstractions;
using GridSystem.GridItems;
using UnityEngine;
using UnityEngine.Serialization;

namespace GridSystem
{
    [ExecuteInEditMode]
    public class GridManager : Singleton<GridManager>
    {
        private const int MinimumItems = 3;
        private const int MaximumRows = 99;
        private const int MaximumColumns = 99;
        private const string GridTag = "Grid";
        public readonly List<GridItem> gridItems = new ();
        private GridItem[,] itemLookup;
        private bool generatingGrid;
        private GameObject gridObject;
        [SerializeField] private Transform gridArea;
        [SerializeField] private GameObject gridPrefab;
        [SerializeField] private GameObject rowPrefab;
        [SerializeField] private GameObject gridItemPrefabPrefab;
        [SerializeField] [Range(MinimumItems, MaximumRows)] public int amountOfRows = MaximumRows / 2;
        [SerializeField] [Range(MinimumItems, MaximumColumns)] public int amountOfColumns = MaximumColumns / 2;

        [ContextMenu(nameof(Initialise))]
        public void Initialise()
        {
            SetGridItems();
            SetNeighbors();
        }
        
        /// <summary>
        /// Returns the grid item at the given column (Key) and row (Value), or null when outside the grid.
        /// </summary>
        public GridItem GetItem(KeyValuePair<int, int> indices)
        {
            var x = indices.Key;
            var y = indices.Value;
            if (itemLookup == null || x < 0 || y < 0 || x >= itemLookup.GetLength(0) || y >= itemLookup.GetLength(1)) return null;
            return itemLookup[x, y];
        }

        /// <summary>
        /// Records a grid item in the column/row lookup used by GetItem.
        /// </summary>
        private void RegisterItem(GridItem gridItem, int column, int row)
        {
            gridItems.Add(gridItem);
            itemLookup[column, row] = gridItem;
        }

        #region Generate Grid
        #if UNITY_EDITOR
        private void OnValidate()
        {
            return;
            if (Application.isPlaying) return;
            if (generatingGrid) return;
            GenerateGrid();
        }

        [ContextMenu(nameof(ResetGridGenerationSystem))]
        public void ResetGridGenerationSystem()
        {
            generatingGrid = false;
            amountOfRows = MaximumRows / 2;
            amountOfColumns = MaximumColumns / 2;
            GenerateGrid();
        }
        #endif 
        
        private void GenerateGrid(Action completeCallback = null)
        {
            generatingGrid = true;
            ResetGrid(()=>CreateGrid(() =>
            {
                gridObject = GameObject.FindWithTag(GridTag);
                generatingGrid = false;
                completeCallback?.Invoke();
            }));
        }

        private void CreateGrid(Action completeCallback = null)
        {
            gridObject = Instantiate(gridPrefab, gridArea);
            itemLookup = new GridItem[amountOfColumns, amountOfRows];
            for (var i = 0; i < amountOfRows; i++)
            {
                var row = Instantiate(rowPrefab, gridObject.transform).transform;
                for (var j = 0; j < amountOfColumns; j++)
                {
                    if (Application.isPlaying)
                    {
                        var gridItem = Instantiate(gridItemPrefabPrefab, row).GetComponent<GridItem>();
                        gridItem.Initialise(new KeyValuePair<int, int>(j, i));
                        RegisterItem(gridItem, j, i);
                    }
                    else
                    {
                        Instantiate(gridItemPrefabPrefab, row);
                    }
                }
            }
            SetNeighbors();
            completeCallback?.Invoke();
        }
        
        private void SetNeighbors()
        {
            foreach (var item in gridItems)
            {
                item.SetNeighbors();
            }
        }
        
        /// <summary>
        /// Collects the grid items from the level hierarchy (rows of columns) and indexes them for constant-time lookups.
        /// </summary>
        private void SetGridItems()
        {
            gridItems.Clear();
            itemLookup = new GridItem[amountOfColumns, amountOfRows];
            gridObject = GameObject.FindWithTag(GridTag); //FindWithTag because caches can be reset in and out of play mode.
            for (var i = 0; i < amountOfRows; i++)
            {
                for (var j = 0; j < amountOfColumns; j++)
                {
                    var gridItem = gridObject.transform.GetChild(i).transform.GetChild(j).GetComponent<GridItem>();
                    RegisterItem(gridItem, j, i);
                    gridItem.Initialise(new KeyValuePair<int, int>(j, i));
                }
            }
        }

        private void ResetGrid(Action completeCallback = null)
        {
            gridObject = GameObject.FindWithTag(GridTag);
            if (!gridObject)
            {
                completeCallback?.Invoke();
                return;
            }
            if (Application.isPlaying)
            {
                Destroy(gridObject);
                completeCallback?.Invoke();
            }
            else
            {
                UnityEditor.EditorApplication.delayCall+=()=>
                {
                    DestroyImmediate(gridObject);
                    completeCallback?.Invoke();
                };
            }
        }
        #endregion Generate Grid
    }
}

