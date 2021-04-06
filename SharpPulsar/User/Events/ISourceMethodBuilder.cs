using SharpPulsar.Messages.Consumer;

namespace SharpPulsar.User.Events
{
    public interface ISourceMethodBuilder<T>
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
        public EventSource<T> Events();
        /// <summary>
        /// Same type of query as EventsByTopicReader but the event query
        /// is completed immediately when it reaches the end of the "result set". Events that are
        /// stored after the query is completed are not included in the event stream.
        /// </summary>
        EventSource<T> CurrentEvents();

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
        EventSource<T> TaggedEvents(Tag tag);
        /// <summary>
        /// Same type of query as EventsByTagReader but the event stream
        /// is completed immediately when it reaches the end of the "result set". Events that are
        /// stored after the query is completed are not included in the event stream.
        /// </summary>
        EventSource<T> CurrentTaggedEvents(Tag tag);
    }
}
