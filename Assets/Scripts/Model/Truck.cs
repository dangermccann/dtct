using System;
using System.Collections.Generic;
using DCTC.Map;

namespace DCTC.Model {

    public enum TruckStatus {
        Idle,
        EnRoute,
        Working
    }


    [Serializable]
    public class Truck {
        public const float BaseCost = 2f;

        public string ID { get; set; }
        public string Name { get; set; }
        public TilePosition Position { get; set; }
        public string DestinationCustomerID { get; set; }
        public TruckStatus Status { get; set; }
        public List<TilePosition> Path { get; set; }
        public float TravelSpeed { get; set; }
        public float WorkSpeed { get; set; }
        public Inventory<int> Inventory { get; set; }
        public float Salary {
            get {
                return UnityEngine.Mathf.Max(0.2f * TravelSpeed * WorkSpeed, 0.05f);
            }
        }

        [NonSerialized]
        public Game Game;

        [NonSerialized]
        public Company Company;

        [field: NonSerialized]
        public event ChangeDelegate Dispatched;

        public Truck() {
            TravelSpeed = 1;
            WorkSpeed = 1;
            Inventory = new Inventory<int>();
        }

        public void Dispatch(string customerID, IList<TilePosition> path) {
            DestinationCustomerID = customerID;
            Status = TruckStatus.EnRoute;
            Path = new List<TilePosition>(path);

            if (Dispatched != null)
                Dispatched();
        }

        public void DestinationReached() {
            Status = TruckStatus.Working;
        }

        public void JobComplete() {
            Status = TruckStatus.Idle;

            Customer customer = Game.GetCustomer(DestinationCustomerID);
            customer.ServiceTruckArrived();
        }
    }
}