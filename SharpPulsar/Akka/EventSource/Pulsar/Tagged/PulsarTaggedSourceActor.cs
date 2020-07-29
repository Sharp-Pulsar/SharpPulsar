using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Akka.Actor;
using PulsarAdmin;
using SharpPulsar.Akka.EventSource.Messages.Pulsar;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.EventSource.Pulsar.Tagged
{
    public class PulsarTaggedSourceActor : ReceiveActor
    {
        private readonly IActorRef _pulsarManager;
        private readonly EventMessageId _endId;
        private readonly IActorRef _child;
        private EventMessageId _lastEventMessageId;
        private ICancelable _flowSenderCancelable;
        private readonly HttpClient _httpClient;
        private readonly IPulsarEventSourceMessage _message;
        private readonly TopicName _topicName;
        private readonly IAdvancedScheduler _scheduler;
        private readonly Tag _tag;
        private long _sequenceId;
        public PulsarTaggedSourceActor(ClientConfigurationData client, ConsumerConfigurationData configuration, IActorRef pulsarManager, IActorRef network, EventMessageId endId, bool isLive, HttpClient httpClient, IPulsarEventSourceMessage message, Tag tag, long fromSequenceId)
        {
            _sequenceId = fromSequenceId;
            _scheduler = Context.System.Scheduler.Advanced;
            _topicName = TopicName.Get(message.Topic);
            _httpClient = httpClient;
            _message = message;
            _tag = tag;
            _endId = endId;
            _lastEventMessageId = endId;
            var topicName = TopicName.Get(configuration.SingleTopic);
            _pulsarManager = pulsarManager;
            _child = Context.ActorOf(Consumer.Consumer.Prop(client, topicName.ToString(), configuration, Interlocked.Increment(ref IdGenerators.ConsumerId), network, true, topicName.PartitionIndex, SubscriptionMode.NonDurable, null, _pulsarManager, true));
            if (isLive)
                LiveConsume();
            else Consume();
        }

        private void Consume()
        {
            Receive<ConsumedMessage>(c =>
            {
                var messageId = (MessageId)c.Message.MessageId;
                if (messageId.LedgerId <= _endId.LedgerId && messageId.EntryId <= _endId.EntryId)
                {
                    var props = c.Message.Properties;
                    var tagged = props.Any(x => x.Key.Equals(_tag.Key, StringComparison.OrdinalIgnoreCase)
                                                && x.Value.Contains(_tag.Value, StringComparison.OrdinalIgnoreCase));
                    if (tagged)
                    {
                        var eventMessage = new EventMessage(c.Message, _sequenceId);
                        _pulsarManager.Tell(eventMessage);
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
            Receive<ConsumedMessage>(c =>
            {
                var messageId = (MessageId)c.Message.MessageId;
                var props = c.Message.Properties;
                var tagged = props.Any(x => x.Key.Equals(_tag.Key, StringComparison.OrdinalIgnoreCase)
                                            && x.Value.Contains(_tag.Value, StringComparison.OrdinalIgnoreCase));
                if (tagged)
                {
                    var eventMessage = new EventMessage(c.Message, _sequenceId);
                    _pulsarManager.Tell(eventMessage);
                }

                _sequenceId++;
            });
            _flowSenderCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(60), SendFlow);
        }

        private void SendFlow()
        {
            try
            {
                var adminRestapi = new PulsarAdminRESTAPI(_message.AdminUrl, _httpClient, true);
                var stats = adminRestapi.GetInternalStats1(_topicName.NamespaceObject.Tenant,
                    _topicName.NamespaceObject.LocalName, _topicName.LocalName);
                var start = MessageIdHelper.NextFlow(stats);
                if (start.Index > _lastEventMessageId.Index)
                {
                    var permits = start.Index - _lastEventMessageId.Index;
                    _child.Tell(new SendFlow(permits));
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

        public static Props Prop(ClientConfigurationData client, ConsumerConfigurationData configuration, IActorRef pulsarManager, IActorRef network, EventMessageId endId, bool isLive, HttpClient httpClient, IPulsarEventSourceMessage message, Tag tag, long fromSequenceId)
        {
            return Props.Create(() => new PulsarTaggedSourceActor(client, configuration, pulsarManager, network, endId, isLive, httpClient, message, tag, fromSequenceId));
        }
        public IStash Stash { get; set; }
    }

}
