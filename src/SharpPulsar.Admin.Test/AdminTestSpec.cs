
using System.Text.Json;
using SharpPulsar.Admin.v2;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SharpPulsar.Admin.Test
{
    [Collection("pulsar admin")]
    public class AdminTestSpec
    {
        private readonly ITestOutputHelper _output;
        private PulsarAdminRESTAPIClient _admin;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public AdminTestSpec(ITestOutputHelper output)
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
        /*
        [Fact]
        public async Task GetStatsPersisten()
        {
            var topic = await _admin.GetStats2Async("public", "default", "query_topics_avro", false, false, false,false);
            _output.WriteLine(JsonSerializer.Serialize(topic, _jsonSerializerOptions));
            Assert.True(true);
        }
        [Fact]
        public async Task GetPersistenceAsync()
        {
            var topic = await _admin.GetPersistenceAsync("public", "default"); 
            _output.WriteLine(JsonSerializer.Serialize(topic, _jsonSerializerOptions));
            Assert.True(true);
        }
        [Fact]
        public async Task GetReplicatorDispatchRateAsync()
        {
            var topic = await _admin.GetReplicatorDispatchRateAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(topic, _jsonSerializerOptions));
            Assert.True(true);
        }*/
    }
}
