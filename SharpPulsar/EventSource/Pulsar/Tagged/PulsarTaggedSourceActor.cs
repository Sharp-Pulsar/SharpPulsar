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

namespace SharpPulsar.EventSource.Pulsar.Tagged
{
    public class PulsarTaggedSourceActor<T> : ReceiveActor
    {
        private readonly IActorRef _pulsarManager;
        private readonly EventMessageId _endId;
        private readonly IActorRef _child;
        private EventMessageId _lastEventMessageId;
        private ICancelable _flowSenderCancelable;
        private readonly HttpClient _httpClient;
        private readonly IPulsarEventSourceMessage<T> _message;
        private readonly TopicName _topicName;
        private readonly IAdvancedScheduler _scheduler;
        private readonly Tag _tag;
        private long _sequenceId;
        public PulsarTaggedSourceActor(ClientConfigurationData client, ReaderConfigurationData<T> readerConfiguration, IActorRef clientActor, IActorRef lookup, IActorRef cnxPool, IActorRef generator, EventMessageId endId, bool isLive, HttpClient httpClient, IPulsarEventSourceMessage<T> message, Tag tag, long fromSequenceId, ISchema<T> schema)
        {
            _sequenceId = fromSequenceId;
            _scheduler = Context.System.Scheduler.Advanced;
            _topicName = TopicName.Get(message.Topic);
            _httpClient = httpClient;
            _message = message;
            _tag = tag;
            _endId = endId;
            _lastEventMessageId = endId;

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
                var messageId = (MessageId)c.MessageId;
                if (messageId.LedgerId <= _endId.LedgerId && messageId.EntryId <= _endId.EntryId)
                {
                    var props = c.Properties;
                    var tagged = props.FirstOrDefault(x => x.Key.Equals(_tag.Key, StringComparison.OrdinalIgnoreCase) && x.Value.Equals(_tag.Value, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrWhiteSpace(tagged.Value))
                    {
                        Context.Parent.Tell(c);
                    }
                    _sequenceId++;
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

                _sequenceId++;
            });
            _flowSenderCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(60), SendFlow);
        }

        private void SendFlow()
        {
            try
            {
                var adminRestapi = new User.Admin(_message.AdminUrl, _httpClient);
                var stats = adminRestapi.GetInternalStats(_topicName.NamespaceObject.Tenant,
                    _topicName.NamespaceObject.LocalName, _topicName.LocalName);
                var start = MessageIdHelper.NextFlow(stats.Body);
                if (start.Index > _lastEventMessageId.Index)
                {
                    var permits = start.Index - _lastEventMessageId.Index;
                    _child.Tell(new IncreaseAvailablePermits((int)permits));
                    _lastEventMessageId = new EventMessageId(start.Ledger, start.Entry, start.Index);
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

        public static Props Prop(ClientConfigurationData client, ReaderConfigurationData<T> readerConfiguration, IActorRef clientActor, IActorRef lookup, IActorRef cnxPool, IActorRef generator, EventMessageId endId, bool isLive, HttpClient httpClient, IPulsarEventSourceMessage<T> message, Tag tag, long fromSequenceId, ISchema<T> schema)
        {
            return Props.Create(() => new PulsarTaggedSourceActor<T>(client, readerConfiguration, clientActor, lookup, cnxPool, generator, endId, isLive, httpClient, message, tag, fromSequenceId, schema));
        }
        public IStash Stash { get; set; }
    }

}
