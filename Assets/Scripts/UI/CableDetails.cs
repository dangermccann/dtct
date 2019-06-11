using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;
using TMPro;

namespace DCTC.UI {
    public class CableDetails : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {


        private GameController gameController;

        // TODO: Global color palette somewhere?
        private static Color ActiveColor = Utilities.CreateColor(0x63C379);
        private static Color DisconnectedColor = Utilities.CreateColor(0xFF7575);

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

            string desc = "";
            desc += gameController.Game.GetOwnerOfCable(cable.Guid).Name;
            desc += " / " + Formatter.FormatDistance(cable.Positions.Count, Formatter.Units.Metric);
            SetText("Description", desc);

            string status;
            if (cable.Status == NetworkStatus.Disconnected) {
                status = "Disconnected from Plant";
                SetColor("Status", DisconnectedColor);
            } else {
                status = "Active";
                SetColor("Status", ActiveColor);
            }
            SetText("Status", status);
        }

        void SetText(string name, string value) {
            transform.Find(name).GetComponent<TextMeshProUGUI>().text = value;
        }

        void SetColor(string name, Color color) {
            transform.Find(name).GetComponent<TextMeshProUGUI>().color = color;
        }

        public void OnDeleteClicked() {
            transform.parent.SendMessage("DeleteCable", cable.Guid);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            transform.parent.SendMessage("HighlightCable", cable.Guid);
        }
        public void OnPointerExit(PointerEventData eventData) {
            transform.parent.SendMessage("UnhighlightCable", cable.Guid);
        }

    }
}