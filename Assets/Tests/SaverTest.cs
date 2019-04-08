using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;
using DCTC.Map;
using DCTC.Model;

namespace DCTC.Test {
    public class SaverTest {

        [Test]
        public void TestSaveGame() {
            NewGameSettings settings = new NewGameSettings() {
                NeighborhoodCountX = 2,
                NeighborhoodCountY = 2,
                NumAIs = 1
            };

            System.Random random = new System.Random(settings.Seed);
            NameGenerator nameGenerator = new NameGenerator(random);

            MapGenerator generator = new MapGenerator(random, settings, nameGenerator);
            MapConfiguration map = generator.Generate();
            IList<TilePosition> hq = generator.GenerateHeadquarters(map, settings.NumAIs + settings.NumHumans);

            Game game = new Game();
            game.LoadConfig();
            game.NewGame(settings, nameGenerator, map, hq);
            game.PopulateCustomers();

            GameSaver saver = new GameSaver();
            saver.SaveGame(game, "test");
            game = saver.LoadGame("test");

            Assert.NotNull(game);
        }

        [Test]
        public void TestSaveMap() {
            System.Random rand = new System.Random(1);
            NameGenerator nameGenerator = new NameGenerator(rand);
            MapGenerator generator = new MapGenerator(rand, 
                new NewGameSettings() {
                    NeighborhoodCountX = 2,
                    NeighborhoodCountY = 2
                },
                nameGenerator);
            MapConfiguration map = generator.Generate();
            Assert.NotNull(map);
            Assert.Positive(map.Neighborhoods.Count);

            GameSaver saver = new GameSaver();

            SavedMap saved = saver.BuildSavedMap(map);
            Assert.NotNull(saved);

            MapConfiguration restored = saver.BuildMap(saved);
            CompareMaps(saved, saver.BuildSavedMap(restored));
        }

        private static void CompareMaps(SavedMap map1, SavedMap map2) {
            Assert.AreEqual(map1 == null, map2 == null);
            if (map1 != null && map2 != null) {
                Assert.AreEqual(map1.Width, map2.Width);
                Assert.AreEqual(map1.Height, map2.Height);

                for (int i = 0; i < map1.Tiles.Count; i++) {
                    Assert.AreEqual(map1.Tiles[i].Position, map2.Tiles[i].Position);
                    Assert.AreEqual(map1.Tiles[i].Type, map2.Tiles[i].Type);
                    Assert.AreEqual(map1.Tiles[i].RoadType, map2.Tiles[i].RoadType);
                    Assert.AreEqual(map1.Tiles[i].MovementCost, map2.Tiles[i].MovementCost);
                }

                for (int j = 0; j < map1.Streets.Count; j++) {
                    Assert.AreEqual(map1.Streets[j].Name, map2.Streets[j].Name);
                    Assert.AreEqual(map1.Streets[j].Size, map2.Streets[j].Size);

                    for (int k = 0; k < map1.Streets[j].Segments.Count; k++) {
                        Assert.AreEqual(map1.Streets[j].Segments[k].Start, map2.Streets[j].Segments[k].Start);
                        Assert.AreEqual(map1.Streets[j].Segments[k].End, map2.Streets[j].Segments[k].End);
                    }
                }

                for (int i = 0; i < map1.Neighborhoods.Count; i++) {
                    Assert.AreEqual(map1.Neighborhoods[i].Name, map2.Neighborhoods[i].Name);
                    Assert.AreEqual(map1.Neighborhoods[i].Width, map2.Neighborhoods[i].Width);
                    Assert.AreEqual(map1.Neighborhoods[i].Height, map2.Neighborhoods[i].Height);
                    Assert.AreEqual(map1.Neighborhoods[i].Position, map2.Neighborhoods[i].Position);

                    for (int j = 0; j < map1.Neighborhoods[i].Buildings.Count; j++) {
                        Assert.AreEqual(map1.Neighborhoods[i].Buildings[j].Anchor, map2.Neighborhoods[i].Buildings[j].Anchor);
                        Assert.AreEqual(map1.Neighborhoods[i].Buildings[j].Type, map2.Neighborhoods[i].Buildings[j].Type);
                        Assert.AreEqual(map1.Neighborhoods[i].Buildings[j].FacingDirection, map2.Neighborhoods[i].Buildings[j].FacingDirection);
                        Assert.AreEqual(map1.Neighborhoods[i].Buildings[j].Width, map2.Neighborhoods[i].Buildings[j].Width);
                        Assert.AreEqual(map1.Neighborhoods[i].Buildings[j].Height, map2.Neighborhoods[i].Buildings[j].Height);
                        Assert.AreEqual(map1.Neighborhoods[i].Buildings[j].SquareMeters, map2.Neighborhoods[i].Buildings[j].SquareMeters);
                    }

                    for (int j = 0; j < map1.Neighborhoods[i].Lots.Count; j++) {
                        Assert.AreEqual(map1.Neighborhoods[i].Lots[j].Anchor, map2.Neighborhoods[i].Lots[j].Anchor);
                        Assert.AreEqual(map1.Neighborhoods[i].Lots[j].Street, map2.Neighborhoods[i].Lots[j].Street);
                        Assert.AreEqual(map1.Neighborhoods[i].Lots[j].StreetNumber, map2.Neighborhoods[i].Lots[j].StreetNumber);
                        Assert.AreEqual(map1.Neighborhoods[i].Lots[j].FacingDirection, map2.Neighborhoods[i].Lots[j].FacingDirection);
                        Assert.AreEqual(map1.Neighborhoods[i].Lots[j].Building, map2.Neighborhoods[i].Lots[j].Building);

                        for (int k = 0; k < map1.Neighborhoods[i].Lots[j].Tiles.Count; k++) {
                            Assert.AreEqual(map1.Neighborhoods[i].Lots[j].Tiles[k], map2.Neighborhoods[i].Lots[j].Tiles[k]);
                        }
                    }
                }
            }
        }
    }

}