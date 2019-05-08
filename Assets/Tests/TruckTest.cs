using NUnit.Framework;
using DCTC.Model;
using DCTC.Map;
using System.Collections.Generic;

namespace DCTC.Test {
    public class TruckTest {

        [Test]
        public void TestEquipmentForServices() {
            Items items = Loader.LoadItems();
            List<CPE> available = new List<CPE>() {
                items.CPE["RJ-11"],
                items.CPE["STB-5"],
                items.CPE["M-101"],
                items.CPE["M-301"]
            };

            List<CPE> results;
            bool success;
            Truck truck = new Truck();

            success = Truck.EquipmentForServices(new List<Services>() { Services.TV }, available, out results);
            Assert.IsTrue(success);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(items.CPE["STB-5"], results[0]);


            success = Truck.EquipmentForServices(new List<Services>() { Services.TV, Services.Broadband }, available, out results);
            Assert.IsTrue(success);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(items.CPE["M-301"], results[0]);


            success = Truck.EquipmentForServices(new List<Services>() { Services.TV, Services.Broadband, Services.Phone }, available, out results);
            Assert.IsTrue(success);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(items.CPE["M-301"], results[0]);



            available = new List<CPE>() {
                items.CPE["RJ-11"],
                items.CPE["STB-5"],
            };

            success = Truck.EquipmentForServices(new List<Services>() { Services.Broadband }, available, out results);
            Assert.IsFalse(success);
            Assert.AreEqual(0, results.Count);

            success = Truck.EquipmentForServices(new List<Services>() { Services.Broadband, Services.Phone }, available, out results);
            Assert.IsFalse(success);
            Assert.AreEqual(1, results.Count);

            success = Truck.EquipmentForServices(new List<Services>() { Services.TV, Services.Phone }, available, out results);
            Assert.IsTrue(success);
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.Contains(items.CPE["RJ-11"]));
            Assert.IsTrue(results.Contains(items.CPE["STB-5"]));


        }

        [Test]
        public void TestDispatch() {
            /**
             *   +----------+
             *   |hh#       |
             *   |hH#       |
             *   |  #       |
             *   |  #       |
             *   |  #       |
             *   |  #C      |
             *   |  #C      |
             *   |          |
             *   |          |
             *   |          |
             *   +----------+
             */

            Game game = new Game();
            game.Customers = new List<Customer>();
            game.Companies = new List<Company>();
            game.LoadConfig();

            MapConfiguration map = new MapConfiguration(10, 10);
            map.CreateTiles();
            game.Map = map;

            TilePosition hq = new TilePosition(1, 1);
            map.Tiles[hq].Lot = new Lot() {
                Tiles = new HashSet<TilePosition>() {
                    new TilePosition(0, 0),
                    new TilePosition(1, 0),
                    new TilePosition(0, 1),
                    new TilePosition(1, 1)
                },
                Anchor = hq,
                Building = new Building(map.Tiles[hq], BuildingType.Headquarters)
            };

            map.AddRoad(new TilePosition(2, 0));
            map.AddRoad(new TilePosition(2, 1));
            map.AddRoad(new TilePosition(2, 2));
            map.AddRoad(new TilePosition(2, 3));
            map.AddRoad(new TilePosition(2, 4));
            map.AddRoad(new TilePosition(2, 5));
            map.AddRoad(new TilePosition(2, 6));

            Customer customer1 = new Customer() {
                ID = "C1",
                Name = "CC 1",
                HomeLocation = new TilePosition(3, 5),
                Status = CustomerStatus.NoProvider,
                Equipment = new Inventory(),
                Game = game
            };
            game.Customers.Add(customer1);

            Customer customer2 = new Customer() {
                ID = "C2",
                Name = "CC 2",
                HomeLocation = new TilePosition(3, 6),
                Status = CustomerStatus.NoProvider,
                Equipment = new Inventory(),
                Game = game
            };
            game.Customers.Add(customer2);

            Company company = new Company() {
                ID = "company",
                Money = 100000,
                Game = game,
                HeadquartersLocation = hq,
                Trucks = new List<Truck>()
            };
            game.Companies.Add(company);
            company.AppendRack();
            company.CallCenter.Company = company;

            company.Trucks.Add(new Truck() {
                Position = ThreeDMap.PositionToWorld(new TilePosition(2, 2)),
                Status = TruckStatus.Idle,
                Company = company,
                Game = game
            });

            Truck truck = company.Trucks[0];

            // Test case where truck has the item in inventory
            truck.Inventory.Add("RJ-11", 1);
            customer1.Services = new List<Services>() { Services.Phone };
            customer1.ProviderID = company.ID;
            customer1.Status = CustomerStatus.Pending;
            company.Customers.Add(customer1);

            company.RollTruck(customer1);
            Assert.AreEqual(1, company.TruckRollQueue.Count);

            // Advance so truck starts route
            company.LightUpdate(1);
            company.Update(1);

            Assert.AreEqual(TruckStatus.EnRoute, truck.Status);
            Assert.AreEqual(0, company.TruckRollQueue.Count);

            // Advance so truck completes job
            for (int i = 0; i < 10; i++) {
                company.LightUpdate(1);
                company.Update(1);
            }

            Assert.AreEqual(TruckStatus.Idle, truck.Status);
            Assert.IsTrue(customer1.Equipment.Contains("RJ-11"));
            Assert.IsFalse(truck.Inventory.Contains("RJ-11"));


            // Test case where there is no available inventory
            customer2.Services = new List<Services>() { Services.TV };
            customer2.ProviderID = company.ID;
            customer2.Status = CustomerStatus.Pending;
            company.Customers.Add(customer2);

            Assert.IsFalse(truck.Dispatch(customer2.ID));

            company.Inventory.Add("STB-5", 1);

            company.RollTruck(customer2);
            company.Update(1);
            Assert.AreEqual(TruckStatus.GettingEquipment, truck.Status);

            for (int i = 0; i < 5; i++) {
                company.LightUpdate(1);
                company.Update(1);
            }

            Assert.AreEqual(TruckStatus.EnRoute, truck.Status);
            Assert.IsTrue(truck.Inventory.Contains("STB-5"));
            Assert.IsFalse(company.Inventory.Contains("STB-5"));

            for (int i = 0; i < 15; i++) {
                company.LightUpdate(1);
                company.Update(1);
            }

            Assert.AreEqual(TruckStatus.Idle, truck.Status);
            Assert.IsFalse(truck.Inventory.Contains("STB-5"));
            Assert.IsFalse(company.Inventory.Contains("STB-5"));
            Assert.IsTrue(customer2.Equipment.Contains("STB-5"));
        }
    }
}