using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;
using System.Collections.Generic;

namespace DCTC.UI {
    public class InventoryDetails : ItemGridContainer {
        public GameObject fieldContent, fieldItemDetails, headendContent;
        public GameObject rackContainer;
        public GameObject RakedItemPrefab, RackPrefab, HeadendItemDetails;

        private SpriteController spriteController;

        private static float rackedItemHeight = 31f;

        void Start() {
            fieldItemDetails.GetComponent<ItemDetails>().ItemBought += OnItemBought;
            spriteController = SpriteController.Get();

            fieldContent.SetActive(true);
            headendContent.SetActive(false);
        }


        protected override IEnumerator Redraw() {
            yield return null;

            if (gameController.Game != null) {

                ClearGrids(fieldContent.transform);

                fieldItemDetails.GetComponent<ItemDetails>().Item = null;

                Items items = gameController.Game.Items;
                AddCategory(fieldContent.transform, fieldItemDetails, "Devices", items.CPE);

                DrawRacks();
            }
        }

        void DrawRacks() {
            Utilities.Clear(rackContainer.transform, 0, 1);

            List<GameObject> rackGOs = new List<GameObject>();
            Company player = gameController.Game.Player;

            for(int i = 0; i < player.Racks.Count; i++) {
                GameObject rackGO = Instantiate(RackPrefab, rackContainer.transform);
                rackGO.GetComponent<Image>().sprite = spriteController.GetSprite("Rack");
                rackGO.name = "Rack " + i;
                rackGO.transform.SetSiblingIndex(i);

                int index = i;
                rackGO.transform.Find("Buy").GetComponent<Button>().onClick.AddListener(() => {
                    StateController.Get().PushState("/headend-buy/" + index);
                });

                RackedItemUI ri = rackGO.GetComponent<RackedItemUI>();
                ri.Item = player.Racks[i];
                ri.PointerEnter += OnItemPointerEnter;
                ri.PointerExit += OnItemPointerExit;
                ri.PointerClick += OnItemPointerClick;

                rackGOs.Add(rackGO);
            }

            int slotIndex = 0;
            int rackIndex = 0;

            foreach(Rack rack in player.Racks) {
                foreach(string id in rack.Contents) {
                    Item item = gameController.Game.Items[id];
                    RackedItem racked = item as RackedItem;
                    Transform container = rackGOs[rackIndex].transform.Find("container");
                    AppendRack(container, racked, slotIndex);
                    slotIndex += racked.RackSpace;
                }
                rackIndex++;
                slotIndex = 0;
            }
        }

        void AppendRack(Transform container, RackedItem racked, int idx) {
            GameObject go = Instantiate(RakedItemPrefab, container);
            go.name = idx.ToString() + " " + racked.ID;
            go.GetComponent<Image>().sprite = spriteController.GetSprite(racked.ID + "_flat");
            RackedItemUI ri = go.GetComponent<RackedItemUI>();
            ri.Item = racked;
            ri.PointerEnter += OnItemPointerEnter;
            ri.PointerExit += OnItemPointerExit;
            ri.PointerClick += OnItemPointerClick;
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(
                rackContainer.GetComponent<RectTransform>().sizeDelta.x,
                rackedItemHeight * racked.RackSpace);
        }

        protected void OnItemPointerEnter(Item item) {
            HeadendItemDetails.GetComponent<ItemInstanceDetails>().Item = item;
        }

        protected void OnItemPointerClick(Item item) {
        }

        protected void OnItemPointerExit(Item item) {
            ItemInstanceDetails id = HeadendItemDetails.GetComponent<ItemInstanceDetails>();

            if (id.Item == item) {
                id.Item = null;
            }
        }
    }
}