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
        private readonly Filter _filter;
        private long _sequenceId;
        public TopicReplayActor(PulsarSystem pulsarSystem, StartReplayTopic replayTopic, IActorRef pulsarManager, IActorRef network, string tag)
        {
            _sequenceId = replayTopic.From;
            _filter = replayTopic.Filter;
            _topicName = TopicName.Get(replayTopic.ReaderConfigurationData.TopicName);
            _pulsarSystem = pulsarSystem;
            _replayTopic = replayTopic;
            _pulsarManager = pulsarManager;
            _network = network;
            Become(Setup);
            ReceiveAny(a=> Stash.Stash());

        }
        private void Setup()
        {
            Receive<ReplayState>(r =>
            {
                var partition = _topicName.PartitionIndex;
                var config = PrepareConsumerConfiguration(_replayTopic.ReaderConfigurationData);
                config.StartMessageId = new BatchMessageId(r.LedgerId.Value, r.EntryId.Value, partition, -1);
                _consumer = Context.ActorOf(Consumer.Consumer.Prop(_replayTopic.ClientConfigurationData,
                    _topicName.ToString(), config, Interlocked.Increment(ref IdGenerators.ConsumerId), _network, true,
                    partition, SubscriptionMode.NonDurable, null, _pulsarManager, true));
                Become(Active);
                Stash.UnstashAll();
            });
            Receive<NullStats>(r =>
            {
                Become(Active);
                Stash.UnstashAll();
            });
            ReceiveAny(_ => Stash.Stash());
            _pulsarSystem.PulsarAdmin(new InternalCommands.Admin(AdminCommands.GetInternalStatsPersistent, new object[] { _topicName.Tenant, _topicName.Namespace, _topicName.ToString().Split("/").Last(), false }, e =>
            {
                var self = Self;
                if (e != null)
                {
                    var data = (PersistentTopicInternalStats)e;
                    var compute = new ComputeMessageId(data, _replayTopic.From, _replayTopic.To, _replayTopic.Max);
                    var result = compute.GetFrom();
                    var replayState = new ReplayState
                    {
                        LedgerId = result.Ledger,
                        EntryId = result.Entry,
                        To = data.NumberOfEntries,
                        Max = result.NumberOfEntries
                    };
                    self.Tell(replayState);
                }
                else
                    self.Tell(NullStats.Instance);
            }, e =>
            {
                var context = Context;
                context.System.Log.Error(e.ToString());
            }, _replayTopic.ClientConfigurationData.ServiceUrl, l =>
            {
                var context = Context;
                context.System.Log.Info(l);
            }));
        }

        private ConsumerConfigurationData PrepareConsumerConfiguration(ReaderConfigurationData readerConfiguration)
        {
            var subscription = "reader-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
            if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
            {
                subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
            }

            var readerListener = readerConfiguration.ReaderListener;
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
        private void GetStats()
        {
            Receive<ReplayState>(r =>
            {
                _pulsarManager.Tell(new NumberOfEntries(_topicName.ToString(), r.Max));
                Become(Active);
                Stash.UnstashAll();
            });
            Receive<NullStats>(r =>
            {
                Become(Active);
                Stash.UnstashAll();
            });
            ReceiveAny(_=> Stash.Stash());
            _pulsarSystem.PulsarAdmin(new InternalCommands.Admin(AdminCommands.GetInternalStatsPersistent, new object[] { _topicName.Tenant, _topicName.Namespace, _topicName.ToString().Split("/").Last(), false }, e =>
            {
                var self = Self;
                if (e != null)
                {
                    var data = (PersistentTopicInternalStats)e;
                    var replayState = new ReplayState
                    {
                        LedgerId = 0,
                        EntryId = 0,
                        To = 0,
                        Max = data.NumberOfEntries
                    };
                    self.Tell(replayState);
                }
                else
                    self.Tell(NullStats.Instance);
            }, e =>
            {
                var context = Context;
                context.System.Log.Error(e.ToString());
            }, _replayTopic.ClientConfigurationData.ServiceUrl, l =>
            {
                var context = Context;
                context.System.Log.Info(l);
            }));
        }
        
        private void Active()
        {
            Receive<ConsumedMessage>(c =>
            {
                var messageId = (MessageId)c.Message.MessageId;
                if (!_replayTopic.Filtered)
                {
                    var eventMessage = new EventMessage(c.Message, _sequenceId, messageId.LedgerId, messageId.EntryId);
                    _pulsarManager.Tell(eventMessage);
                }
                else
                {
                    var props = c.Message.Properties;
                    if (props.ContainsKey(_filter.Key))
                    {
                        var value = props[_filter.Key];
                        if (value.Equals(_filter.Value, StringComparison.OrdinalIgnoreCase))
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
            Receive<GetNumberOfEntries>(g =>
            {
                _topicName = g.TopicName;
                Become(GetStats);
            });

        }

        private void NextPlayStats(NextPlay play)
        {
            _sequenceId = play.From;
            Receive<ReplayState>(r =>
            {
                _consumer.Tell(new SendFlow(r.Max));
                Become(Active);
                Stash.UnstashAll();
            });
            Receive<NullStats>(r =>
            {
                Become(Active);
                Stash.UnstashAll();
            });
            ReceiveAny(_ => Stash.Stash());
            _pulsarSystem.PulsarAdmin(new InternalCommands.Admin(AdminCommands.GetInternalStatsPersistent, new object[] { _topicName.Tenant, _topicName.Namespace, _topicName.ToString().Split("/").Last(), false }, e =>
            {
                var self = Self;
                if (e != null)
                {
                    var data = (PersistentTopicInternalStats)e;
                    var compute = new ComputeMessageId(data, play.From, play.To, play.Max);
                    var result = compute.GetFrom();
                    var replayState = new ReplayState
                    {
                        LedgerId = result.Ledger,
                        EntryId = result.Entry,
                        To = data.NumberOfEntries,
                        Max = result.NumberOfEntries
                    };
                    self.Tell(replayState);
                }
                else
                    self.Tell(NullStats.Instance);
            }, e =>
            {
                var context = Context;
                context.System.Log.Error(e.ToString());
            }, _replayTopic.ClientConfigurationData.ServiceUrl, l =>
            {
                var context = Context;
                context.System.Log.Info(l);
            }));
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
