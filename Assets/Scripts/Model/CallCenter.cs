using System;
using System.Collections.Generic;

namespace DCTC.Model {

    [Serializable]
    public class CallCenter {
        [NonSerialized]
        public Company Company;

        public List<Agent> Agents = new List<Agent>();
        public List<Agent> UnhiredAgents = new List<Agent>();
        public List<string> CallQueue = new List<string>();

        public void FireAgent(Agent agent) {
            if(agent.CurrentCustomerId != null) {
                CallQueue.Add(agent.CurrentCustomerId);
            }
            Agents.Remove(agent);
            UnhiredAgents.Insert(0, agent);
        }

        public void HireAgent(Agent agent) {
            Agents.Insert(0, agent);
            UnhiredAgents.Remove(agent);
        }

        public void Enqueue(string customerId) {
            CallQueue.Insert(CallQueue.Count, customerId);
        }

        public string Dequeue() {
            if (CallQueue.Count == 0)
                return null;

            string id = CallQueue[0];
            CallQueue.RemoveAt(0);
            return id;
        }

        public void Update(float deltaTime) {
            float cost = 0;

            foreach(Agent agent in Agents) {
                agent.Update(deltaTime);
                cost += agent.Salary * deltaTime;
            }

            Company.Money -= cost;
        }
    }

    [Serializable]
    public class Agent {
        public const float BaseCost = 1f;

        [NonSerialized]
        public Company Company;

        const float MaxCallDuration = 0.4f;

        public float Speed { get; set; }
        public float Friendliness { get; set; }
        public float Performance { get; set; }
        public string CurrentCustomerId { get; set; }
        public float Salary { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }

        private float elapsed = 0;

        public Agent() {
            Salary = 0.1f;
        }

        public void Update(float deltaTime) {
            if (CurrentCustomerId != null) {
                elapsed += deltaTime * Speed;
                if (elapsed >= MaxCallDuration) {
                    ResolveCall();
                }
            }
            else {
                CurrentCustomerId = Company.CallCenter.Dequeue();
            }
        }

        void ResolveCall() {
            Customer customer = Company.GetCustomer(CurrentCustomerId);
            if(customer != null) {
                if (customer.Status == CustomerStatus.Pending) {
                    Company.RollTruck(customer);
                }
                else {
                    // Chance that call agent will offend customer
                    if (RandomUtils.Chance(Company.Game.Random, 
                        (1f - Friendliness) * 0.05f * Company.Attributes.CallCenterFriendliness)) {

                        customer.OffendedByAgent();
                        UnityEngine.Debug.LogWarning("Agent offended " + customer.Name);
                    }

                    // Chance that call agent can resolve the problem
                    if (RandomUtils.Chance(Company.Game.Random, 
                        Performance * Company.Attributes.CallCenterEffectiveness)) {

                        customer.ResolveOutage();
                    }
                    else {
                        Company.RollTruck(customer);
                    }
                }
            }
            else {
                // customer must have canceled
            }
            
            CurrentCustomerId = null;
            elapsed = 0;
        }

        public void TakeCall(string customerId) {
            CurrentCustomerId = customerId;
        }
    }
}