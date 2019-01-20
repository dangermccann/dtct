using System;
using UnityEngine;
using DCTC.Model;

namespace DCTC.UI {
    public class TruckCard : UIContainer {

        public Action<Truck> HireFireClicked;

        public GameObject hireButton, fireButton;

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

        public void OnHireFireClicked() {
            HireFireClicked.Invoke(Truck);
        }
    }
}