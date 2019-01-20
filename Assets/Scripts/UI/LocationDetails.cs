using System.Linq;
using UnityEngine;
using TMPro;
using DCTC.Model;
using DCTC.Map;
using DCTC.Controllers;


namespace DCTC.UI {
    public class LocationDetails : MonoBehaviour {

        public GameObject DetailsPrefab;

        private bool initialized = false;
        private int coolDown = 0;
        private GameController gameController;

        private const string prefix = "Details-";

        private Lot lot = null;
        private Customer customer = null;
        private Tile tile = null;

        private TilePosition location = TilePosition.Origin;
        public TilePosition Location {
            get { return location; }
            set {
                if (value.Equals(location))
                    return;

                location = value;
                if (gameController.Map.IsInBounds(location)) {
                    tile = gameController.Map.Tiles[location];
                    lot = tile.Lot;
                    if (lot != null)
                        customer = gameController.Game.FindCustomerByAddress(lot.Anchor);
                    else
                        customer = null;
                } else {
                    lot = null;
                    tile = null;
                    customer = null;
                }

                Redraw();
            }
        }


        void Start() {
            gameController = GameController.Get();
        }

        void Init() {
            initialized = true;
            GameController.Get().Game.CustomerChanged += Game_CustomerChanged;
        }

        void Update() {
            if (!initialized || lot == null)
                return;

            if(++coolDown > 60) {
                coolDown = 0;
                Redraw();
            }
        }

        private void Game_CustomerChanged(Customer customer, Company company) {
            if(this.customer != null && customer.ID == this.customer.ID) {
                Redraw();
            }
        }


        void Clear() {
            Utilities.ClearRemaining(prefix, transform, 1);
        }

        void Redraw() {
            if (initialized == false) {
                Init();
            }

            Clear();

            if (gameController.Map.IsInBounds(location)) {
                this.gameObject.SetActive(true);
            } else {
                this.gameObject.SetActive(false);
                return;
            }

            if (customer != null) {
                DrawCustomer(customer);
            } else if (lot != null) {
                if (lot.Building.Type == BuildingType.Headquarters) {
                    DrawHeadquarters();
                } else {
                    DrawInactiveBuilding();
                }
            }
            else if(tile != null) {
                if(tile.Type == TileType.Road) {
                    Street street = gameController.Map.FindStreet(location);
                    if(street != null) {
                        SetText("Name", street.Name);
                    }
                    else {
                        SetText("Name", "Unknown Road");
                    }


                }
                else {
                    SetText("Name", tile.Type.ToString());
                }
            }
            else {
                this.gameObject.SetActive(false);
            }            
        }

        void DrawHeadquarters() {
            foreach(Company company in gameController.Game.Companies) {
                if(company.HeadquartersLocation == lot.Anchor) {
                    SetText("Name", company.Name + " Headquarters");
                    Append(company.ActiveCustomers.Count() + " Customers");
                    Append(Formatter.FormatPercent(company.Satisfaction) + " Satisfaction");
                }
            }
        }

        void AppendNode(Node node) {

        }

        void AppendCable(Cable cable) {

        }

        void DrawInactiveBuilding() {
            SetText("Name", lot.Building.Type.ToString());
            Append(lot.Address);
            Append("[No Service Provider]");
        }

        void DrawCustomer(Customer customer) {
            SetText("Name", customer.Name + " Household");
            Append(customer.Address);

            Company provider = customer.Provider;
            Append(provider == null ? "[No Service Provider]" : provider.Name);
            if (provider != null) {
                Append(customer.ServiceTier.ToString());
            }

            Append(Formatter.FormatDissatisfaction(customer.Dissatisfaction));
            Append("Income: " + Formatter.FormatCurrency(customer.Wealth * 10));
            Append("Demeanor: " + Formatter.FormatPatience(customer.Patience));
        }


        void SetText(string name, string value) {
            if (value == null)
                value = "-";
            transform.Find(name).gameObject.GetComponent<TextMeshProUGUI>().text = value;
        }

        void Append(string str) {
            GameObject go = Instantiate(DetailsPrefab, transform);
            go.name = prefix + (transform.childCount - 1);
            go.GetComponent<TextMeshProUGUI>().text = str;
        }
    }
}