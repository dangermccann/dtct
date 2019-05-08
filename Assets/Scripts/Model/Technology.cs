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

        public Technology Find(string id) {
            if (ID == id)
                return this;
            foreach(Technology tech in DependantTechnologies) {
                Technology found = tech.Find(id);
                if (found != null)
                    return found;
            }
            return null;
        }

        public List<string> UnlockedItems(Items items) {
            List<string> result = new List<string>();
            foreach (Item item in items.All()) {
                if(item.Technology == ID) {
                    result.Add(item.ID);
                }
            }
            return result;
        }

        public static List<Technology> AvailableTechnologies(Technology graph, Dictionary<string, float> techs) {
            List<Technology> results = new List<Technology>();

            if (techs[graph.ID] < graph.Cost)
                results.Add(graph);
            else {
                foreach (Technology tech in graph.DependantTechnologies) {
                    results.AddRange(AvailableTechnologies(tech, techs));
                }
            }

            return results;
        }

        public static List<Technology> Flatten(Technology graph) {
            List<Technology> result = new List<Technology>();
            result.Add(graph);
            foreach(Technology tech in graph.DependantTechnologies) {
                result.AddRange(Flatten(tech));
            }
            return result;
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