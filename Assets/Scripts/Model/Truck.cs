using System;
using System.Collections.Generic;
using System.Linq;
using DCTC.Map;
using UnityEngine;

namespace DCTC.Model {

    public enum TruckStatus {
        Idle,
        EnRoute,
        GettingEquipment,
        Working
    }


    [Serializable]
    public class Truck {
        public const float BaseCost = 2f;
        private const float BaseTravelSpeed = 15;
        private const float BaseWorkSpeed = 1.5f;
        private const float BaseWork = 1;



        public string ID { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public int CurrentIndex { get; set; }
        public string DestinationCustomerID { get; set; }
        public TruckStatus Status { get; set; }
        public List<TilePosition> Path { get; set; }
        public float TravelSpeed { get; set; }
        public float WorkSpeed { get; set; }
        public Inventory Inventory { get; set; }
        public List<string> PendingEquipmentFromHQ { get; set; }
        public List<string> PendingInstallEquipment { get; set; }
        public float WorkRemaining { get; set; }
        public float Elapsed { get; set; }
        public Vector3 CurrentStart { get; set; }
        public Vector3 CurrentDestination { get; set; }
        public float Salary {
            get {
                return Mathf.Max(0.2f * TravelSpeed * WorkSpeed, 0.05f);
            }
        }

        public TilePosition TilePosition {
            get {
                return ThreeDMap.WorldToPosition(Position);
            }
        }

        [NonSerialized]
        public Game Game;

        [NonSerialized]
        public Company Company;

        [field: NonSerialized]
        public event ChangeDelegate Dispatched;

        public Truck() {
            TravelSpeed = 1;
            WorkSpeed = 1;
            Inventory = new Inventory();
        }

        // TODO: move this somewhere more global        
        public bool EquipmentForServices(IEnumerable<Services> services, IEnumerable<CPE> available, out List<CPE> result) {
            result = new List<CPE>();
            bool retVal = true;

            // Sort the available CPEs by how many services they provide so that
            // we choose the least expensive set of devices
            available = available.OrderBy(c => c.Services.Count);

            if (services.Count() > 1)
                available = available.Reverse();

            foreach (Services service in services) {
                bool found = false;

                foreach(CPE cpe in available) {
                    if(cpe.Services.Contains(service.ToString())) {
                        if(!result.Contains(cpe))
                            result.Add(cpe);
                        found = true;
                        break;
                    }
                }
                if (!found)
                    retVal = false;
            }
            return retVal;
        }

        public bool Dispatch(string customerID) {

            DestinationCustomerID = customerID;
            Customer customer = Game.GetCustomer(customerID);
            PendingEquipmentFromHQ = new List<string>();
            PendingInstallEquipment = new List<string>();

            Path = new List<TilePosition>();

            if (customer.Status == CustomerStatus.Pending) {
                // Customer is pending install
                // Determine what equipment is needed in inventory
                List<CPE> installEquipment = new List<CPE>();

                if(!EquipmentForServices(customer.Services, Game.Items.FromInventory<CPE>(Inventory), out installEquipment)) {
                    // Equipment not available in truck
                    // Determine what equipment we need from HQ
                    PendingInstallEquipment.AddRange(installEquipment.Select(c => c.ID));
                    Inventory combinedInventory = new Inventory();
                    combinedInventory.Add(Inventory);
                    combinedInventory.Add(Company.Inventory);

                    List<CPE> hqEquipment = new List<CPE>();
                    if(EquipmentForServices(customer.Services, Game.Items.FromInventory<CPE>(combinedInventory), out hqEquipment)) {
                        // Equipment availabe in HQ
                        hqEquipment.RemoveAll(cpe => installEquipment.Contains(cpe));

                        if(hqEquipment.Count == 0) {
                            Debug.LogError("Unexpected zero items to get from HQ");
                            return false;
                        }

                        // Go to HQ to get equipment 
                        PendingEquipmentFromHQ = new List<string>(hqEquipment.Select(c => c.ID));
                        PendingInstallEquipment.AddRange(PendingEquipmentFromHQ);
                        Path.AddRange(PathTo(Company.HeadquartersLocation));
                        Status = TruckStatus.GettingEquipment;

                    } else {
                        // Equipment unavailable at HQ
                        return false;
                    }
                }
                else {
                    // Sufficient inventory on truck.  Go to customer's home
                    PendingInstallEquipment.AddRange(installEquipment.Select(c => c.ID));
                    Path.AddRange(PathTo(customer.HomeLocation));
                    Status = TruckStatus.EnRoute;
                }
            }
            else {
                // Go directly to customer's home for repair
                Path.AddRange(PathTo(customer.HomeLocation));
                Status = TruckStatus.EnRoute;
            }

            CurrentIndex = 0;
            WorkRemaining = BaseWork;

            if (NextTile())
                DestinationReached();

            if (Dispatched != null)
                Dispatched();

            return true;
        }

        public void Update(float deltaTime) {
            if (Status == TruckStatus.Working) {
                WorkRemaining -= deltaTime * BaseWorkSpeed * WorkSpeed * Company.Attributes.TruckWorkSpeed;

                if (WorkRemaining <= 0)
                    JobComplete();
            }
            else if(Status == TruckStatus.EnRoute || Status == TruckStatus.GettingEquipment) {
                if ((Position - CurrentDestination).magnitude < 0.05f) {
                    if (NextTile())
                        DestinationReached();
                }

                Elapsed += deltaTime * BaseTravelSpeed * TravelSpeed * Company.Attributes.TruckTravelSpeed;
                Position = Vector3.Lerp(CurrentStart, CurrentDestination, Elapsed);
            }
        }


        private IList<TilePosition> PathTo(TilePosition position) {
            position = Game.Map.NearestRoad(position);

            IList<TilePosition> path = Game.Map.Pathfind(TilePosition, position);
            if (path.Count == 0) {
                Debug.LogError("Unable to pathfind to truck destination: " + position.ToString());
                return path;
            }

            path = new List<TilePosition>(path.Reverse());
            return path;
        }


        private bool NextTile() {
            CurrentIndex++;

            if (CurrentIndex >= Path.Count) {
                return true;
            }

            CurrentStart = Position;
            CurrentDestination = ThreeDMap.PositionToWorld(Path[CurrentIndex]);
            Elapsed = 0;

            return false;
        }


        private void DestinationReached() {
            Customer customer = Game.GetCustomer(DestinationCustomerID);

            if (Status == TruckStatus.GettingEquipment) {
                // Get equipment
                foreach(string id in PendingEquipmentFromHQ) {
                    int amount = Mathf.CeilToInt((float)Company.Inventory[id] / Company.Trucks.Count);
                    Company.Inventory.Subtract(id, amount);
                    Inventory.Add(id, amount);
                }

                // Go to customer's home
                Status = TruckStatus.EnRoute;

                Path = new List<TilePosition>();
                Path.AddRange(PathTo(customer.HomeLocation));
            }
            else if(Status == TruckStatus.EnRoute) {
                Status = TruckStatus.Working;

                foreach(string id in PendingInstallEquipment) {
                    Inventory.Subtract(id, 1);
                    customer.Equipment.Add(id, 1);
                }
            }
            
        }

        private void JobComplete() {
            Status = TruckStatus.Idle;

            Customer customer = Game.GetCustomer(DestinationCustomerID);
            customer.ServiceTruckArrived();
        }
    }
}