using System;
using System.Linq;
using System.Collections.Generic;
using DCTC.Model;
using DCTC.Map;

namespace DCTC.AI {

    [Serializable]
    public class BoilerplateAgent : IAgent {
        [NonSerialized]
        private Company company;
        public Company Company {
            get { return company; }
            set {
                company = value;
            }
        }

        public BoilerplateAgent() { }
        public BoilerplateAgent(Company c) {
            Company = c;
        }

        public bool Execute(float deltaTime) {
            return true;

        }

        public float Score(float deltaTime) {
            return 0;
        }
    }
}