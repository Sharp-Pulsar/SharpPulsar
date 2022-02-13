﻿using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Tutorials.PulsarTestContainer
{
    public class TestcontainerConfiguration : TestcontainerMessageBrokerConfiguration
    {
        public string StartupScriptPath = "/testcontainers_start.sh";
        /// Initializes a new instance of the <see cref="TestcontainerConfiguration" /> class.
        /// </summary>
        /// <param name="image">The Docker image.</param>
        public TestcontainerConfiguration(string image, int pulsarPort)
          : base(image, pulsarPort)
        {
            Environments.Add("PULSAR_MEM", "-Xms512m -Xmx512m -XX:MaxDirectMemorySize=1g");
            Environments.Add("PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled", "true");
            Environments.Add("PULSAR_PREFIX_nettyMaxFrameSizeBytes", "5253120");
            Environments.Add("PULSAR_PREFIX_transactionCoordinatorEnabled", "true");
            Environments.Add("PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled", "false");
            Environments.Add("PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled", "true");
            Environments.Add("PULSAR_PREFIX_brokerEntryMetadataInterceptors", "org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor");
        }

        /// <summary>
        /// Gets the startup callback.
        /// </summary>
        public virtual Func<IRunningDockerContainer, CancellationToken, Task> StartupCallback
          => (container, ct) =>
          {
              return Task.CompletedTask;
              // startupScript = new StringBuilder();
              //startupScript.AppendLine("#!/bin/sh");
              //return container.CopyFileAsync(StartupScriptPath, Encoding.UTF8.GetBytes(startupScript.ToString()), 0x1ff, ct: ct);
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
