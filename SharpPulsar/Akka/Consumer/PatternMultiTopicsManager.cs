using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Akka.Consumer
{
    public class PatternMultiTopicsManager:ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _network;
        private readonly Regex _topicPattern;
        private readonly ClientConfigurationData _client;
        private readonly ConsumerConfigurationData _configuration;
        private IActorRef _multiTopicActor;
        private bool _initialized;
        private readonly Seek _seek;
        private readonly IActorRef _pulsarManager;
        public PatternMultiTopicsManager(ClientConfigurationData client, ConsumerConfigurationData consumer, IActorRef network, Seek seek, IActorRef pulsarManager)
        {
            _pulsarManager = pulsarManager;
            _network = network;
            _seek = seek;
            _configuration = consumer;
            var messageListener = consumer.MessageListener;
            _topicPattern = consumer.TopicsPattern;
            _client = client;
            Receive<ConsumedMessage>(m =>
            {
                if (_configuration.ConsumptionType is ConsumptionType.Listener)
                    messageListener.Received(m.Consumer, m.Message, m.AckSets);
                else if (_configuration.ConsumptionType is ConsumptionType.Queue) _pulsarManager.Tell(m);
            });
            PatternTopics();
            Context.System.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromSeconds(_configuration.PatternAutoDiscoveryPeriod), TimeSpan.FromMilliseconds(_configuration.PatternAutoDiscoveryPeriod), PatternTopics);
        }

        private void PatternTopics()
        {
            var nameSpace = GetNameSpaceFromPattern(_topicPattern);
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewGetTopicsOfNamespaceRequest(nameSpace.ToString(), requestId, CommandGetTopicsOfNamespace.Mode.Persistent);
            var payload = new Payload(request, requestId, "GetTopicsOfNamespace");
            var t =_network.Ask<NamespaceTopics>(payload).GetAwaiter().GetResult();
            var topics = TopicsPatternFilter(t.Topics, _topicPattern);
            if (!_initialized)
            {
                _configuration.TopicNames = new HashSet<string>(topics);
                _multiTopicActor = Context.ActorOf(MultiTopicsManager.Prop(_client, _configuration, _network, true, _seek, _pulsarManager));
                _initialized = true;
            }
            else
            {
                _multiTopicActor.Tell(new UpdatePatternTopicsSubscription(topics.ToImmutableHashSet()));
            }
        }
        private NamespaceName GetNameSpaceFromPattern(Regex pattern)
        {
            return TopicName.Get(pattern.ToString()).NamespaceObject;
        }
        // get topics that match 'topicsPattern' from original topics list
        // return result should contain only topic names, without partition part
        private IList<string> TopicsPatternFilter(IList<string> original, Regex topicsPattern)
        {
            var pattern = topicsPattern.ToString().Contains("://") ? new Regex(Regex.Split(topicsPattern.ToString(), @"\:\/\/")[1]) : topicsPattern;

            return original.Select(TopicName.Get).Select(x => x.ToString()).Where(topic => pattern.Match(Regex.Split(topic,@"\:\/\/")[1]).Success).ToList();
        }

        public static Props Prop(ClientConfigurationData client, ConsumerConfigurationData consumer, IActorRef network, Seek seek, IActorRef pulsarManager)
        {
            return Props.Create(()=> new PatternMultiTopicsManager(client, consumer,network, seek, pulsarManager));
        }
        
        public IStash Stash { get; set; }
    }

}
