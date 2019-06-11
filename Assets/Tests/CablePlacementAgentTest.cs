using NUnit.Framework;
using DCTC.Model;
using DCTC.Map;
using DCTC.AI;
using System.Collections.Generic;
using System.Linq;

namespace DCTC.Test {

    public class CablePlacementAgentTest {

        private Game NewGame() {
            NewGameSettings settings = new NewGameSettings() {
                NeighborhoodCountX = 1,
                NeighborhoodCountY = 1,
                NumAIs = 1,
                NumHumans = 0,
                Seed = -1
            };

            System.Random random = new System.Random(settings.Seed);
            NameGenerator nameGenerator = new NameGenerator(random);

            MapGenerator generator = new MapGenerator(random, settings, nameGenerator);
            MapConfiguration map = generator.Generate();
            IList<TilePosition> hq = generator.GenerateHeadquarters(map, settings.NumAIs + settings.NumHumans);
            Assert.NotNull(hq);

            Game game = new Game();
            game.LoadConfig();
            game.Customers = new List<Customer>();
            game.NewGame(settings, nameGenerator, map, hq);
            game.PopulateCustomers();

            return game;
        }

        [Test]
        public void TestFunctions() {
            Game game = NewGame();
            Company company = game.Companies[0];

            CablePlacementAgent agent = new CablePlacementAgent();
            agent.Company = company;
            TileRectangle rect = agent.ExpansionArea(new List<TilePosition>() { company.HeadquartersLocation }, Direction.South, 10);

            Assert.AreEqual(10, rect.Width);
            Assert.AreEqual(10, rect.Height);

            Assert.AreEqual(Orientation.Horizontal, agent.PrimaryRoadOrientation(rect));

            IEnumerable<TilePosition> result = agent.FilterRoads(rect.Positions, Orientation.Horizontal);
            Assert.AreEqual(RoadType.Horizontal, game.Map.Tiles[result.First()].RoadType);

        }
    }
}