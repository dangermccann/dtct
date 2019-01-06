using UnityEngine;
using TMPro;
using System.Collections.Generic;
using DCTC.Controllers;
using DCTC.Model;

namespace DCTC.UI {
    public class NewGameUI : MonoBehaviour {

        public static readonly string[] seeds = {
            "confiscate", "stray", "install", "quizzical", "dwell", "lively",
            "thump", "muscle", "premium", "high", "crib", "hurt", "place",
            "fowl", "hall", "cloistered", "concerned", "forsake", "envious",
            "liquid", "fertile"
        };

        public static readonly string[] behaviorDescriptions = {
            "Call centers are 30% more efficient.",
            "Trucks travel 25% faster.",
            "Infrastructure is 15% cheaper.",
            "Research new technologies 15% faster.",
            "Customer retention is 20% higher."
        };

        System.Random random;
        void Start() {
            random = new System.Random();
            GenerateSeed();
            SetBehaviorDescription();
        }

        public void GenerateSeed() {
            string seed = RandomUtils.RandomThing(new List<string>(seeds), random);
            transform.FindChildRecursive("SeedField").GetComponent<TMP_InputField>().text = seed;
        }

        public void Continue() {
            NewGameSettings settings = new NewGameSettings();
            settings.Seed = InputValue("SeedField").GetHashCode();
            settings.NeighborhoodCountX = Dropdown("SizeDropdown").value + 1;
            settings.NeighborhoodCountY = settings.NeighborhoodCountX;
            settings.PlayerName = DropdownValue("CompanyDropdown");

            switch(Dropdown("ModifierDropdown").value) {
                case 0:
                    settings.PlayerAttributes.CallCenterEffectiveness *= 1.3f;
                    break;
                case 2:
                    settings.PlayerAttributes.TruckTravelSpeed *= 1.25f;
                    break;
                case 3:
                    settings.PlayerAttributes.InfrastructureCost *= 0.85f;
                    break;
                case 4:
                    settings.PlayerAttributes.ResearchSpeed *= 1.15f;
                    break;
                case 5:
                    settings.PlayerAttributes.CustomerRetention *= 1.2f;
                    break;
            }

            GameController.Get().New(settings);
        }

        public void Back() {
            StateController.Get().ExitAndPushState(States.GameMenu);
        }

        public void SetBehaviorDescription() {
            int index = Dropdown("ModifierDropdown").value;
            transform.FindChildRecursive("ModifierDescription").GetComponent<TextMeshProUGUI>().text 
                = behaviorDescriptions[index];
        }

        private string DropdownValue(string name) {
            TMP_Dropdown dropdown = Dropdown(name);
            return dropdown.options[dropdown.value].text;
        }

        private TMP_Dropdown Dropdown(string name) {
            return transform.FindChildRecursive(name).GetComponent<TMP_Dropdown>();
        }

        private string InputValue(string name) {
            return transform.FindChildRecursive(name).GetComponent<TMP_InputField>().text;
        }

        
    }

}