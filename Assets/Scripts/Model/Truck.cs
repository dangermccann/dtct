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
        public string ID { get; set; }
        public string Name { get; set; }
        public TilePosition Position { get; set; }
        public string DestinationCustomerID { get; set; }
        public TruckStatus Status { get; set; }
        public List<TilePosition> Path { get; set; }
        public float Speed { get; set; }
        public float Salary { get; set; }

        [NonSerialized]
        public Game Game;

        [field: NonSerialized]
        public event ChangeDelegate Dispatched;

        public Truck() {
            Salary = 0.1f;
            Speed = 1;
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