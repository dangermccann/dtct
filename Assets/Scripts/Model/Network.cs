using System;
using System.Linq;
using System.Collections.Generic;
using DCTC.Map;
using DCTC.Pathfinding;

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

        public int MaximumCableDistanceFromNode() {
            if (Nodes.Count > 0)
                return Nodes[0].Attributes.Range;

            return 0;
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

        public bool IsDistanceWithin(TilePosition position, int within) {
            if (Nodes.Count == 0)
                return false;

            // Sort by closest geographic distance
            IEnumerable<Node> sorted = Nodes.OrderBy(n => TilePosition.Distance(n.Position, position));

            // If first (closest) node is too far away, they all will be
            Node first = sorted.First();
            if (TilePosition.Distance(first.Position, position) > within)
                return false;

            foreach (Node node in sorted) {
                // To minimize the possibility of pathfinding, check the absolute distance first
                // If this one is out of range, the rest should be as well, so we'll give up.
                if (TilePosition.Distance(first.Position, position) > within)
                    return false;

                // This perfoms the *expensive* pathfinding check
                int val = DistanceTo(position, node.Position);
                if (val <= within)
                    return true;
            }

            return false;
        }

        [NonSerialized]
        private Dictionary<TilePosition, IPathNode> pathNodes;

        void InitPathfinding() {
            pathNodes = new Dictionary<TilePosition, IPathNode>();

            foreach (Cable cable in Cables) {
                foreach (TilePosition pos in cable.Positions) {
                    if (!pathNodes.ContainsKey(pos)) {
                        pathNodes.Add(pos, new PathNode(pos));
                    }
                }
            }
        }

        public void InvalidatePathfinding() {
            pathNodes = null;
        }

        public IList<TilePosition> PathTo(TilePosition start, TilePosition end) {
            if (pathNodes == null)
                InitPathfinding();

            if (!pathNodes.ContainsKey(start) || !pathNodes.ContainsKey((end))) {
                // The start and end positions must be in the network
                return null;
            }

            AStar pathfinder = new AStar(new List<IPathNode>(pathNodes.Values));

            // Perform search
            IList<IPathNode> results = pathfinder.Search(pathNodes[start], pathNodes[end]);
            if(results == null) {
                UnityEngine.Debug.LogWarning("Pathfinding should work");
                return null;
            }

            // Convert results into usable list of TilePosition objects
            List<TilePosition> positions = new List<TilePosition>();

            foreach (IPathNode result in results) {
                positions.Add(result.Position);
            }

            return positions;
        }

        public int DistanceTo(TilePosition start, TilePosition end) {
            IList<TilePosition> path = PathTo(start, end);
            if (path == null)
                return int.MaxValue;
            else return path.Count;
        }


    }

    public enum NetworkStatus {
        Active,
        Disconnected,
        Overloaded
    }

    [Serializable]
    public class Cable {
        public const string CAT3 = "CAT-3";
        public const string RG6 = "RG-6";
        public const string OM2 = "OM-2";

        public string ID { get; set; }
        public CableType Type { get; set; }
        public List<TilePosition> Positions { get; set; }
        public CableAttributes Attributes { get; set; }
        public string Guid { get; set; }

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

        public Cable(string id, CableAttributes attributes) {
            ID = id;
            Guid = System.Guid.NewGuid().ToString();
            Type = Utilities.ParseEnum<CableType>(attributes.Wiring);
            Attributes = attributes;
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
            get { return Attributes.Cost; }
        }
    }

    public enum CableType {
        Copper,
        Coaxial,
        Optical
    }

    [Serializable]
    public class Node {
        public const string DR100 = "DR-100";
        public const string CR100 = "CR-100";
        public const string OR105 = "OR-105";

        public string ID { get; set; }
        public string Guid { get; set; }
        public CableType Type { get; set; }
        public TilePosition Position { get; set; }
        public NodeAttributes Attributes { get; set; }

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

        public Node(string id, NodeAttributes attributes) {
            ID = id;
            Attributes = attributes;
            Guid = System.Guid.NewGuid().ToString();
            Status = NetworkStatus.Disconnected;
            Type = Utilities.ParseEnum<CableType>(attributes.Wiring);
        }

        public float Cost {
            get { return Attributes.Cost; }
        }
    }

    public enum Services {
        Broadband,
        TV,
        Phone
    }
    
}