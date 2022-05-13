﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Configuration;
using SharpPulsar.Builder;
using SharpPulsar.Interfaces;
using SharpPulsar.ServiceProvider;
using SharpPulsar.User;
using Xunit;

namespace SharpPulsar.TestContainer.ServiceProviderFixtures
{
    public class AutoClusterFailoverFixture : IAsyncLifetime, IDisposable
    {
        public PulsarClient Client;
        public PulsarSystem PulsarSystem;
        private readonly IConfiguration _configuration;
        public virtual PulsarTestcontainerConfiguration Configuration { get; }

        public AutoClusterFailoverFixture()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _configuration = GetIConfigurationRoot(path);
            Configuration = new PulsarTestcontainerConfiguration("apachepulsar/pulsar-all:2.10.0", 6650);
            Container = BuildContainer()
                .WithCleanUp(true)
                .Build();
        }
        public virtual TestcontainersBuilder<PulsarTestcontainer> BuildContainer()
        {
            return (TestcontainersBuilder<PulsarTestcontainer>)new TestcontainersBuilder<PulsarTestcontainer>()
              .WithName("auto-failover-primary")
              .WithPulsar(Configuration)
              .WithPortBinding(6656, 6650)
              .WithPortBinding(8082, 8080)
              .WithExposedPort(6650)
              .WithExposedPort(8080);
        }
        public PulsarTestcontainer Container { get; }
        public virtual async Task InitializeAsync()
        {
            await Container.StartAsync();//;.GetAwaiter().GetResult();
            await AwaitPortReadiness($"http://127.0.0.1:8082/metrics/");
            await SetupSystem();
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

            try
            {
                await Container.DisposeAsync().AsTask();
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


            var client = new PulsarClientConfigBuilder();
            var secondary = clienConfigSetting.GetRequiredSection("secondary-url").Value;
            var failoverDelay = int.Parse(clienConfigSetting.GetRequiredSection("failover-delay").Value);
            var switchBackDelay = int.Parse(clienConfigSetting.GetRequiredSection("switch-back-delay").Value);
            var checkInterval = int.Parse(clienConfigSetting.GetRequiredSection("check-interval").Value);
            var builder = AutoClusterFailover.Builder().Primary(serviceUrl)
            .Secondary(new List<string> { secondary })
            .FailoverDelay(TimeSpan.FromSeconds(failoverDelay))
            .SwitchBackDelay(TimeSpan.FromSeconds(switchBackDelay))
            .CheckInterval(TimeSpan.FromSeconds(checkInterval));
            client.ServiceUrlProvider(new AutoClusterFailover((AutoClusterFailoverBuilder)builder));
            client.EnableTransaction(enableTxn);

            if (operationTime > 0)
                client.OperationTimeout(TimeSpan.FromMilliseconds(operationTime));

            if (!string.IsNullOrWhiteSpace(authCertPath))
                client.AddTrustedAuthCert(new X509Certificate2(File.ReadAllBytes(authCertPath)));

            if (!string.IsNullOrWhiteSpace(authPluginClassName) && !string.IsNullOrWhiteSpace(authParamsString))
                client.Authentication(authPluginClassName, authParamsString);

            client.WebUrl(webUrl);
            client.ConnectionsPerBroker(connectionsPerBroker);
            client.StatsInterval(statsInterval);
            client.AllowTlsInsecureConnection(allowTlsInsecureConnection);
            client.EnableTls(enableTls);
            var system = await PulsarSystem.GetInstanceAsync(client);
            Client = system.NewClient();
            PulsarSystem = system;
        }
        public void Dispose()
        {
            Configuration.Dispose();
        }
    }
}

