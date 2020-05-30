using System;
using System.Linq;
using System.Threading;
using Akka.Actor;
using PulsarAdmin.Models;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility;

namespace SharpPulsar.Akka.EventSource
{
    public class TopicReplayActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly PulsarSystem _pulsarSystem;
        private readonly StartReplayTopic _replayTopic;
        private readonly IActorRef _pulsarManager;
        private readonly IActorRef _network;
        private TopicName _topicName;
        private IActorRef _consumer;
        private readonly Tag _tag;
        private long _sequenceId;
        private readonly IActorRef _self;
        private long _expectedReplayMax;
        private long _currentReplayCount;
        public TopicReplayActor(PulsarSystem pulsarSystem, StartReplayTopic replayTopic, IActorRef pulsarManager, IActorRef network)
        {
            _self = Self;
            _sequenceId = replayTopic.From;
            _tag = replayTopic.Tag;
            _topicName = TopicName.Get(replayTopic.ReaderConfigurationData.TopicName);
            _pulsarSystem = pulsarSystem;
            _replayTopic = replayTopic;
            _pulsarManager = pulsarManager;
            _network = network;
            Become(Setup);

        }
        private void Setup()
        {
            Receive<ReplayState>(r =>
            {
                Become(Active);
                _expectedReplayMax = r.Max.Value;
                var partition = _topicName.PartitionIndex;
                var config = PrepareConsumerConfiguration(_replayTopic.ReaderConfigurationData);
                config.StartMessageId = new BatchMessageId(r.LedgerId.Value, r.EntryId.Value, partition, -1);
                config.ReceiverQueueSize = (int)(r.Max + 1) ;
                _consumer = Context.ActorOf(Consumer.Consumer.Prop(_replayTopic.ClientConfigurationData,
                    _topicName.ToString(), config, Interlocked.Increment(ref IdGenerators.ConsumerId), _network, true,
                    partition, SubscriptionMode.NonDurable, null, _pulsarManager, true));
                Stash.UnstashAll();
            });
            Receive<NullStats>(r =>
            {
                Become(Active);
                Stash.UnstashAll();
            });
            ReceiveAny(_ => Stash.Stash());
            _pulsarSystem.PulsarAdmin(new InternalCommands.Admin(AdminCommands.GetInternalStatsPersistent, new object[] { _topicName.NamespaceObject.Tenant, _topicName.NamespaceObject.LocalName, _topicName.LocalName, false }, e =>
            {
                if (e != null)
                {
                    var data = (PersistentTopicInternalStats)e;
                    var compute = new ComputeMessageId(data, _replayTopic.From, _replayTopic.To, _replayTopic.Max);
                    var result = compute.GetFrom();
                    var replayState = new ReplayState
                    {
                        LedgerId = result.Ledger,
                        EntryId = result.Entry,
                        To = result.To,
                        Max = result.Max
                    };
                    _self.Tell(replayState);
                }
                else
                    _self.Tell(NullStats.Instance);
            }, e =>
            {
                var context = Context;
                context.System.Log.Error(e.ToString());
            }, _replayTopic.AdminUrl, l =>
            {
                var context = Context;
                context.System.Log.Info(l);
            }));
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
        
        private void Active()
        {
            Receive<ConsumedMessage>(c =>
            {
                _currentReplayCount++;
                var messageId = (MessageId)c.Message.MessageId;
                if (!_replayTopic.Tagged)
                {
                    var eventMessage = new EventMessage(c.Message, _sequenceId, messageId.LedgerId, messageId.EntryId);
                    _pulsarManager.Tell(eventMessage);
                }
                else
                {
                    var props = c.Message.Properties;
                    if (props.ContainsKey(_tag.Key))
                    {
                        var value = props[_tag.Key];
                        if (value.Equals(_tag.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            var eventMessage = new EventMessage(c.Message, _sequenceId, messageId.LedgerId, messageId.EntryId);
                            _pulsarManager.Tell(eventMessage);
                        }
                    }
                }
                _sequenceId++;
            });
            Receive<NextPlay>(n =>
            {
                Become(() => NextPlayStats(n));
            });

        }

        private void NextPlayStats(NextPlay play)
        {
            Receive<ReplayState>(r =>
            {
                Become(Active);
                _consumer.Tell(new SendFlow(r.Max));
            });
            Receive<NullStats>(r =>
            {
                Become(Active);
                Stash.UnstashAll();
            });
            ReceiveAny(_ => Stash.Stash());
            _pulsarSystem.PulsarAdmin(new InternalCommands.Admin(AdminCommands.GetInternalStatsPersistent, new object[] { _topicName.NamespaceObject.Tenant, _topicName.NamespaceObject.LocalName, _topicName.LocalName, false }, e =>
            {
                if (e != null)
                {
                    var data = (PersistentTopicInternalStats)e;
                    var compute = new ComputeMessageId(data, play.From, play.To, play.Max);
                    var result = compute.GetFrom();
                    var replayState = new ReplayState
                    {
                        LedgerId = result.Ledger,
                        EntryId = result.Entry,
                        To = result.To,
                        Max = result.Max
                    };
                    _self.Tell(replayState);
                }
                else
                    _self.Tell(NullStats.Instance);
            }, e =>
            {
                var context = Context;
                context.System.Log.Error(e.ToString());
            }, _replayTopic.AdminUrl, l =>
            {
                var context = Context;
                context.System.Log.Info(l);
            }));
        }

        public static Props Prop(PulsarSystem pulsarSystem, StartReplayTopic startReplayTopic, IActorRef pulsarManager, IActorRef network)
        {
            return Props.Create(()=> new TopicReplayActor(pulsarSystem, startReplayTopic, pulsarManager, network));
        }
        public IStash Stash { get; set; }
    }
    
    public sealed class ReplayState
    {
        public long? LedgerId { get; set; }
        public long? EntryId { get; set; }
        public long? Max { get; set; }
        public long? To { get; set; }
    }

    public sealed class NullStats
    {
        public static NullStats Instance = new NullStats();
    }
}
