#region copyright

// -----------------------------------------------------------------------
//  <copyright file="PulsarJournal.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Akka.Persistence.Journal;
using Akka.Serialization;
using SharpPulsar;
using SharpPulsar.Auth;
using SharpPulsar.Builder;
using SharpPulsar.Schemas;
using SharpPulsar.User;
using SharpPulsar.Utils;

namespace Akka.Persistence.Pulsar.Journal
{
   
    public class PulsarJournal : AsyncWriteJournal
    {
        private readonly PulsarSystem _pulsarSystem;
        private readonly PulsarClient _client;
        private readonly AvroSchema<JournalEntry> _journalEntrySchema;
        private readonly PulsarSettings _settings;


        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly Serializer _serializer;
        private static readonly Type PersistentRepresentationType = typeof(IPersistentRepresentation);

        private ImmutableDictionary<string, IImmutableSet<IActorRef>> _persistenceIdSubscribers = ImmutableDictionary.Create<string, IImmutableSet<IActorRef>>();
        private ImmutableDictionary<string, IImmutableSet<IActorRef>> _tagSubscribers = ImmutableDictionary.Create<string, IImmutableSet<IActorRef>>();
        private readonly HashSet<IActorRef> _newEventsSubscriber = new HashSet<IActorRef>();
        private IImmutableDictionary<string, long> _tagSequenceNr = ImmutableDictionary<string, long>.Empty;
        
        private readonly CancellationTokenSource _pendingRequestsCancellation;
        public static readonly ConcurrentDictionary<string, Producer<JournalEntry>> _producers = new ConcurrentDictionary<string, Producer<JournalEntry>>();

        private readonly PulsarJournalExecutor _journalExecutor;
        //public Akka.Serialization.Serialization Serialization => _serialization ??= Context.System.Serialization;

        public PulsarJournal(Config config) 
        {;
            _settings = new PulsarSettings(config);
            _journalEntrySchema = AvroSchema<JournalEntry>.Of(typeof(JournalEntry), new Dictionary<string, string>());
            _pendingRequestsCancellation = new CancellationTokenSource();
            _serializer = Context.System.Serialization.FindSerializerForType(PersistentRepresentationType);
            var builder = new PulsarClientConfigBuilder()
                 .ServiceUrl(_settings.ServiceUrl)
                 .ConnectionsPerBroker(1)
                 .OperationTimeout(_settings.OperationTimeOut)
                 .Authentication(AuthenticationFactory.Create(_settings.AuthClass, _settings.AuthParam));

            if (!(_settings.TrustedCertificateAuthority is null))
            {
                builder = builder.AddTrustedAuthCert(_settings.TrustedCertificateAuthority);
            }

            if (!(_settings.ClientCertificate is null))
            {
                builder = builder.AddTlsCerts(new X509Certificate2Collection { _settings.ClientCertificate });
            }
            var pulsarSystem = PulsarStatic.System ?? PulsarSystem.GetInstance(Context.System);
            if (PulsarStatic.Client == null)
                PulsarStatic.Client = pulsarSystem.NewClient(builder).AsTask().Result;
            _client = PulsarStatic.Client;
            _journalExecutor = new PulsarJournalExecutor(_client, Context.GetLogger(), _serializer);
        }

        /// <summary>
        /// This method replays existing event stream (identified by <paramref name="persistenceId"/>) asynchronously.
        /// It doesn't replay the whole stream, but only a window of it (described by range of [<paramref name="fromSequenceNr"/>, <paramref name="toSequenceNr"/>),
        /// with a limiter of up to <paramref name="max"/> elements - therefore it's possible that it will complete
        /// before the whole window is replayed.
        ///
        /// For every replayed message we need to construct a corresponding <see cref="Persistent"/> instance, that will
        /// be send back to a journal by calling a <paramref name="recoveryCallback"/>.
        /// 
        /// RETENTION POLICY MUST BE SENT AT THE NAMESPACE LEVEL ELSE TOPIC IS DELETED
        /// </summary>
        //Is ReplayMessagesAsync called once per actor lifetime?
        public override async Task ReplayMessagesAsync(IActorContext context, string persistenceId, long fromSequenceNr, long toSequenceNr, long max, Action<IPersistentRepresentation> recoveryCallback)
        {
            await _journalExecutor.ReplayMessages(persistenceId, fromSequenceNr, toSequenceNr, max,
                recoveryCallback, _journalEntrySchema, _settings.Topic);
        }

        /// <summary>
        /// This method is called at the very beginning of the replay procedure to define a possible boundary of replay:
        /// In akka persistence every persistent actor starts from the replay phase, where it replays state from all of
        /// the events emitted so far before being marked as ready for command processing.
        /// </summary>
        public override async Task<long> ReadHighestSequenceNrAsync(string persistenceId, long fromSequenceNr)
        {
            return await _journalExecutor.ReadHighestSequenceNr(persistenceId, fromSequenceNr);
        }
        /// <summary>
        /// Writes a batch of messages. Each <see cref="AtomicWrite"/> can have one or many <see cref="IPersistentRepresentation"/>
        /// events inside its payload, and they all should be written in atomic fashion (in one transaction, all-or-none).
        ///
        /// In case of failure of a single <see cref="AtomicWrite"/> we don't fail right away. Instead we try to write
        /// remaining writes and gather all exceptions produced in the process: they will be returned at the end.
        /// </summary>
        protected override async Task<IImmutableList<Exception>> WriteMessagesAsync(IEnumerable<AtomicWrite> messages)
        {
            var allTags = ImmutableHashSet<string>.Empty;
            var exceptions = new List<Exception>();
            foreach (var message in messages)
            {
                var persistentMessages = ((IImmutableList<IPersistentRepresentation>)message.Payload);

                foreach (var p in persistentMessages)
                {
                    if (p.Payload is Tagged t)
                    {
                        allTags = allTags.Union(t.Tags);
                    }
                }
                var sequenceId = message.HighestSequenceNr > 0 ? message.HighestSequenceNr : 1;
                var properties = new Dictionary<string, string>();
                foreach (var tag in allTags)
                {
                    //HACKING: So that we can easily query for tags
                    properties.TryAdd(tag.Trim().ToLower(), tag.Trim().ToLower());
                }
                var producer = await JournalProducer(message.PersistenceId);
                foreach (var m in persistentMessages)
                {
                    var journalEntry = ToJournalEntry(m);
                    await producer.NewMessage()
                        .Properties(properties)
                        .SequenceId(sequenceId)
                        .Value(journalEntry)
                        .SendAsync();
                }

                exceptions.Add(null);
            }

            /*var result = await Task<IImmutableList<Exception>>
                .Factory
                .ContinueWhenAll(writeTasks.ToArray(),
                    tasks => tasks.Select(t => t.IsFaulted ? TryUnwrapException(t.Exception) : null).ToImmutableList());
            */

            if (HasTagSubscribers && allTags.Count != 0)
            {
                foreach (var tag in allTags)
                {
                    NotifyTagChange(tag);
                }
            }

            return exceptions.ToImmutableList();
        }

        protected override Task DeleteMessagesToAsync(string persistenceId, long toSequenceNr)
        {
            return Task.CompletedTask;
        }

        private JournalEntry ToJournalEntry(IPersistentRepresentation message)
        {
            if (message.Payload is Tagged tagged)
            {
                var payload = tagged.Payload;
                message = message.WithPayload(payload); // need to update the internal payload when working with tags
            }

            var binary = Serialize(message);


            return new JournalEntry
            {
                Id = message.PersistenceId + "_" + message.SequenceNr,
                Ordering = DateTimeHelper.CurrentUnixTimeMillis(), // Auto-populates with timestamp
                IsDeleted = message.IsDeleted,
                Payload = binary,
                Manifest = message.Manifest,
                WritePlugin = message.WriterGuid,
                TimeStamp = message.Timestamp,
                PersistenceId = message.PersistenceId,
                SequenceNr = message.SequenceNr,
                Tags = JsonSerializer.Serialize(tagged.Tags == null ? new List<string>() : tagged.Tags.ToList(), new JsonSerializerOptions { WriteIndented = true })
            };
        }
        protected bool HasTagSubscribers => _tagSubscribers.Count != 0;
        protected override void PostStop()
        {
            base.PostStop();

            // stop all operations executed in the background
            _pendingRequestsCancellation.Cancel();
        }

        private byte[] Serialize(IPersistentRepresentation message)
        {
            return _serializer.ToBinary(message);
        }
        
        protected override bool ReceivePluginInternal(object message)
        {
            switch (message)
            {
                case ReplayTaggedMessages replay:
                    ReplayTaggedMessagesAsync(replay)
                        .AsTask()
                        .PipeTo(replay.ReplyTo, success: h => new RecoverySuccess(h), failure: e => new ReplayMessagesFailure(e));
                    return true;
                case ReplayAllEvents replay:
                    ReplayAllEventsAsync(replay)
                        .PipeTo(replay.ReplyTo, success: h => new EventReplaySuccess(h),
                            failure: e => new EventReplayFailure(e));
                    return true;
                case SubscribePersistenceId subscribe:
                    AddPersistenceIdSubscriber(Sender, subscribe.PersistenceId);
                    Context.Watch(Sender);
                    return true;
                case SelectCurrentPersistenceIds request:
                    SelectAllPersistenceIdsAsync(request.Offset)
                        .AsTask()
                        .PipeTo(request.ReplyTo, success: result => new CurrentPersistenceIds(result.Ids, request.Offset));
                    return true;
                case SubscribeTag subscribe:
                    AddTagSubscriber(Sender, subscribe.Tag);
                    Context.Watch(Sender);
                    return true;
                case SubscribeNewEvents _:
                    AddNewEventsSubscriber(Sender);
                    Context.Watch(Sender);
                    return true;
                case Terminated terminated:
                    RemoveSubscriber(terminated.ActorRef);
                    return true;
                default:
                    return false;
            }
        }

        public void AddNewEventsSubscriber(IActorRef subscriber)
        {
            _newEventsSubscriber.Add(subscriber);
        }
        /// <summary>
        /// Replays all events with given tag withing provided boundaries from current database.
        /// </summary>
        /// <param name="replay">TBD</param>
        /// <returns>TBD</returns>
        private async ValueTask<long> ReplayTaggedMessagesAsync(ReplayTaggedMessages replay)
        {
            /*ait foreach (var m in _journalExecutor.ReplayTagged(replay))
            {
                var ordering = m.SequenceNr;
                var payload = m.Payload;
                var persistent = Deserialize(payload);
                foreach (var adapted in AdaptFromJournal(persistent))
                    replay.ReplyTo.Tell(new ReplayedTaggedMessage(adapted, replay.Tag, ordering),
                        ActorRefs.NoSender);
            } */
            var max = await _journalExecutor.GetMaxOrdering(replay);
            return max;
        }


        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="subscriber">TBD</param>
        /// <param name="tag">TBD</param>
        public void AddTagSubscriber(IActorRef subscriber, string tag)
        {
            if (!_tagSubscribers.TryGetValue(tag, out var subscriptions))
            {
                _tagSubscribers = _tagSubscribers.Add(tag, ImmutableHashSet.Create(subscriber));
            }
            else
            {
                _tagSubscribers = _tagSubscribers.SetItem(tag, subscriptions.Add(subscriber));
            }
        }
        protected virtual async Task<long> ReplayAllEventsAsync(ReplayAllEvents replay)
        {
            return await _journalExecutor
                        .SelectAllEvents(
                            replay.FromOffset,
                            replay.ToOffset,
                            replay.Max,
                            replayedEvent => {
                                foreach (var adapted in AdaptFromJournal(replayedEvent.Persistent))
                                {
                                    replay.ReplyTo.Tell(new ReplayedEvent(adapted, replayedEvent.Offset), ActorRefs.NoSender);
                                }
                            });
        }
        public void AddPersistenceIdSubscriber(IActorRef subscriber, string persistenceId)
        {
            if (!_persistenceIdSubscribers.TryGetValue(persistenceId, out var subscriptions))
            {
                _persistenceIdSubscribers = _persistenceIdSubscribers.Add(persistenceId, ImmutableHashSet.Create(subscriber));
            }
            else
            {
                _persistenceIdSubscribers = _persistenceIdSubscribers.SetItem(persistenceId, subscriptions.Add(subscriber));
            }
        }

        private void RemoveSubscriber(IActorRef subscriber)
        {
            _persistenceIdSubscribers = _persistenceIdSubscribers.SetItems(_persistenceIdSubscribers
                .Where(kv => kv.Value.Contains(subscriber))
                .Select(kv => new KeyValuePair<string, IImmutableSet<IActorRef>>(kv.Key, kv.Value.Remove(subscriber))));

            _tagSubscribers = _tagSubscribers.SetItems(_tagSubscribers
                .Where(kv => kv.Value.Contains(subscriber))
                .Select(kv => new KeyValuePair<string, IImmutableSet<IActorRef>>(kv.Key, kv.Value.Remove(subscriber))));

            _newEventsSubscriber.Remove(subscriber);
        }
        private async ValueTask<(IEnumerable<string> Ids, long LastOrdering)> SelectAllPersistenceIdsAsync(long offset)
        {
            var lastOrdering = await _journalExecutor.SelectHighestSequenceNr();
            var ids = await _journalExecutor.SelectAllPersistenceIds(offset);
            return (ids, lastOrdering);
        }

        private void NotifyTagChange(string tag)
        {
            if (_tagSubscribers.TryGetValue(tag, out var subscribers))
            {
                var changed = new TaggedEventAppended(tag);
                foreach (var subscriber in subscribers)
                    subscriber.Tell(changed);
            }
        }

        private async ValueTask<Producer<JournalEntry>> JournalProducer(string persistenceid)
        {
            var topic = _settings.Topic;
            if (!_producers.TryGetValue(persistenceid, out var producer))
            {
                var producerConfig = new ProducerConfigBuilder<JournalEntry>()
                    .ProducerName($"journal-{persistenceid}")
                    .Topic(topic)
                    .Schema(_journalEntrySchema)
                    .SendTimeout(TimeSpan.FromMilliseconds(10000));
                producer = await _client.NewProducerAsync(_journalEntrySchema, producerConfig);
                _producers[persistenceid] = producer;
                return producer;
            }
            return producer;
        }
    }
}
