using Akka.Actor;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.EventSource.Messages.Presto;
using SharpPulsar.EventSource.Presto;
using SharpPulsar.EventSource.Presto.Tagged;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Sql.Client;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

namespace SharpPulsar.User.Events
{
    public class SqlSourceMethod
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private readonly long _fromMessageId;
        private readonly long _toMessageId;
        private readonly string _brokerWebServiceUrl;
        private readonly ActorSystem _actorSystem;
        private readonly ClientOptions _options;
        private readonly HashSet<string> _selectedColumns;
        public SqlSourceMethod(ActorSystem actorSystem, string tenant, string @namespace, string topic, long fromMessageId, long toMessageId, string brokerWebServiceUrl, ClientOptions options, HashSet<string> selectedColumns)
        {
            _actorSystem = actorSystem;
            _fromMessageId = fromMessageId;
            _toMessageId = toMessageId;
            _tenant = tenant;
            _namespace = @namespace;
            _topic = topic;
            _brokerWebServiceUrl = brokerWebServiceUrl;
            _options = options;
            _selectedColumns = selectedColumns;
        }
        public SqlSource<IEventEnvelope> CurrentEvents()
        {
            var buffer = new BufferBlock<IEventEnvelope>();
            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new CurrentEventsByTopic(_tenant, _namespace, _topic, _selectedColumns, _fromMessageId, _toMessageId, _brokerWebServiceUrl, _options);
            var actor = _actorSystem.ActorOf(CurrentEventsByTopicActor.Prop(msg, buffer), actorName);
            
            return new SqlSource<IEventEnvelope>(_brokerWebServiceUrl, buffer, actor);
        }

        public SqlSource<IEventEnvelope> CurrentTaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");
            
            var buffer = new BufferBlock<IEventEnvelope>();
            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new CurrentEventsByTag(_tenant, _namespace, _topic, _selectedColumns, _fromMessageId, _toMessageId, tag, _options, _brokerWebServiceUrl);
            var actor = _actorSystem.ActorOf(CurrentEventsByTagActor.Prop(msg, buffer), actorName);

            return new SqlSource<IEventEnvelope>(_brokerWebServiceUrl, buffer, actor);
        }

        public SqlSource<IEventEnvelope> Events()
        {
            var buffer = new BufferBlock<IEventEnvelope>();
            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new EventsByTopic(_tenant, _namespace, _topic, _selectedColumns, _fromMessageId, _toMessageId, _options, _brokerWebServiceUrl);
            var actor = _actorSystem.ActorOf(EventsByTopicActor.Prop(msg, buffer), actorName);

            return new SqlSource<IEventEnvelope>(_brokerWebServiceUrl, buffer, actor);
        }

        public SqlSource<IEventEnvelope> TaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");

            var buffer = new BufferBlock<IEventEnvelope>();
            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new EventsByTag(_tenant, _namespace, _topic, _selectedColumns, _fromMessageId, _toMessageId, tag, _options, _brokerWebServiceUrl);
            var actor = _actorSystem.ActorOf(EventsByTagActor.Prop(msg, buffer), actorName);

            return new SqlSource<IEventEnvelope>(_brokerWebServiceUrl, buffer, actor);
        }
    }
}
