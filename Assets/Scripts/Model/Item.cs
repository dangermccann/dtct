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

        private List<Item> all = new List<Item>();

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
            if (all.Count == 0) {
                all.AddRange(CableAttributes.Values.Cast<Item>());
                all.AddRange(NodeAttributes.Values.Cast<Item>());
                all.AddRange(Termination.Values.Cast<Item>());
                all.AddRange(Backhaul.Values.Cast<Item>());
                all.AddRange(Fan.Values.Cast<Item>());
                all.AddRange(Rack.Values.Cast<Item>());
                all.AddRange(CPE.Values.Cast<Item>());
            }
            return all;
        }

        public Item this[string id] {
            get {
                return All().Find((item) => item.ID == id);
            }
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

        public IEnumerable<T> FromInventory<T>(Inventory inventory) where T : Item {
            List<T> items = new List<T>();
            foreach (string id in inventory) {
                if(this[id] is T)
                    items.Add(this[id] as T);
            }
            return items;
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
        public const string HR15 = "HR-15";

        public int Slots { get; set; }
        public List<string> Contents { get; set; }
        public Rack() {
            Contents = new List<string>();
        }

        public Rack(Rack clone) {
            this.ID = clone.ID;
            this.ShortDescription = clone.ShortDescription;
            this.Description = clone.Description;
            this.Image = clone.Image;
            this.Cost = clone.Cost;
            this.Technology = clone.Technology;
            this.Slots = clone.Slots;
            Contents = new List<string>();
        }

        public int AvailableSlots(Items items) {
            int available = Slots;

            foreach(string id in Contents) {
                Item item = items[id];
                available -= (item as RackedItem).RackSpace;
            }

            return available;
        }
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
        public string Color { get; set; }
        public string Wiring { get; set; }
    }

    [Serializable]
    public class NodeAttributes : Item {
        public int Range { get; set; }
        public string Wiring { get; set; }
    }

}