using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCTC.Controllers {
    public class MaterialController : MonoBehaviour {

        public static MaterialController Get() {
            return GameObject.FindGameObjectWithTag("GameController").GetComponent<MaterialController>();
        }

        private Dictionary<string, Material> materials = new Dictionary<string, Material>();

        void Awake() {
            LoadResources("Materials");
        }

        public Material GetMaterial(string material) {
            if (materials.ContainsKey(material))
                return materials[material];
            else return null;
        }

        public bool HasMaterial(string material) {
            return materials.ContainsKey(material);
        }

        private void LoadResources(string directory) {
            Material[] all = Resources.LoadAll<Material>(directory);
            foreach (Material m in all) {
                if (materials.ContainsKey(m.name)) {
                    Debug.LogWarning("Duplicate material: " + m.name);
                    continue;
                }
                materials.Add(m.name, m);
            }
        }

    }
}
