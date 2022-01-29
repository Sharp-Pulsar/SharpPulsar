using DotNet.Testcontainers.Builders;
using Xunit;

namespace SharpPulsar.TestContainer
{
    public class PulsarFixture : IAsyncLifetime, IDisposable
    {
        private readonly PulsarTestcontainerConfiguration _configuration = new PulsarTestcontainerConfiguration();

        public PulsarFixture()
        {
            Container = new TestcontainersBuilder<PulsarTestcontainer>()
              .WithPulsar(_configuration)
              .WithName($"test-core")              
              .WithPortBinding(6650)
              .WithPortBinding(8080)
              .WithPortBinding(8081)
              .WithExposedPort(6650)
              .WithExposedPort(8080)
              .WithExposedPort(8081)
              .Build();
        }
        public PulsarTestcontainer Container { get; }
        public virtual Task InitializeAsync()
        {
            /*var imge = new ImageFromDockerfileBuilder()
                .WithName("sharp-pulsar:2.9.1")                
                .WithDockerfileDirectory(".")
                .WithDeleteIfExists(false)
                .Build().GetAwaiter().GetResult();*/
           return Container.StartAsync();
        }

        public virtual async Task DisposeAsync()
        {
            await Container.DisposeAsync().AsTask();
        }

        public void Dispose()
        {
            _configuration.Dispose();
        }
    }
}
