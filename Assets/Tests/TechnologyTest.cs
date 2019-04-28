using NUnit.Framework;
using DCTC.Model;
using System.Linq;
using System.Collections.Generic;

namespace DCTC.Test {
    public class TechnologyTest {
        [Test]
        public void TestFlatten() {
            Dictionary<string, Technology> techs = Loader.LoadTechnologies();
            Technology first = Technology.BuildGraph(techs);

            List<Technology> flat = Technology.Flatten(first);
            Assert.IsTrue(flat.Count > 3);
            Assert.AreEqual(1, flat.Count(t => t.ID == "Telephony"));
        }

        [Test]
        public void TestThing() {
            var all = Loader.LoadTechnologies();
            var first = Technology.BuildGraph(all);

            var techs = new Dictionary<string, float>();
            var flat = Technology.Flatten(first);
            foreach(Technology tech in flat) {
                techs.Add(tech.ID, 0);
            }

            var available = Technology.AvailableTechnologies(first, techs);
            Assert.AreEqual(2, available.Count);
            Assert.AreEqual(1, available.Count(t => t.ID == "Analog Transmissions"));

            techs["Analog Transmissions"] = 10;
            available = Technology.AvailableTechnologies(first, techs);
            Assert.AreEqual(3, available.Count);
            Assert.AreEqual(1, available.Count(t => t.ID == "Digital Video"));
            Assert.AreEqual(1, available.Count(t => t.ID == "Digital Subscriber Lines"));
        }

        [Test]
        public void TestFind() {
            var all = Loader.LoadTechnologies();
            var first = Technology.BuildGraph(all);

            var tech = first.Find("Fiber Optics");
            Assert.IsNotNull(tech);
        }
    }
}
