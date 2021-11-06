using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.EventSource.Messages.Pulsar;
using SharpPulsar.EventSource.Pulsar;
using SharpPulsar.EventSource.Pulsar.Tagged;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using System;
using System.Text.RegularExpressions;

namespace SharpPulsar.User.Events
{
    public class ReaderSourceMethod<T> 
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private long _fromMessageId;
        private long _toMessageId;
        private string _brokerWebServiceUrl;
        private readonly ReaderConfigBuilder<T> _conf;
        private readonly ClientConfigurationData _clientConfiguration;
        private ActorSystem _actorSystem;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly ISchema<T> _schema;

        public ReaderSourceMethod(ClientConfigurationData clientConfiguration, ISchema<T> schema, ActorSystem actorSystem, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, string tenant, string @namespace, string topic, long fromMessageId, long toMessageId, string brokerWebServiceUrl, ReaderConfigBuilder<T> readerConfigBuilder)
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

        public ReaderSource<T> Events()
        {
            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new EventsByTopic<T>(_tenant, _namespace, _topic, _fromMessageId, _toMessageId, _brokerWebServiceUrl, _conf.ReaderConfigurationData, _clientConfiguration);
            var actor = _actorSystem.ActorOf(EventsByTopicActor<T>.Prop(msg,  _client, _lookup, _cnxPool, _generator, _schema), actorName);
            
            return new ReaderSource<T>(_brokerWebServiceUrl, actor);           
        }
        public ReaderSource<T> CurrentEvents()
        {
            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new CurrentEventsByTopic<T>(_tenant, _namespace, _topic, _fromMessageId, _toMessageId, _brokerWebServiceUrl, _conf.ReaderConfigurationData, _clientConfiguration);
            var actor = _actorSystem.ActorOf(CurrentEventsByTopicActor<T>.Prop(msg, _client, _lookup, _cnxPool, _generator, _schema), actorName);
           
            return new ReaderSource<T>(_brokerWebServiceUrl, actor);
        }

        public ReaderSource<T> TaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");

            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new EventsByTag<T>(_tenant, _namespace, _topic, _fromMessageId, _toMessageId, tag, _brokerWebServiceUrl, _conf.ReaderConfigurationData, _clientConfiguration);
            var actor = _actorSystem.ActorOf(EventsByTagActor<T>.Prop(msg, _client, _lookup, _cnxPool, _generator, _schema), actorName);
            
            return new ReaderSource<T>(_brokerWebServiceUrl, actor);
        }
        public ReaderSource<T> CurrentTaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");

            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new CurrentEventsByTag<T>(_tenant, _namespace, _topic, _fromMessageId, _toMessageId, tag, _brokerWebServiceUrl, _conf.ReaderConfigurationData, _clientConfiguration);
            var actor = _actorSystem.ActorOf(CurrentEventsByTagActor<T>.Prop(msg, _client, _lookup, _cnxPool, _generator, _schema), actorName);
            
            return new ReaderSource<T>(_brokerWebServiceUrl, actor);
        }
    }
}
