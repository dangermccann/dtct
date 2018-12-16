using NUnit.Framework;
using System.Collections.Generic;
using DCTC.Map;
using DCTC.Model;

namespace DCTC.Test {

    public class NetworkTest {
        [Test]
        public void TestNetworkCalculation() {
            /**
             *   +----------+
             *   |    *     |
             *   |    ^     |
             *   |    ^     |
             *   |    ^^^^* |
             *   |        ^ |
             *   |        ^ |
             *   |   *^^^^^ |
             *   +----------+
             */
            List<Network> networks;

            Company company = new Company();
            company.PlaceNode(NodeType.Small, new TilePosition(4, 0));
            company.PlaceNode(NodeType.Small, new TilePosition(8, 3));
            company.PlaceNode(NodeType.Small, new TilePosition(3, 6));

            networks = company.Networks;
            Assert.AreEqual(3, networks.Count);

            company.PlaceCable(CableType.Copper, new List<TilePosition>() {
                new TilePosition(4, 0),
                new TilePosition(4, 1),
                new TilePosition(4, 2),
                new TilePosition(4, 3),
            });

            company.PlaceCable(CableType.Copper, new List<TilePosition>() {
                new TilePosition(4, 3),
                new TilePosition(5, 3),
                new TilePosition(6, 3),
                new TilePosition(7, 3),
                new TilePosition(8, 3),
            });

            networks = company.Networks;
            Assert.AreEqual(2, networks.Count);
            Assert.AreEqual(2, networks[0].Cables.Count);
            Assert.AreEqual(0, networks[1].Cables.Count);

            company.PlaceCable(CableType.Copper, new List<TilePosition>() {
                new TilePosition(8, 3),
                new TilePosition(8, 4),
                new TilePosition(8, 5),
                new TilePosition(8, 6),
            });

            company.PlaceCable(CableType.Copper, new List<TilePosition>() {
                new TilePosition(8, 6),
                new TilePosition(7, 6),
                new TilePosition(6, 6),
                new TilePosition(5, 6),
                new TilePosition(4, 6),
                new TilePosition(3, 6),
            });

            networks = company.Networks;
            Assert.AreEqual(1, networks.Count);
            Assert.AreEqual(4, networks[0].Cables.Count);
            Assert.AreEqual(3, networks[0].Nodes.Count);
            Assert.IsTrue(networks[0].ContainsPosition(new TilePosition(3, 6)));
            Assert.IsFalse(networks[0].ContainsPosition(new TilePosition(2, 2)));

            company.RemoveCablePosition(new TilePosition(8, 6));

            networks = company.Networks;
            Assert.AreEqual(2, networks.Count);
            Assert.AreEqual(3, networks[0].Cables.Count);
            Assert.AreEqual(2, networks[0].Nodes.Count);
            Assert.AreEqual(1, networks[1].Cables.Count);
            Assert.AreEqual(1, networks[1].Nodes.Count);
        }
       
    }
}