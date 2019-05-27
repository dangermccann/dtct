using System;
using System.Collections.Generic;
using DCTC.Map;

namespace DCTC.Model {
	public class Building {

        public TilePosition Anchor;
        public BuildingType Type;
		public Direction FacingDirection;
		public int Width;
		public int Height;
        public string BlockPosition;
        public int Variation;
        public string Color;
        public float SquareMeters;

		public Building (Tile tile, BuildingType type) : this(tile, type, Direction.North) { }
		
		public Building (Tile tile, BuildingType type, Direction facingDirection) 
			: this(tile, type, facingDirection, 1, 1) { }

		public Building (Tile tile, BuildingType type, Direction facingDirection, int width, 
		                 int height) : this(tile, type, facingDirection, width, height, "M", 1) { }

        public Building (Tile tile, BuildingType type, Direction facingDirection, int width, 
		                 int height, string blockPosition, int variation) {


            this.Anchor = tile.Position;
            this.Type = type;
			this.FacingDirection = facingDirection;
			this.Width = width;
			this.Height = height;
            this.BlockPosition = blockPosition;
            this.Variation = variation;
            SquareMeters = 300;
        }

        public bool Contains(TilePosition pos) {
            return (pos.x >= Anchor.x && pos.x < Anchor.x + Width &&
                    pos.y >= Anchor.y && pos.y < Anchor.y + Height);
        }

        public bool IsResidential() {
            switch(Type) {
                case BuildingType.H1:
                case BuildingType.H2:
                case BuildingType.H3:
                case BuildingType.H4:
                case BuildingType.H5:
                case BuildingType.H6:
                case BuildingType.H7:
                case BuildingType.H8:
                case BuildingType.H9:
                case BuildingType.A1:
                case BuildingType.A2:
                    return true;
            }

            return false;
        }

        public bool IsSingleFamily() {
            switch (Type) {
                case BuildingType.H1:
                case BuildingType.H2:
                case BuildingType.H3:
                case BuildingType.H4:
                case BuildingType.H5:
                case BuildingType.H6:
                case BuildingType.H7:
                case BuildingType.H8:
                case BuildingType.H9:
                    return true;
            }

            return false;
        }

        public bool IsMultiDwellingUnit() {
            switch (Type) {
                case BuildingType.A1:
                case BuildingType.A2:
                    return true;
            }
            return false;
        }

	}

	public enum BuildingType {
        H1, H2, H3, H4, H5,
        H6, H7, H8, H9,
        A1, A2,
        Headquarters,
        Park,
	}

    public enum BuildingClassification {
        Residential,
        Commercial,
        Industrial,
        Agricultural,
        Community
    }


}

