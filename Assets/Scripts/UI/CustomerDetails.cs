using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;


namespace DCTC.UI {
    public class CustomerDetails : MonoBehaviour {

        private bool initialized = false;

        private Customer customer;
        public Customer Customer {
            get { return customer; }
            set {
                customer = value;
                Redraw();
            }
        }

        void Init() {
            initialized = true;
            GameController.Get().Game.CustomerChanged += Game_CustomerChanged;
        }

        private void Game_CustomerChanged(Customer _customer, Company company) {
            if(Customer != null && _customer.ID == Customer.ID) {
                Redraw();
            }
        }

        void Redraw() {
            if(initialized == false) {
                Init();
            }

            Clear();

            if (Customer == null) {
                SetText("Name", "[No Customer]");
                return;
            }
            

            SetText("Name", Customer.Name + " Household");
            SetText("Address", Customer.Address);

            Company provider = Customer.Provider;
            SetText("Provider", provider == null ? "[No Service Provider]" : provider.Name);
            if (provider == null) {
                SetActive("ServiceTier", false);
                SetActive("Frustration", false);
            } else {
                SetActive("ServiceTier", true);
                SetText("ServiceTier", Customer.ServiceTier.ToString());

                SetActive("Frustration", true);
                SetText("Frustration", "Satisfaction: " + FormatFrustration(Customer.Frustration));
            }
            
            SetText("Income", "Income: $" + FormatIncome(Customer.Wealth));
            SetText("Patience", "Patience: " + FormatPatience(Customer.Patience));
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

        void SetActive(string name, bool active) {
            transform.Find(name).gameObject.SetActive(active);
        }

        static string FormatIncome(float amount) {
            return (Mathf.RoundToInt(amount * 10)).ToString();
        }

        static string FormatPatience(float amount) {
            if (amount < 0.33f)
                return "Low";
            if (amount < 0.66f)
                return "Medium";
            return "High";
        }

        static string FormatFrustration(float amount) {
            if (amount < 2)
                return "Content";
            if (amount < 5)
                return "Frustrated";
            if (amount < 8)
                return "Angry";
            return "Furious";
        }
    }
}