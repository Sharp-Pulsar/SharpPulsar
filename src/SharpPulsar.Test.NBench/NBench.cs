using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using NBench;
using Pro.NBench.xUnit.XunitExtensions;
using SharpPulsar.Builder;
using SharpPulsar.Test.NBench.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.NBench
{
    [Collection(nameof(NBenchCollection))]
    public class NBench
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private Producer<byte[]> _producer;
        private Consumer<byte[]> _consumer; 
        private readonly string _topic;
        public NBench(ITestOutputHelper output, NBenchCollection fixture)
        {
            _output = output;
            _client = fixture.Client;
            _topic = $"persistent://public/default/nbench-{Guid.NewGuid()}";
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new XunitTraceListener(output));
        }

        [PerfSetup]
        internal async Task Setup(BenchmarkContext context)
        {
            var producerBuilder = new ProducerConfigBuilder<byte[]>();
            producerBuilder.Topic(_topic);
            _producer = await _client.NewProducerAsync(producerBuilder);

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
               .Topic(_topic)
               .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest)
               .SubscriptionName($"nbench-subscriber-{Guid.NewGuid()}");
            _consumer = await _client.NewConsumerAsync(consumerBuilder);
        }
        [NBenchFact]
        [PerfBenchmark(Description = "Test to ensure that a minimal throughput test can be rapidly executed.",
            NumberOfIterations = 3, RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 1000, TestMode = TestMode.Test)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, 10000000.0d)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, ByteConstants.ThirtyTwoKb)]
        [GcTotalAssertion(GcMetric.TotalCollections, GcGeneration.Gen2, MustBe.ExactlyEqualTo, 0.0d)]
        public async Task Benchmark()
        {
            await _producer.NewMessage()
             .Value(Encoding.UTF8.GetBytes("TestMessage"))
             .SendAsync();

            var message = (Message<byte[]>)await _consumer.ReceiveAsync();

            if (message != null)
            {
                _output.WriteLine($"BrokerEntryMetadata[timestamp:{message.BrokerEntryMetadata?.BrokerTimestamp} index: {message.BrokerEntryMetadata?.Index.ToString()}");
                var receivedMessage = Encoding.UTF8.GetString(message.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                Assert.Equal("TestMessage", receivedMessage);
            }
        }

        [PerfCleanup]
        internal async Task Cleanup()
        {
            await _consumer.CloseAsync(); 
            await _producer.CloseAsync();   
            await _client.ShutdownAsync();  
        }
    }
}