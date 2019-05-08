using System;
using System.Collections.Generic;

namespace DCTC.Model {
    public class NameGenerator {

        const string CompaniesSuffix = "Companies";
        const string StreetsSuffix = "Streets";
        const string MinorStreetsSuffix = "MinorStreets";
        const string MajorStreetsSuffix = "MajorStreets";
        const string ModifiersSuffix = "Modifiers";

        Random random;
        List<string> brands, surnames, places, maleNames, femaleNames;
        IDictionary<string, List<string>> suffixes; 

        public NameGenerator(Random random) {
            this.random = random;
            brands = new List<string>(Loader.LoadBrands());
            surnames = new List<string>(Loader.LoadSurnames());
            places = new List<string>(Loader.LoadPlaces());
            maleNames = new List<string>(Loader.LoadMaleNames());
            femaleNames = new List<string>(Loader.LoadFemaleNames());
            suffixes = Loader.LoadSuffixes();
        }

        public void RemoveBrand(string brand) {
            brands.Remove(brand);
        }

        public string CompanyName(bool withSuffix = true) {
            int suffixIdx = random.Next(0, suffixes.Count);
            int brandIdx = random.Next(0, brands.Count);

            if (brands.Count == 0)
                throw new Exception("We've run out of company names!");

            string name = brands[brandIdx];
            brands.RemoveAt(brandIdx);

            string fullName = name;
            if (withSuffix && random.Next(0, 2) == 0) {
                string suffix = RandomSuffix(CompaniesSuffix);
                if (!suffix.StartsWith(","))
                    fullName += " ";
                fullName += suffix;
            }
    
            return fullName;
        }

        public string RandomPlace() {
            int idx = random.Next(0, places.Count);
            string place = places[idx];
            places.RemoveAt(idx);
            return place;
        }

        private string RandomModifier() {
            int modRand = random.Next(0, 10);
            List<string> modifiers = suffixes[ModifiersSuffix];
            if (modRand == 0) {
                return " " + modifiers[random.Next(0, modifiers.Count)];
            }
            return "";
        }

        private string RandomSuffix(string whichSuffix) {
            List<string> suffixList = suffixes[whichSuffix];
            return suffixList[random.Next(0, suffixList.Count)];
        }


        public string RandomSurname() {
            int idx = random.Next(0, surnames.Count);
            return surnames[idx];
        }

        public string RandomMaleName() {
            return maleNames[random.Next(0, maleNames.Count)];
        }

        public string RandomFemaleName() {
            return femaleNames[random.Next(0, femaleNames.Count)];
        }

        public string RandomStreet() {
            return RandomPlace() + " " + RandomSuffix(StreetsSuffix);
        }

        public string RandomMinorStreet() {
            return RandomPlace() + " " + RandomSuffix(MinorStreetsSuffix);
        }

        public string RandomMajorStreet() {
            return RandomPlace() + " " + RandomSuffix(MajorStreetsSuffix);
        }

    }
}
