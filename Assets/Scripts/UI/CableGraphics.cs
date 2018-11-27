﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Map;

namespace DCTC.UI {
    public class CableGraphics : MonoBehaviour {

        public enum GraphicsMode {
            Cursor,
            Placed
        }

        private GameObject vertical, horizontal;
        private readonly float y = 0.1f;

        public Color ValidColor = Color.green;
        public Color InvalidColor = Color.red;

        private void Start() {
            vertical = transform.Find("Vertical").gameObject;
            horizontal = transform.Find("Horizontal").gameObject;
        }

        private Orientation orientation = Orientation.Horizontal;
        public Orientation Orientation {
            get { return orientation; }
            set {
                orientation = value;
                Redraw();
            }
        }

        private bool intersection = false;
        public bool Intersection {
            get { return intersection; }
            set {
                intersection = value;
                Redraw();
            }
        }

        private bool valid = true;
        public bool Valid {
            get { return valid; }
            set {
                valid = value;
                if (!valid)
                    intersection = true;
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

        [HideInInspector]
        private List<TilePosition> points = new List<TilePosition>();
        public List<TilePosition> Points {
            get { return points; }
            set {
                points = value;
                Redraw();
            }
        }

        private void Redraw() {
            if(Mode == GraphicsMode.Cursor) {
                RedrawCursor();
            }
            else {
                RedrawPlaced();
            }
            

            if (valid) {
                SetLineColor(vertical, ValidColor);
                SetLineColor(horizontal, ValidColor);
            } else {
                SetLineColor(vertical, InvalidColor);
                SetLineColor(horizontal, InvalidColor);
            }
        }

        private void RedrawCursor() {
            LineRenderer lr = vertical.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPositions(new Vector3[] {
                new Vector3(1, y, 0),
                new Vector3(1, y, 2),
            });

            lr = horizontal.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPositions(new Vector3[] {
                new Vector3(0, y, 1),
                new Vector3(2, y, 1),
            });

            if (intersection == true) {
                vertical.SetActive(true);
                horizontal.SetActive(true);
            } else {
                if (Orientation == Orientation.Vertical) {
                    vertical.SetActive(true);
                    horizontal.SetActive(false);
                } else {
                    vertical.SetActive(false);
                    horizontal.SetActive(true);
                }
            }
        }

        private void RedrawPlaced() {
            vertical.SetActive(false);
            horizontal.SetActive(true);

            List<Vector3> positions = new List<Vector3>();
            foreach(TilePosition pos in Points) {
                Vector3 world = ThreeDMap.PositionToWorld(pos);
                positions.Add(new Vector3(world.x + 1, y, world.z + 1));
            }

            LineRenderer lr = horizontal.GetComponent<LineRenderer>();
            lr.positionCount = positions.Count;
            lr.SetPositions(positions.ToArray());
        }

        private void SetLineColor(GameObject go, Color color) {
            LineRenderer lr = go.GetComponent<LineRenderer>();
            lr.startColor = color;
            lr.endColor = color;
        }
    }

}