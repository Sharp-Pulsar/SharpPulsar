using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using PulsarAdmin;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using TopicEntries = SharpPulsar.Akka.InternalCommands.Consumer.TopicEntries;

namespace SharpPulsar.Akka.EventSource.Pulsar
{
    public class PulsarTaggedAggregateRoot:ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        private readonly List<string> _routees;
        private int _expectedRoutees;
        private int _currentRoutees;
        private readonly HttpClient _httpClient;
        public PulsarTaggedAggregateRoot(IActorRef network, IActorRef pulsarManager)
        {
            _httpClient = new HttpClient();
            _routees = new List<string>();
            _network = network;
            _pulsarManager = pulsarManager;
            Become(Listening);
        }

        private void GetStats(GetNumberOfEntries numberOfEntries)
        {
            Receive<NamespaceTopics>(t =>
            {
                var topics = TopicsPatternFilter(t.Topics, new Regex(numberOfEntries.Topic));
                var max = 0L;
                var total = 0L;
                var adminRestapi = new PulsarAdminRESTAPI(numberOfEntries.Server, _httpClient, true);
                foreach (var topic in topics)
                {
                    var topicName = TopicName.Get(topic);
                    var data = adminRestapi.GetInternalStats1(topicName.NamespaceObject.Tenant,
                        topicName.NamespaceObject.LocalName, topicName.LocalName, false);
                    if (data?.NumberOfEntries > max)
                        max = data.NumberOfEntries.Value;

                    if (data?.NumberOfEntries != null)
                        total += data.NumberOfEntries.Value;
                }
                _pulsarManager.Tell(new TopicEntries(numberOfEntries.Topic, max, total, topics.Count));
                Become(Listening);
                Stash.UnstashAll();
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
                        var replay = new StartReplayTopic(replayTopic.ClientConfigurationData, PrepareReaderConfigurationData(replayTopic.ReaderConfigurationData, partitionName), replayTopic.AdminUrl, replayTopic.From, replayTopic.To, replayTopic.Max, replayTopic.Tag, replayTopic.Tagged, replayTopic.Source);
                        var routee = Context.ActorOf(PulsarSourceActor.Prop(replay, _pulsarManager, _network), name);
                        _routees.Add(routee.Path.ToString());
                    }
                }
                else
                {
                    var replay = new StartReplayTopic(replayTopic.ClientConfigurationData, PrepareReaderConfigurationData(replayTopic.ReaderConfigurationData, p.Topic), replayTopic.AdminUrl, replayTopic.From, replayTopic.To, replayTopic.Max, replayTopic.Tag, replayTopic.Tagged, replayTopic.Source);

                    var name = Regex.Replace(p.Topic, @"[^\w\d]", "");
                    var routee = Context.ActorOf(PulsarSourceActor.Prop(replay, _pulsarManager, _network), name);
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
                ResetIncludeHead = true,//readerConfiguration.ResetIncludeHead,
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
                var topic = $"tagged-{Regex.Replace(n.Topic, @"[^\w\d]", "")}"; ;
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
        public static Props Prop(IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(() => new PulsarTaggedAggregateRoot(network, pulsarManager));
        }
        public IStash Stash { get; set; }
    }
}
