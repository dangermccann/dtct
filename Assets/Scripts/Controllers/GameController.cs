using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Map;
using DCTC.Model;

namespace DCTC.Controllers {
    public delegate void GameEvent();

    public enum GameSpeed {
        Pause = 0,
        Normal = 1,
        Fast = 2
    }

    public class GameController : MonoBehaviour {
        public MapConfiguration Map { get; private set; }
        public Game Game { get; private set; }

        public event GameEvent GameLoaded;
        public event GameEvent SpeedChanged;

        private GameSpeed gameSpeed = GameSpeed.Pause;
        public GameSpeed GameSpeed {
            get { return gameSpeed; }
            set {
                gameSpeed = value;
                if (SpeedChanged != null)
                    SpeedChanged();
            }
        }

        public static GameController Get() {
            return GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }

        private static string SaveName = "dctc";
        private Thread saveMapThread;
        private GameSaver saver;
        private NameGenerator nameGenerator;
        private Coroutine loopCoroutine;
        private int gameCounter = 0;
        private float lastUpdateTime;
        private IList<TilePosition> headquarters;

        private int GameLoopBatchSize {
            get {
                switch(GameSpeed) {
                    case GameSpeed.Pause:
                        return 1;
                    case GameSpeed.Normal:
                        return 100;
                    case GameSpeed.Fast:
                        return 750;
                }
                return 1;
            }
        }

        private NewGameSettings defaultNewGameSettings = new NewGameSettings() {
            NeighborhoodCountX = 2,
            NeighborhoodCountY = 2
        };

        void Awake() {
            saver = new GameSaver();
            this.GameLoaded += OnGameLoaded;
        }

        public void Unpause() {
            GameSpeed = GameSpeed.Normal;

            if(loopCoroutine == null)
                loopCoroutine = StartCoroutine(GameLoop());
        }

        public void Pause() {
            GameSpeed = GameSpeed.Pause;

            if(loopCoroutine != null) {
                StopCoroutine(loopCoroutine);
                loopCoroutine = null;
            }
        }

        void GenerateMap(NewGameSettings settings) {
            int seed = 100;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            System.Random rand = new System.Random(seed);
            nameGenerator = new NameGenerator(rand);
            MapGenerator generator = new MapGenerator(rand, settings, nameGenerator);
            Map = generator.Generate();
            headquarters = generator.GenerateHeadquarters(Map, settings.NumAIs + settings.NumHumans);

            stopwatch.Stop();
            Debug.Log("Generate map time: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        public void Save() {
            Game.RandomState = Random.state;
            saver.SaveGame(Game, SaveName);
        }

        public void Load() { StartCoroutine( AsyncLoad() ); }
        public IEnumerator AsyncLoad() {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Game = saver.LoadGame(SaveName);
            UnityEngine.Random.state = Game.RandomState;
            Game.Player = Game.Companies.First(c => c.OwnerType == CompanyOwnerType.Human);
            Map = saver.LoadMap(SaveName);
            Game.Map = Map;

            stopwatch.Stop();
            Debug.Log("Game load time: " + stopwatch.ElapsedMilliseconds + "ms");

            if (GameLoaded != null)
                GameLoaded();

            yield return null;
        }

        public void New() { StartCoroutine( AsyncNew() ); }
        private IEnumerator AsyncNew() {
            NewGameSettings settings = defaultNewGameSettings;
            GenerateMap(settings);

            Game = new Game();
            Game.NewGame(settings, nameGenerator, Map, headquarters);
            Game.PopulateCustomers(nameGenerator);

            if (GameLoaded != null)
                GameLoaded();

            // Save map in separate thread
            saveMapThread = new Thread(SaveMap);
            saveMapThread.Priority = System.Threading.ThreadPriority.BelowNormal;
            saveMapThread.Start();

            yield return null;
        }

        public void OnMapDrawComplete() {
            Game.PostLoad();
            Unpause();
        }

        private void SaveMap() {
            Thread.Sleep(3000);
            saver.SaveMap(Map, SaveName);
        }

        private void OnGameLoaded() {
            
        }

        private IEnumerator GameLoop() {
            lastUpdateTime = Time.time;

            while (true) {
                

                int householdIndex = gameCounter % Game.Customers.Count;
                Game.Customers[householdIndex].Update(Time.time);

                foreach(Company company in Game.Companies) {
                    company.Update(Time.time - lastUpdateTime);
                }

                if(gameCounter % 1000000 == 0) {
                    Debug.Log("Game Counter: " + gameCounter);
                }

                lastUpdateTime = Time.time;

                if (++gameCounter % GameLoopBatchSize == 0) {
                    yield return null;
                }
                
            }
        }
    }
}