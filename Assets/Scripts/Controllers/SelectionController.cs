using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using DCTC.Map;
using DCTC.UI;
using DCTC.Pathfinding;
using DCTC.Model;

namespace DCTC.Controllers {
    public class SelectionController : MonoBehaviour {

        public enum SelectionModes {
            None,
            Destroy,
            Cable,
            Node
        }

        public ThreeDCameraController cameraController;
        public ToggleGroup ConstructionToggleGroup;
        public GameObject DestroyPrefab;
        public GameObject CableCursorPrefab;
        public GameObject NodeCursorPrefab;
        public GameObject MapGameObject;
        public GameObject LotSelection;
        public GameObject CustomerDetails;

        [HideInInspector]
        public SelectionModes Mode = SelectionModes.None;

        private GameObject cursorObject = null;
        private GameController gameController;
        private CableGraphics cableCursor;
        private TilePosition cablePlacementStart;
        private AStar pathfinder = null;
        private ThreeDMap mapComponent;
        private Vector3 mouseDownPosition;
        private Cable dragCable;

        private void Awake() {
            gameController = GameController.Get();
        }

        private void Start() {
            mapComponent = MapGameObject.GetComponent<ThreeDMap>();
            cameraController.TileClicked += CameraController_TileClicked;
            cameraController.TileDragged += CameraController_TileDragged;
            LotSelection.SetActive(false);
            CustomerDetails.SetActive(false);
        }

        public void ValueChanged() {
            if (cursorObject != null) {
                DestroyImmediate(cursorObject);
                cursorObject = null;
            }

            if (ConstructionToggleGroup.AnyTogglesOn()) {
                string selected = ConstructionToggleGroup.ActiveToggles().First().name;
                Mode = (SelectionModes) Enum.Parse(typeof(SelectionModes), selected);

                CreateCursor();
            }
            else {
                Mode = SelectionModes.None;
            }

            mapComponent.HighlightRadius = 0;

            Debug.Log("Selection mode is " + Mode.ToString());
        }

        private void CreateCursor() {
            if (Mode == SelectionModes.Cable) {
                cursorObject = Instantiate(CableCursorPrefab);
                cableCursor = cursorObject.GetComponent<CableGraphics>();
            }
            else if (Mode == SelectionModes.Node) {
                cursorObject = Instantiate(NodeCursorPrefab);
            }
            else if(Mode == SelectionModes.Destroy) {
                cursorObject = Instantiate(DestroyPrefab);
                cursorObject.transform.SetParent(GameObject.Find("/Map/Canvas").transform, false);
            }
        }

        private void Update() {
            if (gameController.Map == null)
                return;

            bool ignoreMouse;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                ignoreMouse = true;
            else
                ignoreMouse = false;

            if (Input.GetMouseButtonDown(1)) {
                mouseDownPosition = Input.mousePosition;
            }
            if(Input.GetMouseButtonUp(1) && !ignoreMouse) {
                if((Input.mousePosition - mouseDownPosition).magnitude < 0.25f) {
                    ConstructionToggleGroup.SetAllTogglesOff();
                    LotSelection.SetActive(false);
                    CustomerDetails.SetActive(false);
                }
            }
            if(Input.GetMouseButtonUp(0)) {
                if(dragCable != null && dragCable.Positions.Count == 1) {
                    gameController.Game.Player.RemoveCable(dragCable);
                }
                dragCable = null;
            }

            if(Input.GetMouseButtonDown(0) && !ignoreMouse && Mode == SelectionModes.None) {
                Vector3 world = cameraController.MouseCursorInWorld();
                TilePosition pos = ThreeDMap.WorldToPosition(world);
                if (gameController.Map.Tiles.ContainsKey(pos)) {
                    var outline = LotSelection.GetComponent<LotOutline>();
                    var lot = gameController.Map.Tiles[pos].Lot;
                    if (lot != null) {
                        outline.Positions = lot.Tiles;
                        LotSelection.SetActive(true);
                        outline.Redraw();

                        CustomerDetails.SetActive(true);
                        CustomerDetails.GetComponent<CustomerDetails>().Customer = gameController.Game.FindCustomerByAddress(lot.Anchor);
                    }
                }
            }

            if(cursorObject != null) {
                // Snap to the tile
                Vector3 world = cameraController.MouseCursorInWorld();
                TilePosition pos = ThreeDMap.WorldToPosition(world);
                world = ThreeDMap.PositionToWorld(pos);

                // Hide if we're off the map
                if (!gameController.Map.Tiles.ContainsKey(pos)) {
                    cursorObject.SetActive(false);
                    return;
                }
                else {
                    cursorObject.SetActive(true);
                }

                if (Mode == SelectionModes.Cable) {
                    if (cableCursor.Mode == CableGraphics.GraphicsMode.Placed) {
                        // Placement mode
                        cursorObject.transform.position = Vector3.zero;

                        // Move destination to nearest road on map
                        pos = gameController.Map.NearestRoad(pos);
                        if(pos.x == -1) {
                            return;
                        }

                        // Find best path from start to current point
                        // Build bounding box to limit total nodes
                        TileRectangle boundingBox = MapConfiguration.BoundingBox(cablePlacementStart, pos);
                        boundingBox = gameController.Map.ExpandBoundingBox(boundingBox, 15);

                        // Generate list of nodes from the bounding box
                        List<IPathNode> nodes = new List<IPathNode>();
                        TilePosition p = new TilePosition();
                        for (int x = boundingBox.Left; x <= boundingBox.Right; x++) {
                            for (int y = boundingBox.Bottom; y <= boundingBox.Top; y++) {
                                p.x = x;
                                p.y = y;
                                nodes.Add(gameController.Map.Tiles[p]);
                            }
                        }
                        pathfinder = new AStar(nodes);

                        // Perform search
                        IList<IPathNode> results = pathfinder.Search(
                            gameController.Map.Tiles[cablePlacementStart],
                            gameController.Map.Tiles[pos]);

                        // Convert results to list of TilePositon 
                        if (results != null) {
                            List<TilePosition> points = new List<TilePosition>(results.Select(r => r.Position));
                            cableCursor.Valid = true;
                            cableCursor.Points = points;
                        }
                        else {
                            // No path found
                            cableCursor.Valid = false;
                        }
                    }
                    else {
                        // Cursor mode
                        cursorObject.transform.position = world;

                        Tile tile = gameController.Map.Tiles[pos];
                        if (tile.Type == TileType.Road) {
                            cableCursor.Valid = true;
                            if (tile.RoadType == RoadType.IntersectAll) {
                                cableCursor.Intersection = true;
                            } else {
                                cableCursor.Intersection = false;
                            }

                            if (tile.RoadType == RoadType.Horizontal) {
                                cableCursor.Orientation = Orientation.Horizontal;
                            } else {
                                cableCursor.Orientation = Orientation.Vertical;
                            }
                        } 
                        else if(tile.Type == TileType.Connector) {
                            cableCursor.Valid = true;
                        }
                        else {
                            // Not a road tile; disable selection
                            cableCursor.Valid = false;
                        }
                    }
                }
                else if (Mode == SelectionModes.Node) {
                    Tile tile = gameController.Map.Tiles[pos];
                    if (tile.Type == TileType.Road) {
                        cursorObject.SetActive(true);
                        cursorObject.transform.position = world;
                        mapComponent.HighlightRadius = new Node().ServiceRange;
                    }
                    else {
                        cursorObject.SetActive(false);
                        mapComponent.HighlightRadius = 0;
                    }
                } else if (Mode == SelectionModes.Destroy) {
                    cursorObject.transform.position = world;
                }
            }
        }

        private void CameraController_TileClicked(Vector3 world, TilePosition position) {
            if (!gameController.Map.Tiles.ContainsKey(position)) {
                return;
            }

            if (Mode == SelectionModes.Node) {
                if(!cursorObject.activeSelf) {
                    return;
                }

                gameController.Game.Player.PlaceNode(NodeType.Small, position);

                Destroy(cursorObject);
                CreateCursor();
            }
            else if (Mode == SelectionModes.Cable) {

                // Cables must start on a street
                if (cableCursor == null || cableCursor.Valid == false) {
                    return;
                }

                if (cableCursor.Mode == CableGraphics.GraphicsMode.Cursor) {
                    // First click
                    // Set variable so it draws preview in Update
                    cablePlacementStart = position;
                    cableCursor.Mode = CableGraphics.GraphicsMode.Placed;
                } else {
                    // Second click
                    // Finalize cable placement and create new graphic
                    if (cableCursor.Points.Count > 1) {
                        gameController.Game.Player.PlaceCable(Model.CableType.Copper, cableCursor.Points);
                    }

                    Destroy(cursorObject);
                    CreateCursor();
                }
            } else if (Mode == SelectionModes.Destroy) {
                gameController.Game.Player.RemoveCablePosition(position);
                gameController.Game.Player.RemoveNode(position);
            }
        }

        private void CameraController_TileDragged(Vector3 world, TilePosition position) {
            if (Mode == SelectionModes.Destroy) {
                gameController.Game.Player.RemoveCablePosition(position);
                gameController.Game.Player.RemoveNode(position);
            }
            else if(Mode == SelectionModes.Cable) {
                Tile tile = gameController.Map.Tiles[position];
                if (tile.Type != TileType.Road && tile.Type != TileType.Connector)
                    return;

                if (cableCursor.Mode != CableGraphics.GraphicsMode.Cursor)
                    return;

                if(dragCable == null) {
                    dragCable = gameController.Game.Player.PlaceCable(CableType.Copper, 
                        new List<TilePosition>() { position });
                }
                else {
                    if(!dragCable.Positions.Contains(position)) {
                        if(dragCable.Positions[0].IsAdjacent(position)) {
                            gameController.Game.Player.PrependCable(dragCable, position);
                        }
                        else if (dragCable.Positions.Last().IsAdjacent(position)) {
                            gameController.Game.Player.AppendCable(dragCable, position);
                        }
                    }
                }
            }
        }
    }
}