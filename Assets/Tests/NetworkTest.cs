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
             *   |    ^^^^^ |
             *   |        ^ |
             *   |        ^ |
             *   |   *^^^^^ |
             *   +----------+
             */
            List<Network> networks;

            Company company = new Company();
            company.Money = 1000;
            company.Game = new Game();
            company.Game.LoadConfig();
            company.PlaceNode(Node.DR100, new TilePosition(4, 0));
            company.PlaceNode(Node.DR100, new TilePosition(3, 6));

            networks = company.Networks;
            Assert.AreEqual(0, networks.Count);

            company.PlaceCable(Cable.CAT3, new List<TilePosition>() {
                new TilePosition(4, 0),
                new TilePosition(4, 1),
                new TilePosition(4, 2),
                new TilePosition(4, 3),
            });

            company.PlaceCable(Cable.CAT3, new List<TilePosition>() {
                new TilePosition(4, 3),
                new TilePosition(5, 3),
                new TilePosition(6, 3),
                new TilePosition(7, 3),
                new TilePosition(8, 3),
            });

            networks = company.Networks;
            Assert.AreEqual(1, networks.Count);
            Assert.AreEqual(2, networks[0].Cables.Count);

            company.PlaceCable(Cable.CAT3, new List<TilePosition>() {
                new TilePosition(8, 3),
                new TilePosition(8, 4),
                new TilePosition(8, 5),
                new TilePosition(8, 6),
            });

            company.PlaceCable(Cable.CAT3, new List<TilePosition>() {
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
            Assert.AreEqual(2, networks[0].Nodes.Count);
            Assert.IsTrue(networks[0].ContainsPosition(new TilePosition(3, 6)));
            Assert.IsFalse(networks[0].ContainsPosition(new TilePosition(2, 2)));
            Assert.AreEqual(NetworkStatus.Disconnected, networks[0].Cables[0].Status);

            company.RemoveCablePosition(new TilePosition(8, 6));

            networks = company.Networks;
            Assert.AreEqual(2, networks.Count);
            Assert.AreEqual(3, networks[0].Cables.Count);
            Assert.AreEqual(1, networks[0].Nodes.Count);
            Assert.AreEqual(1, networks[1].Cables.Count);
            Assert.AreEqual(1, networks[1].Nodes.Count);
        }

        [Test]
        public void TestAvailableServices() {
            Game game = new Game();
            game.LoadConfig();
            
            MapConfiguration map = new MapConfiguration(10, 10);
            map.CreateTiles();
            game.Map = map;

            TilePosition hq = new TilePosition(1, 1);
            map.Tiles[hq].Lot = new Lot() {
                Tiles = new HashSet<TilePosition>() {
                    hq,
                    hq + new TilePosition(1, 0),
                    hq + new TilePosition(0, 1),
                    hq + new TilePosition(1, 1),
                },
                Anchor = hq,
                Building = new Building(map.Tiles[hq], BuildingType.Headquarters)
            };

            Company company = new Company() {
                Money = 100000,
                Game = game,
                HeadquartersLocation = hq
            };
            company.AppendRack();

            company.PlaceNode(Node.DR100, new TilePosition(2, 5));

            company.PlaceCable(Cable.CAT3, new List<TilePosition>() {
                new TilePosition(2, 2),
                new TilePosition(2, 3),
                new TilePosition(2, 4),
                new TilePosition(2, 5),
            });

            Assert.AreEqual(1, company.Networks.Count);
            Assert.IsTrue(company.Networks[0].Active);
            Assert.AreEqual(NetworkStatus.Active, company.Networks[0].Cables[0].Status);

            Assert.AreEqual(0, company.Networks[0].AvailableServices.Count);

            company.Purchase(game.Items["POTS-1"] as RackedItem, 1, 0);
            Assert.AreEqual(1, company.Networks[0].AvailableServices.Count);
            Assert.IsTrue(company.Networks[0].AvailableServices.Contains(Services.Phone));


            company.Purchase(game.Items["D-150"] as RackedItem, 1, 0);
            Assert.AreEqual(1, company.Networks[0].AvailableServices.Count);
            Assert.IsFalse(company.Networks[0].AvailableServices.Contains(Services.Broadband));


            company.Purchase(game.Items["R-2500"] as RackedItem, 1, 0);
            Assert.AreEqual(2, company.Networks[0].AvailableServices.Count);
            Assert.IsTrue(company.Networks[0].AvailableServices.Contains(Services.Broadband));

            company.Purchase(game.Items["TV-200"] as RackedItem, 1, 0);
            Assert.AreEqual(2, company.Networks[0].AvailableServices.Count);
            Assert.IsFalse(company.Networks[0].AvailableServices.Contains(Services.TV));


            company.PlaceCable(Cable.RG6, new List<TilePosition>() {
                new TilePosition(1, 1),
                new TilePosition(1, 0),
                new TilePosition(2, 0),
                new TilePosition(3, 0),
            });

            company.PlaceNode(Node.CR100, new TilePosition(3, 0));

            Assert.AreEqual(2, company.Networks.Count);

            Assert.AreEqual(2, company.Networks[0].AvailableServices.Count);
            Assert.IsTrue(company.Networks[1].AvailableServices.Contains(Services.TV));
            Assert.IsTrue(company.Networks[0].AvailableServices.Contains(Services.Broadband));
            Assert.IsTrue(company.Networks[0].AvailableServices.Contains(Services.Phone));

        }

        [Test]
        public void TestServiceArea() {
            /**
             *   +----------+
             *   |  HH*     |
             *   |  HH^     |
             *   |    ^     |
             *   |    ^^^^^ |
             *   |        ^ |
             *   |        ^ |
             *   |   %^^^^^ |
             *   +----------+
             */

            Game game = new Game();
            game.Customers = new List<Customer>();
            game.Companies = new List<Company>();
            game.LoadConfig();

            Company company = new Company();
            company.Money = 1000;
            company.Game = game;
            game.Companies.Add(company);

            MapConfiguration map = new MapConfiguration(10, 10);
            map.CreateTiles();
            game.Map = map;
            TilePosition hq = new TilePosition(2, 0);
            map.Tiles[hq].Lot = new Lot() {
                Tiles = new HashSet<TilePosition>() {
                    new TilePosition(2, 0),
                    new TilePosition(3, 0),
                    new TilePosition(2, 1),
                    new TilePosition(3, 1)
                },
                Anchor = hq,
                Building = new Building(map.Tiles[hq], BuildingType.Headquarters)
            };
            company.HeadquartersLocation = hq;

            company.PlaceNode(Node.DR100, new TilePosition(4, 0));

            company.PlaceCable(Cable.CAT3, new List<TilePosition>() {
                new TilePosition(3, 0),
                new TilePosition(4, 0),
                new TilePosition(4, 1),
                new TilePosition(4, 2),
                new TilePosition(4, 3),
            });

            company.PlaceCable(Cable.CAT3, new List<TilePosition>() {
                new TilePosition(4, 3),
                new TilePosition(5, 3),
                new TilePosition(6, 3),
                new TilePosition(7, 3),
                new TilePosition(8, 3),
            });

            company.PlaceCable(Cable.CAT3, new List<TilePosition>() {
                new TilePosition(8, 3),
                new TilePosition(8, 4),
                new TilePosition(8, 5),
                new TilePosition(8, 6),
            });

            company.PlaceCable(Cable.CAT3, new List<TilePosition>() {
                new TilePosition(8, 6),
                new TilePosition(7, 6),
                new TilePosition(6, 6),
                new TilePosition(5, 6),
                new TilePosition(4, 6),
                new TilePosition(3, 6),
            });

            Assert.IsTrue(company.ServiceArea.Contains(new TilePosition(4, 0)));
            Assert.IsTrue(company.ServiceArea.Contains(new TilePosition(3, 0)));
            Assert.IsTrue(company.ServiceArea.Contains(new TilePosition(5, 1)));
            Assert.IsTrue(company.ServiceArea.Contains(new TilePosition(7, 6)));
            Assert.IsFalse(company.ServiceArea.Contains(new TilePosition(7, 0)));
            Assert.IsFalse(company.ServiceArea.Contains(new TilePosition(5, 6)));
            Assert.IsTrue(company.PotentialServiceArea[CableType.Copper].Contains(new TilePosition(5, 7)));
            Assert.IsFalse(company.ServiceArea.Contains(new TilePosition(0, 0)));
            Assert.IsFalse(company.PotentialServiceArea[CableType.Copper].Contains(new TilePosition(0, 0)));


            company.PlaceNode(Node.DR100, new TilePosition(3, 6));

            Assert.IsTrue(company.ServiceArea.Contains(new TilePosition(4, 0)));
            Assert.IsTrue(company.ServiceArea.Contains(new TilePosition(3, 0)));
            Assert.IsTrue(company.ServiceArea.Contains(new TilePosition(5, 1)));
            Assert.IsTrue(company.ServiceArea.Contains(new TilePosition(7, 6)));
            Assert.IsFalse(company.ServiceArea.Contains(new TilePosition(7, 0)));
            Assert.IsTrue(company.ServiceArea.Contains(new TilePosition(5, 6)));
            Assert.IsFalse(company.ServiceArea.Contains(new TilePosition(0, 0)));
        }
       
    }
}