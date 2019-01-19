using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.UI {
    public class WorkforceDetails : MonoBehaviour {

        public GameObject TruckCardPrefab;
        private Transform activeTrucksContent, availableTrucksContent;
        private GameController gameController;

        public void Awake() {
            activeTrucksContent = transform.Find("Contents/TrucksTab/ActiveTrucks/Viewport/Content");
            availableTrucksContent = transform.Find("Contents/TrucksTab/AvailableTrucks/Viewport/Content");
            gameController = GameController.Get();
        }

        public void Show() {
            Redraw();
        }

        public void Redraw() { 
            Utilities.DestroyAllChildren(activeTrucksContent);
            Utilities.DestroyAllChildren(availableTrucksContent);

            foreach(Truck truck in gameController.Game.Player.Trucks) {
                GameObject go = Instantiate(TruckCardPrefab, activeTrucksContent);
                TruckCard tc = go.GetComponent<TruckCard>();
                tc.Truck = truck;
                tc.IsHired = true;
                tc.HireFireClicked += (clicked) => {
                    gameController.Game.Player.FireTruck(clicked);
                    Redraw();
                };
            }

            foreach (Truck truck in gameController.Game.Player.UnhiredTrucks) {
                GameObject go = Instantiate(TruckCardPrefab, availableTrucksContent);
                TruckCard tc = go.GetComponent<TruckCard>();
                tc.Truck = truck;
                tc.IsHired = false;
                tc.HireFireClicked += (clicked) => {
                    gameController.Game.Player.HireTruck(clicked);
                    Redraw();
                };
            }
        }

    }
}