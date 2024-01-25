using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.TestContainer;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http;
using SharpPulsar.Common.Naming;
using FluentAssertions;
using SharpPulsar.Test.Fixture;
using SharpPulsar.Admin.v2;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class TableViewTests : IAsyncLifetime
    {
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        public ClientConfigurationData _clientConfigurationData;
        private PulsarAdminRESTAPIClient _admin;

        public TableViewTests(ITestOutputHelper output, PulsarFixture fixture)
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8080/admin/v2/")
            };
            _admin = new PulsarAdminRESTAPIClient(http);
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
            _clientConfigurationData = fixture.ClientConfigurationData;
        }
        [Fact]
        public async Task TestTableView()
        {
            var topic = $"persistent://public/default/tableview-{DateTime.Now.Ticks}";
            var count = 20;
            var keys = await PublishMessages(topic, count, false);

            var tv = await _client.NewTableView(ISchema<string>.Bytes)
                .Topic(topic)
                .AutoUpdatePartitionsInterval(TimeSpan.FromSeconds(60))
                .CreateAsync();
            _output.WriteLine($"start tv size: {tv.Size()}");
            tv.ForEachAndListen((k, v) => _output.WriteLine($"{k} -> {Encoding.UTF8.GetString(v)}"));
            await Task.Delay(10000);
            _output.WriteLine($"Current tv size: {tv.Size()}");
            Assert.Equal(tv.Size(), count);
            tv.KeySet().Should().BeEquivalentTo(keys);
            tv.ForEachAndListen((k, v) => _output.WriteLine($"checkpoint {k} -> {Encoding.UTF8.GetString(v)}"));

            // Send more data
            var keys2 = await PublishMessages(topic, count, false);
            //await Task.Delay(3000);
            _output.WriteLine($"Current tv size: {tv.Size()}");
            //await Task.Delay(5000);
            Assert.Equal(tv.Size(), count);
            Assert.True(count >= tv.Size());
            tv.KeySet().Should().BeEquivalentTo(keys2);
        }

        [Fact]
        public async Task TestTableViewUpdatePartitions()
        {
            var topic = $"tableview-{Guid.NewGuid()}";
            try
            {
                await _admin.CreatePartitionedTopicAsync("public", "default", topic, new PartitionedTopicMetadata { Partitions = 3 }, false);
            }
            catch
            {

            }
            topic = $"persistent://public/default/{topic}-0";
            var count = 20;
            var keys = await PublishMessages(topic, count, false);

            var tv = await _client.NewTableView(ISchema<string>.Bytes).Topic(topic)
                .AutoUpdatePartitionsInterval(TimeSpan.FromSeconds(5)).CreateAsync();

            _output.WriteLine($"start tv size: {tv.Size}");

            tv.ForEachAndListen((k, v) => _output.WriteLine($"{k} -> {Encoding.UTF8.GetString(v)}"));
            await Task.Delay(10000);
            _output.WriteLine($"Current tv size: {tv.Size()}");
            Assert.Equal(tv.Size(), count);
            tv.KeySet().Should().BeEquivalentTo(keys);
            tv.ForEachAndListen((k, v) => _output.WriteLine($"checkpoint {k} -> {Encoding.UTF8.GetString(v)}"));

            try
            {
               await _admin.UpdatePartitionedTopic2Async("public", "default", topic, false, false, false, 4);
            }
            catch { }
            var topicName = TopicName.Get(topic);

            // Send more data to partition 3, which is not in the current TableView, need update partitions
            var keys2 = await PublishMessages(topicName.GetPartition(3).ToString(), count, false);
            //await Task.Delay(3000);
            _output.WriteLine($"Current tv size: {tv.Size()}");
            //await Task.Delay(6000);
            Assert.Equal(tv.Size(), count);
            Assert.True(count >= tv.Size());
            tv.KeySet().Should().BeEquivalentTo(keys2);
        }

        private async Task<ISet<string>> PublishMessages(string topic, int count, bool enableBatch)
        {
            var keys = new HashSet<string>();
            var builder = new ProducerConfigBuilder<byte[]>()
                .Topic(topic)
                .MaxPendingMessages(count)
                .BatchingMaxPublishDelay(TimeSpan.FromDays(1));
            if (enableBatch)
            {
                builder.EnableBatching(true);
                builder.BatchingMaxMessages(count);
            }
            else
            {
                builder.EnableBatching(false);
            }
            var producer = await _client.NewProducerAsync(builder);
            for (var i = 0; i < count; i++)
            {
                var key = "key" + i;
                var data = Encoding.UTF8.GetBytes("my-message-" + i);
                await producer.NewMessage().Key(key).Value(data).SendAsync();
                keys.Add(key);
            }
            producer.Flush();
            return keys;
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
}
