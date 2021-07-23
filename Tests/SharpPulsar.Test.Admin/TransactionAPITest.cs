using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Admin
{
    [Collection("pulsar admin")]
    public class TransactionAPITest
    {
        private readonly ITestOutputHelper _output;
        private readonly User.Admin _admin;
        public TransactionAPITest(ITestOutputHelper output)
        {
            _output = output;
            _admin = new User.Admin("http://localhost:8080/", new HttpClient());
        }
        [Fact]
        public async Task Should_Get_Transaction_Metadata()
        {
            var metadata = await _admin.GetTransactionMetadataWithHttpMessagesAsync(new SharpPulsar.Transaction.TxnID(3, 1));
            Assert.True(true);
        }
        [Fact]
        public async Task Should_Get_Transaction_Coordinators()
        {
            var coords = await _admin.GetCoordinatorStatsWithHttpMessagesAsync();
            Assert.True(true);
        }
        [Fact]
        public async Task Should_Get_Transaction_Coordinator_by_Id()
        {
            var coord = await _admin.GetCoordinatorStatsByIdWithHttpMessagesAsync(0);
            Assert.True(true);
        }
    }
}
