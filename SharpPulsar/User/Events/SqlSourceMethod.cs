using SharpPulsar.Messages.Consumer;
using System;

namespace SharpPulsar.User.Events
{
    internal class SqlSourceMethod : ISourceMethodBuilder
    {
        public EventSource CurrentEvents()
        {
            //_pulsarManager.Tell(new EventSource.Messages.Presto.CurrentEventsByTopic(tenant, ns, topic, columns, fromSequenceId, toSequenceId, adminUrl, options));

            throw new NotImplementedException();
        }

        public EventSource CurrentTaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");

            //_pulsarManager.Tell(new EventSource.Messages.Presto.CurrentEventsByTag(tenant, ns, topic, columns, fromSequenceId, toSequenceId, tag, options, adminUrl));
            return null;
        }

        public EventSource Events()
        {
            //_pulsarManager.Tell(new EventSource.Messages.Presto.EventsByTopic(tenant, ns, topic, columns, fromSequenceId, toSequenceId, options, adminUrl));

            throw new NotImplementedException();
        }

        public EventSource TaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");


            //_pulsarManager.Tell(new EventSource.Messages.Presto.EventsByTag(tenant, ns, topic, columns, fromSequenceId, toSequenceId, tag, options, adminUrl));

            return null;
        }
    }
}
