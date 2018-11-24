
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;
using DCTC.Map;
using DCTC.Model;

namespace DCTC.Test
{
    public class StreetTest
	{
        [Test]
        public void TestAllTiles() {
			MapConfiguration map = new MapConfiguration(10, 10);
			Street street = new Street(map);
			street.AddSegment(new TilePosition(1, 1), new TilePosition(1, 6));
			street.AddSegment(new TilePosition(1, 6), new TilePosition(6, 6));

			HashSet<TilePosition> all = new HashSet<TilePosition>(street.AllTiles);
			Assert.AreEqual(11, all.Count);

			Assert.IsTrue(all.Contains(new TilePosition(1, 3)));
			Assert.IsTrue(all.Contains(new TilePosition(3, 6)));
			Assert.IsTrue(all.Contains(new TilePosition(6, 6)));
			Assert.IsTrue(all.Contains(new TilePosition(1, 1)));
			Assert.IsFalse(all.Contains(new TilePosition(0, 1)));
			Assert.IsFalse(all.Contains(new TilePosition(2, 2)));
			Assert.IsFalse(all.Contains(new TilePosition(0, 6)));


			Assert.IsTrue(street.Contains(new TilePosition(1, 3)));
			Assert.IsTrue(street.Contains(new TilePosition(3, 6)));
			Assert.IsTrue(street.Contains(new TilePosition(6, 6)));
			Assert.IsTrue(street.Contains(new TilePosition(1, 1)));
			Assert.IsFalse(street.Contains(new TilePosition(0, 1)));
			Assert.IsFalse(street.Contains(new TilePosition(2, 2)));
			Assert.IsFalse(street.Contains(new TilePosition(0, 6)));
		}

        [Test]
        public void TestSegmentPositions() {
			MapConfiguration map = new MapConfiguration(10, 10);
			Street street = new Street(map);
			street.AddSegment(new TilePosition(1, 1), new TilePosition(1, 6));
			street.AddSegment(new TilePosition(1, 6), new TilePosition(6, 6));

			IList<TilePosition> positions = street.Segments[0].Positions;
			Assert.AreEqual(6, positions.Count);
			Assert.AreEqual(new TilePosition(1, 1), positions[0]);
			Assert.AreEqual(new TilePosition(1, 6), positions[5]);

			positions = street.Segments[1].Positions;
			Assert.AreEqual(6, positions.Count);
			Assert.AreEqual(new TilePosition(1, 6), positions[0]);
			Assert.AreEqual(new TilePosition(5, 6), positions[4]);

		}
	}
}

