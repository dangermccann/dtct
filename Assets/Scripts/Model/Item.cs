using System;
using System.Linq;
using System.Collections.Generic;

namespace DCTC.Model {
    [Serializable]
    public class Items {
        public Dictionary<string, CableAttributes> CableAttributes { get; set; }
        public Dictionary<string, NodeAttributes> NodeAttributes { get; set; }
        public Dictionary<string, Termination> Termination { get; set; }
        public Dictionary<string, Backhaul> Backhaul { get; set; }
        public Dictionary<string, Fan> Fan { get; set; }
        public Dictionary<string, Rack> Rack { get; set; }
        public Dictionary<string, CPE> CPE { get; set; }

        public Items() {
            CableAttributes = new Dictionary<string, CableAttributes>();
            NodeAttributes = new Dictionary<string, NodeAttributes>();
            Termination = new Dictionary<string, Termination>();
            Backhaul = new Dictionary<string, Backhaul>();
            Fan = new Dictionary<string, Fan>();
            Rack = new Dictionary<string, Rack>();
            CPE = new Dictionary<string, CPE>();
        }

        public List<Item> All() {
            List<Item> all = new List<Item>();
            all.AddRange(CableAttributes.Values.Cast<Item>());
            all.AddRange(NodeAttributes.Values.Cast<Item>());
            all.AddRange(Termination.Values.Cast<Item>());
            all.AddRange(Backhaul.Values.Cast<Item>());
            all.AddRange(Fan.Values.Cast<Item>());
            all.AddRange(Rack.Values.Cast<Item>());
            all.AddRange(CPE.Values.Cast<Item>());
            return all;
        }

        public void AssignIDs() {
            foreach(string id in CableAttributes.Keys) {
                CableAttributes[id].ID = id;
            }
            foreach (string id in NodeAttributes.Keys) {
                NodeAttributes[id].ID = id;
            }
            foreach (string id in Termination.Keys) {
                Termination[id].ID = id;
            }
            foreach (string id in Backhaul.Keys) {
                Backhaul[id].ID = id;
            }
            foreach (string id in Fan.Keys) {
                Fan[id].ID = id;
            }
            foreach (string id in Rack.Keys) {
                Rack[id].ID = id;
            }
            foreach (string id in CPE.Keys) {
                CPE[id].ID = id;
            }
        }
    }

    [Serializable]
    public abstract class Item {
        public string ID { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public float Cost { get; set; }
        public string Technology { get; set; }
    }

    [Serializable]
    public abstract class RackedItem : Item {
        public int RackSpace { get; set; }
    }

    [Serializable]
    public class Termination : RackedItem {
        public float Throughput { get; set; }
        public int Subscribers { get; set; }
        public string Requires { get; set; }
        public float Heat { get; set; }
        public string Wiring { get; set; }
        public string Service { get; set; }
    }

    [Serializable]
    public class Backhaul : RackedItem {
        public float Throughput { get; set; }
    }

    [Serializable]
    public class Fan : RackedItem {
        public float Cooling { get; set; }
    }

    [Serializable]
    public class Rack : Item {
        public int Slots { get; set; }
    }

    [Serializable]
    public class CPE : Item {
        public float Throughput { get; set; }
        public float Reliability { get; set; }
        public List<string> Wiring { get; set; }
        public List<string> Services { get; set; }

        public CPE() {
            Wiring = new List<string>();
            Services = new List<string>();
        }
    }

    [Serializable]
    public class CableAttributes : Item {
        public int Range { get; set; }
        public string Color { get; set; }
    }

    [Serializable]
    public class NodeAttributes : Item {
        public string Wiring { get; set; }
    }

}