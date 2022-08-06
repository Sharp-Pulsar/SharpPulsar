﻿using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace SharpPulsar.TestContainer.Configuration
{
    public class PulsarTestTokenContainerConfiguration : TestcontainerMessageBrokerConfiguration
    {
        public string StartupScriptPath = "/testcontainers_start.sh";
        private const string AuthenticationPlugin = "org.apache.pulsar.client.impl.auth.AuthenticationToken";
        private const string SecretKeyPath = "/pulsar/secret.key";
        private const string UserName = "test-user";
        /// Initializes a new instance of the <see cref="PulsarTestcontainerConfiguration" /> class.
        /// </summary>
        /// <param name="image">The Docker image.</param>
        public PulsarTestTokenContainerConfiguration(string image, int pulsarPort)
          : base(image, pulsarPort)
        {
            
            Environments.Add("PULSAR_PREFIX_tokenSecretKey",$"file://{SecretKeyPath}");
            Environments.Add("PULSAR_PREFIX_authenticationRefreshCheckSeconds", "5");
            Environments.Add("superUserRoles", $"{UserName}");
            Environments.Add("authenticationEnabled", "true");
            Environments.Add("authorizationEnabled", "true");
            Environments.Add("authenticationProviders", "org.apache.pulsar.broker.authentication.AuthenticationProviderToken");
            Environments.Add("authenticateOriginalAuthData", "false");
            Environments.Add("brokerClientAuthenticationPlugin", $"{AuthenticationPlugin}");
            Environments.Add("CLIENT_PREFIX_authPlugin", $"{AuthenticationPlugin}");
            Environments.Add("PULSAR_PREFIX_transactionCoordinatorEnabled", "true");
            Environments.Add("PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled", "false");
            Environments.Add("authenticateOriginalAuthData", "false");
            Environments.Add("PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled", "true");
            Environments.Add("PULSAR_PREFIX_brokerEntryMetadataInterceptors", "org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor");           
        }

        public virtual void Env(params (string key, string value)[] envs)
        {
            foreach (var env in envs)
            {
                Environments.Add(env.key, env.value);
            }
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
          = { "/bin/sh", "-c", $"bin/pulsar tokens create-secret-key --output {SecretKeyPath} && export brokerClientAuthenticationParameters=token:$(bin/pulsar tokens create --secret-key {SecretKeyPath} --subject {UserName}) && export CLIENT_PREFIX_authParams=$brokerClientAuthenticationParameters && bin/apply-config-from-env.py conf/standalone.conf && bin/apply-config-from-env-with-prefix.py CLIENT_PREFIX_ conf/client.conf && bin/pulsar standalone --no-functions-worker && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2" };


        /// <inheritdoc />
        public override IWaitForContainerOS WaitStrategy => Wait.ForUnixContainer()
          .UntilPortIsAvailable(DefaultPort);
    }
}
