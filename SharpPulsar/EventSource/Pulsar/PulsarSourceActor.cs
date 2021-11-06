using System;
using System.Linq;
using System.Net.Http;
using Akka.Actor;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.EventSource.Messages.Pulsar;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Utility;
using SharpPulsar.Utils;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

namespace SharpPulsar.EventSource.Pulsar
{
    public class PulsarSourceActor<T> : ReceiveActor
    {
        private readonly long _toOffset;
        private long _currentOffset;
        private long _totalOffset;
        private long _lastEventMessageOffset;
        private readonly IActorRef _child;
        private ICancelable _flowSenderCancelable;
        private readonly IAdvancedScheduler _scheduler;
        public PulsarSourceActor(ClientConfigurationData client, ReaderConfigurationData<T> readerConfiguration, IActorRef clientActor, IActorRef lookup, IActorRef cnxPool, IActorRef generator, long fromOffset, long toOffset, bool isLive, ISchema<T> schema)
        {
            _scheduler = Context.System.Scheduler.Advanced;
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
                    Context.Parent.Tell(c);
                    _child.Tell(new AcknowledgeMessage<T>(c));
                    _child.Tell(new MessageProcessed<T>(c));
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
                Context.Parent.Tell(c);
                _currentOffset = MessageIdUtils.GetOffset(c.Message.MessageId);
            });
            _flowSenderCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(60), SendFlow);
        }

        private void SendFlow()
        {
            try
            {
                var flow = _currentOffset - _lastEventMessageOffset;
                if (flow > 0)
                {
                    _child.Tell(new IncreaseAvailablePermits((int)flow));
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

        public static Props Prop(ClientConfigurationData client, ReaderConfigurationData<T> readerConfiguration, IActorRef clientActor, IActorRef lookup, IActorRef cnxPool, IActorRef generator, long fromOffset, long toOffset, bool isLive, ISchema<T> schema)
        {
            return Props.Create(()=> new PulsarSourceActor<T>(client, readerConfiguration, clientActor, lookup, cnxPool, generator, fromOffset, toOffset, isLive, schema));
        }
        public IStash Stash { get; set; }
    }

}
