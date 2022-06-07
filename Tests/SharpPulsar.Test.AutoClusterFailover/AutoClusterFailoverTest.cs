using SharpPulsar.Builder;
using SharpPulsar.User;
using Xunit.Abstractions;
using System.Text;
using SharpPulsar.TestContainer;
using SharpPulsar.Test.AutoClusterFailover.Fixture;

namespace SharpPulsar.Test.AutoClusterFailover
{
    [Collection(nameof(AutoCollection))]
    public class AutoClusterFailoverTest 
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly string _topic = $"auto-failover-topic-{Guid.NewGuid()}";
        public AutoClusterFailoverTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
        }
        [Fact]
        public async Task ProduceAndConsume()
        {
            try
            {
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

                await Task.Delay(TimeSpan.FromSeconds(30));
                await Act(consumer, producer);

                await producer.CloseAsync();
                await consumer.CloseAsync();

            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.ToString());
            }

        }

    }
}