using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using DCTC.Map;

namespace DCTC.Model {

    public class NewGameSettings {
        public int NumAIs = 3;
        public int NumHumans = 1;
    }

    public class Game {
        [YamlIgnore]
        public Company Player;

        public List<Company> Companies { get; set; }

        public void NewGame(NewGameSettings settings) {
            Companies = new List<Company>();
            for (int i = 0; i < settings.NumAIs; i++) {
                Companies.Add(NewCompany(CompanyOwnerType.AI));
            }

            if (settings.NumHumans > 0) {
                Player = NewCompany(CompanyOwnerType.Human);
                Companies.Add(Player);
            }
        }

        Company NewCompany(CompanyOwnerType type) {
            Company c = new Company();
            c.ID = Guid.NewGuid().ToString();
            c.OwnerType = type;
            return c;
        }
    }

    public enum CompanyOwnerType {
        AI,
        Human
    }

    public delegate void ItemEventDelegate(object item);

    public class Company {
        public event ItemEventDelegate ItemAdded;
        public event ItemEventDelegate ItemRemoved;

        public string ID { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }

        public HashSet<Cable> Cables { get; set; }
        public Dictionary<TilePosition, Node> Nodes { get; set; }
        public HashSet<ServiceTier> ServicesOffered { get; set; }
        public Dictionary<ServiceTier, float> ServicePrices { get; set; }
        public HashSet<Employee> Employees { get; set; }
        public CompanyOwnerType OwnerType { get; set; }

        public Company() {
            Cables = new HashSet<Cable>();
            Nodes = new Dictionary<TilePosition, Node>();
            ServicesOffered = new HashSet<ServiceTier>();
            ServicePrices = new Dictionary<ServiceTier, float>();
            Employees = new HashSet<Employee>();
        }

        public Cable PlaceCable(CableType type, List<TilePosition> positions) {
            Cable cable = new Cable();
            cable.Type = type;
            cable.Positions.AddRange(positions);
            Cables.Add(cable);

            if (ItemAdded != null) ItemAdded(cable);
            return cable;
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
                        TriggerItemRemoved(cable);
                    }
                    else if(cable.Positions[0].Equals(pos) || cable.Positions[cable.Positions.Count - 1].Equals(pos)) {
                        cable.Positions.Remove(pos);

                        // Trigger removed and added events to allow visuals to be updated
                        TriggerItemRemoved(cable);
                        TriggerItemAdded(cable);
                    }
                    else {
                        Cables.Remove(cable);
                        TriggerItemRemoved(cable);

                        int index = cable.Positions.IndexOf(pos);
                        Cable c1 = new Cable();
                        c1.Type = cable.Type;
                        c1.Positions = cable.Positions.GetRange(0, index);
                        Cables.Add(c1);
                        TriggerItemAdded(c1);

                        Cable c2 = new Cable();
                        c2.Type = cable.Type;
                        c2.Positions = cable.Positions.GetRange(index + 1, cable.Positions.Count - index - 1);
                        Cables.Add(c2);
                        TriggerItemAdded(c2);
                    }
                }
            }
        }

        private void TriggerItemAdded(object item) {
            if (ItemAdded != null)
                ItemAdded(item);
        }
        private void TriggerItemRemoved(object item) {
            if (ItemRemoved != null)
                ItemRemoved(item);
        }

        public void PlaceNode(NodeType type, TilePosition position) {
            Node node = new Node();
            node.Type = type;
            node.Position = position;
            Nodes.Add(position, node);
            TriggerItemAdded(node);
        }

        public void RemoveNode(TilePosition pos) {
            if (Nodes.ContainsKey(pos)) {
                Node node = Nodes[pos];
                TriggerItemRemoved(node);
                Nodes.Remove(pos);
            }
            
        }
    }

    public class Cable {
        public string ID { get; set; }
        public CableType Type { get; set; }
        public List<TilePosition> Positions { get; set; }
        public Cable() {
            ID = Guid.NewGuid().ToString();
            Positions = new List<TilePosition>();
        }
    }

    public enum CableType {
        Copper,
        Fiber
    }

    public class Node {
        public string ID { get; set; }
        public NodeType Type { get; set; }
        public TilePosition Position { get; set; }

        public Node() {
            ID = Guid.NewGuid().ToString();
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

    public class Customer {
        public string ID { get; set; }
        public string Name { get; set; }
        public TilePosition HomeLocation { get; set; }
        // Some representation of desirability

        // ID of Company that provides service to this customer
        public string ProviderID { get; set; }

        public ServiceTier ServiceTier { get; set; }
    }


    public enum EmployeeRole {
        CallCenter,
        FieldTech
    }

    public enum EmployeeGender {
        Male,
        Female
    }

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

