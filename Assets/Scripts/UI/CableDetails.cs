using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;
using TMPro;

namespace DCTC.UI {
    public class CableDetails : MonoBehaviour {


        private GameController gameController;

        private Cable cable;
        public Cable Cable {
            get { return cable; }
            set {
                cable = value;
                Redraw();
            }
        }

        void Redraw() {
            if(gameController == null)
                gameController = GameController.Get();

            SetText("Name", cable.ID + " " + cable.Type.ToString() + " Cable");

            string desc = gameController.Game.GetOwnerOfCable(cable.Guid).Name;
            desc += " / " + Formatter.FormatDistance(cable.Positions.Count, Formatter.Units.Metric);
            SetText("Description", desc);
        }

        void SetText(string name, string value) {
            transform.Find(name).GetComponent<TextMeshProUGUI>().text = value;
        }

        public void OnDeleteClicked() {
            transform.parent.SendMessage("DeleteCable", cable.Guid);
        }

    }
}