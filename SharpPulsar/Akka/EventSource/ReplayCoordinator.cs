using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using PulsarAdmin;
using PulsarAdmin.Models;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;
using TopicEntries = SharpPulsar.Akka.InternalCommands.Consumer.TopicEntries;

namespace SharpPulsar.Akka.EventSource
{
    public class ReplayCoordinator: ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        private readonly List<string> _routees;
        private readonly IActorRef _self;
        

        public ReplayCoordinator(IActorRef network, IActorRef pulsarManager)
        {
            _self = Self;
            _routees = new List<string>();
            _network = network;
            _pulsarManager = pulsarManager;
            Context.ActorOf(TaggedCoordinator.Prop(network, pulsarManager), "Tagged");
           
            Become(Listening);
        }

        private void Listening()
        {

            Receive<GetNumberOfEntries>(g =>
            {
                if (g.Topic.EndsWith("*"))
                    Context.Child("Tagged").Tell(g);
                else
                {
                    var topicName = TopicName.Get(g.Topic);
                    var adminRestapi = new PulsarAdminRESTAPI(g.Server, new HttpClient(), true);
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
                    Context.Child("Tagged").Tell(n);
                }
                else
                {
                    var topic = Regex.Replace(n.Topic, @"[^\w\d]", "");
                    var child = Context.Child(topic);
                    if (!child.IsNobody())
                        child.Tell(n);
                    else
                        Console.WriteLine($"[NextPlay] '{n.Topic}' does not have a DJ - request for one first with 'StartReplayTopic' and a DJ will be yours! ;)");
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
                        var routee = Context.ActorOf(TopicReplayActor.Prop(replay, _pulsarManager, _network));
                        _routees.Add(routee.Path.ToString());
                    }
                }
                else
                {
                    var replay = replayTopic;
                    replay.ReaderConfigurationData.TopicName = p.Topic;
                    var routee = Context.ActorOf(TopicReplayActor.Prop(replay, _pulsarManager, _network));
                    _routees.Add(routee.Path.ToString());
                }
                Context.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(_routees)), regexTopic);
                _routees.Clear();
                Become(Listening);
                Stash.UnstashAll();
            });
            NewPartitionMetadataRequest(replayTopic.ReaderConfigurationData.TopicName);
        }

        private void NewPartitionMetadataRequest(string topic)
        {
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewPartitionMetadataRequest(topic, requestId);
            var pay = new Payload(request, requestId, "CommandPartitionedTopicMetadata", topic);
            _network.Tell(pay);
        }
        
        public static Props Prop(IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(()=> new ReplayCoordinator(network, pulsarManager));
        }
        public IStash Stash { get; set; }
    }

}
