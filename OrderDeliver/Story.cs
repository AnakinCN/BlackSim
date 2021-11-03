using Accord.Statistics.Distributions.Univariate;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimulationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace OrderDeliver
{
    /// <summary>
    /// the order delivery story of given challenge
    /// </summary>
    public class Story   //the food deliver story
    {
        /// <summary>
        /// the collection of the Orders
        /// </summary>
        private List<Order> _orders;

        /// <summary>
        /// the cursor of _orders, for generate order received event
        /// </summary>
        private int _orderCursor;

        /// <summary>
        /// the collection of Carriers
        /// </summary>
        private List<Carrier> _carriers;

        /// <summary>
        /// number of total carriers
        /// </summary>
        public int CarrierCount { get; set; }

        /// <summary>
        /// number of total orders delivered
        /// </summary>
        public int TotalOrderDelivered { get; set; }

        /// <summary>
        /// true indicates the First-In-First-Out strategy, false indicates the Matched strategy
        /// </summary>
        public bool FirstInFirstOut { get; set;}

        /// <summary>
        /// the random distribution for order intervals
        /// </summary>
        private ExponentialDistribution _orderIntervalDistribution;

        /// <summary>
        /// the random distribution for carrier arriving intervals
        /// </summary>
        private UniformContinuousDistribution _carrierArrivalDistribution;

        /// <summary>
        /// the simulation strategy in string format
        /// </summary>
        private string _strategy;

        /// <summary>
        /// the Simulation instance
        /// </summary>
        public Simulation Sim { get; private set; }

        /// <summary>
        /// constructor of Story class
        /// </summary>
        /// <param name="firstIn">true indicates the First-In-First-Out strategy, false indicates the Matched strategy</param>
        /// <param name="accelRate">the acceleration rate of simulation, 1.0f by default</param>
        public Story(bool firstIn = false, double accelRate = 1.0f)
        {
            Console.WriteLine("initializing simulation");
            _orderIntervalDistribution = new ExponentialDistribution(2);  //2 delivery orders per second
            _carrierArrivalDistribution = new UniformContinuousDistribution(3, 15); // 3-15 second
            
            _orders = new List<Order>();
            _orderCursor = 0;
            _carriers = new List<Carrier>();
            CarrierCount = 0;
            TotalOrderDelivered = 0;
            FirstInFirstOut = firstIn;
            _strategy = FirstInFirstOut ? "First-In-First-Out" : "Matched";
            _orders = LoadOrders();

            Console.WriteLine($"running story in {_strategy} strategy");
            Sim = new Simulation(accelerationRate: accelRate);
            Sim.Aggregator.GetEvent<PubSubMessage>().Subscribe(_handler);
        }

        private void _handler(Message message)
        {
            switch(message.MessageCode)
            {
                case "AddOneOrderDelivered":
                    AddOneOrderDelivered();
                    break;
                case "BookCarrierArrival":
                    this.BookCarrierArrival((int)message.Parameters["ReceivedOrderID"], (TimeSpan)message.Parameters["EventTime"]);
                    this.BookOrderPrepared((int)message.Parameters["ReceivedOrderID"], (TimeSpan)message.Parameters["EventTime"]);
                    this.BookNextOrder((TimeSpan)message.Parameters["EventTime"]);
                    break;
                case "GetEarlistCarrier":
                    message.Parameters.Add("result", GetEarlistCarrier());
                    break;
                case "FirstInFirstOut":
                    message.Parameters.Add("result", FirstInFirstOut);
                    break;
                case "GetCarrier":
                    message.Parameters.Add("result", GetCarrier((int)message.Parameters["CarrierID"]));
                    break;
                case "GetArbitraryOrder":
                    message.Parameters.Add("result", this.GetArbitraryOrder());
                    break;
                case "MatchedCarrierID":
                    message.Parameters.Add("result", this.GetArbitraryOrder());
                    break;
                case "TotalOrderDelivered":
                    message.Parameters.Add("result", this.TotalOrderDelivered);
                    break;
            }
        }

        /// <summary>
        /// get the count of the simulation's FEL
        /// </summary>
        /// <returns>the count of the simulation's FEL</returns>
        public int GetFELCount()
        {
            return Sim.FEL.Count;
        }

        /// <summary>
        /// running the story
        /// </summary>
        public void Run()
        {
            this.BookNextOrder(TimeSpan.Zero);  //first order
            Sim.Run();
            Console.WriteLine("simulation finished");
            DoStatistics();
            ReviewLogs();
        }

        private void ReviewLogs()
        {
            Console.WriteLine();
            Console.WriteLine("Event Log Replay:");
            Sim.PrintLog();
        }


        /// <summary>
        /// return a carrier by ID
        /// </summary>
        /// <param name="carrierID">the ID of expected carrier</param>
        /// <returns>the expected carrier</returns>
        public Carrier GetCarrier(int carrierID)
        {
            return this._carriers.Where(i => i.ID == carrierID).FirstOrDefault();
        }

        /// <summary>
        /// to print specified message prefixed with the simulation time stamp
        /// </summary>
        /// <param name="message">the message to be printed</param>
        public void Print(string message)
        {
            Sim.Print(message);
        }

        /// <summary>
        /// to print specified message prefixed with the specified time stamp
        /// </summary>
        /// <param name="message">the message to be printed</param>
        /// <param name="time">specified time stamp, usually the event time</param>
        public void Print(string message, TimeSpan time)
        {
            Sim.Print(message,time);
        }

        /// <summary>
        /// total order counting
        /// </summary>
        public void AddOneOrderDelivered()
        {
            this.TotalOrderDelivered++;
        }

        /// <summary>
        /// add a carrier to the _carriers collection
        /// </summary>
        /// <param name="carrier"></param>
        public void AddCarrier(Carrier carrier)
        {
            this._carriers.Add(carrier);
        }

        /// <summary>
        /// print performance statistics
        /// </summary>
        private void DoStatistics()
        {
            Console.WriteLine();
            double orderAverageWait = _orders.Select(i => i.WaitTime.TotalMilliseconds).Average();
            double carrierAverageWait = _carriers.Select(i => i.WaitTime.TotalMilliseconds).Average();
            Console.WriteLine($"in {_strategy} strategy, food average wait {orderAverageWait}, carrier average wait {carrierAverageWait} milliseconds");
        }

        /// <summary>
        /// get next order from _orders collection
        /// </summary>
        /// <returns></returns>
        public Order GetNextOrder()
        {
            if (_orderCursor < _orders.Count)
            {
                return _orders[_orderCursor++];
            }
            return null;
        }

        /// <summary>
        /// get an arbitrary order, which is prepared and not delivered
        /// </summary>
        /// <returns></returns>
        public Order GetArbitraryOrder()
        {
            return _orders.Where(i => i.Prepared && !i.Delivered).FirstOrDefault();
        }

        /// <summary>
        /// get the earliest carrier with min ArrivalTime
        /// </summary>
        /// <returns></returns>
        public Carrier GetEarlistCarrier()
        {
            return _carriers.Where(i => i.Arrived && !i.Delivered).OrderBy(i=>i.ArrivalTime).FirstOrDefault();
        }

        /// <summary>
        /// load orders from the given JSON file
        /// </summary>
        /// <returns></returns>
        public List<Order> LoadOrders()
        {
            List<Order> orders = new List<Order>();
            Console.Write("loading orders from given JSON ... ");
            string jsonfile = "dispatch_orders.json";

            System.IO.StreamReader file = System.IO.File.OpenText(jsonfile);
            JsonTextReader reader = new JsonTextReader(file);
            JArray ja = (JArray)JToken.ReadFrom(reader);
            int id = 0;
            foreach (JObject jo in ja)
            {
                id++;
                Order order = new Order
                (
                    id,
                    (string)jo["id"],       //LongID
                    (string)jo["name"],
                    TimeSpan.FromSeconds((int)(jo["prepTime"]))
                );
                orders.Add(order);
            }

            Console.WriteLine("done");
            return orders;
        }

        /// <summary>
        /// get a random order receiving interval
        /// </summary>
        /// <returns></returns>
        public double GetOrderInterval()
        {
            return _orderIntervalDistribution.Generate();
        }

        /// <summary>
        /// get a random carrier arrival interval
        /// </summary>
        /// <returns></returns>
        public double GetCarrierInterval()
        {
            return _carrierArrivalDistribution.Generate();
        }

        /// <summary>
        /// book a carrier arrival event in the FEL when dispatched
        /// </summary>
        /// <param name="orderID">the integer ID of the order</param>
        /// <param name="eventTime">current time from which the interval is counted</param>
        public void BookCarrierArrival(int orderID, TimeSpan eventTime)     //dispatch and book a carrier arrival event
        {
            Order receivedOrder = this._orders.Where(i => i.ID == orderID).FirstOrDefault();
            TimeSpan carrierArrivalTime =
                eventTime + TimeSpan.FromSeconds(this.GetCarrierInterval());
            Carrier carrier = new Carrier(++this.CarrierCount);
            this.AddCarrier(carrier);
            if (this.FirstInFirstOut)
            {
                this.AddEventLog(new EventLog(
                    "Carrier Dispatched",
                    eventTime,
                    new Dictionary<string, ValueType>()
                    {
                        { "CarrierID" , carrier.ID },
                        { "HasMatchedOrder" , false },
                    }));
                this.Print($"order {receivedOrder.ID} '{receivedOrder.Name}' received, carrier {carrier.ID} dispatched", eventTime);
            }
            else
            {
                receivedOrder.SetMatchedCarrier(carrier.ID);    //dispatch specified carrier
                this.AddEventLog(new EventLog(
                    "Carrier Dispatched",
                    eventTime,
                    new Dictionary<string, ValueType>()
                    {
                        { "CarrierID" , carrier.ID },
                        { "HasMatchedOrder" , true },
                        { "MatchedOrderID" , orderID },
                    }));
                this.Print($"order {receivedOrder.ID} '{receivedOrder.Name}' received, carrier {carrier.ID} assigned and dispatched", eventTime);
            }
            CarrierArrivedEvent carrierArrivalEvent = new CarrierArrivedEvent(carrierArrivalTime, receivedOrder, carrier, this.Sim);
            Sim.AddFutureEvent(carrierArrivalTime, carrierArrivalEvent);
        }

        /// <summary>
        /// book a order prepared event in the FEL
        /// </summary>
        /// <param name="receivedOrderID">the integer ID of the order</param>
        /// <param name="eventTime">current time from which the interval is counted</param>
        public void BookOrderPrepared(int receivedOrderID, TimeSpan eventTime)   //book order prepared event
        {
            Order receivedOrder = _orders.Where(i => i.ID == receivedOrderID).FirstOrDefault();
            TimeSpan PreparedTime = eventTime + receivedOrder.PrepDuration;
            OrderPreparedEvent preparedEvent = new OrderPreparedEvent(PreparedTime, receivedOrder, this.Sim);
            Sim.AddFutureEvent(PreparedTime, preparedEvent);
        }

        /// <summary>
        /// book next order received event in the FEL
        /// </summary>
        /// <param name="eventTime">current time from which the interval is counted</param>
        public void BookNextOrder(TimeSpan eventTime)    //book next order after current
        {
            Order order = this.GetNextOrder();
            if (order == null)
                return;

            TimeSpan nextOrderInterval = TimeSpan.FromSeconds(this.GetOrderInterval());
            TimeSpan bookOrderTime = eventTime + nextOrderInterval;

            OrderReceivedEvent orderArrivalEvent = new OrderReceivedEvent(bookOrderTime, order.ID, this.Sim);
            Sim.AddFutureEvent(bookOrderTime, orderArrivalEvent);
        }

        /// <summary>
        /// add a new event log to the EventLogs collection
        /// </summary>
        /// <param name="log">the EventLog to be added</param>
        public void AddEventLog(EventLog log)
        {
            this.Sim.AddEventLog(log);
        }
    }
}
