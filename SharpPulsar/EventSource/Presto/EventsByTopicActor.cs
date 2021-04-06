using System.Net.Http;
using System.Threading;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Presto;
using SharpPulsar.Messages;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;
using System.Threading.Tasks.Dataflow;

namespace SharpPulsar.EventSource.Presto
{
    public class EventsByTopicActor : ReceiveActor
    {
        private readonly EventsByTopic _message;
        private readonly HttpClient _httpClient;
        private readonly BufferBlock<object> _buffer;
        private readonly User.Admin _admin;
        public EventsByTopicActor(EventsByTopic message, HttpClient httpClient, BufferBlock<object> buffer)
        {
            _admin = new User.Admin(message.AdminUrl, httpClient);
            _buffer = buffer;
            _message = message;
            _httpClient = httpClient;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata(message.Tenant, message.Namespace, message.Topic);
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
        public static Props Prop(EventsByTopic message, HttpClient httpClient, BufferBlock<object> buffer)
        {
            return Props.Create(() => new EventsByTopicActor(message, httpClient, buffer));
        }
    }
}