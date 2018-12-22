using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DCTC.Map;

namespace DCTC.Model {

    public class NewGameSettings {
        public int NumAIs = 3;
        public int NumHumans = 1;
        public int NeighborhoodCountX = 3;
        public int NeighborhoodCountY = 3;
    }

    [Serializable]
    public class Game {
        [NonSerialized]
        public Company Player;

        [NonSerialized]
        public MapConfiguration Map;

        public List<Company> Companies { get; set; }
        public List<Customer> Customers { get; set; }

        [OnDeserialized()]
        internal void OnSerializedMethod(StreamingContext context) {
            foreach(Company c in Companies) {
                c.Game = this;
            }
        }

        public void NewGame(NewGameSettings settings, NameGenerator nameGenerator, MapConfiguration map) {
            this.Map = map;

            Companies = new List<Company>();
            for (int i = 0; i < settings.NumAIs; i++) {
                Companies.Add(GenerateCompany(CompanyOwnerType.AI, nameGenerator));
            }

            if (settings.NumHumans > 0) {
                Player = GenerateCompany(CompanyOwnerType.Human, nameGenerator);
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

        private Company GenerateCompany(CompanyOwnerType type, NameGenerator nameGenerator) {
            Company c = new Company();
            c.ID = Guid.NewGuid().ToString();
            c.Name = nameGenerator.CompanyName();
            c.OwnerType = type;
            c.Game = this;
            return c;
        }

        private Customer GenerateCustomer(Lot home, NameGenerator nameGenerator) {
            Customer customer = new Customer();
            customer.HomeLocation = home.Anchor;
            customer.ID = Guid.NewGuid().ToString();
            customer.Name = nameGenerator.RandomSurname();
            return customer;
        }
    }

    public enum CompanyOwnerType {
        AI,
        Human
    }

    public delegate void ItemEventDelegate(object item);

    [Serializable]
    public class Company {
        [field: NonSerialized]
        public event ItemEventDelegate ItemAdded;
        [field: NonSerialized]
        public event ItemEventDelegate ItemRemoved;

        [NonSerialized]
        public Game Game;

        public string ID { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }

        public List<Cable> Cables { get; set; }
        public Dictionary<TilePosition, Node> Nodes { get; set; }
        public List<ServiceTier> ServicesOffered { get; set; }
        public Dictionary<ServiceTier, float> ServicePrices { get; set; }
        public List<Employee> Employees { get; set; }
        public CompanyOwnerType OwnerType { get; set; }


        [NonSerialized]
        private List<Network> networks = null;
        public List<Network> Networks {
            get {
                if (networks == null)
                    networks = CalculateNetworks();
                return networks;
            }
        }

        [NonSerialized]
        private HashSet<TilePosition> serviceArea = null;
        public HashSet<TilePosition> ServiceArea {
            get {
                if (serviceArea == null)
                    serviceArea = CalculateServiceArea();
                return serviceArea;
            }
        }
        

        public Company() {
            Cables = new List<Cable>();
            Nodes = new Dictionary<TilePosition, Node>();
            ServicesOffered = new List<ServiceTier>();
            ServicePrices = new Dictionary<ServiceTier, float>();
            Employees = new List<Employee>();
        }

        public Cable PlaceCable(CableType type, List<TilePosition> positions) {
            Cable cable = new Cable();
            cable.Type = type;
            cable.Positions.AddRange(positions);
            Cables.Add(cable);
            InvalidateNetworks();
            TriggerItemAdded(cable);
            return cable;
        }

        public void PrependCable(Cable cable, TilePosition position) {
            cable.Positions.Insert(0, position);
            InvalidateNetworks();
            TriggerItemRemoved(cable);
            TriggerItemAdded(cable);
        }

        public void AppendCable(Cable cable, TilePosition position) {
            cable.Positions.Add(position);
            InvalidateNetworks();
            TriggerItemRemoved(cable);
            TriggerItemAdded(cable);
        }

        public void RemoveCablePosition(TilePosition pos) {
            List<Cable> allCables = new List<Cable>();
            allCables.AddRange(Cables);
            foreach(Cable cable in allCables) {
                if(cable.Positions.Contains(pos)) {
                    /* This cable contains the position being removed.  A few cases:
                     * 1. This is the only position in the cable (just delete it)
                     * 2. The position is at the start or end (just adjust the position)
                     * 3. The position is in the middle (delete it and recreate two new ones)
                     */

                    if(cable.Positions.Count == 1) {
                        Cables.Remove(cable);
                        InvalidateNetworks();
                        TriggerItemRemoved(cable);
                    }
                    else if(cable.Positions[0].Equals(pos) || cable.Positions[cable.Positions.Count - 1].Equals(pos)) {
                        cable.Positions.Remove(pos);
                        InvalidateNetworks();

                        // Trigger removed and added events to allow visuals to be updated
                        TriggerItemRemoved(cable);
                        TriggerItemAdded(cable);
                    }
                    else {
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
        }

        public void PlaceNode(NodeType type, TilePosition position) {
            Node node = new Node();
            node.Type = type;
            node.Position = position;
            Nodes.Add(position, node);
            InvalidateNetworks();
            TriggerItemAdded(node);
        }

        public void RemoveNode(TilePosition pos) {
            if (Nodes.ContainsKey(pos)) {
                Node node = Nodes[pos];
                Nodes.Remove(pos);
                InvalidateNetworks();
                TriggerItemRemoved(node);
            }
        }

        public HashSet<TilePosition> CalculateServiceArea() {
            HashSet<TilePosition> serviceArea = new HashSet<TilePosition>();
            foreach(Network network in Networks) {
                foreach(Node node in network.Nodes) {
                    // TODO: does the area change for different network types?
                    // TODO: determine the range based on the node type
                    IEnumerable<TilePosition> positions = Game.Map.Area(node.Position, 4);
                    serviceArea.AddManySafely(positions);
                }

                foreach(Cable cable in network.Cables) {
                    foreach(TilePosition pos in cable.Positions) {
                        // TODO: does the area change for different network types?
                        IEnumerable<TilePosition> positions = Game.Map.Area(pos, 4);
                        serviceArea.AddManySafely(positions);
                    }
                }
            }
            return serviceArea;
        }

        public List<Network> CalculateNetworks() {
            List<Network> networks = new List<Network>();

            HashSet<Node> usedNodes = new HashSet<Node>();

            foreach(Node node in Nodes.Values) {
                if(!usedNodes.Contains(node)) {
                    usedNodes.Add(node);

                    Network network = new Network();
                    networks.Add(network);
                    InsertNetworkNode(network, node);

                    // Find all Nodes that intersect this network
                    // Do this iteratively until no additional nodes have been found to add
                    bool found;
                    do {
                        found = false;
                        foreach(Node otherNode in Nodes.Values) {
                            // TODO: check for node type compatibility 
                            if(!network.Nodes.Contains(otherNode) &&
                                network.ContainsPosition(otherNode.Position)) {
                                InsertNetworkNode(network, otherNode);
                                usedNodes.Add(otherNode);
                                found = true;
                            }
                        }
                    }
                    while (found);
                }
            }

            return networks;
        }


        private void InsertNetworkNode(Network network, Node node) {
            network.Nodes.Add(node);
            IEnumerable<Cable> cables = IntersectingCables(node.Position);
            foreach(Cable cable in cables) {
                // TODO: Check for cable type compatibility 
                if(!network.Cables.Contains(cable)) {
                    network.Cables.Add(cable);

                    bool found;
                    do {
                        found = false;
                        foreach(Cable otherCable in Cables) {
                            // TODO: Check for cable type compatibility 
                            if (!network.Cables.Contains(otherCable) &&
                                otherCable.Intersects(network.Positions)) {
                                network.Cables.Add(otherCable);
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
            foreach(Cable cable in Cables) {
                if(cable.Positions.Contains(position)) {
                    if(!cables.Contains(cable))
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
            networks = null;
            serviceArea = null;
        }
    }

    [Serializable]
    public class Network {
        public List<Cable> Cables;
        public List<Node> Nodes;

        public Network() {
            Cables = new List<Cable>();
            Nodes = new List<Node>();
        }

        public bool ContainsPosition(TilePosition position) {
            return Positions.Contains(position);
        }

        public HashSet<TilePosition> Positions {
            get {
                HashSet<TilePosition> positions = new HashSet<TilePosition>();
                foreach(Cable cable in Cables) {
                    positions.AddManySafely(cable.Positions);
                }
                return positions;
            }
        }

    }

    [Serializable]
    public class Cable {
        public string ID { get; set; }
        public CableType Type { get; set; }
        public List<TilePosition> Positions { get; set; }
        public Cable() {
            ID = Guid.NewGuid().ToString();
            Positions = new List<TilePosition>();
        }

        public bool Intersects(IEnumerable<TilePosition> positions) {
            foreach(TilePosition position in positions) {
                if (Positions.Contains(position))
                    return true;
            }
            return false;
        }
    }

    public enum CableType {
        Copper,
        Fiber
    }

    [Serializable]
    public class Node {
        public string ID { get; set; }
        public NodeType Type { get; set; }
        public TilePosition Position { get; set; }

        public Node() {
            ID = Guid.NewGuid().ToString();
        }

        public int ServiceArea {
            get {
                switch(Type) {
                    case NodeType.Small:
                        return 15;
                    case NodeType.Large:
                        return 30;
                    case NodeType.Fiber:
                        return 30;
                }
                return 0;
            }
        }
    }

    public enum NodeType {
        Small,
        Large,
        Fiber
    }

    public enum ServiceTier {
        BasicTV,
        BasicInternet,
        PremiumTV,
        PremiumInternet,
        BasicDoublePlay,
        PremiumDoublePlay
    }

    [Serializable]
    public class Customer {
        public string ID { get; set; }
        public string Name { get; set; }
        public TilePosition HomeLocation { get; set; }
        // Some representation of desirability

        // ID of Company that provides service to this customer
        public string ProviderID { get; set; }

        public ServiceTier ServiceTier { get; set; }

        public void Update() {

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

