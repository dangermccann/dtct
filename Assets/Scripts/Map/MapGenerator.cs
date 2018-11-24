using System;
using System.Collections.Generic;
using DCTC.Model;

namespace DCTC.Map
{
    [Serializable]
    public class NeighborhoodTemplate
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Segment> Roads { get; set; }
        public List<TemplateReference> Blocks { get; set; }
    }

    [Serializable]
    public class BlockTemplate
    {
        public TilePosition Offset { get; set; }
        public List<TemplateReference> Lots { get; set; }
    }

    [Serializable]
    public class TemplateReference {
        public string Name { get; set; }
        public TilePosition Position { get; set; }
    }

    [Serializable]
    public class LotTemplate
    {
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

    public class MapGenerator {
        NameGenerator nameGenerator;
        System.Random random;

        const int NeighborhoodWidth = 60;
        const int NeighborhoodHeight = 60;
        const int NeighborhoodCountX = 2;
        const int NeighborhoodCountY = 2;

        public MapGenerator(System.Random rand) {
            random = rand;
            nameGenerator = new NameGenerator(rand);
        }


        public MapConfiguration Generate() {
            MapTemplate template = Loader.LoadMapTemplate();

            MapConfiguration map = new MapConfiguration(NeighborhoodWidth * NeighborhoodCountX + 1, 
                NeighborhoodHeight * NeighborhoodCountY + 1);
            map.CreateTiles();

            int startIdx = random.Next(5);
            int nTemplateCount = template.Neighborhoods.Count;
            for(int x = 0; x < NeighborhoodCountX; x++) {
                for(int y = 0; y < NeighborhoodCountY; y++) {
                    GenerateNeighborhood(map, template, (startIdx + x + y) % nTemplateCount, 
                        new TilePosition(x * NeighborhoodWidth, y * NeighborhoodHeight));
                }
            }

            // Final roads on outside
            for(int x = 0; x < NeighborhoodWidth * NeighborhoodCountX; x++) {
                map.AddRoad(new TilePosition(x, NeighborhoodHeight * NeighborhoodCountY));
            }

            for (int y = 0; y < NeighborhoodHeight * NeighborhoodCountY; y++) {
                map.AddRoad(new TilePosition(NeighborhoodWidth * NeighborhoodCountX, y));
            }

            return map;
        }

        void GenerateNeighborhood(MapConfiguration map, MapTemplate template, int neighborhoodIndex, TilePosition offset) {
            NeighborhoodTemplate neighborhoodTemplate = template.Neighborhoods[neighborhoodIndex];
            Neighborhood neighborhood = new Neighborhood(map, neighborhoodTemplate.Width, neighborhoodTemplate.Height);
            neighborhood.Position = offset;
            map.Neighborhoods.Add(neighborhood);

            // Generate streets
            foreach (Segment segment in neighborhoodTemplate.Roads) {
                Segment roadSegment = segment + offset;
                Street street = new Street(map);
                street.Name = nameGenerator.RandomMinorStreet();
                street.Segments.Add(roadSegment);
                neighborhood.AddStreet(street);
                neighborhood.CreateStraightRoad(roadSegment);
            }

            // Generate lots and buildings 
            foreach(TemplateReference blockRef in neighborhoodTemplate.Blocks) {

                if (!template.BlockTemplates.ContainsKey(blockRef.Name))
                    throw new Exception("Template " + blockRef.Name + " not found in BlockTemplates");

                BlockTemplate block = template.BlockTemplates[blockRef.Name];
                foreach(TemplateReference lotRef in block.Lots) {
                    LotTemplate lotTemplate = template.LotTemplates[lotRef.Name];

                    TilePosition lotPosition = offset + blockRef.Position + lotRef.Position;
                    Lot lot = new Lot();
                    lot.Anchor = lotPosition;
                    lot.Facing = lotTemplate.Facing;
                    lot.PopulateTiles(lotTemplate.Width, lotTemplate.Height);
                    

                    BuildingAttributes attributes = BuildingAttributes.GetAttributes(lotTemplate.BuildingType, lotTemplate.Facing);
                    Building building = new Building(map.Tiles[lotPosition], lotTemplate.BuildingType, lotTemplate.Facing,
                        attributes.Width, attributes.Height);
                    lot.Building = building;

                    for (int x = 0; x < lotTemplate.Width; x++) {
                        for (int y = 0; y < lotTemplate.Height; y++) {
                            Tile tile = map.Tiles[new TilePosition(x + lotPosition.x, y + lotPosition.y)];
                            tile.Building = building;
                        }
                    }

                    neighborhood.Lots.Add(lot);
                }
            }
        }
    }
}
