using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Utils;

namespace SharpPulsar.User.Events
{
    public class ReaderSourceBuilder<T>
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private long _fromMessageId;
        private long _toMessageId;
        private string _brokerWebServiceUrl;
        private readonly ReaderConfigBuilder<T> _conf;
        private readonly ActorSystem _actorSystem;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly ISchema<T> _schema;
        private readonly ClientConfigurationData _clientConfiguration;
        public ReaderSourceBuilder(ClientConfigurationData clientConfiguration, ISchema<T> schema, ActorSystem actorSystem, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, string tenant, string @namespace, string topic, long fromMessageId, long toMessageId, string brokerWebServiceUrl, ReaderConfigBuilder<T> readerConfigBuilder)
        {
            _clientConfiguration = clientConfiguration;
            _schema = schema;
            _client = client;
            _lookup = lookup;
            _cnxPool = cnxPool;
            _generator = generator;
            _actorSystem = actorSystem;
            _fromMessageId = fromMessageId;
            _toMessageId = toMessageId;
            _tenant = tenant;
            _namespace = @namespace;
            _topic = topic;
            _brokerWebServiceUrl = brokerWebServiceUrl;
            _conf = readerConfigBuilder;
        }

        public ReaderSourceMethod<T> SourceMethod()
        {
            return new ReaderSourceMethod<T>(_clientConfiguration, _schema, _actorSystem, _client, _lookup, _cnxPool, _generator, _tenant, _namespace, _topic, _fromMessageId, _toMessageId, _brokerWebServiceUrl, _conf);
        }
    }
}
