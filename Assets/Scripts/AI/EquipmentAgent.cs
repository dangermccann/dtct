using System;
using System.Linq;
using System.Collections.Generic;
using DCTC.Model;
using DCTC.Map;

namespace DCTC.AI {

    [Serializable]
    public class EquipmentAgent : IAgent {
        private string pendingPurchaseId = null;
        private int cooldown = 0;

        [NonSerialized]
        private Company company;
        public Company Company {
            get { return company; }
            set {
                company = value;
            }
        }

        public EquipmentAgent() { }
        public EquipmentAgent(Company c) {
            Company = c;
        }

        public bool Execute(float deltaTime) {

            if(pendingPurchaseId != null) {
                // Choose rack
                int rackIdx = -1;
                RackedItem item = company.Game.Items[pendingPurchaseId] as RackedItem;

                for(int i = 0; i < company.Racks.Count; i++) {
                    int available = company.Racks[i].AvailableSlots(company.Game.Items);
                    if(available >= item.RackSpace) {
                        rackIdx = i;
                        break;
                    }
                }

                // Purchase rack if one couldn't be found 
                if(rackIdx == -1) {
                    if(!Errors.Success(company.CanPurchase(Rack.HR15, 1))) {
                        pendingPurchaseId = null;
                        return true;
                    }
                    company.Purchase(Rack.HR15, 1);
                    rackIdx = company.Racks.Count - 1;
                }

                // Attempt purchase 
                if (!Errors.Success(company.CanPurchase(item, 1, rackIdx))) {
                    pendingPurchaseId = null;
                    return true;
                }
                company.Purchase(item, 1, rackIdx);

                pendingPurchaseId = null;
            }
            return true;

        }

        public float Score(float deltaTime) {
            cooldown--;
            if (cooldown > 0)
                return 0;

            cooldown = company.Game.Random.Next(Executor.MinimumCooldown, Executor.StandardCooldown);
            
            float backhaulThroughput = 0;
            Dictionary<Services, float> subscriberCapacity = new Dictionary<Services, float>();
            subscriberCapacity.Add(Services.Phone, 0);
            subscriberCapacity.Add(Services.Broadband, 0);
            subscriberCapacity.Add(Services.TV, 0);

            foreach (string itemID in company.Inventory) {
                Item item = company.Game.Items[itemID];

                // Calcualte backhaul throughput
                if (item is Backhaul) {
                    backhaulThroughput += (item as Backhaul).Throughput;
                }

                // Calcualte capacity of terminators
                if(item is Termination) {
                    Termination term = item as Termination;
                    subscriberCapacity[Utilities.ParseEnum<Services>(term.Service)] += term.Subscribers;
                }
            }

            // Calculate total utilized broadband capacity 
            float broadbandThroughput = 0;
            foreach (Network network in Company.Networks) {
                broadbandThroughput += network.BroadbandThroughput;
            }

            foreach (Network network in Company.Networks) {
                if(!network.Active) {
                    continue;
                }

                // Find out if this network is not offering services that it could be
                List<Services> expectedServices = ExpectedServices(network.CableType);
                foreach(Services service in expectedServices) {

                    string candidateId = null;

                    if (service == Services.Broadband) {
                        if (backhaulThroughput == 0 || backhaulThroughput <= broadbandThroughput) {
                            candidateId = ChooseBackhaul();
                        }
                    }

                    if (candidateId == null) {
                        // Try to termination if we have none for this service, or if we are nearing capacity 
                        if (!network.AvailableServices.Contains(service) ||
                            subscriberCapacity[service] == 0 || 
                            (company.Customers.Count / 2) > subscriberCapacity[service]) {

                            candidateId = ChooseTermination(network.CableType, service);
                        }
                    }

                    if (candidateId != null) {
                        Item item = company.Game.Items[candidateId];
                        if (company.Money > item.Cost) {
                            pendingPurchaseId = candidateId;
                            return 100;
                        }
                    }
                }
            }

            return 0;
        }

        public List<Services> ExpectedServices(CableType type) {
            switch(type) {
                case CableType.Copper:
                    return new List<Services>() { Services.Phone, Services.Broadband };
                case CableType.Coaxial:
                    return new List<Services>() { Services.TV, Services.Broadband };
                case CableType.Optical:
                default:
                    return new List<Services>() { Services.TV, Services.Broadband };
            }
        }

        public string ChooseTermination(CableType cableType, Services service) {
            List<Termination> candidates = new List<Termination>();
            foreach(Item item in company.Game.Items.All()) {
                if(item is Termination) {
                    Termination term = item as Termination;
                    if(term.Wiring == cableType.ToString() && term.Service == service.ToString()) {
                        if(Company.HasTechnology(term.Technology))
                            candidates.Add(term);
                    }
                }
            }

            if (candidates.Count == 0)
                return null;

            return candidates.OrderByDescending(c => c.Subscribers).First().ID;
        }

        public string ChooseBackhaul() {
            List<Backhaul> candidates = new List<Backhaul>();
            foreach (Item item in company.Game.Items.All()) {
                if (item is Backhaul) {
                    Backhaul b = item as Backhaul;
                    if(company.HasTechnology(b.Technology)) {
                        candidates.Add(b);
                    }
                }
            }
            if (candidates.Count == 0)
                return null;

            return candidates.OrderByDescending(d => d.Throughput).First().ID;
        }
    }
}