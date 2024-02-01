
namespace Testcontainers.Pulsar
{
    /// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}" />
    [PublicAPI]
    public sealed class PulsarTokenBuilder : ContainerBuilder<PulsarTokenBuilder, PulsarContainer, PulsarConfiguration>
    {
        public const string PulsarImage = "apachepulsar/pulsar-all:3.1.2";
        private const string AuthenticationPlugin = "org.apache.pulsar.client.impl.auth.AuthenticationToken";
        private const string SecretKeyPath = "/pulsar/secret.key";
        private const string UserName = "test-user";
        public ushort PulsarPort = 6650;
        public ushort PulsarAdminPort = 8080;
        public ushort PulsarSQLPort = 8081;
        public string Token { get; set; }

        public const string StartupScriptFilePath = "/testcontainers.sh";

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaBuilder" /> class.
        /// </summary>
        public PulsarTokenBuilder()
            : this(new PulsarConfiguration())
        {
            DockerResourceConfiguration = Init().DockerResourceConfiguration;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaBuilder" /> class.
        /// </summary>
        /// <param name="resourceConfiguration">The Docker resource configuration.</param>
        private PulsarTokenBuilder(PulsarConfiguration resourceConfiguration)
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
        protected override PulsarTokenBuilder Init()
        {
            return base.Init()
                .WithImage(PulsarImage)
                .WithPortBinding(PulsarPort, PulsarPort)
                .WithPortBinding(PulsarAdminPort, PulsarAdminPort)
                //.WithPortBinding(PulsarSQLPort, PulsarSQLPort)
                //.WithEnvironment("PULSAR_MEM", "-Xms512m -Xmx512m -XX:MaxDirectMemorySize=1g")
                .WithEnvironment("PULSAR_PREFIX_tokenSecretKey", $"file://{SecretKeyPath}")
                .WithEnvironment("PULSAR_PREFIX_authenticationRefreshCheckSeconds", "5")
                .WithEnvironment("superUserRoles", $"{UserName}")
                .WithEnvironment("authenticationEnabled", "true")
                .WithEnvironment("authorizationEnabled", "true")
                .WithEnvironment("authenticationProviders", "org.apache.pulsar.broker.authentication.AuthenticationProviderToken")
                .WithEnvironment("authenticateOriginalAuthData", "false")
                .WithEnvironment("brokerClientAuthenticationPlugin", $"{AuthenticationPlugin}")
                .WithEnvironment("CLIENT_PREFIX_authPlugin", $"{AuthenticationPlugin}")
                //.WithEnvironment("PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled", "true")
                //.WithEnvironment("PULSAR_PREFIX_nettyMaxFrameSizeBytes", "5253120")
                .WithEnvironment("PULSAR_PREFIX_transactionCoordinatorEnabled", "true")
                .WithEnvironment("PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled", "true")
                //.WithEnvironment("PULSAR_PREFIX_defaultRetentionTimeInMinutes", "-1") //Default message retention time. 0 means retention is disabled. -1 means data is not removed by time quota
                //.WithEnvironment("PULSAR_PREFIX_defaultRetentionSizeInMB", "-1") //Default retention size. 0 means retention is disabled. -1 means data is not removed by size quota
                .WithEnvironment("PULSAR_STANDALONE_USE_ZOOKEEPER", "1")
                .WithEnvironment("PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled", "true")
                .WithEnvironment("PULSAR_PREFIX_brokerEntryMetadataInterceptors", "org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor")

                .WithEntrypoint("/bin/sh", "-c")
                .WithCommand("while [ ! -f " + StartupScriptFilePath + " ]; do sleep 0.1; done; " + StartupScriptFilePath)

                .WithWaitStrategy(Wait.ForUnixContainer())
                .WithStartupCallback((container, ct) =>
                {
                    const char lf = '\n';
                    var startupScript = new StringBuilder();
                    startupScript.Append("#!/bin/bash");
                    startupScript.Append(lf);
                    startupScript.Append($"bin/pulsar tokens create-secret-key --output {SecretKeyPath} && ");
                    startupScript.Append($"export brokerClientAuthenticationParameters=token:$(bin/pulsar tokens create --secret-key {SecretKeyPath} --subject {UserName}) && ");
                    startupScript.Append("export CLIENT_PREFIX_authParams=$brokerClientAuthenticationParameters && ");

                    startupScript.Append("bin/apply-config-from-env.py conf/standalone.conf && ");
                    startupScript.Append("bin/apply-config-from-env-with-prefix.py CLIENT_PREFIX_ conf/client.conf && ");

                    startupScript.Append("bin/pulsar standalone --no-functions-worker && ");
                    startupScript.Append("bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2");

                    return container.CopyAsync(Encoding.Default.GetBytes(startupScript.ToString()), StartupScriptFilePath, Unix.FileMode755, ct: ct);
                });
        }

        /// <inheritdoc />
        protected override PulsarTokenBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        {
            return Merge(DockerResourceConfiguration, new PulsarConfiguration(resourceConfiguration));
        }

        protected override PulsarTokenBuilder Clone(IContainerConfiguration resourceConfiguration)
        {
            return Merge(DockerResourceConfiguration, new PulsarConfiguration(resourceConfiguration));
        }

        protected override PulsarTokenBuilder Merge(PulsarConfiguration oldValue, PulsarConfiguration newValue)
        {
            return new PulsarTokenBuilder(new PulsarConfiguration(oldValue, newValue));
        }
    }
}
