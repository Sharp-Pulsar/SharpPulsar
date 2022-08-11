#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PulsarReadJournal.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.Query;

namespace Akka.Persistence.Pulsar.Query
{
    public sealed class PulsarReadJournalProvider : IReadJournalProvider
    {
        private readonly ExtendedActorSystem _system;
        private readonly Config _config;

        public PulsarReadJournalProvider(ExtendedActorSystem system, Config config)
        {
            _system = system;
            _config = config;
        }

        public IReadJournal GetReadJournal() => new PulsarReadJournal(_system, _config);
    }
}