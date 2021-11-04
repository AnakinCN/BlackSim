using BlackSim;
using System;
using System.Collections.Generic;
using Models;
using Prism.Events;

namespace OrderDeliver
{
    /// <summary>
    /// the IEvent class of order prepared
    /// </summary>
    public class OrderPreparedEvent : IEvent
    {
        /// <summary>
        /// the prepared order
        /// </summary>
        private Order _order;

        /// <summary>
        /// the current story
        /// </summary>
        //private IStory _story;

        /// <summary>
        /// the messsage hub
        /// </summary>
        private Simulation _simulation;

        /// <summary>
        /// simulation time of the event
        /// </summary>
        public TimeSpan EventTime { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="eventTime">simulation time of the event</param>
        /// <param name="order">the prepared order</param>
        /// <param name="story">>the story</param>
        public OrderPreparedEvent(TimeSpan eventTime, Order order, Simulation simulation)
        {
            EventTime = eventTime;
            _order = order;
            //_story = story;
            _simulation = simulation;
        }

        /// <summary>
        /// processing when the IEvent takes affect in the simulation loop
        /// </summary>
        public void Process()
        {
            _order.SetPrepared(EventTime);
            Message msg = new Message("FirstInFirstOut", "", new Dictionary<string, object>());
            _simulation.Aggregator.GetEvent<PubSubMessage>().Publish(msg);
            if ((bool)msg.Parameters["result"])
            {
                Message earlestMessage = new Message("GetEarlistCarrier", "", new Dictionary<string, object>());
                _simulation.Aggregator.GetEvent<PubSubMessage>().Publish(earlestMessage);

                //Carrier earliestCarrier = _story.GetEarlistCarrier();
                Carrier earliestCarrier = (Carrier)earlestMessage.Parameters["result"];
                if (earliestCarrier==null)
                {
                    ProcessFirstInWithoutCarrier();
                }
                else
                {
                    ProcessFirstInWithCarrier(earliestCarrier);
                }
            }
            else
            {
                Message matchedMessage = new Message("GetCarrier","",
                    new Dictionary<string, object>()
                    {
                        {"CarrierID", _order.MatchedCarrierID } 
                    });
                //Carrier matchedCarrier = _story.GetCarrier(_order.MatchedCarrierID);
                _simulation.Aggregator.GetEvent<PubSubMessage>().Publish(matchedMessage);
                Carrier matchedCarrier = (Carrier)matchedMessage.Parameters["result"];
                if (matchedCarrier.Arrived)
                {
                    ProcessMatchedWithCarrier(matchedCarrier);
                }
                else
                {
                    ProcessMatchedWithoutCarrier(matchedCarrier);
                }
            }
        }

        /// <summary>
        /// Process the Matched strategy without matched carrier arrived
        /// </summary>
        /// <param name="matchedCarrier"></param>
        private void ProcessMatchedWithoutCarrier(Carrier matchedCarrier)
        {
            _simulation.AddEventLog(new EventLog(
                "Order Prepared",
                EventTime,
                new Dictionary<string, ValueType>(){
                            { "HasMatchedCarrier" , true},
                            { "MatchedCarrierID" , _order.MatchedCarrierID},
                            { "MatchedCarrierAvailable" , false},
                }));
            _simulation.Print($"order '{_order.Name}' prepared but carrier {matchedCarrier.ID} not arrived", EventTime);
        }

        /// <summary>
        /// Process the Matched strategy with matched carrier arrived
        /// </summary>
        /// <param name="matchedCarrier"></param>
        private void ProcessMatchedWithCarrier(Carrier matchedCarrier)
        {
            _order.SetDelivered(EventTime);
            matchedCarrier.SetDelivered(EventTime);
            _simulation.AddEventLog(new EventLog(
                "Order Prepared",
                EventTime,
                new Dictionary<string, ValueType>(){
                            { "OrderID" , _order.ID},
                            { "HasMatchedCarrier" , true},
                            { "MatchedCarrierID" , _order.MatchedCarrierID},
                            { "MatchedCarrierAvailable" , true},
                }));

            _simulation.AddEventLog(new EventLog(
               "Order Picked",
               EventTime,
               new Dictionary<string, ValueType>(){
                           { "OrderID" , _order.ID},
                           { "CarrierID" , matchedCarrier.ID},
                           { "FoodWaitTime" , _order.WaitTime},
                           { "CarrierWaitTime" , matchedCarrier.WaitTime},
               }));

            _simulation.Aggregator.GetEvent<PubSubMessage>().Publish
                (new Message("AddOneOrderDelivered", "", new Dictionary<string, object>()));
            //_story.AddOneOrderDelivered();

            Message totalMessage = new Message("TotalOrderDelivered", "", new Dictionary<string, object>());
            _simulation.Aggregator.GetEvent<PubSubMessage>().Publish(totalMessage);
            int total = (int)totalMessage.Parameters["result"];
            _simulation.Print($"order {_order.ID} '{_order.Name}' prepared and delivered by carrier {matchedCarrier.ID}, carrier waited {matchedCarrier.WaitTime}, {total} orders delivered in total", EventTime);
        }

        /// <summary>
        /// Process the First-In-First-Out strategy with a carrier available
        /// </summary>
        /// <param name="earliestCarrier">the earliest carrier</param>
        private void ProcessFirstInWithCarrier(Carrier earliestCarrier)
        {
            earliestCarrier.SetDelivered(EventTime);
            _order.SetDelivered(EventTime);

            _simulation.AddEventLog(new EventLog(
                "Order Prepared",
                EventTime,
                new Dictionary<string, ValueType>(){
                            { "OrderID" , _order.ID},
                            { "HasMatchedCarrier" , false},
                            { "CarrierAvailable" , true},
                            { "CarrierID" , earliestCarrier.ID},
                }));

            _simulation.AddEventLog(new EventLog(
                "Order Picked",
                EventTime,
                new Dictionary<string, ValueType>(){
                            { "OrderID" , _order.ID},
                            { "CarrierID" , earliestCarrier.ID},
                            { "FoodWaitTime" , _order.WaitTime},
                            { "CarrierWaitTime" , earliestCarrier.WaitTime},
                }));

            _simulation.Aggregator.GetEvent<PubSubMessage>().Publish
                (new Message("AddOneOrderDelivered", "", new Dictionary<string, object>()));
            //_story.AddOneOrderDelivered();

            Message totalMessage = new Message("TotalOrderDelivered", "", new Dictionary<string, object>());
            _simulation.Aggregator.GetEvent<PubSubMessage>().Publish(totalMessage);
            int total = (int)totalMessage.Parameters["result"];
            _simulation.Print($"earliest carrier {earliestCarrier.ID} waited {earliestCarrier.WaitTime} and delivered order {_order.ID} '{_order.Name}', {total} orders delivered in total", EventTime);
        }

        /// <summary>
        /// Process the First-In-First-Out strategy without carrier available
        /// </summary>
        private void ProcessFirstInWithoutCarrier()
        {
            _simulation.AddEventLog(new EventLog(
                "Order Prepared",
                EventTime,
                new Dictionary<string, ValueType>(){
                    { "OrderID" , _order.ID},
                    { "HasMatchedCarrier" , false},
                    { "CarrierAvailable" , false},
                }));
            _simulation.Print($"order {_order.ID} '{_order.Name}' prepared but no carrier available", EventTime);
        }
    }
}