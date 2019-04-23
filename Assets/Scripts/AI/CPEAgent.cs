using System;
using System.Linq;
using System.Collections.Generic;
using DCTC.Model;
using DCTC.Map;

namespace DCTC.AI {

    [Serializable]
    public class CPEAgent : IAgent {
        private int cooldown = 0;
        private string pendingCPE = null;

        [NonSerialized]
        private Company company;
        public Company Company {
            get { return company; }
            set {
                company = value;
            }
        }

        public CPEAgent() { }
        public CPEAgent(Company c) {
            Company = c;
        }

        public bool Execute(float deltaTime) {
            if(pendingCPE != null) {
                Item item = company.Game.Items[pendingCPE];
                int quantity = UnityEngine.Mathf.FloorToInt(company.Money / item.Cost);
                quantity = Math.Min(10, quantity);

                if(quantity > 0) {
                    company.Purchase(item, quantity);
                }

                pendingCPE = null;
            }

            return true;
        }

        public float Score(float deltaTime) {
            cooldown--;
            if (cooldown > 0)
                return 0;

            cooldown = company.Game.Random.Next(10, Executor.StandardCooldown);

            // Total up how many devices we have available for each service
            Dictionary<Services, int> cpe = new Dictionary<Services, int>();
            cpe.Add(Services.Phone, company.InventoryCpeByService(Services.Phone).Total());
            cpe.Add(Services.TV, company.InventoryCpeByService(Services.TV).Total());
            cpe.Add(Services.Broadband, company.InventoryCpeByService(Services.Broadband).Total());

            // Look at the the services we're able to offer in each network
            foreach(Network network in company.Networks) {
                foreach(Services service in network.AvailableServices) {

                    if(cpe[service] < 2) {
                        pendingCPE = ChooseCPE(service, network.CableType);
                        if(pendingCPE != null)
                            return 100;
                    }
                }
            }

            return 0;
        }

        public string ChooseCPE(Services service, CableType cableType) {
            List<CPE> candidates = new List<CPE>();

            foreach (CPE cpe in company.Game.Items.CPE.Values) {
                if (company.HasTechnology(cpe.Technology) && 
                    cpe.Services.Contains(service.ToString()) &&
                    cpe.Wiring.Contains(cableType.ToString()))
                    candidates.Add(cpe);
            }

            if (candidates.Count == 0)
                return null;

            return candidates.OrderByDescending(c => c.Services.Count).First().ID;
        }
    }
}