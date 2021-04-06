using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using System;

namespace SharpPulsar.User.Events
{
    internal class ReaderSourceBuilder<T> : ISourceBuilder<T>
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private long _fromSequenceId;
        private long _toSequenceId;
        private string _brokerWebServiceUrl;
        private readonly ReaderConfigBuilder<T> _conf;
        private readonly ActorSystem _actorSystem;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly ISchema<T> _schema;
        public ReaderSourceBuilder(ISchema<T> schema, ActorSystem actorSystem, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, string tenant, string @namespace, string topic, long fromSequenceId, long toSequenceId, string brokerWebServiceUrl, ReaderConfigBuilder<T> readerConfigBuilder)
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

        public ISourceMethodBuilder<T> SourceMethod()
        {
            return new ReaderSourceMethod<T>(_schema, _actorSystem, _client, _lookup, _cnxPool, _generator, _tenant, _namespace, _topic, _fromSequenceId, _toSequenceId, _brokerWebServiceUrl, _conf);
        }
    }
}
