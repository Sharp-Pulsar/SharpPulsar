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
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class OTelTest : IDisposable
    {
        private readonly ITestOutputHelper _output;
        
        private readonly string _topic;

        private readonly PulsarClient _client;
        private PulsarSystem _pulsarSystem;


        public OTelTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _pulsarSystem = PulsarSystem.GetInstance(fixture.PulsarClientConfig);

            _client = _pulsarSystem.NewClient();
            _topic = $"persistent://public/default/{Guid.NewGuid()}";
            //var t = TestConsoleExporter.Run();
            //_topic = "my-topic-batch-bf719df3";
        }
        [Fact]
        public async Task ProduceAndConsume()
        {
            var exportedItems = new List<Activity>();
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("producer", "consumer")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("inmemory-test"))
            .AddInMemoryExporter(exportedItems)
            .Build();
            var topic = _topic;

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

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Intercept(new ConsumerOTelInterceptor<byte[]>("consumer", _client.Log))
                .Topic(topic)
                //.StartMessageId(77L, 0L, -1, 0)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest)
                .SubscriptionName($"ByteKeysTest-subscriber-{Guid.NewGuid()}");
            var consumer = await _client.NewConsumerAsync(consumerBuilder);

            await Task.Delay(TimeSpan.FromSeconds(10));
            var message = (Message<byte[]>)await consumer.ReceiveAsync();

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
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => _pulsarSystem.Shutdown().GetAwaiter();
    }
}
