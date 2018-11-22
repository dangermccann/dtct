
using System;

namespace DCTC.Map
{
	public enum Direction
	{
		None,
		North, 
		South, 
		East, 
		West
	}

    public static class Directions {
        public static readonly Direction[] All = new Direction[] {
            Direction.North, Direction.South, Direction.East, Direction.West
        };
    }

	public enum Orientation {
		Horizontal,
		Vertical
	}
}

