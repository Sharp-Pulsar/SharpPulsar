using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;

namespace Tutorials.PulsarTestContainer
{
    public static class TestContainerBuilderExtension
    {
        public static ITestcontainersBuilder<TestContainer> WithPulsar(this ITestcontainersBuilder<TestContainer> builder, TestcontainerConfiguration configuration)
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
        public static async Task PulsarWait(this Task task, string address)
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
    }
}

