using System;
using Xunit;
using OrderDeliver;
using BlackSim;
using System.Linq;
using System.Collections.Generic;

namespace Tests
{
    /// <summary>
    /// tests on the given challenge
    /// </summary>
    public class ChallengeStoryTests
    {
        /// <summary>
        /// matched strategy
        /// </summary>
        [Fact]
        public void MatchedStrategyTest()
        {
            Story story = new Story();
            story.Run();
            Assert.Equal(0, story.GetFELCount());
            Assert.Equal(132, story.CarrierCount);
        }

        /// <summary>
        /// matched strategy with 16x acceleration
        /// </summary>
        [Fact]
        public void MatchStrategyAccelerationTest()
        {
            Story story = new Story(firstIn: false, accelRate: 16);
            story.Run();
            Assert.Equal(0, story.GetFELCount());
            Assert.Equal(132, story.CarrierCount);
        }

        /// <summary>
        /// First-In-First_Out strategy with 16x acceleration
        /// </summary>
        [Fact]
        public void FirstInStrategyTest()
        {
            Story story = new Story(firstIn: true);
            story.Run();
            Assert.Equal(0, story.GetFELCount());
            Assert.Equal(132, story.CarrierCount);
        }

        /// <summary>
        /// First-In-First_Out strategy with 16x acceleration
        /// </summary>
        [Fact]
        public void FirstInStrategyAccelerationTest()
        {
            Story story = new Story(firstIn: true, accelRate: 16);
            story.Run();
            Assert.Equal(0, story.GetFELCount());
            Assert.Equal(132, story.CarrierCount);
        }

        /// <summary>
        /// some equations that the Matched story result should match
        /// </summary>
        [Fact]
        public void MatchedCountTest()
        {
            Story story = new Story(firstIn: false, accelRate: 16);
            story.Run();

            List<EventLog> logs = story.Sim.EventLogs;

            Assert.Equal(132, logs.Where(i => i.EventCode == "Order Received").Count());
            Assert.Equal(132, logs.Where(i => i.EventCode == "Carrier Dispatched").Count());
            Assert.Equal(132, logs.Where(i => i.EventCode == "Carrier Arrived").Count());
            Assert.Equal(132, logs.Where(i => i.EventCode == "Order Picked").Count());

            //carrier waited a while
            Assert.Equal(
                logs.Where(i => i.EventCode == "Carrier Arrived" && !(bool)i.Prameters["MatchedOrderPrepared"]).Count(),
                logs.Where(i => i.EventCode == "Order Picked" && (TimeSpan)i.Prameters["CarrierWaitTime"] > TimeSpan.Zero).Count()
                );

            //carrier not waited
            Assert.Equal(
                logs.Where(i => i.EventCode == "Carrier Arrived" && (bool)i.Prameters["MatchedOrderPrepared"]).Count(),
                logs.Where(i => i.EventCode == "Order Picked" && (TimeSpan)i.Prameters["CarrierWaitTime"] == TimeSpan.Zero).Count()
                );

            //not instantly picked when prepared
            Assert.Equal(
                logs.Where(i => i.EventCode == "Order Prepared" && !(bool)i.Prameters["MatchedCarrierAvailable"]).Count(),
                logs.Where(i => i.EventCode == "Carrier Arrived" && (bool)i.Prameters["MatchedOrderPrepared"]).Count()
                );
            Assert.Equal(
               logs.Where(i => i.EventCode == "Order Prepared" && !(bool)i.Prameters["MatchedCarrierAvailable"]).Count(),
               logs.Where(i => i.EventCode == "Order Picked" && (TimeSpan)i.Prameters["FoodWaitTime"] > TimeSpan.Zero).Count()
               );

            //instantly picked when prepared
            Assert.Equal(
               logs.Where(i => i.EventCode == "Order Prepared" && (bool)i.Prameters["MatchedCarrierAvailable"]).Count(),
               logs.Where(i => i.EventCode == "Order Picked" && (TimeSpan)i.Prameters["FoodWaitTime"] == TimeSpan.Zero).Count()
               );
        }

        /// <summary>
        /// some equations that the First-In-First story result should match
        /// </summary>
        [Fact]
        public void FirstInCountTest()
        {
            Story story = new Story(firstIn: true, accelRate: 16);
            story.Run();

            List<EventLog> logs = story.Sim.EventLogs;

            Assert.Equal(132, logs.Where(i => i.EventCode == "Order Received").Count());
            Assert.Equal(132, logs.Where(i => i.EventCode == "Carrier Dispatched").Count());
            Assert.Equal(132, logs.Where(i => i.EventCode == "Carrier Arrived").Count());
            Assert.Equal(132, logs.Where(i => i.EventCode == "Order Picked").Count());

            //carrier waited a while
            Assert.Equal(
                logs.Where(i => i.EventCode == "Carrier Arrived" && !(bool)i.Prameters["AnyOrderPrepared"]).Count(),
                logs.Where(i => i.EventCode == "Order Picked" && (TimeSpan)i.Prameters["CarrierWaitTime"] > TimeSpan.Zero).Count()
                );

            //carrier not waited
            Assert.Equal(
                logs.Where(i => i.EventCode== "Carrier Arrived" && (bool)i.Prameters["AnyOrderPrepared"]).Count(),
                logs.Where(i => i.EventCode == "Order Picked" && (TimeSpan)i.Prameters["CarrierWaitTime"] == TimeSpan.Zero).Count()
                );

            //not instantly picked when prepared
            Assert.Equal(
                logs.Where(i => i.EventCode == "Order Prepared" && !(bool)i.Prameters["CarrierAvailable"]).Count(),
                logs.Where(i => i.EventCode == "Carrier Arrived" && (bool)i.Prameters["AnyOrderPrepared"]).Count()
                );
            Assert.Equal(
               logs.Where(i => i.EventCode == "Order Prepared" && !(bool)i.Prameters["CarrierAvailable"]).Count(),
               logs.Where(i => i.EventCode == "Order Picked" && (TimeSpan)i.Prameters["FoodWaitTime"] > TimeSpan.Zero).Count()
               );

            //instantly picked when prepared
            Assert.Equal(
               logs.Where(i => i.EventCode == "Order Prepared" && (bool)i.Prameters["CarrierAvailable"]).Count(),
               logs.Where(i => i.EventCode == "Order Picked" && (TimeSpan)i.Prameters["FoodWaitTime"]==TimeSpan.Zero).Count()
               );
        }
    }
}
