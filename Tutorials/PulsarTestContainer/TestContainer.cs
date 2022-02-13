using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;

namespace Tutorials.PulsarTestContainer
{
    public class TestContainer : TestcontainerMessageBroker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestContainer" /> class.
        /// </summary>
        /// <param name="configuration">The Testcontainers configuration.</param>
        /// <param name="logger">The logger.</param>
        internal TestContainer(ITestcontainersConfiguration configuration, ILogger logger)
          : base(configuration, logger)
        {
            
        }
        public override Task StopAsync(CancellationToken ct = default)
        {
            return base.StopAsync(ct);
        }

    }
}
