using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Auth;
using SharpPulsar.Builder;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class TokenTests : IDisposable
    {
        private readonly CancellationTokenSource _cts;
        private readonly ITestOutputHelper _output;
        private PulsarSystem _system;
        private ClientConfigBuilder _configBuilder;
        private PulsarFixture _fixture;
        public TokenTests(ITestOutputHelper output, PulsarFixture fixture)
        {
            _cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            _fixture = fixture;
            _output = output;
            _system = fixture.System;
        }

        [Fact]
        public virtual async Task Token_ProducerInstantiation()
        {
            var c = await CreateCient();
            var client = await _system.NewClient(c.builder);
            var producer = new ProducerConfigBuilder<string>();
            producer.Topic(c.topic);
            var stringProducerBuilder = await client!.NewProducerAsync(new StringSchema(), producer);
            Assert.NotNull(stringProducerBuilder);
            await stringProducerBuilder.CloseAsync();
            //_client.Dispose();
        }
        [Fact]
        public virtual async Task Token_ConsumerInstantiation()
        {
            var c = await CreateCient();
            var client = await _system.NewClient(c.builder);
            var consumer = new ConsumerConfigBuilder<string>();
            consumer.Topic(c.topic);
            consumer.SubscriptionName($"token-test-sub-{Guid.NewGuid()}");
            var stringConsumerBuilder = await client!.NewConsumerAsync(new StringSchema(), consumer);
            Assert.NotNull(stringConsumerBuilder);
            await stringConsumerBuilder.CloseAsync();
            //_client.Dispose();
        }
        [Fact]
        public virtual async Task Token_ReaderInstantiation()
        {
            var c = await CreateCient();
            var client = await _system.NewClient(c.builder);
            var reader = new ReaderConfigBuilder<string>();
            reader.Topic(c.topic);
            reader.StartMessageId(IMessageId.Earliest);
            var stringReaderBuilder = await client!.NewReaderAsync(new StringSchema(), reader);
            Assert.NotNull(stringReaderBuilder);
            await stringReaderBuilder.CloseAsync();
            //_client.Dispose();
        }

        [Fact]
        public async Task Token_ProduceAndConsume()
        {
            var c = await CreateCient();
            var client = await _system.NewClient(c.builder);

            var r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var producerBuilder = new ProducerConfigBuilder<byte[]>();
            producerBuilder.Topic(c.topic);
            var producer = await client!.NewProducerAsync(producerBuilder);

            await producer.NewMessage().KeyBytes(byteKey)
               .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
               .Value(Encoding.UTF8.GetBytes("TestMessage"))
               .SendAsync();

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(c.topic)
                //.StartMessageId(77L, 0L, -1, 0)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest)
                .SubscriptionName($"ByteKeysTest-subscriber-{Guid.NewGuid()}");
            var consumer = await client.NewConsumerAsync(consumerBuilder);

            await Task.Delay(TimeSpan.FromSeconds(10));
            var message = (Message<byte[]>)await consumer.ReceiveAsync();

            if (message != null)
                _output.WriteLine($"BrokerEntryMetadata[timestamp:{message.BrokerEntryMetadata?.BrokerTimestamp} index: {message.BrokerEntryMetadata?.Index.ToString()}");

            Assert.Equal(byteKey, message!.KeyBytes);

            Assert.True(message.HasBase64EncodedKey());
            var receivedMessage = Encoding.UTF8.GetString(message.Data);
            _output.WriteLine($"Received message: [{receivedMessage}]");
            Assert.Equal("TestMessage", receivedMessage);
            //producer.Close();
            await consumer.CloseAsync();
            //_client.Dispose();
        }
        [Fact]
        public async Task Token_ProduceAndConsumeBatch()
        {
            var c = await CreateCient();
            var client = await _system.NewClient(c.builder);
            var r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(c.topic)
                .ForceTopicCreation(true)
                .SubscriptionName($"Batch-subscriber-{Guid.NewGuid()}");
            var consumer = await client!.NewConsumerAsync(consumerBuilder);


            var producerBuilder = new ProducerConfigBuilder<byte[]>()
                .Topic(c.topic)
                .SendTimeout(TimeSpan.FromMilliseconds(10000))
                .EnableBatching(true)
                .BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(120000))
                .BatchingMaxMessages(5);

            var producer = await client.NewProducerAsync(producerBuilder);

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
                var receivedMessage = Encoding.UTF8.GetString(message!.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                Assert.Equal($"TestMessage-{i}", receivedMessage);
            }

            await producer.CloseAsync();
            await consumer.CloseAsync();
            //_client.Dispose();
        }
        
        private async Task<string> CreateToken()
        {
            var token = await _fixture.Container!.ExecAsync(new List<string> { @"./bin/pulsar", "tokens", "create", "--secret-key", "/pulsar/secret.key", "--subject", "test-user" });
            return token.Stdout;
        }
        private async ValueTask<(ClientConfigBuilder builder, string topic)> CreateCient()
        {
            var client = new ClientConfigBuilder();
            var serviceUrl = "pulsar://localhost:6650";
            //var webUrl = "http://localhost:8080";
            client.ServiceUrl(serviceUrl);
            //client.WebUrl(webUrl);

            client.Authentication(AuthenticationFactory.Token(await CreateToken()));
            client.ServiceUrl(serviceUrl);
            //client.WebUrl(webUrl);
            _configBuilder = client;
            var topic = $"persistent://public/default/token-{Guid.NewGuid()}";
            return (client, topic);
        }
        public void Dispose() => _cts.Dispose();
    }
}
