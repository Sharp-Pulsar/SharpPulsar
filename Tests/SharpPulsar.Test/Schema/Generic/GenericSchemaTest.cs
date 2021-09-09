using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avro.Generic;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas;
using SharpPulsar.Schemas.Generic;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Schema.Generic
{

    [Collection(nameof(PulsarTests))]
    public class GenericSchemaTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly string _topic = "generic-topic";

        public GenericSchemaTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
        }
        [Fact]
        public void TestGenericTopic()
        {
            var schema = AvroSchema<GenericData>.Of(typeof(GenericData));
            var genericSchema = GenericAvroSchema.Of(schema.SchemaInfo);

            var pBuilder = new ProducerConfigBuilder<IGenericRecord>()
            .Topic(_topic);
            var producer = _client.NewProducer(genericSchema, pBuilder);

            const int messageCount = 10;
            for (var i = 0; i < messageCount; i++)
            {
                var dataForWriter = new GenericRecord((Avro.RecordSchema)genericSchema.AvroSchema);
                dataForWriter.Add("Generic", true);
                dataForWriter.Add("FullName", "Generic Data");
                var record = new GenericAvroRecord(null, genericSchema.AvroSchema, genericSchema.Fields, dataForWriter);
                var receipt = producer.Send(record);
                _output.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
            }

            var messageReceived = 0;
            var builder = new ConsumerConfigBuilder<IGenericRecord>()
            .Topic(_topic)
            .ForceTopicCreation(true)
            .SubscriptionName("generic_sub");
            var consumer = _client.NewConsumer(ISchema<object>.AutoConsume(), builder);
            Thread.Sleep(TimeSpan.FromSeconds(5));
            for (var i = 0; i < messageCount; ++i)
            {
                var m = consumer.Receive();
                Assert.NotNull(m);
                var receivedMessage = m.Value;
                _output.WriteLine($"Received message: [{receivedMessage.GetField("FullName")}]");
                messageReceived++;
                consumer.Acknowledge(m);
            }

            Assert.Equal(10, messageReceived);
        }
    }
    public class GenericData
    {
        public bool Generic { get; set; }
        public string FullName { get; set; }
    }
    public class GenericData2
    {
        public bool Accepted { get; set; }
        public string Address { get; set; }
    }
}
