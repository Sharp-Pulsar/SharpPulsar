using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Akka.Actor;
using SharpPulsar.Akka.EventSource.Messages.Pulsar;
using SharpPulsar.Messages;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility;

namespace SharpPulsar.Akka.EventSource.Pulsar
{
    public class EventsByTopicActor : ReceiveActor
    {
        private readonly EventsByTopic _message;
        private readonly HttpClient _httpClient;
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        public EventsByTopicActor(EventsByTopic message, HttpClient httpClient, IActorRef network, IActorRef pulsarManager)
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
                    var partitionName = partitionTopic.ToString();
                    var msgId = GetMessageIds(partitionTopic);
                    var config = PrepareConsumerConfiguration(_message.Configuration, partitionName, msgId.Start,
                        (int)(msgId.End.Index - msgId.Start.Index));
                    Context.ActorOf(PulsarSourceActor.Prop(_message.ClientConfiguration, config, _pulsarManager, _network, msgId.End, true, _httpClient, _message, msgId.Start.Index));
                    
                }
            }
            else
            {
                var msgId = GetMessageIds(TopicName.Get(p.Topic));
                var config = PrepareConsumerConfiguration(_message.Configuration, p.Topic, msgId.Start, (int)(msgId.End.Index - msgId.Start.Index));
                Context.ActorOf(PulsarSourceActor.Prop(_message.ClientConfiguration, config, _pulsarManager, _network, msgId.End, true, _httpClient, _message, msgId.Start.Index));
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
        private ConsumerConfigurationData PrepareConsumerConfiguration(ReaderConfigurationData readerConfiguration, string topic, EventMessageId start, int permits)
        {
            var subscription = "player-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
            if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
            {
                subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
            }
            var consumerConfiguration = new ConsumerConfigurationData();
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
        public static Props Prop(EventsByTopic message, HttpClient httpClient, IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(() => new EventsByTopicActor(message, httpClient, network, pulsarManager));
        }
    }
}