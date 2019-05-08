using System;
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
        public MapConfiguration Map { get; set; }
        public Game Game { get; private set; }

        public event GameEvent GameLoaded;
        public event GameEvent SpeedChanged;

        private GameSpeed gameSpeed = GameSpeed.Pause;
        public GameSpeed GameSpeed {
            get { return gameSpeed; }
            set {
                previousGameSpeed = gameSpeed;
                gameSpeed = value;
                if (SpeedChanged != null)
                    SpeedChanged();
            }
        }

        public static GameController Get() {
            return GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }

        private const int householdBatchSize = 100;
        private const string SaveName = "dctc";

        private Thread saveMapThread;
        private GameSaver saver;
        private NameGenerator nameGenerator;
        private float lastUpdateTime = -1;
        private IList<TilePosition> headquarters;
        private System.Random random;
        private float cycleDuration = 0;
        private bool quitting = false;
        private bool applicationPaused = false;
        private object monitor = new object();
        private DateTime startTime;
        private Thread gameLoopThread;
        private DateTime gameStart = DateTime.Now;
        private int householdCycle = 0;
        private int companyCycle = 0;
        private GameSpeed previousGameSpeed = GameSpeed.Normal;

        private int GameLoopBatchSize {
            get {
                switch(GameSpeed) {
                    case GameSpeed.Pause:
                        return 1;
                    case GameSpeed.Normal:
                        return 2;
                    case GameSpeed.Fast:
                        return 5;
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

        void OnApplicationPause(bool pauseStatus) {
            applicationPaused = pauseStatus;
        }

        void OnApplicationQuit() {
            GameSpeed = GameSpeed.Pause;
            ExitGameLoop();

            Debug.Log("Quitting...");
        }

        public void Unpause() {
            Unpause(false);
        }

        public void Unpause(bool forceStart) {
            if (forceStart && previousGameSpeed == GameSpeed.Pause)
                previousGameSpeed = GameSpeed.Normal;

            GameSpeed = previousGameSpeed;
        }

        public void Pause() {
            GameSpeed = GameSpeed.Pause;
        }

        public void QuitToTitle() {

        }

        void StartGameLoop() {
            if (gameLoopThread != null) {
                Debug.LogError("Game loop already running!");
                return;
            }

            quitting = false;

            gameLoopThread = new Thread(new ThreadStart(GameLoop));
            //gameLoopThread.Priority = System.Threading.ThreadPriority.BelowNormal;
            gameLoopThread.Start();
            
        }

        void ExitGameLoop() {
            quitting = true;

            lock (monitor) {
                Monitor.Pulse(monitor);
            }

            if (gameLoopThread != null) {
                gameLoopThread.Join();
                gameLoopThread = null;
            }
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
            ExitGameLoop();

            Game.RandomState = UnityEngine.Random.state;
            saver.SaveGame(Game, SaveName);

            StartGameLoop();
        }

        public void Load() {
            Pause(); // Make sure game loop stops

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
            Game.PostLoad();

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
            Unpause();
            StartGameLoop();
        }

        private void SaveMap() {
            Thread.Sleep(5000);
            saver.SaveMap(Map, SaveName);
        }

        private void OnGameLoaded() {
            StateController.Get().ExitAndPushState(States.Map);
        }

        // Invoke LightUpdate on main thread for all Companies
        // This should just be used to Lerp, move trucks, etc
        private void FixedUpdate() {
            if (GameSpeed != GameSpeed.Pause) {
                float dt = Time.fixedDeltaTime;

                if (GameSpeed == GameSpeed.Fast)
                    dt *= 10f;

                foreach (Company company in Game.Companies) {
                    company.LightUpdate(dt);
                }
            }
        }

        // Main game loop runs on separate thread 
        private void GameLoop() {
            while (true) {
                if (quitting)
                    break;

                if (!applicationPaused && GameSpeed != GameSpeed.Pause) {
                    startTime = DateTime.Now;
                    float time = (float) (startTime - gameStart).TotalMilliseconds;
                    time *= 0.001f;  // convert to seconds

                    // prevent deltaTime from being large on first cycle
                    if (lastUpdateTime == -1)
                        lastUpdateTime = time;

                    float deltaTime = time - lastUpdateTime;
                    
                    for(int i = 0; i < householdBatchSize; i++) {
                        int householdIndex = householdCycle % Game.Customers.Count;
                        Game.Customers[householdIndex].Update(time);
                        householdCycle++;
                    }
                    
                    int companyIndex = companyCycle % Game.Companies.Count;
                    Game.Companies[companyIndex].Update(deltaTime);
                    companyCycle++;

                    lastUpdateTime = time;

                    int sleepAmt = GameSpeed == GameSpeed.Normal ? 10 : 1;

                    lock (monitor) {
                        Monitor.Wait(monitor, TimeSpan.FromMilliseconds(sleepAmt));
                    }

                    cycleDuration = (float)(DateTime.Now - startTime).TotalMilliseconds;

                } else {
                    cycleDuration = 0;

                    lock (monitor) {
                        Monitor.Wait(monitor, TimeSpan.FromMilliseconds(100));
                    }
                }
            }
        }
    }
}