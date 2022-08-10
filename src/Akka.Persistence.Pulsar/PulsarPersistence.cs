#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PulsarPersistence.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using Akka.Actor;

namespace Akka.Persistence.Pulsar
{
    public sealed class PulsarPersistence : IExtension
    {
        private readonly ExtendedActorSystem system;
        public PulsarSettings JournalSettings { get; }

        public PulsarPersistence(ExtendedActorSystem system, PulsarSettings journalSettings)
        {
            this.system = system;
            this.JournalSettings = journalSettings;
        }

        public static PulsarPersistence Get(ActorSystem system) => 
            system.WithExtension<PulsarPersistence, PulsarPersistenceProvider>();
    }

    public sealed class PulsarPersistenceProvider : ExtensionIdProvider<PulsarPersistence>
    {
        public override PulsarPersistence CreateExtension(ExtendedActorSystem system)
        {
            var journalSettings = new PulsarSettings(system.Settings.Config.GetConfig("akka.persistence.journal.pulsar"));
            return new PulsarPersistence(system, journalSettings);
        }
    }
}