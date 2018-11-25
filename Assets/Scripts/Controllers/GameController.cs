using UnityEngine;
using DCTC.Map;

namespace DCTC.Controllers {
    public class GameController : MonoBehaviour {
        public MapConfiguration Map { get; private set; }

        public static GameController Get() {
            return GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }

        void Awake() {
            GenerateMap();
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
    }
}