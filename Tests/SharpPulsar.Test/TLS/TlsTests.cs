using System;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.TLS
{
    //18:09:22.527 [pulsar-io-29-1] INFO org.apache.pulsar.broker.service.ServerCnx - [/172.20.0.1:51604] Failed to authenticate: operation=connect, principal=null, reason=No anonymous role, and no authentication provider configured
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

        [Fact(Skip = "Secured Pulsar Image not available at the server side")]
        public virtual void ProducerInstantiation()
        {
            var producer = new ProducerConfigBuilder<string>();
            producer.Topic(_topic);
            var stringProducerBuilder = _client.NewProducer(new StringSchema(), producer);
            Assert.NotNull(stringProducerBuilder);
        }
        [Fact(Skip = "Secured Pulsar Image not available at the server side")]
        public virtual void ConsumerInstantiation()
        {
            var consumer = new ConsumerConfigBuilder<string>();
            consumer.Topic(_topic);
            consumer.SubscriptionName($"tls-test-sub-{Guid.NewGuid()}");
            var stringConsumerBuilder = _client.NewConsumer(new StringSchema(), consumer);
            Assert.NotNull(stringConsumerBuilder);
        }
        [Fact(Skip = "Secured Pulsar Image not available at the server side")]
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
