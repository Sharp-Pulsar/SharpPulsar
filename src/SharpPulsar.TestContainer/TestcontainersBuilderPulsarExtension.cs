
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using SharpPulsar.TestContainer.Configuration;
using SharpPulsar.TestContainer.Container;

namespace SharpPulsar.TestContainer
{
    public static class TestcontainersBuilderPulsarExtension
    {
        public static ContainerBuilder<PulsarTestContainer> WithPulsar(this ContainerBuilder<PulsarTestContainer> builder, PulsarTestContainerConfiguration configuration)
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
      
        public static ITestcontainersBuilder<PulsarTestOAuthContainer> WithPulsar(this ITestcontainersBuilder<PulsarTestOAuthContainer> builder, PulsarTestOAuthContainerConfiguration configuration)
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
        public static ITestcontainersBuilder<TestcontainersContainer> WithPulsar(this ITestcontainersBuilder<TestcontainersContainer> builder, PulsarTestContainerConfiguration configuration)
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
