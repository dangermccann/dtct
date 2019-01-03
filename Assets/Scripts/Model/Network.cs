using System;
using System.Linq;
using System.Collections.Generic;
using DCTC.Map;

namespace DCTC.Model {

    public delegate void ItemEventDelegate(object item);
    public delegate void ChangeDelegate();

    [Serializable]
    public class Network {
        public List<Cable> Cables;
        public List<Node> Nodes;

        [NonSerialized]
        public HashSet<TilePosition> ServiceArea;

        public Network() {
            Cables = new List<Cable>();
            Nodes = new List<Node>();
            ServiceArea = new HashSet<TilePosition>();
        }

        public bool AttachedToConnectors(HashSet<TilePosition> connectors) {
            return Positions.Intersect(connectors).Count() > 0;
        }

        public bool ContainsPosition(TilePosition position) {
            return Positions.Contains(position);
        }

        public HashSet<TilePosition> Positions {
            get {
                HashSet<TilePosition> positions = new HashSet<TilePosition>();
                foreach (Cable cable in Cables) {
                    positions.AddManySafely(cable.Positions);
                }
                return positions;
            }
        }

        public bool Active {
            get {
                return (Nodes.Count > 0 && Nodes[0].Status == NetworkStatus.Active);
            }
        }

        public HashSet<ServiceTier> AvailableServices {
            get {
                if (Nodes.Count == 0)
                    return new HashSet<ServiceTier>();

                if(Nodes[0].Type == NodeType.Fiber) {
                    return new HashSet<ServiceTier>() {
                        ServiceTier.FiberInternet, ServiceTier.FiberTV, ServiceTier.FiberDoublePlay
                    };
                }
                else {
                    NodeType lowestType = NodeType.Large;
                    foreach(Node node in Nodes) {
                        if (node.Type == NodeType.Small)
                            lowestType = NodeType.Small;
                    }

                    if(lowestType == NodeType.Small) {
                        return new HashSet<ServiceTier>() {
                            ServiceTier.BasicInternet, ServiceTier.BasicTV, ServiceTier.BasicDoublePlay
                        };
                    }
                    else {
                        return new HashSet<ServiceTier>() {
                            ServiceTier.BasicInternet, ServiceTier.BasicTV, ServiceTier.BasicDoublePlay,
                            ServiceTier.PremiumInternet, ServiceTier.PremiumTV, ServiceTier.PremiumDoublePlay
                        };
                    }
                }
            }
        }

        public bool IntersectsOneOf(HashSet<TilePosition> other) {
            foreach(TilePosition position in Positions) {
                if (other.Contains(position))
                    return true;
            }
            return false;
        }

    }

    public enum NetworkStatus {
        Active,
        Disconnected,
        Overloaded
    }

    [Serializable]
    public class Cable {
        public string ID { get; set; }
        public CableType Type { get; set; }
        public List<TilePosition> Positions { get; set; }

        private NetworkStatus status;
        public NetworkStatus Status {
            get { return status; }
            set {
                status = value;
                if (StatusChanged != null)
                    StatusChanged();
            }
       }

        [field: NonSerialized]
        public event ChangeDelegate StatusChanged;

        public Cable() {
            ID = Guid.NewGuid().ToString();
            Positions = new List<TilePosition>();
            Status = NetworkStatus.Disconnected;
        }

        public bool Intersects(IEnumerable<TilePosition> positions) {
            foreach (TilePosition position in positions) {
                if (Positions.Contains(position))
                    return true;
            }
            return false;
        }

        public int ServiceRange {
            get {
                switch (Type) {
                    case CableType.Copper:
                        return 4;
                    case CableType.Fiber:
                        return 4;
                }
                return 0;
            }
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

        private NetworkStatus status;
        public NetworkStatus Status {
            get { return status; }
            set {
                status = value;
                if (StatusChanged != null)
                    StatusChanged();
            }
        }

        [field: NonSerialized]
        public event ChangeDelegate StatusChanged;

        public Node() {
            ID = Guid.NewGuid().ToString();
            Status = NetworkStatus.Disconnected;
        }

        public int ServiceRange {
            get {
                switch (Type) {
                    case NodeType.Small:
                        return 4;
                    case NodeType.Large:
                        return 6;
                    case NodeType.Fiber:
                        return 6;
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
        BasicDoublePlay,
        PremiumTV,
        PremiumInternet,
        PremiumDoublePlay,
        FiberTV,
        FiberInternet,
        FiberDoublePlay
    }
}