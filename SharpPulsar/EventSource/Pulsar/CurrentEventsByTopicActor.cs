using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Pulsar;
using SharpPulsar.Messages;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility;
using SharpPulsar.Configuration;

namespace SharpPulsar.EventSource.Pulsar
{
    public class CurrentEventsByTopicActor<T> : ReceiveActor
    {
        private readonly CurrentEventsByTopic _message;
        private readonly HttpClient _httpClient;
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        public CurrentEventsByTopicActor(CurrentEventsByTopic message, HttpClient httpClient, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator)
        {
            _message = message;
            _httpClient = httpClient;
            _network = network;
            _pulsarManager = pulsarManager;
            var partitions = NewPartitionMetadataRequest($"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}");
            Setup(partitions);
            Receive<Terminated>(t =>
            {
                var children =  Context.GetChildren();
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
                    var partitionName = partitionTopic.ToString();
                    var msgId = GetMessageIds(partitionTopic);
                    var config = PrepareConsumerConfiguration(_message.Configuration, partitionName, msgId.Start,
                        (int) (msgId.End.Index - msgId.Start.Index)); 
                    var child = Context.ActorOf(PulsarSourceActor.Prop(_message.ClientConfiguration, config, _pulsarManager,_network, msgId.End, false, _httpClient, _message, msgId.Start.Index));
                    Context.Watch(child);
                }
            }
            else
            {
                var msgId = GetMessageIds(TopicName.Get(p.Topic));
                var config = PrepareConsumerConfiguration(_message.Configuration, p.Topic, msgId.Start, (int)(msgId.End.Index - msgId.Start.Index));
                var child = Context.ActorOf(PulsarSourceActor.Prop(_message.ClientConfiguration, config, _pulsarManager, _network, msgId.End, false, _httpClient, _message, msgId.Start.Index));
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
        private ConsumerConfigurationData<T> PrepareConsumerConfiguration(ReaderConfigurationData<T> readerConfiguration, string topic, EventMessageId start, int permits)
        {
            var subscription = "player-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
            if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
            {
                subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
            }
            var consumerConfiguration = new ConsumerConfigurationData<T>();
            consumerConfiguration.TopicNames.Add(topic);
            consumerConfiguration.SubscriptionName = subscription;
            consumerConfiguration.SubscriptionType = CommandSubscribe.SubType.Exclusive;
            consumerConfiguration.ReceiverQueueSize = readerConfiguration.ReceiverQueueSize;
            consumerConfiguration.ReadCompacted = readerConfiguration.ReadCompacted;
            consumerConfiguration.Schema = readerConfiguration.Schema;
            consumerConfiguration.ConsumerEventListener = readerConfiguration.EventListener;
            consumerConfiguration.StartMessageId = new MessageId(start.LedgerId, start.EntryId, -1);
            consumerConfiguration.ReceiverQueueSize = permits;

            if (readerConfiguration.ReaderName != null)
            {
                consumerConfiguration.ConsumerName = readerConfiguration.ReaderName;
            }

            if (readerConfiguration.ResetIncludeHead)
            {
                consumerConfiguration.ResetIncludeHead = true;
            }

            consumerConfiguration.CryptoFailureAction = readerConfiguration.CryptoFailureAction;
            if (readerConfiguration.CryptoKeyReader != null)
            {
                consumerConfiguration.CryptoKeyReader = readerConfiguration.CryptoKeyReader;
            }

            return consumerConfiguration;
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
        public static Props Prop(CurrentEventsByTopic message, HttpClient httpClient, IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(()=> new CurrentEventsByTopicActor(message, httpClient, network, pulsarManager));
        }
    }
}