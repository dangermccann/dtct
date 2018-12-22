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
        private Dictionary<TilePosition, GameObject> buildings = new Dictionary<TilePosition, GameObject>();
        private Dictionary<string, GameObject> labels = new Dictionary<string, GameObject>();
        private Dictionary<TilePosition, GameObject> ground = new Dictionary<TilePosition, GameObject>();
        private MaterialController materialController;
        private GameController gameController;
        private HashSet<TilePosition> highlightedPositions = new HashSet<TilePosition>();
        private Dictionary<TilePosition, MeshRenderer> buildingRenderers = new Dictionary<TilePosition, MeshRenderer>();

        private int batchCount = 0;
        private const int BatchSize = 100;

        void Start() {
            LoadPrefabs();
            materialController = MaterialController.Get();
            canvas = transform.GetChild(0).gameObject;

            cameraController.TileClicked += OnTileClicked;
            gameController = GameController.Get();

            if(gameController.Map != null) {
                Init(gameController.Map);
            }

            gameController.GameLoaded += OnGameLoaded;
        }

        private void OnGameLoaded() {
            Clear();
            Init(gameController.Map);
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

        private bool groundVisible = true;
        public bool GroundVisible {
            get { return groundVisible; }
            set {
                groundVisible = value;
                foreach(GameObject go in ground.Values) {
                    go.SetActive(groundVisible);
                }
            }
        }
        public void ToggleGround() { GroundVisible = !GroundVisible; }

        private bool labelsVisible = true;
        public bool LabelsVisible {
            get { return labelsVisible; }
            set {
                labelsVisible = value;
                foreach (GameObject go in labels.Values) {
                    go.SetActive(labelsVisible);
                }
            }
        }
        public void ToggleLabels() { LabelsVisible = !LabelsVisible; }

        private bool buildingsVisible = true;
        public bool BuildingsVisible {
            get { return buildingsVisible; }
            set {
                buildingsVisible = value;
                foreach (GameObject go in buildings.Values) {
                    go.SetActive(buildingsVisible);
                }
            }
        }
        public void ToggleBuildings() { BuildingsVisible = !BuildingsVisible; }


        private bool serviceAreaVisible = true;
        public bool ServiceAreaVisible {
            get { return serviceAreaVisible; }
            set {
                serviceAreaVisible = value;
            }
        }
        public void ToggleServiceArea() { ServiceAreaVisible = !ServiceAreaVisible; }


        public int HighlightRadius { get; set; }

        public void Clear() {
            List<GameObject> removals = new List<GameObject>();
            removals.AddRange(GameObject.FindGameObjectsWithTag("Ground"));
            removals.AddRange(GameObject.FindGameObjectsWithTag("Building"));
            removals.AddRange(GameObject.FindGameObjectsWithTag("Road"));
            removals.AddRange(GameObject.FindGameObjectsWithTag("Placed"));

            foreach(GameObject go in removals) {
                GameObject.Destroy(go);
            }

            HighlightRadius = 0;
            highlightedPositions.Clear();
        }

        public override void Init(MapConfiguration config) {
            map = config;
            StartDraw();
            cameraController.ResetToDefault();

            foreach(Company company in gameController.Game.Companies) {
                company.ItemAdded += PlaceItem;
                company.ItemRemoved += RemoveItem;
            }
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
            yield return StartCoroutine( PlaceItems() );
        }


        public void EnterTileDetails(TilePosition pos) {
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
            }
        }

        void Update() {
            if (gameController.Game == null)
                return;

            HashSet<TilePosition> removals = new HashSet<TilePosition>();
            removals.AddMany(highlightedPositions);

            if (HighlightRadius > 0) {
                TilePosition mousePosition = WorldToPosition(cameraController.MouseCursorInWorld());
                for (int x = mousePosition.x - HighlightRadius; x <= mousePosition.x + HighlightRadius; x++) {
                    for (int y = mousePosition.y - HighlightRadius; y <= mousePosition.y + HighlightRadius; y++) {
                        TilePosition pos = new TilePosition(x, y);
                        if (map.IsInBounds(pos)) {
                            HighlightBuilding(pos, true);

                            if (removals.Contains(pos))
                                removals.Remove(pos);

                            if (!highlightedPositions.Contains(pos))
                                highlightedPositions.Add(pos);
                        }
                    }
                }
            }

            HighlightBuildings(removals, false);
            highlightedPositions.RemoveMany(removals);

            IEnumerable<TilePosition> area = gameController.Game.Player.ServiceArea;
            foreach (TilePosition pos in area) {
                Tile tile = map.Tiles[pos];
                if (tile.Building != null) {
                    HighlightBuilding(pos, ServiceAreaVisible);
                }
            }
        }

        private void OnTileClicked(Vector3 world, TilePosition pos) {

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

        void HighlightBuildings(IEnumerable<TilePosition> positions, bool highlight) {
            foreach (TilePosition pos in positions) {
                HighlightBuilding(pos, highlight);
            }
        }

        void HighlightBuilding(TilePosition pos, bool highlight) {
            Tile tile = map.Tiles[pos];
            if (tile.Building != null) {
                MeshRenderer renderer = buildingRenderers[tile.Building.Anchor];
                Material highlightMaterial = GetMaterialForBuilding(tile.Building, true);
                if (highlight) {
                    renderer.AssureMaterialPresent(highlightMaterial);
                }
                else {
                    renderer.AssureMaterialAbsent(highlightMaterial);
                }


                //renderer.material = GetMaterialForBuilding(tile.Building, highlight);
                //renderer.material.color = Utilities.CreateColor(highlight ? 0x8EFFCE : 0x72C7DD);
            }
        }

        GameObject GetTileGameObject(TilePosition pos) {
            return transform.Find(TileName(pos)).gameObject;
        }

        GameObject GetTileGraphics(GameObject go) {
            return go.transform.GetChild(0).gameObject;
        }

        GameObject GetBuildingGraphics(GameObject go) {
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
            ground.Clear();

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
                        ground.Add(pos, tileGo);

                        if(++batchCount % BatchSize == 0) 
                            yield return null;
                    }
                }
            }
        }

        IEnumerator DrawBuildings() {
            GameObject prefab = null;
            buildings.Clear();
            buildingRenderers.Clear();

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
                        buildings.Add(lot.Building.Anchor, buildingGO);
                        buildingRenderers.Add(lot.Building.Anchor, GetBuildingGraphics(buildingGO).GetComponent<MeshRenderer>());

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

                        if (++batchCount % BatchSize == 0)
                            yield return null;
                    }
                }
            }
        }

        IEnumerator DrawLabels() {
            labels.Clear();
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
                    labels.Add(street.Name, labelGO);

                    if (++batchCount % BatchSize == 0)
                        yield return null;

                }
            }
        }

        IEnumerator PlaceItems() {
            foreach(Company company in gameController.Game.Companies) {
                foreach(Cable cable in company.Cables) {
                    PlaceItem(cable);

                    if (++batchCount % BatchSize == 0)
                        yield return null;
                }
                foreach(Node node in company.Nodes.Values) {
                    PlaceItem(node);

                    if (++batchCount % BatchSize == 0)
                        yield return null;
                }
            }
            yield return null;
        }

        private void PlaceItem(object item) {
            if(item is Cable) {
                Cable cable = item as Cable;
                GameObject go = InstantiateObject("Cable", cable.ID, TilePosition.Origin);

                UI.CableGraphics graphics = go.GetComponent<UI.CableGraphics>();
                graphics.Mode = UI.CableGraphics.GraphicsMode.Placed;
                graphics.Points = cable.Positions;
            }
            else if(item is Node) {
                Node node = item as Node;
                InstantiateObject("Node", node.ID, node.Position);
            }
        }

        private void RemoveItem(object item) {
            if(item is Cable) {
                Cable cable = item as Cable;
                RemoveObject("Cable", cable.ID);
            }
            else if (item is Node) {
                Node node = item as Node;
                RemoveObject("Node", node.ID);
            }
        }

        private GameObject InstantiateObject(string prefabName, string ID, TilePosition position) {
            GameObject prefab = prefabs[prefabName];

            GameObject go = Instantiate(prefab);
            go.name = prefabName + " " + ID;
            go.transform.position = PositionToWorld(position);
            go.transform.SetParent(this.transform, false);
            return go;
        }

        private void RemoveObject(string prefabName, string ID) {
            GameObject go = GameObject.Find(prefabName + " " + ID);
            if (go != null) {
                Destroy(go);
            }
        }

        private Material GetMaterialForBuilding(Building building, bool isSelected) {
            string materialName = "Blueprint";
            if (isSelected) {
                materialName = "Selection";
            }

            return materialController.GetMaterial(materialName);
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