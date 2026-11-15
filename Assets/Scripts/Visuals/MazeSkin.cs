using System.Collections.Generic;
using GridSystem.GridItems;
using UnityEngine;

namespace Visuals
{
    /// <summary>
    /// The maze's look: autotiled wall sprites picked from each wall's neighbours, plus the corridor colour.
    /// </summary>
    [CreateAssetMenu(fileName = "MazeSkin", menuName = "Pacman/Maze Skin")]
    public class MazeSkin : ScriptableObject
    {
        /// <summary>
        /// How the wall sheet is laid out: 47 "blob" tiles using all 8 neighbours, or 16 tiles using the 4 orthogonal neighbours.
        /// </summary>
        public enum WallLayout { Blob47, Cardinal16 }

        private const int North = 1, NorthEast = 2, East = 4, SouthEast = 8, South = 16, SouthWest = 32, West = 64, NorthWest = 128;
        private static readonly int[] BlobTileIndexByMask = BuildBlobLookup();

        [SerializeField] private WallLayout layout = WallLayout.Blob47;
        [SerializeField] private Sprite[] wallTiles;
        [SerializeField] private Color wallTint = Color.white;
        [SerializeField] private Color corridorColor = Color.clear;
        [SerializeField] private bool treatOutsideAsWall;

        /// <summary>
        /// Skins every item in a column/row grid: walls get their autotile sprite, corridors get the corridor colour.
        /// </summary>
        public void Apply(GridItem[,] items)
        {
            var columns = items.GetLength(0);
            var rows = items.GetLength(1);
            for (var x = 0; x < columns; x++)
            {
                for (var y = 0; y < rows; y++)
                {
                    var item = items[x, y];
                    if (item == null) continue;
                    if (item is Wall) item.SetAppearance(GetWallSprite(items, x, y), wallTint);
                    else item.SetAppearance(null, corridorColor);
                }
            }
        }

        /// <summary>
        /// Returns the wall tile for the wall at column x, row y (row 0 is the top row).
        /// </summary>
        private Sprite GetWallSprite(GridItem[,] items, int x, int y)
        {
            var index = layout == WallLayout.Cardinal16 ? GetCardinalMask(items, x, y) : BlobTileIndexByMask[GetNeighbourMask(items, x, y)];
            if (wallTiles == null || index >= wallTiles.Length)
            {
                Debug.LogError($"{name} needs {(layout == WallLayout.Cardinal16 ? 16 : 47)} wall tiles but has {wallTiles?.Length ?? 0}.", this);
                return null;
            }
            return wallTiles[index];
        }

        /// <summary>
        /// Builds the 8-neighbour wall mask (N=1, NE=2, E=4, SE=8, S=16, SW=32, W=64, NW=128) for a cell.
        /// </summary>
        private int GetNeighbourMask(GridItem[,] items, int x, int y)
        {
            var mask = 0;
            if (IsWall(items, x, y - 1)) mask |= North;
            if (IsWall(items, x + 1, y - 1)) mask |= NorthEast;
            if (IsWall(items, x + 1, y)) mask |= East;
            if (IsWall(items, x + 1, y + 1)) mask |= SouthEast;
            if (IsWall(items, x, y + 1)) mask |= South;
            if (IsWall(items, x - 1, y + 1)) mask |= SouthWest;
            if (IsWall(items, x - 1, y)) mask |= West;
            if (IsWall(items, x - 1, y - 1)) mask |= NorthWest;
            return mask;
        }

        /// <summary>
        /// Builds the 4-neighbour wall mask (N=1, E=2, S=4, W=8) for a cell.
        /// </summary>
        private int GetCardinalMask(GridItem[,] items, int x, int y)
        {
            return (IsWall(items, x, y - 1) ? 1 : 0) | (IsWall(items, x + 1, y) ? 2 : 0) | (IsWall(items, x, y + 1) ? 4 : 0) | (IsWall(items, x - 1, y) ? 8 : 0);
        }

        /// <summary>
        /// Whether the cell is a wall, treating cells outside the grid as configured.
        /// </summary>
        private bool IsWall(GridItem[,] items, int x, int y)
        {
            if (x < 0 || y < 0 || x >= items.GetLength(0) || y >= items.GetLength(1)) return treatOutsideAsWall;
            return items[x, y] is Wall;
        }

        /// <summary>
        /// Clears diagonal bits whose two adjacent orthogonal neighbours are not both walls, as diagonals only matter for inner corners.
        /// </summary>
        private static int ReduceCorners(int mask)
        {
            if ((mask & North) == 0 || (mask & East) == 0) mask &= ~NorthEast;
            if ((mask & South) == 0 || (mask & East) == 0) mask &= ~SouthEast;
            if ((mask & South) == 0 || (mask & West) == 0) mask &= ~SouthWest;
            if ((mask & North) == 0 || (mask & West) == 0) mask &= ~NorthWest;
            return mask;
        }

        /// <summary>
        /// Maps all 256 neighbour masks to the 47 blob tiles, ordered by ascending reduced mask (the wall sheet's order).
        /// </summary>
        private static int[] BuildBlobLookup()
        {
            var reducedMasks = new SortedSet<int>();
            for (var mask = 0; mask < 256; mask++) reducedMasks.Add(ReduceCorners(mask));
            var tileIndexByReducedMask = new Dictionary<int, int>();
            foreach (var reducedMask in reducedMasks) tileIndexByReducedMask[reducedMask] = tileIndexByReducedMask.Count;
            var lookup = new int[256];
            for (var mask = 0; mask < 256; mask++) lookup[mask] = tileIndexByReducedMask[ReduceCorners(mask)];
            return lookup;
        }
    }
}
