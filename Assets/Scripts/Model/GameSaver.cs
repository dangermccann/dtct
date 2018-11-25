﻿using System;
using System.Collections.Generic;
using DCTC.Map;

namespace DCTC.Model {

    public class GameSaver {


        public static SavedMap SaveMap(MapConfiguration map) {
            if (map == null)
                return null;

            SavedMap saved = new SavedMap();

            saved.Width = map.Width;
            saved.Height = map.Height;

            foreach (Tile tile in map.Tiles.Values) {
                TileConfiguration tc = new TileConfiguration();
                tc.Position = tile.Position;
                tc.Type = tile.Type.ToString();
                tc.RoadType = tile.RoadType.ToString();
                tc.MovementCost = tile.MovementCost;
                saved.Tiles.Add(tc);
            }

            foreach (Neighborhood neighborhood in map.Neighborhoods) {
                NeighborhoodConfiguration nc = new NeighborhoodConfiguration();
                nc.Name = neighborhood.Name;
                nc.Width = neighborhood.Width;
                nc.Height = neighborhood.Height;
                nc.Position = neighborhood.Position;
                saved.Neighborhoods.Add(nc);

                foreach (Lot lot in neighborhood.Lots) {
                    LotConfiguration lc = new LotConfiguration();
                    if(lot.Street != null)
                        lc.Street = lot.Street.Name;
                    lc.StreetNumber = lot.StreetNumber;
                    lc.Anchor = lot.Anchor;
                    lc.FacingDirection = lot.Facing.ToString();
                    lc.Tiles.AddRange(lot.Tiles);
                    nc.Lots.Add(lc);

                    if (lot.Building != null) {
                        lc.Building = lot.Building.Anchor;

                        BuildingConfiguration bc = new BuildingConfiguration();
                        bc.Anchor = lot.Building.Anchor;
                        bc.Type = lot.Building.Type.ToString();
                        bc.FacingDirection = lot.Building.FacingDirection.ToString();
                        bc.Width = lot.Building.Width;
                        bc.Height = lot.Building.Height;
                        bc.Color = lot.Building.Color.ToString();
                        nc.Buildings.Add(bc);
                    }
                }

                foreach (Street street in neighborhood.Streets) {
                    StreetConfiguration sc = new StreetConfiguration();
                    sc.Name = street.Name;
                    sc.Size = street.Size.ToString();
                    sc.Segments.AddRange(street.Segments);
                    nc.Streets.Add(sc);
                }
            }
            return saved;
        }


        public static MapConfiguration BuildMap(SavedMap saved) {

            MapConfiguration map = new MapConfiguration(saved.Width, saved.Height);
            foreach (TileConfiguration tc in saved.Tiles) {
                Tile tile = new Tile(tc.Position);
                tile.Type = (TileType)Enum.Parse(typeof(TileType), tc.Type);
                tile.RoadType = (RoadType)Enum.Parse(typeof(RoadType), tc.RoadType);
                tile.MovementCost = tc.MovementCost;
                map.Tiles.Add(tile.Position, tile);
            }

            foreach (NeighborhoodConfiguration nc in saved.Neighborhoods) {
                Neighborhood neighborhood = new Neighborhood(map, nc.Width, nc.Height);
                neighborhood.Name = nc.Name;
                neighborhood.Position = nc.Position;
                map.Neighborhoods.Add(neighborhood);

                Dictionary<string, Street> streets = new Dictionary<string, Street>();
                Dictionary<TilePosition, Lot> lots = new Dictionary<TilePosition, Lot>();

                foreach (StreetConfiguration sc in nc.Streets) {
                    Street street = new Street(map);
                    street.Name = sc.Name;
                    street.Segments.AddRange(sc.Segments);
                    street.Size = (StreetSize)Enum.Parse(typeof(StreetSize), sc.Size);

                    map.Streets.Add(street);
                    neighborhood.Streets.Add(street);
                    streets.Add(street.Name, street);
                }

                foreach (LotConfiguration lc in nc.Lots) {
                    Lot lot = new Lot();
                    lot.Tiles.AddMany(lc.Tiles);
                    lot.Anchor = lc.Anchor;
                    lot.StreetNumber = lc.StreetNumber;
                    lot.Facing = (Direction)Enum.Parse(typeof(Direction), lc.FacingDirection);
                    if(lc.Street != null)
                        lot.Street = streets[lc.Street];
                    neighborhood.Lots.Add(lot);

                    foreach (TilePosition pos in lot.Tiles) {
                        lots[pos] = lot;
                        map.Tiles[pos].Lot = lot;
                    }
                }

                foreach (BuildingConfiguration bc in nc.Buildings) {
                    BuildingType type = (BuildingType)Enum.Parse(typeof(BuildingType), bc.Type);
                    Building building = new Building(map.Tiles[bc.Anchor], type);
                    building.Anchor = bc.Anchor;
                    building.FacingDirection = (Direction)Enum.Parse(typeof(Direction), bc.FacingDirection);
                    building.Width = bc.Width;
                    building.Height = bc.Height;
                    building.Color = (BuildingColor)Enum.Parse(typeof(BuildingColor), bc.Color);

                    lots[building.Anchor].Building = building;
                    foreach (TilePosition pos in lots[building.Anchor].Tiles) {
                        map.Tiles[pos].Building = building;
                    }
                }

            }

            return map;
        }

    }



    public class StreetConfiguration {
        public string Name { get; set; }
        public string Size { get; set; }
        public List<Segment> Segments { get; set; }

        public StreetConfiguration() {
            Segments = new List<Segment>();
        }
    }

    public class BuildingConfiguration {
        public TilePosition Anchor { get; set; }
        public string Type { get; set; }
        public string FacingDirection { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Color { get; set; }

    }

    public class LotConfiguration {
        public List<TilePosition> Tiles { get; set; }
        public TilePosition Anchor { get; set; }
        public string Street { get; set; }
        public int StreetNumber { get; set; }
        public string FacingDirection { get; set; }
        public TilePosition Building { get; set; }


        public LotConfiguration() {
            Tiles = new List<TilePosition>();
        }
    }

    public class NeighborhoodConfiguration {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public TilePosition Position { get; set; }
        public List<BuildingConfiguration> Buildings { get; set; }
        public List<LotConfiguration> Lots { get; set; }
        public List<StreetConfiguration> Streets { get; set; }

        public NeighborhoodConfiguration() {
            Buildings = new List<BuildingConfiguration>();
            Lots = new List<LotConfiguration>();
            Streets = new List<StreetConfiguration>();
        }
    }

    public class TileConfiguration {
        public TilePosition Position { get; set; }
        public string Type { get; set; }
        public string RoadType { get; set; }
        public int MovementCost { get; set; }

        public TileConfiguration() { }
    }

    public class SavedMap {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<TileConfiguration> Tiles { get; set; }
        public List<NeighborhoodConfiguration> Neighborhoods { get; set; }

        public SavedMap() {
            Tiles = new List<TileConfiguration>();
            Neighborhoods = new List<NeighborhoodConfiguration>();
        }
    }
}
