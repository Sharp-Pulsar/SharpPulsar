using Xunit;
using System.Collections.Generic;
using System;
using SharpPulsar.Builder;
using SharpPulsar.User;
using SharpPulsar.Test.Fixture;
using Xunit.Abstractions;
using SharpPulsar.TestContainer;
using System.Text;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace SharpPulsar.Test.ServiceProvider
{
    [Collection(nameof(IntegrationCollection))]
    public class AutoClusterFailoverTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly string _topic = $"auto-failover-topic-{Guid.NewGuid()}";
        private readonly ITestcontainersBuilder<TestcontainersContainer> _builder;
        private readonly PulsarTestcontainer _container;    
        public AutoClusterFailoverTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _builder = new TestcontainersBuilder<TestcontainersContainer>()
            .WithName("primary-cluster")
            .WithPulsar(fixture.Configuration)
            .WithPortBinding(6655, 6650)
            .WithPortBinding(8088, 8080)
            .WithPortBinding(8082, 8081)
            .WithExposedPort(6650)
            .WithExposedPort(8080)
            .WithExposedPort(8081);
            _output = output;
            _client = fixture.Client;
            _container = fixture.Container;
        }
        [Fact]
        public async Task ProduceAndConsume()
        {
            await using (var testcontainers = _builder.Build())
            {
                await testcontainers.StartAsync();
                var topic = _topic;         

                var producerBuilder = new ProducerConfigBuilder<byte[]>();
                producerBuilder.Topic(topic);
                var producer = await _client.NewProducerAsync(producerBuilder);

                var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                    .Topic(topic)
                    .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest)
                    .SubscriptionName($"subscriber-{Guid.NewGuid()}");
                var consumer = await _client.NewConsumerAsync(consumerBuilder);

                async Task Act(Consumer<byte[]> consumer, Producer<byte[]> producer)
                {
                    var r = new Random(0);
                    var byteKey = new byte[1000];
                    r.NextBytes(byteKey);

                    await producer.NewMessage().KeyBytes(byteKey)
                   .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
                   .Value(Encoding.UTF8.GetBytes("AutoMessage"))
                   .SendAsync();

                    await Task.Delay(TimeSpan.FromSeconds(10));
                    var message = (Message<byte[]>)await consumer.ReceiveAsync();

                    if (message != null)
                        _output.WriteLine($"BrokerEntryMetadata[timestamp:{message.BrokerEntryMetadata?.BrokerTimestamp} index: {message.BrokerEntryMetadata?.Index.ToString()}");

                    Assert.Equal(byteKey, message.KeyBytes);

                    Assert.True(message.HasBase64EncodedKey());
                    var receivedMessage = Encoding.UTF8.GetString(message.Data);
                    _output.WriteLine($"Received message: [{receivedMessage}]");
                    Assert.Equal("AutoMessage", receivedMessage);
                }
                await Act(consumer, producer);
                await _container.StopAsync();

                await Task.Delay(TimeSpan.FromSeconds(30));
                await Act(consumer, producer);
                
                //producer.Close();
                await consumer.CloseAsync();
            }            
        }

    }
}