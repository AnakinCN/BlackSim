using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackSim;

/// <summary>
/// the EventLog class can remember everything happened in the simulation
/// </summary>
public class EventLog
{
    /// <summary>
    /// the event code that define the event type
    /// </summary>
    public string EventCode { get; private set; }

    /// <summary>
    /// the event time
    /// </summary>
    public TimeSpan EventTime { get; private set; }

    /// <summary>
    /// the dictionary that contains every value based parameter
    /// </summary>
    public Dictionary<string, ValueType> Prameters;

    /// <summary>
    /// the constructor
    /// </summary>
    /// <param name="eventCode">the event code that define the event type</param>
    /// <param name="eventTime">the event time in simulation</param>
    /// <param name="parameters">the dictionary that contains every parameter</param>
    public EventLog(string eventCode, TimeSpan eventTime, Dictionary<string, ValueType> parameters)
    {
        EventCode = eventCode;
        EventTime = eventTime;
        Prameters = parameters;
    }
}
