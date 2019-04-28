using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DCTC.Model;

namespace DCTC.UI {
    public class CompanyAttributes : MonoBehaviour {

        private AttributeTable attributes, inventory;
        private CompanyLogo logo;

        private Company company;
        public Company Company {
            get { return company; }
            set {
                company = value;
                Redraw();
            }
        }

        private void Awake() {
            attributes = transform.Find("AttributeTable").GetComponent<AttributeTable>();
            inventory = transform.Find("Inventory").GetComponent<AttributeTable>();
            logo = transform.Find("Logo").GetComponent<CompanyLogo>();
        }

        public void Redraw() {
            logo.Company = Company;
            attributes.Clear();
            attributes.Append("Money", Formatter.FormatCurrency(company.Money));
            attributes.Append("Customers", company.ActiveCustomers.Count().ToString());
            attributes.Append("Satisfaction", Formatter.FormatPercent(company.Satisfaction));
            attributes.Append("Call Queue", company.CallCenter.CallQueue.Count.ToString());
            attributes.Append("Truck Queue", company.TruckRollQueue.Count.ToString());
            if(company.CurrentlyResearching != null) {
                attributes.Append(company.CurrentlyResearching, "");
            }

            inventory.Clear();
            foreach(string id in Company.Inventory) {
                inventory.Append(id, Company.Inventory[id].ToString());
            }
        }
    }
}
