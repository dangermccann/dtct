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

        public MapGenerator(System.Random rand) {
            nameGenerator = new NameGenerator(rand);
        }


        public MapConfiguration Generate() {
            MapTemplate template = Loader.LoadMapTemplate();

            MapConfiguration map = new MapConfiguration(120, 120);
            map.CreateTiles();

            GenerateNeighborhood(map, template, 0, new TilePosition(0, 0));

            return map;
        }

        void GenerateNeighborhood(MapConfiguration map, MapTemplate template, int neighborhoodIndex, TilePosition offset) {
            NeighborhoodTemplate neighborhoodTemplate = template.Neighborhoods[neighborhoodIndex];
            Neighborhood neightborhood = new Neighborhood(map, neighborhoodTemplate.Width, neighborhoodTemplate.Height);
            map.Neighborhoods.Add(neightborhood);

            // Generate streets
            foreach (Segment segment in neighborhoodTemplate.Roads) {
                Segment roadSegment = segment + offset;
                Street street = new Street(map);
                street.Name = nameGenerator.RandomMinorStreet();
                street.Segments.Add(roadSegment);
                neightborhood.AddStreet(street);
                neightborhood.CreateStraightRoad(roadSegment);
            }

            // Generate lots and buildings 
            foreach(TemplateReference blockRef in neighborhoodTemplate.Blocks) {
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

                    neightborhood.Lots.Add(lot);
                }
            }
        }
    }
}
