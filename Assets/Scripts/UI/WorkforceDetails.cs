using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.UI {
    public class WorkforceDetails : MonoBehaviour {

        public GameObject TruckCardPrefab, AgentCardPrefab;
        private Transform activeTrucksContent, availableTrucksContent;
        private Transform activeAgentsContent, availableAgentsContent;
        private GameController gameController;
        private GameObject trucksTab, agentsTab;

        public void Awake() {
            activeTrucksContent = transform.Find("Contents/TrucksTab/ActiveTrucks/Viewport/Content");
            availableTrucksContent = transform.Find("Contents/TrucksTab/AvailableTrucks/Viewport/Content");
            activeAgentsContent = transform.Find("Contents/AgentsTab/ActiveAgents/Viewport/Content");
            availableAgentsContent = transform.Find("Contents/AgentsTab/AvailableAgents/Viewport/Content");
            trucksTab = transform.Find("Contents/TrucksTab").gameObject;
            agentsTab = transform.Find("Contents/AgentsTab").gameObject;
            gameController = GameController.Get();
        }

        private void Start() {
            trucksTab.SetActive(true);
            agentsTab.SetActive(false);

            transform.Find("Contents/CompanyLogo").GetComponent<Image>().sprite 
                = SpriteController.Get().GetSprite(gameController.Game.Player.Name);
        }

        public void Show() {
            Redraw();
        }

        public void Redraw() { 
            Utilities.DestroyAllChildren(activeTrucksContent);
            Utilities.DestroyAllChildren(availableTrucksContent);
            Utilities.DestroyAllChildren(activeAgentsContent);
            Utilities.DestroyAllChildren(availableAgentsContent);

            Company company = gameController.Game.Player;

            foreach (Truck truck in company.Trucks) {
                GameObject go = Instantiate(TruckCardPrefab, activeTrucksContent);
                TruckCard tc = go.GetComponent<TruckCard>();
                tc.Truck = truck;
                tc.IsHired = true;
                tc.HireFireClicked += (clicked) => {
                    company.FireTruck(clicked);
                    Redraw();
                };
            }

            foreach (Truck truck in company.UnhiredTrucks) {
                GameObject go = Instantiate(TruckCardPrefab, availableTrucksContent);
                TruckCard tc = go.GetComponent<TruckCard>();
                tc.Truck = truck;
                tc.IsHired = false;
                tc.HireFireClicked += (clicked) => {
                    company.HireTruck(clicked);
                    Redraw();
                };
            }

            foreach(Agent agent in company.CallCenter.Agents) {
                GameObject go = Instantiate(AgentCardPrefab, activeAgentsContent);
                AgentCard ac = go.GetComponent<AgentCard>();
                ac.Agent = agent;
                ac.IsHired = true;
                ac.HireFireClicked += (clicked) => {
                    company.FireAgent(clicked);
                    Redraw();
                };
            }

            foreach (Agent agent in company.CallCenter.UnhiredAgents) {
                GameObject go = Instantiate(AgentCardPrefab, availableAgentsContent);
                AgentCard ac = go.GetComponent<AgentCard>();
                ac.Agent = agent;
                ac.IsHired = false;
                ac.HireFireClicked += (clicked) => {
                    company.HireAgent(clicked);
                    Redraw();
                };
            }
        }
    }
}