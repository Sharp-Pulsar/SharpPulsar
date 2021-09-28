using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvroSchemaGenerator;
using AvroSchemaGenerator.Attributes;
using SharpPulsar.Configuration;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Schema
{
    [Collection(nameof(PulsarTests))]
    public class SchemaUpgradeTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly string _topic;

        public SchemaUpgradeTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
            _topic = $"persistent://public/default/upgradeable-{Guid.NewGuid()}";
        }
        [Fact]
        public void ProduceAndConsume()
        {
            var record1 = AvroSchema<SimpleRecord>.Of(typeof(SimpleRecord));
            var consumerBuilder = new ConsumerConfigBuilder<SimpleRecord>()
                .Topic(_topic)
                .ConsumerName("avroUpgradeSchema1")
                .SubscriptionName("test-sub");
            var consumer = _client.NewConsumer(record1, consumerBuilder);

            var producerBuilder = new ProducerConfigBuilder<SimpleRecord>()
                .Topic(_topic)
                .ProducerName("avroUpgradeSchema1");
            var producer = _client.NewProducer(record1, producerBuilder);

            producer.NewMessage()               
               .Value(new SimpleRecord { Name = "Ebere Abanonu", Age = int.MaxValue})
               .Send();

            Thread.Sleep(TimeSpan.FromSeconds(10));
            var message = consumer.Receive();

            Assert.NotNull(message);
            consumer.Acknowledge(message);
            consumer.Unsubscribe();

            var record2 = AvroSchema<SimpleRecord2>.Of(typeof(SimpleRecord2));
            var consumerBuilder2 = new ConsumerConfigBuilder<SimpleRecord2>()
                .Topic(_topic)
                .ConsumerName("avroUpgradeSchema2")
                .SubscriptionName("test-sub");
            var consumer1 = _client.NewConsumer(record2, consumerBuilder2);

            var producerBuilder2 = new ProducerConfigBuilder<SimpleRecord2>()
                .Topic(_topic)
                .ProducerName("avroUpgradeSchema2");
            var producer2 = _client.NewProducer(record2, producerBuilder2);

            producer2.NewMessage()
               .Value(new SimpleRecord2 { Name = "Ebere", Age = int.MaxValue, Surname = "Abanonu" })
               .Send();

            Thread.Sleep(TimeSpan.FromSeconds(10));
            var msg = consumer1.Receive();

            Assert.NotNull(msg);
            consumer1.Acknowledge(msg);
            consumer1.Unsubscribe();
        }
    }

    public class SimpleRecord
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    [Aliases("SimpleRecord")]
    public class SimpleRecord2
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Surname { get; set; }
    }
}
