using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using SharpPulsar.TestContainer;

namespace SharpPulsar.Test.Fixture
{
    public  class PulsarMultiTopicFixture: PulsarFixture
    {
        public override TestcontainersBuilder<PulsarTestcontainer> BuildContainer()
        {
            return (TestcontainersBuilder<PulsarTestcontainer>)new TestcontainersBuilder<PulsarTestcontainer>()
              .WithName("multi-topic-integration-tests")
              .WithPulsar(Configuration)
              .WithPortBinding(6670, 6650)
              .WithPortBinding(8090, 8080)
              .WithPortBinding(8091, 8081)
              .WithExposedPort(6650)
              .WithExposedPort(8080)
              .WithExposedPort(8081);
        }

        public override async Task InitializeAsync()
        {
            await Container.StartAsync();//;.GetAwaiter().GetResult();
            await AwaitPortReadiness($"http://127.0.0.1:8090/metrics/");
            await Container.ExecAsync(new List<string> { @"./bin/pulsar", "sql-worker", "start" });

            await AwaitPortReadiness($"http://127.0.0.1:8091/");
            await SetupSystem("pulsar://localhost:6670", "http://localhost:8090");
        }
    }
}
