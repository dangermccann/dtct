using System;
using DCTC.Model;

namespace DCTC.Map {

	public class Tile : Pathfinding.IPathNode {

		public Action<Tile> TypeChanged;
		public Action<Tile> BuildingChanged;

		public TilePosition Position { get; set; }
		public int MovementCost { get; set; }
        public Lot Lot { get; set; }


		protected TileType type;
		public TileType Type {
			get {
				return type;
			}
			set {
				type = value;
				if(TypeChanged != null)
					TypeChanged(this);
			}
		}

		protected RoadType roadType = 0;
		public RoadType RoadType {
			get {
				return roadType;
			}
			set {
                roadType = value;
			}
		}

        public Orientation RoadOrientation {
            get {
                if (RoadType == RoadType.Vertical)
                    return Orientation.Vertical;
                else return Orientation.Horizontal;
            }
        }

		protected Building building = null;
		public Building Building {
			get {
				return building;
			}
			set {
				building = value;

                MovementCost = (building != null) ? MapConfiguration.BuildingMovementCost : MapConfiguration.DefaultMovementCost;

				if(BuildingChanged != null)
					BuildingChanged(this);
			}
		}

		public Tile(TilePosition position, TileType type, RoadType roadType) {
			Position = position;
			Type = type;
            RoadType = roadType;
			MovementCost = MapConfiguration.DefaultMovementCost;
		}

		public Tile(TilePosition position, TileType type) : this(position, type, 0) { }
		public Tile(TilePosition position) : this(position, TileType.Empty) { }
		public Tile() : this(new TilePosition(0, 0)) { }

		public bool CanBuildOn() {
			return Type != TileType.Road;
		}

		public void RemoveAllListeners() {
			foreach(Delegate d in TypeChanged.GetInvocationList()) {
				TypeChanged -= (Action<Tile>) d;
			}
			foreach(Delegate d in BuildingChanged.GetInvocationList()) {
				BuildingChanged -= (Action<Tile>) d;
			}
		}
	}

	public enum TileType {
		Empty,
		Grass,
		Road,
        Connector
	}

	public enum RoadType {
		Horizontal,
		Vertical,
		CornerNE,
		CornerNW,
		CornerSE,
		CornerSW,
		IntersectAll,
		IntersectN,
		IntersectS,
		IntersectE,
		IntersectW
	}
}
