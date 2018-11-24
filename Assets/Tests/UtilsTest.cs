using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;
using DCTC;
using DCTC.Model;

namespace DCTC.Test {
    public class UtilsTest {


        [Test]
        public void TestRemoveAll() {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("1", "test");
            dic.Add("2", "test");
            dic.Add("3", "X");
            dic.Add("4", "XX");
            dic.Add("5", "XXX");

            Assert.IsTrue(dic.ContainsValue("test"));
            dic.RemoveAll((key, value) => value == "test");
            Assert.AreEqual(3, dic.Count);
            Assert.IsFalse(dic.ContainsValue("test"));

            dic.RemoveAll((key, value) => value == "X");
            Assert.AreEqual(2, dic.Count);
        }
    }
}
