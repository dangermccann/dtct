using System;
using System.Collections.Generic;
using DCTC.Map;

namespace DCTC.Model {

    public delegate void ItemEventDelegate(object item);
    public delegate void ChangeDelegate();

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
                foreach (Cable cable in Cables) {
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

        public Node() {
            ID = Guid.NewGuid().ToString();
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
        PremiumTV,
        PremiumInternet,
        BasicDoublePlay,
        PremiumDoublePlay
    }
}