using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Persistence.Pulsar.Query;
using Akka.Serialization;
using SharpPulsar;
using SharpPulsar.Builder;
using SharpPulsar.Messages;
using SharpPulsar.Sql;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;
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
        public async ValueTask ReplayMessages(string persistenceId, long fromSequenceNr, long toSequenceNr, long max, Action<IPersistentRepresentation> recoveryCallback)
        {
            //RETENTION POLICY MUST BE SENT AT THE NAMESPACE ELSE TOPIC IS DELETED
            var from = (MessageId)MessageIdUtils.GetMessageId(fromSequenceNr);
            var to = (MessageId)MessageIdUtils.GetMessageId(toSequenceNr);
            var startMessageId = new ReaderConfigBuilder<byte[]>()
            .StartMessageId(from.LedgerId, from.EntryId, from.PartitionIndex, 0)
            .Topic(persistenceId);
            var reader = await _client.NewReaderAsync(startMessageId);
            async IAsyncEnumerable<Message<byte[]>> message()
            {
                var msg = await reader.ReadNextAsync(TimeSpan.FromMilliseconds(1000));
                while (MessageIdUtils.GetOffset(from) < MessageIdUtils.GetOffset(to))
                {
                    yield return (Message<byte[]>)msg;
                    msg = await reader.ReadNextAsync(TimeSpan.FromMilliseconds(1000));
                }
            };
            await foreach (var msg in message())
            {
                var persistent = new Persistent(msg.Data, msg.SequenceId, persistenceId/*, manifest, sender*/);
                recoveryCallback(persistent);
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
