using System;
using System.Collections.Generic;

namespace DCTC.Model {
    [Serializable]
    public class Technology {
        public string ID { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public float Cost { get; set; }
        public string Prerequisite { get; set; }

        [NonSerialized]
        public Technology PrerequisiteTechnology;

        [NonSerialized]
        public List<Technology> DependantTechnologies = new List<Technology>();


        public static Technology BuildGraph(Dictionary<string, Technology> techs) {
            Technology first = null;
            foreach(Technology tech in techs.Values) {
                if(tech.Prerequisite == null) {
                    first = tech;
                }
                else {
                    tech.PrerequisiteTechnology = techs[tech.Prerequisite];
                }
                AssignDependencies(tech, techs);
            }

            return first;
        }

        private static void AssignDependencies(Technology tech, Dictionary<string, Technology> techs) {
            foreach(Technology candidate in techs.Values) {
                if(candidate.Prerequisite == tech.ID) {
                    tech.DependantTechnologies.Add(candidate);
                }
            }
        }
    }
}