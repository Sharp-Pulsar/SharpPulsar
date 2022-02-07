using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Configuration;
using SharpPulsar.Configuration;
using SharpPulsar.User;
using Xunit;

namespace SharpPulsar.TestContainer
{
    public class PulsarFixture : IAsyncLifetime, IDisposable
    {
        public PulsarClient Client;
        public PulsarSystem PulsarSystem;
        public ClientConfigurationData ClientConfigurationData;
        private readonly IConfiguration _configuration;  
        private readonly int _pulsarPort; 
        private readonly int _adminPort; 
        private readonly int _sqlPort; 
        public virtual PulsarTestcontainerConfiguration Configuration { get; }

        public PulsarFixture()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _configuration = GetIConfigurationRoot(path);
            var clienConfigSetting = _configuration.GetSection("client");
            _pulsarPort = int.Parse(clienConfigSetting.GetSection("pulsar-port").Value);
            _adminPort = int.Parse(clienConfigSetting.GetSection("admin-port").Value);
            _sqlPort = int.Parse(clienConfigSetting.GetSection("sql-port").Value);
            var pulsarImage = clienConfigSetting.GetSection("pulsar-image").Value;
            Configuration = new PulsarTestcontainerConfiguration(pulsarImage, _pulsarPort);
            Container = BuildContainer()
                .WithCleanUp(true)
                .Build();
        }
        public virtual TestcontainersBuilder<PulsarTestcontainer> BuildContainer()
        {
            return (TestcontainersBuilder<PulsarTestcontainer>)new TestcontainersBuilder<PulsarTestcontainer>()
              .WithName($"sharp-pulsar-test-container")
              .WithPulsar(Configuration)
              .WithPortBinding(_pulsarPort, 6650)
              .WithPortBinding(_adminPort, 8080)
              .WithPortBinding(_sqlPort, 8081)
              .WithExposedPort(6650)
              .WithExposedPort(8080)
              .WithExposedPort(8081);
        }
        public PulsarTestcontainer Container { get; }
        public virtual Task InitializeAsync()
        {           
            Container.StartAsync().GetAwaiter().GetResult();
            AwaitPortReadiness($"http://127.0.0.1:{_adminPort}/metrics/").GetAwaiter().GetResult();
            Container.ExecAsync(new List<string> { @"./bin/pulsar", "sql-worker", "start" }).GetAwaiter().GetResult();

            AwaitPortReadiness($"http://127.0.0.1:{_sqlPort}/").GetAwaiter().GetResult();
            SetupSystem().GetAwaiter().GetResult();
            return Task.CompletedTask;  
        }
        public async ValueTask AwaitPortReadiness(string address)
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
        public virtual async Task DisposeAsync()
        {
            await Container.DisposeAsync().AsTask();
            try
            {
                if (Client != null)
                    await Client.ShutdownAsync();
            }
            catch
            {

            }
        }

        public IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
        public virtual async ValueTask SetupSystem()
        {
            var client = new PulsarClientConfigBuilder();
            var clienConfigSetting = _configuration.GetSection("client");
            var serviceUrl = clienConfigSetting.GetSection("service-url").Value;
            var webUrl = clienConfigSetting.GetSection("web-url").Value;
            var authPluginClassName = clienConfigSetting.GetSection("authPluginClassName").Value;
            var authParamsString = clienConfigSetting.GetSection("authParamsString").Value;
            var authCertPath = clienConfigSetting.GetSection("authCertPath").Value;
            var connectionsPerBroker = int.Parse(clienConfigSetting.GetSection("connections-per-broker").Value);
            var statsInterval = TimeSpan.Parse(clienConfigSetting.GetSection("stats-interval").Value);
            var operationTime = int.Parse(clienConfigSetting.GetSection("operationTime").Value);
            var allowTlsInsecureConnection = bool.Parse(clienConfigSetting.GetSection("allowTlsInsecureConnection").Value);
            var enableTls = bool.Parse(clienConfigSetting.GetSection("enableTls").Value);
            var enableTxn = bool.Parse(clienConfigSetting.GetSection("enableTransaction").Value);
            var dedicatedConnection = bool.Parse(clienConfigSetting.GetSection("userDedicatedConnection").Value);


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
            var system = await PulsarSystem.GetInstanceAsync(client);
            Client = system.NewClient();
            PulsarSystem = system;
            ClientConfigurationData = client.ClientConfigurationData;
        }
        public void Dispose()
        {
            Configuration.Dispose();
        }
    }
}
