﻿using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public class GlobalInfo : MonoBehaviour {
        GameController gameController;
        Text money, customerCount, score, calls, trucks;
        int coolOff = 0;
        bool running = false;

        void Start() {
            gameController = GameController.Get();
            gameController.GameLoaded += () => { running = true; };
            if (gameController.Game != null && gameController.Game.Player != null)
                running = true;

            money = transform.FindChildRecursive("Money").gameObject.GetComponent<Text>();
            customerCount = transform.FindChildRecursive("CustomerCount").gameObject.GetComponent<Text>();
            score = transform.FindChildRecursive("Score").gameObject.GetComponent<Text>();
            calls = transform.FindChildRecursive("Calls").gameObject.GetComponent<Text>();
            trucks = transform.FindChildRecursive("Trucks").gameObject.GetComponent<Text>();
        }

        void Update() {
            if (running == false)
                return;

            coolOff--;

            if (coolOff < 0) {
                Redraw();
                coolOff = 30;
            }
        }

        public void Redraw() {
            Company player = gameController.Game.Player;
            money.text = Formatter.FormatCurrency(player.Money);

            int custCount = player.ActiveCustomers.Count();
            string countStr = Formatter.FormatInteger(custCount) + " Active";

            customerCount.text = countStr;

            score.text = player.Satisfaction.ToString("0") + "% Satisfaction";

            calls.text = player.CallCenter.CallQueue.Count.ToString() + " Queued Calls";
            trucks.text = player.TruckRollQueue.Count.ToString() + " Queued Trucks";
        }
    }
}