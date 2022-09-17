
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Admin
{
    public class BookiesTest
    {
        private readonly ITestOutputHelper _output;
        private readonly SharpPulsar.Admin.Bookies _bookies;
        private System.Text.Json.JsonSerializerOptions _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public BookiesTest(ITestOutputHelper output)
        {
            _output = output;
            _bookies = new SharpPulsar.Admin.Bookies("http://localhost:8080/", new HttpClient());
        }
        [Fact]
        public async Task GetBookies()
        {
            var bookie = await _bookies.GetBookiesAsync();
            _output.WriteLine(JsonSerializer.Serialize(bookie, _jsonSerializerOptions));
            Assert.True(bookie != null);
        }
        [Fact]
        public async Task GetBookiesRackInfo()
        {
            var bookie = await _bookies.GetBookiesRackInfoAsync();
            _output.WriteLine(JsonSerializer.Serialize(bookie, _jsonSerializerOptions));
            Assert.True(bookie != null);
        }
        [Fact]
        public async Task GetBookieRackInfo()
        {
            var bookie = await _bookies.GetBookieRackInfoAsync("127.0.0.1");
            _output.WriteLine(JsonSerializer.Serialize(bookie, _jsonSerializerOptions));
            Assert.True(bookie != null);
        }

    }
}
