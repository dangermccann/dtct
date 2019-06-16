using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;
using TMPro;

namespace DCTC.UI {
    public class NodeDetails : MonoBehaviour {


        private GameController gameController;

        // TODO: Global color palette somewhere?
        private static Color ActiveColor = Utilities.CreateColor(0x63C379);
        private static Color DisconnectedColor = Utilities.CreateColor(0xFF7575);

        private Node node;
        public Node Node {
            get { return node; }
            set {
                node = value;
                Redraw();
            }
        }

        void Redraw() {
            if (gameController == null)
                gameController = GameController.Get();

            SetText("Name", node.ID + " " + node.Type.ToString() + " Node");

            Company owner = gameController.Game.GetOwnerOfNode(node.Guid);

            string desc = "";
            desc += owner.Name;
            desc += " / Range: " + Formatter.FormatDistance(node.Attributes.Range, Formatter.Units.Metric);
            SetText("Description", desc);

            string status;
            if (node.Status == NetworkStatus.Disconnected) {
                status = "Disconnected from Plant";
                SetColor("Status", DisconnectedColor);
            } else {
                status = "Active";
                SetColor("Status", ActiveColor);
            }
            SetText("Status", status);

            transform.Find("Button").gameObject.SetActive(owner == gameController.Game.Player);
        }

        void SetText(string name, string value) {
            transform.Find(name).GetComponent<TextMeshProUGUI>().text = value;
        }

        void SetColor(string name, Color color) {
            transform.Find(name).GetComponent<TextMeshProUGUI>().color = color;
        }

        public void OnDeleteClicked() {
            transform.parent.SendMessage("DeleteNode", node.Guid);
        }

    }
}