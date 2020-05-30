using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using PulsarAdmin.Models;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility;

namespace SharpPulsar.Akka.EventSource
{
    public class TaggedCoordinator:ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        private readonly PulsarSystem _pulsarSystem;
        private readonly List<string> _routees;
        private int _expectedRoutees;
        private int _currentRoutees;
        private readonly IActorRef _self;
        private int _expectedStatsCount;
        private int _currentStatsCounts;
        private long? _maxPatternEntries = 0;
        private readonly Dictionary<string, long> _sequenceIds;
        public TaggedCoordinator(IActorRef network, IActorRef pulsarManager, PulsarSystem pulsarSystem)
        {
            _sequenceIds = new Dictionary<string, long>();
            _self = Self;
            _routees = new List<string>();
            _network = network;
            _pulsarManager = pulsarManager;
            _pulsarSystem = pulsarSystem;
            Become(Listening);
        }

        private void GetStats(GetNumberOfEntries numberOfEntries)
        {
            Receive<NamespaceTopics>(t =>
            {
                var topics = TopicsPatternFilter(t.Topics, new Regex(numberOfEntries.Topic));
                _expectedStatsCount = topics.Count;
                foreach (var topic in topics)
                {
                    var topicName = TopicName.Get(topic);
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

            });
            Receive<ReplayState>(r =>
            {
                _currentStatsCounts++;
                if (r.Max > _maxPatternEntries)
                    _maxPatternEntries = r.Max;
                if (_currentStatsCounts == _expectedStatsCount)
                {
                    _pulsarManager.Tell(new NumberOfEntries(numberOfEntries.Topic, _maxPatternEntries.Value));
                    _currentStatsCounts = 0;
                    _expectedStatsCount = 0;
                    _maxPatternEntries = 0;
                    Become(Listening);
                    Stash.UnstashAll();
                }
            });
            Receive<NullStats>(r =>
            {
                _currentStatsCounts++;
                if (_currentStatsCounts == _expectedStatsCount)
                {
                    _pulsarManager.Tell(new NumberOfEntries(numberOfEntries.Topic, _maxPatternEntries.Value));
                    _currentStatsCounts = 0;
                    _expectedStatsCount = 0;
                    _maxPatternEntries = 0;
                    Become(Listening);
                    Stash.UnstashAll();
                }
            });
            ReceiveAny(_ => Stash.Stash());
            NewGetTopicsOfNamespaceRequest(new Regex(numberOfEntries.Topic));
        }

        private void Start(StartReplayTopic replayTopic)
        {
            Receive<NamespaceTopics>(t =>
            {
                var topics = TopicsPatternFilter(t.Topics, new Regex(replayTopic.ReaderConfigurationData.TopicName));
                _expectedRoutees = topics.Count;
                foreach (var topic in topics)
                {
                    NewPartitionMetadataRequest(topic);
                }

            });
            Receive<Partitions>(p =>
            {
                _currentRoutees++;
                if (p.Partition > 0)
                {
                    for (var i = 0; i < p.Partition; i++)
                    {
                        var partitionName = TopicName.Get(p.Topic).GetPartition(i).ToString();

                        var name = Regex.Replace(partitionName, @"[^\w\d]", "");
                        var replay = new StartReplayTopic(replayTopic.ClientConfigurationData, PrepareReaderConfigurationData(replayTopic.ReaderConfigurationData, partitionName), replayTopic.AdminUrl, replayTopic.From, replayTopic.To, replayTopic.Max, replayTopic.Tag, replayTopic.Tagged);
                        var routee = Context.ActorOf(TopicReplayActor.Prop(_pulsarSystem, replay, _pulsarManager, _network), name);
                        _routees.Add(routee.Path.ToString());
                    }
                }
                else
                {
                    var replay = new StartReplayTopic(replayTopic.ClientConfigurationData, PrepareReaderConfigurationData(replayTopic.ReaderConfigurationData, p.Topic), replayTopic.AdminUrl, replayTopic.From, replayTopic.To, replayTopic.Max, replayTopic.Tag, replayTopic.Tagged);

                    var name = Regex.Replace(p.Topic, @"[^\w\d]", "");
                    var routee = Context.ActorOf(TopicReplayActor.Prop(_pulsarSystem, replay, _pulsarManager, _network), name);
                    _routees.Add(routee.Path.ToString());
                }

                if (_currentRoutees == _expectedRoutees)
                {
                    var regexTopic = $"tagged-{Regex.Replace(replayTopic.ReaderConfigurationData.TopicName, @"[^\w\d]", "")}";
                    Context.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(_routees)), regexTopic);
                    _routees.Clear();
                    _expectedRoutees = 0;
                    _currentRoutees = 0;
                    Become(Listening);
                    Stash.UnstashAll();
                }
            });
            NewGetTopicsOfNamespaceRequest(new Regex(replayTopic.ReaderConfigurationData.TopicName));
        }
        private ReaderConfigurationData PrepareReaderConfigurationData(ReaderConfigurationData readerConfiguration, string topic)
        {
            return new ReaderConfigurationData
            {
                TopicName = topic,
                ReaderName = readerConfiguration.ReaderName,
                ReceiverQueueSize = readerConfiguration.ReceiverQueueSize,
                ReadCompacted = readerConfiguration.ReadCompacted,
                Schema = readerConfiguration.Schema,
                EventListener = readerConfiguration.EventListener,
                ResetIncludeHead = readerConfiguration.ResetIncludeHead,
                CryptoFailureAction = readerConfiguration.CryptoFailureAction,
                CryptoKeyReader = readerConfiguration.CryptoKeyReader,
                SubscriptionRolePrefix = readerConfiguration.SubscriptionRolePrefix,
                StartMessageId = readerConfiguration.StartMessageId,
                StartMessageFromRollbackDurationInSec = readerConfiguration.StartMessageFromRollbackDurationInSec
            };
        }

        private void Listening()
        {
            Receive<IEventMessage>(c =>
            {
                _pulsarManager.Tell(c);
            });
            Receive<GetNumberOfEntries>(g =>
            {
                Become(() => GetStats(g));
            });
            Receive<NextPlay>(n =>
            {
                var topic = Regex.Replace(n.Topic, @"[^\w\d]", "");
                var child = Context.Child(topic);
                if (!child.IsNobody())
                    child.Tell(n);
                else
                    Context.System.Log.Info($"[NextPlay] '{n.Topic}' does not have a DJ - request for one first with 'StartReplayTopic' and a DJ will be yours! ;)");
            });

            Receive<StartReplayTopic>(s =>
            {
                var regexTopic = $"tagged-{Regex.Replace(s.ReaderConfigurationData.TopicName, @"[^\w\d]", "")}";
                if (Context.Child(regexTopic).IsNobody())
                {
                    Become(() => Start(s));
                }
            });
        }
        private NamespaceName GetNameSpaceFromPattern(Regex pattern)
        {
            return TopicName.Get(pattern.ToString()).NamespaceObject;
        }
        private void NewPartitionMetadataRequest(string topic)
        {
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewPartitionMetadataRequest(topic, requestId);
            var pay = new Payload(request, requestId, "CommandPartitionedTopicMetadata", topic);
            _network.Tell(pay);
        }
        // get topics that match 'topicsPattern' from original topics list
        // return result should contain only topic names, without partition part
        private IList<string> TopicsPatternFilter(IList<string> original, Regex topicsPattern)
        {
            var pattern = topicsPattern.ToString().Contains("://") ? new Regex(Regex.Split(topicsPattern.ToString(), @"\:\/\/")[1]) : topicsPattern;

            return original.Select(TopicName.Get).Select(x => x.ToString()).Where(topic => pattern.Match(Regex.Split(topic, @"\:\/\/")[1]).Success).ToList();
        }

        private void NewGetTopicsOfNamespaceRequest(Regex regex)
        {
            var nameSpace = GetNameSpaceFromPattern(regex);
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewGetTopicsOfNamespaceRequest(nameSpace.ToString(), requestId, CommandGetTopicsOfNamespace.Mode.Persistent);
            var payload = new Payload(request, requestId, "GetTopicsOfNamespace");
            _network.Tell(payload);
        }
        public static Props Prop(IActorRef network, IActorRef pulsarManager, PulsarSystem pulsarSystem)
        {
            return Props.Create(() => new TaggedCoordinator(network, pulsarManager, pulsarSystem));
        }
        public IStash Stash { get; set; }
    }
}
