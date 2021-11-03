using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Prism.Events;
using SimulationCore;

namespace Tests
{
    public class AggregatorTests
    {
        EventAggregator Aggregator;
        public AggregatorTests()
        {
            Aggregator = new EventAggregator();
            Aggregator.GetEvent<PubSubMessage>().Subscribe(_handler);
        }

        private void _handler(Message message)
        {
            switch(message.MessageCode)
            {
                case "A":
                    message.Parameters.Add("result" , true);
                    break;
            }
        }

        [Fact]
        public void AggregatorTest()
        {
            Message message = new Message("A", "", new Dictionary<string, object>());
            Aggregator.GetEvent<PubSubMessage>().Publish(message);
            Assert.True((bool)message.Parameters["result"]);
        }
    }
}
