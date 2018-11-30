using UnityEngine;
using DCTC.Map;
using DCTC.Model;

namespace DCTC.Controllers {
    public class GameController : MonoBehaviour {
        public MapConfiguration Map { get; private set; }
        public Game Game { get; private set; } 

        public static GameController Get() {
            return GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }

        void Awake() {
            GenerateMap();

            //
            Game = new Game();
            Game.NewGame(new NewGameSettings());
        }

        public void GenerateMap() {
            int seed = 100;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            MapGenerator generator = new MapGenerator(new System.Random(seed));
            Map = generator.Generate();

            stopwatch.Stop();
            Debug.Log("Generate time: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        public void Save() {

        }

        public void Load() {

        }
    }
}