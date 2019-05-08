using System;
using System.Linq;
using System.Collections.Generic;
using DCTC.Model;
using DCTC.Map;

namespace DCTC.AI {

    [Serializable]
    public class PersonnelAgent : IAgent {
        enum Actions {
            None,
            TruckHire,
            TruckFire,
            CallCenterHire,
            CallCenterFire
        }

        private int cooldown = 0;
        private Actions PendingAction = Actions.None;

        [NonSerialized]
        private Company company;
        public Company Company {
            get { return company; }
            set {
                company = value;
            }
        }

        public PersonnelAgent() { }
        public PersonnelAgent(Company c) {
            Company = c;
        }

        public bool Execute(float deltaTime) {
            switch(PendingAction) {
                case Actions.TruckHire:
                    company.HireTruck(RandomUtils.RandomThing(company.UnhiredTrucks, company.Game.Random));
                    break;
                case Actions.TruckFire:
                    company.FireTruck(RandomUtils.RandomThing(company.Trucks, company.Game.Random));
                    break;
                case Actions.CallCenterHire:
                    company.HireAgent(RandomUtils.RandomThing(company.CallCenter.UnhiredAgents, company.Game.Random));
                    break;
                case Actions.CallCenterFire:
                    company.FireAgent(RandomUtils.RandomThing(company.CallCenter.Agents, company.Game.Random));
                    break;
            }

            PendingAction = Actions.None;
            return true;
        }

        public float Score(float deltaTime) {
            cooldown--;
            if (cooldown > 0)
                return 0;

            cooldown = company.Game.Random.Next(Executor.MinimumCooldown, Executor.FastCooldown);

            // Call Center
            if (company.CallCenter.CallQueue.Count > 10 && company.Money >= Agent.BaseCost * 2 &&
                company.CallCenter.UnhiredAgents.Count > 0) {
                PendingAction = Actions.CallCenterHire;
                return company.CallCenter.CallQueue.Count * 25;
            }

            if (company.CallCenter.CallQueue.Count == 0 && company.CallCenter.Agents.Count > 1) {
                if (RandomUtils.Chance(company.Game.Random, 0.1f * company.CallCenter.Agents.Count)) {
                    PendingAction = Actions.CallCenterFire;
                    return 50;
                }
            }

            // Trucks
            if (company.TruckRollQueue.Count > 5 && company.Money >= Truck.BaseCost * 2 && 
                company.UnhiredTrucks.Count > 0) {
                PendingAction = Actions.TruckHire;
                return company.TruckRollQueue.Count * 25;
            }

            if(company.TruckRollQueue.Count == 0 && company.Trucks.Count > 1) {
                if(RandomUtils.Chance(company.Game.Random, 0.1f * company.Trucks.Count)) {
                    PendingAction = Actions.TruckFire;
                    return 50;
                }
            }


            return 0;
        }
    }
}