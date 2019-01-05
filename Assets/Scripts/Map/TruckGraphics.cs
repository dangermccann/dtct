using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.Map {
    public class TruckGraphics : MonoBehaviour {

        private const float baseTravelSpeed = 15;
        private const float baseWorkSpeed = 2;
        private const float baseWork = 1;

        GameObject graphics;
        int currentIndex = 0;
        Vector3 currentDestination, currentStart;
        Quaternion startRotation, destRotation;
        float elapsed = 0;
        float workRemaining = 0;
        bool running = false;
        bool destinationReached = false;
        GameController gameController;

        List<TilePosition> path;
        public List<TilePosition> Path {
            get {
                return path;
            }
            set {
                path = value;
                if (path.Count > 1)
                    BeginJob();
                else
                    CompleteJob();
            }
        }

        Truck truck;
        public Truck Truck {
            get { return truck; }
            set {
                truck = value;
                truck.Dispatched += OnDispatched;
            }
        }

        public TilePosition Position {
            get {
                return ThreeDMap.WorldToPosition(transform.position);
            }
        }

        void Start() {
            graphics = transform.GetChild(0).gameObject;
            gameController = GameController.Get();
        }

        private void OnDestroy() {
            if (truck != null)
                truck.Dispatched -= OnDispatched;
        }

        private void OnDispatched() {
            Path = truck.Path;
        }

        public void CompleteJob() {
            running = false;
            Truck.JobComplete();
        }

        public void BeginJob() {
            running = true;
            currentIndex = 0;
            workRemaining = baseWork;
            destinationReached = false;
            transform.position = ThreeDMap.PositionToWorld(path[0]);
            NextTile();
        }


        void Update() {
            if (!running || gameController.GameSpeed == GameSpeed.Pause)
                return;

            if(destinationReached) {
                workRemaining -= Time.deltaTime * baseWorkSpeed * Truck.Speed * (float)gameController.GameSpeed;
                if (workRemaining <= 0)
                    CompleteJob();

                return;
            }

            if ( ( transform.position - currentDestination).magnitude < 0.05f ) {
                destinationReached = NextTile();

                if (destinationReached)
                    Truck.DestinationReached();
            }

            elapsed += Time.deltaTime * baseTravelSpeed * Truck.Speed * (float)gameController.GameSpeed;

            transform.position = Vector3.Lerp(currentStart, currentDestination, elapsed);
            graphics.transform.rotation = Quaternion.Lerp(startRotation, destRotation, 5.0f * elapsed);
        }

        bool CheckWorkRemaining() {
            if (workRemaining <= 0) {
                return true;
            } else {
                workRemaining -= Time.deltaTime * Truck.Speed * (float)gameController.GameSpeed;
                return false;
            }
        }

        bool NextTile() {
            currentIndex++;

            if (currentIndex >= path.Count) {
                return true;
            }

            startRotation = graphics.transform.rotation;

            Direction direction = MapConfiguration.RelativeDirection(path[currentIndex - 1], path[currentIndex]);

            float y = 0;
            switch(direction) {
                case Direction.North:
                    y = 0;
                    break;
                case Direction.East:
                    y = 90;
                    break;
                case Direction.South:
                    y = 180;
                    break;
                case Direction.West:
                    y = 270;
                    break;
            }

            destRotation = Quaternion.Euler(new Vector3(0, y, 0));

            currentStart = transform.position;
            elapsed = 0;
            currentDestination = ThreeDMap.PositionToWorld(path[currentIndex]);
            Truck.Position = path[currentIndex];

            return false;
        }
    }

}