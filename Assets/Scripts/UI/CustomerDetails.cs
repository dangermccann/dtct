using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;


namespace DCTC.UI {
    public class CustomerDetails : MonoBehaviour {

        private bool initialized = false;
        private int coolDown = 0;

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

        void Update() {
            if (!initialized || Customer == null)
                return;

            if(++coolDown > 60) {
                coolDown = 0;
                Redraw();
            }
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
                SetActive("Satisfaction", false);
            } else {
                SetActive("ServiceTier", true);
                SetText("ServiceTier", Customer.ServiceTier.ToString());

                SetActive("Satisfaction", true);
                string satisfaction = "Satisfaction: " + Formatter.FormatDissatisfaction(Customer.Dissatisfaction);
                satisfaction += " (" + Customer.Dissatisfaction.ToString("0.00") + ")";
                SetText("Satisfaction", satisfaction);
            }
            
            SetText("Income", "Income: " + Formatter.FormatCurrency(Customer.Wealth * 10));
            SetText("Patience", "Demeanor: " + Formatter.FormatPatience(Customer.Patience));
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
    }
}