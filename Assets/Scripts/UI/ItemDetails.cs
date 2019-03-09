using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public delegate void ItemBoughtEvent(Item item, int quantity);

    public class ItemDetails : MonoBehaviour {

        TextMeshProUGUI description, shortDescription;
        AttributeTable attributes;
        Button buyButton;
        TMP_InputField quantity;

        public event ItemBoughtEvent ItemBought;

        private TextMeshProUGUI GetText(string name) {
            return transform.Find(name).GetComponent<TextMeshProUGUI>();
        }

        void Init() {
            shortDescription = GetText("ShortDescription");
            description = GetText("Description");
            attributes = transform.Find("AttributeTable").GetComponent<AttributeTable>();
            buyButton = transform.Find("ButtonRow/Button").GetComponent<Button>();
            quantity = transform.Find("ButtonRow/QuantityInput").GetComponent<TMP_InputField>();

            buyButton.onClick.AddListener(() => {
                if (ItemBought != null)
                    ItemBought(Item, int.Parse(quantity.text));
            });
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
            if (description == null)
                Init();

            attributes.Clear();

            if (Item != null) {
                description.text = Item.Description;
                shortDescription.text = Item.ID + " " + Item.ShortDescription;
                FormatAttribute("Price", Formatter.FormatCurrency(Item.Cost));

                List<KeyValuePair<string, string>> values = ItemAttributesGenerator.GenerateAttributes(item);
                foreach (var pair in values) {
                    FormatAttribute(pair.Key, pair.Value);
                }

            } else {
                description.text = "";
                shortDescription.text = "";
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }

        private void FormatAttribute(string key, string value) {
            attributes.Append("<color=#D1D1D1>" + key, "<color=#FFFFFF>" + value);
        }

    }

    public static class ItemAttributesGenerator {
        public static List<KeyValuePair<string, string>> GenerateAttributes(Item item) {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            if (item is Termination) {
                Termination termination = item as Termination;
                AddAttribute("Service", termination.Service, result);
                AddAttribute("Wiring", termination.Wiring, result);
                AddAttribute("Capacity", termination.Subscribers + " subscribers", result);
                if (termination.Throughput != -1)
                    AddAttribute("Throughput", termination.Throughput + " Gbps", result);
                AddRackspace(termination, result);
                AddAttribute("Heat generated", termination.Heat + " thermal units", result);
            } else if (item is Backhaul) {
                Backhaul backhaul = item as Backhaul;
                if (backhaul.Throughput != -1)
                    AddAttribute("Throughput", backhaul.Throughput + " Gbps", result);
                AddRackspace(backhaul, result);
            } else if (item is Rack) {
                Rack rack = item as Rack;
                AddAttribute("Slots", rack.Slots.ToString(), result);
            } else if (item is Fan) {
                Fan fan = item as Fan;
                AddAttribute("Cooling", fan.Cooling + " thermal units", result);
                AddRackspace(fan, result);
            } else if (item is CPE) {
                CPE cpe = item as CPE;
                AddAttribute("Service", string.Join(", ", cpe.Services.ToArray()), result);
                AddAttribute("Wiring", string.Join(", ", cpe.Wiring.ToArray()), result);
                if (cpe.Throughput != -1)
                    AddAttribute("Throughput", cpe.Throughput + " Gbps", result);
                AddAttribute("Reliability", Formatter.FormatPercent(cpe.Reliability), result);
            }

            return result;
        }

        private static void AddRackspace(RackedItem ri, List<KeyValuePair<string, string>> result) {
            AddAttribute("Rack space", ri.RackSpace + (ri.RackSpace == 1 ? " row" : " rows"), result);
        }

        private static void AddAttribute(string key, string value, List<KeyValuePair<string, string>> result) {
            result.Add(new KeyValuePair<string, string>(key, value));
        }
    }
}