using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DCTC.Map;


namespace DCTC.UI {
    public class LotOutline : MonoBehaviour {

        private LineRenderer lineRenderer;
        private readonly float y = 0.1f;

        [HideInInspector]
        public IEnumerable<TilePosition> Positions;

        private HashSet<TilePosition> positionsSet;

        void Start() {
            lineRenderer = GetComponent<LineRenderer>();
        }

        public void Redraw() {
            positionsSet = new HashSet<TilePosition>(Positions);

            int lefMost = positionsSet.Min(pos => pos.x);
            int bottomMost = positionsSet.Min(pos => (pos.x == lefMost) ? pos.y : int.MaxValue);

            TilePosition current = new TilePosition(lefMost, bottomMost);
            Direction direction = Direction.North;

            var visited = new HashSet<TilePosition>();
            var points = new List<Vector3>();
            float c = 2.0f;

            Vector3 world = ThreeDMap.PositionToWorld(current);
            Vector3 point;
            points.Add(new Vector3(world.x, y, world.z));

            do {
                world = ThreeDMap.PositionToWorld(current);
                switch(direction) {
                    case Direction.North:
                        point = new Vector3(world.x, y, world.z + c);
                        break;
                    case Direction.South:
                        point = new Vector3(world.x + c, y, world.z);
                        break;
                    case Direction.East:
                        point = new Vector3(world.x + c, y, world.z + c);
                        break;
                    case Direction.West:
                    default:
                        point = new Vector3(world.x, y, world.z);
                        break;
                }
                
                points.Add(point);

                visited.Add(current);
                current = Next(current, out direction);
            }
            while (!visited.Contains(current));

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }

        TilePosition Next(TilePosition current, out Direction direction) {
            TilePosition north = MapConfiguration.North(current);
            TilePosition east  = MapConfiguration.East(current);
            TilePosition south = MapConfiguration.South(current);
            TilePosition west  = MapConfiguration.West(current);

            if (positionsSet.Contains(north)) {
                direction = Direction.North;
                return north;
            }
            if (positionsSet.Contains(east)) {
                direction = Direction.East;
                return east;
            }
            if (positionsSet.Contains(south)) {
                direction = Direction.South;
                return south;
            }
            if (positionsSet.Contains(west)) {
                direction = Direction.West;
                return west;
            }

            throw new System.Exception("Can't find next position!");
        }


    }
}
