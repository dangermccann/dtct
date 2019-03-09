using UnityEngine;
using UnityEngine.EventSystems;
using DCTC.Model;

namespace DCTC.UI {
    public delegate void RackedItemPointerEnter(Item item);

    public class RackedItemUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler {
        public Item Item { get; set; }
        public event RackedItemPointerEnter PointerEnter, PointerExit, PointerClick;

        public void OnPointerEnter(PointerEventData eventData) {
            if (PointerEnter != null)
                PointerEnter(Item);
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (PointerExit != null)
                PointerExit(Item);
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (PointerClick != null)
                PointerClick(Item);
        }
    }
}