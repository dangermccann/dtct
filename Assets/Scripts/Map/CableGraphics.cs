using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DCTC.Model;
using DCTC.Controllers;
using DCTC.UI;

namespace DCTC.Map {
    public class CableGraphics : MonoBehaviour {

        public GameObject PolePrefab;
        public GameObject labelPrefab;

        private bool needsRedraw = false;

        private Dictionary<TilePosition, PoleGraphics> poles = new Dictionary<TilePosition, PoleGraphics>();
        private PoleGraphics cursor;
        private GameController gameController;
        private MaterialController materialController;
        private GameObject canvas;
        private GameObject selectionLabel;

        private Cable selectionCable;
        public Cable SelectionCable {
            get {
                return selectionCable;
            }
            set {
                if (selectionCable != null)
                    selectionCable.StatusChanged -= Invalidate;

                selectionCable = value;
                selectionCable.StatusChanged += Invalidate;
            }
        } 

        private void Start() {
            gameController = GameController.Get();
            materialController = MaterialController.Get();
            canvas = transform.parent.GetChild(0).gameObject;
        }

        void OnDestroy() {
            if (selectionCable != null)
                selectionCable.StatusChanged -= Invalidate;
        }

        private void Update() {
            if (needsRedraw) {
                Redraw();
                needsRedraw = false;
            }
        }

        // TODO: change this to work on a per cable basis rather than redrawing everything
        private void Invalidate() {
            needsRedraw = true;
        }

        public void Redraw() {
            RedrawSelection();
            RedrawPlaced();
        }

        public void RemoveSelection() {
            if(selectionCable != null)
                RemoveCable(selectionCable);

            if(selectionLabel != null)
                selectionLabel.SetActive(false);
        }

        public void RedrawSelection() {
            if (selectionCable != null) {
                DrawCable(selectionCable);
                DrawLabel(selectionCable);
            }
        }

        private PoleGraphics GetOrCreatePole(Tile tile) {
            PoleGraphics pole = null;
            TilePosition pos = tile.Position;

            List<TilePosition> candidates = new List<TilePosition>();
            candidates.Add(pos);
            if(tile.RoadType == RoadType.Vertical) {
                candidates.Add(MapConfiguration.North(pos));
                candidates.Add(MapConfiguration.South(pos));
            }
            else if(tile.RoadType == RoadType.Horizontal) {
                candidates.Add(MapConfiguration.East(pos));
                candidates.Add(MapConfiguration.West(pos));
            }

            foreach (TilePosition candidate in candidates) {
                if (poles.ContainsKey(candidate)) {
                    pole = poles[candidate];
                    break;
                }
            }

            if (pole == null) {
                GameObject go = Instantiate(PolePrefab, transform);
                go.name = "Pole " + pos.ToString();
                
                pole = go.GetComponent<PoleGraphics>();
                pole.World = ThreeDMap.PositionToWorld(pos);
                poles.Add(pos, pole);

                if (tile.RoadType == RoadType.Horizontal) {
                    pole.Orientation = Orientation.Horizontal;
                } else {
                    pole.Orientation = Orientation.Vertical;
                }
            }

            return pole;
        }

        public void DrawCable(Cable cable) {
            int idx = 0;
            int total = cable.Positions.Count;

            PoleGraphics last = null;

            foreach (TilePosition pos in cable.Positions) {
                Tile tile = gameController.Map.Tiles[pos];
                if (tile.RoadType == RoadType.Vertical || tile.RoadType == RoadType.Horizontal) {
                    if (idx % 2 == 0 || idx == total - 1) {
                        PoleGraphics pole = GetOrCreatePole(tile);

                        if (pole != last) {
                            int connectionIdx = pole.AddCable(cable, last);
                            pole.SetMaterial(connectionIdx, GetCableMaterial(cable));

                            last = pole;
                        }
                    }

                    idx++;
                }
            }
        }

        private Material GetCableMaterial(Cable cable) {
            return materialController.GetMaterial(cable.Type.ToString());
        }

        public void RemoveCable(Cable cable) {
            foreach (TilePosition pos in cable.Positions) {
                if (poles.ContainsKey(pos)) {
                    PoleGraphics pole = poles[pos];
                    pole.RemoveCable(cable);

                    if (pole.IsEmpty()) {
                        DestroyImmediate(pole.gameObject);
                        poles.Remove(pos);
                    }
                }
            }
        }
        
        public void InitSelection() {
            Debug.Log("InitSelection");
            CancelSelection();
            GameObject go = Instantiate(PolePrefab, transform);
            go.name = "Selection";
            cursor = go.GetComponent<PoleGraphics>();
        }

        public void CancelSelection() {
            if (cursor != null) {
                Destroy(cursor.gameObject);
                cursor = null;
            }

            if (selectionCable != null) {
                RemoveCable(selectionCable);
                selectionCable = null;
            }

            if (selectionLabel != null)
                selectionLabel.SetActive(false);
        }

        public void UpdateSelection(TilePosition pos) {
            pos = gameController.Map.NearestPoleLocation(pos);

            if (gameController.Map.IsInBounds(pos)) {

                Vector3 world = ThreeDMap.PositionToWorld(pos);
                cursor.gameObject.SetActive(true);
                cursor.World = world;

                Tile tile = gameController.Map.Tiles[pos];

                if (tile.RoadType == RoadType.Horizontal) {
                    cursor.Orientation = Orientation.Horizontal;
                } else {
                    cursor.Orientation = Orientation.Vertical;
                }
            }
            else {
                cursor.gameObject.SetActive(false);
            }
        }

        public void RedrawPlaced() {
            
        }

        public bool HiglightPole(TilePosition pos) {
            if(poles.ContainsKey(pos)) {
                poles[pos].Highlight();
                return true;
            }

            return false;
        }

        public void UnhighlightPole(TilePosition pos) {
            if (poles.ContainsKey(pos)) {
                poles[pos].Unhighlight();
            }
        }

        public void HighlightCable(Cable cable) {
            foreach(TilePosition pos in cable.Positions) {
                if (poles.ContainsKey(pos)) {
                    poles[pos].HighlightCable(cable);
                }
            }
        }

        public void UnhighlightCable(Cable cable) {
            foreach (TilePosition pos in cable.Positions) {
                if (poles.ContainsKey(pos)) {
                    poles[pos].UnhighlightCable(cable);
                }
            }
        }

        private void DrawLabel(Cable cable) {
            if (selectionLabel == null) {
                selectionLabel = Instantiate(labelPrefab, canvas.transform);
            }

            if(cable == null || cable.Positions.Count == 0) {
                selectionLabel.SetActive(false);
            }
            else {
                selectionLabel.SetActive(true);
                Vector3 world = ThreeDMap.PositionToWorld(cable.Positions.First());
                selectionLabel.transform.position = new Vector3(world.x, world.y + 1f, world.z);

                float cost = cable.Cost * cable.Positions.Count;
                selectionLabel.GetComponent<TMPro.TextMeshProUGUI>().text = Formatter.FormatCurrency(cost);
            }
            
        }


        private Color CableColor(Cable cable) {
            Color c;
            ColorUtility.TryParseHtmlString(cable.Attributes.Color, out c);
            return c;
        }

    }

}