using System.Collections.Generic;
using UnityEngine;

namespace GridSystem.Transport.Logic
{
    /// <summary>
    /// Pairs transport tiles by grid cell and decides when arriving on one sends the player to its partner.
    /// </summary>
    public class TransportLinks
    {
        private readonly Dictionary<Vector2Int, Vector2Int> partners = new();

        /// <summary>
        /// How many cells are transport tiles (two per linked pair).
        /// </summary>
        public int LinkedCellCount => partners.Count;

        /// <summary>
        /// Links two different cells to each other both ways; refuses a cell linked to itself or to a different partner already, and accepts the same pair twice.
        /// </summary>
        public bool TryLink(Vector2Int first, Vector2Int second)
        {
            if (first == second) return false;
            if (IsLinkedElsewhere(first, second) || IsLinkedElsewhere(second, first)) return false;
            partners[first] = second;
            partners[second] = first;
            return true;
        }

        /// <summary>
        /// Whether moving from one cell onto another lands on a transport tile, giving its partner as the exit; staying on the same cell never does, so a character resting on a tile is not sent back and forth.
        /// </summary>
        public bool TryGetExitOnArrival(Vector2Int from, Vector2Int to, out Vector2Int exit)
        {
            exit = default;
            return from != to && partners.TryGetValue(to, out exit);
        }

        /// <summary>
        /// Whether a cell is already linked to a cell other than the given one.
        /// </summary>
        private bool IsLinkedElsewhere(Vector2Int cell, Vector2Int wanted)
        {
            return partners.TryGetValue(cell, out var partner) && partner != wanted;
        }
    }
}
