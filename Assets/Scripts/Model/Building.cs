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
        public float SquareMeters;

		public Building (Tile tile, BuildingType type) : this(tile, type, Direction.North) { }
		
		public Building (Tile tile, BuildingType type, Direction facingDirection) 
			: this(tile, type, facingDirection, 1, 1) { }

		public Building (Tile tile, BuildingType type, Direction facingDirection, int width, 
		                 int height) : this(tile, type, facingDirection, width, height, 0) { }

        public Building (Tile tile, BuildingType type, Direction facingDirection, int width, 
		                 int height, float squareMeters) {


            this.Anchor = tile.Position;
            this.Type = type;
			this.FacingDirection = facingDirection;
			this.Width = width;
			this.Height = height;
            this.SquareMeters = squareMeters;
        }

        public BuildingAttributes GetAttributes() {
            return BuildingAttributes.GetAttributes(Type, FacingDirection);
        }

	}

	public enum BuildingType {
		SmallHouse,
		Townhouse,
        Ranch,
		SuburbanHouse,
        Apartment,
        StripMall,
        SmallRetail,
        SmallRetail2,
        Retail,
		Barn,
        Factory,
        SmallFactory,
        Warehouse
	}

    public enum BuildingClassification {
        Residential,
        Commercial,
        Industrial,
        Agricultural
    }

	public enum BuildingColor {
		None,
		Gray,
		Red,
		RedOrange,
		Blue,
		Yellow,
		Pink,
		Beige,
		Teal,
        White
	}

	public class BuildingAttributes {
		public int Width, Height;
        public float SquareMeters;
        public BuildingClassification Classification;


        BuildingAttributes() { }

        BuildingAttributes(int width, int height, BuildingClassification classification, float squareMeters) {
			this.Width = width;
			this.Height = height;
            this.Classification = classification;
            this.SquareMeters = squareMeters;
        }

		public static BuildingAttributes GetAttributes(BuildingType type, Direction facing) {
			switch(type) {
                // Residential
				case BuildingType.SmallHouse:
					return new BuildingAttributes(1, 1, BuildingClassification.Residential, 185);

				case BuildingType.Townhouse:
					return new BuildingAttributes(1, 1, BuildingClassification.Residential, 120);

                case BuildingType.Ranch:
					return new BuildingAttributes((facing == Direction.East || facing == Direction.West) ? 1 : 2, 
												  (facing == Direction.East || facing == Direction.West) ? 2 : 1,
                                                  BuildingClassification.Residential, 170);

				case BuildingType.SuburbanHouse:
					return new BuildingAttributes((facing == Direction.East || facing == Direction.West) ? 1 : 2, 
												  (facing == Direction.East || facing == Direction.West) ? 2 : 1,
                                                  BuildingClassification.Residential, 232);
                case BuildingType.Apartment:
                    return new BuildingAttributes(2, 2, BuildingClassification.Residential, 670);

                // Commercial
                case BuildingType.StripMall:
                    return new BuildingAttributes(2, 2, BuildingClassification.Commercial, 278);

                case BuildingType.SmallRetail:
                    return new BuildingAttributes(1, 1, BuildingClassification.Commercial, 150);

                case BuildingType.Retail:
                    return new BuildingAttributes(2, 2, BuildingClassification.Commercial, 300);

                // Aggricultural
                case BuildingType.Barn:
                    return new BuildingAttributes(1, 1, BuildingClassification.Agricultural, 100);

                // Industrial
                case BuildingType.Factory:
                    return new BuildingAttributes(6, 6, BuildingClassification.Industrial, 9000);

                case BuildingType.SmallFactory:
                    return new BuildingAttributes(4, 4, BuildingClassification.Industrial, 4800);

                case BuildingType.Warehouse:
                    return new BuildingAttributes(4, 4, BuildingClassification.Industrial, 4800);

            }

            return new BuildingAttributes();
		}
	}
}

