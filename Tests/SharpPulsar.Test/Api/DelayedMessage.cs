using System;
using System.Threading;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Api
{
    [Collection(nameof(PulsarTests))]
    public class DelayedMessage
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly string _topic;

        public DelayedMessage(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
            _topic = $"persistent://public/default/delayed-{Guid.NewGuid()}";
        }
        [Fact]
        public void TestDeliverAfter()
        {

            var numMessages = 5;
            var consumer = _client.NewConsumer(ISchema<string>.String, new ConsumerConfigBuilder<string>()
                .Topic(_topic)
                .SubscriptionName($"delayed-sub-{Guid.NewGuid()}")

                //deliverat works with shared subscription
                .SubscriptionType(Protocol.Proto.CommandSubscribe.SubType.Shared)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest));
            
            var producer = _client.NewProducer(ISchema<string>.String, new ProducerConfigBuilder<string>()
                .Topic(_topic));

            // delay 5 seconds using DeliverAfter
            for (var i = 0; i < numMessages; i++)
            {
                producer.NewMessage().Value("DeliverAfter message " + i).DeliverAfter(TimeSpan.FromMilliseconds(5000)).Send();
            }
            producer.Flush();

            var numReceived = 0;
            while (numMessages <= 0 || numReceived < numMessages)
            {
                var msg = consumer.Receive();
                if (msg == null)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    continue;
                }
                var dt = (DateTimeHelper.CurrentUnixTimeMillis() - msg.PublishTime) / 1000;
                _output.WriteLine("Consumer Received message : " + msg.Value + "; Difference between publish time and receive time = " + dt + " seconds");
                consumer.Acknowledge(msg);
                ++numReceived;
            }

            _output.WriteLine("Successfully received " + numReceived + " messages");

        }

        [Fact]
        public void TestDeliverAt()
        {

             var numMessages = 15;
            var consumer = _client.NewConsumer(ISchema<string>.String, new ConsumerConfigBuilder<string>()
                .Topic(_topic)
                .SubscriptionName($"at-sub-{Guid.NewGuid()}")
                //deliverat works with shared subscription
                .SubscriptionType(Protocol.Proto.CommandSubscribe.SubType.Shared)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest)
                );

            var producer = _client.NewProducer(ISchema<string>.String, new ProducerConfigBuilder<string>()
                .Topic(_topic));

            // delay 5 seconds using DeliverAfter
            for (var i = 0; i < numMessages; i++)
            {
                producer.NewMessage().Value("DeliverAt message " + i).DeliverAt(DateTimeOffset.UtcNow.AddSeconds(30)).Send();
            }
            producer.Flush();

            var numReceived = 0;
            while (numMessages <= 0 || numReceived < numMessages)
            {
                var msg = consumer.Receive();
                if (msg == null)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    continue;
                }
                _output.WriteLine("Consumer Received message : " + msg.Data + "; Difference between publish time and receive time = " + (DateTimeHelper.CurrentUnixTimeMillis() - msg.PublishTime) / 1000 + " seconds");
                consumer.Acknowledge(msg);
                ++numReceived;
            }

            _output.WriteLine("Successfully received " + numReceived + " messages");

        }

        [Fact]
        public void TestEventime()
        {

            var numMessages = 5;
            var consumer = _client.NewConsumer(ISchema<string>.String, new ConsumerConfigBuilder<string>()
                .Topic(_topic)
                .SubscriptionName($"event-sub-{Guid.NewGuid()}")
                .SubscriptionType(Protocol.Proto.CommandSubscribe.SubType.Exclusive)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest));

            var producer = _client.NewProducer(ISchema<string>.String, new ProducerConfigBuilder<string>()
                .Topic(_topic));

            // delay 5 seconds using DeliverAfter
            for (var i = 0; i < numMessages; i++)
            {
                producer.NewMessage().Value("Message " + i+" with event time").EventTime(DateTime.Now).Send();
            }
            producer.Flush();

            var numReceived = 0;
            while (numMessages <= 0 || numReceived < numMessages)
            {
                var msg = consumer.Receive();
                if (msg == null)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    continue;
                }
                _output.WriteLine($"Consumer Received message : { msg.Data }; with event time {DateTimeOffset.FromUnixTimeMilliseconds(msg.EventTime)}");
                consumer.Acknowledge(msg);
                ++numReceived;
            }

            _output.WriteLine("Successfully received " + numReceived + " messages");

        }
    }
}
