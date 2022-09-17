using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Admin
{
    public class ClustersTest
    {
        private readonly ITestOutputHelper _output;
        private readonly SharpPulsar.Admin.Clusters _clusters;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public ClustersTest(ITestOutputHelper output)
        {
            _output = output;
            _clusters = new SharpPulsar.Admin.Clusters("http://localhost:8080/", new HttpClient());
        }
        [Fact]
        public async Task CreateCluster()
        {
            var cluster = await _clusters.CreateClusterAsync("test", null);
            _output.WriteLine(JsonSerializer.Serialize(cluster, _jsonSerializerOptions));
            Assert.True(cluster != null);
        }
        [Fact]
        public async Task DeleteCluster()
        {
            var cluster = await _clusters.DeleteClusterAsync("test");
            _output.WriteLine(JsonSerializer.Serialize(cluster, _jsonSerializerOptions));
            Assert.True(cluster != null);
        }
        [Fact]
        public async Task DeleteFailureDomain()
        {
            var cluster = await _clusters.DeleteFailureDomainAsync("test", "127.0.0.1");
            _output.WriteLine(JsonSerializer.Serialize(cluster, _jsonSerializerOptions));
            Assert.True(cluster != null);
        }

        [Fact]
        public async Task DeleteNamespaceIsolationPolicy()
        {
            var cluster = await _clusters.DeleteNamespaceIsolationPolicyAsync("test", "127.0.0.1");
            _output.WriteLine(JsonSerializer.Serialize(cluster, _jsonSerializerOptions));
            Assert.True(cluster != null);
        }

        [Fact]
        public async Task GetBrokersWithNamespaceIsolationPolicy()
        {
            var cluster = await _clusters.GetBrokersWithNamespaceIsolationPolicyAsync("test");
            _output.WriteLine(JsonSerializer.Serialize(cluster, _jsonSerializerOptions));
            Assert.True(cluster != null);
        }
        [Fact]
        public async Task GetBrokerWithNamespaceIsolationPolicy()
        {
            var cluster = await _clusters.GetBrokerWithNamespaceIsolationPolicyAsync("test","bk" );
            _output.WriteLine(JsonSerializer.Serialize(cluster, _jsonSerializerOptions));
            Assert.True(cluster != null);
        }

        [Fact]
        public async Task GetCluster()
        {
            var cluster = await _clusters.GetClusterAsync("test");
            _output.WriteLine(JsonSerializer.Serialize(cluster, _jsonSerializerOptions));
            Assert.True(cluster != null);
        }


        [Fact]
        public async Task GetClusters()
        {
            var cluster = await _clusters.GetClustersAsync();
            _output.WriteLine(JsonSerializer.Serialize(cluster, _jsonSerializerOptions));
            Assert.True(cluster != null);
        }
        [Fact]
        public async Task GetDomain()
        {
            var cluster = await _clusters.GetDomainAsync("test", "bk");
            _output.WriteLine(JsonSerializer.Serialize(cluster, _jsonSerializerOptions));
            Assert.True(cluster != null);
        }
        [Fact]
        public async Task GetFailureDomains()
        {
            var cluster = await _clusters.GetFailureDomainsAsync("test");
            _output.WriteLine(JsonSerializer.Serialize(cluster, _jsonSerializerOptions));
            Assert.True(cluster != null);
        }

    }
}
