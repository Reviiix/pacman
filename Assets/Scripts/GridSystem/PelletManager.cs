using System;
using System.Collections.Generic;
using Abstractions;
using GridSystem.GridItems;
using UnityEngine;
using UnityEngine.Serialization;

namespace GridSystem
{
    [ExecuteInEditMode]
    public class PelletManager : Singleton<PelletManager>
    {
        [SerializeField] private List<Blank> allActivePellets = new List<Blank>();
        public int numberOfPellets { get; private set; }

        /// <summary>
        /// Takes the level's pellet count from the pellets themselves, wherever they sit in the hierarchy.
        /// </summary>
        public void Initialise(IReadOnlyCollection<Pellet> pellets)
        {
            numberOfPellets = pellets.Count;
        }

        public void AddPelletToList(Blank gridItem)
        {
            if (allActivePellets.Contains(gridItem)) return;
            allActivePellets.Add(gridItem);
        }

        public void RemovePelletFromList(Blank gridItem)
        {
            if (!allActivePellets.Contains(gridItem)) return;
            allActivePellets.Remove(gridItem);
        }
    }
}