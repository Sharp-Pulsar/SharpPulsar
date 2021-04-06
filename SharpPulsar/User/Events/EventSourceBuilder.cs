using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Sql.Client;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SharpPulsar.User.Events
{
    public class EventSourceBuilder: IEventSourceBuilder
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private long _fromSequenceId;
        private long _toSequenceId;
        private string _brokerWebServiceUrl;
        private readonly ActorSystem _actorSystem;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        public EventSourceBuilder(ActorSystem actorSystem, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, string tenant, string @namespace, string topic, long fromSequenceId, long toSequenceId, string brokerWebServiceUrl)
        {
            if (actorSystem == null)
                throw new ArgumentException("actorSystem is null");

            if (string.IsNullOrWhiteSpace(_brokerWebServiceUrl))
                throw new ArgumentException("AdminUrl is missing");

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic is missing");

            if (string.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentException("Namespace is missing");

            if (string.IsNullOrWhiteSpace(tenant))
                throw new ArgumentException("Tenant is missing");

            if (fromSequenceId < 0)
                throw new ArgumentException("FromSequenceId need to be greater than zero");

            if (toSequenceId <= fromSequenceId)
                throw new ArgumentException("ToSequenceId need to be greater than FromSequenceId");

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
        }

        public ISourceBuilder<T> Reader<T>(ReaderConfigBuilder<T> readerConfigBuilder, ISchema<T> schema)
        {
            if(schema == null)
                throw new NullReferenceException(nameof(schema));

            if (readerConfigBuilder == null)
                throw new NullReferenceException(nameof(readerConfigBuilder));

            return new ReaderSourceBuilder<T>(schema, _actorSystem, _client, _lookup, _cnxPool, _generator, _tenant, _namespace, _topic, _fromSequenceId, _toSequenceId, _brokerWebServiceUrl, readerConfigBuilder);
        }

        public ISourceBuilder<object> Sql(ClientOptions options, HashSet<string> selectedColumns)
        {
            if (selectedColumns == null)
                throw new ArgumentException("Columns cannot be null");

            var columns = selectedColumns.Where(x => !x.StartsWith("__") || !x.EndsWith("__") || !x.Equals("*")).ToImmutableHashSet();

            if (!columns.Any())
                throw new ArgumentException("Columns cannot be null or empty; column cannot start or end with '__'; '*' not allowed!");

            if (options == null)
                throw new ArgumentException("Option is null");

            if (!string.IsNullOrWhiteSpace(options.Execute))
                throw new ArgumentException("Please leave the Execute empty");

            return new SqlSourceBuilder(_actorSystem, _tenant, _namespace, _topic, _fromSequenceId, _toSequenceId, _brokerWebServiceUrl, options, selectedColumns);
        }
    }
}
