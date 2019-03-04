using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public class ItemTile : MonoBehaviour, IPointerClickHandler {
        public event ItemEvent ItemSelected;

        TextMeshProUGUI id, count;
        Image image;

        void Init() {
            id = transform.Find("ID").GetComponent<TextMeshProUGUI>();
            count = transform.Find("Count").GetComponent<TextMeshProUGUI>();
            image = transform.Find("Image").GetComponent<Image>();
        }

        void OnDestroy() {
            if(inventory != null)
                inventory.ItemChanged -= Inventory_ItemChanged;
        }

        private Item item;
        public Item Item {
            get { return item; }
            set {
                item = value;
                Redraw();
            }
        }

        private Inventory<int> inventory;
        public Inventory<int> Inventory {
            get { return inventory; }
            set {
                inventory = value;
                inventory.ItemChanged += Inventory_ItemChanged;
            }
        }

        private void Inventory_ItemChanged(string id) {
            if(item.ID == id) {
                Redraw();
            }
        }

        public void Redraw() {
            if (id == null)
                Init();

            id.text = Item.ID;
            image.sprite = SpriteController.Get().GetSprite(Item.ID);

            if (Inventory != null) {
                int qty = Inventory[Item.ID];
                if (qty > 0)
                    count.text = Formatter.FormatInteger(qty);
                else
                    count.text = "";
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (ItemSelected != null)
                ItemSelected(Item);
        }

    }
}