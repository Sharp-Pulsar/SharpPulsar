﻿using SharpPulsar.Builder;
using SharpPulsar.Configuration;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using SharpPulsar.Trino;
using SharpPulsar.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class EventSourceTests : IAsyncLifetime
    {
        private readonly ITestOutputHelper _output;

        public ClientConfigurationData _clientConfigurationData;
        private PulsarClientConfigBuilder _configBuilder;
        private PulsarClient _client;
        private PulsarSystem _pulsarSystem;

        public EventSourceTests(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _pulsarSystem = fixture.System;
            _clientConfigurationData = fixture.ClientConfigurationData;
            _configBuilder = fixture.ConfigBuilder;
        }
        //[Fact(Skip = "Issue with sql-worker on github action")]
        [Fact]
        
        public virtual async Task SqlSourceTest()
        {
            var topic = $"trino-topics-{Guid.NewGuid()}";
            var ids = await PublishMessages(topic, 50);
            var start = MessageIdUtils.GetOffset(ids.First());
            var end = MessageIdUtils.GetOffset(ids.Last());
            var cols = new HashSet<string> { "text", "EventTime" };
            var option = new ClientOptions { Server = "http://127.0.0.1:8081" };
            var source = _pulsarSystem.EventSource(_client, "public", "default", topic, start, end, "http://127.0.0.1:8080")
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
                    await Task.Delay(TimeSpan.FromSeconds(5));
            }
            await Task.Delay(TimeSpan.FromSeconds(1));
            Assert.True(receivedCount > 0);
        }
        //[Fact(Skip = "Issue with sql-worker on github action")]
        [Fact]
        public virtual async Task SqlSourceTaggedTest()
        {
            var topic = $"trino-topics-{Guid.NewGuid()}";
            var ids = await PublishMessages(topic, 50);
            var start = MessageIdUtils.GetOffset(ids.First());
            var end = MessageIdUtils.GetOffset(ids.Last());
            var cols = new HashSet<string> { "text", "EventTime" };
            var option = new ClientOptions { Server = "http://127.0.0.1:8081" };
            var source = _pulsarSystem.EventSource(_client, "public", "default", topic, start, end, "http://127.0.0.1:8080")
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
                    await Task.Delay(TimeSpan.FromSeconds(5));
            }
            await Task.Delay(TimeSpan.FromSeconds(1));
            Assert.True(receivedCount > 0);
        }
        //[Fact(Skip = "skip for now")]
        [Fact]
        public virtual async Task ReaderSourceTest()
        {
            var topic = $"reader-topics-{Guid.NewGuid()}";
            var ids = await PublishMessages(topic, 50);
            var start = MessageIdUtils.GetOffset(ids.First());
            var end = MessageIdUtils.GetOffset(ids.Last());

            var conf = new ReaderConfigBuilder<DataOp>().Topic(topic);

            var reader = _pulsarSystem.EventSource(_client, "public", "default", topic, start, end, "http://127.0.0.1:8080")
                .Reader(_clientConfigurationData, conf, AvroSchema<DataOp>.Of(typeof(DataOp)))
                .SourceMethod()
                .CurrentEvents();

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
        [Fact]
        public virtual async Task ReaderSourceTaggedTest()
        {
            var topic = $"reader-topics-{Guid.NewGuid()}";
            var ids = await PublishMessages(topic, 50);
            var start = MessageIdUtils.GetOffset(ids.First());
            var end = MessageIdUtils.GetOffset(ids.Last());

            var conf = new ReaderConfigBuilder<DataOp>().Topic(topic);

            var reader = _pulsarSystem.EventSource(_client, "public", "default", topic, start, end, "http://127.0.0.1:8080")
                .Reader(_clientConfigurationData, conf, AvroSchema<DataOp>.Of(typeof(DataOp)))
                .SourceMethod()
                .CurrentTaggedEvents(new Messages.Consumer.Tag("twitter", "mestical"));

            //let leave some time to wire everything up
            await Task.Delay(TimeSpan.FromSeconds(10));
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
            var builder = new ProducerConfigBuilder<DataOpEx>()
                .Topic(topic);
            var producer = await _client.NewProducerAsync(AvroSchema<DataOpEx>.Of(typeof(DataOpEx)), builder);
            for (var i = 0; i < count; i++)
            {
                var key = "key" + i;
                MessageId id = null;
                if (i % 2 == 0)
                    id = await producer.NewMessage().Key(key).Property("twitter", "mestical").Value(new DataOpEx { Text = "my-event-message-" + i, EventTime = DateTimeHelper.CurrentUnixTimeMillis() }).SendAsync();
                else
                    id = await producer.NewMessage().Key(key).Value(new DataOpEx { Text = "my-event-message-" + i, EventTime = DateTimeHelper.CurrentUnixTimeMillis() }).SendAsync();
                ids.Add(id);
            }
            await producer.CloseAsync();//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030

            return ids;
        }
        public async Task InitializeAsync()
        {
            
            _client = await _pulsarSystem.NewClient(_configBuilder);
        }

        public async Task DisposeAsync()
        {
            await _client.ShutdownAsync();
        }
    }
    public class DataOpEx
    {
        public string Text { get; set; }
        public long EventTime { get; set; }
    }
}
