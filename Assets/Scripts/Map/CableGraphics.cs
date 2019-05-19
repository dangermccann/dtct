using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Model;

namespace DCTC.Map {
    public class CableGraphics : MonoBehaviour {

        public enum GraphicsMode {
            Cursor,
            Placed
        }

        public GameObject PolePrefab;

        private bool needsRedraw = false;

        private List<GameObject> poles = new List<GameObject>();

        [HideInInspector]
        public string CableId;

        private Orientation orientation = Orientation.Horizontal;
        public Orientation Orientation {
            get { return orientation; }
            set {
                orientation = value;
                Redraw();
            }
        }

        private bool valid = true;
        public bool Valid {
            get { return valid; }
            set {
                valid = value;
                Redraw();
            }
        }

        private GraphicsMode mode = GraphicsMode.Cursor;
        public GraphicsMode Mode {
            get { return mode; }
            set {
                mode = value;
                Redraw();
            }
        }

        private Cable cable;
        public Cable Cable { get {
                return cable;
            }
            set {
                if (cable != null)
                    cable.StatusChanged -= Invalidate;

                cable = value;
                cable.StatusChanged += Invalidate;
            }
        } 

        [HideInInspector]
        private List<TilePosition> points = new List<TilePosition>();
        public List<TilePosition> Points {
            get { return points; }
            set {
                points = value;
                Redraw();
            }
        }

        void OnDestroy() {
            if (cable != null)
                cable.StatusChanged -= Invalidate;
        }

        private void Update() {
            if (needsRedraw) {
                Redraw();
                needsRedraw = false;
            }
        }

        private void Invalidate() {
            needsRedraw = true;
        }

        private void Redraw() {
            if(Mode == GraphicsMode.Cursor) {
                RedrawCursor();
            }
            else {
                RedrawPlaced();
            }
        }

        private void RedrawCursor() {

        }

        private void RedrawPlaced() {
        }



        private Color CableColor() {
            Color c;
            ColorUtility.TryParseHtmlString(Cable.Attributes.Color, out c);
            return c;
        }

    }

}