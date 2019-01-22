using System.Linq;
using UnityEngine;
using TMPro;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public class GlobalInfo : MonoBehaviour {
        GameController gameController;
        TextMeshProUGUI money, customerCount, score, calls, trucks;
        int coolOff = 0;
        bool running = false;

        void Start() {
            gameController = GameController.Get();
            gameController.GameLoaded += () => { running = true; };
            if (gameController.Game != null && gameController.Game.Player != null)
                running = true;

            money = transform.FindChildRecursive("Money").gameObject.GetComponent<TextMeshProUGUI>();
            customerCount = transform.FindChildRecursive("CustomerCount").gameObject.GetComponent<TextMeshProUGUI>();
            score = transform.FindChildRecursive("Score").gameObject.GetComponent<TextMeshProUGUI>();
            calls = transform.FindChildRecursive("Calls").gameObject.GetComponent<TextMeshProUGUI>();
            trucks = transform.FindChildRecursive("Trucks").gameObject.GetComponent<TextMeshProUGUI>();
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

            score.text = Formatter.FormatPercent(player.Satisfaction) + " Satisfaction";

            calls.text = player.CallCenter.CallQueue.Count.ToString() + " Queued Calls";
            trucks.text = player.TruckRollQueue.Count.ToString() + " Queued Trucks";
        }
    }
}