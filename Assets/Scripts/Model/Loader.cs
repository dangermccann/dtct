using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using DCTC.Map;


namespace DCTC.Model {
    public class Loader {

        private const string Package = "Config/";
        private const string BrandsYaml = Package + "brands";
        private const string SurnamesYaml = Package + "surnames";
        private const string NamesYaml = Package + "names";
        private const string PlacesYaml = Package + "places";
        private const string Suffixes = Package + "suffixes";
        private const string MapYaml = Package + "map";
        private const string ItemsYaml = Package + "items";
        private const string TechnologiesYaml = Package + "technologies";

        public static IList<string> LoadBrands() {
            Dictionary<string, List<string>> parsed;
            Parse(BrandsYaml, out parsed);
            return parsed["Brands"];
        }

        public static IList<string> LoadSurnames() {
            Dictionary<string, List<string>> parsed;
            Parse(SurnamesYaml, out parsed);
            return parsed["Surnames"];
        }

        public static IList<string> LoadMaleNames() {
            Dictionary<string, List<string>> parsed;
            Parse(NamesYaml, out parsed);
            return parsed["Male"];
        }

        public static IList<string> LoadFemaleNames() {
            Dictionary<string, List<string>> parsed;
            Parse(NamesYaml, out parsed);
            return parsed["Female"];
        }

        public static IList<string> LoadPlaces() {
            Dictionary<string, List<string>> parsed;
            Parse(PlacesYaml, out parsed);
            return parsed["Places"];
        }

        public static IDictionary<string, List<string>> LoadSuffixes() {
            Dictionary<string, List<string>> parsed;
            Parse(Suffixes, out parsed);
            return parsed;
        }

        public static MapTemplate LoadMapTemplate()
        {
            MapTemplate template = new MapTemplate();
            Parse(MapYaml, out template);
            return template;
        }

        public static Items LoadItems() {
            Items items = new Items();
            Parse(ItemsYaml, out items);
            items.AssignIDs();
            return items;
        }

        public static Dictionary<string, Technology> LoadTechnologies() {
            Dictionary<string, Technology> all = new Dictionary<string, Technology>();
            Parse(TechnologiesYaml, out all);
            foreach(string id in all.Keys) {
                all[id].ID = id;
            } 
            return all;
        }

        private static void Parse<T>(string resource, out T result) {
            TextAsset yaml = Resources.Load(resource) as TextAsset;
            result = CreateDeserializer().Deserialize<T>(yaml.text);
        }

        private static IDeserializer CreateDeserializer() {
            return new DeserializerBuilder().WithNamingConvention(new PascalCaseNamingConvention()).Build();
        }

    }
}
