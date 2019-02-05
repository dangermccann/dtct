using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public class ItemTooltip : MonoBehaviour {

        TextMeshProUGUI id, description, shortDescription;
        Image image;

        void Awake() {
            id = GetText("ID");
            description = GetText("Description");
            shortDescription = GetText("ShortDescription");
            image = transform.Find("Image").GetComponent<Image>();
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

        public void Redraw() {
            id.text = Item.ID;
            description.text = Item.Description;
            shortDescription.text = Item.ShortDescription;
            image.sprite = SpriteController.Get().GetSprite(Item.ID);
        }

    }
}