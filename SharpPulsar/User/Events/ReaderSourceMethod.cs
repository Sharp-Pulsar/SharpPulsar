using SharpPulsar.Configuration;
using SharpPulsar.Messages.Consumer;
using System;

namespace SharpPulsar.User.Events
{
    internal class ReaderSourceMethod<T> : ISourceMethodBuilder
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private long _fromSequenceId;
        private long _toSequenceId;
        private string _brokerWebServiceUrl;
        private readonly ReaderConfigBuilder<T> _conf;

        public ReaderSourceMethod(string tenant, string @namespace, string topic, long fromSequenceId, long toSequenceId, string brokerWebServiceUrl, ReaderConfigBuilder<T> readerConfigBuilder)
        {
            _fromSequenceId = fromSequenceId;
            _toSequenceId = toSequenceId;
            _tenant = tenant;
            _namespace = @namespace;
            _topic = topic;
            _brokerWebServiceUrl = brokerWebServiceUrl;
            _conf = readerConfigBuilder;
        }

        public EventSource Events()
        {
            //_pulsarManager.Tell(new EventSource.Messages.Pulsar.EventsByTopic(tenant, ns, topic, fromSequenceId, toSequenceId, adminUrl, configuration, _conf));
            return null;
        }
        public EventSource CurrentEvents()
        {
            //_pulsarManager.Tell(new EventSource.Messages.Pulsar.CurrentEventsByTopic(tenant, ns, topic, fromSequenceId, toSequenceId, adminUrl, configuration, _conf));
            return null;
        }

        public EventSource TaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");

            //_pulsarManager.Tell(new EventSource.Messages.Pulsar.EventsByTag(tenant, ns, topic, fromSequenceId, toSequenceId, tag, adminUrl, configuration, _conf));
            return null;
        }
        public EventSource CurrentTaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");

            //_pulsarManager.Tell(new EventSource.Messages.Pulsar.CurrentEventsByTag(tenant, ns, topic, fromSequenceId, toSequenceId, tag, adminUrl, configuration, _conf));
            return null;
        }
    }
}
