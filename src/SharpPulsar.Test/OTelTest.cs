using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SharpPulsar.Builder;
using SharpPulsar.Telemetry.Trace;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class OTelTest : IAsyncLifetime
    {
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        public OTelTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
        }
        [Fact]
        public async Task ProduceAndConsume()
        {
            var topic = $"persistent://public/default/{Guid.NewGuid()}";
            var exportedItems = new List<Activity>();
            Sdk.CreateTracerProviderBuilder()
            .AddSource("producer", "consumer")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("inmemory-test"))
            .AddInMemoryExporter(exportedItems)
            .Build();

            var r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var producerBuilder = new ProducerConfigBuilder<byte[]>()
                .Intercept(new ProducerOTelInterceptor<byte[]>("producer", _client.Log))
                .Topic(topic);
            var producer = await _client.NewProducerAsync(producerBuilder);

            await producer.NewMessage().KeyBytes(byteKey)
               .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
               .Value(Encoding.UTF8.GetBytes("TestMessage"))
               .SendAsync();
            await Task.Delay(TimeSpan.FromSeconds(1));
            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Intercept(new ConsumerOTelInterceptor<byte[]>("consumer", _client.Log))
                .Topic(topic)
                //.StartMessageId(77L, 0L, -1, 0)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest)
                .SubscriptionName($"ByteKeysTest-subscriber-{Guid.NewGuid()}");
            var consumer = await _client.NewConsumerAsync(consumerBuilder);

            await Task.Delay(TimeSpan.FromSeconds(10));
            var message = (Message<byte[]>)await consumer.ReceiveAsync(TimeSpan.FromSeconds(10));

            if (message != null)
                _output.WriteLine($"BrokerEntryMetadata[timestamp:{message.BrokerEntryMetadata?.BrokerTimestamp} index: {message.BrokerEntryMetadata?.Index.ToString()}");

            Assert.Equal(byteKey, message.KeyBytes);

            Assert.True(message.HasBase64EncodedKey());
            var receivedMessage = Encoding.UTF8.GetString(message.Data);
            _output.WriteLine($"Received message: [{receivedMessage}]");
            Assert.Equal("TestMessage", receivedMessage);
            await consumer.AcknowledgeAsync(message);
            //producer.Close();
            await consumer.CloseAsync();
            foreach (var activity in exportedItems)
            {
                _output.WriteLine($"ActivitySource: {activity.Source.Name} logged the activity {activity.DisplayName} with tags: {activity.Tags.Count()}");
                foreach (var tag in activity.Tags)
                {
                    _output.WriteLine($"{tag.Key}:{tag.Value}");
                }
            }
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
