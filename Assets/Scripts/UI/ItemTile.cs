using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public class ItemTile : MonoBehaviour, IPointerClickHandler {
        public event ItemEvent ItemSelected;

        TextMeshProUGUI id;
        Image image;

        void Init() {
            id = transform.Find("ID").GetComponent<TextMeshProUGUI>();
            image = transform.Find("Image").GetComponent<Image>();
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
            if (id == null)
                Init();

            id.text = Item.ID;
            image.sprite = SpriteController.Get().GetSprite(Item.ID);
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (ItemSelected != null)
                ItemSelected(Item);
        }

    }
}