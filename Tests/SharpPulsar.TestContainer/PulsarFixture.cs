using DotNet.Testcontainers.Builders;
using Xunit;

namespace SharpPulsar.TestContainer
{
    public abstract class PulsarFixture : IAsyncLifetime, IDisposable
    {
        public PulsarTestcontainerConfiguration Configuration = new PulsarTestcontainerConfiguration();

        public PulsarFixture()
        {
            Container = BuildContainer()
                .WithCleanUp(true)
                .Build();
        }
        public abstract TestcontainersBuilder<PulsarTestcontainer> BuildContainer();
        public PulsarTestcontainer Container { get; }
        public virtual Task InitializeAsync()
        {            
           return Container.StartAsync();
        }

        public virtual async Task DisposeAsync()
        {
            await Container.DisposeAsync().AsTask();
        }

        public void Dispose()
        {
            Configuration.Dispose();
        }
    }
}
