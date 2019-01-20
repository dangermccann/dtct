using UnityEngine;
using TMPro;
using System.Collections;

namespace DCTC.UI {
    public class UIContainer : MonoBehaviour {

        protected void SetText(string name, string value) {
            GetText(name).text = value;
        }

        protected TextMeshProUGUI GetText(string name) {
            return transform.Find(name).GetComponent<TextMeshProUGUI>();
        }

    }
}