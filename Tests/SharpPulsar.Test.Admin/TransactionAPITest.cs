using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Admin
{
    [Collection("pulsar admin")]
    public class TransactionAPITest
    {
        private readonly ITestOutputHelper _output;
        private readonly SharpPulsar.Admin.Public.Admin _admin;
        public TransactionAPITest(ITestOutputHelper output)
        {
            _output = output;
            _admin = new SharpPulsar.Admin.Public.Admin("http://localhost:8080/", new HttpClient());
        }
        [Fact]
        public async Task Should_Get_Transaction_Metadata()
        {
            var metadata = await _admin.GetTransactionMetadataAsync(new TxnID(3, 1));
            _output.WriteLine(JsonSerializer.Serialize(metadata.Body));
            Assert.True(metadata.Body != null);
        }
        [Fact]
        public async Task Should_Get_Transaction_Coordinators()
        {
            var coords = await _admin.GetCoordinatorStatsAsync();
            _output.WriteLine(JsonSerializer.Serialize(coords.Body));
            //Assert.True(coords.Body != null);
        }
        [Fact]
        public async Task Should_Get_Transaction_Coordinator_by_Id()
        {
            var coord = await _admin.GetCoordinatorStatsByIdAsync(0);
            _output.WriteLine(JsonSerializer.Serialize(coord.Body));
            Assert.True(coord.Body != null);
        }
    }
}
