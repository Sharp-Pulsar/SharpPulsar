#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PulsarEventsByTagSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.Pulsar.Query;
using Akka.Persistence.Query;
using Akka.Persistence.TCK.Query;
using Akka.Streams.TestKit;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.Pulsar.Tests
{
    public class PulsarEventsByTagSpec : EventsByTagSpec
    {
        private static readonly Config SpecConfig = ConfigurationFactory.ParseString(@"
            akka.persistence.journal.plugin = ""akka.persistence.journal.pulsar""
            akka.test.single-expect-default = 60s
        ").WithFallback(PulsarPersistence.DefaultConfiguration());
        public PulsarEventsByTagSpec(ITestOutputHelper output) : base(SpecConfig, output: output)
        {
            ReadJournal = Sys.ReadJournalFor<PulsarReadJournal>(PulsarReadJournal.Identifier);
        }

        [Fact]
        public override void ReadJournal_live_query_EventsByTag_should_find_new_events()
        {
            var queries = ReadJournal as IEventsByTagQuery;

            var b = Sys.ActorOf(Kits.ProberTestActor.Prop("b"));
            var d = Sys.ActorOf(Kits.ProberTestActor.Prop("d"));

            b.Tell("a black car");
            ExpectMsg("a black car-done");

            var blackSrc = queries.EventsByTag("black", offset: NoOffset.Instance);
            var probe = blackSrc.RunWith(this.SinkProbe<EventEnvelope>(), Materializer);
            probe.Request(2);
            probe.ExpectNext<EventEnvelope>(p => p.PersistenceId == "b" && p.SequenceNr == 1L && p.Event.Equals("a black car"));
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(100));

            d.Tell("a black dog");
            ExpectMsg("a black dog-done");
            d.Tell("a black night");
            ExpectMsg("a black night-done");

            probe.ExpectNext<EventEnvelope>(p => p.PersistenceId == "d" && p.SequenceNr == 1L && p.Event.Equals("a black dog"));
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(100));
            probe.Request(10);
            probe.ExpectNext<EventEnvelope>(p => p.PersistenceId == "d" && p.SequenceNr == 2L && p.Event.Equals("a black night"));
            probe.Cancel();
        }

        [Fact]
        public override void ReadJournal_live_query_EventsByTag_should_find_events_from_offset_exclusive()
        {
            var queries = ReadJournal as IEventsByTagQuery;

            var a = Sys.ActorOf(Kits.ProberTestActor.Prop("a"));
            var b = Sys.ActorOf(Kits.ProberTestActor.Prop("b"));
            var c = Sys.ActorOf(Kits.ProberTestActor.Prop("c"));

            a.Tell("hello");
            ExpectMsg("hello-done");
            a.Tell("a green apple");
            ExpectMsg("a green apple-done");
            b.Tell("a black car");
            ExpectMsg("a black car-done");
            a.Tell("something else");
            ExpectMsg("something else-done");
            a.Tell("a green banana");
            ExpectMsg("a green banana-done");
            b.Tell("a green leaf");
            ExpectMsg("a green leaf-done");
            c.Tell("a green cucumber");
            ExpectMsg("a green cucumber-done");

            var greenSrc1 = queries.EventsByTag("green", offset: NoOffset.Instance);
            var probe1 = greenSrc1.RunWith(this.SinkProbe<EventEnvelope>(), Materializer);
            probe1.Request(2);
            probe1.ExpectNext<EventEnvelope>(p => p.PersistenceId == "a" && p.SequenceNr == 2L && p.Event.Equals("a green apple"));
            var offs = probe1.ExpectNext<EventEnvelope>(p => p.PersistenceId == "a" && p.SequenceNr == 4L && p.Event.Equals("a green banana")).Offset;
            probe1.Cancel();

            var greenSrc2 = queries.EventsByTag("green", offset: offs);
            var probe2 = greenSrc2.RunWith(this.SinkProbe<EventEnvelope>(), Materializer);
            probe2.Request(10);
            probe2.ExpectNext<EventEnvelope>(p => p.PersistenceId == "b" && p.SequenceNr == 2L && p.Event.Equals("a green leaf"));
            probe2.ExpectNext<EventEnvelope>(p => p.PersistenceId == "c" && p.SequenceNr == 1L && p.Event.Equals("a green cucumber"));
            probe2.ExpectNoMsg(TimeSpan.FromMilliseconds(100));
            probe2.Cancel();
        }
    }
}