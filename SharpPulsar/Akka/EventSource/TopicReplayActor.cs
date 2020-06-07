using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Akka.Actor;
using PulsarAdmin;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility;

namespace SharpPulsar.Akka.EventSource
{
    public class TopicReplayActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly StartReplayTopic _replayTopic;
        private readonly IActorRef _pulsarManager;
        private readonly IActorRef _network;
        private TopicName _topicName;
        private IActorRef _consumer;
        private readonly Tag _tag;
        private long _sequenceId;
        private readonly IActorRef _self;
        public TopicReplayActor(StartReplayTopic replayTopic, IActorRef pulsarManager, IActorRef network)
        {
            _self = Self;
            _sequenceId = replayTopic.From;
            _tag = replayTopic.Tag;
            _topicName = TopicName.Get(replayTopic.ReaderConfigurationData.TopicName);
            _replayTopic = replayTopic;
            _pulsarManager = pulsarManager;
            _network = network;
            Setup();

        }
        private void Setup()
        {
            var adminRestapi = new PulsarAdminRESTAPI(_replayTopic.AdminUrl, new HttpClient(), true);
            var data = adminRestapi.GetInternalStats1(_topicName.NamespaceObject.Tenant, _topicName.NamespaceObject.LocalName, _topicName.LocalName, false);
            var compute = new ComputeMessageId(data, _replayTopic.From, _replayTopic.To, _replayTopic.Max);
            var (ledger, entry, max, _) = compute.GetFrom();
           
            var partition = _topicName.PartitionIndex;
            var config = PrepareConsumerConfiguration(_replayTopic.ReaderConfigurationData);
            config.StartMessageId = (ledger == null || entry == null)? (BatchMessageId)MessageIdFields.Latest: new BatchMessageId(ledger.Value, entry.Value, partition, -1);
            config.ReceiverQueueSize = (int)(max);
            _consumer = Context.ActorOf(Consumer.Consumer.Prop(_replayTopic.ClientConfigurationData,
                _topicName.ToString(), config, Interlocked.Increment(ref IdGenerators.ConsumerId), _network, true,
                partition, SubscriptionMode.NonDurable, null, _pulsarManager, true));
            Active();
        }

        private void Active()
        {
            Receive<ConsumedMessage>(c =>
            {
                var messageId = (MessageId)c.Message.MessageId;
                if (!_replayTopic.Tagged)
                {
                    var eventMessage = new EventMessage(c.Message, c.Message.SequenceId, messageId.LedgerId, messageId.EntryId);
                    _pulsarManager.Tell(eventMessage);
                }
                else
                {
                    var props = c.Message.Properties;
                    var tagged = props.Any(x => x.Key.Equals(_tag.Key, StringComparison.OrdinalIgnoreCase)
                                                && x.Value.Contains(_tag.Value, StringComparison.OrdinalIgnoreCase));
                    if (tagged)
                    {
                        var eventMessage = new EventMessage(c.Message, _sequenceId, messageId.LedgerId, messageId.EntryId);
                        Context.Parent.Tell(eventMessage);
                        Context.System.Log.Info($"Tag '{_tag.Key}':'{_tag.Value}' matched");
                    }
                    else
                    {
                        Context.Parent.Tell(new NotTagged(_sequenceId, c.Message.TopicName));
                    }
                }
                _sequenceId++;
            });
            Receive<NextPlay>(NextPlayStats);
        }
        private ConsumerConfigurationData PrepareConsumerConfiguration(ReaderConfigurationData readerConfiguration)
        {
            var subscription = "player-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
            if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
            {
                subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
            }
            var consumerConfiguration = new ConsumerConfigurationData();
            consumerConfiguration.TopicNames.Add(readerConfiguration.TopicName);
            consumerConfiguration.SubscriptionName = subscription;
            consumerConfiguration.SubscriptionType = CommandSubscribe.SubType.Exclusive;
            consumerConfiguration.ReceiverQueueSize = readerConfiguration.ReceiverQueueSize;
            consumerConfiguration.ReadCompacted = readerConfiguration.ReadCompacted;
            consumerConfiguration.Schema = readerConfiguration.Schema;
            consumerConfiguration.ConsumerEventListener = readerConfiguration.EventListener;

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
        
        private void NextPlayStats(NextPlay play)
        {
            var adminRestapi = new PulsarAdminRESTAPI(_replayTopic.AdminUrl, new HttpClient(), true);
            var data = adminRestapi.GetInternalStats1(_topicName.NamespaceObject.Tenant, _topicName.NamespaceObject.LocalName, _topicName.LocalName, false);
            if (data == null) return;
            var compute = new ComputeMessageId(data, play.From, play.To, play.Max);
            var (_, _, max, _) = compute.GetFrom();
            if(max != null)
                _consumer.Tell(new SendFlow(max));
        }

        public static Props Prop(StartReplayTopic startReplayTopic, IActorRef pulsarManager, IActorRef network)
        {
            return Props.Create(()=> new TopicReplayActor(startReplayTopic, pulsarManager, network));
        }
        public IStash Stash { get; set; }
    }
    
}
