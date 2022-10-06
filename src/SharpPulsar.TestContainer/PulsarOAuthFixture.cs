using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Configuration;
using SharpPulsar.Auth.OAuth2;
using SharpPulsar.Builder;
using SharpPulsar.Configuration;
using SharpPulsar.TestContainer.Configuration;
using SharpPulsar.TestContainer.Container;
using SharpPulsar.User;
using Xunit;

namespace SharpPulsar.TestContainer
{
    public class PulsarOAuthFixture : IAsyncLifetime
    {
        public PulsarClient Client;
        public PulsarSystem PulsarSystem;
        private readonly IConfiguration _configuration;

        public ClientConfigurationData ClientConfigurationData;
        public virtual PulsarTestOAuthContainerConfiguration Configuration { get; }
        public PulsarOAuthFixture()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _configuration = GetIConfigurationRoot(path);
            Configuration = new PulsarTestOAuthContainerConfiguration("apachepulsar/pulsar-all:2.10.1", 6655);
            Container = BuildContainer()
                .WithCleanUp(true)
                .Build();
        }
        public virtual TestcontainersBuilder<PulsarTestOAuthContainer> BuildContainer()
        {
            return (TestcontainersBuilder<PulsarTestOAuthContainer>)new TestcontainersBuilder<PulsarTestOAuthContainer>()
              .WithName("oauth-tests")
              .WithPulsar(Configuration)
              .WithPortBinding(6655, 6650)
              .WithPortBinding(8085, 8080)
              .WithPortBinding(8085, 8081)
              .WithExposedPort(6655)
              .WithExposedPort(8085)
              .WithExposedPort(8085);
        }
        public PulsarTestOAuthContainer Container { get; }
        public virtual async Task InitializeAsync()
        {
            await Container.StartAsync();//;.GetAwaiter().GetResult();
            await AwaitPortReadiness($"http://127.0.0.1:8085/metrics/");
            await Container.ExecAsync(new List<string> { @"./bin/pulsar", "sql-worker", "start" });

            await AwaitPortReadiness($"http://127.0.0.1:8085/");
            await SetupSystem();
        }
        public IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
        public virtual async ValueTask SetupSystem(string? service = null, string? web = null)
        {
            var fileUri = new Uri(GetConfigFilePath());
            var issuerUrl = new Uri("https://auth.streamnative.cloud/");
            var audience = "urn:sn:pulsar:o-r7y4o:sharp";
            var client = new PulsarClientConfigBuilder();
            var clienConfigSetting = _configuration.GetSection("client");
            var serviceUrl = service ?? clienConfigSetting.GetSection("service-url").Value;

            var webUrl = web ?? clienConfigSetting.GetSection("web-url").Value;
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
            
            client.ServiceUrl(serviceUrl);
            client.WebUrl(webUrl);
            client.ConnectionsPerBroker(connectionsPerBroker);
            client.StatsInterval(statsInterval);
            client.AllowTlsInsecureConnection(allowTlsInsecureConnection);
            client.EnableTls(enableTls);
            client.Authentication(AuthenticationFactoryOAuth2.ClientCredentials(issuerUrl, fileUri, audience));
            
            var system = await PulsarSystem.GetInstanceAsync(client);
            Client = system.NewClient();
            PulsarSystem = system;
            ClientConfigurationData = client.ClientConfigurationData;
        }
        public Task DisposeAsync()
        {
            try
            {
                if (Client != null)
                    Client.ShutdownAsync().Wait();
            }
            catch
            {

            }
            return Task.CompletedTask;
        }
        
        private async ValueTask AwaitPortReadiness(string address)
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
            return File.ReadAllText(ret);
        }
    }
}
