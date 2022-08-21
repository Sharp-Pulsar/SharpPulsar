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
//docker run --name pulsar_local -it --env PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true --env PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120 --env PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false -p 6650:6650 -p 8080:8080 -p 8081:8081 apachepulsar/pulsar-all:2.10.1 bash -c "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nfw -nss && bin/pulsar-admin namespaces set-retention public/default --time 365000 --size -1 "
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