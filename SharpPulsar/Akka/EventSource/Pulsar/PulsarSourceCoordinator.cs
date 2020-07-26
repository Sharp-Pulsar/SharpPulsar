using System.Net.Http;
using System.Text.RegularExpressions;
using Akka.Actor;
using SharpPulsar.Akka.EventSource.Messages.Pulsar;
using SharpPulsar.Akka.EventSource.Pulsar.Tagged;

namespace SharpPulsar.Akka.EventSource.Pulsar
{
    public class PulsarSourceCoordinator: ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        private readonly HttpClient _httpClient;


        public PulsarSourceCoordinator(IActorRef network, IActorRef pulsarManager)
        {
            _httpClient = new HttpClient();
            _network = network;
            _pulsarManager = pulsarManager;
            Receive<IPulsarEventSourceMessage>(Handle);
            
        }

        public static Props Prop(IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(()=> new PulsarSourceCoordinator(network, pulsarManager));
        }
        public IStash Stash { get; set; }

        private void Handle(IPulsarEventSourceMessage message)
        {
            var ns = Regex.Replace(message.Topic, @"[^\w\d]", "");
            var child = Context.Child(ns);
            if (!child.IsNobody())
                return;
            switch (message)
            {
                case CurrentEventsByTopic byTopic:
                    Context.ActorOf(CurrentEventsByTopicActor.Prop(byTopic, _httpClient, _network, _pulsarManager), ns);
                    break;
                case EventsByTopic top:
                    Context.ActorOf(EventsByTopicActor.Prop(top, _httpClient, _network, _pulsarManager), ns);
                    break;
                case CurrentEventsByTag cTag:
                    Context.ActorOf(CurrentEventsByTagActor.Prop(cTag, _httpClient, _network, _pulsarManager), ns);
                    break;
                case EventsByTag tag:
                    Context.ActorOf(EventsByTagActor.Prop(tag, _httpClient, _network, _pulsarManager), ns);
                    break;
            }
        }
    }

    public sealed class EventMessageId
    {
        public EventMessageId(long ledgerId, long entryId, long index)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
            Index = index;
        }

        public long LedgerId { get; }
        public long EntryId { get; }
        public long Index { get; }
    }
}
