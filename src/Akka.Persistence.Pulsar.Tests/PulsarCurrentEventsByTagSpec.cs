#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PulsarCurrentEventsByTagSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using Akka.Configuration;
using Akka.Persistence.Query;
using Akka.Persistence.TCK.Query;
using Xunit.Abstractions;

namespace Akka.Persistence.Pulsar.Tests
{
    public class PulsarCurrentEventsByTagSpec : CurrentEventsByTagSpec
    {
        private static readonly Config SpecConfig = ConfigurationFactory.ParseString(@"
            akka.persistence.journal.plugin = ""akka.persistence.journal.pulsar""
            akka.test.single-expect-default = 10s
        ");
        
        public PulsarCurrentEventsByTagSpec(ITestOutputHelper output) : base(SpecConfig, output: output)
        {
            ReadJournal = Sys.ReadJournalFor<PulsarReadJournal>(PulsarReadJournal.Identifier);
        }
    }
}