﻿/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Akka.Actor;

namespace SharpPulsar.Test.NBench.Fixtures
{
    using DotNet.Testcontainers.Builders;
    using Microsoft.Extensions.Configuration;
    using SharpPulsar.Configuration;
    using SharpPulsar.Test.EventSourcing;
    using SharpPulsar.TestContainer;
    using SharpPulsar.User;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Xunit;
    //https://blog.dangl.me/archive/running-sql-server-integration-tests-in-net-core-projects-via-docker/
    public class NBenchFixture : PulsarFixture
    {
        public PulsarClient Client;
        public PulsarSystem PulsarSystem;
        public ClientConfigurationData ClientConfigurationData;

        private readonly NBenchContainerConfiguration _configuration;

        public NBenchFixture()
        {
            _configuration = new NBenchContainerConfiguration("apachepulsar/pulsar-all:2.9.1", 6650);
        }
        public IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
        public override TestcontainersBuilder<PulsarTestcontainer> BuildContainer()
        {
            return (TestcontainersBuilder<PulsarTestcontainer>)new TestcontainersBuilder<PulsarTestcontainer>()
               .WithName($"test-nbench")
               .WithPulsar(_configuration)
               .WithPortBinding(6650)
               .WithPortBinding(8080)
               .WithPortBinding(8081)
               .WithExposedPort(6650)
               .WithExposedPort(8080)
               .WithExposedPort(8081);
        }
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await SetupSystem();
        }
        public override async Task DisposeAsync()
        {
            Client?.Shutdown();
            await base.DisposeAsync();
        }
        private async ValueTask SetupSystem()
        {
            var client = new PulsarClientConfigBuilder();
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config = GetIConfigurationRoot(path);
            var clienConfigSetting = config.GetSection("client");
            var serviceUrl = clienConfigSetting.GetSection("service-url").Value;
            var webUrl = clienConfigSetting.GetSection("web-url").Value;
            var connectionsPerBroker = int.Parse(clienConfigSetting.GetSection("connections-per-broker").Value);
            var operationTime = int.Parse(clienConfigSetting.GetSection("operationTime").Value);
            client.ServiceUrl(serviceUrl);
            client.WebUrl(webUrl);
            client.ConnectionsPerBroker(connectionsPerBroker);
            var system = await PulsarSystem.GetInstanceAsync(client, actorSystemName:"pulsar-nbench");
            Client = system.NewClient();
            PulsarSystem = system;
            ClientConfigurationData = client.ClientConfigurationData;
        }
    }
}
