using System;

namespace Models
{
    /// <summary>
    /// the Carrier class
    /// </summary>
    public class Carrier
    {
        /// <summary>
        /// the integer ID of carrier 
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// indicates that the carrier has arrived at the kitchen
        /// </summary>
        public bool Arrived { get; private set; }

        /// <summary>
        /// indicates that the carrier has delivered a order
        /// </summary>
        public bool Delivered { get; private set; }

        /// <summary>
        /// the simulation time when the carrier arrived at the kitchen
        /// </summary>
        public TimeSpan ArrivalTime { get; private set; }

        /// <summary>
        /// the wait time since the carrier arrived at the kitchen till delivered the order
        /// </summary>
        public TimeSpan WaitTime { get; private set; }

        /// <summary>
        /// constructor of Carrier class
        /// </summary>
        /// <param name="id">the integer ID of the carrier</param>
        public Carrier(int id)
        {
            ID = id;
            Arrived = false;
            Delivered = false;
        }

        /// <summary>
        /// set the carrier status to 'Delivered' the order
        /// </summary>
        /// <param name="tsDelivered">the simulation time when the carrier delivered the order</param>
        public void SetDelivered(TimeSpan tsDelivered)
        {
            WaitTime = tsDelivered - ArrivalTime;
            Delivered = true;
        }

        /// <summary>
        /// set the carrier status to 'Arrived' at the kitchen
        /// </summary>
        /// <param name="tsArrived">>the simulation time when the carrier arrived at the kitchen</param>
        public void SetArrived(TimeSpan tsArrived)
        {
            Arrived = true;
            ArrivalTime = tsArrived;
        }
    }
}
