using System;
using System.Collections.Generic;
using System.Linq;
using DCTC.Map;
using UnityEngine;


namespace DCTC.Model {
    [Serializable]
    public class Company {
        [field: NonSerialized]
        public event ItemEventDelegate ItemAdded;
        [field: NonSerialized]
        public event ItemEventDelegate ItemRemoved;
        [field: NonSerialized]
        public event ChangeDelegate ServiceAreaChanged;

        [NonSerialized]
        private Game game;

        [NonSerialized]
        private HashSet<Cable> OrphanedCables;

        public string ID { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public CompanyAttributes Attributes { get; set; }

        public List<Cable> Cables { get; set; }
        public List<Node> Nodes { get; set; }
        public Dictionary<Services, float> ServicePrices { get; set; }
        public List<Truck> Trucks { get; set; }
        public List<Truck> UnhiredTrucks { get; set; }
        public List<string> TruckRollQueue { get; set; }
        public CompanyOwnerType OwnerType { get; set; }
        public float Money { get; set; }
        public TilePosition HeadquartersLocation { get; set; }
        public CallCenter CallCenter { get; set; }
        public Inventory Inventory { get; set; }
        public List<Rack> Racks { get; set; }

        public int RackLimit = 3;
        public int InventoryLimit = 750;

        public Building Headquarters {
            get {
                return game.Map.Tiles[HeadquartersLocation].Building;
            }
        }

        private SerializableColor color;
        public Color Color {
            get { return color.GetColor(); }
            set { color = new SerializableColor(value); }
        }

        [NonSerialized]
        HashSet<Customer> customers = new HashSet<Customer>();
        public HashSet<Customer> Customers {
            get {
                return customers;
            }
        }

        [NonSerialized]
        private List<Network> networks = null;
        public List<Network> Networks {
            get {
                if (networks == null)
                    CalculateNetworks();
                return networks;
            }
        }

        [NonSerialized]
        private HashSet<TilePosition> serviceArea = null;
        public HashSet<TilePosition> ServiceArea {
            get {
                if (serviceArea == null)
                    CalculateServiceArea();
                return serviceArea;
            }
        }

        public HashSet<TilePosition> HeadquartersConnectors {
            get {
                if (HeadquartersLocation.Equals(TilePosition.Origin))
                    return new HashSet<TilePosition>();

                return Game.Map.Tiles[HeadquartersLocation].Lot.Corners();
            }
        }

        public Game Game {
            get { return game; }
            set {
                game = value;
                game.CustomerChanged += OnCustomerChanged;
            }
        }

        public IEnumerable<Customer> ActiveCustomers {
            get {
                return Customers.Where(c => c.Status != CustomerStatus.Pending);
            }
        }

        public float Satisfaction {
            get {
                List<Customer> actives = new List<Customer>(ActiveCustomers);
                if (actives.Count == 0)
                    return 0;

                float val = 0f;
                foreach(Customer c in actives) {
                    val += c.Dissatisfaction;
                }
                val /= actives.Count;
                return (1 - val);
            }
        }

        public IEnumerable<TilePosition> CustomerHouses {
            get {
                return Customers.Select(c => c.HomeLocation);
            }
        }

        public Company() {
            Cables = new List<Cable>();
            Nodes = new List<Node>();
            ServicePrices = new Dictionary<Services, float>();
            TruckRollQueue = new List<string>();
            CallCenter = new CallCenter();
            Attributes = new CompanyAttributes();
            HeadquartersLocation = TilePosition.Origin;
            Inventory = new Inventory();
            Racks = new List<Rack>();

            InitPrices();
        }

        public int InventoryConsumed() {
            int total = 0;
            foreach (string id in Game.Items.CPE.Keys) {
                total += Inventory[id];
            }
            return total;
        }

        public int CanPurchase(Item item, int qty) {
            return CanPurchaseInternal(item, qty, -1);
        }

        public int CanPurchase(RackedItem item, int qty, int rackIdx) {
            return CanPurchaseInternal(item, qty, rackIdx);
        }

        private int CanPurchaseInternal(Item item, int qty, int rackIdx) {
            if (item.Cost * qty > Money)
                return Errors.INSUFFICIENT_MONEY;

            if (item is RackedItem) {
                Rack rack = Racks[rackIdx];
                RackedItem racked = item as RackedItem;
                if (rack.AvailableSlots(Game.Items) < racked.RackSpace * qty) {
                    return Errors.INSUFFICIENT_RACKSPACE;
                }
            }
            else if (item is Rack) {
                if (Racks.Count >= RackLimit)
                    return Errors.MAXIMUM_RACKS;
            }
            else if(item is CPE) {
                int consumed = InventoryConsumed();
                consumed += qty;
                if (consumed > InventoryLimit)
                    return Errors.INSUFFICIENT_INVENTORY;
            }

             return Errors.OK;
        }

        public void Purchase(Item item, int qty) {
            PurchaseInternal(item, qty, -1);
        }
        public void Purchase(RackedItem item, int qty, int rackIdx) {
            PurchaseInternal(item, qty, rackIdx);
        }

        private void PurchaseInternal(Item item, int qty, int rackIdx) {
            int err = CanPurchaseInternal(item, qty, rackIdx);
            if (!Errors.Success(err)) {
                throw new Exception(String.Format("Purchase error: {0:X}", err));
            }

            if (item.ID == "HR-15") {
                AppendRack();
            }
            else if(item is RackedItem) {
                Rack rack = Racks[rackIdx];
                RackedItem racked = item as RackedItem;

                for (int i = 0; i < qty; i++) {
                    rack.Contents.Add(racked.ID);
                }
            }

            Inventory[item.ID] += qty;
            Money -= item.Cost * qty;

            Debug.Log("Bought " + qty.ToString() + " " + item.ID);

            InvalidateNetworks();
        }

        public void AppendRack() {
            Rack rack = new Rack(Game.Items["HR-15"] as Rack);
            Racks.Add(rack);
        }

        public void RefreshCustomers() {
            customers = new HashSet<Customer>();
            foreach (Customer customer in game.Customers) {
                if (customer.ProviderID == ID)
                    customers.Add(customer);
            }
        }

        public void RollTruck(Customer customer) {
            TruckRollQueue.Insert(TruckRollQueue.Count, customer.ID);
        }

        public Customer GetCustomer(string id) {
            return Customers.FirstOrDefault(c => c.ID == id);
        }

        public Cable PlaceCable(CableType type, List<TilePosition> positions) {

            string posStr = positions.Aggregate("", (current, next) => current + " " + next);
            Debug.Log("Place: " + posStr);

            Cable cable = new Cable();
            cable.Type = type;
            cable.Positions.AddRange(positions);
            Cables.Add(cable);
            Money -= cable.Cost * positions.Count * Attributes.InfrastructureCost;
            InvalidateNetworks();
            TriggerItemAdded(cable);
            return cable;
        }

        public void PrependCable(Cable cable, TilePosition position) {
            cable.Positions.Insert(0, position);
            Money -= cable.Cost * Attributes.InfrastructureCost;
            InvalidateNetworks();
            TriggerItemRemoved(cable);
            TriggerItemAdded(cable);
        }

        public void AppendCable(Cable cable, TilePosition position) {
            cable.Positions.Add(position);
            Money -= cable.Cost * Attributes.InfrastructureCost;
            InvalidateNetworks();
            TriggerItemRemoved(cable);
            TriggerItemAdded(cable);
        }

        public void RemoveCable(Cable cable) {
            string posStr = cable.Positions.Aggregate("", (current, next) => current + " " + next);
            Debug.Log("Remove: " + posStr);

            Cables.Remove(cable);
            InvalidateNetworks();
            TriggerItemRemoved(cable);
        }


        public void RemoveCablePosition(TilePosition pos) {
            List<Cable> allCables = new List<Cable>();
            allCables.AddRange(Cables);
            foreach (Cable cable in allCables) {
                if (cable.Positions.Contains(pos)) {
                    /* This cable contains the position being removed.  A few cases:
                     * 1. This is the only position in the cable (just delete it)
                     * 2. The position is at the start or end (just adjust the position)
                     * 3. The position is in the middle (delete it and recreate two new ones)
                     */

                    if (cable.Positions.Count == 1) {
                        Cables.Remove(cable);
                        InvalidateNetworks();
                        TriggerItemRemoved(cable);
                    } else if (cable.Positions[0].Equals(pos) || cable.Positions[cable.Positions.Count - 1].Equals(pos)) {
                        cable.Positions.Remove(pos);
                        InvalidateNetworks();

                        // Trigger removed and added events to allow visuals to be updated
                        TriggerItemRemoved(cable);
                        TriggerItemAdded(cable);
                    } else {
                        Cables.Remove(cable);
                        InvalidateNetworks();
                        TriggerItemRemoved(cable);

                        int index = cable.Positions.IndexOf(pos);
                        Cable c1 = new Cable();
                        c1.Type = cable.Type;
                        c1.Positions = cable.Positions.GetRange(0, index);
                        Cables.Add(c1);
                        InvalidateNetworks();
                        TriggerItemAdded(c1);

                        Cable c2 = new Cable();
                        c2.Type = cable.Type;
                        c2.Positions = cable.Positions.GetRange(index + 1, cable.Positions.Count - index - 1);
                        Cables.Add(c2);
                        InvalidateNetworks();
                        TriggerItemAdded(c2);
                    }
                }
            }

            // Clean up any cables of length 1
            allCables = new List<Cable>();
            allCables.AddRange(Cables);
            foreach (Cable cable in allCables) {
                if(cable.Positions.Count <= 1) {
                    Cables.Remove(cable);
                    InvalidateNetworks();
                    TriggerItemRemoved(cable);
                }
            }
        }

        public void PlaceNode(CableType type, TilePosition position) {
            Node node = new Node();
            node.Type = type;
            node.Position = position;
            Nodes.Add(node);
            Money -= node.Cost * Attributes.InfrastructureCost;
            InvalidateNetworks();
            TriggerItemAdded(node);
        }

        public void RemoveNode(TilePosition pos) {
            for(int i = Nodes.Count - 1; i >= 0; i--) {
                Node node = Nodes[i];
                if (node.Position.Equals(pos)) {
                    RemoveNode(node);
                }
            }
        }

        public void RemoveNode(Node node) {
            Nodes.Remove(node);
            InvalidateNetworks();
            TriggerItemRemoved(node);
        }

        public void HireTruck(Truck truck) {
            UnhiredTrucks.Remove(truck);
            Trucks.Insert(0, truck);
            Money -= Truck.BaseCost;
            TriggerItemAdded(truck);
        }

        public void FireTruck(Truck truck) {
            UnhiredTrucks.Insert(0, truck);
            Trucks.Remove(truck);

            if (truck.Status != TruckStatus.Idle) {
                TruckRollQueue.Insert(0, truck.DestinationCustomerID);
            }

            TriggerItemRemoved(truck);
        }

        public void HireAgent(Agent agent) {
            CallCenter.HireAgent(agent);
            Money -= Agent.BaseCost;
        }

        public void FireAgent(Agent agent) {
            CallCenter.FireAgent(agent);
        }

        public List<CPE> InventoryCpe {
            get {
                List<CPE> cpe = new List<CPE>();

                foreach (string id in Inventory) {
                    Item item = Game.Items[id];
                    if (item is CPE) {
                        cpe.Add(item as CPE);
                    }
                }
                return cpe;
            }
        }

        public Dictionary<string, int> InventoryCpeByService(Services service) {
            Dictionary<string, int> inv = new Dictionary<string, int>();
            foreach(CPE cpe in InventoryCpe) {
                if(cpe.Services.Contains(service.ToString())) {
                    inv.Add(cpe.ID, Inventory[cpe.ID]);
                }
            }
            return inv;
        }

        public Dictionary<Services, float> FindServicesForLocation(TilePosition position) {
            HashSet<Services> services = new HashSet<Services>();
            foreach(Network network in Networks) {
                if(network.ServiceArea.Contains(position) && network.AvailableServices.Count > 0) {
                    services.AddManySafely(network.AvailableServices);
                }
            }

            Dictionary<Services, float> results = new Dictionary<Services, float>();
            foreach(Services service in services) {
                results.Add(service, ServicePrices[service]);
            }
            return results;
        }

        private void OnCustomerChanged(Customer customer, Company company) {
            if (TruckRollQueue.Contains(customer.ID))
                TruckRollQueue.Remove(customer.ID);

            if (company != null && company.ID == ID) {
                if (!customers.Contains(customer))
                    customers.Add(customer);

                if (customer.Status == CustomerStatus.Pending)
                    CallCenter.Enqueue(customer.ID);

                if (customer.Status == CustomerStatus.Outage)
                    CallCenter.Enqueue(customer.ID);


            } else if (customers.Contains(customer)) {
                customers.Remove(customer);
            }
        }


        private void CalculateServiceArea() {
            HashSet<TilePosition> serviceArea = new HashSet<TilePosition>();
            foreach (Network network in Networks) {
                network.ServiceArea.Clear();

                if (!network.Active)
                    continue;

                foreach (Cable cable in network.Cables) {
                    foreach (TilePosition pos in cable.Positions) {
                        IEnumerable<TilePosition> positions = Game.Map.Area(pos, cable.ServiceRange);
                        network.ServiceArea.AddManySafely(positions);
                        serviceArea.AddManySafely(positions);
                    }
                }
            }
            this.serviceArea = serviceArea;
        }

        

        private void CalculateNetworks() {
            List<Network> networks = new List<Network>();

            // Build network from each of the individual cables
            HashSet<Cable> usedCables = new HashSet<Cable>();
            foreach(Cable cable in Cables) {
                if (!usedCables.Contains(cable)) {
                    usedCables.Add(cable);

                    Network network = new Network();
                    network.CableType = cable.Type;
                    networks.Add(network);
                    network.Cables.Add(cable);

                    bool found;
                    do {
                        found = false;
                        foreach (Cable otherCable in Cables) {
                            if (!network.Cables.Contains(otherCable) &&
                                network.IntersectsOneOf(new HashSet<TilePosition>(otherCable.Positions)) &&
                                otherCable.Type == network.CableType) {
                                network.Cables.Add(otherCable);
                                usedCables.Add(otherCable);
                                found = true;
                            }
                        }
                    }
                    while (found);

                    foreach(Node node in Nodes) {
                        if(node.Type == network.CableType && network.ContainsPosition(node.Position)) {
                            network.Nodes.Add(node);
                        }
                    }
                }
            }

            this.networks = networks;

            UpdateNetworkStatus();
            UpdateNetworkServices();
        }

        void UpdateNetworkStatus() {
            // Update network state for all elements
            HashSet<TilePosition> connectors = HeadquartersConnectors;
            foreach (Network network in Networks) {
                bool intersects = false;
                if (network.IntersectsOneOf(connectors)) {
                    intersects = true;
                }

                foreach (Cable cable in network.Cables) {
                    cable.Status = intersects ? NetworkStatus.Active : NetworkStatus.Disconnected;
                }

                foreach (Node node in network.Nodes) {
                    node.Status = intersects ? NetworkStatus.Active : NetworkStatus.Disconnected;
                }
            }
        }

        void UpdateNetworkServices() {
            foreach (Network network in Networks) {
                network.AvailableServices.Clear();
                network.ServiceCapacity.Clear();
                network.BroadbandThroughput = 0;

                if (network.Active) {
                    // Find backhaul items and throughput
                    int backhaul = 0;
                    float backhaulThroughput = 0;
                    foreach (string itemID in Inventory) {
                        Item item = Game.Items[itemID];
                        if (item is Backhaul) {
                            backhaul += Inventory[itemID];
                            backhaulThroughput += (item as Backhaul).Throughput;
                        }
                    }

                    // Find termination items to determine which services are available
                    foreach (string itemID in Inventory) {
                        Item item = Game.Items[itemID];
                        if(item is Termination) {
                            Termination term = item as Termination;
                            CableType type = Utilities.ParseEnum<CableType>(term.Wiring);
                            if (type == network.CableType) {
                                if (term.Requires == null || (term.Requires == "Backhaul" && backhaul > 0)) {
                                    Services s = Utilities.ParseEnum<Services>(term.Service);
                                    if (!network.AvailableServices.Contains(s))
                                        network.AvailableServices.Add(s);

                                    if (!network.ServiceCapacity.ContainsKey(s)) {
                                        network.ServiceCapacity[s] = term.Subscribers;
                                    } else {
                                        network.ServiceCapacity[s] += term.Subscribers;
                                    }

                                    if (s == Services.Broadband) {
                                        network.BroadbandThroughput = Mathf.Max(network.BroadbandThroughput, term.Throughput);
                                    }
                                }
                            }
                        }
                    }

                    network.BroadbandThroughput = Mathf.Min(network.BroadbandThroughput, backhaulThroughput);
                }
            }
        }

        public void Update(float deltaTime) {
            CallCenter.Update(deltaTime);

            foreach (Truck truck in Trucks) {
                if (truck.Status == TruckStatus.Idle) {
                    if(TruckRollQueue.Count > 0) {
                        for(int i = 0; i < TruckRollQueue.Count; i++) {
                            string customerID = TruckRollQueue[i];
                            if (truck.Dispatch(customerID)) {
                                TruckRollQueue.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
                else {
                    truck.Update(deltaTime);
                }

                Money -= truck.Salary * deltaTime;
            }

            HashSet<Customer> customers = Customers;
            foreach (Customer customer in customers) {
                if(customer.Status == CustomerStatus.Subscribed) {
                    foreach(Services service in customer.Services)
                    Money += ServicePrices[service] * deltaTime;
                }
            }
        }

        private void InsertNetworkNode(Network network, Node node) {
            network.Nodes.Add(node);
            IEnumerable<Cable> cables = IntersectingCables(node.Position);
            foreach (Cable cable in cables) {
                if (!network.Cables.Contains(cable) && cable.Type == network.CableType) {
                    network.Cables.Add(cable);

                    if (OrphanedCables.Contains(cable))
                        OrphanedCables.Remove(cable);

                    bool found;
                    do {
                        found = false;
                        foreach (Cable otherCable in Cables) {
                            if (!network.Cables.Contains(otherCable) &&
                                otherCable.Intersects(network.Positions) &&
                                otherCable.Type == network.CableType) {
                                network.Cables.Add(otherCable);

                                if (OrphanedCables.Contains(otherCable))
                                    OrphanedCables.Remove(otherCable);

                                found = true;
                            }
                        }
                    }
                    while (found);
                }
            }
        }

        private IEnumerable<Cable> IntersectingCables(TilePosition position) {
            HashSet<Cable> cables = new HashSet<Cable>();
            foreach (Cable cable in Cables) {
                if (cable.Positions.Contains(position)) {
                    if (!cables.Contains(cable))
                        cables.Add(cable);


                }
            }
            return cables;
        }

        private void TriggerItemAdded(object item) {
            if (ItemAdded != null)
                ItemAdded(item);
        }
        private void TriggerItemRemoved(object item) {
            if (ItemRemoved != null)
                ItemRemoved(item);
        }

        private void InvalidateNetworks() {
            CalculateNetworks();
            serviceArea = null;

            if (ServiceAreaChanged != null)
                ServiceAreaChanged();
        }

        private void InitPrices() {
            ServicePrices[Services.Broadband] = 10.0f;
            ServicePrices[Services.TV] = 10.0f;
            ServicePrices[Services.Phone] = 3.0f;
        }
    }

    [Serializable]
    public class CompanyAttributes {
        public float CallCenterFriendliness = 1f;
        public float CallCenterEffectiveness = 1f;
        public float TruckTravelSpeed = 1f;
        public float TruckWorkSpeed = 1f;
        public float CustomerRetention = 1f;
        public float InfrastructureCost = 1f;
        public float ResearchSpeed = 1f;
        public float ServiceSatisfaction = 1f;
    }
}