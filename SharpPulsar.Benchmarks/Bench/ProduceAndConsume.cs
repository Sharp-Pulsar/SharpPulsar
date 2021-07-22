using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using SharpPulsar.Configuration;
using SharpPulsar.User;

namespace SharpPulsar.Benchmarks.Bench
{
    [Config(typeof(BenchmarkConfig))]
    [SimpleJob(RunStrategy.Throughput, targetCount: 1, warmupCount: 1)]
    public class ProduceAndConsume
    {
        static string _benchTopic = $"persistent://public/default/benchTopic-8";
        private PulsarClient _client;
        private PulsarSystem _pulsarSystem;
        private Producer<byte[]> _producer;
        private Consumer<byte[]> _consumer;

        [Params(100)]
        public int Iterations;

        [GlobalSetup]
        public void Setup()
        {
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650");
            _pulsarSystem = PulsarSystem.GetInstance(clientConfig);

            _client = _pulsarSystem.NewClient();

            _producer = _client.NewProducer(new ProducerConfigBuilder<byte[]>()
               .Topic(_benchTopic));

            _consumer = _client.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(_benchTopic)
                .ForceTopicCreation(true)
                .SubscriptionName($"bench-sub-{Guid.NewGuid()}")
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest));
        }
        [GlobalCleanup]
        public void Cleanup()
        {
           // _pulsarSystem.Shutdown().GetAwaiter().GetResult();
            _client.Shutdown();
        }

        [Benchmark]
        public void Measure_Publish_Rate()
        {
            PublishMessages(Iterations);
        }
        [Benchmark]
        public void Measure_Consume_Rate()
        {
            ConsumeMessages(Iterations);
        }

        private void PublishMessages(int iterations)
        {
            for(var i = 0; i < iterations; i++)
            {
                var data = Encoding.UTF8.GetBytes($"bench mark [{i}]");
                var id = _producer.NewMessage().Value(data).Send();
                Console.WriteLine($"Message Id({id.LedgerId}:{id.EntryId})");
            }
        }
        private void ConsumeMessages(int iterations)
        {
            for(var i = 0; i < iterations; i++)
            {
                var message = (Message<byte[]>)_consumer.Receive();
                if (message != null)
                {
                    _consumer.Acknowledge(message);
                    var res = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"message '{res}' from bench topic: {message.TopicName}");
                }
            }
        }
    }
}
