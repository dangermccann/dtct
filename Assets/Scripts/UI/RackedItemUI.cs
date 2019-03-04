using UnityEngine;
using UnityEngine.EventSystems;
using DCTC.Model;

namespace DCTC.UI {
    public delegate void RackedItemPointerEnter(RackedItem item);

    public class RackedItemUI : MonoBehaviour, IPointerEnterHandler {
        public RackedItem Item { get; set; }
        public event RackedItemPointerEnter PointerEnter;

        public void OnPointerEnter(PointerEventData eventData) {
            if (PointerEnter != null)
                PointerEnter(Item);
        }
    }
}