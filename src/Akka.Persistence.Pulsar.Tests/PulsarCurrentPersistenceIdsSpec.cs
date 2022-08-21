#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PulsarCurrentPersistenceIdsSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.Pulsar.Query;
using Akka.Persistence.Pulsar.Tests.Kits;
using Akka.Persistence.Query;
using Akka.Persistence.TCK.Query;
using Akka.Streams.TestKit;
using Akka.Util.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.Pulsar.Tests
{
    public class PulsarCurrentPersistenceIdsSpec : CurrentPersistenceIdsSpec
    {
        private static readonly Config SpecConfig = ConfigurationFactory.ParseString(@"
            akka.persistence.journal.plugin = ""akka.persistence.journal.pulsar""
            akka.test.single-expect-default = 60s
        ").WithFallback(PulsarPersistence.DefaultConfiguration());
        public PulsarCurrentPersistenceIdsSpec(ITestOutputHelper output) : base(SpecConfig, output: output)
        {
            ReadJournal = Sys.ReadJournalFor<PulsarReadJournal>(PulsarReadJournal.Identifier);
        }

        [Fact]
        public override void ReadJournal_CurrentPersistenceIds_should_find_existing_events()
        {
            var queries = ReadJournal.AsInstanceOf<ICurrentPersistenceIdsQuery>();

            Setup("a", 1);
            Setup("b", 1);
            Setup("c", 1);

            var source = queries.CurrentPersistenceIds();
            var probe = source.RunWith(this.SinkProbe<string>(), Materializer);
            probe.Within(TimeSpan.FromSeconds(10), () =>
                probe.Request(4)
                    .ExpectNextUnordered("a", "b", "c")
                    .ExpectComplete());
        }

        [Fact]
        public override void ReadJournal_CurrentPersistenceIds_should_deliver_persistenceId_only_once()
        {
            var queries = ReadJournal.AsInstanceOf<ICurrentPersistenceIdsQuery>();

            Setup("d", 100);

            var source = queries.CurrentPersistenceIds();
            var probe = source.RunWith(this.SinkProbe<string>(), Materializer);
            probe.Within(TimeSpan.FromSeconds(10), () =>
                probe.Request(10)
                    .ExpectNext("d")
                    .ExpectComplete());
        }

        [Fact]
        public override void ReadJournal_query_CurrentPersistenceIds_should_not_see_new_events_after_complete()
        {
            var queries = ReadJournal.AsInstanceOf<ICurrentPersistenceIdsQuery>();

            Setup("a", 1);
            Setup("b", 1);
            Setup("c", 1);

            var greenSrc = queries.CurrentPersistenceIds();
            var probe = greenSrc.RunWith(this.SinkProbe<string>(), Materializer);
            probe.Request(2)
                .ExpectNext("a")
                .ExpectNext("c")
                .ExpectNoMsg(TimeSpan.FromMilliseconds(100));

            Setup("d", 1);

            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(100));
            probe.Request(5)
                .ExpectNext("b")
                .ExpectComplete();
        }

        private IActorRef Setup(string persistenceId, int n)
        {
            var pref = Sys.ActorOf(ProberTestActor.Prop(persistenceId));
            for (int i = 1; i <= n; i++)
            {
                pref.Tell($"{persistenceId}-{i}");
                ExpectMsg($"{persistenceId}-{i}-done");
            }

            return pref;
        }

        protected override void Dispose(bool disposing)
        {
            Materializer.Dispose();
            base.Dispose(disposing);
        }
    }
}