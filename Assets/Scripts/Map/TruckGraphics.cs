using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCTC.Model;
using DCTC.Controllers;

namespace DCTC.Map {
    public class TruckGraphics : MonoBehaviour {

        private const float baseTravelSpeed = 15;
        private const float baseWorkSpeed = 1.5f;
        private const float baseWork = 1;

        GameObject graphics;
        Quaternion startRotation, destRotation;
        GameController gameController;

        public float rotationSpeed = 900f;

        Truck truck;
        public Truck Truck {
            get { return truck; }
            set {
                truck = value;
                transform.position = truck.Position;
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

        void Update() {
            if (Truck == null || Truck.Status == TruckStatus.Idle)
                return;

            if (gameController.GameSpeed == GameSpeed.Pause)
                return;

            transform.position = Truck.Position;
            graphics.transform.rotation = Quaternion.RotateTowards(graphics.transform.rotation, CalculateRotation(Truck.CurrentIndex), 
                Time.deltaTime * rotationSpeed);
        }


        Quaternion CalculateRotation(int index) {
            float y = 0;

            if (index > 0 & index < Truck.Path.Count) {
                Direction direction = MapConfiguration.RelativeDirection(Truck.Path[index - 1], Truck.Path[index]);
                
                switch (direction) {
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
            }

            return Quaternion.Euler(new Vector3(0, y, 0));
        }
    }

}