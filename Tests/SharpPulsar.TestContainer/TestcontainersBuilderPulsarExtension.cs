
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

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
        public static ITestcontainersBuilder<TestcontainersContainer> WithPulsar(this ITestcontainersBuilder<TestcontainersContainer> builder, PulsarTestcontainerConfiguration configuration)
        {
            builder = configuration.Environments.Aggregate(builder, (current, environment)
              => current.WithEnvironment(environment.Key, environment.Value));

            return builder
              .WithImage(configuration.Image)
              .WithCommand(configuration.Command)
              .WithPortBinding(configuration.Port, configuration.DefaultPort)
              .WithWaitStrategy(configuration.WaitStrategy)
              .WithStartupCallback(configuration.StartupCallback);
        }
    }
}
