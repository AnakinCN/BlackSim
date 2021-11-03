using SimulationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class PlusEvent : IEvent
    {
        /// <summary>
        /// the testing story
        /// </summary>
        private INumberStory _story;

        /// <summary>
        /// the constructor
        /// </summary>
        /// <param name="story">the testing story</param>
        public PlusEvent(INumberStory story) { _story = story; }

        /// <summary>
        /// the event time in the simulation
        /// </summary>
        public TimeSpan EventTime { get; set; }

        /// <summary>
        /// process the number minus operation
        /// </summary>
        public void Process() { _story.Number++; }
    }
}
