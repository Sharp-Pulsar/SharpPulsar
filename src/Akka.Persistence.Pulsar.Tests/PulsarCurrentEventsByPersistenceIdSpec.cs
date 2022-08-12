#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PulsarCurrentEventsByPersistenceIdSpec.cs" company="Akka.NET Project">
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
using Akka.Streams;
using Akka.Streams.Actors;
using Akka.Streams.Dsl;
using Akka.Streams.TestKit;
using Akka.Util.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.Pulsar.Tests
{
    public class PulsarCurrentEventsByPersistenceIdSpec : Akka.TestKit.Xunit2.TestKit
    {
        private static readonly Config SpecConfig = ConfigurationFactory.ParseString(@"
            akka.persistence.journal.plugin = ""akka.persistence.journal.pulsar""
            akka.test.single-expect-default = 60s
        ").WithFallback(PulsarPersistence.DefaultConfiguration());

        protected ActorMaterializer Materializer { get; }

        protected IReadJournal ReadJournal { get; set; }
        private Prober _receiverProbe;
        private long _timeout = 30_000;

        public PulsarCurrentEventsByPersistenceIdSpec(ITestOutputHelper output) : base(SpecConfig,
            "PulsarCurrentEventsByPersistenceIdSpec", output)
        {
            Materializer = Sys.Materializer();
            ReadJournal = Sys.ReadJournalFor<PulsarReadJournal>(PulsarReadJournal.Identifier);
            _receiverProbe = new Prober(Sys);
        }

        [Fact]
        public void ReadJournal_should_implement_ICurrentEventsByPersistenceIdQuery()
        {
            Assert.IsAssignableFrom<ICurrentEventsByPersistenceIdQuery>(ReadJournal);
        }

        [Fact]
        public void ReadJournal_CurrentEventsByPersistenceId_should_find_existing_events()
        {
            var persistenceId = Guid.NewGuid().ToString();
            var queries = ReadJournal.AsInstanceOf<ICurrentEventsByPersistenceIdQuery>();
            var pref = Setup(persistenceId);

            var src = queries.CurrentEventsByPersistenceId(persistenceId, 1, 3);
            var probe = src.Select(x => x.Event).RunWith(Sink.ActorSubscriber<object>(ManualSubscriber.Props(_receiverProbe.Ref)), Sys.Materializer()); 
            
            ExpectNoMsg(200);
            probe.Tell("ready-3"); //requesting 2
            _receiverProbe.ExpectMessage($"{persistenceId}-1", _timeout);
            _receiverProbe.ExpectMessage($"{persistenceId}-2", _timeout);
            _receiverProbe.ExpectMessage($"{persistenceId}-3", _timeout);
            _receiverProbe.ExpectMessage<OnComplete>(_timeout);
        }

        [Fact]
        public void
            ReadJournal_CurrentEventsByPersistenceId_should_find_existing_events_up_to_a_sequence_number()
        {
            var persistenceId = Guid.NewGuid().ToString();
            var queries = ReadJournal.AsInstanceOf<ICurrentEventsByPersistenceIdQuery>();
            var pref = Setup(persistenceId);

            var src = queries.CurrentEventsByPersistenceId(persistenceId, 1, 3L);
            var probe = src.Select(x => x.Event).RunWith(Sink.ActorSubscriber<object>(ManualSubscriber.Props(_receiverProbe.Ref)), Sys.Materializer());

            ExpectNoMsg(200);
            probe.Tell("ready-2"); //requesting 2
            _receiverProbe.ExpectMessage($"{persistenceId}-1", _timeout);
            _receiverProbe.ExpectMessage($"{persistenceId}-2", _timeout);
            _receiverProbe.ExpectMessage<OnComplete>(_timeout);
        }

        [Fact]
        public void ReadJournal_CurrentEventsByPersistenceId_should_Replay_8()
        {
            var persistenceId = Guid.NewGuid().ToString();
            var queries = ReadJournal.AsInstanceOf<ICurrentEventsByPersistenceIdQuery>();
            var pref = Setup(persistenceId);

            var src = queries.CurrentEventsByPersistenceId(persistenceId, 1, long.MaxValue);
            var probe = src.Select(x => x.Event).RunWith(Sink.ActorSubscriber<object>(ManualSubscriber.Props(_receiverProbe.Ref)), Sys.Materializer());

            pref.Tell($"{persistenceId}-6");
            pref.Tell($"{persistenceId}-7");
            pref.Tell($"{persistenceId}-8");
            ExpectMsg($"{persistenceId}-6-done");
            ExpectMsg($"{persistenceId}-7-done");
            ExpectMsg($"{persistenceId}-8-done");
            ExpectNoMsg(200);
            probe.Tell("ready-8"); //requesting 2
            _receiverProbe.ExpectMessage($"{persistenceId}-1", _timeout);
            _receiverProbe.ExpectMessage($"{persistenceId}-2", _timeout);
            _receiverProbe.ExpectMessage($"{persistenceId}-3", _timeout);
            _receiverProbe.ExpectMessage($"{persistenceId}-4", _timeout);
            _receiverProbe.ExpectMessage($"{persistenceId}-5", _timeout);
            _receiverProbe.ExpectMessage($"{persistenceId}-6", _timeout);
            _receiverProbe.ExpectMessage($"{persistenceId}-7", _timeout);
            _receiverProbe.ExpectMessage<OnComplete>(_timeout);
        }

        private IActorRef Setup(string persistenceId)
        {
            var pref = SetupEmpty(persistenceId);

            pref.Tell(persistenceId + "-1", _receiverProbe.Ref);
            pref.Tell(persistenceId + "-2", _receiverProbe.Ref);
            pref.Tell(persistenceId + "-3", _receiverProbe.Ref);
            pref.Tell(persistenceId + "-4", _receiverProbe.Ref);
            pref.Tell(persistenceId + "-5", _receiverProbe.Ref);

            _receiverProbe.ExpectMessage(persistenceId + "-1-done", _timeout);
            _receiverProbe.ExpectMessage(persistenceId + "-2-done", _timeout);
            _receiverProbe.ExpectMessage(persistenceId + "-3-done", _timeout);
            _receiverProbe.ExpectMessage(persistenceId + "-4-done", _timeout);
            _receiverProbe.ExpectMessage(persistenceId + "-5-done", _timeout);
            return pref;
        }

        private IActorRef SetupEmpty(string persistenceId)
        {
            return Sys.ActorOf(ProberTestActor.Prop(persistenceId));
        }

        protected override void Dispose(bool disposing)
        {
            Materializer.Dispose();
            base.Dispose(disposing);
        }
    }
}