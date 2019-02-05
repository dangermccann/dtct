using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.UI {
    public class InventoryDetails : MonoBehaviour {
        public GameObject ItemTilePrefab, ItemGridPrefab, HeaderPrefab;
        private Transform fieldContent, headendContent;
        private GameController gameController;

        void OnEnable() {
            fieldContent = transform.Find("Field/ItemList/Viewport/Content");
            headendContent = transform.Find("Headend/ItemList/Viewport/Content");
            gameController = GameController.Get();
            Redraw();
        }

        void Redraw() {
            if (gameController.Game == null)
                return;

            Utilities.Clear(fieldContent);

            Items items = gameController.Game.Items;
            AddCategory(headendContent, "Termination", items.Termination);
            AddCategory(headendContent, "Backhaul", items.Backhaul);
            AddCategory(headendContent, "Fans", items.Fan);
            AddCategory(headendContent, "Racks", items.Rack);
            AddCategory(fieldContent, "Devices", items.CPE);
        }

        void AddCategory<T>(Transform _parent, string name, Dictionary<string, T> categoryItems) where T : Item {
            GameObject header = Instantiate(HeaderPrefab, _parent);
            header.GetComponent<TMPro.TextMeshProUGUI>().text = name;

            GameObject grid = Instantiate(ItemGridPrefab, _parent);

            foreach (string id in categoryItems.Keys) {
                T item = categoryItems[id];
                GameObject tile = Instantiate(ItemTilePrefab, grid.transform);
                tile.GetComponent<ItemTile>().Item = item;
            }
        }
        
    }
}