using System;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using SharpPulsar.Configuration;
using SharpPulsar.User;

namespace SharpPulsar.Benchmarks.Bench
{
    [Config(typeof(BenchmarkConfig))]
    [SimpleJob(RunStrategy.Throughput, targetCount: 1, warmupCount: 0)]
    public class Producers1000
    {
        private PulsarClient _client;
        private PulsarSystem _pulsarSystem;

        [GlobalSetup]
        public async Task Setup()
        {
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650");
            _pulsarSystem = await PulsarSystem.GetInstanceAsync(clientConfig);

            _client = _pulsarSystem.NewClient();
        }
        [GlobalCleanup]
        public void Cleanup()
        {
            // _pulsarSystem.Shutdown().GetAwaiter().GetResult();
            _client.Shutdown();
        }
        [Benchmark]
        public void A_1_000_producers()
        {
            for (var i = 0; i < 1000; i++)
            {
               var p = _client.NewProducer(new ProducerConfigBuilder<byte[]>()
                    .Topic($"A_1_000_producers-{i}"));
                var data = Encoding.UTF8.GetBytes($"A_1_000_producers-{i}");
                var id = p.NewMessage().Value(data).Send();
                Console.WriteLine($"A_1_000_producers({id.LedgerId}:{id.EntryId})");
            }

            Console.WriteLine("10,000 producers");
        }
    }
}
