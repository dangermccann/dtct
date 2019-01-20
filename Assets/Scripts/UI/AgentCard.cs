using System;
using UnityEngine;
using DCTC.Model;

namespace DCTC.UI {
    public class AgentCard : UIContainer {

        public Action<Agent> HireFireClicked;

        public GameObject hireButton, fireButton;

        private Agent agent;
        public Agent Agent{
            get { return agent; }
            set {
                agent = value;
                Redraw();
            }
        }

        public bool IsHired {
            set {
                fireButton.SetActive(value);
                hireButton.SetActive(!value);
            }
        }

        void Redraw() {
            if (Agent == null)
                return;

            SetText("AgentName", Agent.Name);
            SetText("AgentAttributes/Speed", Formatter.FormatPercent(Agent.Speed));
            SetText("AgentAttributes/Friendliness", Formatter.FormatPercent(Agent.Friendliness));
            SetText("AgentAttributes/Salary", Formatter.FormatCurrency(Agent.Salary * 100f) + " / day");
        }



        public void OnHireFireClicked() {
            HireFireClicked.Invoke(Agent);
        }
    }
}