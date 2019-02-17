using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.UI {
    public delegate void ItemEvent(Item item);

    public class InventoryDetails : MonoBehaviour {
        public GameObject ItemTilePrefab, ItemGridPrefab;
        public GameObject fieldContent, headendContent, fieldItemDetails, headendItemDetails;
        private GameController gameController;

        void Start() {
            fieldItemDetails.GetComponent<ItemDetails>().ItemBought += OnItemBought;
            headendItemDetails.GetComponent<ItemDetails>().ItemBought += OnItemBought;
        }

        void OnItemBought(Item i, int qty) {
            Debug.Log("Bought " + qty.ToString() + " " + i.ID);
            gameController.Game.Player.Inventory[i.ID] += qty;
        }

        void OnEnable() {
            gameController = GameController.Get();
            Redraw();
        }

        void Redraw() {
            if (gameController.Game == null)
                return;

            ClearGrids(fieldContent.transform);
            ClearGrids(headendContent.transform);

            headendItemDetails.GetComponent<ItemDetails>().Item = null;
            fieldItemDetails.GetComponent<ItemDetails>().Item = null;

            Items items = gameController.Game.Items;
            AddCategory(headendContent.transform, headendItemDetails, "Termination", items.Termination);
            AddCategory(headendContent.transform, headendItemDetails, "Backhaul", items.Backhaul);

            Dictionary<string, Item> rackItems = new Dictionary<string, Item>();
            rackItems.AddMany(items.Fan);
            rackItems.AddMany(items.Rack);

            AddCategory(headendContent.transform, headendItemDetails, "Racks", rackItems);
            AddCategory(fieldContent.transform, fieldItemDetails, "Devices", items.CPE);
        }

        void AddCategory<T>(Transform _parent, GameObject details, 
            string name, Dictionary<string, T> categoryItems) where T : Item {

            GameObject grid = Instantiate(ItemGridPrefab, _parent);
            grid.transform.SetSiblingIndex(_parent.childCount - 2);
            Utilities.Clear(grid.transform);
            ToggleGroup group = _parent.GetComponent<ToggleGroup>();
            ItemDetails itemDetails = details.GetComponent<ItemDetails>();

            List<string> orderedKeys = new List<string>(categoryItems.Keys);
            orderedKeys.Sort();

            foreach (string id in orderedKeys) {
                T item = categoryItems[id];
                GameObject tile = Instantiate(ItemTilePrefab, grid.transform);

                ItemTile itemTile = tile.GetComponent<ItemTile>();
                tile.GetComponent<Toggle>().group = group;
                itemTile.Item = item;

                if (itemDetails.Item == null) {
                    itemDetails.Item = item;
                    itemTile.GetComponent<Toggle>().isOn = true;
                }

                // TODO: will this produce a memory leak?
                itemTile.ItemSelected += (Item i) => {
                    details.GetComponent<ItemDetails>().Item = i;
                };
            }
        }

        void ClearGrids(Transform container) {
            for (int i = container.childCount - 2; i >= 1; i--) {
                ClearGrids(container.GetChild(i));
                Destroy(container.GetChild(i).gameObject);
            }
        }
        
    }
}