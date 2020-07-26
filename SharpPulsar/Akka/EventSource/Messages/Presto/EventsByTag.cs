﻿using System.Collections.Immutable;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.Sql.Client;

namespace SharpPulsar.Akka.EventSource.Messages.Presto
{
    /// <summary>
    /// <see cref="EventsByTag"/> is used for retrieving events that were marked with
    /// a given tag, e.g. all events of an Aggregate Root type.
    /// To tag events you create an a message with tag key and value as message property.
    /// Connection is made for each topic in the namespace
    /// The query is not completed when it reaches the end of the currently stored events,
    /// but it continues to push new events when new events are persisted.
    /// Corresponding query that is completed when it reaches the end of the currently
    /// stored events is provided by <see cref="CurrentEventsByTag"/>.
    /// </summary>
    public sealed class EventsByTag : IPrestoEventSourceMessage
    {
        public EventsByTag(string tenant, string ns, string topic, ImmutableHashSet<string> columns, long fromSequenceId, long toSequenceId, Tag tag, SourceType source,  ClientOptions options)
        {
            Tenant = tenant;
            Namespace = ns;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
            Tag = tag;
            Source = source;
            Topic = topic;
            Columns = columns;
            Options = options;
        }
        public Tag Tag { get; }
        public string Tenant { get; }
        public string Namespace { get; }
        public string Topic { get; }
        public long FromSequenceId { get; } //Compute ledgerId and entryId for this 
        public long ToSequenceId { get; } //Compute ledgerId and entryId for this 
        public SourceType Source { get; }
        public ClientOptions Options { get; }
        public ImmutableHashSet<string> Columns { get; }
    }
}
