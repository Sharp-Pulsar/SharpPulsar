/*
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

namespace SharpPulsar.Test.Integration.Fixtures
{
    using DotNet.Testcontainers.Builders;
    using Microsoft.Extensions.Configuration;
    using SharpPulsar.Configuration;
    using SharpPulsar.Test.EventSourcing;
    using SharpPulsar.TestContainer;
    using SharpPulsar.User;
    using System.IO;
    using System.Reflection;
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
    public class PulsarIntegrationFixture : PulsarFixture
    {
        public PulsarClient Client;
        public PulsarSystem PulsarSystem;
        public ClientConfigurationData ClientConfigurationData;
        private readonly IntegrationContainerConfiguration _configuration;

        public PulsarIntegrationFixture()
        {
            _configuration = new IntegrationContainerConfiguration("apachepulsar/pulsar-all:2.9.1", 6650);
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
               .WithName($"test-core")
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
            var system = await PulsarSystem.GetInstanceAsync(client, actorSystemName: "pulsar-integration-test");
            Client = system.NewClient();
            PulsarSystem = system;
            ClientConfigurationData = client.ClientConfigurationData;
        }
    }
}
