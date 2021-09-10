using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        private readonly string _topic = $"generic-topic-{Guid.NewGuid()}";

        public GenericSchemaTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
        }
        [Fact]
        public void TestGenericTopic()
        {
            var schema = AvroSchema<ComplexGenericData>.Of(typeof(ComplexGenericData));
            var genericSchema = GenericAvroSchema.Of(schema.SchemaInfo);

            var pBuilder = new ProducerConfigBuilder<IGenericRecord>()
            .Topic(_topic);
            var producer = _client.NewProducer(genericSchema, pBuilder);

            const int messageCount = 10;
            for (var i = 0; i < messageCount; i++)
            {
                var dataForWriter = new GenericRecord((Avro.RecordSchema)genericSchema.AvroSchema);
                dataForWriter.Add("Feature", "Education");
                //dataForWriter.Add("StringData", new Dictionary<string, string> { { "Index", i.ToString() }, { "FirstName", "Ebere"}, { "LastName", "Abanonu" } });
                dataForWriter.Add("ComplexData", ToBytes(new ComplexData { ProductId = i, Point = i*2, Sales = i*2*5}));
                var record = new GenericAvroRecord(null, genericSchema.AvroSchema, genericSchema.Fields, dataForWriter);
                var receipt = producer.Send(record);
                _output.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
            }

            var messageReceived = 0;
            var builder = new ConsumerConfigBuilder<IGenericRecord>()
            .Topic(_topic)
            .ForceTopicCreation(true)
            .SubscriptionName($"generic_sub");
            var consumer = _client.NewConsumer(ISchema<object>.AutoConsume(), builder);
            Thread.Sleep(TimeSpan.FromSeconds(5));
            for (var i = 0; i < messageCount; ++i)
            {
                var m = consumer.Receive();
                Assert.NotNull(m);
                var receivedMessage = m.Value;
                var feature = receivedMessage.GetField("Feature").ToString();
                //var strinData = (Dictionary<string, string>)receivedMessage.GetField("StringData");
                var complexData = FromBytes<ComplexData>((byte[])receivedMessage.GetField("ComplexData"));
                _output.WriteLine(feature);
                //_output.WriteLine(JsonSerializer.Serialize(strinData, new JsonSerializerOptions { WriteIndented = true }));
                _output.WriteLine(JsonSerializer.Serialize(complexData, new JsonSerializerOptions { WriteIndented = true }));
                messageReceived++;
                consumer.Acknowledge(m);
            }

            Assert.Equal(10, messageReceived);
        }
        private byte[] ToBytes<T>(T obj)
        {
            if (obj == null)
                return null;

            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        // Convert a byte array to an Object
        private T FromBytes<T>(byte[] array)
        {
            var memStream = new MemoryStream();
            var binForm = new BinaryFormatter();
            memStream.Write(array, 0, array.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = (T)binForm.Deserialize(memStream);

            return obj;
        }
    }
    public class ComplexGenericData
    {
        public string Feature { get; set; }
        //public Dictionary<string, string> StringData { get; set; }
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
