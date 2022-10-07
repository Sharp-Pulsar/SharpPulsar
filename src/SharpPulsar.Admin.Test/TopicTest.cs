
using System.Text.Json;
using SharpPulsar.Admin.v2;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SharpPulsar.Admin.Test
{
    [Collection("pulsar admin")]
    public class TopicTest
    {
        private readonly ITestOutputHelper _output;
        private PulsarAdminRESTAPIClient _admin;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public TopicTest(ITestOutputHelper output)
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
            Assert.True(topic != null);
        }
    }
}
