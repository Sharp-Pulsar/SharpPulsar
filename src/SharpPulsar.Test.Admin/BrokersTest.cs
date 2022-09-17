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
        public async Task BacklogQuotaCheck()
        {
            try
            {

                await _brokers.BacklogQuotaCheckAsync();
                Assert.True(true);
            }
            catch(Exception ex)
            {
                _output.WriteLine(JsonSerializer.Serialize(ex.ToString(), _jsonSerializerOptions));
            }
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
