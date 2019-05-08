using UnityEngine;
using System.Collections.Generic;
using DCTC.Model;
using DCTC.Map;

namespace DCTC.Controllers {
    public class TestEnvironmentController : MonoBehaviour {

        public ThreeDMap ThreeDMap;
        private MapConfiguration map;

        private void Start() {
            Debug.Log("PRESS SPACE BAR");
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space))
                Redraw();
        }

        void Redraw() {
            map = new MapConfiguration(7, 8);
            map.Tiles = new Dictionary<TilePosition, Tile>() {
                { new TilePosition(0, 0),  new Tile(new TilePosition(0, 0), TileType.Road, RoadType.CornerNE) },
                { new TilePosition(0, 1),  new Tile(new TilePosition(0, 1), TileType.Road, RoadType.Vertical) },
                { new TilePosition(0, 2),  new Tile(new TilePosition(0, 2), TileType.Road, RoadType.Vertical) },
                { new TilePosition(0, 3),  new Tile(new TilePosition(0, 3), TileType.Road, RoadType.Vertical) },
                { new TilePosition(0, 4),  new Tile(new TilePosition(0, 4), TileType.Road, RoadType.Vertical) },
                { new TilePosition(0, 5),  new Tile(new TilePosition(0, 5), TileType.Road, RoadType.Vertical) },
                { new TilePosition(0, 6),  new Tile(new TilePosition(0, 6), TileType.Road, RoadType.Vertical) },
                { new TilePosition(0, 7),  new Tile(new TilePosition(0, 7), TileType.Road, RoadType.CornerSE) },


                { new TilePosition(1, 0),  new Tile(new TilePosition(1, 0), TileType.Road, RoadType.Horizontal) },
                { new TilePosition(1, 1),  new Tile(new TilePosition(1, 1), TileType.Grass) },
                { new TilePosition(1, 2),  new Tile(new TilePosition(1, 2), TileType.Grass) },
                { new TilePosition(1, 3),  new Tile(new TilePosition(1, 3), TileType.Grass) },
                { new TilePosition(1, 4),  new Tile(new TilePosition(1, 4), TileType.Grass) },
                { new TilePosition(1, 5),  new Tile(new TilePosition(1, 5), TileType.Grass) },
                { new TilePosition(1, 6),  new Tile(new TilePosition(1, 6), TileType.Grass) },
                { new TilePosition(1, 7),  new Tile(new TilePosition(1, 7), TileType.Road, RoadType.Horizontal) },


                { new TilePosition(2, 0),  new Tile(new TilePosition(2, 0), TileType.Road, RoadType.Horizontal) },
                { new TilePosition(2, 1),  new Tile(new TilePosition(2, 1), TileType.Grass) },
                { new TilePosition(2, 2),  new Tile(new TilePosition(2, 2), TileType.Grass) },
                { new TilePosition(2, 3),  new Tile(new TilePosition(2, 3), TileType.Grass) },
                { new TilePosition(2, 4),  new Tile(new TilePosition(2, 4), TileType.Grass) },
                { new TilePosition(2, 5),  new Tile(new TilePosition(2, 5), TileType.Grass) },
                { new TilePosition(2, 6),  new Tile(new TilePosition(2, 6), TileType.Grass) },
                { new TilePosition(2, 7),  new Tile(new TilePosition(2, 7), TileType.Road, RoadType.Horizontal) },


                { new TilePosition(3, 0),  new Tile(new TilePosition(3, 0), TileType.Road, RoadType.Horizontal) },
                { new TilePosition(3, 1),  new Tile(new TilePosition(3, 1), TileType.Grass) },
                { new TilePosition(3, 2),  new Tile(new TilePosition(3, 2), TileType.Grass) },
                { new TilePosition(3, 3),  new Tile(new TilePosition(3, 3), TileType.Grass) },
                { new TilePosition(3, 4),  new Tile(new TilePosition(3, 4), TileType.Grass) },
                { new TilePosition(3, 5),  new Tile(new TilePosition(3, 5), TileType.Grass) },
                { new TilePosition(3, 6),  new Tile(new TilePosition(3, 6), TileType.Grass) },
                { new TilePosition(3, 7),  new Tile(new TilePosition(3, 7), TileType.Road, RoadType.Horizontal) },


                { new TilePosition(4, 0),  new Tile(new TilePosition(4, 0), TileType.Road, RoadType.Horizontal) },
                { new TilePosition(4, 1),  new Tile(new TilePosition(4, 1), TileType.Grass) },
                { new TilePosition(4, 2),  new Tile(new TilePosition(4, 2), TileType.Grass) },
                { new TilePosition(4, 3),  new Tile(new TilePosition(4, 3), TileType.Grass) },
                { new TilePosition(4, 4),  new Tile(new TilePosition(4, 4), TileType.Grass) },
                { new TilePosition(4, 5),  new Tile(new TilePosition(4, 5), TileType.Grass) },
                { new TilePosition(4, 6),  new Tile(new TilePosition(4, 6), TileType.Grass) },
                { new TilePosition(4, 7),  new Tile(new TilePosition(4, 7), TileType.Road, RoadType.Horizontal) },


                { new TilePosition(5, 0),  new Tile(new TilePosition(5, 0), TileType.Road, RoadType.Horizontal) },
                { new TilePosition(5, 1),  new Tile(new TilePosition(5, 1), TileType.Grass) },
                { new TilePosition(5, 2),  new Tile(new TilePosition(5, 2), TileType.Grass) },
                { new TilePosition(5, 3),  new Tile(new TilePosition(5, 3), TileType.Grass) },
                { new TilePosition(5, 4),  new Tile(new TilePosition(5, 4), TileType.Grass) },
                { new TilePosition(5, 5),  new Tile(new TilePosition(5, 5), TileType.Grass) },
                { new TilePosition(5, 6),  new Tile(new TilePosition(5, 6), TileType.Grass) },
                { new TilePosition(5, 7),  new Tile(new TilePosition(5, 7), TileType.Road, RoadType.Horizontal) },


                { new TilePosition(6, 0),  new Tile(new TilePosition(6, 0), TileType.Road, RoadType.CornerNW) },
                { new TilePosition(6, 1),  new Tile(new TilePosition(6, 1), TileType.Road, RoadType.Vertical) },
                { new TilePosition(6, 2),  new Tile(new TilePosition(6, 2), TileType.Road, RoadType.Vertical) },
                { new TilePosition(6, 3),  new Tile(new TilePosition(6, 3), TileType.Road, RoadType.Vertical) },
                { new TilePosition(6, 4),  new Tile(new TilePosition(6, 4), TileType.Road, RoadType.Vertical) },
                { new TilePosition(6, 5),  new Tile(new TilePosition(6, 5), TileType.Road, RoadType.Vertical) },
                { new TilePosition(6, 6),  new Tile(new TilePosition(6, 6), TileType.Road, RoadType.Vertical) },
                { new TilePosition(6, 7),  new Tile(new TilePosition(6, 7), TileType.Road, RoadType.CornerSW) },

            };

            GameController.Get().Map = map;
            ThreeDMap.Init(map);
        }

    }
}
