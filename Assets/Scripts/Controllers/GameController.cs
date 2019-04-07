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
        private System.Random random;

        private int GameLoopBatchSize {
            get {
                switch(GameSpeed) {
                    case GameSpeed.Pause:
                        return 1;
                    case GameSpeed.Normal:
                        return 10;
                    case GameSpeed.Fast:
                        return 75;
                }
                return 1;
            }
        }

        void Awake() {
            saver = new GameSaver();
            this.GameLoaded += OnGameLoaded;
        }

        private void Start() {
            StateController.Get().PushState(States.Title);
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

        public void QuitToTitle() {

        }

        void GenerateMap(NewGameSettings settings) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            MapGenerator generator = new MapGenerator(random, settings, nameGenerator);
            Map = generator.Generate();
            headquarters = generator.GenerateHeadquarters(Map, settings.NumAIs + settings.NumHumans);

            stopwatch.Stop();
            Debug.Log("Generate map time: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        public void Save() {
            Game.RandomState = Random.state;
            saver.SaveGame(Game, SaveName);
        }

        public void Load() {
            Game = saver.LoadGame(SaveName);

            StateController.Get().ExitAndPushState(States.Loading,
                new Dictionary<string, string>() { { "Company", Game.Settings.PlayerName } });
            StartCoroutine( AsyncLoad() );
        }
        public IEnumerator AsyncLoad() {
            yield return null;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Game.LoadConfig();
            UnityEngine.Random.state = Game.RandomState;
            random = Game.Random;
            nameGenerator = new NameGenerator(random);
            Game.NameGenerator = nameGenerator;
            Game.Player = Game.Companies.First(c => c.OwnerType == CompanyOwnerType.Human);
            Map = saver.LoadMap(SaveName);
            Game.Map = Map;

            stopwatch.Stop();
            Debug.Log("Game load time: " + stopwatch.ElapsedMilliseconds + "ms");

            if (GameLoaded != null)
                GameLoaded();

            yield return null;
        }

        public void NewGameMenu() {
            StateController.Get().ExitAndPushState(States.NewGame);
        }
        public void New(NewGameSettings settings) {
            StateController.Get().ExitAndPushState(States.Loading, 
                new Dictionary<string, string>() { { "Company", settings.PlayerName } });
            StartCoroutine(AsyncNew(settings));
        }
        private IEnumerator AsyncNew(NewGameSettings settings) {
            yield return null;

            random = new System.Random(settings.Seed);
            nameGenerator = new NameGenerator(random);

            //UnityEngine.Profiling.Profiler.BeginSample("generate-map");
            GenerateMap(settings);
            //UnityEngine.Profiling.Profiler.EndSample();

            //UnityEngine.Profiling.Profiler.BeginSample("new-game");
            Game = new Game();
            Game.LoadConfig();
            Game.NewGame(settings, nameGenerator, Map, headquarters);
            Game.PopulateCustomers();
            //UnityEngine.Profiling.Profiler.EndSample();

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
            StateController.Get().ExitAndPushState(States.Map);
        }

        private IEnumerator GameLoop() {
            lastUpdateTime = Time.time;

            while (true) {
                if (GameSpeed == GameSpeed.Pause) {
                    lastUpdateTime = Time.time;
                    yield return null;
                } else {

                    int householdIndex = gameCounter % Game.Customers.Count;
                    Game.Customers[householdIndex].Update(Time.time);

                    foreach (Company company in Game.Companies) {
                        company.Update(Time.time - lastUpdateTime);
                    }

                    lastUpdateTime = Time.time;

                    if (++gameCounter % GameLoopBatchSize == 0) {
                        yield return null;
                    }
                }
                
            }
        }
    }
}