﻿using System.Linq;
using System.Threading;
using UnityEngine;
using DCTC.Map;
using DCTC.Model;

namespace DCTC.Controllers {
    public delegate void GameLoadedEvent();

    public class GameController : MonoBehaviour {
        public MapConfiguration Map { get; private set; }
        public Game Game { get; private set; }

        public event GameLoadedEvent GameLoaded;

        public static GameController Get() {
            return GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }

        private static string SaveName = "dctc";
        private Thread saveMapThread;
        private GameSaver saver;

        void Awake() {
            //New();
            saver = new GameSaver();
        }

        public void GenerateMap() {
            int seed = 100;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            MapGenerator generator = new MapGenerator(new System.Random(seed));
            Map = generator.Generate();

            stopwatch.Stop();
            Debug.Log("Generate map time: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        public void Save() {
            saver.SaveGame(Game, SaveName);
        }

        public void Load() {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Game = saver.LoadGame(SaveName);
            Game.Player = Game.Companies.First(c => c.OwnerType == CompanyOwnerType.Human);
            Map = saver.LoadMap(SaveName);

            stopwatch.Stop();
            Debug.Log("Game load time: " + stopwatch.ElapsedMilliseconds + "ms");

            if (GameLoaded != null)
                GameLoaded();
        }

        public void New() {
            GenerateMap();

            Game = new Game();
            Game.NewGame(new NewGameSettings());

            if (GameLoaded != null)
                GameLoaded();

            // Save map in separate thread
            saveMapThread = new Thread(SaveMap);
            saveMapThread.Start();
        }

        private void SaveMap() {
            saver.SaveMap(Map, SaveName);
        }
    }
}