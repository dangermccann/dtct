using UnityEngine;
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

        [HideInInspector]
        public SelectionModes Mode = SelectionModes.None;

        private GameObject cursorObject = null;
        private GameController gameController;
        private CableGraphics cableCursor;
        private TilePosition cablePlacementStart;
        private AStar pathfinder = null;

        private void Awake() {
            gameController = GameController.Get();
        }

        private void Start() {
            cameraController.TileClicked += CameraController_TileClicked;
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
                        boundingBox = gameController.Map.ExpandBoundingBox(boundingBox, 5);

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
                        } else {
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
                    }
                    else {
                        cursorObject.SetActive(false);
                    }
                } else if (Mode == SelectionModes.Destroy) {
                    cursorObject.transform.position = world;
                }
            }
        }

        private void CameraController_TileClicked(Vector3 position) {
            TilePosition tilePosition = ThreeDMap.WorldToPosition(position);

            if (!gameController.Map.Tiles.ContainsKey(tilePosition)) {
                return;
            }

            if (Mode == SelectionModes.Node) {
                if(!cursorObject.activeSelf) {
                    return;
                }

                gameController.Game.Player.PlaceNode(NodeType.Small, tilePosition);

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
                    cablePlacementStart = tilePosition;
                    cableCursor.Mode = CableGraphics.GraphicsMode.Placed;
                } else {
                    // Second click
                    // Finalize cable placement and create new graphic
                    gameController.Game.Player.PlaceCable(Model.CableType.Copper, cableCursor.Points);

                    Destroy(cursorObject);
                    CreateCursor();
                }
            } else if (Mode == SelectionModes.Destroy) {
                gameController.Game.Player.RemoveCablePosition(tilePosition);
                gameController.Game.Player.RemoveNode(tilePosition);
            }
        }
    }
}