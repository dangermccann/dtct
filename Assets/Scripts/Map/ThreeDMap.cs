using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.Map {
    public enum OverlayMode {
        Customers,
        ServiceArea
    }

    public class ThreeDMap : AbstractMap {

        public ThreeDCameraController cameraController;

        public delegate void TileSelectEventHandler(Tile tile);
        public event GameEvent OverlayModeChanged;
        public event GameEvent DrawComplete;

        public bool lablesVisible = false;

        private MapConfiguration map;
        private GameObject canvas;
        private CableGraphics cableGraphics;
        private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        private Dictionary<TilePosition, GameObject> buildings = new Dictionary<TilePosition, GameObject>();
        private Dictionary<string, GameObject> labels = new Dictionary<string, GameObject>();
        private Dictionary<TilePosition, GameObject> ground = new Dictionary<TilePosition, GameObject>();
        private MaterialController materialController;
        private GameController gameController;
        private Dictionary<TilePosition, MeshRenderer> buildingRenderers = new Dictionary<TilePosition, MeshRenderer>();
        private Dictionary<string, HashSet<TilePosition>> colorizedBuildings = new Dictionary<string, HashSet<TilePosition>>();

        private int batchCount = 0;

        private const float WorldUnitsPerTile = 4f;
        private const string MapPrefabsLocation = "Prefabs/Map";
        private const bool centerQuadTiles = true;
        private const bool skipRoadsAdjacentToCorners = true;
        private const float quadScaleReduction = 0.5f;

        private const int BatchSize = 100;
        private const string OverlayKey = "Overlay";
        private const string SelectionKey = "Selection";

        void Start() {
            LoadPrefabs();
            materialController = MaterialController.Get();
            canvas = transform.GetChild(0).gameObject;
            cableGraphics = transform.GetChild(1).GetComponent<CableGraphics>();

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
            GameObject[] all = Resources.LoadAll<GameObject>(MapPrefabsLocation);
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

        private OverlayMode overlayMode = OverlayMode.Customers;
        public OverlayMode OverlayMode {
            get { return overlayMode; }
            set {
                overlayMode = value;
                if (OverlayModeChanged != null)
                    OverlayModeChanged();

                RedrawOverlay();
            }
        }


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
            colorizedBuildings.Clear();
            map = null;
        }

        public override void Init(MapConfiguration config) {
            map = config;

            if (!colorizedBuildings.ContainsKey(SelectionKey)) {
                colorizedBuildings.Add(SelectionKey, new HashSet<TilePosition>());
                colorizedBuildings.Add(OverlayKey, new HashSet<TilePosition>());
            }

            StartDraw();
            cameraController.ResetToDefault();
        }

        public override void StartDraw() {
            StartCoroutine(Redraw());
        }

        public override void MoveCameraTo(TilePosition position) {
            cameraController.MoveCamera(PositionToWorld(position));
        }



        // TODO: move this
        private bool serviceAreadChanged = false;
        private ConcurrentQueue<object> placedItems = new ConcurrentQueue<object>();
        private ConcurrentQueue<object> removedItems = new ConcurrentQueue<object>();
        private ConcurrentQueue<Customer> changedCustomers = new ConcurrentQueue<Customer>();

        IEnumerator Redraw() {
            batchCount = 0;

            yield return StartCoroutine( DrawGround() );
            yield return StartCoroutine( DrawBuildings() );
            yield return StartCoroutine( DrawLabels() );
            yield return StartCoroutine( PlaceItems() );

            RedrawOverlay();

            if (gameController.Game != null) {
                foreach (Company company in gameController.Game.Companies) {
                    company.ItemAdded += (obj) => placedItems.Enqueue(obj);
                    company.ItemRemoved += (obj) => removedItems.Enqueue(obj);
                    HighlightBuilding(company.HeadquartersLocation, company.Color, 0.50f);
                }

                gameController.Game.Player.ServiceAreaChanged += () => serviceAreadChanged = true;
                gameController.Game.CustomerChanged += (customer, company) => changedCustomers.Enqueue(customer);

                if (DrawComplete != null)
                    DrawComplete();
                gameController.OnMapDrawComplete();

                NavigateCameraTo(gameController.Game.Player.HeadquartersLocation);
            }
        }


        public void NavigateCameraTo(TilePosition pos) {
            Tile tile = map.Tiles[pos];
            Vector3 world = PositionToWorld(pos);

            int tileCount = 1;
            if (tile.Lot != null)
                tileCount = tile.Lot.Tiles.Count;
            float distance = Mathf.Lerp(15, 60, tileCount / 30f);

            Direction cameraFacing = Direction.North;

            cameraController.FocusOnPosition(world, cameraFacing, distance);
        }

        void Update() {
            if (gameController.Game == null || map == null)
                return;


            HashSet<TilePosition> removals = new HashSet<TilePosition>();
            removals.AddMany(colorizedBuildings[SelectionKey]);

            if (HighlightRadius > 0) {
                TilePosition mousePosition = WorldToPosition(cameraController.MouseCursorInWorld());
                for (int x = mousePosition.x - HighlightRadius; x <= mousePosition.x + HighlightRadius; x++) {
                    for (int y = mousePosition.y - HighlightRadius; y <= mousePosition.y + HighlightRadius; y++) {
                        TilePosition pos = new TilePosition(x, y);
                        if (map.IsInBounds(pos)) {
                            HighlightBuilding(pos, gameController.Game.Player.Color, 0.5f);

                            if (removals.Contains(pos))
                                removals.Remove(pos);

                            if (!colorizedBuildings[SelectionKey].Contains(pos))
                                colorizedBuildings[SelectionKey].Add(pos);
                        }
                    }
                }
            }

            foreach(TilePosition removal in removals) {
                if (!colorizedBuildings[OverlayKey].Contains(removal))
                    RedrawBuildingOverlay(removal);
            }
            

            colorizedBuildings[SelectionKey].RemoveMany(removals);

            // Queued operations that respond to events
            if(serviceAreadChanged) {
                OnServiceAreaChanged();
                serviceAreadChanged = false;
            }

            // Remove items before placing new ones in case the same item was removed and re-added 
            object item;
            if (removedItems.TryDequeue(out item)) {
                RemoveItem(item);
            }

            item = null;
            if (placedItems.TryDequeue(out item)) {
                PlaceItem(item);
            }


            Customer customer;
            if(changedCustomers.TryDequeue(out customer)) {
                CustomerChanged(customer);
            }
        }

        private void OnServiceAreaChanged() {
            if (OverlayMode == OverlayMode.ServiceArea)
                DrawServiceArea();
        }

        private void CustomerChanged(Customer customer) {
            if (OverlayMode == OverlayMode.Customers)
                RedrawBuildingOverlay(customer.HomeLocation);
        }

        private void RedrawOverlay() {
            ClearBuildingOverlay(OverlayKey);

            switch (overlayMode) {
                case OverlayMode.Customers:
                    DrawCustomers();
                    break;
                case OverlayMode.ServiceArea:
                    DrawServiceArea();
                    break;
            }
        }

        private void RedrawBuildingOverlay(TilePosition pos) {
            Tile tile = map.Tiles[pos];
            Company player = gameController.Game.Player;

            if (tile.Building != null) {
                Color color = Color.clear;
                float intensity = 0;

                switch(overlayMode) {
                    case OverlayMode.Customers:
                        Customer customer = gameController.Game.FindCustomerByAddress(pos);
                        if(customer != null) {
                            switch(customer.Status) {
                                case CustomerStatus.Pending:
                                    color = Color.white;
                                    intensity = 0.25f;
                                    break;
                                case CustomerStatus.Outage:
                                    color = Color.red;
                                    intensity = 0.5f;
                                    break;
                                case CustomerStatus.Subscribed:
                                    color = player.Color;
                                    intensity = 0.5f;
                                    break;
                            }
                        }
                        break;

                    case OverlayMode.ServiceArea:
                        if(player.ServiceArea.Contains(pos)) {
                            color = player.Color;
                            intensity = 0.5f;
                        }
                        break;
                }

                HighlightBuilding(pos, color, intensity);
            }
        }

        private void DrawServiceArea() {
            if (gameController.Game == null)
                return;

            Company player = gameController.Game.Player;
            UpdateBuildingOverlays(OverlayKey, player.ServiceArea, player.Color, 0.5f);
        }

        private void DrawCustomers() {
            if (gameController.Game == null)
                return;

            Company player = gameController.Game.Player;
            foreach(TilePosition pos in player.CustomerHouses) {
                RedrawBuildingOverlay(pos);
                colorizedBuildings[OverlayKey].Add(pos);
            }
        }

        private void UpdateBuildingOverlays(string key, IEnumerable<TilePosition> positions, Color color, float intensity) {
            HashSet<TilePosition> buildings = new HashSet<TilePosition>();
            buildings.AddMany(colorizedBuildings[key]);

            foreach (TilePosition pos in positions) {
                Tile tile = map.Tiles[pos];
                if (tile.Building != null) {
                    if (!colorizedBuildings[key].Contains(tile.Position)) {
                        colorizedBuildings[key].Add(tile.Position);
                        HighlightBuilding(pos, color, intensity);
                    }

                    buildings.Remove(tile.Position);
                }
            }

            foreach (TilePosition pos in buildings) {
                HighlightBuilding(pos, Color.clear, 0);
                colorizedBuildings[key].Remove(pos);
            }
        }

        private void ClearBuildingOverlay(string key) {
            foreach (TilePosition pos in colorizedBuildings[key]) {
                HighlightBuilding(pos, Color.clear, 0);
            }
            colorizedBuildings[key].Clear();
        }

        private void OnTileClicked(Vector3 world, TilePosition pos) {

            if (map.Tiles.ContainsKey(pos)) {
                Tile tile = map.Tiles[pos];
                if(tile.Building != null) {
                    Debug.Log(BuildingName(pos) + " " + tile.Building.Type.ToString());
                }
                else {
                    if(tile.Type == TileType.Road) {
                        Debug.Log(TileName(pos) + " " + map.FindStreet(pos)?.Name);
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

        void HighlightBuildings(IEnumerable<TilePosition> positions, Color color, float intensity) {
            foreach (TilePosition pos in positions) {
                HighlightBuilding(pos, color, intensity);
            }
        }

        void HighlightBuilding(TilePosition pos, Color color, float intensity) {

            Tile tile = map.Tiles[pos];
            if (tile.Building != null) {
                MeshRenderer renderer = buildingRenderers[tile.Building.Anchor];

                foreach(Material material in renderer.materials) {
                    if(color != Color.clear) {
                        material.SetColor("_EmissionColor", 
                            new Vector4(color.r, color.g, color.b, 0) * intensity);
                        material.EnableKeyword("_EMISSION");
                    }
                    else {
                        material.DisableKeyword("_EMISSION");
                    }
                }
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
                    Vector3 world = Vector3.zero;
                    Tile tile = map.Tiles[pos];
                    Vector3 scale = Vector3.one;

                    if (tile.Type == TileType.Road) {


                        // Skip horizontal and vertical roads that are adjacent to corners and intersections
                        bool skip = false;
                        if (skipRoadsAdjacentToCorners) {
                            
                            if (tile.RoadType == RoadType.Horizontal || tile.RoadType == RoadType.Vertical) {
                                List<TilePosition> adjacents = map.AdjacentPositions(pos);
                                foreach (TilePosition adjacent in adjacents) {
                                    Tile at = map.Tiles[adjacent];
                                    if (at.Type == TileType.Road &&
                                        at.RoadType != RoadType.Horizontal &&
                                        at.RoadType != RoadType.Vertical) {
                                        skip = true;
                                        break;
                                    }
                                }

                            }
                        }

                        if (!skip) {
                            string prefabName = "Road" + tile.RoadType.ToString();
                            if (!prefabs.ContainsKey(prefabName)) {
                                Debug.LogWarning("Prefab " + prefabName + " not found");
                                continue;
                            }
                                

                            prefab = prefabs[prefabName];
                            world = PositionToWorld(pos);

                            if (tile.RoadType != RoadType.Horizontal && tile.RoadType != RoadType.Vertical) {
                                world = new Vector3(world.x, Random.Range(-0.003f, 0.003f), world.z);
                            }
                        }

                    }  
                    else {
                        // Optimization to stretch quad 
                        if(!completedQuads.ContainsKey(pos)) {
                            prefab = prefabs["Quad"];
                            int quadZ = z + 1;
                            int quadX = x + 1;
                            Tile quadTile;

                            while(quadZ < map.Height) {
                                quadTile = map.Tiles[new TilePosition(x, quadZ)];
                                if(quadTile.Type == TileType.Road) {
                                    break;
                                }
                                else {
                                    quadZ++;
                                }
                            }

                            while (quadX < map.Width) {
                                quadTile = map.Tiles[new TilePosition(quadX, z)];
                                if (quadTile.Type == TileType.Road) {
                                    break;
                                } else {
                                    quadX++;
                                }
                            }

                            for(int qx = x; qx < quadX; qx++) {
                                for(int qz = z; qz < quadZ; qz++) {
                                    completedQuads.Add(new TilePosition(qx, qz), prefab);
                                }
                            }

                            int dx = (quadX - x);
                            int dz = (quadZ - z);
                            scale.x = dx * WorldUnitsPerTile / 2 - quadScaleReduction;
                            scale.z = dz * WorldUnitsPerTile / 2 - quadScaleReduction;

                            // Place quad in center
                            if (centerQuadTiles)
                                world = PositionToWorld(new Vector2(x + (dx - 1) / 2f, z + (dz - 1) / 2f));
                            else
                                world = PositionToWorld(pos);
                        }
                        
                    }

                    if (prefab != null) {
                        GameObject tileGo = Instantiate(prefab);
                        tileGo.name = TileName(pos);
                        tileGo.transform.SetParent(this.transform, false);
                        tileGo.transform.position = world;
                        tileGo.transform.localScale = scale;
                        ground.Add(pos, tileGo);

                        if(++batchCount % BatchSize == 0) 
                            yield return null;
                    }

                    if (tile.Type == TileType.Connector && gameController.Game.Player.Headquarters.Contains(pos)) {
                        prefab = prefabs["ConnectorTile"];
                        GameObject tileGo = Instantiate(prefab);
                        tileGo.name = TileName(pos);
                        tileGo.transform.SetParent(this.transform, false);
                        world = PositionToWorld(pos);
                        tileGo.transform.position = new Vector3(world.x, 0.01f, world.z);
                    }
                }
            }
        }

        string LotPrefabName(Lot lot) {
            string str = "";
            str += lot.Building.Type.ToString() + "-";
            str += lot.Building.FacingDirection.ToString().Substring(0, 1) + "-";
            str += lot.Building.BlockPosition + "-";
            str += lot.Building.Width.ToString() + "x" + lot.Building.Height.ToString() + "-";
            str += lot.Building.Variation.ToString();
            return str;
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
                        string prefabName = LotPrefabName(lot);

                        if (!prefabs.ContainsKey(prefabName)) {
                            Debug.LogWarning("Missing prefab: " + prefabName);
                            continue;
                        }

                        prefab = prefabs[prefabName];
                        GameObject buildingGO = Instantiate(prefab);
                        buildingGO.name = BuildingName(lot.Building.Anchor);
                        buildingGO.transform.SetParent(this.transform, false);
                        buildingGO.SetActive(true);

                        Vector2 pos = new Vector2(lot.Anchor.x + (lot.Building.Width - 1) / 2f, lot.Anchor.y + (lot.Building.Height - 1) / 2f);
                        buildingGO.transform.position = PositionToWorld(pos);
                        buildings.Add(lot.Building.Anchor, buildingGO);
                        buildingRenderers.Add(lot.Building.Anchor, GetBuildingGraphics(buildingGO).GetComponent<MeshRenderer>());

                        if(lot.Building.Color != null) {
                            Material material = materialController.GetMaterial(lot.Building.Type.ToString() + lot.Building.Color);
                            GetBuildingGraphics(buildingGO).GetComponent<MeshRenderer>().material = material;
                        }

                        if (++batchCount % BatchSize == 0)
                            yield return null;
                    }
                }
            }
        }

        IEnumerator DrawLabels() {
            labels.Clear();

            if (lablesVisible) {
                int index = 0;

                foreach (Street street in map.Streets) {

                    Segment segment = street.Segments[0];
                    Vector2 length = segment.End.AsVector() - segment.Start.AsVector();

                    if (length.magnitude < 3)
                        continue;


                    Vector2 pos;
                    if (index % 3 == 0)
                        pos = new Vector2(segment.Start.x + length.x / 3f, segment.Start.y + length.y / 3f);
                    else if (index % 3 == 1)
                        pos = new Vector2(segment.Start.x + length.x / 2f, segment.Start.y + length.y / 2f);
                    else
                        pos = new Vector2(segment.Start.x + length.x / 1.5f, segment.Start.y + length.y / 1.5f);

                    GameObject labelGO = Instantiate(prefabs["RoadLabel"]);
                    labelGO.name = "Label (" + street.Name + ")";
                    labelGO.transform.SetParent(canvas.transform, false);

                    Vector3 world = PositionToWorld(pos);
                    labelGO.transform.position = new Vector3(world.x, world.y + 0.05f, world.z); ;

                    if (segment.Orientation == Orientation.Vertical) {
                        labelGO.transform.Rotate(Vector3.up, 90, Space.World);
                    }

                    labelGO.GetComponent<TextMeshPro>().text = street.Name;
                    labels.Add(street.Name, labelGO);

                    index++;

                    if (++batchCount % BatchSize == 0)
                        yield return null;

                }
            }
        }

        IEnumerator PlaceItems() {
            if (gameController.Game == null)
                yield break;

            foreach(Company company in gameController.Game.Companies) {
                foreach(Cable cable in company.Cables) {
                    PlaceItem(cable);

                    if (++batchCount % BatchSize == 0)
                        yield return null;
                }
                foreach(Node node in company.Nodes) {
                    PlaceItem(node);

                    if (++batchCount % BatchSize == 0)
                        yield return null;
                }
                foreach(Truck truck in company.Trucks) {
                    PlaceItem(truck);

                    if (++batchCount % BatchSize == 0)
                        yield return null;
                }
            }
            yield return null;
        }

        private void PlaceItem(object item) {
            if(item is Cable) {
                Cable cable = item as Cable;
                cableGraphics.DrawCable(cable);
            }
            else if(item is Node) {
                Node node = item as Node;
                cableGraphics.DrawNode(node);
            }
            else if(item is Truck) {
                Truck truck = item as Truck;
                GameObject go = InstantiateObject("Van", truck.ID, truck.TilePosition);
                go.GetComponent<TruckGraphics>().Truck = truck;
            }
        }

        public void PlaceCable(Cable cable) {
            PlaceItem(cable);
        }

        private void RemoveItem(object item) {
            if(item is Cable) {
                Cable cable = item as Cable;
                cableGraphics.RemoveCable(cable);
            }
            else if (item is Node) {
                Node node = item as Node;
                cableGraphics.RemoveNode(node);
            } else if (item is Truck) {
                Truck truck = item as Truck;
                RemoveObject("Van", truck.ID);
            }
        }

        private GameObject InstantiateObject(string prefabName, string ID, TilePosition position) {
            if(!prefabs.ContainsKey(prefabName)) {
                Debug.LogWarning("Can not find prefab " + prefabName);
                return null;
            }

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
            else {
                Debug.LogWarning("Could not find " + prefabName + " " + ID);
            }
        }

        public static Vector3 PositionToWorld(TilePosition pos) {
            return PositionToWorld(pos.AsVector());
        }

        public static Vector3 PositionToWorld(Vector2 pos) {
            return new Vector3(pos.x * WorldUnitsPerTile, 0, pos.y * WorldUnitsPerTile);
        }

        public static TilePosition WorldToPosition(Vector3 world) {
            return new TilePosition(Mathf.FloorToInt(world.x / WorldUnitsPerTile), Mathf.FloorToInt(world.z / WorldUnitsPerTile));
        }

        
    } 
}