using System.Net.Http;
using Akka.Actor;
using SharpPulsar.Akka.EventSource.Messages.Presto;
using SharpPulsar.Common.Naming;
using SharpPulsar.EventSource;

namespace SharpPulsar.Akka.EventSource.Presto.Tagged
{
    public class EventsByTagActor : ReceiveActor
    {
        private readonly EventsByTag _message;
        private readonly HttpClient _httpClient;
        private readonly User.Admin _admin;
        private readonly IActorRef _pulsarManager;
        public EventsByTagActor(EventsByTag message, HttpClient httpClient, IActorRef pulsarManager)
        {
            _admin = new User.Admin(message.AdminUrl, new HttpClient(), true);
            _message = message;
            _httpClient = httpClient;
            _pulsarManager = pulsarManager;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata(message.Tenant, message.Namespace, message.Topic, true);
            Setup(partitions.Body, topic);
        }

        private void Setup(Admin.Models.PartitionedTopicMetadata p, string topic)
        {
            if (p.Partitions > 0)
            {
                for (var i = 0; i < p.Partitions; i++)
                {
                    var partitionTopic = TopicName.Get(topic).GetPartition(i);
                    var msgId = GetMessageIds(partitionTopic);
                    Context.ActorOf(PrestoTaggedSourceActor.Prop(_pulsarManager, msgId.Start, msgId.End, true, _httpClient, _message, _message.Tag));

                }
            }
            else
            {
                var msgId = GetMessageIds(TopicName.Get(topic));
                Context.ActorOf(PrestoTaggedSourceActor.Prop(_pulsarManager, msgId.Start, msgId.End, true, _httpClient, _message, _message.Tag));
            }
        }        
        private (EventMessageId Start, EventMessageId End) GetMessageIds(TopicName topic)
        {
            var stats = _admin.GetInternalStats(topic.NamespaceObject.Tenant, topic.NamespaceObject.LocalName, topic.LocalName);
            var start = MessageIdHelper.Calculate(_message.FromSequenceId, stats.Body);
            var startMessageId = new EventMessageId(start.Ledger, start.Entry, start.Index);
            var end = MessageIdHelper.Calculate(_message.ToSequenceId, stats.Body);
            var endMessageId = new EventMessageId(end.Ledger, end.Entry, end.Index);
            return (startMessageId, endMessageId);
        }
        public static Props Prop(EventsByTag message, HttpClient httpClient, IActorRef pulsarManager)
        {
            return Props.Create(() => new EventsByTagActor(message, httpClient, pulsarManager));
        }
    }
}