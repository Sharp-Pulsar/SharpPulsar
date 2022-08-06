using System.Reflection;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using System.Text.Json;

namespace SharpPulsar.TestContainer.Configuration
{
    public class PulsarTestOAuthContainerConfiguration : TestcontainerMessageBrokerConfiguration
    {
        public string StartupScriptPath = "/testcontainers_start.sh";
        private const string UserName = "test-user";

        /// Initializes a new instance of the <see cref="PulsarTestcontainerConfiguration" /> class.
        /// </summary>
        /// <param name="image">The Docker image.</param>
        public PulsarTestOAuthContainerConfiguration(string image, int pulsarPort)
          : base(image, pulsarPort)
        {
            var authParam = new Dictionary<string, string> {
                { "privateKey", $"{GetConfigFilePath()}" },
                {"issuerUrl", "https://auth.streamnative.cloud/" },
                {"audience","urn:sn:pulsar:o-r7y4o:sharp" }
            };
            var json = JsonSerializer.Serialize(authParam);  
            Environments.Add("superUserRoles", $"{UserName}");
            Environments.Add("authenticationEnabled", "true");
            Environments.Add("authorizationEnabled", "true");
            Environments.Add("authenticateOriginalAuthData", "false");
            Environments.Add("authenticationProviders", "org.apache.pulsar.broker.authentication.AuthenticationProviderToken");
            Environments.Add("authorizationProvider", "org.apache.pulsar.broker.authorization.PulsarAuthorizationProvider");
            Environments.Add("brokerClientAuthenticationPlugin", "org.apache.pulsar.client.impl.auth.oauth2.AuthenticationOAuth2");
            Environments.Add("brokerClientAuthenticationParameters", json);
            Environments.Add("authorizationAllowWildcardsMatching", "false");
            Environments.Add("tokenPublicKey", "file:///pulsar/oauth_public2.key");
            Environments.Add("tokenAuthClaim", "https://pulsar.com/tokenAuthClaim");
            Environments.Add("PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled", "true");
            Environments.Add("PULSAR_PREFIX_brokerEntryMetadataInterceptors", "org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor");
            
            /*Environments.Add("CLIENT_PREFIX_tlsAllowInsecureConnection", "false");
            Environments.Add("CLIENT_PREFIX_tlsEnableHostnameVerification", "true");
            Environments.Add("CLIENT_PREFIX_authParams", json);
            Environments.Add("CLIENT_PREFIX_tlsAllowInsecureConnection", "false");
            Environments.Add("CLIENT_PREFIX_tlsAllowInsecureConnection", "false");*/
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
          = { "/bin/sh", "-c", $"bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone --no-functions-worker && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2" };


        /// <inheritdoc />
        public override IWaitForContainerOS WaitStrategy => Wait.ForUnixContainer()
          .UntilPortIsAvailable(DefaultPort);
        private string GetConfigFilePath()
        {
            var configFolderName = "Oauth2Files";
            var privateKeyFileName = "o-r7y4o-eabanonu.json";
            var startup = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var indexOfConfigDir = startup.IndexOf(startup, StringComparison.Ordinal);
            var examplesFolder = startup.Substring(0, startup.Length - indexOfConfigDir);
            var configFolder = Path.Combine(examplesFolder, configFolderName);
            var ret = Path.Combine(configFolder, privateKeyFileName);
            if (!File.Exists(ret)) throw new FileNotFoundException("can't find credentials file");
            return ret;
        }
    }
}
