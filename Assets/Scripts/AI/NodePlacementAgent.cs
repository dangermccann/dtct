using System;
using System.Linq;
using System.Collections.Generic;
using DCTC.Model;
using DCTC.Map;

namespace DCTC.AI {

    [Serializable]
    public class NodePlacementAgent : IAgent {
        private int cooldown = 0;


        [NonSerialized]
        private Company company;
        public Company Company {
            get { return company; }
            set {
                company = value;
            }
        }

        public NodePlacementAgent() { }
        public NodePlacementAgent(Company c) {
            Company = c;
        }

        public bool Execute(float deltaTime) {

            // Prioritize networks that have no nodes
            foreach (Network network in company.Networks) {
                if (network.Nodes.Count == 0 && network.Positions.Count > 5) {
                    TilePosition pos = RandomUtils.RandomThing(network.Positions, company.Game.Random);
                    string nodeId = ChooseNode(network.CableType);

                    if (company.Money >= company.Game.Items[nodeId].Cost) {
                        company.PlaceNode(nodeId, pos);
                        return true;
                    } else
                        return true;
                } 
            }

            // If all networks have a node, then randomly choose a cable position that is out of range
            //...
            int tries = 10;
            while(tries > 0) {
                Network network = RandomUtils.RandomThing(company.Networks, company.Game.Random);
                TilePosition pos = RandomUtils.RandomThing(network.Positions, company.Game.Random);
                if (!network.IsDistanceWithin(pos, network.MaximumCableDistanceFromNode())) {
                    string nodeId = ChooseNode(network.CableType);
                    if (company.Money >= company.Game.Items[nodeId].Cost) {
                        company.PlaceNode(nodeId, pos);
                        break;
                    }
                    else {
                        break;
                    }
                }
                tries--;
            }

            return true;
        }

        private string ChooseNode(CableType type) {
            switch(type) {
                case CableType.Copper:
                    return Node.DR100;
                case CableType.Coaxial:
                    return Node.CR100;
                case CableType.Optical:
                    return Node.OR105;
            }
            return "";
        }

        public float Score(float deltaTime) {
            cooldown--;
            if (cooldown > 0)
                return 0;

            cooldown = company.Game.Random.Next(Executor.MinimumCooldown, Executor.StandardCooldown);


            int count = 0;
            const float multiplier = 0.25f;

            foreach (CableType type in Company.PotentialServiceArea.Keys) {
                foreach(TilePosition pos in Company.PotentialServiceArea[type]) {
                    if(!Company.ServiceArea.Contains(pos)) {
                        count++;
                    }
                }
            }

            return multiplier * count;
        }
    }
}