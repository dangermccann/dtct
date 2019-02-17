using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public delegate void ItemBoughtEvent(Item item, int quantity);

    public class ItemDetails : MonoBehaviour {

        TextMeshProUGUI price, description, shortDescription, attributes;
        Button buyButton;
        TMP_InputField quantity;

        public event ItemBoughtEvent ItemBought;

        private TextMeshProUGUI GetText(string name) {
            return transform.Find(name).GetComponent<TextMeshProUGUI>();
        }

        void Init() {
            description = GetText("Description");
            shortDescription = GetText("ShortDescription");
            price = GetText("Price");
            attributes = GetText("Attributes");
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

            if (Item != null) {
                description.text = Item.Description;
                shortDescription.text = Item.ID + " " + Item.ShortDescription;
                price.text = FormatAttribute("Price", Formatter.FormatCurrency(Item.Cost));
                string att = "";

                if (Item is Termination) {
                    Termination termination = Item as Termination;
                    att += FormatAttribute("Service", termination.Service);
                    att += FormatAttribute("Wiring", termination.Wiring);
                    att += FormatAttribute("Capacity", termination.Subscribers + " subscribers");
                    if(termination.Throughput != -1)
                        att += FormatAttribute("Throughput", termination.Throughput + " Gbps");
                    att += FormatRackspace(termination);
                    att += FormatAttribute("Heat generated", termination.Heat + " thermal units");
                } else if(Item is Backhaul) {
                    Backhaul backhaul = Item as Backhaul;
                    if(backhaul.Throughput != -1)
                        att += FormatAttribute("Throughput", backhaul.Throughput + " Gbps");
                    att += FormatRackspace(backhaul);
                } else if(Item is Rack) {
                    Rack rack = Item as Rack;
                    att += FormatAttribute("Slots", rack.Slots.ToString());
                } else if(Item is Fan) {
                    Fan fan = Item as Fan;
                    att += FormatAttribute("Cooling", fan.Cooling + " thermal units");
                    att += FormatRackspace(fan);
                } else if (Item is CPE) {
                    CPE cpe = Item as CPE;
                    att += FormatAttribute("Service", string.Join(", ", cpe.Services.ToArray()));
                    att += FormatAttribute("Wiring", string.Join(", ", cpe.Wiring.ToArray()));
                    if(cpe.Throughput != -1)
                        att += FormatAttribute("Throughput", cpe.Throughput + " Gbps");
                    att += FormatAttribute("Reliability", Formatter.FormatPercent(cpe.Reliability));
                }

                attributes.text = att;

            }
            else {
                description.text = "";
                shortDescription.text = "";
                price.text = "";
                attributes.text = "";
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }

        private string FormatRackspace(RackedItem ri) {
            return FormatAttribute("Rack space required", ri.RackSpace + (ri.RackSpace == 1 ? " row" : " rows")); ;
        }

        private static string FormatAttribute(string key, string value) {
            return "<color=#D1D1D1>" + key + ": <color=#FFFFFF>" + value + "\n";
        }

    }
}