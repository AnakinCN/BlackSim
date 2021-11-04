using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prism.Mvvm;
using Prism;
using Prism.Events;

namespace BlackSim
{
    /// <summary>
    /// the very tiny simulation core that supports a story
    /// </summary>
    public class Simulation  //the simulation engine
    {
        /// <summary>
        /// the message hub
        /// </summary>
        public EventAggregator Aggregator { get; set; }

        /// <summary>
        /// the Future Event List
        /// </summary>
        public SortedList<TimeSpan, IEvent> FEL { get; private set; }
        
        /// <summary>
        /// the event logs remembering everything that happened in the simulation
        /// </summary>
        public List<EventLog> EventLogs;

        /// <summary>
        /// time of the simulation
        /// </summary>
        public TimeSpan SimulationTime { get; private set; } 

        /// <summary>
        /// the acceleration rate, 1.0f by default
        /// </summary>
        private double _accelerationRate;

        /// <summary>
        /// Constructor of Simulation class
        /// </summary>
        /// <param name="accelerationRate">the acceleration rate of simulation, 1.0f by default</param>
        public Simulation(double accelerationRate = 1.0f)
        {
            SimulationTime = TimeSpan.Zero;
            FEL = new SortedList<TimeSpan, IEvent>();
            EventLogs = new List<EventLog>();
            _accelerationRate = accelerationRate;

            Aggregator = new EventAggregator();
        }

        /// <summary>
        /// Run the simulation till the FEL is empty
        /// </summary>
        public void Run()
        {
            TimeSpan runStart = SimulationTime;     //for new run, relaying stopped SimulationTime

            Stopwatch stopWatch = new Stopwatch();  //better accuracy than DateTime.Now
            stopWatch.Start();
            
            do
            {
                SimulationTime = runStart + stopWatch.Elapsed * _accelerationRate;
            }
            while (ProcessFEL());  //the simulation loop
            stopWatch.Stop();
        }

        /// <summary>
        /// to book a future event in the FEL that implements the IEvent interface
        /// </summary>
        /// <param name="simTime">simulation time of the future event</param>
        /// <param name="futureEvent">the IEvent to be added</param>
        /// <returns>the actual time arranged in the FEL</returns>
        public TimeSpan AddFutureEvent(TimeSpan simTime, IEvent futureEvent)
        {
            while(FEL.ContainsKey(simTime))     //if there is a IEvent with the same simulation time, it should be slightly postponed
            {
                simTime += TimeSpan.FromMilliseconds(0.0001);
            }
            this.FEL.Add(simTime, futureEvent);
            return simTime;
        }

        /// <summary>
        /// process the FEL from the top, when a IEvent is finished, it is removed from the FEL
        /// </summary>
        /// <returns></returns>
        private bool ProcessFEL()   //the simulation core
        {
            if (FEL.Count == 0)
                return false;

            while (FEL.Count > 0 && FEL.First().Key < SimulationTime)
            {
                IEvent currentEvent = FEL.First().Value;    //always process the top future event
                currentEvent.Process();
                FEL.RemoveAt(0);                            //remove from the FEL
            };

            return true;
        }

        /// <summary>
        /// to print specified message prefixed with the simulation time stamp
        /// </summary>
        /// <param name="message">the message to be printed</param>
        public void Print(string message)
        {
            Console.WriteLine($"[{SimulationTime}] {message}");
        }

        /// <summary>
        /// to print specified message prefixed with specified time stamp, the event time usually
        /// </summary>
        /// <param name="message">the message to be printed</param>
        /// <param name="time">specified time stamp</param>
        public void Print(string message, TimeSpan time)
        {
            Console.WriteLine($"[{time}] {message}");
        }

        /// <summary>
        /// add a new event log to the EventLogs collection
        /// </summary>
        /// <param name="log">the EventLog to be remembered</param>
        public void AddEventLog(EventLog log)
        {
            this.EventLogs.Add(log);
        }

        /// <summary>
        /// print all event logs
        /// </summary>
        public void PrintLog()
        {
            Console.WriteLine("-------------------------- Event Logs -------------------------------");
            foreach (EventLog log in EventLogs)
            {
                Console.Write($"[{log.EventTime}] {log.EventCode} - ");
                foreach (string key in log.Prameters.Keys)
                {
                    Console.Write($" {key}: {log.Prameters[key]}, ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("---------------------------------------------------------------------");
        }
    }
}
