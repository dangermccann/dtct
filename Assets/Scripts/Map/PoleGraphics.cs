using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Model;

namespace DCTC.Map {
    public class PoleGraphics : MonoBehaviour {
        public float sagAmount = 0.3f;
        public float SelectionItensity = 1.0f;
        public Color SelectionColor = Color.cyan;


        private Vector3 world;
        public Vector3 World {
            get { return world; }
            set {
                world = value;
                Redraw();
            }
        }

        private Orientation orientation = Orientation.Horizontal;
        public Orientation Orientation {
            get { return orientation; }
            set {
                orientation = value;
                Redraw();
            }
        }

        private List<Cable> cables = new List<Cable>();

        void Redraw() {
            if (orientation == Orientation.Vertical) {
                transform.position = new Vector3(world.x + 3, world.y, world.z);
            } else {
                transform.position = new Vector3(world.x, world.y, world.z + 3);
            }

            // rotate 
            if (orientation == Orientation.Horizontal) {
                transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
            }
            else {
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }
        }

        public int AddCable(Cable cable, PoleGraphics to) {
            cables.Add(cable);
            int idx = cables.Count - 1;

            // TODO: what happens when we exceed 4?
            idx = idx % transform.childCount;

            if (to != null) {
                Cable_Procedural_Static comp = GetProcedural(idx);
                comp.sagAmplitude = sagAmount;
                comp.endPointTransform = to.Connection(cable);
                comp.Animate();
            }

            return idx;
        }

        public int CableIndex(Cable cable) {
            return cables.IndexOf(cable);
        }

        public void RemoveCable(Cable cable) {
            if (!cables.Contains(cable))
                return;

            int idx = cables.IndexOf(cable);
            for(int i = idx + 1; i < cables.Count; i++) {
                Cable_Procedural_Static comp0 = GetProcedural(i - 1);
                Cable_Procedural_Static comp1 = GetProcedural(i);
                comp0.endPointTransform = comp1.endPointTransform;
                comp0.Animate();
            }
            cables.RemoveAt(idx);
        }

        public bool IsEmpty() {
            return cables.Count == 0;
        }

        public Transform Connection(int idx) {
            return transform.Find("Connections/Cable" + idx.ToString());
        }

        public Transform Connection(Cable cable) {
            return Connection(cables.IndexOf(cable));
        }

        public void SetMaterial(int idx, Material material) {
            Connection(idx).GetComponent<LineRenderer>().material = material;
        }

        public void Select() {
            Transform graphics = transform.Find("Graphics");
            Select(graphics.gameObject, SelectionColor);

            for(int i = 0; i < graphics.childCount; i++) {
                Select(graphics.GetChild(i).gameObject, SelectionColor);
            }
        }

        public void Deselect() {
            Transform graphics = transform.Find("Graphics");
            Select(graphics.gameObject, Color.clear);

            for (int i = 0; i < graphics.childCount; i++) {
                Select(graphics.GetChild(i).gameObject, Color.clear);
            }
        }

        private void Select(GameObject go, Color color) {
            Material material = go.GetComponent<MeshRenderer>().material;

            if (color != Color.clear) {
                material.SetColor("_EmissionColor",
                    new Vector4(color.r, color.g, color.b, 0) * SelectionItensity);
                material.EnableKeyword("_EMISSION");
            } else {
                material.DisableKeyword("_EMISSION");
            }
        }

        private Cable_Procedural_Static GetProcedural(int idx) {
            GameObject connection = Connection(idx).gameObject;
            return connection.GetComponent<Cable_Procedural_Static>();
        }

    }
}