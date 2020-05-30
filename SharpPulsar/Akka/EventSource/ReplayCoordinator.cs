using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using PulsarAdmin.Models;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;

namespace SharpPulsar.Akka.EventSource
{
    public class ReplayCoordinator: ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        private readonly PulsarSystem _pulsarSystem;
        private readonly List<string> _routees;
        private readonly IActorRef _self;
        

        public ReplayCoordinator(IActorRef network, IActorRef pulsarManager, PulsarSystem pulsarSystem)
        {
            _self = Self;
            _routees = new List<string>();
            _network = network;
            _pulsarManager = pulsarManager;
            _pulsarSystem = pulsarSystem;
            Context.ActorOf(TaggedCoordinator.Prop(network, pulsarManager, pulsarSystem), "Tagged");
           
            Become(Listening);
        }

        private void Listening()
        {

            Receive<GetNumberOfEntries>(g =>
            {
                if (g.Topic.EndsWith("*"))
                    Context.Child("Tagged").Tell(g);
                else 
                    Become(()=>GetStats(g));
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
                        Context.System.Log.Info($"[NextPlay] '{n.Topic}' does not have a DJ - request for one first with 'StartReplayTopic' and a DJ will be yours! ;)");
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
                        var routee = Context.ActorOf(TopicReplayActor.Prop(_pulsarSystem, replay, _pulsarManager, _network));
                        _routees.Add(routee.Path.ToString());
                    }
                }
                else
                {
                    var replay = replayTopic;
                    replay.ReaderConfigurationData.TopicName = p.Topic;
                    var routee = Context.ActorOf(TopicReplayActor.Prop(_pulsarSystem, replay, _pulsarManager, _network));
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
        private void GetStats(GetNumberOfEntries numberOfEntries)
        {
            var topicName = TopicName.Get(numberOfEntries.Topic);
            Receive<ReplayState>(r =>
            {
                _pulsarManager.Tell(new NumberOfEntries(topicName.ToString(), r.Max));
                Become(Listening);
                Stash.UnstashAll();
            });
            Receive<NullStats>(r =>
            {
                Become(Listening);
                Stash.UnstashAll();
            });
            ReceiveAny(_ => Stash.Stash());
            _pulsarSystem.PulsarAdmin(new InternalCommands.Admin(AdminCommands.GetInternalStatsPersistent, new object[] { topicName.NamespaceObject.Tenant, topicName.NamespaceObject.LocalName, topicName.LocalName, false }, e =>
            {
                if (e != null)
                {
                    var data = (PersistentTopicInternalStats)e;
                    var compute = new ComputeMessageId(data, numberOfEntries.From, numberOfEntries.To, numberOfEntries.Max);
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
            }, numberOfEntries.Server, l =>
            {
                var context = Context;
                context.System.Log.Info(l);
            }));
        }

        public static Props Prop(IActorRef network, IActorRef pulsarManager, PulsarSystem pulsarSystem)
        {
            return Props.Create(()=> new ReplayCoordinator(network, pulsarManager, pulsarSystem));
        }
        public IStash Stash { get; set; }
    }
}
