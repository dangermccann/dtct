using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public class ItemInstanceDetails : MonoBehaviour {
        TextMeshProUGUI title, description;
        AttributeTable attributes;

        private void Start() {
            Redraw();
        }

        void Init() {
            title = GetText("Title");
            description = GetText("Description");
            attributes = transform.Find("AttributeTable").GetComponent<AttributeTable>();
        }

        private TextMeshProUGUI GetText(string name) {
            return transform.Find(name).GetComponent<TextMeshProUGUI>();
        }

        private Item item;
        public Item Item {
            get { return item; }
            set {
                item = value;
                Redraw();
            }
        }

        void Redraw() {
            if (description == null)
                Init();

            attributes.Clear();

            if (item != null) {
                title.text = Item.ID + " " + Item.ShortDescription;
                description.text = Item.Description;

                List<KeyValuePair<string, string>> values = ItemAttributesGenerator.GenerateAttributes(item);
                foreach(var pair in values) {
                    FormatAttribute(pair.Key, pair.Value);
                }
            }
            else {
                title.text = "";
                description.text = "";
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        private void FormatAttribute(string key, string value) {
            attributes.Append("<color=#D1D1D1>" + key,  "<color=#FFFFFF>" + value);
        }

    }
}