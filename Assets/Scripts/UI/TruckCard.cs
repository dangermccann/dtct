using System;
using UnityEngine;
using TMPro;
using DCTC.Model;

namespace DCTC.UI {
    public class TruckCard : MonoBehaviour {

        public Action<Truck> HireFireClicked;

        private GameObject hireButton, fireButton;

        void Awake() {
            hireButton = transform.Find("HireButton").gameObject;
            fireButton = transform.Find("FireButton").gameObject;
        }

        private Truck truck;
        public Truck Truck {
            get { return truck; }
            set {
                truck = value;
                Redraw();
            }
        }

        public bool IsHired {
            set {
                fireButton.SetActive(value);
                hireButton.SetActive(!value);
            }
        }

        void Redraw() {
            if (Truck == null)
                return;

            SetText("TruckName", Truck.Name);
            SetText("TruckAttributes/TravelSpeed", Formatter.FormatPercent(Truck.TravelSpeed));
            SetText("TruckAttributes/WorkSpeed", Formatter.FormatPercent(Truck.WorkSpeed));
            SetText("TruckAttributes/Salary", Formatter.FormatCurrency(Truck.Salary * 100f) + " / day");
        }

        void SetText(string name, string value) {
            GetText(name).text = value;
        }

        TextMeshProUGUI GetText(string name) {
            return transform.Find(name).GetComponent<TextMeshProUGUI>();
        }

        public void OnHireFireClicked() {
            HireFireClicked.Invoke(Truck);
        }
    }
}