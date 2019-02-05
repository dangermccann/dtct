using NUnit.Framework;
using DCTC.Model;
using System.Linq;
using System.Collections.Generic;

namespace DCTC.Test {
    public class ItemsTest {
        [Test]
        public void TestLoad() {
            Items items = Loader.LoadItems();
            Assert.Greater(items.CableAttributes.Count, 0);
            Assert.Greater(items.NodeAttributes.Count, 0);
            Assert.Greater(items.Termination.Count, 0);
            Assert.Greater(items.Backhaul.Count, 0);
            Assert.Greater(items.Fan.Count, 0);
            Assert.Greater(items.Rack.Count, 0);
            Assert.Greater(items.CPE.Count, 0);
            Assert.NotNull(items.CableAttributes.First().Value.ID);
            Assert.NotNull(items.NodeAttributes.First().Value.ID);
        }

        [Test]
        public void TestTechnologies() {
            Dictionary<string, Technology> techs = Loader.LoadTechnologies();
            Assert.Greater(techs.Count, 1);
            Assert.NotNull(techs.First().Value.ID);
        }

        [Test]
        public void TestTechGraph() {
            Dictionary<string, Technology> techs = Loader.LoadTechnologies();
            Technology first = Technology.BuildGraph(techs);
            Assert.NotNull(first);
            Assert.Greater(first.DependantTechnologies.Count, 0);
            Assert.NotNull(first.DependantTechnologies[0].ID);
            Assert.AreEqual(first.DependantTechnologies[0].Prerequisite, first.ID);
            Assert.AreEqual(first.DependantTechnologies[0].PrerequisiteTechnology, first);
        }
    }
}
