﻿using System;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Interfaces;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class DelayedMessage : IAsyncLifetime
    {
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        //private TaskCompletionSource<PulsarClient> _tcs;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        private readonly string _topic;

        public DelayedMessage(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
            _topic = $"persistent://public/default/delayed-{Guid.NewGuid()}";
        }
        [Fact]
        public async Task TestDeliverAfter()
        {

            var numMessages = 5;
            var consumer = await _client.NewConsumerAsync(ISchema<string>.String, new ConsumerConfigBuilder<string>()
                .Topic(_topic)
                .SubscriptionName($"delayed-sub-{Guid.NewGuid()}")

                //deliverat works with shared subscription
                .SubscriptionType(Protocol.Proto.CommandSubscribe.SubType.Shared)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest));

            var producer = await _client.NewProducerAsync(ISchema<string>.String, new ProducerConfigBuilder<string>()
                .Topic(_topic));

            // delay 5 seconds using DeliverAfter
            for (var i = 0; i < numMessages; i++)
            {
                await producer.NewMessage().Value("DeliverAfter message " + i).DeliverAfter(TimeSpan.FromMilliseconds(5000)).SendAsync();
            }
            producer.Flush();

            var numReceived = 0;
            while (numMessages <= 0 || numReceived < numMessages)
            {
                var msg = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (msg == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    continue;
                }
                var dt = (DateTimeHelper.CurrentUnixTimeMillis() - msg.PublishTime) / 1000;
                _output.WriteLine("Consumer Received message : " + msg.Value + "; Difference between publish time and receive time = " + dt + " seconds");
                await consumer.AcknowledgeAsync(msg);
                ++numReceived;
            }

            _output.WriteLine("Successfully received " + numReceived + " messages");
            
        }

        [Fact]
        public async Task TestDeliverAt()
        {

            var numMessages = 15;
            var consumer = await _client.NewConsumerAsync(ISchema<string>.String, new ConsumerConfigBuilder<string>()
                .Topic(_topic)
                .SubscriptionName($"at-sub-{Guid.NewGuid()}")
                //deliverat works with shared subscription
                .SubscriptionType(Protocol.Proto.CommandSubscribe.SubType.Shared)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest)
                );

            var producer = await _client.NewProducerAsync(ISchema<string>.String, new ProducerConfigBuilder<string>()
                .Topic(_topic));

            // delay 5 seconds using DeliverAfter
            for (var i = 0; i < numMessages; i++)
            {
                await producer.NewMessage().Value("DeliverAt message " + i).DeliverAt(DateTimeOffset.UtcNow.AddSeconds(30)).SendAsync();
            }
            producer.Flush();

            var numReceived = 0;
            while (numMessages <= 0 || numReceived < numMessages)
            {
                var msg = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (msg == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    continue;
                }
                _output.WriteLine("Consumer Received message : " + msg.Value + "; Difference between publish time and receive time = " + (DateTimeHelper.CurrentUnixTimeMillis() - msg.PublishTime) / 1000 + " seconds");
                await consumer.AcknowledgeAsync(msg);
                ++numReceived;
            }

            _output.WriteLine("Successfully received " + numReceived + " messages");
        }

        [Fact]
        public async Task TestEventime()
        {

            var numMessages = 5;
            var consumer = await _client.NewConsumerAsync(ISchema<string>.String, new ConsumerConfigBuilder<string>()
                .Topic(_topic)
                .SubscriptionName($"event-sub-{Guid.NewGuid()}")
                .SubscriptionType(Protocol.Proto.CommandSubscribe.SubType.Exclusive)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest));

            var producer = await _client.NewProducerAsync(ISchema<string>.String, new ProducerConfigBuilder<string>()
                .Topic(_topic));

            // delay 5 seconds using DeliverAfter
            for (var i = 0; i < numMessages; i++)
            {
                await producer.NewMessage().Value("Message " + i + " with event time").EventTime(DateTime.Now).SendAsync();
            }
            producer.Flush();

            var numReceived = 0;
            while (numMessages <= 0 || numReceived < numMessages)
            {
                var msg = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (msg == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    continue;
                }
                _output.WriteLine($"Consumer Received message : {msg.Value}; with event time {DateTimeOffset.FromUnixTimeMilliseconds(msg.EventTime)}");
                await consumer.AcknowledgeAsync(msg);
                ++numReceived;
            }

            _output.WriteLine("Successfully received " + numReceived + " messages");
        }
        public async Task InitializeAsync()
        {
            _client = await _system.NewClient(_configBuilder);
        }

        public async Task DisposeAsync()
        {
            await _client.ShutdownAsync();
        }
    }

}
