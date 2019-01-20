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
        private HashSet<TilePosition> visited;
        private List<Vector3> points;

        void Start() {
            lineRenderer = GetComponent<LineRenderer>();
        }

        // TODO: this doesn't work for non-rectangular lots!
        public void Redraw() {
            Vector3[] points = CalculatePoints();
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }

        private void CalculatePoint(TilePosition current) {
            Vector3 point;
            TilePosition north = MapConfiguration.North(current);
            TilePosition east = MapConfiguration.East(current);
            TilePosition south = MapConfiguration.South(current);
            TilePosition west = MapConfiguration.West(current);

            if(positionsSet.Contains(north) && positionsSet.Contains(south) 
                && positionsSet.Contains(east) && positionsSet.Contains(west)) {
                // We're on a tile in the interior. 
                return;
            }

            Vector3 world = ThreeDMap.PositionToWorld(current);
            float c = 2.0f;

            visited.Add(current);

            if (!positionsSet.Contains(west)) {
                point = new Vector3(world.x, y, world.z + c);
                if (!points.Contains(point))
                    points.Add(point);
                else
                    return;
            } 

            if (!positionsSet.Contains(north)) {
                point = new Vector3(world.x + c, y, world.z + c);
                if (!points.Contains(point))
                    points.Add(point);
                else
                    return;
            } else if (!visited.Contains(north)) {
                CalculatePoint(north);
            }

            if (!positionsSet.Contains(east)) {
                point = new Vector3(world.x + c, y, world.z);
                if (!points.Contains(point))
                    points.Add(point);
                else
                    return;
            } else if (!visited.Contains(east)) {
                CalculatePoint(east);
            }

            if (!positionsSet.Contains(south)) {
                point = new Vector3(world.x, y, world.z);
                if (!points.Contains(point))
                    points.Add(point);
                else
                    return;
            } else if (!visited.Contains(south)) {
                CalculatePoint(south);
            }

            if (positionsSet.Contains(west) && !visited.Contains(west)) {
                CalculatePoint(west);
            }
        }
    

        public Vector3[] CalculatePoints() { 
            positionsSet = new HashSet<TilePosition>(Positions);
            visited = new HashSet<TilePosition>();

            int leftMost = positionsSet.Min(pos => pos.x);
            int bottomMost = positionsSet.Min(pos => (pos.x == leftMost) ? pos.y : int.MaxValue);

            TilePosition current = new TilePosition(leftMost, bottomMost);

            points = new List<Vector3>();
        
            Vector3 world = ThreeDMap.PositionToWorld(current);
            points.Add(new Vector3(world.x, y, world.z));

            CalculatePoint(current);

            return points.ToArray();
        }


    }
}
