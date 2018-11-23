using System;
using System.Collections.Generic;
using DCTC.Model;

namespace DCTC.Map
{
    [Serializable]
    public class NeighborhoodTemplate
    {
        public List<Segment> Roads { get; set; }
        public List<BlockTemplate> Blocks { get; set; }
    }

    [Serializable]
    public class BlockTemplate
    {
        public TilePosition Offset { get; set; }
        public List<LotTemplate> Lots { get; set; }
    }

    [Serializable]
    public class LotTemplate
    {
        public TilePosition Position { get; set; }
        public BuildingType BuildingType { get; set; }
        public Direction Facing { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    [Serializable]
    public class MapTemplate
    {
        public Dictionary<string, LotTemplate> LotTemplates { get; set; }
        public Dictionary<string, BlockTemplate> BlockTemplates { get; set; }
        public List<NeighborhoodTemplate> Neighborhoods { get; set; }
    }

    public class MapGenerator
    {
        public MapConfiguration Generate()
        {
            MapTemplate template = Loader.LoadMapTemplate();

            MapConfiguration map = new MapConfiguration();
            return map;
        }
    }
}
