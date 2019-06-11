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
        public SelectionController selectionController;
        public CableGraphics cableGraphics;
        private GameController gameController;

        private IEnumerable<Cable> cables = new List<Cable>();
        public IEnumerable<Cable> Cables {
            get { return cables; }
            set {
                cables = value;
                Redraw();
            }
        }

        private void Start() {
            gameController = GameController.Get();
            selectionController.SelectionChanged += OnSelectionChange;
        }

        private void OnSelectionChange() {
            if(selectionController.Mode == SelectionController.SelectionModes.Selected) {
                TilePosition pos = selectionController.SelectedPosition;
                if(pos != TilePosition.Origin) {
                    Cables = gameController.Game.GetCablesAt(pos);
                }
                else {
                    Cables = new List<Cable>();
                }
            }
            else {
                Cables = new List<Cable>();
            }
        }

        void Redraw() {
            Utilities.Clear(transform);
            foreach(Cable cable in cables) {
                GameObject go = Instantiate(CableDetailsPrefab, transform);
                go.GetComponent<CableDetails>().Cable = cable;
            }
        }

        void DeleteCable(string guid) {
            gameController.Game.Player.RemoveCable(guid);
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