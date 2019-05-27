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
        public string BlockPosition { get; set; }
        public int Variation { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public LotTemplate() { }
        public LotTemplate(string pattern) {
            string[] splits = pattern.Split('-');

            BuildingType = Utilities.ParseEnum<BuildingType>(splits[0]);
            switch(splits[1]) {
                case "N":
                    Facing = Direction.North;
                    break;
                case "S":
                    Facing = Direction.South;
                    break;
                case "E":
                    Facing = Direction.East;
                    break;
                case "W":
                    Facing = Direction.West;
                    break;
            }
            BlockPosition = splits[2];

            Width = int.Parse(splits[3].Split('x')[0]);
            Height = int.Parse(splits[3].Split('x')[1]);
            Variation = int.Parse(splits[4]);
        }
    }

    [Serializable]
    public class MapTemplate
    {
        public List<string> LotTemplates { get; set; }
        public Dictionary<string, BlockTemplate> BlockTemplates { get; set; }
        public List<NeighborhoodTemplate> Neighborhoods { get; set; }
        public Dictionary<string, List<string>> BuildingColors { get; set; }
    }

    public class MapGenerator {
        NameGenerator nameGenerator;
        System.Random random;

        const int NeighborhoodWidth = 60;
        const int NeighborhoodHeight = 60;
        private int neighborhoodCountX;
        private int neighborhoodCountY;
        private NewGameSettings settings;

        public MapGenerator(System.Random rand, NewGameSettings settings, NameGenerator nameGenerator) {
            this.random = rand;
            this.nameGenerator = nameGenerator;
            this.neighborhoodCountX = settings.NeighborhoodCountX;
            this.neighborhoodCountY = settings.NeighborhoodCountY;
            this.settings = settings;
        }

        public MapConfiguration Generate() {
            MapTemplate template = Loader.LoadMapTemplate();

            MapConfiguration map = new MapConfiguration(NeighborhoodWidth * neighborhoodCountX + 1, 
                NeighborhoodHeight * neighborhoodCountY + 1);
            map.CreateTiles();

            int startIdx = settings.Seed == -1 ? 2 : random.Next(5);
            int nTemplateCount = template.Neighborhoods.Count;
            for(int x = 0; x < neighborhoodCountX; x++) {
                for(int y = 0; y < neighborhoodCountY; y++) {
                    GenerateNeighborhood(map, template, (startIdx + x + y) % nTemplateCount, 
                        new TilePosition(x * NeighborhoodWidth, y * NeighborhoodHeight));
                }
            }

            // Final roads on outside
            Segment segment = new Segment(new TilePosition(0, NeighborhoodHeight * neighborhoodCountY),
                new TilePosition(NeighborhoodWidth * neighborhoodCountX - 1, NeighborhoodHeight * neighborhoodCountY));
            GenerateStreet(map, nameGenerator, segment, map.Neighborhoods[0]);

            segment = new Segment(new TilePosition(NeighborhoodWidth * neighborhoodCountX, 0),
                new TilePosition(NeighborhoodWidth * neighborhoodCountX, NeighborhoodHeight * neighborhoodCountY));
            GenerateStreet(map, nameGenerator, segment, map.Neighborhoods[0]);

            foreach (Neighborhood neighborhood in map.Neighborhoods) {
                AssignStreets(neighborhood, map);
            }

            return map;
        }

        public IList<TilePosition> GenerateHeadquarters(MapConfiguration map, int count) {
            List<TilePosition> candidates = new List<TilePosition>();
            foreach(Neighborhood neighborhood in map.Neighborhoods) {
                foreach(Lot lot in neighborhood.Lots) {
                    if(lot.Building != null) {
                        if(lot.Building.Type == BuildingType.None) {
                            candidates.Add(lot.Anchor);
                        }
                    }
                }
            }

            if(candidates.Count <= count) {
                UnityEngine.Debug.LogError("Couldn't find enough buildings to replace with headquarters");
                return null;
            }

            List<TilePosition> replacements = new List<TilePosition>();
            while(count > 0) {
                TilePosition candidate;
                if (settings.Seed != -1)
                    candidate = RandomUtils.RandomThing(candidates, random);
                else 
                    candidate = candidates[0];

                replacements.Add(candidate);
                candidates.Remove(candidate);
                count--;
            }

            foreach(TilePosition replacement in replacements) {
                Lot lot = map.Tiles[replacement].Lot;
                lot.Building.Type = BuildingType.Headquarters;

                // Add connectors at all four corners
                HashSet<TilePosition> corners = lot.Corners();
                foreach(TilePosition corner in corners) {
                    map.Tiles[corner].Type = TileType.Connector;
                    map.Tiles[corner].MovementCost = 1;
                }
            }



            return replacements;
        }

        void GenerateStreet(MapConfiguration map, NameGenerator nameGenerator, Segment segment, Neighborhood neighborhood) {
            Street street = new Street(map);
            street.Name = nameGenerator.RandomMinorStreet();
            street.Segments.Add(segment);
            map.Streets.Add(street);
            neighborhood.CreateStraightRoad(segment);
        }

        void GenerateNeighborhood(MapConfiguration map, MapTemplate template, int neighborhoodIndex, TilePosition offset) {
            NeighborhoodTemplate neighborhoodTemplate = template.Neighborhoods[neighborhoodIndex];
            Neighborhood neighborhood = new Neighborhood(map, neighborhoodTemplate.Width, neighborhoodTemplate.Height);
            neighborhood.Index = neighborhoodIndex;
            neighborhood.Position = offset;
            map.Neighborhoods.Add(neighborhood);

            // Generate streets
            foreach (Segment segment in neighborhoodTemplate.Roads) {
                GenerateStreet(map, nameGenerator, segment + offset, neighborhood);
            }

            Dictionary<string, LotTemplate> lotTemplates = new Dictionary<string, LotTemplate>();
            foreach(string pattern in template.LotTemplates) {
                LotTemplate lt = new LotTemplate(pattern);
                lotTemplates[pattern] = lt;
            }

            // Generate lots and buildings 
            foreach(TemplateReference blockRef in neighborhoodTemplate.Blocks) {

                if (!template.BlockTemplates.ContainsKey(blockRef.Name))
                    throw new Exception("Template " + blockRef.Name + " not found in BlockTemplates");

                BlockTemplate block = template.BlockTemplates[blockRef.Name];



                foreach(TemplateReference lotRef in block.Lots) {
                    if (!lotTemplates.ContainsKey(lotRef.Name))
                        throw new Exception("Template " + lotRef.Name + " not found in Lot Templates");

                    LotTemplate lotTemplate = lotTemplates[lotRef.Name];

                    TilePosition lotPosition = offset + blockRef.Position + lotRef.Position;
                    Lot lot = new Lot();
                    lot.Anchor = lotPosition;
                    lot.Facing = lotTemplate.Facing;
                    lot.PopulateTiles(lotTemplate.Width, lotTemplate.Height);
                                        
                    Building building = new Building(map.Tiles[lotPosition], lotTemplate.BuildingType, lotTemplate.Facing,
                        lotTemplate.Width, lotTemplate.Height, lotTemplate.BlockPosition, lotTemplate.Variation);
                    lot.Building = building;

                    if(template.BuildingColors.ContainsKey(lotTemplate.BuildingType.ToString())) {
                        lot.Building.Color = RandomUtils.RandomThing<string>(template.BuildingColors[lotTemplate.BuildingType.ToString()], random);
                    }

                    for (int x = 0; x < lotTemplate.Width; x++) {
                        for (int y = 0; y < lotTemplate.Height; y++) {
                            Tile tile = map.Tiles[new TilePosition(x + lotPosition.x, y + lotPosition.y)];
                            tile.Building = building;
                            tile.Lot = lot;
                        }
                    }

                    neighborhood.Lots.Add(lot);
                }
            }
        }

        private void AssignStreets(Neighborhood neighborhood, MapConfiguration map) {
            // Assign streets to lots
            Dictionary<string, int> lotNumbers = new Dictionary<string, int>();

            for (int x = 0; x < neighborhood.Width; x++) {
                for (int y = 0; y < neighborhood.Height; y++) {
                    Tile tile = map.Tiles[new TilePosition(neighborhood.Position.x + x, neighborhood.Position.y + y)];
                    if (tile.Building != null && tile.Lot.Street == null) {
                        Direction facing = tile.Building.FacingDirection;
                        TilePosition next = MapConfiguration.NextTile(tile.Position, facing);

                        if (map.Tiles.ContainsKey(next) && map.Tiles[next].Type == TileType.Road) {
                            Street street = map.FindStreet(next);
                            tile.Lot.Street = street;

                            if (!lotNumbers.ContainsKey(street.Name)) {
                                lotNumbers.Add(street.Name, StartingStreetNumber(facing, neighborhood));
                            }

                            tile.Lot.StreetNumber = lotNumbers[street.Name];
                            lotNumbers[street.Name] += 2;
                        }
                    }
                }
            }
        }

        private int StartingStreetNumber(Direction facing, Neighborhood neighborhood) {
            int start = neighborhood.Index * 100;
            if (facing == Direction.North || facing == Direction.East)
                return start + 2;
            else return start + 1;
        }
    }
}
