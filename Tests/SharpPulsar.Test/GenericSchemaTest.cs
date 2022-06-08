using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Avro.Generic;
using SharpPulsar.Builder;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas;
using SharpPulsar.Schemas.Generic;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{

    [Collection(nameof(PulsarCollection))]
    public class GenericSchemaTest : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly string _topic = $"generic-topic-{Guid.NewGuid()}";

        private readonly PulsarClient _client;
        private PulsarSystem _pulsarSystem;


        public GenericSchemaTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _pulsarSystem = PulsarSystem.GetInstance(fixture.PulsarClientConfig);

            _client = _pulsarSystem.NewClient();
        }
        [Fact]
        public async Task TestGenericTopic()
        {
            var schema = AvroSchema<ComplexGenericData>.Of(typeof(ComplexGenericData));
            var genericSchema = GenericSchema.Of(schema.SchemaInfo);
            _output.WriteLine(schema.SchemaInfo.SchemaDefinition);
            var pBuilder = new ProducerConfigBuilder<IGenericRecord>()
            .Topic(_topic);
            var producer = await _client.NewProducerAsync(genericSchema, pBuilder);

            const int messageCount = 10;
            for (var i = 0; i < messageCount; i++)
            {
                var dataForWriter = new GenericRecord((Avro.RecordSchema)genericSchema.AvroSchema);
                dataForWriter.Add("Feature", "Education");
                dataForWriter.Add("StringData", new Dictionary<string, string> { { "Index", i.ToString() }, { "FirstName", "Ebere" }, { "LastName", "Abanonu" } });
                dataForWriter.Add("ComplexData", ToBytes(new ComplexData { ProductId = i, Point = i * 2, Sales = i * 2 * 5 }));
                var record = new GenericAvroRecord(null, genericSchema.AvroSchema, genericSchema.Fields, dataForWriter);
                var receipt = producer.Send(record);
                _output.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
            }

            var messageReceived = 0;
            var builder = new ConsumerConfigBuilder<IGenericRecord>()
            .Topic(_topic)
            .ForceTopicCreation(true)
            .SubscriptionName($"generic_sub");
            var consumer = await _client.NewConsumerAsync(ISchema<object>.AutoConsume(), builder);
            await Task.Delay(TimeSpan.FromSeconds(5));
            for (var i = 0; i < messageCount; ++i)
            {
                var m = await consumer.ReceiveAsync();
                Assert.NotNull(m);
                var receivedMessage = m.Value;
                var feature = receivedMessage.GetField("Feature").ToString();
                var strinData = (Dictionary<string, object>)receivedMessage.GetField("StringData");
                var complexData = FromBytes<ComplexData>((byte[])receivedMessage.GetField("ComplexData"));
                _output.WriteLine(feature);
                _output.WriteLine(JsonSerializer.Serialize(strinData, new JsonSerializerOptions { WriteIndented = true }));
                _output.WriteLine(JsonSerializer.Serialize(complexData, new JsonSerializerOptions { WriteIndented = true }));
                messageReceived++;
                await consumer.AcknowledgeAsync(m);
            }

            Assert.Equal(10, messageReceived);
            await producer.CloseAsync();
            await consumer.CloseAsync();
        }
        
        private byte[] ToBytes<T>(T obj)
        {
            if (obj == null)
                return null;

            return JsonSerializer.SerializeToUtf8Bytes(obj,
                     new JsonSerializerOptions { WriteIndented = false, IgnoreNullValues = true });
        }

        // Convert a byte array to an Object
        private T FromBytes<T>(byte[] array)
        {
            return JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(array));
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => _pulsarSystem.Shutdown().GetAwaiter();
    }
    public class ComplexGenericData
    {
        public string Feature { get; set; }
        public Dictionary<string, string> StringData { get; set; }
        public byte[] ComplexData { get; set; }

    }
    [Serializable]
    public class ComplexData
    {
        public int ProductId { get; set; }
        public int Point { get; set; }
        public long Sales { get; set; }
    }
}
