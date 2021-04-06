using System;
using System.Net.Http;
using System.Threading;
using Akka.Actor;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;

namespace SharpPulsar.EventSource.Pulsar
{
    public class PulsarSourceActor : ReceiveActor
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
        private long _sequenceId;
        public PulsarSourceActor(ClientConfigurationData client, ConsumerConfigurationData configuration, IActorRef pulsarManager, IActorRef network, EventMessageId endId, bool isLive, HttpClient httpClient, IPulsarEventSourceMessage message, long fromSequenceId)
        {
            _sequenceId = fromSequenceId;
            _scheduler = Context.System.Scheduler.Advanced;
            _topicName = TopicName.Get(message.Topic);
            _httpClient = httpClient;
            _message = message;
            _endId = endId;
            _lastEventMessageId = endId;
            var topicName = TopicName.Get(configuration.SingleTopic);
            _pulsarManager = pulsarManager;
            _child = Context.ActorOf(Consumer.Consumer.Prop(client, topicName.ToString(), configuration, Interlocked.Increment(ref IdGenerators.ConsumerId), network, true, topicName.PartitionIndex, SubscriptionMode.NonDurable, null, _pulsarManager, true));
            if(isLive)
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
                    var eventMessage = new EventMessage(c.Message, _sequenceId);
                    _pulsarManager.Tell(eventMessage);
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
                var eventMessage = new EventMessage(c.Message, _sequenceId);
                _pulsarManager.Tell(eventMessage);
                _sequenceId++;
            });
            _flowSenderCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(60), SendFlow);
        }

        private void SendFlow()
        {
            try
            {
                var adminRestapi = new PulsarAdminRESTAPI(_message.AdminUrl, _httpClient, true);
                var statsTask = adminRestapi.GetInternalStats1Async(_topicName.NamespaceObject.Tenant,
                    _topicName.NamespaceObject.LocalName, _topicName.LocalName);
                var stats = SynchronizationContextSwitcher.NoContext(async () => await statsTask).Result;
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

        public static Props Prop(ClientConfigurationData client, ConsumerConfigurationData configuration, IActorRef pulsarManager, IActorRef network, EventMessageId endId, bool isLive, HttpClient httpClient, IPulsarEventSourceMessage message, long fromSequence)
        {
            return Props.Create(()=> new PulsarSourceActor(client, configuration, pulsarManager, network, endId, isLive, httpClient, message, fromSequence));
        }
        public IStash Stash { get; set; }
    }

}
