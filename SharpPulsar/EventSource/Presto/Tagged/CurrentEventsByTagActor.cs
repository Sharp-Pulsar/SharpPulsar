using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Akka.Actor;
using SharpPulsar.Akka.EventSource.Messages.Presto;
using SharpPulsar.Messages;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;

namespace SharpPulsar.Akka.EventSource.Presto.Tagged
{
    public class CurrentEventsByTagActor : ReceiveActor
    {
        private readonly CurrentEventsByTag _message;
        private readonly HttpClient _httpClient;
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        public CurrentEventsByTagActor(CurrentEventsByTag message, HttpClient httpClient, IActorRef network, IActorRef pulsarManager)
        {
            _message = message;
            _httpClient = httpClient;
            _network = network;
            _pulsarManager = pulsarManager;
            var partitions = NewPartitionMetadataRequest($"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}");
            Setup(partitions);
            Receive<Terminated>(t =>
            {
                var children = Context.GetChildren();
                if (!children.Any())
                {
                    Context.System.Log.Info($"All children exited, shutting down in 5 seconds :{Self.Path}");
                    Self.GracefulStop(TimeSpan.FromSeconds(5));
                }
            });
        }

        private void Setup(Partitions p)
        {
            if (p.Partition > 0)
            {
                for (var i = 0; i < p.Partition; i++)
                {
                    var partitionTopic = TopicName.Get(p.Topic).GetPartition(i);
                    var msgId = GetMessageIds(partitionTopic);
                    var child = Context.ActorOf(PrestoTaggedSourceActor.Prop(_pulsarManager, msgId.Start, msgId.End, false, _httpClient, _message, _message.Tag));
                    Context.Watch(child);
                }
            }
            else
            {
                var msgId = GetMessageIds(TopicName.Get(p.Topic));
                var child = Context.ActorOf(PrestoTaggedSourceActor.Prop(_pulsarManager, msgId.Start, msgId.End, false, _httpClient, _message, _message.Tag));
                Context.Watch(child);
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
        public static Props Prop(CurrentEventsByTag message, HttpClient httpClient, IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(() => new CurrentEventsByTagActor(message, httpClient, network, pulsarManager));
        }
    }
}