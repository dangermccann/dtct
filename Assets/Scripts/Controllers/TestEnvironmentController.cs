using UnityEngine;
using System.Collections.Generic;
using DCTC.Model;
using DCTC.Map;

namespace DCTC.Controllers {
    public class TestEnvironmentController : MonoBehaviour {

        public ThreeDMap ThreeDMap;
        private MapConfiguration map;

        private void Start() {
            Generate();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space))
                Generate();
        }

        void Generate() {
            GameObject.Find("Environment").SetActive(false);

            System.Random rand = new System.Random();
            NewGameSettings settings = new NewGameSettings() {
                NeighborhoodCountX = 2,
                NeighborhoodCountY = 2
            };

            MapGenerator generator = new MapGenerator(rand, settings, new NameGenerator(rand));
            map = generator.Generate();

            GameController.Get().Map = map;
            ThreeDMap.Init(map);
        }

        void Redraw() {
            GameObject.Find("Environment").SetActive(false);

            int width = 8;
            int height = 13;
            int numBlocksX = 5;
            int numBlocksY = 3;

            map = new MapConfiguration(width + (width - 1) * (numBlocksX - 1), height + (height - 1) * (numBlocksY - 1));
            map.Tiles = new Dictionary<TilePosition, Tile>();

            
            for(int x = 0; x < numBlocksX; x++) {
                for(int y = 0; y < numBlocksY; y++) {
                    CreateBlock(Mathf.Max(0, x * (width - 1)), Mathf.Max(0, y * (height - 1)), width, height);
                }
            }

            map.Neighborhoods.Add(new Neighborhood(map, map.Width, map.Height));


            GameController.Get().Map = map;
            ThreeDMap.Init(map);

            Cable cable = new Cable("", new CableAttributes() { Color = "0xffffff", Wiring = "Copper" });
            for(int z = 29; z >= 0; z--) {
                cable.Positions.Add(new TilePosition(0, z));
            }

            for (int x = 1; x < 30; x++) {
                cable.Positions.Add(new TilePosition(x, 0));
            }
            ThreeDMap.PlaceCable(cable);
        }




        void CreateBuilding(int x, int y, BuildingType type, int width, int height,
            Direction facing, Neighborhood neighborhood, string blockPos, int variation) {
            TilePosition position = new TilePosition(x, y);
            Lot lot = new Lot();
            lot.Anchor = position;
            lot.Facing = facing;

            Building building = new Building(map.Tiles[position], type, facing,
                        width, height, blockPos, variation);
            lot.Building = building;

            for (int dx = 0; dx < width; dx++) {
                for (int dy = 0; dy < height; dy++) {
                    Tile tile = map.Tiles[new TilePosition(dx + position.x, dy + position.y)];
                    tile.Building = building;
                    tile.Lot = lot;
                }
            }

            neighborhood.Lots.Add(lot);
        }

        void CreateBlock(int offsetX, int offsetY, int width, int height) {
            TilePosition pos;

            // corners
            pos = new TilePosition(offsetX, offsetY);
            RoadType firstType = RoadType.CornerNE;
            if (offsetX > 0 && offsetY > 0)
                firstType = RoadType.IntersectAll;
            else if (offsetX > 0)
                firstType = RoadType.IntersectN;
            else if (offsetY > 0)
                firstType = RoadType.IntersectE;
            map.Tiles[pos] = new Tile(pos, TileType.Road, firstType);

            pos = new TilePosition(offsetX + width - 1, offsetY);
            map.Tiles[pos] = new Tile(pos, TileType.Road, offsetY > 0 ? RoadType.IntersectW : RoadType.CornerNW);

            pos = new TilePosition(offsetX, offsetY + height - 1);
            map.Tiles[pos] = new Tile(pos, TileType.Road, offsetX > 0 ? RoadType.IntersectS : RoadType.CornerSE);

            pos = new TilePosition(offsetX + width - 1, offsetY + height - 1);
            map.Tiles[pos] = new Tile(pos, TileType.Road, RoadType.CornerSW);



            for (int x = 1; x < width - 1; x++) {
                // bottom and top
                pos = new TilePosition(offsetX + x, offsetY);
                map.Tiles[pos] = new Tile(pos, TileType.Road, RoadType.Horizontal);

                pos = new TilePosition(offsetX + x, offsetY + height - 1);
                map.Tiles[pos] = new Tile(pos, TileType.Road, RoadType.Horizontal);
            }

            for (int y = 1; y < height - 1; y++) {
                // sides 
                pos = new TilePosition(offsetX, offsetY + y);
                map.Tiles[pos] = new Tile(pos, TileType.Road, RoadType.Vertical);

                pos = new TilePosition(offsetX + width - 1, offsetY + y);
                map.Tiles[pos] = new Tile(pos, TileType.Road, RoadType.Vertical);
            }


            for (int x = 1; x < width - 1; x++) {
                for (int y = 1; y < height - 1; y++) {
                    // inside
                    pos = new TilePosition(offsetX + x, offsetY + y);
                    map.Tiles[pos] = new Tile(pos, TileType.Grass);
                }
            }

        }
    }
}
