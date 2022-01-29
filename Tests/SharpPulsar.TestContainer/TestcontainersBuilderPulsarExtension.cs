using DotNet.Testcontainers.Builders;

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
              .WithPortBinding(8080, 8080)
              .WithPortBinding(8081, 8081)
              .WithWaitStrategy(configuration.WaitStrategy)
              //.WithStartupCallback(configuration.StartupCallback)
              .ConfigureContainer(container =>
              {
                  container.ContainerPort = configuration.DefaultPort;
              });
        }
    }
}
