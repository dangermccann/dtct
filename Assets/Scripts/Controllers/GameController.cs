using System.Linq;
using System.Threading;
using System.Collections;
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
        private NameGenerator nameGenerator;

        void Awake() {
            //New();
            saver = new GameSaver();
        }

        public void GenerateMap(NewGameSettings settings) {
            int seed = 100;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            System.Random rand = new System.Random(seed);
            nameGenerator = new NameGenerator(rand);
            MapGenerator generator = new MapGenerator(rand, settings, nameGenerator);
            Map = generator.Generate();

            stopwatch.Stop();
            Debug.Log("Generate map time: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        public void Save() {
            saver.SaveGame(Game, SaveName);
        }

        public void Load() { StartCoroutine( AsyncLoad() ); }
        public IEnumerator AsyncLoad() {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Game = saver.LoadGame(SaveName);
            Game.Player = Game.Companies.First(c => c.OwnerType == CompanyOwnerType.Human);
            Map = saver.LoadMap(SaveName);

            stopwatch.Stop();
            Debug.Log("Game load time: " + stopwatch.ElapsedMilliseconds + "ms");

            if (GameLoaded != null)
                GameLoaded();

            yield return null;
        }

        public void New() { StartCoroutine( AsyncNew() ); }
        private IEnumerator AsyncNew() {
            NewGameSettings settings = new NewGameSettings();
            GenerateMap(settings);

            Game = new Game();
            Game.NewGame(settings, nameGenerator);
            Game.PopulateCustomers(Map, nameGenerator);

            if (GameLoaded != null)
                GameLoaded();

            // Save map in separate thread
            saveMapThread = new Thread(SaveMap);
            saveMapThread.Priority = System.Threading.ThreadPriority.BelowNormal;
            saveMapThread.Start();

            yield return null;
        }

        private void SaveMap() {
            Thread.Sleep(3000);
            saver.SaveMap(Map, SaveName);
        }
    }
}