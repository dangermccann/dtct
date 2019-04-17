using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;
using DCTC.Map;
using DCTC.Model;

namespace DCTC.Test {
    public class TestMap {

        [Test]
        public void TestOppositeDirection() {
            Assert.AreEqual(Direction.South, MapConfiguration.OppositeDirection(Direction.North));
            Assert.AreEqual(Direction.East, MapConfiguration.OppositeDirection(Direction.West));
        }

        [Test]
        public void TestNextTile() {
            TilePosition pos = new TilePosition(3, 4);
            Assert.AreEqual(new TilePosition(4, 4), MapConfiguration.NextTile(pos, Direction.East));
            Assert.AreEqual(new TilePosition(3, 3), MapConfiguration.NextTile(pos, Direction.South));
        }

        [Test]
        public void TestCornerDirection() {
            Assert.AreEqual(Direction.None, MapConfiguration.CornerDirection(new List<Direction>(), Direction.None));

            List<Direction> dirs = new List<Direction>() { Direction.North, Direction.East };
            Assert.AreEqual(Direction.East, MapConfiguration.CornerDirection(dirs, Direction.North));
            Assert.AreEqual(Direction.North, MapConfiguration.CornerDirection(dirs, Direction.East));

            dirs = new List<Direction>() { Direction.North, Direction.South };
            Assert.AreEqual(Direction.None, MapConfiguration.CornerDirection(dirs, Direction.North));
        }

        [Test]
        public void TestAdjacentPositions() {
            MapConfiguration map = new MapConfiguration(10, 10);

            List<TilePosition> adjacent = map.AdjacentPositions(new TilePosition(5, 5));
            Assert.AreEqual(4, adjacent.Count);
            Assert.IsTrue(adjacent.Contains(new TilePosition(6, 5)));
            Assert.IsTrue(adjacent.Contains(new TilePosition(5, 6)));


            adjacent = map.AdjacentPositions(new TilePosition(9, 9));
            Assert.AreEqual(2, adjacent.Count);
            Assert.IsTrue(adjacent.Contains(new TilePosition(8, 9)));
            Assert.IsFalse(adjacent.Contains(new TilePosition(9, 9)));
        }

        [Test]
        public void TestAdjacentRoads() {
            MapConfiguration map = new MapConfiguration(10, 10);
            map.CreateTiles();

            map.AddRoad(new TilePosition(1, 1));
            map.AddRoad(new TilePosition(2, 1));
            map.AddRoad(new TilePosition(3, 1));
            map.AddRoad(new TilePosition(3, 2));
            map.AddRoad(new TilePosition(3, 0));

            List<Direction> roads = map.AdjacentRoads(new TilePosition(0, 0));
            Assert.AreEqual(0, roads.Count);

            roads = map.AdjacentRoads(new TilePosition(1, 0));
            Assert.AreEqual(1, roads.Count);
            Assert.AreEqual(Direction.North, roads[0]);

            roads = map.AdjacentRoads(new TilePosition(2, 2));
            Assert.AreEqual(2, roads.Count);
            Assert.IsTrue(roads.Contains(Direction.South));
            Assert.IsTrue(roads.Contains(Direction.East));
            Assert.IsFalse(roads.Contains(Direction.West));
        }

        [Test]
        public void TestRoads() {
            MapConfiguration map = new MapConfiguration(10, 10);
            map.CreateTiles();

            map.AddRoad(new TilePosition(1, 1));
            map.AddRoad(new TilePosition(2, 1));
            map.AddRoad(new TilePosition(3, 1));
            map.AddRoad(new TilePosition(3, 2));
            map.AddRoad(new TilePosition(3, 0));
            map.AddRoad(new TilePosition(4, 2));

            Assert.AreEqual(RoadType.IntersectW, map.Tiles[new TilePosition(3, 1)].RoadType);
            Assert.AreEqual(RoadType.CornerSE, map.Tiles[new TilePosition(3, 2)].RoadType);
            Assert.AreEqual(RoadType.Horizontal, map.Tiles[new TilePosition(1, 1)].RoadType);

            map.AddRoad(new TilePosition(4, 1));
            Assert.AreEqual(RoadType.IntersectAll, map.Tiles[new TilePosition(3, 1)].RoadType);
        }

        [Test]
        public void TestBoundingBox() {
            TileRectangle tr = MapConfiguration.BoundingBox(new TilePosition(10, 5),
                                                            new TilePosition(5, 10));
            Assert.AreEqual(5, tr.Left);
            Assert.AreEqual(5, tr.Bottom);
            Assert.AreEqual(10, tr.Right);
            Assert.AreEqual(10, tr.Top);
        }

        [Test]
        public void TestExpandBoundingBox() {
            TileRectangle tr = MapConfiguration.BoundingBox(new TilePosition(10, 5),
                                                            new TilePosition(5, 10));
            MapConfiguration map = new MapConfiguration(11, 11);

            tr = map.ExpandBoundingBox(tr, 2);
            Assert.AreEqual(3, tr.Left);
            Assert.AreEqual(3, tr.Bottom);
            Assert.AreEqual(10, tr.Right);
            Assert.AreEqual(10, tr.Top);
        }

        [Test]
        public void TestArea() {
            MapConfiguration map = new MapConfiguration(100, 100);
            TileRectangle rect;

            rect = map.Area(new TilePosition(10, 10), Direction.North, 10);
            Assert.AreEqual(20, rect.Top);
            Assert.AreEqual(10, rect.Bottom);
            Assert.AreEqual(5, rect.Left);
            Assert.AreEqual(15, rect.Right);

            rect = map.Area(new TilePosition(10, 10), Direction.South, 10);
            Assert.AreEqual(10, rect.Top);
            Assert.AreEqual(0, rect.Bottom);
            Assert.AreEqual(5, rect.Left);
            Assert.AreEqual(15, rect.Right);

            rect = map.Area(new TilePosition(10, 10), Direction.East, 10);
            Assert.AreEqual(15, rect.Top);
            Assert.AreEqual(5, rect.Bottom);
            Assert.AreEqual(10, rect.Left);
            Assert.AreEqual(20, rect.Right);

            rect = map.Area(new TilePosition(5, 5), Direction.West, 10);
            Assert.AreEqual(10, rect.Top);
            Assert.AreEqual(0, rect.Bottom);
            Assert.AreEqual(0, rect.Left);
            Assert.AreEqual(5, rect.Right);
        }
    }
}