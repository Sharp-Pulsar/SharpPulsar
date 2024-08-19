using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Admin.v2;
using SharpPulsar.Test.Fixture;
using Xunit;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class ZO_AdminTest 
    {
        private readonly ITestOutputHelper _output;
        private PulsarAdminRESTAPIClient _admin;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public ZO_AdminTest(ITestOutputHelper output)
        {
            _output = output;
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8080/admin/v2/")
            };
            _admin = new PulsarAdminRESTAPIClient(client);
        }
        [Fact]
        public async Task GetAllTopics()
        {
            var topic = await _admin.GetTopicsAsync("public", "default", Mode.PERSISTENT, true);
            _output.WriteLine(JsonSerializer.Serialize(topic, _jsonSerializerOptions));
            Assert.True(true);
        }

        [Fact]
        public async Task GetOffloadThreshold()
        {
            var topic = await _admin.GetOffloadThresholdAsync("public", "default"); //10MB 10000000l
            _output.WriteLine(JsonSerializer.Serialize(topic, _jsonSerializerOptions));
            Assert.True(true);
        }

        [Fact]
        public async Task GetPropertiesAsync()
        {
            var topic = await _admin.GetPropertiesAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(topic, _jsonSerializerOptions));
            Assert.True(true);
        }
    }
}
