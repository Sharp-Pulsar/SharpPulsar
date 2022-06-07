using SharpPulsar.Builder;
using SharpPulsar.Configuration;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.Schemas;
using SharpPulsar.Sql.Client;
using SharpPulsar.Test.EventSource.Fixture;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using SharpPulsar.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.EventSource
{
    [Collection(nameof(EventSourceCollection))]
    public class EventSourceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
		public readonly PulsarSystem _pulsarSystem;
		public ClientConfigurationData _clientConfigurationData;

		public EventSourceTests(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
            _pulsarSystem = fixture.PulsarSystem;
			_clientConfigurationData = _pulsarSystem.ClientConfigurationData;
        }
		//[Fact(Skip = "Issue with sql-worker on github action")]
		[Fact]
		public virtual async Task SqlSourceTest()
		{
			var topic = $"presto-topics-{Guid.NewGuid()}";
			var ids = await PublishMessages(topic, 50);
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
					await Task.Delay(TimeSpan.FromSeconds(10));
			}
            await Task.Delay(TimeSpan.FromSeconds(5));
			Assert.True(receivedCount > 0);
		}
		//[Fact(Skip = "Issue with sql-worker on github action")]
		[Fact]
		public virtual async Task SqlSourceTaggedTest()
		{
			var topic = $"presto-topics-{Guid.NewGuid()}";
            var ids = await PublishMessages(topic, 50);
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
					await Task.Delay(TimeSpan.FromSeconds(10));
			}
            await Task.Delay(TimeSpan.FromSeconds(5));
            Assert.True(receivedCount > 0);
		}
		[Fact (Skip = "skip for now")]
		public virtual async Task ReaderSourceTest()
		{
			var topic = $"reader-topics-{Guid.NewGuid()}";
            var ids = await PublishMessages(topic, 50);
            var start = MessageIdUtils.GetOffset(ids.First());
            var end = MessageIdUtils.GetOffset(ids.Last());

            var conf = new ReaderConfigBuilder<DataOp>().Topic(topic);

			var reader = _pulsarSystem.EventSource("public", "default", topic, start, end, "http://127.0.0.1:8080")
				.Reader(_clientConfigurationData, conf, AvroSchema<DataOp>.Of(typeof(DataOp)))
				.SourceMethod()
				.CurrentEvents();

            //let leave some time to wire everything up
            await Task.Delay(TimeSpan.FromSeconds(5));
            var receivedCount = 0;
            await foreach (var response in reader.CurrentEvents(TimeSpan.FromSeconds(40)))
            {
                receivedCount++;
                _output.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
            }
            Assert.True(receivedCount > 0);
		}
        [Fact(Skip = "skip for now")]
        public virtual async Task ReaderSourceTaggedTest()
		{
			var topic = $"reader-topics-{Guid.NewGuid()}";
            var ids = await PublishMessages(topic, 50);
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
            await foreach (var response in reader.CurrentEvents(TimeSpan.FromSeconds(40)))
            {
                receivedCount++;
                _output.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
            }

            Assert.True(receivedCount > 0);
		}
		private async Task<ISet<MessageId>> PublishMessages(string topic, int count)
		{
			var ids = new HashSet<MessageId>();
			var builder = new ProducerConfigBuilder<DataOp>()
				.Topic(topic);
			var producer = await _client.NewProducerAsync(AvroSchema<DataOp>.Of(typeof(DataOp)), builder);
			for (var i = 0; i < count; i++)
			{
				var key = "key" + i;
                MessageId id = null;
				if(i % 2 == 0)
					id = await producer.NewMessage().Key(key).Property("twitter", "mestical").Value(new DataOp { Text = "my-event-message-" + i, EventTime = DateTimeHelper.CurrentUnixTimeMillis() }).SendAsync();
				else
					id = await producer.NewMessage().Key(key).Value(new DataOp { Text = "my-event-message-" + i, EventTime = DateTimeHelper.CurrentUnixTimeMillis() }).SendAsync();
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
