using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;
using DCTC.Map;
using DCTC.Model;

namespace DCTC.Test {
    public class PlacementTest {
        [Test]
        public void TestCablePlacement() {
            Company company = new Company();

            int addedCount = 0;
            company.ItemAdded += (object item) => {
                addedCount++;
            };

            int removedCount = 0;
            List<Cable> removed = new List<Cable>();
            company.ItemRemoved += (object item) => {
                removedCount++;
                removed.Add(item as Cable);
            };

            Cable c1 = company.PlaceCable(CableType.Copper, new List<TilePosition>() {
                new TilePosition(3, 0),
                new TilePosition(3, 1),
                new TilePosition(3, 2),
                new TilePosition(3, 3),
                new TilePosition(3, 4),
                new TilePosition(3, 5),
            });

            Cable c2 = company.PlaceCable(CableType.Copper, new List<TilePosition>() {
                new TilePosition(0, 3),
                new TilePosition(1, 3),
                new TilePosition(2, 3),
                new TilePosition(3, 3),
                new TilePosition(4, 3),
                new TilePosition(5, 3),
            });

            Assert.AreEqual(2, addedCount);
            Assert.AreEqual(2, company.Cables.Count);

            company.RemoveCablePosition(new TilePosition(3, 0));
            Assert.AreEqual(1, removedCount);
            Assert.AreEqual(3, addedCount);
            Assert.AreEqual(5, c1.Positions.Count);

            company.RemoveCablePosition(new TilePosition(3, 3));
            Assert.AreEqual(7, addedCount);
            Assert.AreEqual(4, company.Cables.Count);
            Assert.AreEqual(3, removedCount);
            Assert.That(removed.Contains(c1));
            Assert.That(removed.Contains(c2));
        }
    }
}