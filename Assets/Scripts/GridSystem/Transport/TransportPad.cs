using UnityEngine;

namespace GridSystem.Transport
{
    /// <summary>
    /// Marks the grid tile it sits on as a transport tile that sends the player to its partner pad's tile.
    /// </summary>
    public class TransportPad : MonoBehaviour
    {
        [SerializeField] [Tooltip("The pad this one sends the player to. Link both pads of a pair to each other.")]
        private TransportPad partner;

        /// <summary>
        /// The pad this one sends the player to.
        /// </summary>
        public TransportPad Partner => partner;
    }
}
