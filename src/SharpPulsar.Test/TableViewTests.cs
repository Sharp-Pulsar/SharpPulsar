using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Configuration;
using SharpPulsar.TestContainer;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http;
using SharpPulsar.Common.Naming;
using FluentAssertions;
using SharpPulsar.Test.Fixture;
using SharpPulsar.Admin.v2;
using SharpPulsar.Table;

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
        public void TestLoadConfigFromMap()
        {
            IDictionary<string, object> configMap = new Dictionary<string, object>();
            configMap["TopicName"] = "persistent://public/default/test";
            configMap["SubscriptionName"] = "test-sub";
            configMap["AutoUpdatePartitionsSeconds"] = TimeSpan.FromMilliseconds(60);
            var config = new TableViewConfigurationData();
            config = (TableViewConfigurationData)ConfigurationDataUtils.LoadData(configMap, config);

            Assert.Equal(configMap["TopicName"], config.TopicName);
            Assert.Equal(configMap["SubscriptionName"], config.SubscriptionName);
            Assert.Equal(configMap["AutoUpdatePartitionsSeconds"], config.AutoUpdatePartitionsSeconds);
        }

        /// <summary>
        /// Case1:
        /// 1. Slow down the rate of reading messages.
        /// 2. Send some messages
        /// 3. Call new `refresh` API, it will wait for reading all the messages completed.
        /// Case2:
        /// 1. No new messages.
        /// 2. Call new `refresh` API, it will be completed immediately.
        /// Case3:
        /// 1. multi-partition topic, p1, p2 has new message, p3 has no new messages.
        /// 2. Call new `refresh` API, it will be completed after read new messages.
        /// </summary>
        /// 
        [Theory]
        [InlineData(2)]
        //[InlineData(5)]
        //[InlineData(10)]
        public async Task TestRefreshAPI(int partition)
        {
            // 1. Prepare resource.
            var topic = $"testRefreshAPI-{Guid.NewGuid}";
            try
            {
                if (partition == 0)
                {
                    await _admin.CreateNonPartitionedTopicAsync("public", "default", topic, false, new Dictionary<string, string>());
                }
                else
                {
                    await _admin.CreatePartitionedTopic2Async("public", "default", topic, new PartitionedTopicMetadata { Partitions = partition }, false);
                }
            }
            catch { }    
            
            topic = $"{topic}-partition-0";
            var tv = await _client.NewTableView(ISchema<string>.Bytes)
                .Topic(topic)
                .CreateAsync();

            // 2. Add a listen action to provide the test environment.
            // The listen action will be triggered when there are incoming messages every time.
            // This is a sync operation, so sleep in the listen action can slow down the reading rate of messages.
            tv.Listen(async (k, v) =>
            {
                try
                {
                   await Task.Delay(10000);
                }
                catch (Exception e)
                {
                    throw;
                }
            });
            // 3. Send 20 messages. After refresh, all the messages should be received.
            var count = 20;
            var keys = await PublishMessages(topic, count, false);
            // After message sending completely, the table view will take at least 2 seconds to receive all the messages.
            // If there is not the refresh operation, all messages will not be received.
            tv.RefreshAsync();
            await Task.Delay(10000);
            _output.WriteLine($"start tv size: {tv.Size()}");
            // The key of each message is different.
            Assert.Equal(tv.Size(), count);
            _output.WriteLine($"Assert.Equal tv size: {tv.Size()}");
            Assert.Equal(tv.KeySet(), keys);
            _output.WriteLine($"Assert.Equal tv KeySet: {tv.KeySet()}");
            // 4. Test refresh operation can be completed when there is a partition with on new messages
            // or no new message for no partition topic.
            if (partition > 0)
            {
                await PublishMessages(topic, partition - 1, count, false);
                tv.RefreshAsync();
                await Task.Delay(TimeSpan.FromSeconds(5));
                Assert.Equal(tv.Size(), count + partition - 1);
                _output.WriteLine($"Assert.Equal tv Size: {tv.Size()}:::::{count + partition - 1}");
            }
            else
            {
                tv.RefreshAsync();
                await Task.Delay(TimeSpan.FromSeconds(5));
                _output.WriteLine($"Assert.Equal tv Size: {tv.Size()}");
            }

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
        private async ValueTask<ISet<string>> PublishMessages(string topic, int count, bool enableBatch)
        {
            return await PublishMessages(topic, 0, count, enableBatch);
        }

        private async ValueTask<ISet<string>> PublishMessages(string topic, int keyStartPosition, int count, bool enableBatch)
        {
            var keys = new HashSet<string>();
            var builder = new ProducerConfigBuilder<byte[]>()
                .Topic(topic)
                .MessageRoutingMode(Common.MessageRoutingMode.RandomMode)
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
            for (var i = keyStartPosition; i < keyStartPosition + count; i++)
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
