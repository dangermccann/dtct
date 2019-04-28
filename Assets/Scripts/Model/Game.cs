using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using DCTC.Map;
using DCTC.AI;

namespace DCTC.Model {

    [Serializable]
    public class NewGameSettings {
        public int Seed = 0;
        public int NumAIs = 3;
        public int NumHumans = 1;
        public int NeighborhoodCountX = 3;
        public int NeighborhoodCountY = 3;
        public string PlayerName = "Human";
        public int StartingMoney = 1000;
        public CompanyAttributes PlayerAttributes = new CompanyAttributes();
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

        public UnityEngine.Random.State RandomState;

        [NonSerialized]
        public System.Random Random;

        [NonSerialized]
        public NameGenerator NameGenerator;

        [NonSerialized]
        public Items Items;

        [NonSerialized]
        public Technology TechnologyGraph;

        [OnDeserialized()]
        internal void OnSerializedMethod(StreamingContext context) {
            foreach (Company c in Companies) {
                c.Game = this;
                c.RefreshCustomers();
                c.CallCenter.Company = c;
                foreach (Agent a in c.CallCenter.Agents) {
                    a.Company = c;
                }

                foreach (Agent a in c.CallCenter.UnhiredAgents) {
                    a.Company = c;
                }

                foreach (Truck t in c.Trucks) {
                    t.Game = this;
                    t.Company = c;
                }

                foreach (Truck t in c.UnhiredTrucks) {
                    t.Game = this;
                    t.Company = c;
                }
            }
            foreach(Customer c in Customers) {
                c.Game = this;
            }

            this.Random = new System.Random(Settings.Seed);
        }

        public void LoadConfig() {
            Items = Loader.LoadItems();
            TechnologyGraph = Technology.BuildGraph(Loader.LoadTechnologies());
        }

        public void NewGame(NewGameSettings settings, NameGenerator nameGenerator, MapConfiguration map,
            IList<TilePosition> headquarters) {
            this.Settings = settings;
            this.Map = map;
            this.Random = new System.Random(Settings.Seed);
            this.NameGenerator = nameGenerator;
            UnityEngine.Random.InitState(Settings.Seed);

            Companies = new List<Company>();
            for (int i = 0; i < settings.NumAIs; i++) {
                Company company = GenerateCompany(CompanyOwnerType.AI, headquarters[i]);
                // TODO: give opponants "personalities" and support difficulty levels
                company.Money = settings.StartingMoney;
                company.Attributes = new CompanyAttributes();
                company.Inventory[Rack.HR15] = 1;
                company.AppendRack();
                Companies.Add(company);
            }

            if (settings.NumHumans > 0) {
                Player = GenerateCompany(CompanyOwnerType.Human, headquarters[settings.NumAIs]);
                Player.Name = settings.PlayerName;
                Player.Attributes = settings.PlayerAttributes;
                Player.Inventory[Rack.HR15] = 1;
                Player.AppendRack();
                Player.Money = settings.StartingMoney;
                Companies.Add(Player);
            }
        }

        public void PopulateCustomers() {
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
                                Customers.Add(GenerateCustomer(lot));
                                break;

                            case BuildingType.Apartment:
                                // Create customer accounts for all tenants 
                                break;
                        }
                    }
                }
            }
        }

        public void PostLoad() {
            foreach (Company c in Companies) {
                if (c.AIExecutor != null) { // Will be null for Human player
                    foreach (IAgent agent in c.AIExecutor.Agents) {
                        agent.Company = c;
                    }
                }
            }
        }

        public Company GetCompany(string id) {
            return Companies.Find(c => c.ID == id);
        }

        public Customer GetCustomer(string id) {
            return Customers.Find(c => c.ID == id);
        }

        public Customer FindCustomerByAddress(TilePosition address) {
            return Customers.Where(c => c.HomeLocation.Equals(address)).FirstOrDefault();
        }

        public void OnCustomerChanged(Customer customer) {
            Company company = GetCompany(customer.ProviderID);
            //Debug.Log("Customer " + customer.Name + " changed to " +  ((company == null) ? "[none]" : company.Name));
            if (CustomerChanged != null)
                CustomerChanged(customer, company);
        }

        private Company GenerateCompany(CompanyOwnerType type, TilePosition headquartersLocation) {
            TilePosition truckPosition = Map.NearestRoad(headquartersLocation);
            Company company = new Company {
                ID = Guid.NewGuid().ToString(),
                Name = NameGenerator.CompanyName(),
                OwnerType = type,
                Game = this,
                Trucks = new List<Truck>(),
                UnhiredTrucks = new List<Truck>(),
                Color = UnityEngine.Random.ColorHSV(0, 1, 1, 1, 0.7f, 1, 1, 1),
                HeadquartersLocation = headquartersLocation
            };

            company.Trucks.Add(GenerateTruck(company, truckPosition));

            for(int i = 0; i < 10; i++) {
                company.UnhiredTrucks.Add(GenerateTruck(company, truckPosition));
            }
            company.CallCenter.Company = company;

            // Start with one agent
            Agent agent = GenerateAgent(company);
            company.CallCenter.Agents.Add(agent);

            for (int i = 0; i < 10; i++) {
                company.CallCenter.UnhiredAgents.Add(GenerateAgent(company));
            }

            if(type == CompanyOwnerType.AI) {
                // Add all AI agent classes 
                company.AIExecutor = new Executor(new List<IAgent>() {
                    new CablePlacementAgent(company),
                    new NodePlacementAgent(company),
                    new EquipmentAgent(company),
                    new CPEAgent(company),
                    new PersonnelAgent(company),
                    new TechnologyAgent(company)
                });
            }

            // Seed techs
            List<Technology> techs = Technology.Flatten(TechnologyGraph);
            foreach (Technology tech in techs) {
                company.Technologies.Add(tech.ID, 0);
            }

            return company;
        }

        public Truck GenerateTruck(Company company, TilePosition position) {
            string name = RandomUtils.Chance(Random, 0.5) ? NameGenerator.RandomMaleName() : NameGenerator.RandomFemaleName();

            return new Truck() {
                ID = Guid.NewGuid().ToString(),
                Name = name,
                Position = ThreeDMap.PositionToWorld(position),
                Status = TruckStatus.Idle,
                TravelSpeed = RandomUtils.RandomFloat(0.55f, 1.0f, this.Random),
                WorkSpeed = RandomUtils.RandomFloat(0.55f, 1.0f, this.Random),
                Company = company,
                Game = this
            };
        }

        private Customer GenerateCustomer(Lot home) {
            Customer customer = new Customer {
                HomeLocation = home.Anchor,
                ID = Guid.NewGuid().ToString(),
                Name = NameGenerator.RandomSurname(),
                Game = this,
                IncomeLevel = RandomUtils.RandomFloat(1.5f, 2.5f, this.Random),
                Patience = RandomUtils.RandomFloat(0.2f, 1.0f, this.Random)
            };
            return customer;
        }

        public Agent GenerateAgent(Company company) {
            string name = RandomUtils.Chance(Random, 0.5) ? NameGenerator.RandomMaleName() : NameGenerator.RandomFemaleName();

            Agent agent = new Agent() {
                ID = Guid.NewGuid().ToString(),
                Name = name,
                Speed = RandomUtils.RandomFloat(0.45f, 0.99f, Random),
                Friendliness = RandomUtils.RandomFloat(0.35f, 0.99f, Random),
                Performance = RandomUtils.RandomFloat(0.55f, 0.99f, Random),
                Company = company
            };
            return agent;
        }
    }

    public enum CompanyOwnerType {
        AI,
        Human
    }
}

