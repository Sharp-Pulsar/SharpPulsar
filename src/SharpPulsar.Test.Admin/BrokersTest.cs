using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Admin
{
    public class BrokersTest
    {
        private readonly ITestOutputHelper _output;
        private readonly SharpPulsar.Admin.Brokers _brokers;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public BrokersTest(ITestOutputHelper output)
        {
            _output = output;
            _brokers = new SharpPulsar.Admin.Brokers("http://localhost:8080/", new HttpClient());
        }
        [Fact]
        public async Task GetActiveBrokers()
        {
            var broker = await _brokers.GetActiveBrokersAsync("");
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }
        [Fact]
        public async Task GetAllDynamicConfigurations()
        {
            var broker = await _brokers.GetAllDynamicConfigurationsAsync();
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }

        [Fact]
        public async Task GetDynamicConfigurationNames()
        {
            var broker = await _brokers.GetDynamicConfigurationNamesAsync();
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }

        [Fact]
        public async Task GetInternalConfigurationData()
        {
            var broker = await _brokers.GetInternalConfigurationDataAsync();
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }

        [Fact]
        public async Task GetLeaderBroker()
        {
            var broker = await _brokers.GetLeaderBrokerAsync();
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }

        [Fact]
        public async Task BacklogQuotaCheck()
        {
            var broker = await _brokers.BacklogQuotaCheckAsync();
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }
        [Fact]
        public async Task DeleteDynamicConfiguration()
        {
            var broker = await _brokers.DeleteDynamicConfigurationAsync("");
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }
        [Fact]
        public async Task Healthcheck()
        {
            var broker = await _brokers.HealthcheckAsync(TopicVersion.V2);
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }
        [Fact]
        public async Task IsReady()
        {
            var broker = await _brokers.IsReadyAsync();
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }
        [Fact]
        public async Task ShutDownBrokerGracefully()
        {
            var broker = await _brokers.ShutDownBrokerGracefullyAsync(1);
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }
        [Fact]
        public async Task UpdateDynamicConfiguration()
        {
            var broker = await _brokers.UpdateDynamicConfigurationAsync("", "");
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }
        [Fact]
        public async Task GetOwnedNamespaces()
        {
            var broker = await _brokers.GetOwnedNamespacesAsync("", "");
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }
        [Fact]
        public async Task GetRuntimeConfigurations()
        {
            var broker = await _brokers.GetRuntimeConfigurationsAsync();
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }

        [Fact]
        public async Task Version()
        {
            var broker = await _brokers.VersionAsync();
            _output.WriteLine(JsonSerializer.Serialize(broker, _jsonSerializerOptions));
            Assert.True(broker != null);
        }
    }
}
