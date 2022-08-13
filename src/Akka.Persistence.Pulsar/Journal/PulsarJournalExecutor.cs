using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Serialization;
using SharpPulsar;
using SharpPulsar.Builder;
using SharpPulsar.Schemas;
using SharpPulsar.User;
using SharpPulsar.Utils;

namespace Akka.Persistence.Pulsar.Journal
{
    public sealed class PulsarJournalExecutor
    {
        private readonly ILoggingAdapter _log ;
        private readonly Serializer _serializer;
        private readonly PulsarClient _client;
        private static readonly Type PersistentRepresentationType = typeof(IPersistentRepresentation);

        public PulsarJournalExecutor(PulsarClient client, ILoggingAdapter log, Serializer serializer)
        {
            _client = client;   
            _log = log;
            _serializer = serializer; 
        }
        public async ValueTask ReplayMessages(string persistenceId, long fromSequenceNr, long toSequenceNr, long max, Action<IPersistentRepresentation> recoveryCallback,
            AvroSchema<JournalEntry> journalEntrySchema, string topic )
        {
            //RETENTION POLICY MUST BE SENT AT THE NAMESPACE ELSE TOPIC IS DELETED
            var from = (MessageId)MessageIdUtils.GetMessageId(fromSequenceNr);
            var take = Math.Min(toSequenceNr - fromSequenceNr, max);
            var to = (MessageId)MessageIdUtils.GetMessageId(take);
            var startMessageId = new ReaderConfigBuilder<JournalEntry>()
            .StartMessageId(from.LedgerId, from.EntryId, from.PartitionIndex, 0)
            .Schema(journalEntrySchema)
            .Topic(topic);
            var reader = await _client.NewReaderAsync(journalEntrySchema, startMessageId);
            await foreach (var msg in Message(reader, fromSequenceNr, take))
            {
                var persistent = new Persistent(Deserialize(msg.Value.Payload), (long)msg.BrokerEntryMetadata.BrokerTimestamp, persistenceId, msg.Value.Manifest, msg.Value.IsDeleted, ActorRefs.NoSender, msg.Value.WritePlugin, msg.Value.TimeStamp);
                recoveryCallback(persistent);
            }
            await reader.CloseAsync();
        }
        private async IAsyncEnumerable<Message<JournalEntry>> Message(Reader<JournalEntry> r, long fromSequenceNr, long max)
        {
            var msg = await r.ReadNextAsync();
            while (fromSequenceNr < max)
            {
                if (msg != null)
                    yield return (Message<JournalEntry>)msg;

                msg = await r.ReadNextAsync();
            }
        }
        public async ValueTask<long> SelectAllEvents(
            long fromOffset,
            long toOffset,
            long max,
            Action<ReplayedEvent> callback)
        {
            var maxOrdering = await SelectHighestSequenceNr();
            return maxOrdering;
        }
        public async ValueTask<long> ReadHighestSequenceNr(string persistenceId, long fromSequenceNr)
        {
            return 0;
        }
        public async ValueTask<long> SelectHighestSequenceNr()
        {            
            return await Task.FromResult(0);
        }
        internal IPersistentRepresentation Deserialize(byte[] bytes)
        {
            return (IPersistentRepresentation)_serializer.FromBinary(bytes, PersistentRepresentationType);
        }
        public async ValueTask<IEnumerable<string>> SelectAllPersistenceIds(long offset)
        {
            var ids = new List<string>();
            return await Task.FromResult(ids);
        }
        internal async ValueTask<long> GetMaxOrdering(ReplayTaggedMessages replay)
        {
            var topic = $"journal".ToLower();
            var max = 0L;
            return max;
        }
    }
}
