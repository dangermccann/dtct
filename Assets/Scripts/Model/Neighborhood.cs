using System;
using System.Collections.Generic;
using System.Linq;
using DCTC.Map;

namespace DCTC.Model
{
    public enum SectionType {
        Residential,
        //Commercial,
        Industrial,
        Agricultural,
    }

    public class NeighborhoodAttributes {
		public int MinWidth, MaxWidth, MinHeight, MaxHeight;
	}

	public class Neighborhood {
		public string Name;
		public int Width;
		public int Height;
		public TilePosition Position;
		public MapConfiguration Map;
		public List<Street> Streets = new List<Street>();
        public List<Lot> Lots = new List<Lot>();
        public SectionType SectonType;

		
		public Neighborhood(MapConfiguration map, int width, int height) {
			this.Map = map;
			this.Width = width;
			this.Height = height;
		}

		public IEnumerable<Tile> AllTiles {
			get {
				List<Tile> tiles = new List<Tile>();

				for(int x = Position.x; x < Position.x + Width; x++) {
					for(int y = Position.y; y < Position.y + Height; y++) {
						tiles.Add(Map.Tiles[new TilePosition(x, y)]);
					}
				}

				return tiles;
			}
		}

        public IEnumerable<TilePosition> AllPositions {
            get {
                List<TilePosition> positions = new List<TilePosition>();

                for (int x = Position.x; x < Position.x + Width; x++) {
                    for (int y = Position.y; y < Position.y + Height; y++) {
                        positions.Add(new TilePosition(x, y));
                    }
                }

                return positions;
            }
        }

        public virtual int MinimumLotWidth {
            get { return 1; }
        }

        public virtual int MinimumLotHeight {
            get { return 1; }
        }

        public virtual void BeforePopulate(System.Random random) { }
		public Building CreateBuilding(System.Random random, Tile tile, Lot lot) {

            // TODO  
            return null;
        }

        public void Populate(System.Random random) {
            BeforePopulate(random);
            foreach(Lot lot in Lots) {
                Tile anchorTile = Map.Tiles[lot.Anchor];
                Building building = CreateBuilding(random, anchorTile, lot);

                if (building != null) {
                    ApplyBuilding(building);
                    lot.Building = building;
                }
            }
        }


		protected void ApplyBuilding(Building building) {
			foreach(TilePosition pos in building.Positions) { 
				Tile tile = Map.Tiles[new TilePosition(pos.x, pos.y)];
				tile.Building = building;
				
			}
		}


		public virtual void AssignNamesAndAddresses(System.Random random, NameGenerator nameGenerator) {
			Name = nameGenerator.RandomPlace();

			foreach(Street street in Streets) {
				street.GenerateName(nameGenerator);
			}
		}

		
		public virtual bool CanCreateAt(TilePosition position) {
			
			if(Width <= 0 || Height <= 0) 
				return false;
			
			if(position.x + Width >= Map.Width) 
				return false;
			
			if(position.y + Height >= Map.Height) 
				return false;

			int buffer = 2;

			foreach(Neighborhood neighborhood in Map.Neighborhoods) {
				if(neighborhood.Overlaps(new TileRectangle(new TilePosition(position.x - buffer, position.y - buffer), 
                                                           new TilePosition(position.x + Width + buffer, position.y + Height + buffer))))
					return false;
			}

			return true;
		}
		
		public void FloodFill(TileType type) {
			for(int x = 0; x < Width; x++) {
				for(int y = 0; y < Height; y++) {
					TilePosition pos = new TilePosition(Position.x + x, Position.y + y);
					Map.Tiles[pos].Type = type;
				}
			}
		}

        public void CreateStraightRoad(TilePosition from, TilePosition to, Orientation orientation) {
            if(orientation == Orientation.Horizontal) {
                for (int x = from.x; x <= to.x; x++) {
                    Map.AddRoad(new TilePosition(x, from.y));
                }
            }
            else {
                for (int y = from.y; y <= to.y; y++) {
                    Map.AddRoad(new TilePosition(from.x, y));
                }
            }
        }

		public void CreateRectangularRoad(TilePosition bottomLeft, TilePosition topRight) {
			for(int x = bottomLeft.x; x <= topRight.x; x++) {
				Map.AddRoad(new TilePosition(Position.x + x, Position.y + bottomLeft.y));
				Map.AddRoad(new TilePosition(Position.x + x, Position.y + topRight.y));
			}
			
			for(int y = bottomLeft.y; y <= topRight.y; y++) {
				Map.AddRoad(new TilePosition(Position.x + bottomLeft.x, Position.y + y));
				Map.AddRoad(new TilePosition(Position.x + topRight.x, Position.y + y));
			}
		}

		public void CreateBorderingRoad() {
			CreateRectangularRoad(new TilePosition(0, 0), new TilePosition(Width - 1, Height - 1));
		}

		public List<Street> CreateRectangleOfStreets(TilePosition bottomLeft, TilePosition topRight) {
			List<Street> result = new List<Street>();

			// north
			Street s = new Street(Map);
			s.AddSegment(new TilePosition(bottomLeft.x, topRight.y), new TilePosition(topRight.x, topRight.y));
			result.Add(s);

			// south
			s = new Street(Map);
			s.AddSegment(new TilePosition(bottomLeft.x, bottomLeft.y), new TilePosition(topRight.x, bottomLeft.y));
			result.Add(s);

			// east
			s = new Street(Map);
			s.AddSegment(new TilePosition(topRight.x, bottomLeft.y), new TilePosition(topRight.x, topRight.y));
			result.Add(s);

			// west
			s = new Street(Map);
			s.AddSegment(new TilePosition(bottomLeft.x, bottomLeft.y), new TilePosition(bottomLeft.x, topRight.y));
			result.Add(s);

			AddStreets(result);

			return result;
		}

		public List<Street> CreateBorderingStreets() {
			return CreateRectangleOfStreets(new TilePosition(Position.x, Position.y), 
			                                new TilePosition(Position.x + Width - 1, Position.y + Height - 1));
		}

		public TileRectangle Rectangle() {
			return new TileRectangle(Position, new TilePosition(Position.x + Width - 1, Position.y + Height - 1));
		}

		public bool Overlaps(Neighborhood neighborhood) {
			return Overlaps(neighborhood.Rectangle());
		}

		public bool Overlaps(TileRectangle other) {
            TileRectangle rect = Rectangle();
			return other.Right >= rect.Left && other.Left <= rect.Right && other.Top >= rect.Bottom && other.Bottom <= rect.Top;
		}

		public bool IsInside(TilePosition point) {
			return point.IsInRect(Rectangle());
		}

		public TilePosition Center() {
			TilePosition pos = new TilePosition();
			pos.x = Position.x + (Width - 1) / 2;
			pos.y = Position.y + (Height - 1) / 2;
			return pos;
		}

		public void AddStreets(List<Street> streets) {
			foreach(Street s in streets) {
				AddStreet(s);
			}
		}

		public void AddStreet(Street street) {
			Streets.Add(street);
			Map.Streets.Add(street);
		}

		public Street FindStreet(TilePosition position) {
			foreach(Street street in Streets) {
				if(street.Contains(position)) {
					return street;
				}
			}
			return null;
		}

        public TilePosition RandomStreetTile(System.Random random) {
            return RandomUtils.RandomThing<Street>(Streets, random).AllTiles.First();
        }
	}
}

