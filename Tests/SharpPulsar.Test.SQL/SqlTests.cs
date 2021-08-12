using SharpPulsar.Configuration;
using SharpPulsar.Schemas;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;
using SharpPulsar.Test.SQL.Fixtures;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using SharpPulsar.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.SQL
{
	[Collection(nameof(PulsarSqlTests))]
	public class SqlTests
	{
		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

		public SqlTests(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		//[Fact(Skip ="Issue with sql-worker on github action")]
		[Fact]
		public virtual void TestQuerySql()
		{
			var topic = $"query_topics_avro";
			PublishMessages(topic, 5);
			var sql = PulsarSystem.NewSql();
			var option = new ClientOptions { Server = "http://127.0.0.1:8081", Execute = @$"select * from ""{topic}""", Catalog = "pulsar", Schema = "public/default" };
			var query = new SqlQuery(option, e => { _output.WriteLine(e.ToString()); }, _output.WriteLine);

            Thread.Sleep(TimeSpan.FromSeconds(10));
            sql.SendQuery(query);
			var receivedCount = 0;

            Thread.Sleep(TimeSpan.FromSeconds(10));
            var response = sql.Read(TimeSpan.FromSeconds(30));
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
                sql.SendQuery(query);
            }

            Assert.True(receivedCount > 1);
        }

        //[Fact(Skip = "Issue with sql-worker on github action")]
        [Fact]
        public void TestAvro()
        {
            PlainAvroProducer($"journal-{Guid.NewGuid()}");
        }
        [Fact(Skip = "Issue with sql-worker on github action")]
        public void TestKeyValue()
        {
            PlainKeyValueProducer($"keyvalue");
        }
		private ISet<string> PublishMessages(string topic, int count)
		{
            ISet<string> keys = new HashSet<string>();
            var builder = new ProducerConfigBuilder<DataOp>()
                .Topic(topic);
            var producer = _client.NewProducer(AvroSchema<DataOp>.Of(typeof(DataOp)), builder);
            for (var i = 0; i < count; i++)
            {
                var key = "key" + i;
                producer.NewMessage().Key(key).Value(new DataOp { Text = "my-sql-message-" + i }).Send();
                keys.Add(key);
            }
            return keys;
        }
        private void PlainAvroProducer(string topic)
        {
            var jsonSchem = AvroSchema<JournalEntry>.Of(typeof(JournalEntry));
            var builder = new ConsumerConfigBuilder<JournalEntry>()
                .Topic(topic)
                .SubscriptionName($"my-subscriber-name-{DateTimeHelper.CurrentUnixTimeMillis()}")
                .AckTimeout(TimeSpan.FromMilliseconds(20000))
                .ForceTopicCreation(true)
                .AcknowledgmentGroupTime(0);
            var consumer = _client.NewConsumer(jsonSchem, builder);
            var producerConfig = new ProducerConfigBuilder<JournalEntry>()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(10000);

            var producer = _client.NewProducer(jsonSchem, producerConfig);

            for (var i = 0; i < 10; i++)
            {
                var student = new Students
                {
                    Name = $"[{i}] Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    Age = 202+i,
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
                    ["Properties"] = JsonSerializer.Serialize(new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }, new JsonSerializerOptions{WriteIndented = true})
                };
                var id = producer.NewMessage().Properties(metadata).Value(journal).Send();
            }
            Thread.Sleep(TimeSpan.FromSeconds(5));
            for (var i = 0; i < 10; i++)
            {
                var msg = consumer.Receive();
                if (msg != null)
                {
                    var receivedMessage = msg.Value;
                    _output.WriteLine(JsonSerializer.Serialize(receivedMessage, new JsonSerializerOptions{WriteIndented = true}));
                }
                
            }
        }

        private void PlainKeyValueProducer(string topic)
        {
            //var jsonSchem = AvroSchema<JournalEntry>.Of(typeof(JournalEntry));
            var jsonSchem = KeyValueSchema<string,string>.Of(ISchema<string>.String, ISchema<string>.String);
            var builder = new ConsumerConfigBuilder<KeyValue<string, string>>()
                .Topic(topic)
                .SubscriptionName($"subscriber-name-{DateTimeHelper.CurrentUnixTimeMillis()}")
                .AckTimeout(TimeSpan.FromMilliseconds(20000))
                .ForceTopicCreation(true)
                .AcknowledgmentGroupTime(0);
            var consumer = _client.NewConsumer(jsonSchem, builder);
            var producerConfig = new ProducerConfigBuilder<KeyValue<string, string>>()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(10000);

            var producer = _client.NewProducer(jsonSchem, producerConfig);

            for (var i = 0; i < 10; i++)
            {
                var metadata = new Dictionary<string, string>
                {
                    ["Key"] = "Single",
                    ["Properties"] = JsonSerializer.Serialize(new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }, new JsonSerializerOptions { WriteIndented = true })
                };
                var id = producer.NewMessage().Properties(metadata).Value<string, string>(new KeyValue<string, string>("Ebere", $"[{i}]Ebere")).Send();
                _output.WriteLine(id.ToString());
            }
            Thread.Sleep(TimeSpan.FromSeconds(5));
            for (var i = 0; i < 10; i++)
            {
                var msg = consumer.Receive();
                if (msg != null)
                {
                    var kv = msg.Value;
                    _output.WriteLine($"key:{kv.Key}, value:{kv.Value}");
                }

            }
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
