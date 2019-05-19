using System;
using System.Collections.Generic;
using DCTC.Model;
using DCTC.Pathfinding;

namespace DCTC.Map {

	public class MapConfiguration {
		public static TileType DefaultTileType = TileType.Grass;
		public static int DefaultMovementCost = 1000000;
        public static int BuildingMovementCost = 1000000;
        public static int RoadMovementCost = 1;

		public int Width;
		public int Height;
		public Dictionary<TilePosition, Tile> Tiles;
		public List<Neighborhood> Neighborhoods;
		public HashSet<Street> Streets = new HashSet<Street>();

		public MapConfiguration() : this(0, 0) { }

		public MapConfiguration(int width, int height) {
			Width = width;
			Height = height;
			Tiles = new Dictionary<TilePosition, Tile>();
			Neighborhoods = new List<Neighborhood>();
		}

		public void CreateTiles() {
			for(int x = 0; x < Width; x++) {
				for(int y = 0; y < Height; y++) {
					TilePosition position = new TilePosition(x, y);
					Tiles.Add(position, new Tile(position, DefaultTileType));
				}
			}
		}

		public void InvalidateRoads(Tile tile) {
			InvalidateRoads(tile.Position);
		}

		public void InvalidateRoads(TilePosition position) {
			Tile tile = Tiles[position];
			Tile north, south, east, west;
			LoadAdjacentTiles(position, out north, out south, out east, out west);
			
			if(HasRoad(north) && HasRoad(south) && HasRoad(east) && HasRoad(west)) {
				tile.RoadType = RoadType.IntersectAll;
				return;
			}
			
			if(HasRoad(north) && HasRoad(south) && !HasRoad(east) && HasRoad(west)) {
				tile.RoadType = RoadType.IntersectW;
				return;
			}
			
			if(HasRoad(north) && HasRoad(south) && HasRoad(east) && !HasRoad(west)) {
				tile.RoadType = RoadType.IntersectE;
				return;
			}
			
			if(HasRoad(north) && !HasRoad(south) && HasRoad(east) && HasRoad(west)) {
				tile.RoadType = RoadType.IntersectN;
				return;
			}
			
			if(!HasRoad(north) && HasRoad(south) && HasRoad(east) && HasRoad(west)) {
				tile.RoadType = RoadType.IntersectS;
				return;
			}
			
			if(HasRoad(north) && !HasRoad(south) && HasRoad(east) && !HasRoad(west)) {
				tile.RoadType = RoadType.CornerNE;
				return;
			}
			
			if(HasRoad(north) && !HasRoad(south) && !HasRoad(east) && HasRoad(west)) {
				tile.RoadType = RoadType.CornerNW;
				return;
			}
			
			if(!HasRoad(north) && HasRoad(south) && HasRoad(east) && !HasRoad(west)) {
				tile.RoadType = RoadType.CornerSE;
				return;
			}
			
			if(!HasRoad(north) && HasRoad(south) && !HasRoad(east) && HasRoad(west)) {
				tile.RoadType = RoadType.CornerSW;
				return;
			}

			if(HasRoad(north) || HasRoad(south)) {
				tile.RoadType = RoadType.Vertical;
				return;
			}

			tile.RoadType = RoadType.Horizontal;
		}

		public void AddRoad(TilePosition position) {
			Tile tile = Tiles[position];
			tile.Type = TileType.Road;
			tile.MovementCost = RoadMovementCost;

			InvalidateRoads(tile);

			Tile north, south, east, west;
			LoadAdjacentTiles(position, out north, out south, out east, out west);
			if(north != null) InvalidateRoads(north);
			if(south != null) InvalidateRoads(south);
			if(east != null) InvalidateRoads(east);
			if(west != null) InvalidateRoads(west);
			
		}

		public void LoadAdjacentTiles(TilePosition pos, out Tile north, out Tile south, out Tile east, out Tile west) {
			TilePosition northPos = North(pos);
			TilePosition southPos = South(pos);
			TilePosition eastPos  = East(pos);
			TilePosition westPos  = West(pos);

			north = IsInBounds(northPos) ? Tiles[northPos] : null;
			south = IsInBounds(southPos) ? Tiles[southPos] : null;
			east  = IsInBounds(eastPos)  ? Tiles[eastPos] : null;
			west  = IsInBounds(westPos)  ? Tiles[westPos] : null;
		}

		public bool IsInBounds(TilePosition pos) {
			return pos.x >= 0 && 
				   pos.y >= 0 && 
				   pos.x < Width && 
				   pos.y < Height;
		}

        public TileRectangle ExpandBoundingBox(TileRectangle rect, int amount) {
            return new TileRectangle(
                Math.Max(rect.Left - amount, 0),
                Math.Max(rect.Bottom - amount, 0),
                Math.Min(rect.Right + amount, Width - 1),
                Math.Min(rect.Top + amount, Height - 1)
            );
        }

		public List<Direction> AdjacentRoads(Tile tile) {
			return AdjacentRoads(tile.Position);
		}


		public List<Direction> AdjacentRoads(TilePosition pos) {
			List<Direction> result = new List<Direction>();

			TilePosition north = North(pos);
			TilePosition south = South(pos);
			TilePosition east  = East(pos);
			TilePosition west  = West(pos);

			if(IsInBounds(north) && HasRoad(Tiles[north]))
			   result.Add(Direction.North);

			if(IsInBounds(south) && HasRoad(Tiles[south]))
				result.Add(Direction.South);

			if(IsInBounds(east) && HasRoad(Tiles[east]))
				result.Add(Direction.East);

			if(IsInBounds(west) && HasRoad(Tiles[west]))
				result.Add(Direction.West);
			
			return result;
		}

		public List<TilePosition> AdjacentPositions(TilePosition pos) {
			List<TilePosition> tiles = new List<TilePosition>();

			TilePosition candidate = North(pos);
			if(IsInBounds(candidate))
				tiles.Add(candidate);

			candidate = South(pos);
			if(IsInBounds(candidate))
				tiles.Add(candidate);

			candidate = East(pos);
			if(IsInBounds(candidate))
				tiles.Add(candidate);

			candidate = West(pos);
			if(IsInBounds(candidate))
				tiles.Add(candidate);

			return tiles;
		}

		public List<TilePosition> AdjacentPositions(TilePosition pos, Orientation orientation) {
			List<TilePosition> tiles = new List<TilePosition>();
			TilePosition candidate;

			if(orientation == Orientation.Vertical) {
				candidate = North(pos);
				if(IsInBounds(candidate))
					tiles.Add(candidate);
				
				candidate = South(pos);
				if(IsInBounds(candidate))
					tiles.Add(candidate);
			}

			if(orientation == Orientation.Horizontal) {
				candidate = East(pos);
				if(IsInBounds(candidate))
					tiles.Add(candidate);
				
				candidate = West(pos);
				if(IsInBounds(candidate))
					tiles.Add(candidate);
			}
			
			return tiles;
		}

		public Street FindStreet(TilePosition position) {
			foreach(Street street in Streets) {
                if (street.Contains(position))
                    return street;
			}
			return null;
		}

		public Neighborhood FindNeighborhood(TilePosition position) {
			foreach(Neighborhood n in Neighborhoods) {
				if(n.IsInside(position)) {
					return n;
				}
			}
			return null;
		}

        public TilePosition NearestRoad(TilePosition position) {
            if (HasRoad(position))
                return position;

            bool bail = false;
            int count = 5;

            TilePosition north = position;
            TilePosition south = position;
            TilePosition east  = position;
            TilePosition west  = position;

            while (!bail) {
                east  = East(east);
                if (HasRoad(east)) return east;

                west  = West(west);
                if (HasRoad(west)) return west;

                north = North(north);
                if (HasRoad(north)) return north;

                south = South(south);
                if (HasRoad(south)) return south;

                count--;

                if (count < 0)
                    bail = true;
            }

            return new TilePosition(-1, -1);
        }

        public TilePosition NearestPoleLocation(TilePosition position) {
            if (IsValidPoleLocation(position))
                return position;

            bool bail = false;
            int count = 5;

            TilePosition north = position;
            TilePosition south = position;
            TilePosition east = position;
            TilePosition west = position;

            while (!bail) {
                east = East(east);
                if (IsValidPoleLocation(east)) return east;

                west = West(west);
                if (IsValidPoleLocation(west)) return west;

                north = North(north);
                if (IsValidPoleLocation(north)) return north;

                south = South(south);
                if (IsValidPoleLocation(south)) return south;

                count--;

                if (count < 0)
                    bail = true;
            }

            return new TilePosition(-1, -1);
        }

        public IList<TilePosition> Pathfind(TilePosition start, TilePosition end, int tolerence = 15) {
            // Build bounding box of candidate nodes
            TileRectangle boundingBox = MapConfiguration.BoundingBox(start, end);
            boundingBox = ExpandBoundingBox(boundingBox, tolerence);

            // Generate list of nodes from the bounding box
            List<IPathNode> nodes = new List<IPathNode>();
            TilePosition p = new TilePosition();
            for (int x = boundingBox.Left; x <= boundingBox.Right; x++) {
                for (int y = boundingBox.Bottom; y <= boundingBox.Top; y++) {
                    p.x = x;
                    p.y = y;
                    nodes.Add(Tiles[p]);
                }
            }
            AStar pathfinder = new AStar(nodes);

            // Perform search
            IList<IPathNode> results = pathfinder.Search(
                Tiles[start],
                Tiles[end]);

            // Convert results into usable list of TilePosition objects
            List<TilePosition> positions = new List<TilePosition>();

            foreach(IPathNode result in results) {
                positions.Add(result.Position);
            }

            return positions;
        }


		/// <summary>
		/// Determines if a building is on a corner.  Pass in the Directions that have adjacent roads
		/// and the direction the building is facing, and the function returns a Direction indicating
		/// that the building has a corner towards that direction, or Direction.None if it isn't on a
		/// corner. 
		/// </summary>
		/// <returns>The direction of the corner.</returns>
		/// <param name="roads">The Directions towards which there are roads.</param>
		/// <param name="facing">The Direction that the building is facing.</param>
		public static Direction CornerDirection(List<Direction> roads, Direction facing) {
			roads = new List<Direction>(roads);

			if(roads.Count < 2) 
				return Direction.None;

			if(facing == Direction.North || facing == Direction.South) {
				roads.Remove(Direction.North);
				roads.Remove(Direction.South);
			}

			if(facing == Direction.East || facing == Direction.West) {
				roads.Remove(Direction.East);
				roads.Remove(Direction.West);
			}

			if(roads.Count == 0)
				return Direction.None;

			return roads[0];
		}


		public static TilePosition North(TilePosition pos) {
			return new TilePosition(pos.x, pos.y + 1);
		}

		public static TilePosition South(TilePosition pos) {
			return new TilePosition(pos.x, pos.y - 1);
		}

		public static TilePosition East(TilePosition pos) {
			return new TilePosition(pos.x + 1, pos.y);
		}

		public static TilePosition West(TilePosition pos) {
			return new TilePosition(pos.x - 1, pos.y);
		}

		public static TilePosition NextTile(TilePosition pos, Direction direction) {
			switch(direction) {
				case Direction.North:
					return North(pos);
				case Direction.South:
					return South(pos);
				case Direction.East:
					return East(pos);
				case Direction.West:
					return West(pos);
			}

			return pos;

		}

		public static Direction OppositeDirection(Direction direction) {
			switch(direction) {
				case Direction.North:
					return Direction.South;
				case Direction.South:
					return Direction.North;
				case Direction.East:
					return Direction.West;
				case Direction.West:
					return Direction.East;
			}
			return Direction.None;
		}

        public static Direction RelativeDirection(TilePosition from, TilePosition to) {
            if (to.y > from.y)
                return Direction.North;
            if (to.x > from.x)
                return Direction.East;
            if (to.y < from.y)
                return Direction.South;
            else return Direction.West;
        }

		public static Orientation OppositeOrientation(Orientation orientation) {
			if(orientation == Orientation.Vertical)
				return Orientation.Horizontal;
			else 
				return Orientation.Vertical;
		}

        public bool HasRoad(TilePosition pos) {
            if(Tiles.ContainsKey(pos))
                return HasRoad(Tiles[pos]);
            return false;
        }

        public bool IsValidPoleLocation(TilePosition pos) {
            if (Tiles.ContainsKey(pos))
                return HasRoad(Tiles[pos]) && 
                    (Tiles[pos].RoadType == RoadType.Horizontal || Tiles[pos].RoadType == RoadType.Vertical);
            return false;
        }

        public IEnumerable<TilePosition> Area(TilePosition center, int radius) {
            List<TilePosition> area = new List<TilePosition>();

            for(int x = center.x - radius; x <= center.x + radius; x++) {
                for(int y = center.y - radius; y <= center.y + radius; y++) {
                    TilePosition pos = new TilePosition(x, y);
                    if(IsInBounds(pos)) {
                        area.Add(pos);
                    }
                }
            }

            return area;
        }

        public TilePosition Clamp(TilePosition pos) {
            return new TilePosition(Math.Min(Width - 1, Math.Max(0, pos.x)), Math.Min(Height - 1, Math.Max(0, pos.y)));
        }

        public TileRectangle Clamp(TileRectangle rect) {
            return new TileRectangle(Clamp(rect.BottomLeft), Clamp(rect.TopRight));
        }

        public TileRectangle Area(TilePosition start, Direction direction, int distance) {
            TileRectangle rect;
            switch (direction) {
                case Direction.North:
                    rect = new TileRectangle(start.x - distance / 2, start.y, start.x + distance / 2, start.y + distance);
                    break;
                case Direction.South:
                    rect = new TileRectangle(start.x - distance / 2, start.y - distance, start.x + distance / 2, start.y);
                    break;
                case Direction.East:
                    rect = new TileRectangle(start.x, start.y - distance / 2, start.x + distance, start.y + distance / 2);
                    break;
                case Direction.West:
                default:
                    rect = new TileRectangle(start.x - distance, start.y - distance / 2, start.x, start.y + distance / 2);
                    break;
            }

            return Clamp(rect);
        }

        public static bool HasRoad(Tile tile) {
			return tile != null && tile.Type == TileType.Road;
		}

        public static TileRectangle BoundingBox(Neighborhood n1, Neighborhood n2) {
            return BoundingBox(n1.Position, n2.Position);
        }

        public static TileRectangle BoundingBox(TilePosition t1, TilePosition t2) {
            TilePosition bottomLeft = new TilePosition(Math.Min(t1.x, t2.x), Math.Min(t1.y, t2.y));
            TilePosition topRight   = new TilePosition(Math.Max(t1.x, t2.x), Math.Max(t1.y, t2.y));
            return new TileRectangle(bottomLeft, topRight);
        }

        public HashSet<TilePosition> LotPeriphery(Lot lot) {
            HashSet<TilePosition> lotPositions = new HashSet<TilePosition>(lot.Tiles);
            HashSet<TilePosition> removals = new HashSet<TilePosition>();

            foreach(TilePosition pos in lotPositions) {
                if(    lotPositions.Contains(North(pos))
                    && lotPositions.Contains(South(pos))
                    && lotPositions.Contains(East(pos))
                    && lotPositions.Contains(West(pos))) {
                    removals.Add(pos);
                }
            }

            lotPositions.RemoveMany(removals);

            return lotPositions;
        }


    }
}
