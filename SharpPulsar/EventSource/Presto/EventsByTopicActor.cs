using System.Net.Http;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Presto;
using SharpPulsar.Common.Naming;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.Admin.Admin.Models;
using SharpPulsar.EventSource.Messages;

namespace SharpPulsar.EventSource.Presto
{
    public class EventsByTopicActor : ReceiveActor
    {
        private readonly EventsByTopic _message;
        private readonly HttpClient _httpClient;
        private readonly BufferBlock<IEventEnvelope> _buffer;
        private readonly Admin.Public.Admin _admin;
        public EventsByTopicActor(EventsByTopic message, HttpClient httpClient, BufferBlock<IEventEnvelope> buffer)
        {
            _admin = new Admin.Public.Admin(message.AdminUrl, httpClient);
            _buffer = buffer;
            _message = message;
            _httpClient = httpClient;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata(message.Tenant, message.Namespace, message.Topic);
            Setup(partitions.Body, topic);
        }

        private void Setup(PartitionedTopicMetadata p, string topic)
        {
            if (p.Partitions > 0)
            {
                for (var i = 0; i < p.Partitions; i++)
                {
                    var partitionTopic = TopicName.Get(topic).GetPartition(i);
                    var msgId = GetMessageIds(partitionTopic);
                    Context.ActorOf(PrestoSourceActor.Prop(_buffer, msgId.Start, msgId.End, true, _httpClient, _message));
                    
                }
            }
            else
            {
                var msgId = GetMessageIds(TopicName.Get(topic));
                Context.ActorOf(PrestoSourceActor.Prop(_buffer, msgId.Start, msgId.End, true, _httpClient, _message));
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
        public static Props Prop(EventsByTopic message, HttpClient httpClient, BufferBlock<IEventEnvelope> buffer)
        {
            return Props.Create(() => new EventsByTopicActor(message, httpClient, buffer));
        }
    }
}