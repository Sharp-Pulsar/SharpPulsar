using Docker.DotNet.Models;
using System.Text;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using JetBrains.Annotations;

namespace Testcontainers.Pulsar
{
    /// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}" />
    [PublicAPI]
    public sealed class PulsarBuilder : ContainerBuilder<PulsarBuilder, PulsarContainer, PulsarConfiguration>
    {
        public const string StartupScriptFilePath = "/testcontainers.sh";

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaBuilder" /> class.
        /// </summary>
        public PulsarBuilder()
            : this(new PulsarConfiguration())
        {
            DockerResourceConfiguration = Init().DockerResourceConfiguration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaBuilder" /> class.
        /// </summary>
        /// <param name="resourceConfiguration">The Docker resource configuration.</param>
        private PulsarBuilder(PulsarConfiguration resourceConfiguration)
            : base(resourceConfiguration)
        {
            DockerResourceConfiguration = resourceConfiguration;
        }

        /// <inheritdoc />
        protected override PulsarConfiguration DockerResourceConfiguration { get; }

        /// <inheritdoc />
        public override PulsarContainer Build()
        {
            Validate();
            return new PulsarContainer(DockerResourceConfiguration, TestcontainersSettings.Logger);
        }
        /// <inheritdoc />
        protected override PulsarBuilder Init()
        {
            return base.Init()
                .WithImage(KafkaImage)
                .WithPortBinding(KafkaPort, true)
                .WithPortBinding(BrokerPort, true)
                .WithPortBinding(ZookeeperPort, true)
                .WithEnvironment("KAFKA_LISTENERS", "PLAINTEXT://0.0.0.0:" + KafkaPort + ",BROKER://0.0.0.0:" + BrokerPort)
                .WithEnvironment("KAFKA_LISTENER_SECURITY_PROTOCOL_MAP", "BROKER:PLAINTEXT,PLAINTEXT:PLAINTEXT")
                .WithEnvironment("KAFKA_INTER_BROKER_LISTENER_NAME", "BROKER")
                .WithEnvironment("KAFKA_BROKER_ID", "1")
                .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
                .WithEnvironment("KAFKA_OFFSETS_TOPIC_NUM_PARTITIONS", "1")
                .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR", "1")
                .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_MIN_ISR", "1")
                .WithEnvironment("KAFKA_LOG_FLUSH_INTERVAL_MESSAGES", long.MaxValue.ToString())
                .WithEnvironment("KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS", "0")
                .WithEnvironment("KAFKA_ZOOKEEPER_CONNECT", "localhost:" + ZookeeperPort)
                .WithEntrypoint("/bin/sh", "-c")
                .WithCommand("while [ ! -f " + StartupScriptFilePath + " ]; do sleep 0.1; done; " + StartupScriptFilePath)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("\\[KafkaServer id=\\d+\\] started"))
                .WithStartupCallback((container, ct) =>
                {
                    const char lf = '\n';
                    var startupScript = new StringBuilder();
                    startupScript.Append("#!/bin/bash");
                    startupScript.Append(lf);
                    startupScript.Append("echo 'clientPort=" + ZookeeperPort + "' > zookeeper.properties");
                    startupScript.Append(lf);
                    startupScript.Append("echo 'dataDir=/var/lib/zookeeper/data' >> zookeeper.properties");
                    startupScript.Append(lf);
                    startupScript.Append("echo 'dataLogDir=/var/lib/zookeeper/log' >> zookeeper.properties");
                    startupScript.Append(lf);
                    startupScript.Append("zookeeper-server-start zookeeper.properties &");
                    startupScript.Append(lf);
                    startupScript.Append("export KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://" + container.Hostname + ":" + container.GetMappedPublicPort(KafkaPort) + ",BROKER://" + container.Hostname + ":" + BrokerPort);
                    startupScript.Append(lf);
                    startupScript.Append("echo '' > /etc/confluent/docker/ensure");
                    startupScript.Append(lf);
                    startupScript.Append("/etc/confluent/docker/run");
                    return container.CopyFileAsync(StartupScriptFilePath, Encoding.Default.GetBytes(startupScript.ToString()), 493, ct: ct);
                });
        }

        /// <inheritdoc />
        protected override PulsarBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        {
            return Merge(DockerResourceConfiguration, new PulsarConfiguration(resourceConfiguration));
        }

        /// <inheritdoc />
        protected override PulsarBuilder Clone(IContainerConfiguration resourceConfiguration)
        {
            return Merge(DockerResourceConfiguration, new PulsarConfiguration(resourceConfiguration));
        }
    }
}
