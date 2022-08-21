using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.Pulsar.Tests.Kits;
using Akka.Persistence.TCK;
using Akka.Persistence.TCK.Serialization;
using Akka.TestKit;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.Pulsar.Tests
{
    public class PulsarSnapshotStoreSpec : PluginSpec
    {
        public static Config SpecConfig = ConfigurationFactory.ParseString(@"
            akka.persistence.snapshot-store.plugin = ""akka.persistence.snapshot-store.pulsar""
            akka.test.single-expect-default = 60s
        ").WithFallback(PulsarPersistence.DefaultConfiguration());
        private static readonly string _specConfigTemplate = @"
            akka.persistence.publish-plugin-commands = on
            akka.persistence.snapshot-store {
                plugin = ""akka.persistence.snapshot-store.my""
                my {
                    class = ""TestPersistencePlugin.MySnapshotStore, TestPersistencePlugin""
                    plugin-dispatcher = ""akka.persistence.dispatchers.default-plugin-dispatcher""
                }
            }
            akka.actor{
                serializers{
                    persistence-tck-test=""Akka.Persistence.TCK.Serialization.TestSerializer,Akka.Persistence.TCK""
                }
                serialization-bindings {
                    ""Akka.Persistence.TCK.Serialization.TestPayload,Akka.Persistence.TCK"" = persistence-tck-test
                }
            }";

        protected static readonly Config Config =
            ConfigurationFactory.ParseString(_specConfigTemplate);

        protected override bool SupportsSerialization => true;

        private readonly Prober _senderProbe;
        protected List<SnapshotMetadata> Metadata;
        private long _timeout = 10_000;
        private string _pid;
        public PulsarSnapshotStoreSpec(ITestOutputHelper output) : base(FromConfig(SpecConfig).WithFallback(Config), output: output)
        {
            _pid = Guid.NewGuid().ToString();
            _senderProbe = new Prober(Sys);
            Initialize();
        }

        protected IActorRef SnapshotStore => Extension.SnapshotStoreFor(null);

        /// <summary>
        /// The limit defines a number of bytes persistence plugin can support to store the snapshot.
        /// If plugin does not support persistence of the snapshots of 10000 bytes or may support more than default size,
        /// the value can be overriden by the SnapshotStoreSpec implementation with a note in a plugin documentation.
        /// </summary>
        protected virtual int SnapshotByteSizeLimit { get; } = 10000;

        /// <summary>
        /// Initializes a snapshot store with set of predefined snapshots.
        /// </summary>
        protected IEnumerable<SnapshotMetadata> Initialize()
        {
            return Metadata = WriteSnapshots().ToList();
        }

        private static Config ConfigFromTemplate(Type snapshotStoreType)
        {
            return ConfigurationFactory.ParseString(string.Format(_specConfigTemplate, snapshotStoreType.FullName));
        }

        private IEnumerable<SnapshotMetadata> WriteSnapshots()
        {
            for (int i = 1; i <= 5; i++)
            {
                var metadata = new SnapshotMetadata(_pid, i + 10);
                SnapshotStore.Tell(new SaveSnapshot(metadata, $"s-{i}"), _senderProbe.Ref);
                yield return _senderProbe.ExpectMessage<SaveSnapshotSuccess>(_timeout).Metadata;
            }
        }

        [Fact]
        public void SnapshotStore_should_not_load_a_snapshot_given_an_invalid_persistence_id()
        {
            SnapshotStore.Tell(new LoadSnapshot("invalid", SnapshotSelectionCriteria.Latest, long.MaxValue), _senderProbe.Ref);
            _senderProbe.ExpectMessage<LoadSnapshotResult>(_timeout, result => result.Snapshot == null && result.ToSequenceNr == long.MaxValue);
        }

        [Fact]
        public void SnapshotStore_should_not_load_a_snapshot_given_non_matching_timestamp_criteria()
        {
            SnapshotStore.Tell(new LoadSnapshot(_pid, new SnapshotSelectionCriteria(long.MaxValue, new DateTime(100000)), long.MaxValue), _senderProbe.Ref);
            _senderProbe.ExpectMessage<LoadSnapshotResult>(_timeout,result => result.Snapshot == null && result.ToSequenceNr == long.MaxValue);
        }

        [Fact]
        public void SnapshotStore_should_not_load_a_snapshot_given_non_matching_sequence_number_criteria()
        {
            SnapshotStore.Tell(new LoadSnapshot(_pid, new SnapshotSelectionCriteria(7), long.MaxValue), _senderProbe.Ref);
            _senderProbe.ExpectMessage<LoadSnapshotResult>(_timeout,result => result.Snapshot == null && result.ToSequenceNr == long.MaxValue);

            SnapshotStore.Tell(new LoadSnapshot(_pid, SnapshotSelectionCriteria.Latest, 7), _senderProbe.Ref);
            _senderProbe.ExpectMessage<LoadSnapshotResult>(_timeout, result => result.Snapshot == null && result.ToSequenceNr == 7);
        }

        [Fact]
        public void SnapshotStore_should_load_the_most_recent_snapshot()
        {
            SnapshotStore.Tell(new LoadSnapshot(_pid, SnapshotSelectionCriteria.Latest, long.MaxValue), _senderProbe.Ref);
            _senderProbe.ExpectMessage<LoadSnapshotResult>(_timeout,result =>
                result.ToSequenceNr == long.MaxValue
                && result.Snapshot != null
                && result.Snapshot.Metadata.Equals(Metadata[4])
                && result.Snapshot.Snapshot.ToString() == "s-5");
        }

        [Fact]
        public void SnapshotStore_should_load_the_most_recent_snapshot_matching_an_upper_sequence_number_bound()
        {
            SnapshotStore.Tell(new LoadSnapshot(_pid, new SnapshotSelectionCriteria(13), long.MaxValue), _senderProbe.Ref);
            _senderProbe.ExpectMessage<LoadSnapshotResult>(_timeout,result =>
                result.ToSequenceNr == long.MaxValue
                && result.Snapshot != null
                && result.Snapshot.Metadata.Equals(Metadata[2])
                && result.Snapshot.Snapshot.ToString() == "s-3");

            SnapshotStore.Tell(new LoadSnapshot(_pid, SnapshotSelectionCriteria.Latest, 13), _senderProbe.Ref);
            _senderProbe.ExpectMessage<LoadSnapshotResult>(_timeout,result =>
                result.ToSequenceNr == 13
                && result.Snapshot != null
                && result.Snapshot.Metadata.Equals(Metadata[2])
                && result.Snapshot.Snapshot.ToString() == "s-3");
        }

        [Fact]
        public void SnapshotStore_should_load_the_most_recent_snapshot_matching_an_upper_sequence_number_and_timestamp_bound()
        {
            SnapshotStore.Tell(new LoadSnapshot(_pid, new SnapshotSelectionCriteria(13, Metadata[2].Timestamp), long.MaxValue), _senderProbe.Ref);
            _senderProbe.ExpectMessage<LoadSnapshotResult>(_timeout,result =>
                result.ToSequenceNr == long.MaxValue
                && result.Snapshot != null
                && result.Snapshot.Metadata.Equals(Metadata[2])
                && result.Snapshot.Snapshot.ToString() == "s-3");

            SnapshotStore.Tell(new LoadSnapshot(_pid, new SnapshotSelectionCriteria(long.MaxValue, Metadata[2].Timestamp), 13), _senderProbe.Ref);
            _senderProbe.ExpectMessage<LoadSnapshotResult>(_timeout,result =>
                result.ToSequenceNr == 13
                && result.Snapshot != null
                && result.Snapshot.Metadata.Equals(Metadata[2])
                && result.Snapshot.Snapshot.ToString() == "s-3");
        }

        [Fact]
        public void SnapshotStore_should_save_and_overwrite_snapshot_with_same_sequence_number()
        {
            var md = Metadata[4];
            SnapshotStore.Tell(new SaveSnapshot(md, "s-5-modified"), _senderProbe.Ref);
            var md2 = _senderProbe.ExpectMessage<SaveSnapshotSuccess>(_timeout).Metadata;
            Assert.Equal(md.SequenceNr, md2.SequenceNr);
            SnapshotStore.Tell(new LoadSnapshot(_pid, new SnapshotSelectionCriteria(md.SequenceNr), long.MaxValue), _senderProbe.Ref);
            var result = _senderProbe.ExpectMessage<LoadSnapshotResult>(_timeout);
            Assert.Equal("s-5-modified", result.Snapshot.Snapshot.ToString());
            Assert.Equal(md.SequenceNr, result.Snapshot.Metadata.SequenceNr);
            // metadata timestamp may have been changed
        }

        [Fact]
        public void SnapshotStore_should_save_bigger_size_snapshot()
        {
            var metadata = new SnapshotMetadata(_pid, 100);
            var bigSnapshot = new byte[SnapshotByteSizeLimit];
            new Random().NextBytes(bigSnapshot);
            SnapshotStore.Tell(new SaveSnapshot(metadata, bigSnapshot), _senderProbe.Ref);
            _senderProbe.ExpectMessage<SaveSnapshotSuccess>(_timeout);
        }


        [Fact]
        public void ShouldSerializeSnapshots()
        {
            if (!SupportsSerialization) return;

            var probe = CreateTestProbe();
            var metadata = new SnapshotMetadata(_pid, 100L);
            var snap = new TestPayload(probe.Ref);

            SnapshotStore.Tell(new SaveSnapshot(metadata, snap), _senderProbe.Ref);
            _senderProbe.ExpectMessage<SaveSnapshotSuccess>(o =>
            {
                Assertions.AssertEqual(metadata.PersistenceId, o.Metadata.PersistenceId);
                Assertions.AssertEqual(metadata.SequenceNr, o.Metadata.SequenceNr);
            }, _timeout);

            var pid = _pid;
            SnapshotStore.Tell(new LoadSnapshot(pid, SnapshotSelectionCriteria.Latest, long.MaxValue), _senderProbe.Ref);
            _senderProbe.ExpectMessage<LoadSnapshotResult>(l =>
            {
                Assertions.AssertEqual(pid, l.Snapshot.Metadata.PersistenceId);
                Assertions.AssertEqual(100L, l.Snapshot.Metadata.SequenceNr);
                Assertions.AssertEqual(l.Snapshot.Snapshot, snap);
            }, _timeout);
        }
    }
}