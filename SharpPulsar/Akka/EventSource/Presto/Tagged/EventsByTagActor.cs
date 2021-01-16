using System.Net.Http;
using System.Threading;
using Akka.Actor;
using Nito.AsyncEx;
using PulsarAdmin;
using SharpPulsar.Akka.EventSource.Messages.Presto;
using SharpPulsar.Messages;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;

namespace SharpPulsar.Akka.EventSource.Presto.Tagged
{
    public class EventsByTagActor : ReceiveActor
    {
        private readonly EventsByTag _message;
        private readonly HttpClient _httpClient;
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        public EventsByTagActor(EventsByTag message, HttpClient httpClient, IActorRef network, IActorRef pulsarManager)
        {
            _message = message;
            _httpClient = httpClient;
            _network = network;
            _pulsarManager = pulsarManager;
            var partitions = NewPartitionMetadataRequest($"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}");
            Setup(partitions);
        }

        private void Setup(Partitions p)
        {
            if (p.Partition > 0)
            {
                for (var i = 0; i < p.Partition; i++)
                {
                    var partitionTopic = TopicName.Get(p.Topic).GetPartition(i);
                    var msgId = GetMessageIds(partitionTopic);
                    Context.ActorOf(PrestoTaggedSourceActor.Prop(_pulsarManager, msgId.Start, msgId.End, true, _httpClient, _message, _message.Tag));

                }
            }
            else
            {
                var msgId = GetMessageIds(TopicName.Get(p.Topic));
                Context.ActorOf(PrestoTaggedSourceActor.Prop(_pulsarManager, msgId.Start, msgId.End, true, _httpClient, _message, _message.Tag));
            }
        }
        private Partitions NewPartitionMetadataRequest(string topic)
        {
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewPartitionMetadataRequest(topic, requestId);
            var pay = new Payload(request, requestId, "CommandPartitionedTopicMetadata", topic);
            var ask = _network.Ask<Partitions>(pay);
            return SynchronizationContextSwitcher.NoContext(async () => await ask).Result;
        }
        
        private (EventMessageId Start, EventMessageId End) GetMessageIds(TopicName topic)
        {
            var adminRestapi = new PulsarAdminRESTAPI(_message.AdminUrl, _httpClient, true);
            var statsTask = adminRestapi.GetInternalStats1Async(topic.NamespaceObject.Tenant, topic.NamespaceObject.LocalName, topic.LocalName);
            var stats = SynchronizationContextSwitcher.NoContext(async () => await statsTask).Result;
            var start = MessageIdHelper.Calculate(_message.FromSequenceId, stats);
            var startMessageId = new EventMessageId(start.Ledger, start.Entry, start.Index);
            var end = MessageIdHelper.Calculate(_message.ToSequenceId, stats);
            var endMessageId = new EventMessageId(end.Ledger, end.Entry, end.Index);
            return (startMessageId, endMessageId);
        }
        public static Props Prop(EventsByTag message, HttpClient httpClient, IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(() => new EventsByTagActor(message, httpClient, network, pulsarManager));
        }
    }
}