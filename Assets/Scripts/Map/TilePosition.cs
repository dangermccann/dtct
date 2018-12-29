using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Converters;

namespace DCTC.Map {
    [Serializable]
    public class TilePosition : IEquatable<TilePosition> {
        [YamlMember(Alias = "x", ApplyNamingConventions = false)]
        public int x { get; set; }

        [YamlMember(Alias = "y", ApplyNamingConventions = false)]
        public int y { get; set; }

        public static TilePosition Origin {
            get { return new TilePosition(0, 0);  }
        }

        public TilePosition() { }
        public TilePosition(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public bool IsInRect(TileRectangle rect) {
            return x >= rect.Left
                && x <= rect.Right
                && y >= rect.Bottom
                && y <= rect.Top;
        }

        public bool IsAdjacent(TilePosition other) {
            return (Math.Abs(x - other.x) == 1 && y == other.y) ||
                   (Math.Abs(y - other.y) == 1 && x == other.x);
        }


        public override bool Equals(System.Object obj) {
            // If parameter is null return false.
            if (obj == null) {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            TilePosition p = obj as TilePosition;
            if ((System.Object)p == null) {
                return false;
            }

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }

        public bool Equals(TilePosition p) {
            // If parameter is null return false:
            if ((object)p == null) {
                return false;
            }

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }

        public override int GetHashCode() {
            return x ^ y;
        }

        public override string ToString() {
            return "(" + x + ", " + y + ")";
        }

        public static TilePosition Parse(string str) {
            if (!str.StartsWith("(") || !str.EndsWith(")"))
                throw new ArgumentException();

            str = str.Substring(1, str.Length - 2);
            string[] splits = str.Split(',');

            if (splits.Length != 2)
                throw new ArgumentException();

            int x = int.Parse(splits[0].Trim());
            int y = int.Parse(splits[1].Trim());

            return new TilePosition(x, y);
        }

        public static int Distance(TilePosition from, TilePosition to) {
            return Math.Abs(to.x - from.x) + Math.Abs(to.y - from.y);
        }

        public static TilePosition operator +(TilePosition t1, TilePosition t2) {
            TilePosition result = new TilePosition();
            result.x = t1.x + t2.x;
            result.y = t1.y + t2.y;
            return result;
        }

        public static TilePosition operator -(TilePosition t1, TilePosition t2) {
            TilePosition result = new TilePosition();
            result.x = t1.x - t2.x;
            result.y = t1.y - t2.y;
            return result;
        }

        /// <summary>
        /// Returns a range of TilePositions repsenting a user selection or rectangle of tiles relative to
        /// a fixed starting point.  
        /// </summary>
        /// <param name="start"></param>
        /// <param name="horizontalDistance"></param>
        /// <param name="verticalDistance"></param>
        /// <param name="horizontalDirection"></param>
        /// <param name="verticalDirection"></param>
        /// <returns></returns>
        public static List<TilePosition> RangeSelection(TilePosition start, int horizontalDistance, int verticalDistance,
                Direction horizontalDirection, Direction verticalDirection) {

            if (horizontalDirection == Direction.North || horizontalDirection == Direction.South)
                throw new ArgumentException("Bad horizontalDirection " + horizontalDirection);
            if (verticalDirection == Direction.East || verticalDirection == Direction.West)
                throw new ArgumentException("Bad verticalDirection " + verticalDirection);


            List<TilePosition> positions = new List<TilePosition>();

            for (int x = 0; x < horizontalDistance; x++) {
                for (int y = 0; y < verticalDistance; y++) {
                    TilePosition pos = new TilePosition();
                    pos.x = horizontalDirection == Direction.East ? start.x + x : start.x - x;
                    pos.y = verticalDirection == Direction.North ? start.y + y : start.y - y;
                    positions.Add(pos);
                }
            }

            return positions;
        }

    }

    [Serializable]
    public class TileRectangle {
        TilePosition BottomLeft;
        TilePosition TopRight;

        public TileRectangle(int left, int bottom, int right, int top) :
            this(new TilePosition(left, bottom), new TilePosition(right, top)) { }

        public TileRectangle(TilePosition bottomLeft, TilePosition topRight) {
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        public int Width { get { return TopRight.x - BottomLeft.x; } }
        public int Height { get { return TopRight.y - BottomLeft.y; } }

        public int Left { get { return BottomLeft.x; } }
        public int Right { get { return TopRight.x; } }
        public int Bottom { get { return BottomLeft.y; } }
        public int Top { get { return TopRight.y; } }
    }
}
