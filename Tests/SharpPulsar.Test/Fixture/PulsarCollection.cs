

namespace SharpPulsar.Test.Fixture
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using SharpPulsar.Builder;
    using SharpPulsar.TestContainer;
    using SharpPulsar.User;
    using Xunit;

    [CollectionDefinition(nameof(PulsarCollection), DisableParallelization = true)]
    public class PulsarCollection : ICollectionFixture<PulsarFixture> 
    {
        public PulsarClient Client;
        public PulsarSystem PulsarSystem;
        private readonly IConfiguration _configuration;
        public PulsarCollection()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _configuration = GetIConfigurationRoot(path);
            var client = SetupSystem();
            var system = PulsarSystem.GetInstanceAsync(client).Result;
            Client = system.NewClient();
            PulsarSystem = system;
        }
        
        private PulsarClientConfigBuilder SetupSystem(string? service = null, string? web = null)
        {
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


            var client = new PulsarClientConfigBuilder();
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
            return client;
        }
        public IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
    }
}
