using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace SharpPulsar.TestContainer
{
    public class PulsarFixture : IAsyncLifetime, IDisposable
    {
        private readonly IConfiguration _configuration; 
        public virtual PulsarTestcontainerConfiguration Configuration { get; }

        public PulsarFixture()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _configuration = GetIConfigurationRoot(path);
            Configuration = new PulsarTestcontainerConfiguration("apachepulsar/pulsar-all:2.9.1", 6650);
            Container = BuildContainer()
                .WithCleanUp(true)
                .Build();
        }
        public virtual TestcontainersBuilder<PulsarTestcontainer> BuildContainer()
        {
            return (TestcontainersBuilder<PulsarTestcontainer>)new TestcontainersBuilder<PulsarTestcontainer>()
              .WithName("sql-integration-tests")
              .WithPulsar(Configuration)
              .WithPortBinding(6650, 6650)
              .WithPortBinding(8080, 8080)
              .WithPortBinding(8081, 8081)
              .WithExposedPort(6650)
              .WithExposedPort(8080)
              .WithExposedPort(8081);
        }
        public PulsarTestcontainer Container { get; }
        public virtual async Task InitializeAsync()
        {
            await Container.StartAsync();//;.GetAwaiter().GetResult();
            AwaitPortReadiness($"http://127.0.0.1:8080/metrics/").GetAwaiter().GetResult();
            Container.ExecAsync(new List<string> { @"./bin/pulsar", "sql-worker", "start" }).GetAwaiter().GetResult();

            AwaitPortReadiness($"http://127.0.0.1:8081/").GetAwaiter().GetResult();
            await Task.CompletedTask;  
        }
        public async ValueTask AwaitPortReadiness(string address)
        {
            var waitTries = 20;

            using var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };

            using var client = new HttpClient(handler);

            while (waitTries > 0)
            {
                try
                {
                    await client.GetAsync(address).ConfigureAwait(false);
                    return;
                }
                catch
                {
                    waitTries--;
                    await Task.Delay(5000).ConfigureAwait(false);
                }
            }

            throw new Exception("Unable to confirm Pulsar has initialized");
        }
        public virtual async Task DisposeAsync()
        {
            await Container.DisposeAsync().AsTask();
        }

        public IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
        public void Dispose()
        {
            Configuration.Dispose();
        }
    }
}
