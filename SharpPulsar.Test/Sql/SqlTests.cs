using AvroSchemaGenerator;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Messages;
using SharpPulsar.Schemas;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Sql
{
	[Collection(nameof(PulsarTests))]
	public class SqlTests
	{
		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

		public SqlTests(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		[Fact]
		public virtual void TestQuerySql()
		{
			string topic = $"presto-topics-aef6d294-d12c-46db-b551-5f5abbcd4cda";
			//PublishMessages(topic, 50);
			var sql = PulsarSystem.NewSql();
			var option = new ClientOptions { Server = "http://127.0.0.1:8081", Execute = @$"select * from ""{topic}""", Catalog = "pulsar", Schema = "public/default" };
			var query = new SqlQuery(option, e => { Console.WriteLine(e.ToString()); }, Console.WriteLine);
			sql.SendQuery(query);
			Thread.Sleep(TimeSpan.FromSeconds(30));
			foreach (var m in sql.ReadQueryResults())
			{
				var data = m.Response;
				switch (data)
				{
					case DataResponse dr:
						_output.WriteLine(JsonSerializer.Serialize(dr, new JsonSerializerOptions { WriteIndented = true }));
						break;
					case StatsResponse sr:
						_output.WriteLine(JsonSerializer.Serialize(sr, new JsonSerializerOptions { WriteIndented = true }));
						break;
					case ErrorResponse er:
						_output.WriteLine(JsonSerializer.Serialize(er, new JsonSerializerOptions { WriteIndented = true }));
						break;
				}
			}
		}
		private ISet<string> PublishMessages(string topic, int count)
		{
			ISet<string> keys = new HashSet<string>();
			var builder = new ProducerConfigBuilder<DataOp>()
				.Topic(topic);
			var producer = _client.NewProducer(AvroSchema<DataOp>.Of(typeof(DataOp)), builder);
			for (int i = 0; i < count; i++)
			{
				string key = "key" + i;
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
