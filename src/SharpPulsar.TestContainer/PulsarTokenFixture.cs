using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using SharpPulsar.Builder;
using SharpPulsar.Configuration;
using Testcontainers.Pulsar;
using Xunit;

namespace SharpPulsar.TestContainer
{
    public class PulsarTokenFixture : IAsyncLifetime
    {
        //private const string AuthenticationPlugin = "org.apache.pulsar.client.impl.auth.AuthenticationToken";
        //private const string SecretKeyPath = "/pulsar/secret.key";
        //private const string UserName = "test-user";
        //private const int Port = 6650;
        public PulsarSystem? PulsarSystem;
        public ClientConfigurationData? ClientConfigurationData;
        public PulsarClientConfigBuilder? ConfigBuilder;
        public string? Token;
        private PulsarContainer? _container;
        private readonly IConfiguration _configuration;
        //private readonly IMessageSink _messageSink;
        //private readonly IContainerService _cluster;
        public PulsarContainer Container { get { return _container; } } 
        public PulsarTokenFixture(/*IMessageSink messageSink*/)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            _configuration = GetIConfigurationRoot(path);
            /*_messageSink = messageSink;

            var environmentVariables = new[]
            {
                $"PULSAR_PREFIX_tokenSecretKey=file://{SecretKeyPath}",
                "PULSAR_PREFIX_authenticationRefreshCheckSeconds=5",
                $"superUserRoles={UserName}",
                "authenticationEnabled=true",
                "authorizationEnabled=true",
                "authenticationProviders=org.apache.pulsar.broker.authentication.AuthenticationProviderToken",
                "authenticateOriginalAuthData=false",
                $"brokerClientAuthenticationPlugin={AuthenticationPlugin}",
                $"CLIENT_PREFIX_authPlugin={AuthenticationPlugin}",
                $"PULSAR_PREFIX_transactionCoordinatorEnabled=true",
                $"PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false",
                $"PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled=true",
                $"PULSAR_PREFIX_brokerEntryMetadataInterceptors=org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor"
            };

            var arguments = "\"" +
                $"bin/pulsar tokens create-secret-key --output {SecretKeyPath} && " +
                $"export brokerClientAuthenticationParameters=token:$(bin/pulsar tokens create --secret-key {SecretKeyPath} --subject {UserName}) && " +
                "export CLIENT_PREFIX_authParams=$brokerClientAuthenticationParameters && " +
                "bin/apply-config-from-env.py conf/standalone.conf && " +
                "bin/apply-config-from-env-with-prefix.py CLIENT_PREFIX_ conf/client.conf && " +
                "bin/pulsar standalone --no-functions-worker && " +
                "bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2"
                + "\"";

            _cluster = new Ductus.FluentDocker.Builders.Builder()
                .UseContainer()
                .UseImage("apachepulsar/pulsar-all:3.1.2")
                .WithEnvironment(environmentVariables)
                .ExposePort(Port)
                .Command("/bin/bash -c", arguments)
                .Build();*/

            ServiceUrl = "pulsar://localhost:6650";
        }
        public string ServiceUrl { get; private set; }
        public virtual async Task InitializeAsync()
        {
            _container = new PulsarTokenBuilder()
              .WithCleanUp(true)
              .Build();

            await _container.StartAsync();
            Console.WriteLine("Start Test Container");
            await AwaitPortReadiness($"http://127.0.0.1:8080/metrics/");

            //await Task.Delay(2000);
            //var s = await _container.ExecAsync(new List<string> { @"./bin/pulsar", "tokens", "create", "--secret-key", "/pulsar/secret.key", "--subject", "test-user" });
            //Token = s.Stdout;
            //_cluster.StateChange += (sender, args) => _messageSink.OnMessage(new DiagnosticMessage($"The Pulsar cluster changed state to: {args.State}"));
            //_cluster.Start();
            //_cluster.WaitForMessageInLogs("Successfully updated the policies on namespace public/default", int.MaxValue);
            //var endpoint = _cluster.ToHostExposedEndpoint($"{Port}/tcp");
            //await Task.Delay(5000);
            //_messageSink.OnMessage(new DiagnosticMessage($"Endpoint opened at {endpoint}"));
            //ServiceUrl = $"pulsar://localhost:6650";
            //Token = CreateToken(Timeout.InfiniteTimeSpan);
            SetupSystem();
            await Task.CompletedTask;
        }
        public IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
        public virtual void SetupSystem()
        {
            var client = new PulsarClientConfigBuilder();
            var clienConfigSetting = _configuration.GetSection("client");
            var serviceUrl = ServiceUrl;
            var webUrl = clienConfigSetting.GetSection("web-url").Value;
            var authPluginClassName = clienConfigSetting.GetSection("authPluginClassName").Value;
            var authParamsString = clienConfigSetting.GetSection("authParamsString").Value;
            var authCertPath = clienConfigSetting.GetSection("authCertPath").Value;
            var connectionsPerBroker = int.Parse(clienConfigSetting.GetSection("connections-per-broker").Value!);
            var statsInterval = TimeSpan.Parse(clienConfigSetting.GetSection("stats-interval").Value!);
            var operationTime = int.Parse(clienConfigSetting.GetSection("operationTime").Value!);
            var allowTlsInsecureConnection = bool.Parse(clienConfigSetting.GetSection("allowTlsInsecureConnection").Value!);
            var enableTls = bool.Parse(clienConfigSetting.GetSection("enableTls").Value!);
            var enableTxn = bool.Parse(clienConfigSetting.GetSection("enableTransaction").Value!);
            var dedicatedConnection = bool.Parse(clienConfigSetting.GetSection("userDedicatedConnection").Value!);


            client.EnableTransaction(enableTxn);

            if (operationTime > 0)
                client.OperationTimeout(TimeSpan.FromMilliseconds(operationTime));

            if (!string.IsNullOrWhiteSpace(authCertPath))
                client.AddTrustedAuthCert(new X509Certificate2(File.ReadAllBytes(authCertPath)));

            if (!string.IsNullOrWhiteSpace(authPluginClassName) && !string.IsNullOrWhiteSpace(authParamsString))
                client.Authentication(authPluginClassName, authParamsString);

            client.ServiceUrl(serviceUrl);
            client.WebUrl(webUrl);
            client.ConnectionsPerBroker(connectionsPerBroker);
            client.StatsInterval(statsInterval);
            client.AllowTlsInsecureConnection(allowTlsInsecureConnection);
            client.EnableTls(enableTls);
            //client.Authentication(AuthenticationFactory.Token(Token));
            //client.Authentication(AuthenticationFactoryOAuth2.ClientCredentials(issuerUrl, fileUri, audience));
            //PulsarSystem = PulsarSystem.GetInstance(actorSystemName: "token");
           
            ConfigBuilder = client;
            ClientConfigurationData = client.ClientConfigurationData;
        }
        private static async ValueTask AwaitPortReadiness(string address)
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
        public async  Task DisposeAsync()
        {

            //_cluster.Remove(true);
            //_cluster.Stop();
            //_cluster.Dispose();
            await _container.StopAsync();
            //return Task.CompletedTask;
        }
        /*public string CreateToken(TimeSpan expiryTime)
        {
            var arguments = $"bin/pulsar tokens create --secret-key {SecretKeyPath} --subject {UserName}";

            if (expiryTime != Timeout.InfiniteTimeSpan)
                arguments += $" --expiry-time {expiryTime.TotalSeconds}s";

            var result = _cluster.Execute(arguments);

            if (!result.Success)
                throw new InvalidOperationException($"Could not create the token: {result.Error}");

            return result.Data[0];
        }
        public void CreateSql()
        {
            var arguments = $"bin/pulsar sql-worker start";

            var result = _cluster.Execute(arguments);
            if (!result.Success)
                throw new InvalidOperationException($"Could not create the token: {result.Error}");

            //await AwaitPortReadiness($"http://127.0.0.1:8081/");
        }
        public void CreatePartitionedTopic(string topic, int numberOfPartitions)
        {
            var arguments = $"bin/pulsar-admin topics create-partitioned-topic {topic} -p {numberOfPartitions}";

            var result = _cluster.Execute(arguments);

            if (!result.Success)
                throw new Exception($"Could not create the partitioned topic: {result.Error}");
        }*/
    }
}
