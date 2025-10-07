using System.Reflection;
using System.Text;
using Akka.Configuration;
using DotNet.Testcontainers.Configurations;
using Microsoft.Extensions.Configuration;
using SharpPulsar.Builder;
using SharpPulsar.Configuration;
using Testcontainers.Pulsar;
using Xunit;
namespace SharpPulsar.TestContainer
{
    public class PulsarFixture : IAsyncLifetime
    {
        public PulsarSystem? System;
        private readonly IConfiguration _configuration;
        public ClientConfigBuilder? ConfigBuilder;
        public ClientConfigurationData? ClientConfigurationData; 
        public string? Token;
        private PulsarContainer? _container; 
        private const string SecretKeyPath = "/pulsar/secret.key";
        private const string UserName = "test-user";

        public const string StartupScriptFilePath = "/testcontainers.sh";
        public PulsarContainer Container { get { return _container!; } }
        public PulsarFixture()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            _configuration = GetIConfigurationRoot(path);            
        }
        
       
        public IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
        public virtual void SetupSystem(string? service = null, string? web = null)
        {
            System = PulsarSystem.GetInstance(actorSystemName: "tests", config: ConfigurationFactory.ParseString(@"
            akka
            {
                log-dead-letters = off
                loglevel = DEBUG
			    log-config-on-start = on 
                loggers=[""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
			    actor 
                {              
				      debug 
				      {
					      receive = on
					      autoreceive = on
					      lifecycle = on
					      event-stream = on
					      unhandled = on
				      }  
			    }
                coordinated-shutdown
                {
                    exit-clr = on
                }
            }"));
            var client = new ClientConfigBuilder();
            var clienConfigSetting = _configuration.GetSection("client");
            var serviceUrl = service ?? clienConfigSetting.GetSection("service-url").Value;
            var webUrl = web ?? clienConfigSetting.GetSection("web-url").Value;
            //var authPluginClassName = clienConfigSetting.GetSection("authPluginClassName").Value;
            //var authParamsString = clienConfigSetting.GetSection("authParamsString").Value;
            //var authCertPath = clienConfigSetting.GetSection("authCertPath").Value;
            var connectionsPerBroker = int.Parse(clienConfigSetting.GetSection("connections-per-broker").Value!);
            var statsInterval = TimeSpan.Parse(clienConfigSetting.GetSection("stats-interval").Value!);
            //var operationTime = int.Parse(clienConfigSetting.GetSection("operationTime").Value!);
            var allowTlsInsecureConnection = bool.Parse(clienConfigSetting.GetSection("allowTlsInsecureConnection").Value!);
            var enableTls = bool.Parse(clienConfigSetting.GetSection("enableTls").Value!);
            var enableTxn = bool.Parse(clienConfigSetting.GetSection("enableTransaction").Value!);
            //var dedicatedConnection = bool.Parse(clienConfigSetting.GetSection("userDedicatedConnection").Value!);


            client.EnableTransaction(enableTxn);

            /* if (operationTime > 0)
                client.OperationTimeout(TimeSpan.FromMilliseconds(operationTime));

            if (!string.IsNullOrWhiteSpace(authCertPath))
                client.AddTrustedAuthCert(new X509Certificate2(File.ReadAllBytes(authCertPath)));

            if (!string.IsNullOrWhiteSpace(authPluginClassName) && !string.IsNullOrWhiteSpace(authParamsString))
                client.Authentication(authPluginClassName, authParamsString);
            */
            client.ServiceUrl(serviceUrl);
            client.WebUrl(webUrl);
            client.ConnectionsPerBroker(connectionsPerBroker);
            client.StatsInterval(statsInterval);
            client.AllowTlsInsecureConnection(allowTlsInsecureConnection);
            //client.Authentication(AuthenticationFactory.Token(token));
            client.EnableTls(enableTls);
            ConfigBuilder = client;
            ClientConfigurationData = client.ClientConfigurationData;
        }
        public virtual async Task InitializeAsync()
        {
            _container = new PulsarBuilder()
            .WithImage("apachepulsar/pulsar-all:3.1.2")
            .WithPortBinding(6650, 6650)
            .WithPortBinding(6651, 6651)
            .WithPortBinding(8080, 8080)
            .WithPortBinding(8081, 8081)
            .WithEnvironment("PULSAR_MEM", "-Xms512m -Xmx512m -XX:MaxDirectMemorySize=1g")
            .WithEnvironment("PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled", "true")
            .WithEnvironment("PULSAR_PREFIX_nettyMaxFrameSizeBytes", "5253120")
            .WithEnvironment("PULSAR_PREFIX_transactionCoordinatorEnabled", "true")
            .WithEnvironment("PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled", "true")
            .WithEnvironment("PULSAR_PREFIX_defaultRetentionTimeInMinutes", "-1") //Default message retention time. 0 means retention is disabled. -1 means data is not removed by time quota
            .WithEnvironment("PULSAR_PREFIX_defaultRetentionSizeInMB", "-1") //Default retention size. 0 means retention is disabled. -1 means data is not removed by size quota
            .WithEnvironment("PULSAR_STANDALONE_USE_ZOOKEEPER", "1")
            .WithEnvironment("PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled", "true")
            .WithEnvironment("PULSAR_PREFIX_brokerEntryMetadataInterceptors", "org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor")
            //.WithAuthentication()
            //.WithWaitStrategy(Wait.ForUnixContainer())

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
            })
            .WithCleanUp(true)
            .Build();
            Console.WriteLine("Test Container");
            await _container.StartAsync();//;.GetAwaiter().GetResult();]
            Console.WriteLine("Start Test Container");
            await AwaitPortReadiness($"http://127.0.0.1:8080/metrics/");
            Console.WriteLine("ExecAsync Test Container");
            await Container.ExecAsync(new List<string> { @"./bin/pulsar", "sql-worker", "start" });

            await AwaitPortReadiness($"http://127.0.0.1:8081/");
            Console.WriteLine("AwaitPortReadiness Test Container");
            //var s = await _container.ExecAsync(new List<string> { @"./bin/pulsar", "tokens", "create", "--secret-key", "/pulsar/secret.key", "--subject", "test-user" });
            SetupSystem();
            await Task.CompletedTask;
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
        public async Task DisposeAsync()
        {
            await _container!.StopAsync();
        }
        
    }
}
