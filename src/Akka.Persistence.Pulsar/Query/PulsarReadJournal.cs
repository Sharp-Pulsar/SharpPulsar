#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PulsarReadJournal.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Akka.Persistence.Pulsar.Journal;
using Akka.Persistence.Query;
using Akka.Serialization;
using Akka.Streams.Dsl;
using SharpPulsar;
using SharpPulsar.Auth;
using SharpPulsar.Builder;
using SharpPulsar.Common.Naming;
using SharpPulsar.Schemas;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;
using SharpPulsar.Sql.Public;

namespace Akka.Persistence.Pulsar.Query
{
    public sealed class PulsarReadJournal :
        IPersistenceIdsQuery,
        ICurrentPersistenceIdsQuery,
        IEventsByPersistenceIdQuery,
        ICurrentEventsByPersistenceIdQuery,
        IEventsByTagQuery,
        ICurrentEventsByTagQuery,
        IAllEventsQuery,
        ICurrentAllEventsQuery
    {
        /// <summary>
        /// HOCON identifier
        /// </summary>
        public const string Identifier = "akka.persistence.query.journal.pulsar";

        private readonly SqlInstance _sql;
        private readonly LiveSqlInstance _liveSql;
        private readonly ClientOptions _sqlClientOptions;
        private readonly ILoggingAdapter _log;
        private readonly SerializationHelper _serializer;
        private readonly TopicName _topicName;
        private readonly TimeSpan _refreshInterval;
        private readonly string _writeJournalPluginId;
        private readonly int _maxBufferSize;
        private readonly PulsarSettings _settings;

        /// <inheritdoc />
        public PulsarReadJournal(ExtendedActorSystem system, Config config)
        {
            _settings = new PulsarSettings(config);
            _refreshInterval = config.GetTimeSpan("refresh-interval");
            _writeJournalPluginId = config.GetString("write-plugin");
            _maxBufferSize = config.GetInt("max-buffer-size");
            _log = system.Log;
            _serializer = new SerializationHelper(system);
            _topicName = TopicName.Get($"{_settings.Topic}");
            _sqlClientOptions = new ClientOptions
            {
                Server = _settings.TrinoServer,
                Catalog = "pulsar",
                Schema = $"{_topicName.Tenant}/{_topicName.NamespaceObject.LocalName}"
            };
            _sql = PulsarSystem.Sql(system, _sqlClientOptions);
            _liveSql = new LiveSqlInstance(system, _sqlClientOptions, _settings.Topic, _refreshInterval, DateTime.Parse("1970-01-18 20:27:56.387"));
        }

        /// <summary>
        /// <para>
        /// <see cref="PersistenceIds"/> is used for retrieving all `persistenceIds` of all
        /// persistent actors.
        /// </para>
        /// The returned event stream is unordered and you can expect different order for multiple
        /// executions of the query.
        /// <para>
        /// The stream is not completed when it reaches the end of the currently used `persistenceIds`,
        /// but it continues to push new `persistenceIds` when new persistent actors are created.
        /// Corresponding query that is completed when it reaches the end of the currently
        /// currently used `persistenceIds` is provided by <see cref="CurrentPersistenceIds"/>.
        /// </para>
        /// The SQL write journal is notifying the query side as soon as new `persistenceIds` are
        /// created and there is no periodic polling or batching involved in this query.
        /// <para>
        /// The stream is completed with failure if there is a failure in executing the query in the
        /// backend journal.
        /// </para>
        /// </summary>
        public Source<string, NotUsed> PersistenceIds() =>
            Source.ActorPublisher<string>(AllPersistenceIdsPublisher.Props(true, _writeJournalPluginId))
            .MapMaterializedValue(_ => NotUsed.Instance)
            .Named("AllPersistenceIds") as Source<string, NotUsed>;

        /// <summary>
        /// Same type of query as <see cref="PersistenceIds"/> but the stream
        /// is completed immediately when it reaches the end of the "result set". Persistent
        /// actors that are created after the query is completed are not included in the stream.
        /// </summary>
        public Source<string, NotUsed> CurrentPersistenceIds() =>
            Source.ActorPublisher<string>(AllPersistenceIdsPublisher.Props(false, _writeJournalPluginId))
            .MapMaterializedValue(_ => NotUsed.Instance)
            .Named("CurrentPersistenceIds") as Source<string, NotUsed>;

        /// <summary>
        /// <see cref="EventsByPersistenceId"/> is used for retrieving events for a specific
        /// <see cref="PersistentActor"/> identified by <see cref="Eventsourced.PersistenceId"/>.
        /// <para>
        /// You can retrieve a subset of all events by specifying <paramref name="fromSequenceNr"/> and <paramref name="toSequenceNr"/>
        /// or use `0L` and <see cref="long.MaxValue"/> respectively to retrieve all events. Note that
        /// the corresponding sequence number of each event is provided in the
        /// <see cref="EventEnvelope"/>, which makes it possible to resume the
        /// stream at a later point from a given sequence number.
        /// </para>
        /// The returned event stream is ordered by sequence number, i.e. the same order as the
        /// <see cref="PersistentActor"/> persisted the events. The same prefix of stream elements (in same order)
        ///  are returned for multiple executions of the query, except for when events have been deleted.
        /// <para>
        /// The stream is not completed when it reaches the end of the currently stored events,
        /// but it continues to push new events when new events are persisted.
        /// Corresponding query that is completed when it reaches the end of the currently
        /// stored events is provided by <see cref="CurrentEventsByPersistenceId"/>.
        /// </para>
        /// The SQLite write journal is notifying the query side as soon as events are persisted, but for
        /// efficiency reasons the query side retrieves the events in batches that sometimes can
        /// be delayed up to the configured `refresh-interval`.
        /// <para></para>
        /// The stream is completed with failure if there is a failure in executing the query in the
        /// backend journal.
        /// </summary>
        public Source<EventEnvelope, NotUsed> EventsByPersistenceId(string persistenceId, long fromSequenceNr, long toSequenceNr) 
        {

            return Source.FromGraph(new AsyncEnumerableSource<JournalEntry>(Messages(persistenceId, fromSequenceNr, toSequenceNr)))
                .Select(message =>
                {
                    var payload = message.Payload;
                    var der = _serializer.PersistentFromBytes(payload);
                    return new EventEnvelope(offset: new Sequence(message.SequenceNr), persistenceId, message.SequenceNr, der, message.PublishTime);
                })
               .MapMaterializedValue(_ => NotUsed.Instance)
               .Named("EventsByPersistenceId-" + persistenceId);
        }

        /// <summary>
        /// Same type of query as <see cref="EventsByPersistenceId"/> but the event stream
        /// is completed immediately when it reaches the end of the "result set". Events that are
        /// stored after the query is completed are not included in the event stream.
        /// </summary>
        /// 
        public async IAsyncEnumerable<JournalEntry> Messages(string persistenceId, long fromSequenceNr, long toSequenceNr)
        {
            //RETENTION POLICY MUST BE SENT AT THE NAMESPACE ELSE TOPIC IS DELETED
            var topic = _topicName.EncodedLocalName;
            if (fromSequenceNr > 0)
                fromSequenceNr = fromSequenceNr - 1;

            //__partition__ | __event_time__ | __publish_time__ | __message_id__ | __sequence_id__ | __producer_name__ | __key__ | __properties__
            var take = Math.Min(toSequenceNr, fromSequenceNr);
            _sqlClientOptions.Execute = $"select Id, __producer_name__ as PersistenceId, __sequence_id__ as SequenceNr, IsDeleted, Payload, Ordering, Tags, __partition__ as Partition, __event_time__ as EventTime, __publish_time__ as PublicTime, __message_id__ as MessageId, __key__ as Key, __properties__ as Properties from {topic} WHERE __producer_name__ = '{persistenceId}' AND __sequence_id__ BETWEEN {fromSequenceNr} AND {toSequenceNr} ORDER BY __sequence_id__ ASC LIMIT {take}";
            var data = await _sql.ExecuteAsync(TimeSpan.FromSeconds(5));
            switch (data.Response)
            {
                case DataResponse dr:
                    {
                        var records = dr.Data;
                        for (var i = 0; i < records.Count; i++)
                        {
                            var journal = JsonSerializer.Deserialize<JournalEntry>(JsonSerializer.Serialize(records[i]));
                            
                            yield return journal;
                        }
                        if (_log.IsDebugEnabled)
                            _log.Info(JsonSerializer.Serialize(dr.StatementStats, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    break;
                case StatsResponse sr:
                    if (_log.IsDebugEnabled)
                        _log.Info(JsonSerializer.Serialize(sr.Stats, new JsonSerializerOptions { WriteIndented = true }));
                    break;
                case ErrorResponse er:
                    _log.Error(er?.Error.FailureInfo.Message);
                    break;
                default:
                    break;
            }
        }

        public Source<EventEnvelope, NotUsed> CurrentEventsByPersistenceId(string persistenceId, long fromSequenceNr, long toSequenceNr) =>
                Source.ActorPublisher<EventEnvelope>(EventsByPersistenceIdPublisher.Props(persistenceId, fromSequenceNr, toSequenceNr, null, _maxBufferSize, _writeJournalPluginId))
                    .MapMaterializedValue(_ => NotUsed.Instance)
                    .Named("CurrentEventsByPersistenceId-" + persistenceId) as Source<EventEnvelope, NotUsed>;

        /// <summary>
        /// <see cref="EventsByTag"/> is used for retrieving events that were marked with
        /// a given tag, e.g. all events of an Aggregate Root type.
        /// <para></para>
        /// To tag events you create an <see cref="IEventAdapter"/> that wraps the events
        /// in a <see cref="Tagged"/> with the given `tags`.
        /// <para></para>
        /// You can use <see cref="NoOffset"/> to retrieve all events with a given tag or retrieve a subset of all
        /// events by specifying a <see cref="Sequence"/>. The `offset` corresponds to an ordered sequence number for
        /// the specific tag. Note that the corresponding offset of each event is provided in the
        /// <see cref="EventEnvelope"/>, which makes it possible to resume the
        /// stream at a later point from a given offset.
        /// <para></para>
        /// The `offset` is exclusive, i.e. the event with the exact same sequence number will not be included
        /// in the returned stream.This means that you can use the offset that is returned in <see cref="EventEnvelope"/>
        /// as the `offset` parameter in a subsequent query.
        /// <para></para>
        /// In addition to the <paramref name="offset"/> the <see cref="EventEnvelope"/> also provides `persistenceId` and `sequenceNr`
        /// for each event. The `sequenceNr` is the sequence number for the persistent actor with the
        /// `persistenceId` that persisted the event. The `persistenceId` + `sequenceNr` is an unique
        /// identifier for the event.
        /// <para></para>
        /// The returned event stream is ordered by the offset (tag sequence number), which corresponds
        /// to the same order as the write journal stored the events. The same stream elements (in same order)
        /// are returned for multiple executions of the query. Deleted events are not deleted from the
        /// tagged event stream.
        /// <para></para>
        /// The stream is not completed when it reaches the end of the currently stored events,
        /// but it continues to push new events when new events are persisted.
        /// Corresponding query that is completed when it reaches the end of the currently
        /// stored events is provided by <see cref="CurrentEventsByTag"/>.
        /// <para></para>
        /// The SQL write journal is notifying the query side as soon as tagged events are persisted, but for
        /// efficiency reasons the query side retrieves the events in batches that sometimes can
        /// be delayed up to the configured `refresh-interval`.
        /// <para></para>
        /// The stream is completed with failure if there is a failure in executing the query in the
        /// backend journal.
        /// </summary>
        public Source<EventEnvelope, NotUsed> EventsByTag(string tag, Offset offset = null)
        {
            offset = offset ?? new Sequence(0L);
            switch (offset)
            {
                case Sequence seq:
                    return Source.ActorPublisher<EventEnvelope>(EventsByTagPublisher.Props(tag, seq.Value, long.MaxValue, _refreshInterval, _maxBufferSize, _writeJournalPluginId))
                        .MapMaterializedValue(_ => NotUsed.Instance)
                        .Named($"EventsByTag-{tag}");
                case NoOffset _:
                    return EventsByTag(tag, new Sequence(0L));
                default:
                    throw new ArgumentException($"PulsarReadJournal does not support {offset.GetType().Name} offsets");
            }
        }

        /// <summary>
        /// Same type of query as <see cref="EventsByTag"/> but the event stream
        /// is completed immediately when it reaches the end of the "result set". Events that are
        /// stored after the query is completed are not included in the event stream.
        /// </summary>
        public Source<EventEnvelope, NotUsed> CurrentEventsByTag(string tag, Offset offset = null)
        {
            offset ??= new Sequence(1L);
            switch (offset)
            {
                case Sequence seq:
                    return Source.ActorPublisher<EventEnvelope>(EventsByTagPublisher.Props(tag, seq.Value, long.MaxValue, null, _maxBufferSize, _writeJournalPluginId))
                        .MapMaterializedValue(_ => NotUsed.Instance)
                        .Named($"CurrentEventsByTag-{tag}");
                case NoOffset _:
                    return CurrentEventsByTag(tag, new Sequence(1L));
                default:
                    throw new ArgumentException($"PulsarReadJournal does not support {offset.GetType().Name} offsets");
            }
        }

        public Source<EventEnvelope, NotUsed> AllEvents(Offset offset = null)
        {
            Sequence seq;
            switch (offset)
            {
                case null:
                case NoOffset _:
                    seq = new Sequence(0L);
                    break;
                case Sequence s:
                    seq = s;
                    break;
                default:
                    throw new ArgumentException($"SqlReadJournal does not support {offset.GetType().Name} offsets");
            }

            return Source.ActorPublisher<EventEnvelope>(AllEventsPublisher.Props(seq.Value, _refreshInterval, _maxBufferSize, _writeJournalPluginId))
                .MapMaterializedValue(_ => NotUsed.Instance)
                .Named("AllEvents");
        }

        public Source<EventEnvelope, NotUsed> CurrentAllEvents(Offset offset)
        {
            Sequence seq;
            switch (offset)
            {
                case null:
                case NoOffset _:
                    seq = new Sequence(0L);
                    break;
                case Sequence s:
                    seq = s;
                    break;
                default:
                    throw new ArgumentException($"SqlReadJournal does not support {offset.GetType().Name} offsets");
            }

            return Source.ActorPublisher<EventEnvelope>(AllEventsPublisher.Props(seq.Value, null, _maxBufferSize, _writeJournalPluginId))
                .MapMaterializedValue(_ => NotUsed.Instance)
                .Named("CurrentAllEvents");
        }
    }
}