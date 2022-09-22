using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Admin;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Admin
{
    public class NonPersistentTopicsTest
    {
        private readonly ITestOutputHelper _output;
        private readonly NonPersistentTopics _nonPersistentTopics;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public NonPersistentTopicsTest(ITestOutputHelper output)
        {
            _output = output;
            _nonPersistentTopics = new NonPersistentTopics("http://localhost:8080/", new HttpClient());
        }
        [Fact]
        public async Task CreatePartitionedTopic()
        {
            var nonPersistentTopicse = await _nonPersistentTopics.CreatePartitionedTopicAsync("public", "default", "topic", 1);
            _output.WriteLine(JsonSerializer.Serialize(nonPersistentTopicse, _jsonSerializerOptions));
            Assert.True(nonPersistentTopicse != null);
        }
        [Fact]
        public async Task GetEntryFilters()
        {
            var nonPersistentTopicse = await _nonPersistentTopics.GetEntryFiltersAsync("public", "default", "topic");
            _output.WriteLine(JsonSerializer.Serialize(nonPersistentTopicse, _jsonSerializerOptions));
            Assert.True(nonPersistentTopicse != null);
        }
        [Fact]
        public async Task GetInternalStats()
        {
            var nonPersistentTopicse = await _nonPersistentTopics.GetInternalStatsAsync("public", "default", "topic");
            _output.WriteLine(JsonSerializer.Serialize(nonPersistentTopicse, _jsonSerializerOptions));
            Assert.True(nonPersistentTopicse != null);
        }
        [Fact]
        public async Task GetList()
        {
            var nonPersistentTopicse = await _nonPersistentTopics.GetListAsync("public", "default", "topic", true);
            _output.WriteLine(JsonSerializer.Serialize(nonPersistentTopicse, _jsonSerializerOptions));
            Assert.True(nonPersistentTopicse != null);
        }
        [Fact]
        public async Task GetListFromBundle()
        {
            var nonPersistentTopicse = await _nonPersistentTopics.GetListFromBundleAsync("public", "default", "topic");
            _output.WriteLine(JsonSerializer.Serialize(nonPersistentTopicse, _jsonSerializerOptions));
            Assert.True(nonPersistentTopicse != null);
        }
        [Fact]
        public async Task GetPartitionedMetadata()
        {
            var nonPersistentTopicse = await _nonPersistentTopics.GetPartitionedMetadataAsync("public", "default", "topic");
            _output.WriteLine(JsonSerializer.Serialize(nonPersistentTopicse, _jsonSerializerOptions));
            Assert.True(nonPersistentTopicse != null);
        }
        [Fact]
        public async Task GetPartitionedStats()
        {
            var nonPersistentTopicse = await _nonPersistentTopics.GetPartitionedStatsAsync("public", "default", "topic");
            _output.WriteLine(JsonSerializer.Serialize(nonPersistentTopicse, _jsonSerializerOptions));
            Assert.True(nonPersistentTopicse != null);
        }
        [Fact]
        public async Task RemoveEntryFilters()
        {
            var nonPersistentTopicse = await _nonPersistentTopics.RemoveEntryFiltersAsync("public", "default", "topic");
            _output.WriteLine(JsonSerializer.Serialize(nonPersistentTopicse, _jsonSerializerOptions));
            Assert.True(nonPersistentTopicse != null);
        }
        [Fact]
        public async Task SetEntryFilters()
        {
            var nonPersistentTopicse = await _nonPersistentTopics.SetEntryFiltersAsync("public", "default", "topic");
            _output.WriteLine(JsonSerializer.Serialize(nonPersistentTopicse, _jsonSerializerOptions));
            Assert.True(nonPersistentTopicse != null);
        }
        [Fact]
        public async Task TruncateTopic()
        {
            var nonPersistentTopicse = await _nonPersistentTopics.TruncateTopicAsync("public", "default", "topic");
            _output.WriteLine(JsonSerializer.Serialize(nonPersistentTopicse, _jsonSerializerOptions));
            Assert.True(nonPersistentTopicse != null);
        }
        [Fact]
        public async Task UnloadTopic()
        {
            var nonPersistentTopicse = await _nonPersistentTopics.UnloadTopicAsync("public", "default", "topic");
            _output.WriteLine(JsonSerializer.Serialize(nonPersistentTopicse, _jsonSerializerOptions));
            Assert.True(nonPersistentTopicse != null);
        }
    }
}
