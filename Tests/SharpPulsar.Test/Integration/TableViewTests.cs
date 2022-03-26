using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using SharpPulsar.Admin.Admin.Models;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http;
using SharpPulsar.Common.Naming;
using FluentAssertions;

namespace SharpPulsar.Test.Integration
{
    [Collection(nameof(IntegrationCollection))]
    public class TableViewTests
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        public readonly PulsarSystem _pulsarSystem;
        public ClientConfigurationData _clientConfigurationData;
        private Admin.Public.Admin _admin;

        public TableViewTests(ITestOutputHelper output, PulsarFixture fixture)
        {
            _admin = new Admin.Public.Admin("http://localhost:8080/", new HttpClient());
            _output = output;
            _client = fixture.Client;
            _pulsarSystem = fixture.PulsarSystem;
            _clientConfigurationData = _pulsarSystem.ClientConfigurationData;
        }
        [Fact]  
        public async Task TestTableView()
        {
            var topic = $"persistent://public/default/tableview-{DateTime.Now.Ticks}";
            var count = 20;
            var keys = await PublishMessages(topic, count, false);

            var tv = await _client.NewTableViewBuilder(ISchema<string>.Bytes)
                .Topic(topic)
                .AutoUpdatePartitionsInterval(TimeSpan.FromSeconds(60))
                .CreateAsync();
            _output.WriteLine($"start tv size: {tv.Size()}");
            tv.ForEachAndListen((k, v) => _output.WriteLine($"{k} -> {Encoding.UTF8.GetString(v)}"));
            await Task.Delay(5000);
            _output.WriteLine($"Current tv size: {tv.Size()}");
            Assert.Equal(tv.Size(), count);
            tv.KeySet().Should().BeEquivalentTo(keys);
            tv.ForEachAndListen((k, v) => _output.WriteLine($"checkpoint {k} -> {Encoding.UTF8.GetString(v)}"));

            // Send more data
            var keys2 = await PublishMessages(topic, count * 2, false);
            await Task.Delay(5000);
            _output.WriteLine($"Current tv size: {tv.Size()}");
            Assert.Equal(tv.Size(), count * 2);
            tv.KeySet().Should().BeEquivalentTo(keys2);
        }

        [Fact]
        public async Task TestTableViewUpdatePartitions()
        {
            var topic = $"tableview-partitions-{DateTime.Now.Ticks}";
            try
            {
                var result = await _admin.CreatePartitionedTopicAsync("public", "default", topic, 3);
            }
            catch
            {
                
            }
            
            var count = 20;
            var keys = await PublishMessages(topic, count, false);
            
            var tv = await _client.NewTableViewBuilder(ISchema<string>.Bytes).Topic(topic)
                .AutoUpdatePartitionsInterval(TimeSpan.FromSeconds(5)).CreateAsync();

            _output.WriteLine($"start tv size: {tv.Size}");

            tv.ForEachAndListen((k, v) => _output.WriteLine($"{k} -> {Encoding.UTF8.GetString(v)}"));
            await Task.Delay(5000);
            _output.WriteLine($"Current tv size: {tv.Size()}");
            Assert.Equal(tv.Size(), count);
            tv.KeySet().Should().BeEquivalentTo(keys);
            tv.ForEachAndListen((k, v) => _output.WriteLine($"checkpoint {k} -> {Encoding.UTF8.GetString(v)}"));

            try
            {
                _admin.UpdatePartitionedTopic("public", "default", topic, 4);
            }
            catch { }   
            var topicName = TopicName.Get(topic);

            // Send more data to partition 3, which is not in the current TableView, need update partitions
            var keys2 = await PublishMessages(topicName.GetPartition(3).ToString(), count * 2, false);
            await Task.Delay(60000);
            _output.WriteLine($"Current tv size: {tv.Size()}");
            Assert.Equal(tv.Size(), count * 2);
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
    }
}
