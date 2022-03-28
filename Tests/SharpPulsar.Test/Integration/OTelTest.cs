using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Trace;
using SharpPulsar.Builder;
using SharpPulsar.Telemetry.Trace;
using SharpPulsar.Test.Fixture;
using SharpPulsar.Test.OTel;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Integration
{
    [Collection(nameof(IntegrationCollection))]
    public class OTelTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly string _topic;

        public OTelTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
            _topic = $"persistent://public/default/{Guid.NewGuid()}";
            //var t = TestConsoleExporter.Run();
            //_topic = "my-topic-batch-bf719df3";
        }
        [Fact]
        public async Task ProduceAndConsume()
        {

            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetSampler(new AlwaysOnSampler())
            .AddSource("producer", "consumer")
            .AddConsoleExporter()
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
            //producer.Close();
            await consumer.CloseAsync();
        }
    }
}
