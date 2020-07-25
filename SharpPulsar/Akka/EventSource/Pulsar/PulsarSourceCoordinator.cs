using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using Nito.AsyncEx;
using PulsarAdmin;
using PulsarAdmin.Models;
using SharpPulsar.Akka.EventSource.Messages;
using SharpPulsar.Akka.EventSource.Messages.Presto;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;
using TopicEntries = SharpPulsar.Akka.InternalCommands.Consumer.TopicEntries;

namespace SharpPulsar.Akka.EventSource.Pulsar
{
    public class PulsarSourceCoordinator: ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        private readonly List<string> _routees;
        private readonly HttpClient _httpClient;


        public PulsarSourceCoordinator(IActorRef network, IActorRef pulsarManager)
        {
            _httpClient = new HttpClient();
            _routees = new List<string>();
            _network = network;
            _pulsarManager = pulsarManager;
            Context.ActorOf(PulsarTaggedAggregateRoot.Prop(network, pulsarManager), "Tagged");
            Receive<CurrentEventsByTopic>(c => { });
            Receive<EventsByTopic>(c => { });
            Receive<CurrentEventsByTag>(c => { });
            Receive<EventsByTag>(c => { });
           
            Become(Listening);
        }

        private void Listening()
        {

            Receive<GetNumberOfEntries>(g =>
            {
                if (g.Topic.EndsWith("*"))
                    Context.Child("PulsarTagged").Tell(g);
                else
                {
                    var topicName = TopicName.Get(g.Topic);
                    var adminRestapi = new PulsarAdminRESTAPI(g.Server, _httpClient, true);
                    var data = adminRestapi.GetInternalStats1(topicName.NamespaceObject.Tenant,
                        topicName.NamespaceObject.LocalName, topicName.LocalName, false);
                    var entry = data != null ? data.NumberOfEntries : 0L;
                    _pulsarManager.Tell(new TopicEntries(g.Topic, entry, entry, 1));
                }
            });
            Receive<NextPlay>(n =>
            {
                if (n.Tagged)
                {
                    Context.Child("PulsarTagged").Tell(n);
                }
                else
                {
                    var topic = Regex.Replace(n.Topic, @"[^\w\d]", "");
                    var child = Context.Child(topic);
                    if (!child.IsNobody())
                        child.Tell(n);
                    else
                        Context.System.Log.Warning($"[NextPlay] '{n.Topic}' does not have a DJ - request for one first with 'StartReplayTopic' and a DJ will be yours! ;)");
                }
            });

            Receive<StartReplayTopic>(s =>
            {
                var regexTopic = Regex.Replace(s.ReaderConfigurationData.TopicName, @"[^\w\d]", "");

                var child = Context.Child(regexTopic);
                if (s.Tagged)
                {
                    Context.Child("Tagged").Tell(s);
                }
                else
                {
                    if (child.IsNobody())
                    {
                        Become(() => Start(s));
                    }
                }
            });
        }

        private void Start(StartReplayTopic replayTopic)
        {
            Receive<Partitions>(p =>
            {
                var regexTopic = Regex.Replace(replayTopic.ReaderConfigurationData.TopicName, @"[^\w\d]", "");
                if (p.Partition > 0)
                {
                    for (var i = 0; i < p.Partition; i++)
                    {
                        var replay = replayTopic;
                        var partitionName = TopicName.Get(p.Topic).GetPartition(i).ToString();
                        replay.ReaderConfigurationData.TopicName = partitionName;
                        var routee = Context.ActorOf(PulsarSourceActor.Prop(replay, _pulsarManager, _network));
                        _routees.Add(routee.Path.ToString());
                    }
                }
                else
                {
                    var replay = replayTopic;
                    replay.ReaderConfigurationData.TopicName = p.Topic;
                    var routee = Context.ActorOf(PulsarSourceActor.Prop(replay, _pulsarManager, _network));
                    _routees.Add(routee.Path.ToString());
                }
                Context.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(_routees)), regexTopic);
                _routees.Clear();
                Become(Listening);
                Stash.UnstashAll();
            });
            NewPartitionMetadataRequest(replayTopic.ReaderConfigurationData.TopicName);
        }

        private Partitions NewPartitionMetadataRequest(string topic)
        {
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewPartitionMetadataRequest(topic, requestId);
            var pay = new Payload(request, requestId, "CommandPartitionedTopicMetadata", topic);
            var ask = _network.Ask<Partitions>(pay);
            return SynchronizationContextSwitcher.NoContext(async () => await ask).Result;
        }
        
        public static Props Prop(IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(()=> new PulsarSourceCoordinator(network, pulsarManager));
        }
        public IStash Stash { get; set; }
    }

    public class CurrentEventsByTopicActor : ReceiveActor
    {
        private readonly IEventSourceMessage _message;
        private readonly HttpClient _httpClient;
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        private PersistentTopicInternalStats _persistentTopicInternalStats;
        private EventMessageId _startMessageId;
        private EventMessageId _endMessageId;
        public CurrentEventsByTopicActor(IEventSourceMessage message, HttpClient httpClient, IActorRef network, IActorRef pulsarManager)
        {
            _message = message;
            _httpClient = httpClient;
            _network = network;
            _pulsarManager = pulsarManager;
        }
        private Partitions NewPartitionMetadataRequest(string topic)
        {
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewPartitionMetadataRequest(topic, requestId);
            var pay = new Payload(request, requestId, "CommandPartitionedTopicMetadata", topic);
            var ask = _network.Ask<Partitions>(pay);
            return SynchronizationContextSwitcher.NoContext(async () => await ask).Result;
        }
        protected override void PreStart()
        {
            var adminRestapi = new PulsarAdminRESTAPI(_message.AdminUrl, _httpClient, true);
            _persistentTopicInternalStats = adminRestapi.GetInternalStats1(_message.Tenant,_message.Namespace, _message.Topic);
            var start = MessageIdHelper.Calculate(_message.FromSequenceId, _persistentTopicInternalStats);
            _startMessageId = new EventMessageId(start.Ledger, start.Entry, start.Index);
            var end = MessageIdHelper.Calculate(_message.ToSequenceId, _persistentTopicInternalStats);
            _endMessageId = new EventMessageId(end.Ledger, end.Entry, end.Index);
        }

        public static Props Prop()
        {
            return Props.Create(()=> new CurrentEventsByTopicActor());
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
