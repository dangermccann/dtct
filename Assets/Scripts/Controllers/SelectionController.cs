﻿using UnityEngine;
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
            Cable,
            Node,
            Selected
        }

        public enum CablePlacementMode {
            Cursor,
            Placed
        }

        public event ChangeDelegate SelectionChanged;
        public ThreeDCameraController cameraController;
        public ToggleGroup ConstructionToggleGroup;
        public GameObject MapGameObject;
        public GameObject LotSelection;
        public GameObject LocationDetails;
        public CableGraphics cableGraphics;
        public GameObject invalidNode;

        public TilePosition SelectedPosition {
            get { return selectedPosition; }
        }


        [HideInInspector]
        public SelectionModes Mode = SelectionModes.None;

        [HideInInspector]
        public string NodeId;

        [HideInInspector]
        public string CableId;

        
        private GameController gameController;
        private TilePosition cablePlacementStart;
        private ThreeDMap mapComponent;
        private Vector3 mouseDownPosition;
        private CablePlacementMode placementMode = CablePlacementMode.Cursor;
        private TilePosition lastPosition = TilePosition.Origin;
        private TilePosition selectedPosition = TilePosition.Origin;


        private void Awake() {
            gameController = GameController.Get();
        }

        private void Start() {
            mapComponent = MapGameObject.GetComponent<ThreeDMap>();
            cameraController.TileClicked += CameraController_TileClicked;
            LotSelection.SetActive(false);
        }

        public void SetSelection(string mode) {
            SetSelection((SelectionModes)Enum.Parse(typeof(SelectionModes), mode));
        }

        public void SetSelection(SelectionModes mode) {
            invalidNode.SetActive(false);

            Mode = mode;

            if (Mode != SelectionModes.None) {
                InitSelection();
            } else {
                LotSelection.SetActive(false);
                cableGraphics.UnhighlightPole(selectedPosition);
                selectedPosition = TilePosition.Origin;
            }

            if (Mode != SelectionModes.Cable)
                cableGraphics.CancelSelection();

            if(Mode == SelectionModes.Node || Mode == SelectionModes.Cable)
                mapComponent.OverlayMode = OverlayMode.ServiceArea;
            else
                mapComponent.OverlayMode = OverlayMode.Customers;

            SelectionChanged?.Invoke();

            Debug.Log("Selection mode is " + Mode.ToString());
        }

        private void InitSelection() {
            if (Mode == SelectionModes.Cable) {
                placementMode = CablePlacementMode.Cursor;
                cableGraphics.InitSelection();
                cableGraphics.SelectionCable = new Cable(CableId, gameController.Game.Items.CableAttributes[CableId]);
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
                if((Input.mousePosition - mouseDownPosition).magnitude < 0.5f) {
                    ConstructionToggleGroup.SetAllTogglesOff();
                    SetSelection(SelectionModes.None);
                }
            }


            Vector3 world = cameraController.MouseCursorInWorld();
            TilePosition pos = ThreeDMap.WorldToPosition(world);

            if(!ignoreMouse)
                LocationDetails.GetComponent<LocationDetails>().Location = pos;


            if (Input.GetMouseButtonDown(0) && !ignoreMouse && Mode == SelectionModes.None) {
                if (gameController.Map.Tiles.ContainsKey(pos)) {
                    var outline = LotSelection.GetComponent<LotOutline>();
                    var lot = gameController.Map.Tiles[pos].Lot;
                    if (lot != null) {
                        outline.Positions = lot.Tiles;
                        LotSelection.SetActive(true);
                        outline.Redraw();
                        SetSelection(SelectionModes.Selected);
                    }
                }
            }

            // Snap to the tile
            world = ThreeDMap.PositionToWorld(pos);

                

            if (Mode == SelectionModes.Cable) {
                if (placementMode == CablePlacementMode.Placed) {
                    // Placement mode
                    // Move destination to nearest valid location
                    pos = gameController.Map.NearestPoleLocation(pos);

                    if (!gameController.Map.IsInBounds(pos)) {
                        return;
                    }

                    if (pos.Equals(lastPosition))
                        return;

                    lastPosition = pos;

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
                    AStar pathfinder = new AStar(nodes);

                    // Perform search
                    IList<IPathNode> results = pathfinder.Search(
                        gameController.Map.Tiles[cablePlacementStart],
                        gameController.Map.Tiles[pos]);


                    cableGraphics.RemoveSelection();

                    // Convert results to list of TilePositon 
                    if (results != null) {
                        List<TilePosition> points = new List<TilePosition>(results.Select(r => r.Position));
                        cableGraphics.SelectionCable.Positions = points;
                    }
                    else {
                        // No path found
                        cableGraphics.SelectionCable.Positions = new List<TilePosition>();
                    }

                    cableGraphics.RedrawSelection();
                }
                else {
                    // Cursor mode
                    cableGraphics.UpdateSelection(pos);
                }
            }
            else if (Mode == SelectionModes.Node) {
                // Hide if we're off the map
                if (!gameController.Map.Tiles.ContainsKey(pos)) {
                    invalidNode.SetActive(true);
                    return;
                } 

                // Make sure we're on top of one of our cables
                Tile tile = gameController.Map.Tiles[pos];
                bool valid = false;

                if (tile.Type == TileType.Road) {
                    if (gameController.Game.Player.CablePositions.Contains(pos)) {
                        pos = gameController.Map.NearestPoleLocation(pos);

                        if (gameController.Map.IsValidPoleLocation(pos)) {

                            if (pos != selectedPosition)
                                cableGraphics.UnhighlightPole(selectedPosition);

                            selectedPosition = pos;
                            cableGraphics.HighlightPole(selectedPosition);
                            invalidNode.SetActive(false);
                            valid = true;
                        }

                    }
                }

                if(!valid) {
                    cableGraphics.UnhighlightPole(selectedPosition);
                    invalidNode.SetActive(true);
                } 
            }
        }

        private void CameraController_TileClicked(Vector3 world, TilePosition position) {
            if (!gameController.Map.Tiles.ContainsKey(position)) {
                return;
            }
            if (Mode == SelectionModes.None || Mode == SelectionModes.Selected) {
                cableGraphics.UnhighlightPole(selectedPosition);

                if(cableGraphics.HighlightPole(position)) {
                    selectedPosition = position;
                    SetSelection(SelectionModes.Selected);
                }
                else {
                    selectedPosition = TilePosition.Origin;
                    SetSelection(SelectionModes.None);
                }

                return;
            }

            if (Mode == SelectionModes.Node) {
                if(invalidNode.activeSelf) {
                    return;
                }

                position = gameController.Map.NearestPoleLocation(position);
                gameController.Game.Player.PlaceNode(NodeId, position);
                cableGraphics.UnhighlightPole(selectedPosition);
            }
            else if (Mode == SelectionModes.Cable) {

                if (!gameController.Map.IsInBounds(position)) {
                    return;
                }

                if (placementMode == CablePlacementMode.Cursor) {
                    // First click
                    // Set variable so it draws preview in Update
                    position = gameController.Map.NearestPoleLocation(position);

                    cablePlacementStart = position;
                    placementMode = CablePlacementMode.Placed;
                } else {
                    // Second click
                    // Finalize cable placement and create new graphic
                    if (cableGraphics.SelectionCable.Positions.Count > 1) {
                        gameController.Game.Player.PlaceCable(CableId, cableGraphics.SelectionCable.Positions);
                    }

                    cableGraphics.CancelSelection();
                    InitSelection();
                }
            } 
        }
    }
}