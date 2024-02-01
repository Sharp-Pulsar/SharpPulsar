using System.Text;
using Akka.Configuration;
using SharpCompress;
using SharpPulsar.Auth;
using SharpPulsar.Auth.OAuth2;
using SharpPulsar.Builder;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Token.Fixture;
using SharpPulsar.TestContainer;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Token
{
    [Collection(nameof(PulsarTokenCollection))]
    public class TokenTests : IAsyncLifetime
    {
        private readonly ITestOutputHelper _output;
        private readonly string _topic;
        private PulsarClient? _client;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        public TokenTests(ITestOutputHelper output, PulsarTokenFixture fixture)
        {
            var s = fixture.Container.ExecAsync(new List<string> { @"./bin/pulsar", "tokens", "create", "--secret-key", "/pulsar/secret.key", "--subject", "test-user" })
                .GetAwaiter()
                .GetResult();
            _output = output;
            var client = new PulsarClientConfigBuilder();
            var serviceUrl = "pulsar://localhost:6650";
            var webUrl = "http://localhost:8080";
            client.ServiceUrl(serviceUrl);
            client.WebUrl(webUrl);
            
            client.Authentication(AuthenticationFactory.Token(s.Stdout));
            client.ServiceUrl(serviceUrl);
            client.WebUrl(webUrl);
            _system = PulsarSystem.GetInstance(actorSystemName: "token", config: ConfigurationFactory.ParseString(@"
            akka
            {
                loglevel = DEBUG
			    log-config-on-start = on 
                loggers=[""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
			    actor 
                {              
				      debug 
				      {
					      receive = on
					      autoreceive = on
					      lifecycle = on
					      event-stream = on
					      unhandled = on
				      }  
			    }
                coordinated-shutdown
                {
                    exit-clr = on
                }
            }"));
            _configBuilder = client;           
            _topic = $"persistent://public/default/token-{Guid.NewGuid()}";
        }

        [Fact]
        public virtual async Task Token_ProducerInstantiation()
        {
            var producer = new ProducerConfigBuilder<string>();
            producer.Topic(_topic);
            var stringProducerBuilder = await _client!.NewProducerAsync(new StringSchema(), producer);
            Assert.NotNull(stringProducerBuilder);
            await stringProducerBuilder.CloseAsync();
            _client.Dispose();
        }
        [Fact]
        public virtual async Task Token_ConsumerInstantiation()
        {
            var consumer = new ConsumerConfigBuilder<string>();
            consumer.Topic(_topic);
            consumer.SubscriptionName($"token-test-sub-{Guid.NewGuid()}");
            var stringConsumerBuilder = await _client!.NewConsumerAsync(new StringSchema(), consumer);
            Assert.NotNull(stringConsumerBuilder);
            await stringConsumerBuilder.CloseAsync();
            _client.Dispose();
        }
        [Fact]
        public virtual async void Token_ReaderInstantiation()
        {
            var reader = new ReaderConfigBuilder<string>();
            reader.Topic(_topic);
            reader.StartMessageId(IMessageId.Earliest);
            var stringReaderBuilder = await _client!.NewReaderAsync(new StringSchema(), reader);
            Assert.NotNull(stringReaderBuilder);
            await stringReaderBuilder.CloseAsync();
            _client.Dispose();
        }

        [Fact]
        public async Task Token_ProduceAndConsume()
        {
            var topic = $"persistent://public/default/token-{Guid.NewGuid}";

            var r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var producerBuilder = new ProducerConfigBuilder<byte[]>();
            producerBuilder.Topic(topic);
            var producer = await _client!.NewProducerAsync(producerBuilder);

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

            Assert.Equal(byteKey, message!.KeyBytes);

            Assert.True(message.HasBase64EncodedKey());
            var receivedMessage = Encoding.UTF8.GetString(message.Data);
            _output.WriteLine($"Received message: [{receivedMessage}]");
            Assert.Equal("TestMessage", receivedMessage);
            //producer.Close();
            await consumer.CloseAsync();
            _client.Dispose();
        }
        [Fact]
        public async Task Token_ProduceAndConsumeBatch()
        {

            var r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(_topic)
                .ForceTopicCreation(true)
                .SubscriptionName($"Batch-subscriber-{Guid.NewGuid()}");
            var consumer = await _client!.NewConsumerAsync(consumerBuilder);


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
                var receivedMessage = Encoding.UTF8.GetString(message!.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                Assert.Equal($"TestMessage-{i}", receivedMessage);
            }

            await producer.CloseAsync();
            await consumer.CloseAsync();
            _client.Dispose();
        }
        public async Task InitializeAsync()
        {

            _client = await _system.NewClient(_configBuilder);
        }

        public async Task DisposeAsync()
        {
            await _client!.ShutdownAsync();
        }
    }
}
