using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using SharpPulsar.TestContainer.TestUtils;

namespace SharpPulsar.TestContainer
{
    public static class TestcontainersBuilderPulsarExtension
    {
        public static ITestcontainersBuilder<PulsarTestcontainer> WithPulsar(this ITestcontainersBuilder<PulsarTestcontainer> builder, PulsarTestcontainerConfiguration configuration)
        {
            builder = configuration.Environments.Aggregate(builder, (current, environment)
              => current.WithEnvironment(environment.Key, environment.Value));

            return builder
              .WithImage(configuration.Image)
              .WithCommand(configuration.Command)
              .WithPortBinding(configuration.Port, configuration.DefaultPort)
              .WithWaitStrategy(configuration.WaitStrategy)
              .WithStartupCallback(configuration.StartupCallback)
              .ConfigureContainer(container =>
              {
                  container.ContainerPort = configuration.DefaultPort;
              });
        }
    }
}
