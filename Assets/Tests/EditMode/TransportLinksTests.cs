using GridSystem.Transport.Logic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Checks how TransportLinks pairs transport tiles and when arriving on one sends the player to its partner.
    /// </summary>
    public class TransportLinksTests
    {
        private static readonly Vector2Int Left = new(1, 10);
        private static readonly Vector2Int Right = new(15, 10);
        private static readonly Vector2Int Other = new(5, 5);

        /// <summary>
        /// Arriving on either tile of a linked pair exits at the other.
        /// </summary>
        [Test]
        public void ArrivingOnALinkedTileExitsAtItsPartnerBothWays()
        {
            var links = new TransportLinks();
            Assert.IsTrue(links.TryLink(Left, Right));
            Assert.IsTrue(links.TryGetExitOnArrival(new Vector2Int(2, 10), Left, out var fromLeft));
            Assert.AreEqual(Right, fromLeft);
            Assert.IsTrue(links.TryGetExitOnArrival(new Vector2Int(14, 10), Right, out var fromRight));
            Assert.AreEqual(Left, fromRight);
        }

        /// <summary>
        /// A character that stays on a transport tile, such as one blocked by a wall, is not sent anywhere.
        /// </summary>
        [Test]
        public void StayingOnATransportTileDoesNotTransport()
        {
            var links = new TransportLinks();
            links.TryLink(Left, Right);
            Assert.IsFalse(links.TryGetExitOnArrival(Left, Left, out _));
        }

        /// <summary>
        /// Arriving on a cell that is not a transport tile is not a transport.
        /// </summary>
        [Test]
        public void ArrivingOnAnUnlinkedCellDoesNotTransport()
        {
            var links = new TransportLinks();
            links.TryLink(Left, Right);
            Assert.IsFalse(links.TryGetExitOnArrival(Left, Other, out _));
        }

        /// <summary>
        /// A cell cannot be linked to itself.
        /// </summary>
        [Test]
        public void LinkingACellToItselfIsRefused()
        {
            var links = new TransportLinks();
            Assert.IsFalse(links.TryLink(Left, Left));
            Assert.AreEqual(0, links.LinkedCellCount);
        }

        /// <summary>
        /// A cell already linked to one partner cannot be linked to another, and keeps its first partner.
        /// </summary>
        [Test]
        public void LinkingACellToASecondPartnerIsRefused()
        {
            var links = new TransportLinks();
            links.TryLink(Left, Right);
            Assert.IsFalse(links.TryLink(Left, Other));
            Assert.IsFalse(links.TryLink(Other, Right));
            Assert.IsTrue(links.TryGetExitOnArrival(Other, Left, out var exit));
            Assert.AreEqual(Right, exit);
            Assert.AreEqual(2, links.LinkedCellCount);
        }

        /// <summary>
        /// Linking the same pair again, as each pad of a pair does, is accepted and changes nothing.
        /// </summary>
        [Test]
        public void LinkingTheSamePairTwiceIsAccepted()
        {
            var links = new TransportLinks();
            Assert.IsTrue(links.TryLink(Left, Right));
            Assert.IsTrue(links.TryLink(Right, Left));
            Assert.AreEqual(2, links.LinkedCellCount);
        }
    }
}
