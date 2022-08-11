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

    public sealed class PulsarPersistence : IExtension
    {
        private readonly ExtendedActorSystem _system;
        
        /// <summary>
        /// Returns a default query configuration for akka persistence SQLite-based journals and snapshot stores.
        /// </summary>
        /// <returns></returns>
        public static Config DefaultConfiguration()
        {
            return ConfigurationFactory.FromResource<PulsarReadJournal>("Akka.Persistence.Pulsar.reference.conf");
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