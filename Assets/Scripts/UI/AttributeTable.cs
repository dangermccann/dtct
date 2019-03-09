using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DCTC.UI {
    
    public class AttributeTable : MonoBehaviour {
        public GameObject AttributePrefab;

        public void Append(string key, string value) {
            AppendOne(key);
            AppendOne(value);
        }

        public void Clear() {
            Utilities.Clear(this.transform);
        }

        private void AppendOne(string str) {
            GameObject go = Instantiate(AttributePrefab, this.transform);
            go.GetComponent<TextMeshProUGUI>().text = str;
        }
    }
}