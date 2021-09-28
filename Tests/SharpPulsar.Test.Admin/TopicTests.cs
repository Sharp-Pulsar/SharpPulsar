using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Admin
{
    [Collection("pulsar admin")]
    public class TopicTests
    {
        private readonly ITestOutputHelper _output;
        private readonly SharpPulsar.Admin.Public.Admin _admin;
        public TopicTests(ITestOutputHelper output)
        {
            _output = output;
            _admin = new SharpPulsar.Admin.Public.Admin("http://localhost:8080/", new HttpClient());
        }
        [Fact]
        public async Task GetAllTopics()
        {
            var metadata = await _admin.GetTopicsAsync();
            Assert.True(metadata.Body != null);
        }
    }
}
