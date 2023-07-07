using System.Text.Json;
using SharpPulsar.Admin.v3.Transactions;
using Xunit.Abstractions;

namespace SharpPulsar.Admin.Test
{
    [Collection("pulsar admin")]
    public class TransactionSpec
    {
        private readonly ITestOutputHelper _output;
        private PulsarTransactionsRESTAPIClient _admin;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public TransactionSpec(ITestOutputHelper output)
        {
            _output = output;
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8080/admin/v3/")
            };
            _admin = new PulsarTransactionsRESTAPIClient(client);
        }
    }
}
