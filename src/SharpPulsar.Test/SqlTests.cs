using SharpPulsar.Schemas;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using SharpPulsar.Interfaces;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;
using SharpPulsar.TestContainer;
using SharpPulsar.Test.Fixture;
using SharpPulsar.Builder;
using Akka.Actor;
using SharpPulsar.Trino;
using SharpPulsar.Trino.Message;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class SqlTests : IAsyncLifetime
    {
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        private ActorSystem _actorSystem;

        public SqlTests(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
            _actorSystem = ActorSystem.Create("Sql");
            //fixture.CreateSql();
        }
        //[Fact(Skip ="Issue with sql-worker on github action")]
        [Fact]
        public virtual async Task TestQuerySql()
        {
            var topic = $"query_topics_avro";
            await PublishMessages(topic, 5);
            var option = new ClientOptions { Server = "http://127.0.0.1:8081", Execute = @$"select * from ""{topic}""", Catalog = "pulsar", Schema = "public/default" };

            var sql = PulsarSystem.Sql(option);

            var receivedCount = 0;

            var response = await sql.ExecuteAsync(TimeSpan.FromSeconds(30));
            if (response != null)
            {
                var data = response.Response;
                switch (data)
                {
                    case DataResponse dr:
                        {
                            for (var i = 0; i < dr.Data.Count; i++)
                            {
                                var ob = dr.Data.ElementAt(i)["text"].ToString();
                                _output.WriteLine(ob);
                                receivedCount++;
                            }
                            _output.WriteLine(JsonSerializer.Serialize(dr.StatementStats, new JsonSerializerOptions { WriteIndented = true }));
                        }
                        break;
                    case StatsResponse sr:
                        _output.WriteLine(JsonSerializer.Serialize(sr.Stats, new JsonSerializerOptions { WriteIndented = true }));
                        break;
                    case ErrorResponse er:
                        _output.WriteLine(JsonSerializer.Serialize(er, new JsonSerializerOptions { WriteIndented = true }));
                        break;
                }
            }

            Assert.True(receivedCount > 1);
        }

        //[Fact(Skip = "Issue with sql-worker on github action")]
        [Fact]
        public async Task TestAvro()
        {
            await PlainAvroProducer($"journal-{Guid.NewGuid()}");
        }
        //[Fact(Skip = "Issue with sql-worker")]
        [Fact]
        public async Task TestKeyValue()
        {
            await PlainKeyValueProducer($"keyvalue");
        }
        private async Task<ISet<string>> PublishMessages(string topic, int count)
        {
            ISet<string> keys = new HashSet<string>();
            var builder = new ProducerConfigBuilder<DataOp>()
                .Topic(topic);
            var producer = await _client.NewProducerAsync(AvroSchema<DataOp>.Of(typeof(DataOp)), builder);
            for (var i = 0; i < count; i++)
            {
                var key = "key" + i;
                await producer.NewMessage().Key(key).Value(new DataOp { Text = "my-sql-message-" + i }).SendAsync();
                keys.Add(key);
            }
            return keys;
        }
        private async Task PlainAvroProducer(string topic)
        {
            var jsonSchem = AvroSchema<JournalEntry>.Of(typeof(JournalEntry));
            
            var producerConfig = new ProducerConfigBuilder<JournalEntry>()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(TimeSpan.FromMilliseconds(10000));

            var producer = await _client.NewProducerAsync(jsonSchem, producerConfig);

            for (var i = 0; i < 10; i++)
            {
                var student = new Students
                {
                    Name = $"[{i}] Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    Age = 202 + i,
                    School = "Akka-Pulsar university"
                };
                var journal = new JournalEntry
                {
                    Id = $"[{i}]Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()}",
                    PersistenceId = "sampleActor",
                    IsDeleted = false,
                    Ordering = 0,
                    Payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(student)),
                    SequenceNr = 0,
                    Tags = "root"
                };
                var metadata = new Dictionary<string, string>
                {
                    ["Key"] = "Single",
                    ["Properties"] = JsonSerializer.Serialize(new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }, new JsonSerializerOptions { WriteIndented = true })
                };
                var id = await producer.NewMessage().Properties(metadata).Value(journal).SendAsync();
            }
            var builder = new ConsumerConfigBuilder<JournalEntry>()
                .Topic(topic)
                .SubscriptionName($"my-subscriber-name-{DateTimeHelper.CurrentUnixTimeMillis()}")
                .AckTimeout(TimeSpan.FromMilliseconds(20000))
                .ForceTopicCreation(true)

                .AcknowledgmentGroupTime(TimeSpan.Zero);
            var consumer = await _client.NewConsumerAsync(jsonSchem, builder);
            await Task.Delay(TimeSpan.FromSeconds(1));
            for (var i = 0; i < 10; i++)
            {
                var msg = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (msg != null)
                {
                    var receivedMessage = msg.Value;
                    _output.WriteLine(JsonSerializer.Serialize(receivedMessage, new JsonSerializerOptions { WriteIndented = true }));
                }

            }
        }

        private async Task PlainKeyValueProducer(string topic)
        {
            //var jsonSchem = AvroSchema<JournalEntry>.Of(typeof(JournalEntry));
            var jsonSchem = KeyValueSchema<string, string>.Of(ISchema<string>.String, ISchema<string>.String);
            
            var producerConfig = new ProducerConfigBuilder<KeyValue<string, string>>()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(TimeSpan.FromMilliseconds(10000));

            var producer = await _client.NewProducerAsync(jsonSchem, producerConfig);

            for (var i = 0; i < 10; i++)
            {
                var metadata = new Dictionary<string, string>
                {
                    ["Key"] = "Single",
                    ["Properties"] = JsonSerializer.Serialize(new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }, new JsonSerializerOptions { WriteIndented = true })
                };
                var id = await producer.NewMessage().Properties(metadata).Value<string, string>(new KeyValue<string, string>("Ebere", $"[{i}]Ebere")).SendAsync();
                _output.WriteLine(id.ToString());
            }
            var builder = new ConsumerConfigBuilder<KeyValue<string, string>>()
                .Topic(topic)
                .SubscriptionName($"subscriber-name-{DateTimeHelper.CurrentUnixTimeMillis()}")
                .AckTimeout(TimeSpan.FromMilliseconds(20000))
                .ForceTopicCreation(true)
                .AcknowledgmentGroupTime(TimeSpan.Zero);
            var consumer = await _client.NewConsumerAsync(jsonSchem, builder);
            await Task.Delay(TimeSpan.FromSeconds(1));
            for (var i = 0; i < 10; i++)
            {
                var msg = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (msg != null)
                {
                    var kv = msg.Value;
                    _output.WriteLine($"key:{kv.Key}, value:{kv.Value}");
                }

            }

        }
        public async Task InitializeAsync()
        {

            _client = await _system.NewClient(_configBuilder);
        }

        public async Task DisposeAsync()
        {
            await _client.ShutdownAsync();
        }
    }
    public class Students
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
    }
    public class DataOp
    {
        public string Text { get; set; }
    }
    public class JournalEntry
    {
        public string Id { get; set; }

        public string PersistenceId { get; set; }

        public long SequenceNr { get; set; }

        public bool IsDeleted { get; set; }

        public byte[] Payload { get; set; }
        public long Ordering { get; set; }
        public string Tags { get; set; }
    }
}
