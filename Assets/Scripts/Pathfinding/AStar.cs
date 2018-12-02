
using System;
using System.Collections.Generic;
using DCTC.Map;

namespace DCTC.Pathfinding
{
	public interface IPathNode {
		TilePosition Position { get; }
		int MovementCost { get; }
	}

	class AStarElement {
		public IPathNode Node;
		public int GCost;			// Cost to get from start to this node
		public int HCost;			// Heuristic estimate to get from this node to goal
		public AStarElement Parent;	// Element before this one in the path

		public int FCost {
			get { return GCost + HCost; }
		}

		public TilePosition Position {
			get { return Node.Position; }
		}

		public int MovementCost {
			get { return Node.MovementCost; }
		}

		public AStarElement(IPathNode node) {
			this.Node = node;
			this.GCost = int.MaxValue;
		}
	}

	public class AStar {
		Dictionary<TilePosition, AStarElement> AllElements;
		PriorityQueue<int, AStarElement> OpenElements;
		Dictionary<TilePosition, AStarElement> ClosedElements;

        public int MinimumMovementCost = 10000;

		public AStar(ICollection<IPathNode> nodes) {
			AllElements = new Dictionary<TilePosition, AStarElement>();
			foreach(IPathNode node in nodes) {
				AllElements.Add(node.Position, new AStarElement(node));
			}
		}

		public IList<IPathNode> Search(IPathNode fromNode, IPathNode toNode) {
			OpenElements = new PriorityQueue<int, AStarElement>();
			ClosedElements = new Dictionary<TilePosition, AStarElement>();

			AStarElement current = AllElements[fromNode.Position];
			AStarElement goal = AllElements[toNode.Position];

			current.GCost = 0;
			current.HCost = Distance(current, goal);
			OpenElements.Enqueue(current, current.GCost);

			while(!OpenElements.IsEmpty) {
				current = OpenElements.Dequeue();

				//Debug.Log("Inspecting " + current.Position);

				if(current.Position == goal.Position) {
					//Debug.Log("Reached goal!");
					return ReconstructPath(current);
				}

				ClosedElements.Add(current.Position, current);

				IList<AStarElement> neighbors = Neighbors(current);
				foreach(AStarElement neighbor in neighbors) {
					if(ClosedElements.ContainsKey(neighbor.Position))
						continue;

                    if (neighbor.MovementCost > MinimumMovementCost)
                        continue;

					int tentativeGCost = current.GCost + neighbor.MovementCost;
					if(tentativeGCost >= neighbor.GCost) {
						continue;
					}

					int oldCost = neighbor.GCost;

					neighbor.Parent = current;
					neighbor.GCost = tentativeGCost;
					neighbor.HCost = Distance(neighbor, goal);

					if(OpenElements.Contains(neighbor, oldCost))
						OpenElements.Replace(neighbor, oldCost, neighbor.GCost);
					else
					   OpenElements.Enqueue(neighbor, neighbor.GCost);

					//Debug.Log("Found neighbor " + neighbor.Position + " GCost=" + neighbor.GCost);
				}
			}


			return null;
		}

		IList<AStarElement> Neighbors(AStarElement element) {
			TilePosition pos;
			List<AStarElement> result = new List<AStarElement>();

			pos = new TilePosition(element.Position.x + 1, element.Position.y);
			if(AllElements.ContainsKey(pos))
				result.Add(AllElements[pos]);

			pos = new TilePosition(element.Position.x - 1, element.Position.y);
			if(AllElements.ContainsKey(pos))
				result.Add(AllElements[pos]);

			pos = new TilePosition(element.Position.x, element.Position.y + 1);
			if(AllElements.ContainsKey(pos))
				result.Add(AllElements[pos]);

			pos = new TilePosition(element.Position.x, element.Position.y - 1);
			if(AllElements.ContainsKey(pos))
				result.Add(AllElements[pos]);

			return result;
		}

		IList<IPathNode> ReconstructPath(AStarElement current) {
			List<IPathNode> result = new List<IPathNode>();
			while(current != null) {
				result.Add(current.Node);
				current = current.Parent;
			}

			return result;
		}

		int Distance(AStarElement from, AStarElement to) {
			return TilePosition.Distance(from.Position, to.Position);
		}

	}
}

