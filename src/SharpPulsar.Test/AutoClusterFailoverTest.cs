using SharpPulsar.Builder;
using Xunit.Abstractions;
using System.Text;
using SharpPulsar.TestContainer;
using SharpPulsar.Test.Fixture;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Akka.Actor;
using SharpPulsar.ServiceProvider;
using Ductus.FluentDocker.Commands;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class AutoClusterFailoverTest : IAsyncLifetime
    {
        private readonly string _topic = $"auto-failover-topic-{Guid.NewGuid()}";

        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        //private TaskCompletionSource<PulsarClient> _tcs;
        private PulsarSystem _system;
       // private PulsarClientConfigBuilder _configBuilder;
        public AutoClusterFailoverTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
           // _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
        }
        [Fact]
        public async Task Auto_ProduceAndConsume()
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

                    await Task.Delay(TimeSpan.FromSeconds(1));
                    var message = (Message<byte[]>)await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));

                    if (message != null)
                        _output.WriteLine($"BrokerEntryMetadata[timestamp:{message.BrokerEntryMetadata?.BrokerTimestamp} index: {message.BrokerEntryMetadata?.Index.ToString()}");

                    Assert.Equal(byteKey, message.KeyBytes);

                    Assert.True(message.HasBase64EncodedKey());
                    var receivedMessage = Encoding.UTF8.GetString(message.Data);
                    _output.WriteLine($"Received message: [{receivedMessage}]. Time: {DateTime.Now.ToLongTimeString()}");
                    Assert.Equal("AutoMessage", receivedMessage);
                }
                await Act(consumer, producer);

                await Task.Delay(TimeSpan.FromSeconds(1));
                await Act(consumer, producer);

                await producer.CloseAsync();
                await consumer.CloseAsync();
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.ToString());
            }

        }
        public async Task InitializeAsync()
        {
            /*_tcs = new TaskCompletionSource<PulsarClient>(TaskCreationOptions.RunContinuationsAsynchronously);
            //_client = fixture.System.NewClient(fixture.ConfigBuilder).AsTask().GetAwaiter().GetResult();
            new Action(async () =>
            {
                var client = await _system.NewClient(_configBuilder);
                _tcs.TrySetResult(client);
            })();
           _client = await _tcs.Task; */
            var auto = new AutoClusterFailover((AutoClusterFailoverBuilder)new AutoClusterFailoverBuilder()
                .Primary("pulsar://localhost:6650")
                .Secondary(new List<string> { "pulsar://localhost:6650" })
                .CheckInterval(TimeSpan.FromSeconds(20))
                .FailoverDelay(TimeSpan.FromSeconds(20))
                .SwitchBackDelay(TimeSpan.FromSeconds(20)));
            var b = new PulsarClientConfigBuilder().ServiceUrlProvider(auto);
            _client = await _system.NewClient(b);
        }

        public async Task DisposeAsync()
        {
            await _client.ShutdownAsync();
        }
    }
}