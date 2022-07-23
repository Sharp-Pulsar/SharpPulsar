using System;
using System.Threading.Tasks;
using AvroSchemaGenerator.Attributes;
using SharpPulsar.Schemas;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;
using SharpPulsar.Test.Fixture;
using SharpPulsar.Builder;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class SchemaUpgradeTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        public SchemaUpgradeTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;

            _client = fixture.Client;
        }
        //[Fact(Skip ="A")]
        [Fact]
        public async Task SchemaProduceAndConsume()
        {
            var topic = $"persistent://public/default/upgradeable-{Guid.NewGuid()}";
            var record1 = AvroSchema<SimpleRecord>.Of(typeof(SimpleRecord));
           
            var producerBuilder = new ProducerConfigBuilder<SimpleRecord>()
                .Topic(topic)
                .ProducerName("avroUpgradeSchema1");
            var producer = await _client.NewProducerAsync(record1, producerBuilder);
            for (var i = 1; i < 2; i++)
            {
                await producer.NewMessage()
               .Value(new SimpleRecord { Name = "Ebere Abanonu", Age = int.MaxValue })
               .SendAsync();

            }

            var consumerBuilder = new ConsumerConfigBuilder<SimpleRecord>()
               .Topic(topic)
               .ConsumerName("avroUpgradeSchema1")
               .SubscriptionName("test-sub");
            var consumer = await _client.NewConsumerAsync(record1, consumerBuilder);

            await Task.Delay(TimeSpan.FromSeconds(10));
            var message = await consumer.ReceiveAsync();

            Assert.NotNull(message);
            await consumer.AcknowledgeAsync(message);
            consumer.Unsubscribe();
            topic = $"persistent://public/default/upgradeable-{Guid.NewGuid()}";
            var record2 = AvroSchema<SimpleRecord2>.Of(typeof(SimpleRecord2));
            
            var producerBuilder2 = new ProducerConfigBuilder<SimpleRecord2>()
                .Topic(topic)
                .ProducerName("avroUpgradeSchema2");
            var producer2 = await _client.NewProducerAsync(record2, producerBuilder2);

            for (var i = 1; i < 2; i++)
            {
                await producer2.NewMessage()
               .Value(new SimpleRecord2 { Name = "Ebere", Age = int.MaxValue, Surname = "Abanonu" })
               .SendAsync();
            }
           

            var consumerBuilder2 = new ConsumerConfigBuilder<SimpleRecord2>()
                .Topic(topic)
                .ConsumerName("avroUpgradeSchema2")
                .SubscriptionName("test-sub");
            var consumer1 = await _client.NewConsumerAsync(record2, consumerBuilder2);

            await Task.Delay(TimeSpan.FromSeconds(10));
            var msg = await consumer1.ReceiveAsync();

            Assert.NotNull(msg);
            await consumer1.AcknowledgeAsync(msg);
            consumer1.Unsubscribe();

            await producer.CloseAsync();
            await producer2.CloseAsync();
            await consumer.CloseAsync();
            await consumer1.CloseAsync();
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
