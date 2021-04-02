using SharpPulsar.Messages.Consumer;
using SharpPulsar.Sql.Client;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SharpPulsar.User
{
    public class EventSource
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;
        /// <summary>
        /// EventsByTopicReader is used for retrieving events for a specific topics 
        /// <para>
        /// You can retrieve a subset of all events by specifying <paramref name="fromSequenceId"/> and <paramref name="toSequenceId"/>
        /// or use `0L` and <see cref="long.MaxValue"/> respectively to retrieve all events. Note that
        /// the corresponding sequence id of each event is provided in the
        /// <see cref="EventMessage"/>, which makes it possible to resume the
        /// stream at a later point from a given sequence id.
        /// </para>
        /// The returned event stream is ordered by ledgerId and entryId.
        /// <para>
        /// The stream is not completed when it reaches the end of the currently stored events,
        /// but it continues to push new events when new events are persisted.
        /// Corresponding query that is completed when it reaches the end of the currently
        /// stored events is provided by CurrentEventsByTopicReader.
        /// </para>
        /// </summary>
        public void StartEventsByTopicReader(string tenant, string ns, string topic, long fromSequenceId, long toSequenceId, string adminUrl, ReaderConfigurationData configuration)
        {
            if (configuration == null)
                throw new ArgumentException("Configuration null");

            if (string.IsNullOrWhiteSpace(adminUrl))
                throw new ArgumentException("AdminUrl is missing");

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic is missing");

            if (string.IsNullOrWhiteSpace(ns))
                throw new ArgumentException("Namespace is missing");

            if (string.IsNullOrWhiteSpace(tenant))
                throw new ArgumentException("Tenant is missing");

            if (fromSequenceId < 0)
                throw new ArgumentException("FromSequenceId need to be greater than zero");

            if (toSequenceId <= fromSequenceId)
                throw new ArgumentException("ToSequenceId need to be greater than FromSequenceId");

            _pulsarManager.Tell(new EventSource.Messages.Pulsar.EventsByTopic(tenant, ns, topic, fromSequenceId, toSequenceId, adminUrl, configuration, _conf));
        }
        /// <summary>
        /// Same type of query as EventsByTopicReader but the event query
        /// is completed immediately when it reaches the end of the "result set". Events that are
        /// stored after the query is completed are not included in the event stream.
        /// </summary>
        public void StartCurrentEventsByTopicReader(string tenant, string ns, string topic, long fromSequenceId, long toSequenceId, string adminUrl, ReaderConfigurationData configuration)
        {
            if (configuration == null)
                throw new ArgumentException("Configuration null");

            if (string.IsNullOrWhiteSpace(adminUrl))
                throw new ArgumentException("AdminUrl is missing");

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic is missing");

            if (string.IsNullOrWhiteSpace(ns))
                throw new ArgumentException("Namespace is missing");

            if (string.IsNullOrWhiteSpace(tenant))
                throw new ArgumentException("Tenant is missing");

            if (fromSequenceId < 0)
                throw new ArgumentException("FromSequenceId need to be greater than zero");

            if (toSequenceId <= fromSequenceId)
                throw new ArgumentException("ToSequenceId need to be greater than FromSequenceId");

            _pulsarManager.Tell(new EventSource.Messages.Pulsar.CurrentEventsByTopic(tenant, ns, topic, fromSequenceId, toSequenceId, adminUrl, configuration, _conf));
        }

        /// <summary>
        /// EventsByTagReader is used for retrieving events that were marked with
        /// a given tag, e.g. all events of an Aggregate Root type.
        /// To tag events you create an a message with tag key and value as message property.
        /// Connection is made for each topic in the namespace
        /// The query is not completed when it reaches the end of the currently stored events,
        /// but it continues to push new events when new events are persisted.
        /// Corresponding query that is completed when it reaches the end of the currently
        /// stored events is provided by CurrentEventsByTagReader.
        /// </summary>
        public void StartEventsByTagReader(string tenant, string ns, string topic, Tag tag, long fromSequenceId, long toSequenceId, string adminUrl, ReaderConfigurationData configuration)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");
            if (configuration == null)
                throw new ArgumentException("Configuration null");

            if (string.IsNullOrWhiteSpace(adminUrl))
                throw new ArgumentException("AdminUrl is missing");

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic is missing");

            if (string.IsNullOrWhiteSpace(ns))
                throw new ArgumentException("Namespace is missing");

            if (string.IsNullOrWhiteSpace(tenant))
                throw new ArgumentException("Tenant is missing");

            if (fromSequenceId < 0)
                throw new ArgumentException("FromSequenceId need to be greater than zero");

            if (toSequenceId <= fromSequenceId)
                throw new ArgumentException("ToSequenceId need to be greater than FromSequenceId");

            _pulsarManager.Tell(new EventSource.Messages.Pulsar.EventsByTag(tenant, ns, topic, fromSequenceId, toSequenceId, tag, adminUrl, configuration, _conf));
        }
        /// <summary>
        /// Same type of query as EventsByTagReader but the event stream
        /// is completed immediately when it reaches the end of the "result set". Events that are
        /// stored after the query is completed are not included in the event stream.
        /// </summary>
        public void CurrentEventsByTagReader(string tenant, string ns, string topic, Tag tag, long fromSequenceId, long toSequenceId, string adminUrl, ReaderConfigurationData configuration)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");
            if (configuration == null)
                throw new ArgumentException("Configuration null");

            if (string.IsNullOrWhiteSpace(adminUrl))
                throw new ArgumentException("AdminUrl is missing");

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic is missing");

            if (string.IsNullOrWhiteSpace(ns))
                throw new ArgumentException("Namespace is missing");

            if (string.IsNullOrWhiteSpace(tenant))
                throw new ArgumentException("Tenant is missing");

            if (fromSequenceId < 0)
                throw new ArgumentException("FromSequenceId need to be greater than zero");

            if (toSequenceId <= fromSequenceId)
                throw new ArgumentException("ToSequenceId need to be greater than FromSequenceId");

            _pulsarManager.Tell(new EventSource.Messages.Pulsar.CurrentEventsByTag(tenant, ns, topic, fromSequenceId, toSequenceId, tag, adminUrl, configuration, _conf));
        }

        public void EventsByTopicSql(string tenant, string ns, string topic, ImmutableHashSet<string> cols, long fromSequenceId, long toSequenceId, ClientOptions options, string adminUrl)
        {

            if (cols == null)
                throw new ArgumentException("Columns cannot be null");

            var columns = cols.Where(x => !x.StartsWith("__") || !x.EndsWith("__") || !x.Equals("*")).ToImmutableHashSet();

            if (!columns.Any())
                throw new ArgumentException("Columns cannot be null or empty; column cannot start or end with '__'; '*' not allowed!");


            if (options == null)
                throw new ArgumentException("Option is null");

            if (!string.IsNullOrWhiteSpace(options.Execute))
                throw new ArgumentException("Please leave the Execute empty");

            if (string.IsNullOrWhiteSpace(adminUrl))
                throw new ArgumentException("AdminUrl is missing");

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic is missing");

            if (string.IsNullOrWhiteSpace(ns))
                throw new ArgumentException("Namespace is missing");

            if (string.IsNullOrWhiteSpace(tenant))
                throw new ArgumentException("Tenant is missing");

            if (fromSequenceId < 0)
                throw new ArgumentException("FromSequenceId need to be greater than zero");

            if (toSequenceId <= fromSequenceId)
                throw new ArgumentException("ToSequenceId need to be greater than FromSequenceId");

            _pulsarManager.Tell(new EventSource.Messages.Presto.EventsByTopic(tenant, ns, topic, columns, fromSequenceId, toSequenceId, options, adminUrl));
        }

        public void CurrentEventsByTopicPresto(string tenant, string ns, string topic, ImmutableHashSet<string> cols, long fromSequenceId, long toSequenceId, ClientOptions options, string adminUrl)
        {

            if (cols == null)
                throw new ArgumentException("Columns cannot be null");

            var columns = cols.Where(x => !x.StartsWith("__") || !x.EndsWith("__") || !x.Equals("*")).ToImmutableHashSet();

            if (!columns.Any())
                throw new ArgumentException("Columns cannot be null or empty; column cannot start or end with '__'; '*' not allowed!");


            if (options == null)
                throw new ArgumentException("Option is null");

            if (!string.IsNullOrWhiteSpace(options.Execute))
                throw new ArgumentException("Please leave the Execute empty");

            if (string.IsNullOrWhiteSpace(adminUrl))
                throw new ArgumentException("AdminUrl is missing");

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic is missing");

            if (string.IsNullOrWhiteSpace(ns))
                throw new ArgumentException("Namespace is missing");

            if (string.IsNullOrWhiteSpace(tenant))
                throw new ArgumentException("Tenant is missing");

            if (fromSequenceId < 0)
                throw new ArgumentException("FromSequenceId need to be greater than zero");

            if (toSequenceId <= fromSequenceId)
                throw new ArgumentException("ToSequenceId need to be greater than FromSequenceId");

            _pulsarManager.Tell(new EventSource.Messages.Presto.CurrentEventsByTopic(tenant, ns, topic, columns, fromSequenceId, toSequenceId, adminUrl, options));
        }
        public void EventsByTagPresto(string tenant, string ns, string topic, Tag tag, ImmutableHashSet<string> cols, long fromSequenceId, long toSequenceId, ClientOptions options, string adminUrl)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");

            if (cols == null)
                throw new ArgumentException("Columns cannot be null");

            var columns = cols.Where(x => !x.StartsWith("__") || !x.EndsWith("__") || !x.Equals("*")).ToImmutableHashSet();

            if (!columns.Any())
                throw new ArgumentException("Columns cannot be null or empty; column cannot start or end with '__'; '*' not allowed!");

            if (options == null)
                throw new ArgumentException("Option is null");

            if (!string.IsNullOrWhiteSpace(options.Execute))
                throw new ArgumentException("Please leave the Execute empty");

            if (string.IsNullOrWhiteSpace(adminUrl))
                throw new ArgumentException("AdminUrl is missing");

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic is missing");

            if (string.IsNullOrWhiteSpace(ns))
                throw new ArgumentException("Namespace is missing");

            if (string.IsNullOrWhiteSpace(tenant))
                throw new ArgumentException("Tenant is missing");

            if (fromSequenceId < 0)
                throw new ArgumentException("FromSequenceId need to be greater than zero");

            if (toSequenceId <= fromSequenceId)
                throw new ArgumentException("ToSequenceId need to be greater than FromSequenceId");

            _pulsarManager.Tell(new EventSource.Messages.Presto.EventsByTag(tenant, ns, topic, columns, fromSequenceId, toSequenceId, tag, options, adminUrl));
        }
        public void CurrentEventsByTagPresto(string tenant, string ns, string topic, Tag tag, ImmutableHashSet<string> cols, long fromSequenceId, long toSequenceId, ClientOptions options, string adminUrl)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");

            if (cols == null)
                throw new ArgumentException("Columns cannot be null");

            var columns = cols.Where(x => !x.StartsWith("__") || !x.EndsWith("__") || !x.Equals("*")).ToImmutableHashSet();

            if (!columns.Any())
                throw new ArgumentException("Columns cannot be null or empty; column cannot start or end with '__'; '*' not allowed!");

            if (options == null)
                throw new ArgumentException("Option is null");

            if (!string.IsNullOrWhiteSpace(options.Execute))
                throw new ArgumentException("Please leave the Execute empty");

            if (string.IsNullOrWhiteSpace(adminUrl))
                throw new ArgumentException("AdminUrl is missing");

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic is missing");

            if (string.IsNullOrWhiteSpace(ns))
                throw new ArgumentException("Namespace is missing");

            if (string.IsNullOrWhiteSpace(tenant))
                throw new ArgumentException("Tenant is missing");

            if (fromSequenceId < 0)
                throw new ArgumentException("FromSequenceId need to be greater than zero");

            if (toSequenceId <= fromSequenceId)
                throw new ArgumentException("ToSequenceId need to be greater than FromSequenceId");

            _pulsarManager.Tell(new EventSource.Messages.Presto.CurrentEventsByTag(tenant, ns, topic, columns, fromSequenceId, toSequenceId, tag, options, adminUrl));
        }
        public void EventTopics(IEventTopics message)
        {
            if (message == null)
                throw new ArgumentException("message is null");

            if (string.IsNullOrWhiteSpace(message.Namespace))
                throw new ArgumentException("Namespace is missing");

            if (string.IsNullOrWhiteSpace(message.Tenant))
                throw new ArgumentException("Tenant is missing");
            _pulsarManager.Tell(message);
        }

        public ActiveTopics CurrentActiveTopics(int timeoutMs = 5000, CancellationToken token = default)
        {
            if (_managerState.ActiveTopicsQueue.TryTake(out var msg, timeoutMs, token))
            {
                return msg;
            }

            return null;
        }
        public IEnumerable<ActiveTopics> ActiveTopics(int timeoutMs = 5000, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_managerState.ActiveTopicsQueue.TryTake(out var msg, timeoutMs, token))
                {
                    yield return msg;
                }

            }
        }
        /// <summary>
        /// Reads existing events and future events from Presto
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventEnvelope></returns>
        public IEnumerable<IEventEnvelope> SourceEventsFromPresto(int timeoutMs = 5000, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_managerState.PrestoEventQueue.TryTake(out var msg, timeoutMs, token))
                {
                    yield return msg;
                }
            }
        }

        /// <summary>
        /// Reads existing events from Presto
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventEnvelope></returns>
        public IEnumerable<IEventEnvelope> SourceCurrentEventsFromPresto(int timeoutMs = 5000, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_managerState.PrestoEventQueue.TryTake(out var msg, timeoutMs, token))
                {
                    yield return msg;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Reads existing events and future events from pulsar broker
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventMessage></returns>
        public IEnumerable<EventMessage> SourceEventsFromReader(int timeoutMs = 5000, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_managerState.PulsarEventQueue.TryTake(out var msg, timeoutMs, token))
                {
                    yield return msg;
                }
            }
        }

        /// <summary>
        /// Reads existing events from pulsar broker
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns>IEnumerable<EventMessage></returns>
        public IEnumerable<EventMessage> SourceCurrentEventsFromReader(int timeoutMs = 5000, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_managerState.PulsarEventQueue.TryTake(out var msg, timeoutMs, token))
                {
                    yield return msg;
                }
                else
                {
                    break;
                }
            }
        }
    }
    public enum SourceFrom
    {
        Sql,
        Reader
    }
    public enum EventMethod
    {
        /// <summary>
        /// EventsByTopicReader is used for retrieving events for a specific topics 
        /// <para>
        /// You can retrieve a subset of all events by specifying <paramref name="fromSequenceId"/> and <paramref name="toSequenceId"/>
        /// or use `0L` and <see cref="long.MaxValue"/> respectively to retrieve all events. Note that
        /// the corresponding sequence id of each event is provided in the
        /// <see cref="EventMessage"/>, which makes it possible to resume the
        /// stream at a later point from a given sequence id.
        /// </para>
        /// The returned event stream is ordered by ledgerId and entryId.
        /// <para>
        /// The stream is not completed when it reaches the end of the currently stored events,
        /// but it continues to push new events when new events are persisted.
        /// Corresponding query that is completed when it reaches the end of the currently
        /// stored events is provided by CurrentEventsByTopicReader.
        /// </para>
        /// </summary>
        EventsByTopic,
        /// <summary>
        /// Same type of query as EventsByTopicReader but the event query
        /// is completed immediately when it reaches the end of the "result set". Events that are
        /// stored after the query is completed are not included in the event stream.
        /// </summary>
        CurrentEventsByTopic,
        /// <summary>
        /// EventsByTagReader is used for retrieving events that were marked with
        /// a given tag, e.g. all events of an Aggregate Root type.
        /// To tag events you create an a message with tag key and value as message property.
        /// Connection is made for each topic in the namespace
        /// The query is not completed when it reaches the end of the currently stored events,
        /// but it continues to push new events when new events are persisted.
        /// Corresponding query that is completed when it reaches the end of the currently
        /// stored events is provided by CurrentEventsByTagReader.
        /// </summary>
        EventsByTag,
        /// <summary>
        /// Same type of query as EventsByTagReader but the event stream
        /// is completed immediately when it reaches the end of the "result set". Events that are
        /// stored after the query is completed are not included in the event stream.
        /// </summary>
        CurrentEventsByTag
    }

    public class EventInput
    {
        public SourceFrom SourceFrom { get; }
        public EventMethod EventMethod { get; }
        public object[] InputArgs { get; }
        public EventInput(SourceFrom sourceFrom, EventMethod eventMethod, object[] args)
        {
            SourceFrom = sourceFrom;
            EventMethod = eventMethod;
            InputArgs = args;
        }
    }
}
