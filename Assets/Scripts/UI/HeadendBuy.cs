using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.UI {
    public class HeadendBuy : ItemGridContainer {
        public GameObject headendContent, headendItemDetails;
        bool didDraw = false;

        void Start() {
            headendItemDetails.GetComponent<ItemDetails>().ItemBought += OnItemBought;
            Redraw();
        }

        protected override IEnumerator Redraw() {
            yield return null;


            if (!didDraw && gameController.Game != null) {
                ClearGrids(headendContent.transform);

                headendItemDetails.GetComponent<ItemDetails>().Item = null;

                Items items = gameController.Game.Items;

                AddCategory(headendContent.transform, headendItemDetails, "Termination", items.Termination);
                AddCategory(headendContent.transform, headendItemDetails, "Backhaul", items.Backhaul);

                Dictionary<string, Item> rackItems = new Dictionary<string, Item>();
                rackItems.AddMany(items.Fan);
                rackItems.AddMany(items.Rack);
                AddCategory(headendContent.transform, headendItemDetails, "Racks", rackItems);

                didDraw = true;
            }
        }


    }
}