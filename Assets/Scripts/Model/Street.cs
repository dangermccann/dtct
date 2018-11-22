using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using DCTC.Map;

namespace DCTC.Model
{
	public enum StreetSize {
		Large,
		Medium,
		Small
	}

    [Serializable]
	public class Segment {

		public TilePosition Start { get; set; }
		public TilePosition End { get; set; }

        public Segment() {  }

		public Segment(TilePosition start, TilePosition end) { 
			this.Start = start;
			this.End = end;

			if(start.x != end.x && start.y != end.y)
				throw new Exception("Invalid segment arguments");
		}

        [YamlIgnore]
        public Orientation Orientation {
			get {
				if(Start.x == End.x)
					return Orientation.Vertical;
				else
					return Orientation.Horizontal;
			}
		}

        [YamlIgnore]
        public IList<TilePosition> Positions {
			get {
				List<TilePosition> positions = new List<TilePosition>();

				int diff;
				if(Orientation == Orientation.Horizontal) {
					diff = End.x - Start.x;
					for(int i = 0; i <= Math.Abs(diff); i++) {
						if(Start.x < End.x)
							positions.Add(new TilePosition(Start.x + i, Start.y));
						else
							positions.Add(new TilePosition(Start.x - i, Start.y));
					}

				}
				else {
					diff = End.y - Start.y;
					for(int i = 0; i <= Math.Abs(diff); i++) {
						if(Start.y < End.y)
							positions.Add(new TilePosition(Start.x, Start.y + i));
						else
							positions.Add(new TilePosition(Start.x, Start.y - 1));
					}
				}

				return positions;
			}
		}

        [YamlIgnore]
        public int Length {
            get {
                if (Orientation == Orientation.Horizontal)
                    return Math.Abs(End.x - Start.x);
                else
                    return Math.Abs(End.y - Start.y);
            }
        }

		public bool Contains(TilePosition pos) {
			if(pos.x == Start.x && pos.x == End.x && pos.y >= Start.y && pos.y <= End.y)
				return true;

			if(pos.y == Start.y && pos.y == End.y && pos.x >= Start.x && pos.x <= End.x)
				return true;

			return false;
		}
	}

	public class Street
	{
		public string Name;
		public List<Segment> Segments = new List<Segment>();
		public StreetSize Size = StreetSize.Medium;
		public MapConfiguration Map;

		public Street(MapConfiguration map) { 
			this.Map = map;
		}

		public IEnumerable<TilePosition> AllTiles {
			get { 
				HashSet<TilePosition> tiles = new HashSet<TilePosition>();

				foreach(Segment s in Segments) {
					if(s.Start.x == s.End.x) {
						for(int y = s.Start.y; y <= s.End.y; y++) {
							TilePosition pos = new TilePosition(s.Start.x, y);
							if(!tiles.Contains(pos))
								tiles.Add(pos);
						}
					}
					else {
						for(int x = s.Start.x; x <= s.End.x; x++) {
							TilePosition pos = new TilePosition(x, s.Start.y);
							if(!tiles.Contains(pos))
								tiles.Add(pos);
						}
					}
				}

				return tiles;
			}
		}

        public int Length {
            get {
                int length = 0;
                foreach(Segment segment in Segments) {
                    length += segment.Length;
                }
                return length;
            }
        }

		public Segment AddSegment(TilePosition start, TilePosition end) {
			Segment s = new Segment(start, end);
			Segments.Add(s);
			return s;
		}

		public string GenerateName(NameGenerator generator) {
			switch(Size) {
				case StreetSize.Small:
					Name = generator.RandomMinorStreet();
					break;

				case StreetSize.Medium:
					Name = generator.RandomStreet();
					break;

				case StreetSize.Large:
					Name = generator.RandomMajorStreet();
					break;
			}
			return Name;
		}

		public bool Contains(TilePosition pos) {
			foreach(Segment s in Segments) {
				if(s.Contains(pos))
					return true;
			}

			return false;
		}

		public static void GenerateNames(List<Street> streets, NameGenerator generator) {
			foreach(Street s in streets) {
				s.GenerateName(generator);
			}
		}

	}
}

