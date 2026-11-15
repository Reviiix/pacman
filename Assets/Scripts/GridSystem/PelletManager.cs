using System.Collections.Generic;
using Abstractions;
using GridSystem.GridItems;

namespace GridSystem
{
    /// <summary>
    /// Holds the level's pellet count for the win check.
    /// </summary>
    public class PelletManager : Singleton<PelletManager>
    {
        public int numberOfPellets { get; private set; }

        /// <summary>
        /// Takes the level's pellet count from the pellets themselves, wherever they sit in the hierarchy.
        /// </summary>
        public void Initialise(IReadOnlyCollection<Pellet> pellets)
        {
            numberOfPellets = pellets.Count;
        }
    }
}