using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Tls.Fixtures;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Tls
{
    [Collection(nameof(PulsarTlsCollection))]
    public class TlsTests
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly string _topic;

        public TlsTests(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
            _topic = $"persistent://public/default/tls-{Guid.NewGuid()}";
        }

        [Fact(Skip = "not ready")]
        public virtual async Task ProducerInstantiation()
        {
            var producer = new ProducerConfigBuilder<string>();
            producer.Topic(_topic);
            var stringProducerBuilder = await _client.NewProducerAsync(new StringSchema(), producer);
            Assert.NotNull(stringProducerBuilder);
            await stringProducerBuilder.CloseAsync();
        }
        [Fact(Skip = "not ready")]
        public virtual async Task ConsumerInstantiation()
        {
            var consumer = new ConsumerConfigBuilder<string>();
            consumer.Topic(_topic);
            consumer.SubscriptionName($"tls-test-sub-{Guid.NewGuid()}");
            var stringConsumerBuilder = await _client.NewConsumerAsync(new StringSchema(), consumer);
            Assert.NotNull(stringConsumerBuilder);
            await stringConsumerBuilder.CloseAsync();
        }
        [Fact(Skip = "not ready")]
        public virtual async void ReaderInstantiation()
        {
            var reader = new ReaderConfigBuilder<string>();
            reader.Topic(_topic);
            reader.StartMessageId(IMessageId.Earliest);
            var stringReaderBuilder = await _client.NewReaderAsync(new StringSchema(), reader);
            Assert.NotNull(stringReaderBuilder);
            await stringReaderBuilder.CloseAsync();
        }

        [Fact(Skip = "not ready")]
        public async Task ProduceAndConsume()
        {
            var topic = $"persistent://public/default/tls-{Guid.NewGuid}";

            var r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var producerBuilder = new ProducerConfigBuilder<byte[]>();
            producerBuilder.Topic(topic);
            var producer = await _client.NewProducerAsync(producerBuilder);

            await producer.NewMessage().KeyBytes(byteKey)
               .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
               .Value(Encoding.UTF8.GetBytes("TestMessage"))
               .SendAsync();

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
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
        [Fact(Skip = "not ready")]
        public async Task ProduceAndConsumeBatch()
        {

            var r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(_topic)
                .ForceTopicCreation(true)
                .SubscriptionName($"Batch-subscriber-{Guid.NewGuid()}");
            var consumer = await _client.NewConsumerAsync(consumerBuilder);


            var producerBuilder = new ProducerConfigBuilder<byte[]>()
                .Topic(_topic)
                .SendTimeout(TimeSpan.FromMilliseconds(10000))
                .EnableBatching(true)
                .BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(120000))
                .BatchingMaxMessages(5);

            var producer = await _client.NewProducerAsync(producerBuilder);

            for (var i = 0; i < 5; i++)
            {
                var id = await producer.NewMessage().KeyBytes(byteKey)
                   .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
                   .Value(Encoding.UTF8.GetBytes($"TestMessage-{i}"))
                   .SendAsync();
                if (id == null)
                    _output.WriteLine($"Id is null");
                else
                    _output.WriteLine($"Id: {id}");
            }
            producer.Flush();
            await Task.Delay(TimeSpan.FromSeconds(10));
            for (var i = 0; i < 5; i++)
            {
                var message = (Message<byte[]>)await consumer.ReceiveAsync();
                if (message != null)
                    _output.WriteLine($"BrokerEntryMetadata[timestamp:{message.BrokerEntryMetadata.BrokerTimestamp} index: {message.BrokerEntryMetadata?.Index.ToString()}");

                Assert.Equal(byteKey, message?.KeyBytes);
                Assert.True(message?.HasBase64EncodedKey());
                var receivedMessage = Encoding.UTF8.GetString(message.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                Assert.Equal($"TestMessage-{i}", receivedMessage);
            }

            await producer.CloseAsync();
            await consumer.CloseAsync();
        }
    }
}
