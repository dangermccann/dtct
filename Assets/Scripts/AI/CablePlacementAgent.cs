﻿using System;
using System.Linq;
using System.Collections.Generic;
using DCTC.Model;
using DCTC.Map;

namespace DCTC.AI {

    [Serializable]
    public class CablePlacementAgent : IAgent {

        [Serializable]
        private class Evaluation {
            public float bestScore = float.MinValue;
            public IEnumerable<TilePosition> startPositions;
            public IList<TilePosition> targets;
            public string cableId;
            public float costEstimate;
        }


        [NonSerialized]
        private Company company;
        public Company Company {
            get { return company; }
            set {
                company = value;
                map = company.Game.Map;
            }
        }

        [NonSerialized]
        private Dictionary<TilePosition, Customer> customers;

        [NonSerialized]
        private MapConfiguration map;

        private int executionCoolDown = 0;
        private int cableCoolDown = 0;
        private int currentCableIndex = -1;
        private Evaluation currentEval;

        private const int expansionDistance = 18;

        public CablePlacementAgent() { }
        public CablePlacementAgent(Company c) {
            Company = c;
        }

        private void  Init() {
            if (company.Game.Customers != null)
                customers = company.Game.Customers.ToDictionary(c1 => c1.HomeLocation);
            else
                customers = new Dictionary<TilePosition, Customer>();

            // Randomize the building times of AIs
            cableCoolDown = company.Game.Random.Next(Executor.MinimumCooldown, Executor.FastCooldown);
        }

        public bool Execute(float deltaTime) {
            if (customers == null)
                Init();

            if (currentCableIndex == -1) {
                currentEval = Evaluate();
                currentCableIndex = 0;
                return false;
            }

            if (currentCableIndex < currentEval.targets.Count) {
                cableCoolDown--;

                if (cableCoolDown > 0) {
                    return false;
                }

                // Build cable to the target
                TilePosition target = currentEval.targets[currentCableIndex];

                // Find the closest existing cable position to start from
                TilePosition from = TilePosition.Closest(currentEval.startPositions, target);

                // Calculate cable path
                IList<TilePosition> path = map.Pathfind(from, target);
                if (path != null) {
                    // Prevent cable creation from taking up too much remainnig money
                    float cost = path.Count * company.Game.Items[currentEval.cableId].Cost;
                    if (cost / company.Money > 0.5f) {
                        CompleteExecution();
                        return true;
                    } else {
                        // Create cable
                        company.PlaceCable(currentEval.cableId, path);
                    }
                }

                currentCableIndex++;
                cableCoolDown = company.Game.Random.Next(Executor.MinimumCooldown, Executor.FastCooldown);

                return false;
            } else {
                // All cables have been built
                CompleteExecution();
                return true;
            }
        }

        private void CompleteExecution() {
            executionCoolDown = company.Game.Random.Next(Executor.MinimumCooldown, Executor.StandardCooldown); 
            currentCableIndex = -1;
            currentEval = null;
            cableCoolDown = 0;
        }

        public float Score(float deltaTime) {
            if (customers == null)
                Init();

            executionCoolDown--;

            if (executionCoolDown <= 0) {
                // Prevent over-expansion
                if (company.PotentialServiceArea.Count > 0) {
                    int maxPotential = company.PotentialServiceArea.Values.Max(s => s.Count);

                    if (maxPotential > 0 && company.ServiceArea.Count > 0) {
                        if (company.ServiceArea.Count / maxPotential < 0.65f) {
                            return 0;
                        }
                    }
                }


                Evaluation eval = Evaluate();

                // Prevent expansion when it will consume too much available money 
                float chance = 1 - (eval.costEstimate / company.Money);
                if (RandomUtils.Chance(company.Game.Random, chance)) {
                    executionCoolDown = company.Game.Random.Next(Executor.MinimumCooldown, Executor.StandardCooldown); ;
                    return 0;
                }

                return eval.bestScore;

            } else {
                return 0;
            }
        }

        private float AreaPotentialScore(TileRectangle area, CableType cableType, Dictionary<TilePosition, Customer> customers) {
            // Score is based on the income of households with no provider or another provider, relative to the 
            // avergae income of the population.  

            float score = 0;
            float averageIncome = customers.Values.Average(c => c.IncomeLevel);

            for (int x = area.Left; x <= area.Right; x++) {
                for (int y = area.Bottom; y <= area.Top; y++) {
                    TilePosition pos = new TilePosition(x, y);

                    // Ignore positions already in the network
                    if (company.PotentialServiceArea[cableType].Contains(pos))
                        continue;

                    if (customers.ContainsKey(pos)) {
                        float current = 0;

                        Customer c = customers[pos];
                        float incomeScore = c.IncomeLevel / averageIncome;
                        if (c.ProviderID == null) {
                            current = 0.75f * incomeScore;
                        } else if (c.ProviderID != company.ID) {
                            current = 0.25f * incomeScore;
                        }

                        // Deprioritize if it's in a competitor's potential cable area
                        foreach(Company other in company.Game.Companies) {
                            if (other == company)
                                continue;

                            if(other.PotentialServiceArea[cableType].Contains(pos)) {
                                current *= 0.25f;
                                break;
                            }
                        }

                        score += current;
                    }
                }
            }
            return score;
        }

        // Returns an area for expansion given a direction 
        public TileRectangle ExpansionArea(IEnumerable<TilePosition> positions, Direction direction, int howFar) {
            switch (direction) {
                case Direction.North:
                    return map.Area(positions.OrderByDescending(p => p.y).First(), Direction.North, howFar);
                case Direction.South:
                    return map.Area(positions.OrderBy(p => p.y).First(), Direction.South, howFar);
                case Direction.East:
                    return map.Area(positions.OrderByDescending(p => p.x).First(), Direction.East, howFar);
                case Direction.West:
                    return map.Area(positions.OrderBy(p => p.x).First(), Direction.West, howFar);
            }

            return null;
        }

        // Calculates the optimal direction for expansion based on avaialble customers
        private Direction BestDirection(IEnumerable<TilePosition> positions, CableType cableType, int howFar, out float bestScore) {
            Direction direction = Direction.None;
            bestScore = float.MinValue;

            TileRectangle north, south, east, west;

            east = ExpansionArea(positions, Direction.East, howFar);
            south = ExpansionArea(positions, Direction.South, howFar);
            west = ExpansionArea(positions, Direction.West, howFar);
            north = ExpansionArea(positions, Direction.North, howFar);

            Dictionary<Direction, float> scores = new Dictionary<Direction, float>() {
                { Direction.North, AreaPotentialScore(north, cableType, customers) },
                { Direction.South, AreaPotentialScore(south, cableType, customers) },
                { Direction.East,  AreaPotentialScore(east, cableType, customers)  },
                { Direction.West,  AreaPotentialScore(west, cableType, customers)  },
            };

            Direction winner = scores.OrderByDescending(s => s.Value).First().Key;
            if (scores[winner] > bestScore) {
                bestScore = scores[winner];
                direction = winner;
            }

            return direction;
        }

        public Orientation PrimaryRoadOrientation(TileRectangle area) {
            int horizontalCount = 0, verticalCount = 0;

            for (int x = area.Left; x <= area.Right; x++) {
                for (int y = area.Bottom; y <= area.Top; y++) {
                    TilePosition pos = new TilePosition(x, y);
                    Tile tile = map.Tiles[pos];
                    if (tile.Type == TileType.Road) {
                        if (tile.RoadType == RoadType.Horizontal)
                            horizontalCount++;
                        if (tile.RoadType == RoadType.Vertical)
                            verticalCount++;
                    }
                }
            }

            return horizontalCount > verticalCount ? Orientation.Horizontal : Orientation.Vertical;
        }

        // Filters the list of positions to include only roads with the specified Orientation 
        public IEnumerable<TilePosition> FilterRoads(IEnumerable<TilePosition> targets, Orientation targetOrientation) {
            return targets.Where((p) => {
                Tile t = map.Tiles[p];
                return t.Type == TileType.Road &&
                        (t.RoadType == RoadType.Vertical || t.RoadType == RoadType.Horizontal) &&
                        t.RoadOrientation == targetOrientation;
            });
        }

        private Evaluation Evaluate() {
            Direction bestDirection = Direction.None;
            TileRectangle targetArea;
            CableType targetCableType;

            Evaluation eval = new Evaluation();

            if (company.Networks.Count == 0) {
                // First network will always be Copper
                targetCableType = CableType.Copper;

                // Start at HQ and choose optimal direction 
                bestDirection = BestDirection(company.HeadquartersConnectors, targetCableType, expansionDistance, out eval.bestScore);

                // Determine the target area for expansion 
                targetArea = ExpansionArea(company.HeadquartersConnectors, bestDirection, expansionDistance);

                eval.startPositions = company.HeadquartersConnectors;

            } else {
                Network bestNetwork = null;

                // Look at each network to pick the one with the most potential
                foreach (Network network in company.Networks) {
                    // Choose a point to expand toward
                    float directionScore;
                    List<TilePosition> positions = new List<TilePosition>(network.Positions);
                    Direction direction = BestDirection(positions, network.CableType, expansionDistance, out directionScore);
                    if (directionScore > eval.bestScore) {
                        bestNetwork = network;
                        eval.bestScore = directionScore;
                        bestDirection = direction;
                    }
                }

                // Determine the target area for expansion 
                targetArea = ExpansionArea(bestNetwork.Positions, bestDirection, expansionDistance);
                targetCableType = bestNetwork.CableType;
                eval.startPositions = bestNetwork.Positions;
            }

            // TODO: determine whether to start a new network with a new cable type
            // ...


            // Expand into target area
            // Identify tiles in target area that match the primary road orientation 
            Orientation targetOrientation = PrimaryRoadOrientation(targetArea);
            List<TilePosition> targets = new List<TilePosition>();
            List<TilePosition> secondaryTargets = new List<TilePosition>();

            // Create list of target tiles based on the direction and primary road orientation 
            switch (bestDirection) {
                case Direction.North:
                    if (targetOrientation == Orientation.Vertical) {
                        targets = new TileRectangle(targetArea.Left, targetArea.Top, targetArea.Right, targetArea.Top).Positions;
                        secondaryTargets = new TileRectangle(targetArea.Left, targetArea.Top - 1, targetArea.Right, targetArea.Top - 1).Positions;
                    } else {
                        targets.AddRange(new TileRectangle(targetArea.Left, targetArea.Bottom, targetArea.Left, targetArea.Top).Positions);
                        targets.AddRange(new TileRectangle(targetArea.Right, targetArea.Bottom, targetArea.Right, targetArea.Top).Positions);
                    }
                    break;

                case Direction.South:
                    if (targetOrientation == Orientation.Vertical) {
                        targets = new TileRectangle(targetArea.Left, targetArea.Bottom, targetArea.Right, targetArea.Bottom).Positions;
                        secondaryTargets = new TileRectangle(targetArea.Left, targetArea.Bottom + 1, targetArea.Right, targetArea.Bottom + 1).Positions;
                    } else {
                        targets.AddRange(new TileRectangle(targetArea.Left, targetArea.Bottom, targetArea.Left, targetArea.Top).Positions);
                        targets.AddRange(new TileRectangle(targetArea.Right, targetArea.Bottom, targetArea.Right, targetArea.Top).Positions);
                    }
                    break;

                case Direction.East:
                    if (targetOrientation == Orientation.Horizontal) {
                        targets = new TileRectangle(targetArea.Right, targetArea.Bottom, targetArea.Right, targetArea.Top).Positions;
                        secondaryTargets = new TileRectangle(targetArea.Right - 1, targetArea.Bottom, targetArea.Right - 1, targetArea.Top).Positions;
                    } else {
                        targets.AddRange(new TileRectangle(targetArea.Left, targetArea.Bottom, targetArea.Right, targetArea.Bottom).Positions);
                        targets.AddRange(new TileRectangle(targetArea.Left, targetArea.Top, targetArea.Right, targetArea.Top).Positions);
                    }
                    break;

                case Direction.West:
                    if (targetOrientation == Orientation.Horizontal) {
                        targets = new TileRectangle(targetArea.Left, targetArea.Bottom, targetArea.Left, targetArea.Top).Positions;
                        secondaryTargets = new TileRectangle(targetArea.Left + 1, targetArea.Bottom, targetArea.Left + 1, targetArea.Top).Positions;
                    } else {
                        targets.AddRange(new TileRectangle(targetArea.Left, targetArea.Bottom, targetArea.Right, targetArea.Bottom).Positions);
                        targets.AddRange(new TileRectangle(targetArea.Left, targetArea.Top, targetArea.Right, targetArea.Top).Positions);
                    }
                    break;
            }

            // Filter out only roads that have the right orientation 
            targets = new List<TilePosition>(FilterRoads(targets, targetOrientation));
            secondaryTargets = new List<TilePosition>(FilterRoads(secondaryTargets, targetOrientation));

            // Determine if we prefer the primary or secondary targets
            if (secondaryTargets.Count() > targets.Count()) {
                targets = secondaryTargets;
            }

            eval.targets = targets;

            // Determine which cable to place
            switch (targetCableType) {
                case CableType.Copper:
                    eval.cableId = Cable.CAT3;
                    break;
                case CableType.Coaxial:
                    eval.cableId = Cable.RG6;
                    break;
                case CableType.Optical:
                default:
                    eval.cableId = Cable.OM2;
                    break;
            }

            eval.costEstimate = company.CableCost(eval.cableId, eval.targets.Count);

            return eval;
        }

        
    }
}