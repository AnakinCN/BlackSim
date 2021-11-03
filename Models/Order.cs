using System;
namespace Models
{

    /// <summary>
    /// the Order class
    /// </summary>
    public class Order
    {
        /// <summary>
        /// the given long ID in the JSON file
        /// </summary>
        public string LongID { get; }

        /// <summary>
        /// the integer ID used in the story
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// the order name given in the JSON file
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// the order prepTime given in the JSON file
        /// </summary>
        public TimeSpan PrepDuration { get; }

        /// <summary>
        /// the status if the order is prepared
        /// </summary>
        public bool Prepared { get; private set; }

        /// <summary>
        /// the status if the order is delivered
        /// </summary>
        public bool Delivered { get; private set; }

        /// <summary>
        /// the integer ID of matched carrier
        /// </summary>
        public int MatchedCarrierID { get; private set; }

        /// <summary>
        /// the simulation time when the order is prepared
        /// </summary>
        public TimeSpan PreparedTime { get; private set; }

        /// <summary>
        /// the wait time since prepared until delivered
        /// </summary>
        public TimeSpan WaitTime { get; private set; }

        /// <summary>
        /// the constructor of the Order class
        /// </summary>
        /// <param name="id">the integer ID</param>
        /// <param name="longId">the long ID given in the JSON file</param>
        /// <param name="name">the order prepTime given in the JSON file</param>
        /// <param name="prepDuration">the order prepTime given in the JSON file</param>
        public Order(int id, string longId, string name, TimeSpan prepDuration)
        {
            this.ID = id;
            this.LongID = longId;
            this.Name = name;
            this.PrepDuration = prepDuration;
            this.Prepared = false;
            this.Delivered = false;
            
        }



        /// <summary>
        /// dispatch a carrier
        /// </summary>
        /// <param name="carrierID">the integer ID of matched carrier</param>
            public void SetMatchedCarrier(int carrierID)
        {
            this.MatchedCarrierID = carrierID;
        }

        /// <summary>
        /// set the order status to 'Prepared'
        /// </summary>
        /// <param name="tsPrepared">the simulation time when the order set 'Prepared'</param>
        public void SetPrepared(TimeSpan tsPrepared)
        {
            Prepared = true;
            PreparedTime = tsPrepared;
        }

        /// <summary>
        /// set the order status to 'Delivered'
        /// </summary>
        /// <param name="tsDelivered">the simulation time when the order set 'Delivered'</param>
        public void SetDelivered(TimeSpan tsDelivered)
        {
            Delivered = true;
            WaitTime = tsDelivered - PreparedTime;
        }
    }

}

