using System.Linq;
using System.Text;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace SharpPulsar.TestContainer
{
    public class PulsarTestcontainerConfiguration : TestcontainerMessageBrokerConfiguration
    {
        public string StartupScriptPath = "/testcontainers_start.sh";
        /// Initializes a new instance of the <see cref="PulsarTestcontainerConfiguration" /> class.
        /// </summary>
        /// <param name="image">The Docker image.</param>
        public PulsarTestcontainerConfiguration(string image, int pulsarPort)
          : base(image, pulsarPort)
        {
            Environments.Add("PULSAR_MEM","-Xms512m -Xmx512m -XX:MaxDirectMemorySize=1g");
            Environments.Add("PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled","true");
            Environments.Add("PULSAR_PREFIX_nettyMaxFrameSizeBytes","5253120");
            Environments.Add("PULSAR_PREFIX_transactionCoordinatorEnabled","true");
            Environments.Add("PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled","false");
            Environments.Add("PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled","true");
            Environments.Add("PULSAR_PREFIX_brokerEntryMetadataInterceptors","org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor");           
        }
        /// <summary>
        /// Gets the startup callback.
        /// </summary>
        public virtual Func<IRunningDockerContainer, CancellationToken, Task> StartupCallback
          => (container, ct) =>
          {
              var startupScript = new StringBuilder();
              startupScript.AppendLine("#!/bin/sh");
              //startupScript.AppendLine($"echo 'clientPort={ZookeeperPort}' > zookeeper.properties");
              startupScript.AppendLine("echo 'dataDir=/var/lib/zookeeper/data' >> zookeeper.properties");
              startupScript.AppendLine("echo 'dataLogDir=/var/lib/zookeeper/log' >> zookeeper.properties");
              startupScript.AppendLine("zookeeper-server-start zookeeper.properties &");
              //startupScript.AppendLine($"export KAFKA_ADVERTISED_LISTENERS='PLAINTEXT://{container.Hostname}:{container.GetMappedPublicPort(DefaultPort)},BROKER://localhost:{AdminPort}'");
              startupScript.AppendLine(". /etc/confluent/docker/bash-config");
              startupScript.AppendLine("/etc/confluent/docker/configure");
              startupScript.AppendLine("/etc/confluent/docker/launch");
              return container.CopyFileAsync(StartupScriptPath, Encoding.UTF8.GetBytes(startupScript.ToString()), 0x1ff, ct: ct);
          };

        /// <summary>
        /// Gets the command.
        /// </summary>
        public virtual string[] Command { get; }
          = { "/bin/sh", "-c", "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nfw && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2" };

        
        /// <inheritdoc />
        public override IWaitForContainerOS WaitStrategy => Wait.ForUnixContainer()
          .UntilPortIsAvailable(DefaultPort);
    }
    
 }

