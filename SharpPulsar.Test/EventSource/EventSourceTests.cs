using SharpPulsar.Configuration;
using SharpPulsar.EventSource.Messages;
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

namespace SharpPulsar.Test.EventSource
{
    [Collection(nameof(PulsarTests))]
    public class EventSourceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
		public readonly PulsarSystem _pulsarSystem;
		public ClientConfigurationData _clientConfigurationData;

		public EventSourceTests(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
			_pulsarSystem = fixture.PulsarSystem;
			_clientConfigurationData = fixture.ClientConfigurationData;
        }
		[Fact(Skip = "Issue with sql-worker on github action")]
		//[Fact]
		public virtual void SqlSourceTest()
		{
			var topic = $"presto-topics-{Guid.NewGuid()}";
			PublishMessages(topic, 50);
			var cols = new HashSet<string> { "text", "EventTime" };
			var option = new ClientOptions { Server = "http://127.0.0.1:8081" };
			var source = _pulsarSystem.EventSource("public", "default", topic, 0, 50, "http://127.0.0.1:8080")
				.Sql(option, cols)
				.SourceMethod()
				.CurrentEvents();
			var receivedCount = 0;

			for (var i = 0; i < 45; i++)
			{
				var response = source.CurrentEvents();
				foreach (var data in response)
				{
					i++;
					receivedCount++;
					switch (data)
					{
						case EventEnvelope dr:
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
				if (receivedCount == 0)
					Thread.Sleep(TimeSpan.FromSeconds(10));
			}
			Assert.True(receivedCount > 0);
		}
		[Fact(Skip = "Issue with sql-worker on github action")]
		//[Fact]
		public virtual void SqlSourceTaggedTest()
		{
			var topic = $"presto-topics-{Guid.NewGuid()}";
			PublishMessages(topic, 50);
			var cols = new HashSet<string> { "text", "EventTime" };
			var option = new ClientOptions { Server = "http://127.0.0.1:8081" };
			var source = _pulsarSystem.EventSource("public", "default", topic, 0, 50, "http://127.0.0.1:8080")
				.Sql(option, cols)
				.SourceMethod()
				.CurrentTaggedEvents(new Messages.Consumer.Tag("twitter", "mestical"));
			var receivedCount = 0;

			for (var i = 0; i < 45; i++)
			{
				var response = source.CurrentEvents();
				foreach (var data in response)
				{
					i++;
					receivedCount++;
					switch (data)
					{
						case EventEnvelope dr:
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
				if (receivedCount == 0)
					Thread.Sleep(TimeSpan.FromSeconds(10));
			}
			Assert.True(receivedCount > 0);
		}
		[Fact]
		public virtual void ReaderSourceTest()
		{
			var topic = $"reader-topics-{Guid.NewGuid()}";
			PublishMessages(topic, 50);

			var conf = new ReaderConfigBuilder<DataOp>().Topic(topic);

			var reader = _pulsarSystem.EventSource("public", "default", topic, 0, 1000, "http://127.0.0.1:8080")
				.Reader(_clientConfigurationData, conf, AvroSchema<DataOp>.Of(typeof(DataOp)))
				.SourceMethod()
				.CurrentEvents();

			var receivedCount = 0;
			for (var i = 0; i < 50; i++)
			{
				var response = reader.CurrentEvents(); 
				foreach(var data in response)
                {
					i++;
					receivedCount++; 
					_output.WriteLine(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));

				}
				if (receivedCount == 0)
					Thread.Sleep(TimeSpan.FromSeconds(10));
			}
			Assert.True(receivedCount > 0);
		}
		[Fact]
		public virtual void ReaderSourceTaggedTest()
		{
			var topic = $"reader-topics-{Guid.NewGuid()}";
			PublishMessages(topic, 50);

			var conf = new ReaderConfigBuilder<DataOp>().Topic(topic);

			var reader = _pulsarSystem.EventSource("public", "default", topic, 0, 1000, "http://127.0.0.1:8080")
				.Reader(_clientConfigurationData, conf, AvroSchema<DataOp>.Of(typeof(DataOp)))
				.SourceMethod()
				.CurrentTaggedEvents(new Messages.Consumer.Tag("twitter", "mestical"));

			var receivedCount = 0;
			for (var i = 0; i < 50; i++)
			{
				var response = reader.CurrentEvents();
				foreach(var data in response)
                {
					i++;
					receivedCount++; 
					_output.WriteLine(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));

				}
				if (receivedCount == 0)
					Thread.Sleep(TimeSpan.FromSeconds(10));
			}
			Assert.True(receivedCount > 0);
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
				if(i % 2 == 0)
					producer.NewMessage().Key(key).Property("twitter", "mestical").Value(new DataOp { Text = "my-event-message-" + i, EventTime = DateTimeHelper.CurrentUnixTimeMillis() }).Send();
				else
					producer.NewMessage().Key(key).Value(new DataOp { Text = "my-event-message-" + i, EventTime = DateTimeHelper.CurrentUnixTimeMillis() }).Send();
				keys.Add(key);
			}
			return keys;
		}
	}
	public class DataOp
	{
		public string Text { get; set; }
		public long EventTime { get; set; }
	}
}
