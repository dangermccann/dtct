using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCTC.UI {
    [RequireComponent(typeof(Toggle))]
    public class TriStateToggle : MonoBehaviour {
        public GameObject PrimaryGraphic, SecondaryGraphic;
        private Toggle toggle;
        private void Start() {
            toggle = GetComponent<Toggle>();
            SecondaryGraphic.SetActive(false);
        }

        bool secondary = false;
        public bool IsSecondary {
            get {
                return secondary;
            }
            set {
                secondary = value;
                SecondaryGraphic.SetActive(secondary);
            }
        }

    }

}