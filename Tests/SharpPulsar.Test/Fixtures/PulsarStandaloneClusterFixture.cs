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

namespace SharpPulsar.Test.Fixtures
{
    using DotNet.Testcontainers.Builders;
    using Microsoft.Extensions.Configuration;
    using SharpPulsar.Configuration;
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

    //txn = docker run --name pulsar_local -it --env PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true --env PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120 --env PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false --env PULSAR_PREFIX_enableExposingBrokerEntryMetadataToClient=true --env PULSAR_PREFIX_transactionCoordinatorEnabled=true -p 6650:6650 -p 8080:8080 -p 8081:8081 --mount source=pulsarconf,target=/pulsar/conf  apachepulsar/pulsar-all:2.9.0 bash -c "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nfw -nss && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 8 && bin/pulsar-admin namespaces set-retention public/default --time 365000 --size -1 && bin/pulsar-admin topics create-partitioned-topic persistent://public/default/testReadFromPartition --partitions 3 && bin/pulsar-admin topics create-partitioned-topic persistent://public/default/TestReadMessageWithBatchingWithMessageInclusive --partitions 3 && bin/pulsar-admin topics create-partitioned-topic persistent://public/default/TestReadMessageWithoutBatchingWithMessageInclusive --partitions 3 && bin/pulsar-admin topics create-partitioned-topic persistent://public/default/TestReadMessageWithBatching --partitions 3 && bin/pulsar-admin topics create-partitioned-topic persistent://public/default/ReadMessageWithoutBatching --partitions 3"

    //docker run --name pulsar_local -it --env PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true --env PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120 --env PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false -p 6650:6650 -p 8080:8080 -p 8081:8081 --mount source=pulsarconf,target=/pulsar/conf  apachepulsar/pulsar-all:2.9.0 bash -c "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nfw -nss && bin/pulsar-admin namespaces set-retention public/default --time 365000 --size -1 && bin/pulsar-admin topics create-partitioned-topic persistent://public/default/testReadFromPartition --partitions 3 && bin/pulsar-admin topics create-partitioned-topic persistent://public/default/TestReadMessageWithBatchingWithMessageInclusive --partitions 3 && bin/pulsar-admin topics create-partitioned-topic persistent://public/default/TestReadMessageWithoutBatchingWithMessageInclusive --partitions 3 && bin/pulsar-admin topics create-partitioned-topic persistent://public/default/TestReadMessageWithBatching --partitions 3 && bin/pulsar-admin topics create-partitioned-topic persistent://public/default/ReadMessageWithoutBatching --partitions 3"
    //docker exec -it pulsar_local bash -c ""
    //docker exec -it pulsar_local bin/pulsar-admin topics create-partitioned-topic persistent://public/ReadMessageWithBatchingWithMessageInclusive-56 --partitions 3 

    //with log leve: docker run -it --env PULSAR_PREFIX_enableExposingBrokerEntryMetadataToClient=true --env PULSAR_LOG_LEVEL=debug --env PULSAR_LOG_ROOT_LEVEL=debug -p 6650:6650 -p 8080:8080 --mount source=pulsardata,target=/pulsar/data --mount source=pulsarconf,target=/pulsar/conf apachepulsar/pulsar-all:2.9.0 bin/pulsar standalone

    //docker run -it --env PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled=true --env PULSAR_PREFIX_brokerEntryMetadataInterceptors=org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor -p 6650:6650 -p 8080:8080 --mount source=pulsardata,target=/pulsar/data --mount source=pulsarconf,target=/pulsar/conf apachepulsar/pulsar-all:2.9.0 bash -c "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone"
    //docker run -it --env PULSAR_PREFIX_brokerEntryMetadataInterceptors=org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor -p 6650:6650 -p 8080:8080 --mount source=pulsarconf,target=/pulsar/conf apachepulsar/pulsar-all:2.9.0 bash -c "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone"
    public class PulsarStandaloneClusterFixture : PulsarFixture
    {
        public PulsarClient Client;
        public PulsarSystem PulsarSystem;
        public ClientConfigurationData ClientConfigurationData;
        public IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
        public override PulsarTestcontainer BuildContainer()
        {
            return new TestcontainersBuilder<PulsarTestcontainer>()
               .WithPulsar(Configuration)
               .WithName($"test-core")
               .WithPortBinding(6650)
               .WithPortBinding(8080)
               .WithPortBinding(8081)
               .WithExposedPort(6650)
               .WithExposedPort(8080)
               .WithExposedPort(8081)
               .WithCleanUp(true)
               .Build();
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
            var system = await PulsarSystem.GetInstanceAsync(client, actorSystemName: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
            Client = system.NewClient();
            PulsarSystem = system;
            ClientConfigurationData = client.ClientConfigurationData;
        }
    }
}
