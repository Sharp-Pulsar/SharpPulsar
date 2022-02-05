using DotNet.Testcontainers.Builders;
using SharpPulsar.TestContainer.TestUtils;
using Xunit;

namespace SharpPulsar.TestContainer
{
    public abstract class PulsarFixture : IAsyncLifetime, IDisposable
    {
        public abstract PulsarTestcontainerConfiguration Configuration { get; }

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
