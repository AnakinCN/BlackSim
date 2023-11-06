using System;

namespace BlackSim;

/// <summary>
/// the IEvent interface that can be stored in a future event list
/// </summary>
public interface IEvent
{
    /// <summary>
    /// the process when the simulation time comes to current event
    /// </summary>
    public void Process();

    /// <summary>
    /// the simulation time of the event
    /// </summary>
    public TimeSpan EventTime { get; set; }
}
