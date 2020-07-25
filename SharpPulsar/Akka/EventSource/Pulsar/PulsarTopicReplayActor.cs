using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Akka.Actor;
using PulsarAdmin;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Batch;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility;

namespace SharpPulsar.Akka.EventSource.Pulsar
{
    public class PulsarTopicReplayActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly StartReplayTopic _replayTopic;
        private readonly IActorRef _pulsarManager;
        private readonly IActorRef _network;
        private readonly TopicName _topicName;
        private IActorRef _consumer;
        private readonly Tag _tag;
        private long _lastConsumedSequenceId;
        public PulsarTopicReplayActor(StartReplayTopic replayTopic, IActorRef pulsarManager, IActorRef network)
        {
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
            var data = adminRestapi.GetInternalStats1(_topicName.NamespaceObject.Tenant, _topicName.NamespaceObject.LocalName, _topicName.LocalName);
            var compute = new ComputeMessageId(data, _replayTopic.From, _replayTopic.To, _replayTopic.Max);
            var (ledger, entry, max, _) = compute.GetFrom();
           
            var partition = _topicName.PartitionIndex;
            var config = PrepareConsumerConfiguration(_replayTopic.ReaderConfigurationData);
            config.StartMessageId = (ledger == null || entry == null)? (BatchMessageId)MessageIdFields.Latest: new BatchMessageId(ledger.Value, entry.Value, partition, -1);
            config.ReceiverQueueSize = max.HasValue? (int)(max.Value): 100;
            _consumer = Context.ActorOf(Consumer.Consumer.Prop(_replayTopic.ClientConfigurationData, _topicName.ToString(), config, Interlocked.Increment(ref IdGenerators.ConsumerId), _network, true, partition, SubscriptionMode.NonDurable, null, _pulsarManager, true));
            Consume();
        }

        private void Consume()
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
                        var eventMessage = new EventMessage(c.Message, c.Message.SequenceId, messageId.LedgerId, messageId.EntryId);
                        Context.Parent.Tell(eventMessage);
                    }
                    else
                    {
                        Context.Parent.Tell(new NotTagged(c.Message, c.Message.SequenceId, c.Message.TopicName, messageId.LedgerId, messageId.EntryId));
                    }
                }

                _lastConsumedSequenceId = c.Message.SequenceId;
            });
            Receive<ReceiveTimeout>(t =>
            {
                Context.SetReceiveTimeout(null);
                Become(NextPlay);
            });
            Receive<NextPlay>(_=> Stash.Stash());
            //to track last sequence id for lagging player
            Context.SetReceiveTimeout(TimeSpan.FromSeconds(5));
        }

        //Since we have saved the last consumed sequence id before the timeout,
        //we can discard any Messages, they will be replayed after all from the last saved sequence id
        private void NextPlay()
        {
            Receive<NextPlay>(NextPlayStats);
            Stash.UnstashAll();
        }

        protected override void Unhandled(object message)
        {
            //Since we have saved the last consumed sequence id before the timeout,
            //we can discard any Messages, they will be replayed after all, from the last saved sequence id
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
            //for lagging player
            var @from = play.From > _lastConsumedSequenceId ? (_lastConsumedSequenceId + 1) : play.From;
            var adminRestapi = new PulsarAdminRESTAPI(_replayTopic.AdminUrl, new HttpClient(), true);
            var data = adminRestapi.GetInternalStats1(_topicName.NamespaceObject.Tenant, _topicName.NamespaceObject.LocalName, _topicName.LocalName, false);
            if (data == null) return;
            var compute = new ComputeMessageId(data, @from, play.To, play.Max);
            var (_, _, max, _) = compute.GetFrom();
            if(max != null)
                _consumer.Tell(new SendFlow(max));
            Become(Consume);
        }

        public static Props Prop(StartReplayTopic startReplayTopic, IActorRef pulsarManager, IActorRef network)
        {
            return Props.Create(()=> new PulsarTopicReplayActor(startReplayTopic, pulsarManager, network));
        }
        public IStash Stash { get; set; }
    }

    public sealed class ReplayLag
    {
        public long LastSequence { get; set; }
        public long CurrentSequence { get; set; }

    }
}
