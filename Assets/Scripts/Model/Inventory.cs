using System;
using System.Collections;
using System.Collections.Generic;

namespace DCTC.Model {

    public delegate void InventoryChangedEvent(string item);

    [Serializable]
    public class Inventory<T> : IEnumerable<string> {
        [field: NonSerialized]
        public event InventoryChangedEvent ItemChanged;

        private Dictionary<string, T> inventory = new Dictionary<string, T>();

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

        public bool Contains(string item) {
            return inventory.ContainsKey(item);
        }

        public IEnumerator<string> GetEnumerator() {
            return inventory.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}