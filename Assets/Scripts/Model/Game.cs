using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using DCTC.Map;
using UnityEngine;

namespace DCTC.Model {

    [Serializable]
    public class NewGameSettings {
        public int Seed = 0;
        public int NumAIs = 3;
        public int NumHumans = 1;
        public int NeighborhoodCountX = 3;
        public int NeighborhoodCountY = 3;
        public string PlayerName = "Human";
    }

    public delegate void CustomerChangedEvent(Customer customer, Company company);

    [Serializable]
    public class Game {
        [NonSerialized]
        public Company Player;

        [NonSerialized]
        public MapConfiguration Map;

        [field: NonSerialized]
        public event CustomerChangedEvent CustomerChanged;

        public NewGameSettings Settings;
        public List<Company> Companies { get; set; }
        public List<Customer> Customers { get; set; }

        [NonSerialized]
        public System.Random Random;

        [OnDeserialized()]
        internal void OnSerializedMethod(StreamingContext context) {
            foreach(Company c in Companies) {
                c.Game = this;
            }
            foreach(Customer c in Customers) {
                c.Game = this;
            }

            this.Random = new System.Random(Settings.Seed);
        }

        public void NewGame(NewGameSettings settings, NameGenerator nameGenerator, MapConfiguration map) {
            this.Settings = settings;
            this.Map = map;
            this.Random = new System.Random(Settings.Seed);

            Companies = new List<Company>();
            for (int i = 0; i < settings.NumAIs; i++) {
                Companies.Add(GenerateCompany(CompanyOwnerType.AI, nameGenerator));
            }

            if (settings.NumHumans > 0) {
                Player = GenerateCompany(CompanyOwnerType.Human, nameGenerator);
                Player.Name = settings.PlayerName;
                Companies.Add(Player);
            }
        }

        public void PopulateCustomers(NameGenerator nameGenerator) {
            Customers = new List<Customer>();
            foreach (Neighborhood neighborhood in Map.Neighborhoods) {
                foreach(Lot lot in neighborhood.Lots) {
                    if(lot.Building != null) {
                        switch(lot.Building.Type) {
                            case BuildingType.SmallHouse:
                            case BuildingType.Ranch:
                            case BuildingType.SuburbanHouse:
                            case BuildingType.Townhouse:
                                // Create single customer account
                                Customers.Add(GenerateCustomer(lot, nameGenerator));
                                break;

                            case BuildingType.Apartment:
                                // Create customer accounts for all tenants 
                                break;
                        }
                    }
                }
            }
        }

        public Company GetCompany(string id) {
            return Companies.Find(c => c.ID == id);
        }

        public Customer FindCustomerByAddress(TilePosition address) {
            return Customers.Where(c => c.HomeLocation.Equals(address)).FirstOrDefault();
        }

        public void OnCustomerChanged(Customer customer) {
            Company company = GetCompany(customer.ProviderID);
            Debug.Log("Customer " + customer.Name + " changed to " +  ((company == null) ? "[none]" : company.Name));
            if (CustomerChanged != null)
                CustomerChanged(customer, company);
        }

        private Company GenerateCompany(CompanyOwnerType type, NameGenerator nameGenerator) {
            Company c = new Company {
                ID = Guid.NewGuid().ToString(),
                Name = nameGenerator.CompanyName(),
                OwnerType = type,
                Game = this
            };
            return c;
        }

        private Customer GenerateCustomer(Lot home, NameGenerator nameGenerator) {
            Customer customer = new Customer {
                HomeLocation = home.Anchor,
                ID = Guid.NewGuid().ToString(),
                Name = nameGenerator.RandomSurname(),
                Game = this,
                IncomeLevel = RandomUtils.RandomFloat(1.5f, 2.5f, this.Random),
                Patience = RandomUtils.RandomFloat(0.2f, 1.0f, this.Random)
            };
            return customer;
        }
    }

    public enum CompanyOwnerType {
        AI,
        Human
    }




    public enum EmployeeRole {
        CallCenter,
        FieldTech
    }

    public enum EmployeeGender {
        Male,
        Female
    }

    [Serializable]
    public class Employee {
        public string ID { get; set; }
        public string Name { get; set; }
        public EmployeeRole Role { get; set; }
        public EmployeeGender Gender { get; set; }
        public int Age { get; set; }
        public float Salary { get; set; }
        public float Competency { get; set; }
    }
}

