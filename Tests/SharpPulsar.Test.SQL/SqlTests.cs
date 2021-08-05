using SharpPulsar.Configuration;
using SharpPulsar.Schemas;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;
using SharpPulsar.Test.SQL.Fixtures;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
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
			var topic = $"presto-topics-{Guid.NewGuid()}";
			PublishMessages(topic, 50);
            Thread.Sleep(TimeSpan.FromSeconds(5));
			var sql = PulsarSystem.NewSql();
			var option = new ClientOptions { Server = "http://127.0.0.1:8081", Execute = @$"select * from ""{topic}""", Catalog = "pulsar", Schema = "public/default" };
			var query = new SqlQuery(option, e => { _output.WriteLine(e.ToString()); }, _output.WriteLine);
			sql.SendQuery(query);
			var receivedCount = 0;
            var response = sql.Read(TimeSpan.FromSeconds(30));
            var data = response.Response;
            switch (data)
            {
                case DataResponse dr:
                    {
                        for(var i = 0; i < dr.Data.Count; i++)
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
            sql.SendQuery(query);
            response = sql.Read(TimeSpan.FromSeconds(30));
            Assert.Equal(45, receivedCount);
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
	}
	public class DataOp
    {
		public string Text { get; set; }
    }
}
