using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Admin
{
    public class BrokerStatsTest
    {
        private readonly ITestOutputHelper _output;
        private readonly SharpPulsar.Admin.BrokerStats _brokerStats;
        private System.Text.Json.JsonSerializerOptions _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public BrokerStatsTest(ITestOutputHelper output)
        {
            _output = output;
            _brokerStats = new SharpPulsar.Admin.BrokerStats("http://localhost:8080/", new HttpClient());
        }
        [Fact]
        public async Task GetMBeans()
        {
            var brokerStats = await _brokerStats.GetMBeansAsync();
            _output.WriteLine(System.Text.Json.JsonSerializer.Serialize(brokerStats, _jsonSerializerOptions));
            Assert.True(brokerStats != null);
        }
        [Fact]
        public async Task GetAllocatorStats()
        {
            var brokerStats = await _brokerStats.GetAllocatorStatsAsync("");
            _output.WriteLine(System.Text.Json.JsonSerializer.Serialize(brokerStats, _jsonSerializerOptions));
            Assert.True(brokerStats != null);
        }
        [Fact]
        public async Task GetBrokerResourceAvailability()
        {
            var brokerStats = await _brokerStats.GetBrokerResourceAvailabilityAsync("public", "default");
            _output.WriteLine(System.Text.Json.JsonSerializer.Serialize(brokerStats, _jsonSerializerOptions));
            Assert.True(brokerStats != null);
        }

        [Fact]
        public async Task GetLoadReport()
        {
            var brokerStats = await _brokerStats.GetLoadReportAsync();
            _output.WriteLine(System.Text.Json.JsonSerializer.Serialize(brokerStats, _jsonSerializerOptions));
            Assert.True(brokerStats != null);
        }
        [Fact]
        public async Task GetMetrics()
        {
            var brokerStats = await _brokerStats.GetMetricsAsync();
            _output.WriteLine(System.Text.Json.JsonSerializer.Serialize(brokerStats, _jsonSerializerOptions));
            Assert.True(brokerStats != null);
        }
        [Fact]
        public async Task GetPendingBookieOpsStats()
        {
            var brokerStats = await _brokerStats.GetPendingBookieOpsStatsAsync();
            _output.WriteLine(System.Text.Json.JsonSerializer.Serialize(brokerStats, _jsonSerializerOptions));
            Assert.True(brokerStats != null);
        }

        [Fact]
        public async Task GetTopics()
        {
            var brokerStats = await _brokerStats.GetTopicsAsync();
            _output.WriteLine(System.Text.Json.JsonSerializer.Serialize(brokerStats, _jsonSerializerOptions));
            Assert.True(brokerStats != null);
        }
    }
}
