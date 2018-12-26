using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using DCTC.Map;
using DCTC.UI;

namespace DCTC.Test {
    public class LotOutlineTest {
        [Test]
        public void TestSimple() {
            var go = new GameObject("Test");
            LotOutline outline = go.AddComponent<LotOutline>();
            outline.Positions = new List<TilePosition>() { new TilePosition(1, 1) };
            List<Vector3> points = new List<Vector3>(outline.CalculatePoints());
            Assert.IsTrue(points.Contains(new Vector3(2, 0.1f, 2)));
            Assert.IsTrue(points.Contains(new Vector3(2, 0.1f, 4)));
            Assert.IsTrue(points.Contains(new Vector3(4, 0.1f, 4)));
            Assert.IsTrue(points.Contains(new Vector3(4, 0.1f, 2)));
            Assert.AreEqual(4, points.Count);
        }


        [Test]
        public void TestSmallRect() {
            var go = new GameObject("Test");
            LotOutline outline = go.AddComponent<LotOutline>();
            outline.Positions = new List<TilePosition>() {
                new TilePosition(1, 1),
                new TilePosition(1, 2),
                new TilePosition(1, 3),
                new TilePosition(1, 4)
            };
            List<Vector3> points = new List<Vector3>(outline.CalculatePoints());
            Assert.IsTrue(points.Contains(new Vector3(2, 0.1f, 2)));
            Assert.IsTrue(points.Contains(new Vector3(2, 0.1f, 10)));
            Assert.IsTrue(points.Contains(new Vector3(4, 0.1f, 10)));
            Assert.IsTrue(points.Contains(new Vector3(4, 0.1f, 2)));
            Assert.AreEqual(10, points.Count);
        }

        [Test]
        public void TestRectangle() {
            var go = new GameObject("Test");
            LotOutline outline = go.AddComponent<LotOutline>();
            var positions = new List<TilePosition>();
            for(int x = 0; x < 4; x++) {
                for(int y = 0; y < 4; y++) {
                    positions.Add(new TilePosition(x, y));
                }
            }
            outline.Positions = positions;

            List<Vector3> points = new List<Vector3>(outline.CalculatePoints());
            Assert.IsTrue(points.Contains(new Vector3(0, 0.1f, 0)));
            Assert.IsTrue(points.Contains(new Vector3(0, 0.1f, 8)));
            Assert.IsTrue(points.Contains(new Vector3(8, 0.1f, 8)));
            Assert.IsTrue(points.Contains(new Vector3(8, 0.1f, 0)));
            Assert.AreEqual(16, points.Count);
        }

        [Test]
        public void TestComplex() {
            var go = new GameObject("Test");
            LotOutline outline = go.AddComponent<LotOutline>();
            var positions = new List<TilePosition>();
            for (int x = 0; x <= 5; x++) {
                for (int y = 2; y <= 3; y++) {
                    positions.Add(new TilePosition(x, y));
                }
            }

            for (int x = 2; x <= 3; x++) {
                for (int y = 0; y <= 5; y++) {
                    TilePosition pos = new TilePosition(x, y);
                    if(!positions.Contains(pos))
                        positions.Add(pos);
                }
            }
            outline.Positions = positions;

            List<Vector3> points = new List<Vector3>(outline.CalculatePoints());
            Assert.IsFalse(points.Contains(new Vector3(0, 0.1f, 0)));
            Assert.IsTrue(points.Contains(new Vector3(4, 0.1f, 0)));
            Assert.IsTrue(points.Contains(new Vector3(4, 0.1f, 10)));
            Assert.IsTrue(points.Contains(new Vector3(10, 0.1f, 4)));

        }
    }
}