using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using DotNet.Testcontainers.Builders;
using SharpPulsar.Builder;
using Xunit;

namespace SharpPulsar.TestContainer
{
    public class PulsarFixture : IAsyncLifetime, IDisposable
    {
        
        public virtual PulsarTestcontainerConfiguration Configuration { get; }

        public PulsarFixture()
        {
            Configuration = new PulsarTestcontainerConfiguration("apachepulsar/pulsar-all:2.10.0", 6650);
            Container = BuildContainer()
                .WithCleanUp(true)
                .Build();
        }
        public virtual TestcontainersBuilder<PulsarTestcontainer> BuildContainer()
        {
            return (TestcontainersBuilder<PulsarTestcontainer>)new TestcontainersBuilder<PulsarTestcontainer>()
              .WithName("integration-tests")
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
            await AwaitPortReadiness($"http://127.0.0.1:8080/metrics/");
            await Container.ExecAsync(new List<string> { @"./bin/pulsar", "sql-worker", "start" });

            await AwaitPortReadiness($"http://127.0.0.1:8081/");
            
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
            
            try
            {
                await Container.DisposeAsync().AsTask();
            }
            catch
            {

            }
        }

        public void Dispose()
        {
            Configuration.Dispose();
        }
    }
}
