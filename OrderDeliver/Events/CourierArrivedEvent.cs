using BlackSim;
using System;
using System.Collections.Generic;
using Models;
using Prism.Events;

namespace OrderDeliver;

/// <summary>
/// the IEvent class of carrier arrived
/// </summary>
class CarrierArrivedEvent : IEvent
{
    /// <summary>
    /// the order that the carrier answers for in the Matched strategy
    /// </summary>
    private Order MatchedOrder;

    /// <summary>
    /// the carrier arrived
    /// </summary>
    private Carrier ArrivedCarrier;

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
    /// <param name="order">the matched order in Matched strategy</param>
    /// <param name="carrier">the carrier</param>
    /// <param name="story">the challenge story</param>
    public CarrierArrivedEvent(TimeSpan eventTime, Order order, Carrier carrier, Simulation simulation)
    {
        EventTime = eventTime;
        MatchedOrder = order;
        ArrivedCarrier = carrier;
        _simulation = simulation;
    }

    /// <summary>
    /// processing when the IEvent takes affect in the simulation loop
    /// </summary>
    public void Process()
    {
        ArrivedCarrier.SetArrived(EventTime);
        Message msg = new Message("FirstInFirstOut", "", new Dictionary<string, object>());
        _simulation.Aggregator.GetEvent<PubSubMessage>().Publish(msg);
        if ((bool)msg.Parameters["result"])
        {
            Message orderMsg = new Message("GetArbitraryOrder","", new Dictionary<string, object>());
            _simulation.Aggregator.GetEvent<PubSubMessage>().Publish(orderMsg);
            Order arbitraryOrder = (Order)orderMsg.Parameters["result"]; 
            if (arbitraryOrder == null)
            {
                ProcessFirstInWithoutOrder();
            }
            else
            {
                ProcessFirstInWithOrder(arbitraryOrder);
            }
        }
        else
        {
            if (MatchedOrder.Prepared)
            {
                ProcessMatchedWithOrder();
            }
            else
            {
                ProcessMatchedWithoutOrder();
            }
        }
    }

    /// <summary>
    /// Process the Matched strategy without Order prepared
    /// </summary>
    private void ProcessMatchedWithoutOrder()
    {
        _simulation.AddEventLog(new EventLog(
            "Carrier Arrived",
            EventTime,
            new Dictionary<string, ValueType>(){
                { "CarrierID" , ArrivedCarrier.ID},
                { "MatchedOrderPrepared" , false}
            }));
        _simulation.Print($"carrier {ArrivedCarrier.ID} arrived, but matched order {MatchedOrder.ID} '{MatchedOrder.Name}' not prepared, start waiting", EventTime);
    }

    /// <summary>
    /// Process the Matched strategy with Order prepared
    /// </summary>
    private void ProcessMatchedWithOrder()
    {
        MatchedOrder.SetDelivered(EventTime);
        ArrivedCarrier.SetDelivered(EventTime);

        _simulation.AddEventLog(new EventLog(
            "Carrier Arrived",
            EventTime,
            new Dictionary<string, ValueType>(){
                { "CarrierID" , ArrivedCarrier.ID},
                { "MatchedOrderPrepared" , true}
            }));

        _simulation.AddEventLog(new EventLog(
            "Order Picked",
            EventTime,
            new Dictionary<string, ValueType>(){
                { "OrderID", MatchedOrder.ID},
                { "CarrierID" , ArrivedCarrier.ID},
                { "FoodWaitTime" , MatchedOrder.WaitTime},
                { "CarrierWaitTime" , ArrivedCarrier.WaitTime},
            }));

        _simulation.Aggregator.GetEvent<PubSubMessage>().Publish
            (new Message("AddOneOrderDelivered" , "", new Dictionary<string, object>()));
        //_story.AddOneOrderDelivered();

        Message totalMessage = new Message("TotalOrderDelivered", "", new Dictionary<string, object>());
        _simulation.Aggregator.GetEvent<PubSubMessage>().Publish
            (totalMessage);
        int total = (int)totalMessage.Parameters["result"];
        _simulation.Print($"carrier {ArrivedCarrier.ID} arrived and matched order {MatchedOrder.ID} delivered: '{MatchedOrder.Name}', with food wait time {MatchedOrder.WaitTime}, {total} orders delivered in total", EventTime);
    }

    /// <summary>
    /// Process the First-In-First-Out strategy with Order prepared
    /// </summary>
    private void ProcessFirstInWithOrder(Order arbitraryOrder)
    {
        arbitraryOrder.SetDelivered(EventTime);
        ArrivedCarrier.SetDelivered(EventTime);

        _simulation.AddEventLog(new EventLog(
            "Carrier Arrived",
            EventTime,
            new Dictionary<string, ValueType>(){
                { "CarrierID" , ArrivedCarrier.ID},
                { "AnyOrderPrepared" , true},
                { "PreparedOrderID" , arbitraryOrder.ID}
            }));

        _simulation.AddEventLog(new EventLog(
            "Order Picked",
            EventTime,
            new Dictionary<string, ValueType>(){
                { "OrderID" , arbitraryOrder.ID},
                { "CarrierID" , ArrivedCarrier.ID},
                { "FoodWaitTime" , arbitraryOrder.WaitTime},
                { "CarrierWaitTime" , ArrivedCarrier.WaitTime},
            }));

        _simulation.Aggregator.GetEvent<PubSubMessage>().Publish(
            new Message(
                "AddOneOrderDelivered", "",
                new Dictionary<string,object>()
                )
            );
        //_story.AddOneOrderDelivered();

        Message totalMessage = new Message("TotalOrderDelivered", "", new Dictionary<string, object>());
        _simulation.Aggregator.GetEvent<PubSubMessage>().Publish
            (totalMessage);
        int total = (int)totalMessage.Parameters["result"];
        _simulation.Print($"carrier {ArrivedCarrier.ID} arrived, took arbitrary order {arbitraryOrder.ID} '{arbitraryOrder.Name}' and delivered, food waited {arbitraryOrder.WaitTime}, {total} orders delivered in total", EventTime);
    }

    /// <summary>
    /// Process the First-In-First-Out strategy without Order prepared
    /// </summary>
    private void ProcessFirstInWithoutOrder()
    {
        _simulation.AddEventLog(new EventLog(
            "Carrier Arrived",
            EventTime,
            new Dictionary<string, ValueType>(){
                { "CarrierID" , ArrivedCarrier.ID},
                { "AnyOrderPrepared" , false}
            }));
        _simulation.Print($"carrier {ArrivedCarrier.ID} arrived but no order prepared, start waiting", EventTime);
    }
}
