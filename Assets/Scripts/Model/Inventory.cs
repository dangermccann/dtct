using System;
using System.Collections;
using System.Collections.Generic;

namespace DCTC.Model {

    public delegate void InventoryChangedEvent(string item);

    [Serializable]
    public class Inventory : InventoryBase<int> {
        public override void Add(string key, int amount) {
            if (!inventory.ContainsKey(key))
                inventory[key] = amount;
            else
                inventory[key] += amount;
        }

        public override void Subtract(string key, int amount) {
            inventory[key] -= amount;
            if(inventory[key] <= 0) {
                inventory.Remove(key);
            }
        }
    }

    [Serializable]
    public abstract class InventoryBase<T> : IEnumerable<string> {
        [field: NonSerialized]
        public event InventoryChangedEvent ItemChanged;

        protected Dictionary<string, T> inventory = new Dictionary<string, T>();

        public T this[string item] {
            get {
                if (Contains(item))
                    return inventory[item];
                return default(T);
            }
            set {
                inventory[item] = value;
                if (ItemChanged != null)
                    ItemChanged(item);
            }
        }

        public abstract void Add(string key, T amount);
        public abstract void Subtract(string key, T amount);

        public void Add(InventoryBase<T> added) {
            foreach(string key in added) {
                Add(key, added[key]);
            }
        }

        public bool Contains(string item) {
            return inventory.ContainsKey(item);
        }

        public void Clear() {
            inventory.Clear();
        }

        public IEnumerator<string> GetEnumerator() {
            return inventory.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}