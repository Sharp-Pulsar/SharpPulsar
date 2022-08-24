
//docker run --name pulsar_local -it --env PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true --env PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120 --env PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false -p 6650:6650 -p 8080:8080 -p 8081:8081 apachepulsar/pulsar-all:2.10.1 bash -c "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nfw -nss && bin/pulsar-admin namespaces set-retention public/default --time 365000 --size -1 "

#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PlusarJournalSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.Pulsar.Tests.Kits;
using Akka.Persistence.TCK;
using Akka.Persistence.TCK.Journal;
using Akka.Persistence.TCK.Serialization;
using Akka.TestKit;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.Pulsar.Tests
{
    [Collection("PlusarJournalSpec")]
    public class PlusarJournalSpec : PluginSpec
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
        protected static readonly Config Config =
            ConfigurationFactory.ParseString(@"akka.persistence.publish-plugin-commands = on
            akka.actor{
                serializers{
                    persistence-tck-test=""Akka.Persistence.TCK.Serialization.TestSerializer,Akka.Persistence.TCK""
                }
                serialization-bindings {
                    ""Akka.Persistence.TCK.Serialization.TestPayload,Akka.Persistence.TCK"" = persistence-tck-test
                }
            }");

        private static readonly string _specConfigTemplate = @"
            akka.persistence {{
                publish-plugin-commands = on
                journal {{
                    plugin = ""akka.persistence.journal.testspec""
                    testspec {{
                        class = ""{0}""
                        plugin-dispatcher = ""akka.actor.default-dispatcher""
                    }}
                }}
            }}
        ";

        private TestProbe _senderProbe;
        private Prober _receiverProbe;
        private string _pid;
        private long _timeout;
        protected override bool SupportsSerialization => true;

        /// <summary>
        /// Initializes a journal with set o predefined messages.
        /// </summary>
        protected IEnumerable<AtomicWrite> Initialize()
        {
            _timeout = 180000;
            _pid = Guid.NewGuid().ToString();
            _senderProbe = CreateTestProbe();
            _receiverProbe = new Prober(Sys);
            PreparePersistenceId(_pid);
            return WriteMessages(1, 6, _pid, _senderProbe.Ref, WriterGuid);
        }

        /// <summary>
        /// Overridable hook that is called before populating the journal for the next test case.
        /// <paramref name="pid"/> is the persistenceId that will be used in the test.
        /// This method may be needed to clean pre-existing events from the log.
        /// </summary>
        /// <param name="pid"></param>
        protected virtual void PreparePersistenceId(string pid)
        {
        }

        /// <summary>
        /// Implementation may override and return false if it does not support
        /// atomic writes of several events, as emitted by
        /// <see cref="Eventsourced.PersistAll{TEvent}(IEnumerable{TEvent},Action{TEvent})"/>.
        /// </summary>
        protected virtual bool SupportsAtomicPersistAllOfSeveralEvents { get { return true; } }

        /// <summary>
        /// When true enables tests which check if the Journal properly rejects
        /// writes of objects which are not serializable.
        /// </summary>
        protected virtual bool SupportsRejectingNonSerializableObjects { get { return true; } }

        protected IActorRef Journal { get { return Extension.JournalFor(null); } }

        private static Config ConfigFromTemplate(Type journalType)
        {
            var config = string.Format(_specConfigTemplate, journalType.AssemblyQualifiedName);
            return ConfigurationFactory.ParseString(config);
        }

        protected bool IsReplayedMessage(ReplayedMessage message, long seqNr, bool isDeleted = false)
        {
            var p = message.Persistent;
            return p.IsDeleted == isDeleted
                   //&& p.Payload.ToString() == "a-" + seqNr
                   && p.PersistenceId == _pid
                   && p.SequenceNr >= seqNr;
        }

        private AtomicWrite[] WriteMessages(int from, int to, string pid, IActorRef sender, string writerGuid)
        {
            Func<long, Persistent> persistent = i => new Persistent("a-" + i, i, pid, string.Empty, false, sender, writerGuid);
            var messages = (SupportsAtomicPersistAllOfSeveralEvents
                ? Enumerable.Range(from, to - 1)
                    .Select(
                        i =>
                            i == to - 1
                                ? new AtomicWrite(
                                    new[] { persistent(i), persistent(i + 1) }.ToImmutableList<IPersistentRepresentation>())
                                : new AtomicWrite(persistent(i)))
                : Enumerable.Range(from, to).Select(i => new AtomicWrite(persistent(i))))
                .ToArray();
            var probe = CreateTestProbe();

            Journal.Tell(new WriteMessages(messages, probe.Ref, ActorInstanceId));
            probe.ExpectMsg<WriteMessagesSuccessful>();
            for (int i = from; i <= to; i++)
            {
                var n = i;
                probe.ExpectMsg<WriteMessageSuccess>(m =>
                        m.Persistent.Payload.ToString() == ("a-" + n) && m.Persistent.SequenceNr == (long)n &&
                        m.Persistent.PersistenceId == _pid);
            }

            return messages;
        }

        [Fact]
        public void Journal_should_replay_all_messages()
        {
            Journal.Tell(new ReplayMessages(1, long.MaxValue, long.MaxValue, _pid, _receiverProbe.Ref));
            for (int i = 1; i <= 5; i++)
                _receiverProbe.ExpectMessage<ReplayedMessage>(_timeout, m => IsReplayedMessage(m, i));
            _receiverProbe.ExpectMessage<RecoverySuccess>(_timeout);
        }

        [Fact]
        public void Journal_should_not_replay_messages_if_count_limit_equals_zero()
        {
            Journal.Tell(new ReplayMessages(2, 4, 0, _pid, _receiverProbe.Ref));
            _receiverProbe.ExpectMessage<RecoverySuccess>(_timeout);
        }

        [Fact]
        public void Journal_should_not_replay_messages_if_lower_sequence_number_bound_is_greater_than_upper_sequence_number_bound()
        {
            Journal.Tell(new ReplayMessages(3, 2, long.MaxValue, _pid, _receiverProbe.Ref));
            _receiverProbe.ExpectMessage<RecoverySuccess>(_timeout);
        }
        [Fact]
        public void Journal_should_serialize_events()
        {
            if (!SupportsSerialization) return;
            var @event = new TestPayload(_receiverProbe.Ref);

            var pid = _pid;
            var writerGuid = WriterGuid;

            var aw = new AtomicWrite(
                new Persistent(@event, 7L, _pid, sender: ActorRefs.NoSender, writerGuid: WriterGuid));
            Journal.Tell(new WriteMessages(new[] { aw }, _receiverProbe.Ref, ActorInstanceId));
            _receiverProbe.ExpectMessage<WriteMessagesSuccessful>(_timeout);
            _receiverProbe.ExpectMessage<WriteMessageSuccess>(o =>
            {
                Assertions.AssertEqual(writerGuid, o.Persistent.WriterGuid);
                Assertions.AssertEqual(pid, o.Persistent.PersistenceId);
                Assertions.AssertEqual(7L, o.Persistent.SequenceNr);
                Assertions.AssertTrue(o.Persistent.Sender == ActorRefs.NoSender || o.Persistent.Sender.Equals(Sys.DeadLetters), $"Expected WriteMessagesSuccess.Persistent.Sender to be null or {Sys.DeadLetters}, but found {o.Persistent.Sender}");
                Assertions.AssertEqual(@event, o.Persistent.Payload);
            }, _timeout);

            aw = new AtomicWrite(
                new Persistent(@event, 8L, _pid, sender: ActorRefs.NoSender, writerGuid: WriterGuid));
            Journal.Tell(new WriteMessages(new[] { aw }, _receiverProbe.Ref, ActorInstanceId));
            _receiverProbe.ExpectMessage<WriteMessagesSuccessful>(_timeout);
            _receiverProbe.ExpectMessage<WriteMessageSuccess>(o =>
            {
                Assertions.AssertEqual(writerGuid, o.Persistent.WriterGuid);
                Assertions.AssertEqual(pid, o.Persistent.PersistenceId);
                Assertions.AssertEqual(8L, o.Persistent.SequenceNr);
                Assertions.AssertTrue(o.Persistent.Sender == ActorRefs.NoSender || o.Persistent.Sender.Equals(Sys.DeadLetters), $"Expected WriteMessagesSuccess.Persistent.Sender to be null or {Sys.DeadLetters}, but found {o.Persistent.Sender}");
                Assertions.AssertEqual(@event, o.Persistent.Payload);
            }, _timeout);

            aw = new AtomicWrite(
                new Persistent(@event, 9L, _pid, sender: ActorRefs.NoSender, writerGuid: WriterGuid));
            Journal.Tell(new WriteMessages(new[] { aw }, _receiverProbe.Ref, ActorInstanceId));

            _receiverProbe.ExpectMessage<WriteMessagesSuccessful>(_timeout);
            _receiverProbe.ExpectMessage<WriteMessageSuccess>(o =>
            {
                Assertions.AssertEqual(writerGuid, o.Persistent.WriterGuid);
                Assertions.AssertEqual(pid, o.Persistent.PersistenceId);
                Assertions.AssertEqual(9L, o.Persistent.SequenceNr);
                Assertions.AssertTrue(o.Persistent.Sender == ActorRefs.NoSender || o.Persistent.Sender.Equals(Sys.DeadLetters), $"Expected WriteMessagesSuccess.Persistent.Sender to be null or {Sys.DeadLetters}, but found {o.Persistent.Sender}");
                Assertions.AssertEqual(@event, o.Persistent.Payload);
            }, _timeout);

            Journal.Tell(new ReplayMessages(8, 9, long.MaxValue, _pid, _receiverProbe.Ref));

            _receiverProbe.ExpectMessage<ReplayedMessage>(o =>
            {
                Assertions.AssertEqual(writerGuid, o.Persistent.WriterGuid);
                Assertions.AssertEqual(pid, o.Persistent.PersistenceId);
                Assertions.AssertEqual(8L, o.Persistent.SequenceNr);
                Assertions.AssertTrue(o.Persistent.Sender == ActorRefs.NoSender || o.Persistent.Sender.Equals(Sys.DeadLetters), $"Expected WriteMessagesSuccess.Persistent.Sender to be null or {Sys.DeadLetters}, but found {o.Persistent.Sender}");
                Assertions.AssertEqual(@event, o.Persistent.Payload);
            }, _timeout);

            Assertions.AssertEqual(_receiverProbe.ExpectMessage<RecoverySuccess>(_timeout).HighestSequenceNr, 9L);
        }

    }
}