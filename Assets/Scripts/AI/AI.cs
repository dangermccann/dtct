using System;
using System.Linq;
using System.Collections.Generic;
using DCTC.Model;
using DCTC.Map;

namespace DCTC.AI {

    public interface IAgent {
        float Score(float deltaTime);
        bool Execute(float deltaTime);
        Company Company { get; set; }
    }

    
    [Serializable]
    public class Executor {
        public const int StandardCooldown = 300;
        public const int FastCooldown = 100;
        public const int MinimumCooldown = 25;

        private float lastExecutionTime = 0;
        private int currentIndex = -1;

        public List<IAgent> Agents { get; set; }

        public Executor(List<IAgent> _agents) {
            this.Agents = _agents;
        }

        public Executor() {
            Agents = new List<IAgent>();
        }

        public void Update(float deltaTime) {

            if (currentIndex != -1) {
                lastExecutionTime += deltaTime;
                if (Agents[currentIndex].Execute(deltaTime)) {
                    currentIndex = -1;
                }
            }
            else {
                float score = 0;
                for(int i = 0; i < Agents.Count; i++) {
                    IAgent agent = Agents[i];
                    float s = agent.Score(lastExecutionTime + deltaTime);
                    if(s > score) {
                        score = s;
                        currentIndex = i;
                    }
                }

                // 
                if(score > 0  && currentIndex != -1) {
                    IAgent agent = Agents[currentIndex];
                    UnityEngine.Debug.Log("AI selection: " + agent.Company.Name + " - " + agent.GetType().FullName + " score: " + score);
                }

                lastExecutionTime = 0;
            }
        }
    }
}