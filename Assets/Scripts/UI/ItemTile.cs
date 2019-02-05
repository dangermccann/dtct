using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public class ItemTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        TextMeshProUGUI id;
        Image image;

        void Awake() {
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
            id.text = Item.ID;
            image.sprite = SpriteController.Get().GetSprite(Item.ID);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            Debug.Log("Enter " + item.ID);
        }

        public void OnPointerExit(PointerEventData eventData) {
            Debug.Log("Exit " + item.ID);
        }

    }
}