using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Model;

namespace DCTC.Map {
    public class PoleGraphics : MonoBehaviour {
        public float sagAmount = 0.4f;
        public float SelectionItensity = 1.0f;
        public Color SelectionColor = Color.cyan;
        private const int ConnectionPoints = 4;


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

        private Cable[] cables = new Cable[8];

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

        public int NextSlot() {
            for(int i = 0; i < cables.Length; i++) {
                if (cables[i] == null)
                    return i;
            }

            // TODO: what happens when we exceed maximum?
            return -1;
        }

        public int CableCount() {
            int count = 0;
            for (int i = 0; i < cables.Length; i++) {
                if (cables[i] != null)
                    count++;
            }
            return count;
        }

        public int AddCable(Cable cable, PoleGraphics to) {
            int idx = CableIndex(cable);

            if(idx == -1)
                idx = NextSlot();

            cables[idx] = cable;
            
            if (to != null) {
                Cable_Procedural_Static comp = GetProcedural(idx);
                comp.sagAmplitude = sagAmount;
                comp.endPointTransform = to.Connection(cable);
                comp.Animate();
            }

            return idx;
        }

        public int CableIndex(Cable cable) {
            for (int i = 0; i < cables.Length; i++) {
                if (cables[i] == cable)
                    return i;
            }
            return -1;
        }

        public void RemoveCable(Cable cable) {
            int idx = CableIndex(cable);
            if (idx == -1)
                return;

            Cable_Procedural_Static comp = GetProcedural(idx);
            comp.endPointTransform = null;
            comp.Animate();

            cables[idx] = null;
        }

        public bool IsEmpty() {
            return CableCount() == 0;
        }

        public static PoleGraphics GraphicsFromConnection(Transform conn) {
            return conn.parent.parent.gameObject.GetComponent<PoleGraphics>();
        }

        public Transform Connection(int idx) {
            if (idx == -1)
                return null;

            idx = idx % ConnectionPoints;
            return transform.Find("Connections/Cable" + idx.ToString());
        }

        public Transform Connection(Cable cable) {
            return Connection(CableIndex(cable));
        }

        public void SetMaterial(int idx, Material material) {
            Connection(idx).GetComponent<LineRenderer>().material = material;
        }

        public void Highlight() {
            Transform graphics = transform.Find("Graphics");
            Highlight(graphics.gameObject.GetComponent<MeshRenderer>(), SelectionColor);

            for(int i = 0; i < graphics.childCount; i++) {
                Highlight(graphics.GetChild(i).gameObject.GetComponent<MeshRenderer>(), SelectionColor);
            }
        }

        public void Unhighlight() {
            Transform graphics = transform.Find("Graphics");
            Highlight(graphics.gameObject.GetComponent<MeshRenderer>(), Color.clear);

            for (int i = 0; i < graphics.childCount; i++) {
                Highlight(graphics.GetChild(i).gameObject.GetComponent<MeshRenderer>(), Color.clear);
            }
        }

        public void HighlightCable(Cable cable) {
            int idx = CableIndex(cable);
            if(idx != -1) {
                Highlight(Connection(idx).GetComponent<LineRenderer>(), SelectionColor);
            }
        }

        public void UnhighlightCable(Cable cable) {
            int idx = CableIndex(cable);
            if (idx != -1) {
                Highlight(Connection(idx).GetComponent<LineRenderer>(), Color.clear);
            }
        }

        private void Highlight(Renderer renderer, Color color) {
            Material material = renderer.material;

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