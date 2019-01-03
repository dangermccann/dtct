using System;
using System.Linq;
using System.Collections.Generic;
using DCTC.Map;

namespace DCTC.Model 
{ 
    public class Lot {
        public HashSet<TilePosition> Tiles;
        public TilePosition Anchor;
        public Street Street;
        public int StreetNumber;
        public Direction Facing;
        public Building Building;

        public decimal SquareMeters {
            get { return Tiles.Count * Sizes.SquareMetersPerTile;  }
        }

        public Lot() {
            Tiles = new HashSet<TilePosition>();
        }

        public string Address {
            get {
                if (Street == null)
                    return null;
                return StreetNumber.ToString() + " " + Street.Name;
            }
        }

        public void PopulateTiles(int width, int height) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    TilePosition pos = new TilePosition(Anchor.x + x, Anchor.y + y);
                    Tiles.Add(pos);
                }
            }
        }

        public bool WillFitBuilding(BuildingAttributes attributes) {
            for(int x = 0; x < attributes.Width; x++) {
                for(int y = 0; y < attributes.Height; y++) {
                    TilePosition pos = new TilePosition(Anchor.x + x, Anchor.y + y);
                    if (!Tiles.Contains(pos))
                        return false;
                }
            }

            return true;
        }

        public HashSet<TilePosition> Corners() {
            int left = Tiles.Min(p => p.x);
            int right = Tiles.Max(p => p.x);
            int bottom = Tiles.Min(p => p.y);
            int top = Tiles.Max(p => p.y);

            return new HashSet<TilePosition>() {
                new TilePosition(left, bottom),
                new TilePosition(left, top),
                new TilePosition(right, bottom),
                new TilePosition(right, top),
            };
        }
    }
}
