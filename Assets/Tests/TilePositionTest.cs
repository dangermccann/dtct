﻿using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using DCTC.Map;
using DCTC.Model;

namespace DCTC.Test { 

    public class TilePositionTest {

        [Test]
        public void TestParse() {
            TilePosition pos = new TilePosition(23, 34);
            TilePosition pos2 = TilePosition.Parse(pos.ToString());
            Assert.AreEqual(pos, pos2);
        }

        [Test]
        public void TestInvalidInput1() {
            Assert.Throws<ArgumentException>( () => TilePosition.Parse("1,2"));
        }

        [Test]
        public void TestInvalidInput2() {
            Assert.Throws<ArgumentException>( () => TilePosition.Parse("(1)"));
        }

        [Test]
        public void TestEquatable() {
            Assert.IsTrue(new TilePosition(4, 5).Equals( new TilePosition(4, 5) )); 
            Assert.IsFalse(new TilePosition(5, 4).Equals( new TilePosition(4, 5) ));
        }

        [Test]
        public void TestEquatableOperator() {
            Assert.IsTrue(new TilePosition(4, 5) == new TilePosition(4, 5));
            Assert.IsTrue(new TilePosition(5, 4) != new TilePosition(4, 5));
        }
    }
}
