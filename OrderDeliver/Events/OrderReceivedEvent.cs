using Prism.Events;
using BlackSim;
using System;
using System.Collections.Generic;

namespace OrderDeliver;

/// <summary>
/// the IEvent class of order received
/// </summary>
class OrderReceivedEvent : IEvent
{
    /// <summary>
    /// the integer ID of received order
    /// </summary>
    private int _receivedOrderID;

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
    /// <param name="orderID">the integer ID of received order</param>
    /// <param name="story">the story</param>
    public OrderReceivedEvent(TimeSpan eventTime, int orderID, Simulation simulation)
    {
        EventTime = eventTime;
        _receivedOrderID = orderID;
        _simulation = simulation;
    }

    /// <summary>
    /// processing when the IEvent takes effect in the simulation loop
    /// </summary>
    public void Process()
    {
        _simulation.AddEventLog(new EventLog(
            "Order Received",
            EventTime,
            new Dictionary<string, ValueType>()
            {
                { "OrderID" , _receivedOrderID},
            }));
        _simulation.Aggregator.GetEvent<PubSubMessage>().Publish(
            new Message("BookCarrierArrival", _receivedOrderID.ToString(), new Dictionary<string, object>()
            {
                { "ReceivedOrderID",_receivedOrderID },
                { "EventTime", EventTime}}
            ));

        Message bookMessage = new Message("BookCarrierArrival", "", new Dictionary<string, object>() 
        { 
            { "ReceivedOrderID",_receivedOrderID },
            { "EventTime" , EventTime} }
        );

        //_story.BookCarrierArrival(_receivedOrderID, EventTime);
        //_story.BookOrderPrepared(_receivedOrderID, EventTime);
        //_story.BookNextOrder(EventTime);
    }
}
