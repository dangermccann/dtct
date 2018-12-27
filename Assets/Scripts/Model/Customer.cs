using System;
using System.Collections.Generic;
using System.Linq;
using DCTC.Map;
using UnityEngine;

namespace DCTC.Model {

    [Serializable]
    public class Customer {
        public string ID { get; set; }
        public string Name { get; set; }
        public TilePosition HomeLocation { get; set; }
        public float IncomeLevel { get; set; }
        public float Patience { get; set; }
        public float Frustration { get; set; }

        // ID of Company that provides service to this customer
        public string ProviderID { get; set; }

        public ServiceTier ServiceTier { get; set; }

        private int invocationsSinceProviderChange = 0;


        [NonSerialized]
        public Game Game;

        [NonSerialized]
        private float lastUpdateTime = 0f;

        public Building Home {
            get {
                return Game.Map.Tiles[HomeLocation].Building;
            }
        }

        public string Address {
            get {
                return Game.Map.Tiles[HomeLocation].Lot.Address;
            }
        }

        public float Wealth {
            get { return IncomeLevel * Home.SquareMeters; }
        }

        public Company Provider {
            get {
                if (ProviderID == null)
                    return null;
                return Game.GetCompany(ProviderID);
            }
        }

        public void Update(float time) {
            float deltaTime = time - lastUpdateTime;

            List<Company> candidates = FindServiceProviders();

            if (ProviderID == null) {
                if (candidates.Count > 0 && WillChangeProvider()) {
                    ChangeProvider(ChooseProvider(candidates));
                }
            } else {
                // Determine if company no longer provides service
                // TODO: cancel service if provider no longer offers selected service tier
                Company provider = candidates.Find(c => c.ID == ProviderID);
                if (provider == null) {
                    ChangeProvider(null);
                } else {
                    // Determine if customer wishes to switch to a different provider or cancel service
                    candidates.Remove(provider);
                    if (WillChangeProvider()) {
                        ChangeProvider(ChooseProvider(candidates));
                    }
                }


            }

            lastUpdateTime = time;
            invocationsSinceProviderChange++;
        }

        public List<Company> FindServiceProviders() {
            List<Company> candidates = new List<Company>();
            foreach (Company company in Game.Companies) {
                if (company.ServiceArea.Contains(HomeLocation)) {
                    candidates.Add(company);
                }
            }
            return candidates;
        }

        private bool WillChangeProvider() {
            // Look for a company to add service
            float timeInerta = (ProviderID == null) ? 50 : 500;
            float wealthChance = RandomUtils.LinearLikelihood(100f, 1000f, Wealth);
            float timeChance = RandomUtils.LinearLikelihood(0, timeInerta, invocationsSinceProviderChange);
            float chance = Mathf.Clamp01(wealthChance * timeChance * Patience * (Frustration + 1));
            return RandomUtils.Chance(Game.Random, chance);
        }

        private Company ChooseProvider(List<Company> companies) {
            if (companies.Count == 0)
                return null;

            // TODO: use attributes of the companies to weigh them
            return RandomUtils.RandomThing(companies, Game.Random);
        }

        private ServiceTier ChooseServiceTier(Company provider) {
            Dictionary<ServiceTier, float> services = provider.FindServicesForLocation(HomeLocation);
            float maxScore = float.MinValue;
            ServiceTier bestTier = services.Keys.First();
            foreach (ServiceTier tier in services.Keys) {
                float score = ServiceTierScore(services[tier]);
                if (score > maxScore) {
                    maxScore = score;
                    bestTier = tier;
                }
            }
            return bestTier;
        }

        private float ServiceTierScore(float price) {
            // Normalize price to range from 0 to pi 
            float maxPrice = 10f;
            float normalizedPrice = Mathf.Min(price, maxPrice) / maxPrice * Mathf.PI;

            // Normalize price to range from 0 to 2*pi/3
            float maxWealth = 900f;
            float minWealth = 200f;
            float normalizedWealth = Mathf.Min(Mathf.Max(0, Wealth - minWealth), maxWealth) / (maxWealth - minWealth);
            normalizedWealth *= (2.0f * Mathf.PI / 3.0f);

            return Mathf.Cos(normalizedPrice - normalizedWealth);

        }

        private void ChangeProvider(Company company) {
            ProviderID = (company == null) ? null : company.ID;
            Frustration = 0;
            invocationsSinceProviderChange = 0;
            if(company != null)
                ServiceTier = ChooseServiceTier(company);

            Game.OnCustomerChanged(this);
        }
    }
}