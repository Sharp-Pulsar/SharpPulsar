#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PulsarCurrentEventsByTagSpec.cs" company="Akka.NET Project">
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
using Akka.Streams;
using Akka.Streams.Actors;
using Akka.Streams.Dsl;
using Akka.Streams.TestKit;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.Pulsar.Tests
{
    public class PulsarCurrentEventsByTagSpec : CurrentEventsByTagSpec
    {
        private static readonly Config SpecConfig = ConfigurationFactory.ParseString(@"
            akka.persistence.journal.plugin = ""akka.persistence.journal.pulsar""
			akka.persistence.journal.pulsar.event-adapters = {utc-tagger = ""Sample.Event.EventTagger, Sample""}
			akka.persistence.journal.pulsar.event-adapter-bindings = {  ""System.String, System.Private.CoreLib"" = utc-tagger}
            akka.test.single-expect-default = 60s
        ").WithFallback(PulsarPersistence.DefaultConfiguration());
        private Prober _receiverProbe;
        private long _timeout = 30_000;
        public PulsarCurrentEventsByTagSpec(ITestOutputHelper output) : base(SpecConfig, output: output)
        {
            ReadJournal = Sys.ReadJournalFor<PulsarReadJournal>(PulsarReadJournal.Identifier);
            _receiverProbe = new Prober(Sys);
        }
        [Fact]
        public override void ReadJournal_query_CurrentEventsByTag_should_find_existing_events()
        {
            var persistenceId = Guid.NewGuid().ToString();
            var queries = ReadJournal as ICurrentEventsByTagQuery;
            var a = Sys.ActorOf(ProberTestActor.Prop($"event-a-{persistenceId}")); 
            var b = Sys.ActorOf(ProberTestActor.Prop($"event-b-{persistenceId}")); 

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

            var greenSrc = queries.CurrentEventsByTag("green", offset: new Sequence(1L));
            var probe = greenSrc.RunWith(Sink.ActorSubscriber<EventEnvelope>(ManualSubscriber.Props(_receiverProbe.Ref)), Materializer);
            probe.Tell("ready-2"); //requesting 2

            _receiverProbe.ExpectMessage<EventEnvelope>(_timeout, p => p.PersistenceId == $"event-a-{persistenceId}" && p.SequenceNr == 2L && p.Event.Equals("a green apple"));
            _receiverProbe.ExpectMessage<EventEnvelope>(_timeout,p => p.PersistenceId == $"event-a-{persistenceId}" && p.SequenceNr == 4L && p.Event.Equals("a green banana"));
            ExpectNoMsg(TimeSpan.FromMilliseconds(500));
            probe.Tell("ready-2"); //requesting 2
            _receiverProbe.ExpectMessage<EventEnvelope>(_timeout,p => p.PersistenceId == $"event-b-{persistenceId}" && p.SequenceNr == 2L && p.Event.Equals("a green leaf"));
            _receiverProbe.ExpectMessage<OnComplete>(_timeout);

            var blackSrc = queries.CurrentEventsByTag("black", offset: NoOffset.Instance);
            var probe2 = blackSrc.RunWith(Sink.ActorSubscriber<EventEnvelope>(ManualSubscriber.Props(_receiverProbe.Ref)), Materializer);
            probe2.Tell("ready-5");
            _receiverProbe.ExpectMessage<EventEnvelope>(_timeout, p => p.PersistenceId == $"event-b-{persistenceId}" && p.SequenceNr == 1L && p.Event.Equals("a black car"));
            _receiverProbe.ExpectMessage<OnComplete>(_timeout);

            var appleSrc = queries.CurrentEventsByTag("apple", offset: NoOffset.Instance);
            var probe3 = appleSrc.RunWith(Sink.ActorSubscriber<EventEnvelope>(ManualSubscriber.Props(_receiverProbe.Ref)), Materializer);
            probe3.Tell("ready-5");
            _receiverProbe.ExpectMessage<EventEnvelope>(_timeout, p => p.PersistenceId == $"event-a-{persistenceId}" && p.SequenceNr == 2L && p.Event.Equals("a green apple"));
            _receiverProbe.ExpectMessage<OnComplete>(_timeout);
        }

        [Fact]
        public override void ReadJournal_query_CurrentEventsByTag_should_complete_when_no_events()
        {
            var queries = ReadJournal as ICurrentEventsByTagQuery;
            var a = Sys.ActorOf(Kits.ProberTestActor.Prop("a"));
            var b = Sys.ActorOf(Kits.ProberTestActor.Prop("b"));

            a.Tell("a green apple");
            ExpectMsg("a green apple-done");
            b.Tell("a black car");
            ExpectMsg("a black car-done");

            var greenSrc = queries.CurrentEventsByTag("pink", offset: NoOffset.Instance);
            var probe = greenSrc.RunWith(this.SinkProbe<EventEnvelope>(), Materializer);
            probe.Request(2).ExpectComplete();
        }

        // Unit test failed because of time-out but passed when the delays were raised to 300
        [Fact]
        public override void ReadJournal_query_CurrentEventsByTag_should_not_see_new_events_after_complete()
        {
            var queries = ReadJournal as ICurrentEventsByTagQuery;
            ReadJournal_query_CurrentEventsByTag_should_find_existing_events();

            var c = Sys.ActorOf(Kits.ProberTestActor.Prop("c"));

            var greenSrc = queries.CurrentEventsByTag("green", offset: NoOffset.Instance);
            var probe = greenSrc.RunWith(this.SinkProbe<EventEnvelope>(), Materializer);
            probe.Request(2);
            probe.ExpectNext<EventEnvelope>(p => p.PersistenceId == "a" && p.SequenceNr == 2L && p.Event.Equals("a green apple"));
            probe.ExpectNext<EventEnvelope>(p => p.PersistenceId == "a" && p.SequenceNr == 4L && p.Event.Equals("a green banana"));
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(100));

            c.Tell("a green cucumber");
            ExpectMsg("a green cucumber-done");

            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(100));
            probe.Request(5);
            probe.ExpectNext<EventEnvelope>(p => p.PersistenceId == "b" && p.SequenceNr == 2L && p.Event.Equals("a green leaf"));
            probe.ExpectComplete(); // green cucumber not seen
        }

        [Fact]
        public override void ReadJournal_query_CurrentEventsByTag_should_find_events_from_offset_exclusive()
        {
            var queries = ReadJournal as ICurrentEventsByTagQuery;

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

            var greenSrc1 = queries.CurrentEventsByTag("green", offset: NoOffset.Instance);
            var probe1 = greenSrc1.RunWith(this.SinkProbe<EventEnvelope>(), Materializer);
            probe1.Request(2);
            probe1.ExpectNext<EventEnvelope>(p => p.PersistenceId == "a" && p.SequenceNr == 2L && p.Event.Equals("a green apple"));
            var offs = probe1.ExpectNext<EventEnvelope>(p => p.PersistenceId == "a" && p.SequenceNr == 4L && p.Event.Equals("a green banana")).Offset;
            probe1.Cancel();

            var greenSrc = queries.CurrentEventsByTag("green", offset: offs);
            var probe2 = greenSrc.RunWith(this.SinkProbe<EventEnvelope>(), Materializer);
            probe2.Request(10);
            // note that banana is not included, since exclusive offset
            probe2.ExpectNext<EventEnvelope>(p => p.PersistenceId == "b" && p.SequenceNr == 2L && p.Event.Equals("a green leaf"));
            probe2.Cancel();
        }

        [Fact]
        public override void ReadJournal_query_CurrentEventsByTag_should_see_all_150_events()
        {
            var queries = ReadJournal as ICurrentEventsByTagQuery;
            var a = Sys.ActorOf(Kits.ProberTestActor.Prop("a"));

            for (int i = 0; i < 150; ++i)
            {
                a.Tell("a green apple");
                ExpectMsg("a green apple-done");
            }

            var greenSrc = queries.CurrentEventsByTag("green", offset: NoOffset.Instance);
            var probe = greenSrc.RunWith(this.SinkProbe<EventEnvelope>(), Materializer);
            probe.Request(150);
            for (int i = 0; i < 150; ++i)
            {
                probe.ExpectNext<EventEnvelope>(p =>
                    p.PersistenceId == "a" && p.SequenceNr == (i + 1) && p.Event.Equals("a green apple"));
            }

            probe.ExpectComplete();
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));
        }
    }
}