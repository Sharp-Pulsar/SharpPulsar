using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.TLS
{
    [Collection(nameof(PulsarTlsTests))]
    public class TlsTests
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly string _topic;

        public TlsTests(ITestOutputHelper output, PulsarTlsStandaloneClusterFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
            _topic = $"persistent://public/default/tls-topic-{Guid.NewGuid()}";
        }

        [Fact]
        public virtual void ProducerInstantiation()
        {
            var producer = new ProducerConfigBuilder<string>();
            producer.Topic(_topic);
            var stringProducerBuilder = _client.NewProducer(new StringSchema(), producer);
            Assert.NotNull(stringProducerBuilder);
        }
        [Fact]
        public virtual void ConsumerInstantiation()
        {
            var consumer = new ConsumerConfigBuilder<string>();
            consumer.Topic(_topic);
            consumer.SubscriptionName($"tls-test-sub-{Guid.NewGuid()}");
            var stringConsumerBuilder = _client.NewConsumer(new StringSchema(), consumer);
            Assert.NotNull(stringConsumerBuilder);
        }
        [Fact]
        public virtual void ReaderInstantiation()
        {
            var reader = new ReaderConfigBuilder<string>();
            reader.Topic(_topic);
            reader.StartMessageId(IMessageId.Earliest);
            var stringReaderBuilder = _client.NewReader(new StringSchema(), reader);
            Assert.NotNull(stringReaderBuilder);
        }
    }
}
