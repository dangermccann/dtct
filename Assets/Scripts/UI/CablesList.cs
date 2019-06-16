using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Map;
using DCTC.Controllers;

namespace DCTC.UI {
    public class CablesList : MonoBehaviour {

        public GameObject CableDetailsPrefab;
        public GameObject NodeDetailsPrefab;
        public SelectionController selectionController;
        public CableGraphics cableGraphics;
        private GameController gameController;

        private IEnumerable<Cable> cables = new List<Cable>();
        private IEnumerable<Node> nodes = new List<Node>();

        private void Start() {
            gameController = GameController.Get();
            selectionController.SelectionChanged += OnSelectionChange;
        }

        private void OnSelectionChange() {
            if(selectionController.Mode == SelectionController.SelectionModes.Selected) {
                TilePosition pos = selectionController.SelectedPosition;
                if(pos != TilePosition.Origin) {
                    cables = gameController.Game.GetCablesAt(pos);
                    nodes = gameController.Game.GetNodesAt(pos);
                }
                else {
                    cables = new List<Cable>();
                    nodes = new List<Node>();
                }
            }
            else {
                cables = new List<Cable>();
                nodes = new List<Node>();
            }

            Redraw();
        }

        void Redraw() {
            Utilities.Clear(transform);
            foreach(Cable cable in cables) {
                GameObject go = Instantiate(CableDetailsPrefab, transform);
                go.GetComponent<CableDetails>().Cable = cable;
            }

            foreach (Node node in nodes) {
                GameObject go = Instantiate(NodeDetailsPrefab, transform);
                go.GetComponent<NodeDetails>().Node = node;
            }
        }

        void DeleteCable(string guid) {
            gameController.Game.Player.RemoveCable(guid);
            OnSelectionChange();
        }

        void DeleteNode(string guid) {
            gameController.Game.Player.RemoveNode(guid);
            OnSelectionChange();
        }

        void HighlightCable(string guid) {
            cableGraphics.HighlightCable(cables.First(c => c.Guid == guid));
        }

        void UnhighlightCable(string guid) {
            cableGraphics.UnhighlightCable(cables.First(c => c.Guid == guid));
        }
    }
}