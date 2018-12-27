using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;


namespace DCTC.UI {
    public class CustomerDetails : MonoBehaviour {

        private Customer customer;
        public Customer Customer {
            get { return customer; }
            set {
                customer = value;
                Redraw();
            }
        }

        void Redraw() {
            Clear();

            if (Customer == null) {
                SetText("Name", "[No Customer]");
                return;
            }
            

            SetText("Name", Customer.Name);
            SetText("Address", Customer.Address);

            Company provider = Customer.Provider;
            SetText("Provider", provider == null ? "[No Service Provider]" : provider.Name);

            SetText("ServiceTier", provider == null ? "-" : Customer.ServiceTier.ToString());
            SetText("Income", "Income: $" + Customer.IncomeLevel.ToString());
            SetText("Patience", "Patience: " + Customer.Patience.ToString());
            SetText("Frustration", "Frustration: " + Customer.Frustration.ToString());
        }

        void Clear() {
            for(int i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).gameObject.GetComponent<Text>().text = "";
            }
        }

        void SetText(string name, string value) {
            if (value == null)
                value = "-";
            transform.Find(name).gameObject.GetComponent<Text>().text = value;
        }
    }
}