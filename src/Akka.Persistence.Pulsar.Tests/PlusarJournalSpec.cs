#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PlusarJournalSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using Akka.Configuration;
using Akka.Persistence.TCK.Journal;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.Pulsar.Tests
{
    [Collection("PlusarJournalSpec")]
    public class PlusarJournalSpec : JournalSpec
    {
        private static readonly Config SpecConfig = ConfigurationFactory.ParseString(@"
            akka.persistence.journal.plugin = ""akka.persistence.journal.pulsar""
            akka.test.single-expect-default = 180s
        ").WithFallback(PulsarPersistence.DefaultConfiguration());
        public PlusarJournalSpec(ITestOutputHelper output) : base(FromConfig(SpecConfig).WithFallback(Config), "JournalSpec", output)
        {
            PulsarPersistence.Get(Sys);
            Initialize();
        }
    }
}