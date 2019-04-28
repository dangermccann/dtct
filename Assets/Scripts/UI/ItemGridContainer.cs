using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.UI {
    public delegate void ItemEvent(Item item);

    public abstract class ItemGridContainer : MonoBehaviour {
        public GameObject ItemTilePrefab, ItemGridPrefab;
        public GameObject ResultText;
        protected GameController gameController;
        private Coroutine hideResult;

        protected void OnItemBought(Item i, int qty) {
            UIStateInvocation current = StateController.Get().Current;
            Company player = gameController.Game.Player;
            int err = Errors.OK;

            if (i is RackedItem) {
                int rack = int.Parse(current.Parameters["rack"]);
                RackedItem racked = i as RackedItem;
                err = player.CanPurchase(racked, qty, rack);
                if (Errors.Success(err)) {
                    player.Purchase(racked, qty, rack);
                    ShowSuccess(racked, qty);
                } else {
                    ShowError(err);
                }
            } else {
                err = player.CanPurchase(i, qty);
                if (Errors.Success(err)) {
                    player.Purchase(i, qty);
                    ShowSuccess(i, qty);
                } else {
                    ShowError(err);
                }
            }
        }

        protected void ShowError(int err) {
            if (hideResult != null)
                StopCoroutine(hideResult);

            Company player = gameController.Game.Player;

            string str = "<color=#D6272E>";
            switch (err) {
                case Errors.INSUFFICIENT_MONEY:
                    str += "Insufficient funds";
                    break;
                case Errors.INSUFFICIENT_RACKSPACE:
                    str += "Insufficient rack space";
                    break;
                case Errors.INSUFFICIENT_INVENTORY:
                    str += "Insufficient inventory space (maximum " + 
                        Formatter.FormatInteger(player.InventoryLimit) + " items)";
                    break;
                case Errors.MAXIMUM_RACKS:
                    str += "Reached maxumim of " + player.RackLimit + " racks";
                    break;
            }

            SetResultText(str);

            hideResult = StartCoroutine(DelayedHideResult());
        }

        protected void ShowSuccess(Item i, int qty) {
            if (hideResult != null)
                StopCoroutine(hideResult);

            string str = "<color=#27D639>";
            str += "Purchased " + i.ID + " (x" + Formatter.FormatInteger(qty) + ")";
            SetResultText(str);

            hideResult = StartCoroutine(DelayedHideResult());
        }

        protected IEnumerator DelayedHideResult() {
            yield return new WaitForSeconds(2);
            SetResultText("");
            hideResult = null;
        }

        protected void SetResultText(string str) {
            ResultText.GetComponent<TMPro.TextMeshProUGUI>().text = str;
        }
 
        void OnEnable() {
            gameController = GameController.Get();
            SetResultText("");
            StartCoroutine(Redraw());
        }

        protected abstract IEnumerator Redraw();

        protected void AddCategory<T>(Transform _parent, GameObject details,
            string name, Dictionary<string, T> categoryItems) where T : Item {

            GameObject grid = Instantiate(ItemGridPrefab, _parent);
            grid.transform.SetSiblingIndex(_parent.childCount - 2);
            Utilities.Clear(grid.transform);
            ToggleGroup group = _parent.GetComponent<ToggleGroup>();
            ItemDetails itemDetails = details.GetComponent<ItemDetails>();

            List<string> orderedKeys = new List<string>(categoryItems.Keys);
            orderedKeys.Sort();

            Company player = gameController.Game.Player;

            foreach (string id in orderedKeys) {
                T item = categoryItems[id];

                if (!player.HasTechnology(item.Technology))
                    continue;

                GameObject tile = Instantiate(ItemTilePrefab, grid.transform);
                tile.name = "Tile " + item.ID;

                ItemTile itemTile = tile.GetComponent<ItemTile>();
                tile.GetComponent<Toggle>().group = group;
                itemTile.Inventory = gameController.Game.Player.Inventory;
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

        protected void ClearGrids(Transform container) {
            for (int i = container.childCount - 2; i >= 1; i--) {
                ClearGrids(container.GetChild(i));
                Destroy(container.GetChild(i).gameObject);
            }
        }
    }
}