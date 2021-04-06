using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using System;

namespace SharpPulsar.User.Events
{
    internal class ReaderSourceMethod<T> : ISourceMethodBuilder<T>
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private long _fromSequenceId;
        private long _toSequenceId;
        private string _brokerWebServiceUrl;
        private readonly ReaderConfigBuilder<T> _conf;
        private ActorSystem _actorSystem;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly ISchema<T> _schema;

        public ReaderSourceMethod(ISchema<T> schema, ActorSystem actorSystem, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, string tenant, string @namespace, string topic, long fromSequenceId, long toSequenceId, string brokerWebServiceUrl, ReaderConfigBuilder<T> readerConfigBuilder)
        {
            _schema = schema;
            _client = client;
            _lookup = lookup;
            _cnxPool = cnxPool;
            _generator = generator;
            _actorSystem = actorSystem;
            _fromSequenceId = fromSequenceId;
            _toSequenceId = toSequenceId;
            _tenant = tenant;
            _namespace = @namespace;
            _topic = topic;
            _brokerWebServiceUrl = brokerWebServiceUrl;
            _conf = readerConfigBuilder;
        }

        public EventSource<T> Events()
        {
            //_pulsarManager.Tell(new EventSource.Messages.Pulsar.EventsByTopic(tenant, ns, topic, fromSequenceId, toSequenceId, adminUrl, configuration, _conf));
            return null;
        }
        public EventSource<T> CurrentEvents()
        {
            //_pulsarManager.Tell(new EventSource.Messages.Pulsar.CurrentEventsByTopic(tenant, ns, topic, fromSequenceId, toSequenceId, adminUrl, configuration, _conf));
            return null;
        }

        public EventSource<T> TaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");

            //_pulsarManager.Tell(new EventSource.Messages.Pulsar.EventsByTag(tenant, ns, topic, fromSequenceId, toSequenceId, tag, adminUrl, configuration, _conf));
            return null;
        }
        public EventSource<T> CurrentTaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");

            //_pulsarManager.Tell(new EventSource.Messages.Pulsar.CurrentEventsByTag(tenant, ns, topic, fromSequenceId, toSequenceId, tag, adminUrl, configuration, _conf));
            return null;
        }
    }
}
