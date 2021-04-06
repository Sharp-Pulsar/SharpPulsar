using Akka.Actor;
using SharpPulsar.EventSource.Messages.Presto;
using SharpPulsar.EventSource.Presto;
using SharpPulsar.EventSource.Presto.Tagged;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Sql.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

namespace SharpPulsar.User.Events
{
    internal class SqlSourceMethod : ISourceMethodBuilder
    {
        private readonly string _tenant;
        private readonly string _namespace;
        private readonly string _topic;

        private readonly long _fromSequenceId;
        private readonly long _toSequenceId;
        private readonly string _brokerWebServiceUrl;
        private readonly ActorSystem _actorSystem;
        private readonly ClientOptions _options;
        private readonly HashSet<string> _selectedColumns;
        public SqlSourceMethod(ActorSystem actorSystem, string tenant, string @namespace, string topic, long fromSequenceId, long toSequenceId, string brokerWebServiceUrl, ClientOptions options, HashSet<string> selectedColumns)
        {
            _actorSystem = actorSystem;
            _fromSequenceId = fromSequenceId;
            _toSequenceId = toSequenceId;
            _tenant = tenant;
            _namespace = @namespace;
            _topic = topic;
            _brokerWebServiceUrl = brokerWebServiceUrl;
            _options = options;
            _selectedColumns = selectedColumns;
        }
        public EventSource CurrentEvents()
        {
            var buffer = new BufferBlock<object>();
            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new CurrentEventsByTopic(_tenant, _namespace, _topic, _selectedColumns, _fromSequenceId, _toSequenceId, _brokerWebServiceUrl, _options);
            var actor = _actorSystem.ActorOf(CurrentEventsByTopicActor.Prop(msg, new HttpClient(), buffer), actorName);
            
            return new EventSource(_brokerWebServiceUrl, buffer, actor);
        }

        public EventSource CurrentTaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");
            
            var buffer = new BufferBlock<object>();
            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new CurrentEventsByTag(_tenant, _namespace, _topic, _selectedColumns, _fromSequenceId, _toSequenceId, tag, _options, _brokerWebServiceUrl);
            var actor = _actorSystem.ActorOf(CurrentEventsByTagActor.Prop(msg, new HttpClient(), buffer), actorName);

            return new EventSource(_brokerWebServiceUrl, buffer, actor);
        }

        public EventSource Events()
        {
            var buffer = new BufferBlock<object>();
            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new EventsByTopic(_tenant, _namespace, _topic, _selectedColumns, _fromSequenceId, _toSequenceId, _options, _brokerWebServiceUrl);
            var actor = _actorSystem.ActorOf(EventsByTopicActor.Prop(msg, new HttpClient(), buffer), actorName);

            return new EventSource(_brokerWebServiceUrl, buffer, actor);
        }

        public EventSource TaggedEvents(Tag tag)
        {
            if (tag == null)
                throw new ArgumentException("Tag is null");

            var buffer = new BufferBlock<object>();
            var actorName = Regex.Replace(_topic, @"[^\w\d]", "");
            var msg = new EventsByTag(_tenant, _namespace, _topic, _selectedColumns, _fromSequenceId, _toSequenceId, tag, _options, _brokerWebServiceUrl);
            var actor = _actorSystem.ActorOf(EventsByTagActor.Prop(msg, new HttpClient(), buffer), actorName);

            return new EventSource(_brokerWebServiceUrl, buffer, actor);
        }
    }
}
