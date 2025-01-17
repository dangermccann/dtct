﻿using System;
using System.Collections.Generic;
using System.Linq;
using DCTC.Map;
using UnityEngine;

namespace DCTC.Model {

    public enum CustomerStatus {
        NoProvider,
        Pending,
        Subscribed,
        Outage
    }

    [Serializable]
    public class Customer {
        private const float baseTurnoverCooldown = 75f;
        private const float ChangeInvocationsInitial = 125;
        private const float ChangeInvocationsPendingInstall = 350;
        private const float ChangeChanceOutage = 0.25f;
        private const float ChangeChanceSubscribed = 0.05f;

        
        public string ID { get; set; }
        public string Name { get; set; }
        public TilePosition HomeLocation { get; set; }
        public float IncomeLevel { get; set; }
        public float Patience { get; set; }
        public float Dissatisfaction { get; set; }
        public CustomerStatus Status { get; set; }
        public Inventory Equipment { get; set; }

        // ID of Company that provides service to this customer
        public string ProviderID { get; set; }

        public List<Services> Services { get; set; }

        private int invocationsSinceProviderChange = 0;
        private float turnoverCooldown = 0;


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

        public Customer() {
            Status = CustomerStatus.NoProvider;
            Equipment = new Inventory();
        }

        public void Update(float time) {
            if (lastUpdateTime == 0)
                lastUpdateTime = time;

            float deltaTime = time - lastUpdateTime;

            UpdateDissatisfaction(deltaTime);

            List<Company> candidates = FindServiceProviders();

            if (Status == CustomerStatus.NoProvider) {
                if(turnoverCooldown > 0 && turnoverCooldown - deltaTime <= 0) {
                    //Debug.Log(Name + " reached end of cooldown period");
                }

                turnoverCooldown -= deltaTime;

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
                    candidates.Remove(provider);

                    // Determine if customer wishes to switch to a different provider or cancel service
                    if (WillChangeProvider()) {
                        ChangeProvider(ChooseProvider(candidates));
                    }
                    else {
                        // Small chance of outage
                        if (Status == CustomerStatus.Subscribed) {
                            if (RandomUtils.Chance(Game.Random, 0.002)) {
                                EnterOutage();
                            }
                        }
                    }
                }


            }

            lastUpdateTime = time;
            invocationsSinceProviderChange++;
        }

        public List<Company> FindServiceProviders() {
            List<Company> candidates = new List<Company>();
            foreach (Company company in Game.Companies) {
                if (company.ServiceArea.Contains(HomeLocation) && 
                    company.FindServicesForLocation(HomeLocation).Count > 0) {
                    candidates.Add(company);
                }
            }
            return candidates;
        }

        public void ServiceTruckArrived() {
            if (Status == CustomerStatus.Outage || Status == CustomerStatus.Pending)
                Status = CustomerStatus.Subscribed;
            Game.OnCustomerChanged(this);
        }

        public void ResolveOutage() {
            Status = CustomerStatus.Subscribed;
            Game.OnCustomerChanged(this);
        }

        public void OffendedByAgent() {
            Dissatisfaction += Patience * 0.3f;
        }

        private void UpdateDissatisfaction(float deltaTime) {
            // TODO: consider price increases 
            // TODO: remember what factors impact dissatisfaction the most per customer

            // Adjust Dissatisfaction due to wait times 
            if (Status == CustomerStatus.Outage)
                Dissatisfaction += Patience * deltaTime * 0.15f;
            else if (Status == CustomerStatus.Pending)
                Dissatisfaction += Patience * deltaTime * 0.02f;
            else if (Status == CustomerStatus.Subscribed)
                Dissatisfaction -= deltaTime * 0.005f * Provider.Attributes.ServiceSatisfaction;
            else
                Dissatisfaction -= deltaTime * 0.002f;

            Dissatisfaction = Mathf.Clamp01(Dissatisfaction);
        }

        private void EnterOutage() {
            Status = CustomerStatus.Outage;
            Dissatisfaction += Patience * 0.25f;
            Game.OnCustomerChanged(this);
        }

        private bool WillChangeProvider() {
            float churnChance = 0;
            float timeChance = 1;
            switch (Status) {
                case CustomerStatus.NoProvider:
                    churnChance = RandomUtils.LinearLikelihood(100f, 1000f, Wealth);
                    churnChance *= (1.0f - Mathf.Min(1f, Dissatisfaction * 5f));

                    if (turnoverCooldown > 0)
                        timeChance = 0;
                    else
                        timeChance = RandomUtils.LinearLikelihood(0, ChangeInvocationsInitial, invocationsSinceProviderChange);
                    break;

                case CustomerStatus.Subscribed:
                    churnChance = Mathf.Max(0.1f, Dissatisfaction) * Patience / Provider.Attributes.CustomerRetention;
                    timeChance = ChangeChanceSubscribed;
                    break;

                case CustomerStatus.Outage:
                    churnChance = Mathf.Max(0.1f, Dissatisfaction) * Patience;
                    timeChance = ChangeChanceOutage;
                    break;

                case CustomerStatus.Pending:
                    churnChance = Mathf.Max(0.1f, Dissatisfaction) * Patience;
                    timeChance = RandomUtils.LinearLikelihood(0, ChangeInvocationsPendingInstall, invocationsSinceProviderChange);
                    break;
            }

            float chance = Mathf.Clamp01(churnChance * timeChance);
            return RandomUtils.Chance(Game.Random, chance);
        }

        private Company ChooseProvider(List<Company> companies) {
            if (companies.Count == 0)
                return null;

            // TODO: use attributes of the companies to weigh them
            return RandomUtils.RandomThing(companies, Game.Random);
        }

        private List<Services> ChooseServices(Company provider) {
            Dictionary<Services, float> services = provider.FindServicesForLocation(HomeLocation);

            if(services.Count == 0) {
                return new List<Services>();
            }

            List<Services> result = new List<Services>();
            foreach(Services service in services.Keys) {
                // TODO: adjust threshold
                if(ServiceScore(services[service]) > 0) {
                    result.Add(service);
                }
            }
            return result;
        }

        private float ServiceScore(float price) {
            // Normalize price to range from 0 to pi 
            float maxPrice = 20f;
            float normalizedPrice = Mathf.Min(price, maxPrice) / maxPrice * Mathf.PI;

            // Normalize price to range from 0 to 2*pi/3
            float maxWealth = 900f;
            float minWealth = 200f;
            float normalizedWealth = Mathf.Min(Mathf.Max(0, Wealth - minWealth), maxWealth) / (maxWealth - minWealth);
            normalizedWealth *= (2.0f * Mathf.PI / 3.0f);

            return Mathf.Cos(Math.Max(0, normalizedPrice - normalizedWealth));

        }

        private void ChangeProvider(Company company) {
               
            if(company != null) {
                Dissatisfaction /= 2.0f;
                ProviderID = company.ID;
                Status = CustomerStatus.Pending;
                Services = ChooseServices(company);
            }
            else {
                // Return equipment
                if(Provider != null) {
                    Provider.Inventory.Add(Equipment);
                    Equipment.Clear();
                }

                ProviderID = null;
                Status = CustomerStatus.NoProvider;
                turnoverCooldown = baseTurnoverCooldown * RandomUtils.RandomFloat(0.25f, 1.0f, Game.Random);

                //Debug.Log(Name + " turned over (" + UI.Formatter.FormatDissatisfaction(Dissatisfaction) + ")");
            }

            invocationsSinceProviderChange = 0;
            Game.OnCustomerChanged(this);
        }
    }
}