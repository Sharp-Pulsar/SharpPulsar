using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.Sql.Live;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Akka.Consumer
{
    public class PatternMultiTopicsManager:ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _network;
        private readonly ConsumerConfigurationData _configuration;
        private IActorRef _multiTopicActor;
        private bool _initialized;
        public PatternMultiTopicsManager(ClientConfigurationData client, ConsumerConfigurationData consumer, IActorRef network, Seek seek, IActorRef pulsarManager)
        {
            var pulsarManager1 = pulsarManager;
            _network = network;
            _configuration = consumer;
            var messageListener = consumer.MessageListener;
            var topicsPattern = consumer.TopicsPattern;

            Receive<NamespaceTopics>(t =>
            {
                var topics = TopicsPatternFilter(t.Topics, topicsPattern);
                if (!_initialized)
                { 
                    _configuration.TopicNames = new HashSet<string>(topics);
                   _multiTopicActor = Context.ActorOf(MultiTopicsManager.Prop(client, _configuration, _network, true, seek, pulsarManager1));
                   _initialized = true;
                }
                else
                {
                    _multiTopicActor.Tell(new UpdatePatternTopicsSubscription(topics.ToImmutableHashSet()));
                }
            });
            Receive<ConsumedMessage>(m =>
            {
                if (_configuration.ConsumptionType is ConsumptionType.Listener)
                    messageListener.Received(m.Consumer, m.Message);
                else if (_configuration.ConsumptionType is ConsumptionType.Queue) pulsarManager1.Tell(new ConsumedMessage(m.Consumer, m.Message));
            });
            Receive<RefreshPatternTopics>(m =>
            {
                PatternTopics();
            });
            PatternTopics();
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(_configuration.PatternAutoDiscoveryPeriod), TimeSpan.FromMilliseconds(_configuration.PatternAutoDiscoveryPeriod), Self, RefreshPatternTopics.Instance, Nobody.Instance);
        }

        private void PatternTopics()
        {
            var nameSpace = GetNameSpaceFromPattern(_configuration.TopicsPattern);
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewGetTopicsOfNamespaceRequest(nameSpace.ToString(), requestId, CommandGetTopicsOfNamespace.Mode.Persistent);
            var payload = new Payload(request, requestId, "GetTopicsOfNamespace");
            _network.Tell(payload);
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

    public sealed class RefreshPatternTopics
    {
        public static RefreshPatternTopics Instance = new RefreshPatternTopics();
    }
}
