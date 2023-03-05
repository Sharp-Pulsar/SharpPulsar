using System.Text.Json;
using SharpPulsar.Admin.v3.Transactions;
using Xunit.Abstractions;

namespace SharpPulsar.Admin.Test
{
    [Collection("pulsar admin")]
    public class Transaction
    {
        private readonly ITestOutputHelper _output;
        private PulsarTransactionsRESTAPIClient _admin;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public Transaction(ITestOutputHelper output)
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
