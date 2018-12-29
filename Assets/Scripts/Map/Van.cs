using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.Map {
    public class Van : MonoBehaviour {

        public float speed = 10;

        GameObject graphics;
        int currentIndex = 0;
        Vector3 currentDestination, currentStart;
        Quaternion startRotation, destRotation;
        float elapsed = 0;
        bool running = false;
        GameController gameController;

        List<TilePosition> path;
        public List<TilePosition> Path {
            get {
                return path;
            }
            set {
                path = value;
                if (path.Count > 1)
                    Go();
                else
                    Pause();
            }
        }

        Truck truck;
        public Truck Truck {
            get { return truck; }
            set {
                truck = value;
                truck.Dispatched += () => { Path = truck.Path; };
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

        public void Pause() {
            running = false;
            Truck.DestinationReached();
        }

        public void Go() {
            running = true;
            currentIndex = 0;
            transform.position = ThreeDMap.PositionToWorld(path[0]);
            Next();
        }


        void Update() {
            if (!running || gameController.GameSpeed == GameSpeed.Pause)
                return;

            if( ( transform.position - currentDestination).magnitude < 0.05f ) {
                Next();
            }
            elapsed += Time.deltaTime * speed *  (float) gameController.GameSpeed;

            transform.position = Vector3.Lerp(currentStart, currentDestination, elapsed);
            graphics.transform.rotation = Quaternion.Lerp(startRotation, destRotation, 5.0f * elapsed);
        }

        void Next() {
            currentIndex++;

            if (currentIndex >= path.Count) {
                Pause();
                return;
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
        }
    }

}