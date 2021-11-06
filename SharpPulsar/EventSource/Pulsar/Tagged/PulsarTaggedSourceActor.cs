using System;
using System.Linq;
using System.Net.Http;
using Akka.Actor;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.EventSource.Messages.Pulsar;
using SharpPulsar.Interfaces;
using SharpPulsar.Common;
using SharpPulsar.Utility;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Utils;

namespace SharpPulsar.EventSource.Pulsar.Tagged
{
    public class PulsarTaggedSourceActor<T> : ReceiveActor
    {
        private readonly IActorRef _pulsarManager;
        private readonly IActorRef _child;
        private ICancelable _flowSenderCancelable;
        private readonly IAdvancedScheduler _scheduler;
        private readonly Tag _tag;
        private readonly long _toOffset;
        private long _currentOffset;
        private long _totalOffset;
        private long _lastEventMessageOffset;
        public PulsarTaggedSourceActor(ClientConfigurationData client, ReaderConfigurationData<T> readerConfiguration, IActorRef clientActor, IActorRef lookup, IActorRef cnxPool, IActorRef generator, long fromOffset, long toOffset, bool isLive, Tag tag, ISchema<T> schema)
        {
            _scheduler = Context.System.Scheduler.Advanced;
            _tag = tag;
            _toOffset = toOffset;
            _totalOffset = toOffset - fromOffset;
            _lastEventMessageOffset = fromOffset;
            var topicName = TopicName.Get(readerConfiguration.TopicName);
            IActorRef stateA = Context.ActorOf(Props.Create(() => new ConsumerStateActor()), $"StateActor{Guid.NewGuid()}");
            var subscription = "player-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
            if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
            {
                subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
            }

            ConsumerConfigurationData<T> consumerConfiguration = new ConsumerConfigurationData<T>();
            consumerConfiguration.TopicNames.Add(readerConfiguration.TopicName);
            consumerConfiguration.SubscriptionName = subscription;
            consumerConfiguration.SubscriptionType = SubType.Exclusive;
            consumerConfiguration.SubscriptionMode = SubscriptionMode.NonDurable;
            consumerConfiguration.ReceiverQueueSize = readerConfiguration.ReceiverQueueSize;
            consumerConfiguration.ReadCompacted = readerConfiguration.ReadCompacted;
            consumerConfiguration.StartMessageId = readerConfiguration.StartMessageId;

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

            if (readerConfiguration.KeyHashRanges != null)
            {
                consumerConfiguration.KeySharedPolicy = KeySharedPolicy.StickyHashRange().GetRanges(readerConfiguration.KeyHashRanges.ToArray());
            }

            var partitionIdx = TopicName.GetPartitionIndex(readerConfiguration.TopicName);
            var consumerId = generator.Ask<long>(NewConsumerId.Instance).GetAwaiter().GetResult();
            _child = Context.ActorOf(Props.Create(() => new ConsumerActor<T>(consumerId, stateA, clientActor, lookup, cnxPool, generator, readerConfiguration.TopicName, consumerConfiguration, Context.System.Scheduler.Advanced, partitionIdx, true, readerConfiguration.StartMessageId, readerConfiguration.StartMessageFromRollbackDurationInSec, schema, true, client)));
            _child.Tell(Connect.Instance);
            Receive<ICumulative>(m => {
                _child.Tell(m);
            });
            Receive<IAcknowledge>(m => {
                _child.Tell(m);
            });
            Receive<MessageProcessed<T>>(m => {
                _child.Tell(m);
            });
            if (isLive)
                LiveConsume();
            else Consume();
        }

        private void Consume()
        {
            Receive<ReceivedMessage<T>>(m =>
            {
                var c = m.Message;
                var messageId = MessageIdUtils.GetOffset(m.Message.MessageId);
                if (messageId <= _toOffset)
                {
                    var props = c.Properties;
                    var tagged = props.FirstOrDefault(x => x.Key.Equals(_tag.Key, StringComparison.OrdinalIgnoreCase) && x.Value.Equals(_tag.Value, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrWhiteSpace(tagged.Value))
                    {
                        Context.Parent.Tell(c);
                    }
                    //_sequenceId++;
                }
                else Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
            Receive<ReceiveTimeout>(t => { Self.GracefulStop(TimeSpan.FromSeconds(5)); });
            //to track last sequence id for lagging player
            Context.SetReceiveTimeout(TimeSpan.FromSeconds(30));
        }
        private void LiveConsume()
        {
            Receive<ReceivedMessage<T>>(c =>
            {
                var props = c.Message.Properties;
                var tagged = props.FirstOrDefault(x => x.Key.Equals(_tag.Key, StringComparison.OrdinalIgnoreCase) && x.Value.Equals(_tag.Value, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(tagged.Value))
                {
                    Context.Parent.Tell(c);
                }
                _currentOffset = MessageIdUtils.GetOffset(c.Message.MessageId);
                //_sequenceId++;
            });
            _flowSenderCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(60), SendFlow);
        }

        private void SendFlow()
        {
            try
            {
                //STILL NOT CLEAR WHAT TO DO HERE
                if ((_currentOffset - _lastEventMessageOffset) <= _totalOffset)
                {
                    _child.Tell(new IncreaseAvailablePermits((int)_totalOffset));
                    _lastEventMessageOffset = _currentOffset;
                }
            }
            finally
            {
                _flowSenderCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(5), SendFlow);
            }
        }
        protected override void Unhandled(object message)
        {
            //Since we have saved the last consumed sequence id before the timeout,
            //we can discard any Messages, they will be replayed after all, from the last saved sequence id
        }

        protected override void PostStop()
        {
            _flowSenderCancelable?.Cancel();
        }

        public static Props Prop(ClientConfigurationData client, ReaderConfigurationData<T> readerConfiguration, IActorRef clientActor, IActorRef lookup, IActorRef cnxPool, IActorRef generator, long fromOffset, long toOffset, bool isLive, Tag tag, ISchema<T> schema)
        {
            return Props.Create(() => new PulsarTaggedSourceActor<T>(client, readerConfiguration, clientActor, lookup, cnxPool, generator, fromOffset, toOffset, isLive, tag, schema));
        }
        public IStash Stash { get; set; }
    }

}
