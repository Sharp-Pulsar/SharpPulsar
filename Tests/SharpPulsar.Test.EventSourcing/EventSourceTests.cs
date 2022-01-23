using SharpPulsar.Configuration;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.Schemas;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;
using SharpPulsar.Test.EventSourcing.Fixtures;
using SharpPulsar.User;
using SharpPulsar.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.EventSourcing
{
    [Collection(nameof(PulsarEventsTests))]
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
		//[Fact(Skip = "Issue with sql-worker on github action")]
		[Fact]
		public virtual void SqlSourceTest()
		{
			var topic = $"presto-topics-{Guid.NewGuid()}";
			var ids = PublishMessages(topic, 50);
            var start = MessageIdUtils.GetOffset(ids.First());
            var end = MessageIdUtils.GetOffset(ids.Last());
			var cols = new HashSet<string> { "text", "EventTime" };
			var option = new ClientOptions { Server = "http://127.0.0.1:8081" };
			var source = _pulsarSystem.EventSource("public", "default", topic, start, end,"http://127.0.0.1:8080")
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
						case EventStats sr:
							_output.WriteLine(JsonSerializer.Serialize(sr.Stats, new JsonSerializerOptions { WriteIndented = true }));
							break;
						case EventError er:
							_output.WriteLine(JsonSerializer.Serialize(er.Error, new JsonSerializerOptions { WriteIndented = true }));
							break;
					}

				}
				if (receivedCount == 0)
					Thread.Sleep(TimeSpan.FromSeconds(10));
			}
			Assert.True(receivedCount > 0);
		}
		//[Fact(Skip = "Issue with sql-worker on github action")]
		[Fact]
		public virtual void SqlSourceTaggedTest()
		{
			var topic = $"presto-topics-{Guid.NewGuid()}";
            var ids = PublishMessages(topic, 50);
            var start = MessageIdUtils.GetOffset(ids.First());
            var end = MessageIdUtils.GetOffset(ids.Last());
            var cols = new HashSet<string> { "text", "EventTime" };
			var option = new ClientOptions { Server = "http://127.0.0.1:8081" };
			var source = _pulsarSystem.EventSource("public", "default", topic, start, end, "http://127.0.0.1:8080")
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
                        case EventStats sr:
                            _output.WriteLine(JsonSerializer.Serialize(sr.Stats, new JsonSerializerOptions { WriteIndented = true }));
                            break;
                        case EventError er:
                            _output.WriteLine(JsonSerializer.Serialize(er.Error, new JsonSerializerOptions { WriteIndented = true }));
                            break;
                    }

				}
				if (receivedCount == 0)
					Thread.Sleep(TimeSpan.FromSeconds(10));
			}
			Assert.True(receivedCount > 0);
		}
		[Fact]
		public virtual async Task ReaderSourceTest()
		{
			var topic = $"reader-topics-{Guid.NewGuid()}";
            var ids = PublishMessages(topic, 50);
            var start = MessageIdUtils.GetOffset(ids.First());
            var end = MessageIdUtils.GetOffset(ids.Last());

            var conf = new ReaderConfigBuilder<DataOp>().Topic(topic);

			var reader = _pulsarSystem.EventSource("public", "default", topic, start, end, "http://127.0.0.1:8080")
				.Reader(_clientConfigurationData, conf, AvroSchema<DataOp>.Of(typeof(DataOp)))
				.SourceMethod()
				.CurrentEvents();

            //let leave some time to wire everything up
            await Task.Delay(TimeSpan.FromSeconds(20));
            var receivedCount = 0;
            while(receivedCount == 0)
            {
                await foreach (var response in reader.CurrentEvents(TimeSpan.FromSeconds(5)))
                {
                    receivedCount++;
                    _output.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            Assert.True(receivedCount > 0);
		}
		[Fact]
		public virtual async Task ReaderSourceTaggedTest()
		{
			var topic = $"reader-topics-{Guid.NewGuid()}";
            var ids = PublishMessages(topic, 50);
            var start = MessageIdUtils.GetOffset(ids.First());
            var end = MessageIdUtils.GetOffset(ids.Last());

            var conf = new ReaderConfigBuilder<DataOp>().Topic(topic);

			var reader = _pulsarSystem.EventSource("public", "default", topic, start, end, "http://127.0.0.1:8080")
				.Reader(_clientConfigurationData, conf, AvroSchema<DataOp>.Of(typeof(DataOp)))
				.SourceMethod()
				.CurrentTaggedEvents(new Messages.Consumer.Tag("twitter", "mestical"));

            //let leave some time to wire everything up
            await Task.Delay(TimeSpan.FromSeconds(20));
            var receivedCount = 0;
            while (receivedCount == 0)
            {
                await foreach (var response in reader.CurrentEvents(TimeSpan.FromSeconds(5)))
                {
                    receivedCount++;
                    _output.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));                    
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            
			Assert.True(receivedCount > 0);
		}
		private ISet<MessageId> PublishMessages(string topic, int count)
		{
			var ids = new HashSet<MessageId>();
			var builder = new ProducerConfigBuilder<DataOp>()
				.Topic(topic);
			var producer = _client.NewProducer(AvroSchema<DataOp>.Of(typeof(DataOp)), builder);
			for (var i = 0; i < count; i++)
			{
				var key = "key" + i;
                MessageId id = null;
				if(i % 2 == 0)
					id = producer.NewMessage().Key(key).Property("twitter", "mestical").Value(new DataOp { Text = "my-event-message-" + i, EventTime = DateTimeHelper.CurrentUnixTimeMillis() }).Send();
				else
					id = producer.NewMessage().Key(key).Value(new DataOp { Text = "my-event-message-" + i, EventTime = DateTimeHelper.CurrentUnixTimeMillis() }).Send();
				ids.Add(id);
			}
			return ids;
		}
	}
	public class DataOp
	{
		public string Text { get; set; }
		public long EventTime { get; set; }
	}
}
