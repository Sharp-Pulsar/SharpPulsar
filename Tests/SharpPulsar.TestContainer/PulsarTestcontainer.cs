using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;
using SharpPulsar.TestContainer.TestUtils;

namespace SharpPulsar.TestContainer
{
    public sealed class PulsarTestcontainer : TestcontainerMessageBroker
    {
        public readonly DIContainer DIContainer = DIContainer.Default;
        /// <summary>
        /// Initializes a new instance of the <see cref="PulsarTestcontainer" /> class.
        /// </summary>
        /// <param name="configuration">The Testcontainers configuration.</param>
        /// <param name="logger">The logger.</param>
        internal PulsarTestcontainer(ITestcontainersConfiguration configuration, ILogger logger)
          : base(configuration, logger)
        {
            DIContainer.RegisterDockerClient();
        }
        public override Task StopAsync(CancellationToken ct = default)
        {
            DIContainer.DisposeAsync().GetAwaiter().GetResult(); 
            return base.StopAsync(ct);
        }
        public async Task<GetArchiveFromContainerResponse> CopyFilesFromContainer(string path)
        {
            return await DIContainer.Get<DockerClient>().Containers.GetArchiveFromContainerAsync(path, Id);
        }
        public async Task<GetArchiveFromContainerResponse> CopyFilesFromContainer(string containerName, string path)
        {
            return await DIContainer.Get<DockerClient>().Containers.GetArchiveFromContainerByNameAsync(path, containerName);
        }
    }
}