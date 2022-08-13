#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PulsarPersistence.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion


using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.Pulsar.Query;

namespace Akka.Persistence.Pulsar
{
    //https://github.com/eaba/SharpPulsar/blob/b8bd327643f2fd30eb29696656b10e1b419f2da6/Tests/SharpPulsar.Test/Fixtures/PulsarStandaloneClusterFixture.cs
    //docker run --name pulsar_local -it --env PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true --env PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120 --env PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false --env PULSAR_PREFIX_transactionCoordinatorEnabled=true --env PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled=true --env PULSAR_PREFIX_brokerEntryMetadataInterceptors=org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor -p 6650:6650 -p 8080:8080 -p 8081:8081 apachepulsar/pulsar-all:2.10.1 bash -c "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nfw -nss && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2 && bin/pulsar-admin namespaces set-retention public/default --time -1 --size -1"
    //docker exec -it pulsar_local bash -c ""
    //docker exec -it pulsar_local bin/pulsar-admin topics create-partitioned-topic persistent://public/ReadMessageWithBatchingWithMessageInclusive-56 --partitions 3 

    public sealed class PulsarPersistence : IExtension
    {
        private readonly ExtendedActorSystem _system;
        
        /// <summary>
        /// Returns a default query configuration for akka persistence SQLite-based journals and snapshot stores.
        /// </summary>
        /// <returns></returns>
        public static Config DefaultConfiguration()
        {
            var config = ConfigurationFactory.FromResource<PulsarReadJournal>("Akka.Persistence.Pulsar.reference.conf");
            return config;
        }
        
        public PulsarPersistence(ExtendedActorSystem system)
        {
            _system = system;
        }

        public static PulsarPersistence Get(ActorSystem system) => 
            system.WithExtension<PulsarPersistence, PulsarPersistenceProvider>();
    }

    public sealed class PulsarPersistenceProvider : ExtensionIdProvider<PulsarPersistence>
    {
        public override PulsarPersistence CreateExtension(ExtendedActorSystem system)
        {
            return new PulsarPersistence(system);
        }
    }
}