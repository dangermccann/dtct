using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.Map {
    public class ThreeDMap : AbstractMap {

        public ThreeDCameraController cameraController;

        public delegate void TileSelectEventHandler(Tile tile);
        public event TileSelectEventHandler TileSelected;

        private MapConfiguration map;
        private GameObject canvas;
        private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        private HashSet<TilePosition> selectedTiles = new HashSet<TilePosition>();
        private MaterialController materialController;
        private GameController gameController;

        private int batchCount = 0;
        private const int BatchSize = 200;

        void Start() {
            LoadPrefabs();
            materialController = MaterialController.Get();
            canvas = transform.GetChild(0).gameObject;

            cameraController.TileClicked += OnTileClicked;
            gameController = GameController.Get();

            if(gameController.Map != null) {
                Init(gameController.Map);
                StartDraw();
                cameraController.ResetToDefault();
            }
        }

        void LoadPrefabs() {
            GameObject[] all = Resources.LoadAll<GameObject>("Prefabs/Map");
            foreach (GameObject go in all) {
                if (prefabs.ContainsKey(go.name)) {
                    Debug.LogWarning("Duplicate prefab: " + go.name);
                    continue;
                }
                prefabs.Add(go.name, go);
            }
        }

        public override void Init(MapConfiguration config) {
            map = config;
        }

        public override void StartDraw() {
            StartCoroutine(Redraw());
        }

        public override void MoveCameraTo(TilePosition position) {
            cameraController.MoveCamera(PositionToWorld(position));
        }

        IEnumerator Redraw() {
            batchCount = 0;

            yield return StartCoroutine( DrawGround() );
            yield return StartCoroutine( DrawBuildings() );
            yield return StartCoroutine( DrawLabels() );
        }


        public void EnterTileDetails(TilePosition pos) {
            Select(pos);

            Tile tile = map.Tiles[pos];
            Vector3 world = PositionToWorld(pos);
            cameraController.SaveCameraLocation();

            int tileCount = 1;
            if (tile.Lot != null)
                tileCount = tile.Lot.Tiles.Count;
            float distance = Mathf.Lerp(15, 60, tileCount / 30f);

            Direction cameraFacing = Direction.North;
            if (tile.Building != null)
                cameraFacing = MapConfiguration.OppositeDirection(tile.Building.FacingDirection);

            cameraController.FocusOnPosition(world, cameraFacing, distance);
            cameraController.SelectionEnabled = false;

            if (TileSelected != null)
                TileSelected(tile);
        }

        public void ExitLotDetails() {
            Debug.Log("ExitLotDetails " + cameraController.SelectionEnabled);

            if(cameraController.SelectionEnabled == false) {
                cameraController.RestoreCameraLocation();
                cameraController.SelectionEnabled = true;
                ClearSelection();
            }
        }

        void Update() {
            if(Input.GetKeyDown(KeyCode.Escape)) {
                
            }
        }

        private void OnTileClicked(Vector3 world) {
            TilePosition pos = WorldToPosition(world);

            if (map.Tiles.ContainsKey(pos)) {
                Tile tile = map.Tiles[pos];
                if(tile.Building != null) {
                    Debug.Log(BuildingName(pos) + " " + tile.Building.Type.ToString());
                }
                else {
                    if(tile.Type == TileType.Road) {
                        Debug.Log(TileName(pos) + " " + map.FindStreet(pos).Name);
                    }
                    else {
                        Debug.Log(TileName(pos) + " " + tile.Type.ToString());
                    }
                }
            }
            else {
                Debug.Log("Tile Clicked world=" + world + ", pos=" + pos);
            }
        }

        void Select(TilePosition pos) {
            ClearSelection();

            Tile tile = map.Tiles[pos];

            if (tile.Lot != null) {
                selectedTiles.AddMany(tile.Lot.Tiles);
            } else {
                selectedTiles.Add(tile.Position);
            }

            foreach (TilePosition selectedPos in selectedTiles) {
                ApplyMaterial(selectedPos, GetMaterialForTile(tile, true));
            }
        }

        void ClearSelection() {
            foreach (TilePosition pos in selectedTiles) {
                Tile tile = map.Tiles[pos];
                ApplyMaterial(pos, GetMaterialForTile(tile, false));
            }
            selectedTiles.Clear();
        }

        void ApplyMaterial(TilePosition pos, Material mat) {
             ApplyMaterial(GetTileGraphics(GetTileGameObject(pos)), mat);
        }

        void ApplyMaterial(GameObject go, Material mat) {
            if (go != null) {
                go.GetComponent<MeshRenderer>().material = mat;
            }
        }

        GameObject GetTileGameObject(TilePosition pos) {
            return transform.Find(TileName(pos)).gameObject;
        }

        GameObject GetTileGraphics(GameObject go) {
            return go.transform.GetChild(0).gameObject;
        }

        string TileName(TilePosition pos) {
            return "Tile (" + pos.x + ", " + pos.y + ")";
        }

        string BuildingName(TilePosition pos) {
            return "Building (" + pos.x + ", " + pos.y + ")";
        }


        IEnumerator DrawGround() {
            GameObject prefab;

            Dictionary<TilePosition, GameObject> completedQuads = new Dictionary<TilePosition, GameObject>();

            for (int z = 0; z < map.Height; z++) {
                for (int x = 0; x < map.Width; x++) {

                    prefab = null;
                    TilePosition pos = new TilePosition(x, z);
                    Tile tile = map.Tiles[pos];
                    Vector3 scale = Vector3.one;

                    if (tile.Type == TileType.Road) {
                        prefab = prefabs["Road" + tile.RoadType.ToString()];
                    } else {
                        // Optimization to stretch quad 
                        if(!completedQuads.ContainsKey(pos)) {
                            prefab = prefabs["Quad"];
                            completedQuads.Add(pos, prefab);
                            int quadZ = z + 1;
                            int quadX = x + 1;
                            Tile quadTile;

                            while(quadZ < map.Height) {
                                quadTile = map.Tiles[new TilePosition(x, quadZ)];
                                if(quadTile.Type != tile.Type) {
                                    break;
                                }
                                else {
                                    completedQuads.Add(quadTile.Position, prefab);
                                    scale.z += 1;
                                    quadZ++;
                                }
                            }

                            while (quadX < map.Width) {
                                quadTile = map.Tiles[new TilePosition(quadX, z)];
                                if (quadTile.Type != tile.Type) {
                                    break;
                                } else {
                                    completedQuads.Add(quadTile.Position, prefab);
                                    scale.x += 1;
                                    quadX++;
                                }
                            }

                        }
                        
                    }

                    if (prefab != null) {
                        GameObject tileGo = Instantiate(prefab);
                        tileGo.name = TileName(pos); ;
                        tileGo.transform.SetParent(this.transform, false);
                        tileGo.transform.position = new Vector3(x * 2, 0, z * 2);
                        tileGo.transform.localScale = scale;
                        ApplyMaterial(pos, GetMaterialForTile(tile, false));
                        batchCount++;

                        if (batchCount > BatchSize) {
                            batchCount = 0;
                            yield return null;
                        }
                    }
                }
            }
        }

        IEnumerator DrawBuildings() {
            GameObject prefab = null;

            for (int i = 0; i < map.Neighborhoods.Count; i++) {
                Neighborhood n = map.Neighborhoods[i];
                for (int j = 0; j < n.Lots.Count; j++) {
                    Lot lot = n.Lots[j];

                    if (lot.Building != null) {
                        string prefabName = lot.Building.Type.ToString();

                        if(!prefabs.ContainsKey(prefabName)) {
                            Debug.LogWarning("Missing prefab: " + prefabName);
                            continue;
                        }

                        prefab = prefabs[prefabName];
                        GameObject buildingGO = Instantiate(prefab);
                        buildingGO.name = BuildingName(lot.Building.Anchor);
                        buildingGO.transform.SetParent(this.transform, false);
                        buildingGO.transform.position = new Vector3(lot.Building.Anchor.x * 2, 0, lot.Building.Anchor.y * 2);

                        float rotation = 0;
                        switch (lot.Building.FacingDirection) {
                            case Direction.East:
                                rotation = 0;
                                break;
                            case Direction.South:
                                rotation = 90;
                                break;
                            case Direction.West:
                                rotation = 180;
                                break;
                            case Direction.North:
                                rotation = 270;
                                break;
                        }

                        if (rotation != 0) {
                            Transform meshTransform = buildingGO.transform.GetChild(0).transform;

                            meshTransform.Rotate(Vector3.up, rotation, Space.World);

                            // If we're rotating 90 or 270 degrees and this isn't a square lot, flip the x and z coordinates of the mesh
                            if(lot.Building.Width != lot.Building.Height) {
                                if (lot.Building.FacingDirection == Direction.North || lot.Building.FacingDirection == Direction.South) {
                                    meshTransform.localPosition = new Vector3(meshTransform.localPosition.z, meshTransform.localPosition.y, meshTransform.localPosition.x);
                                }
                            }
                        }

                        batchCount++;
                        if (batchCount > BatchSize) {
                            batchCount = 0;
                            yield return null;
                        }
                    }
                }
            }
        }

        IEnumerator DrawLabels() {
            foreach (Neighborhood neighborhood in map.Neighborhoods) {
                foreach (Street street in neighborhood.Streets) {

                    Segment segment = street.Segments[0];
                    Vector2 length = segment.End.AsVector() - segment.Start.AsVector();

                    if (length.magnitude < 3)
                        continue;

                    Vector2 pos = new Vector2(segment.Start.x + length.x / 2f, segment.Start.y + length.y / 2f);

                    GameObject labelGO = Instantiate(prefabs["RoadLabel"]);
                    labelGO.name = "Label (" + street.Name + ")";
                    labelGO.transform.SetParent(canvas.transform, false);

                    Vector3 world = PositionToWorld(pos);
                    labelGO.transform.position = new Vector3(world.x + 1f, world.y + 0.05f, world.z + 1f); ;

                    if (segment.Orientation == Orientation.Vertical) {
                        labelGO.transform.Rotate(Vector3.up, 90, Space.World);
                    }

                    labelGO.GetComponent<Text>().text = street.Name;

                    batchCount++;
                    if (batchCount > BatchSize) {
                        batchCount = 0;
                        yield return null;
                    }

                }
            }
        }

        private Material GetMaterialForTile(Tile tile, bool isSelected) {
            string materialName = "Blueprint Light";

            if(isSelected) {
                materialName = "Selection";
            }
            else if(tile.Type == TileType.Road) {
                materialName = "BlueprintRoad";
            }
            else if(tile.Type == TileType.FruitFarm || tile.Type == TileType.VegetableFarm ||
                tile.Type == TileType.SoybeanFarm || tile.Type == TileType.TilledSoil) {
                materialName = "Farm";
            }

            return materialController.GetMaterial(materialName);            
        }

        public static Vector3 PositionToWorld(TilePosition pos) {
            return PositionToWorld(pos.AsVector());
        }

        public static Vector3 PositionToWorld(Vector2 pos) {
            return new Vector3(pos.x * 2, 0, pos.y * 2);
        }

        public static TilePosition WorldToPosition(Vector3 world) {
            return new TilePosition(Mathf.FloorToInt(world.x / 2f), Mathf.FloorToInt(world.z / 2f));
        }

        
    } 
}