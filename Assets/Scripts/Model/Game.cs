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
                IncomeLevel = RandomUtils.RandomFloat(1.0f, 3.0f, this.Random),
                Patience = RandomUtils.RandomFloat(0.2f, 1.0f, this.Random)
            };
            return customer;
        }
    }

    public enum CompanyOwnerType {
        AI,
        Human
    }


    [Serializable]
    public class Customer {
        public string ID { get; set; }
        public string Name { get; set; }
        public TilePosition HomeLocation { get; set; }
        public float IncomeLevel { get; set; }
        public float Patience { get; set; }
        public float Frustration { get; set; }

        // ID of Company that provides service to this customer
        public string ProviderID { get; set; }

        public ServiceTier ServiceTier { get; set; }

        private int invocationsSinceProviderChange = 0;


        [NonSerialized]
        public Game Game;

        [NonSerialized]
        private float lastUpdateTime = 0f;

        public Building Home {
            get {
                return Game.Map.Tiles[HomeLocation].Building;
            }
        }

        public float Wealth {
            get { return IncomeLevel * Home.SquareMeters; }
        }

        public void Update(float time) {
            float deltaTime = time - lastUpdateTime;

            List<Company> candidates = FindServiceProviders();

            if(ProviderID == null) {
                if(candidates.Count > 0 && WillChangeProvider()) {
                    ChangeProvider(ChooseProvider(candidates));
                }
            }
            else {
                // Determine if company no longer provides service 
                Company provider = candidates.Find(c => c.ID == ProviderID);
                if (provider == null) {
                    ChangeProvider(null);
                }
                else {
                    // Determine if customer wishes to switch to a different provider or cancel service
                    candidates.Remove(provider);
                    if (WillChangeProvider()) {
                        ChangeProvider(ChooseProvider(candidates));
                    }
                }


            }

            lastUpdateTime = time;
            invocationsSinceProviderChange++;
        }

        public List<Company> FindServiceProviders() {
            List<Company> candidates = new List<Company>();
            foreach(Company company in Game.Companies) {
                if(company.ServiceArea.Contains(HomeLocation)) {
                    candidates.Add(company);
                }
            }
            return candidates;
        }

        private bool WillChangeProvider() {
            // Look for a company to add service
            float timeInerta = (ProviderID == null) ? 50 : 500;
            float wealthChance = RandomUtils.LinearLikelihood(100f, 1000f, Wealth);
            float timeChance = RandomUtils.LinearLikelihood(0, timeInerta, invocationsSinceProviderChange);
            float chance = Mathf.Clamp01(wealthChance * timeChance * Patience * (Frustration + 1));
            return RandomUtils.Chance(Game.Random, chance);
        }

        private Company ChooseProvider(List<Company> companies) {
            if (companies.Count == 0)
                return null;

            // TODO: use attributes of the companies to weigh them
            return RandomUtils.RandomThing(companies, Game.Random);
        }

        private void ChangeProvider(Company company) {
            ProviderID = (company == null) ? null : company.ID;
            Frustration = 0;
            invocationsSinceProviderChange = 0;

            Game.OnCustomerChanged(this);
        }
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

