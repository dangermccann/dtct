using System;
using System.Linq;
using System.Collections.Generic;
using DCTC.Model;
using DCTC.Map;

namespace DCTC.AI {

    [Serializable]
    public class TechnologyAgent : IAgent {
        private int cooldown = 0;
        private string next = null;

        [NonSerialized]
        private Company company;
        public Company Company {
            get { return company; }
            set {
                company = value;
            }
        }

        public TechnologyAgent() { }
        public TechnologyAgent(Company c) {
            Company = c;
        }

        public bool Execute(float deltaTime) {
            company.CurrentlyResearching = next;
            next = null;
            return true;
        }

        public float Score(float deltaTime) {
            cooldown--;
            if (cooldown > 0)
                return 0;

            cooldown = company.Game.Random.Next(Executor.MinimumCooldown, Executor.FastCooldown);

            List<Technology> available = Technology.AvailableTechnologies(company.Game.TechnologyGraph, company.Technologies);

            if (company.CurrentlyResearching == null && available.Count > 0) {
                next = RandomUtils.RandomThing(available, company.Game.Random).ID;
                return 100;
            }

            return 0;
        }
    }
}