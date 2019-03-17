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

        public CableType CableType;

        [NonSerialized]
        public HashSet<Services> AvailableServices;

        [NonSerialized]
        public Dictionary<Services, int> ServiceCapacity;

        [NonSerialized]
        public float BroadbandThroughput = 0f;

        [NonSerialized]
        public HashSet<TilePosition> ServiceArea;

        public Network() {
            Cables = new List<Cable>();
            Nodes = new List<Node>();
            AvailableServices = new HashSet<Services>();
            ServiceArea = new HashSet<TilePosition>();
            ServiceCapacity = new Dictionary<Services, int>();
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
                return 2;
            }
        }

        public float Cost {
            get {
                switch(Type) {
                    case CableType.Copper:
                        return 1;
                    case CableType.Coaxial:
                        return 1;
                    case CableType.Optical:
                        return 2;
                }
                return 0;
            }
        }
    }

    public enum CableType {
        Copper,
        Coaxial,
        Optical
    }

    [Serializable]
    public class Node {
        public string ID { get; set; }
        public CableType Type { get; set; }
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

        public float Cost {
            get {
                switch (Type) {
                    case CableType.Copper:
                        return 4;
                    case CableType.Coaxial:
                        return 6;
                    case CableType.Optical:
                        return 8;
                }
                return 0;
            }
        }
    }

    public enum Services {
        Broadband,
        TV,
        Phone
    }
    
}