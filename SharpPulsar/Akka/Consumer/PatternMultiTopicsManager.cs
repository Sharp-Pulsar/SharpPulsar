using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Akka.Consumer
{
    public class PatternMultiTopicsManager:ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _network;
        private readonly Regex _topicsPattern;
        private readonly IMessageListener _messageListener;
        private readonly ConsumerConfigurationData _configuration;
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly IActorRef _pulsarManager;
        private readonly Seek _seek;
        public PatternMultiTopicsManager(ClientConfigurationData client, ConsumerConfigurationData consumer, IActorRef network, Seek seek, IActorRef pulsarManager)
        {
            _pulsarManager = pulsarManager;
            _seek = seek;
            _network = network;
            _clientConfiguration = client;
            _configuration = consumer;
            _messageListener = consumer.MessageListener;
            _topicsPattern = consumer.TopicsPattern;
            BecomeStart();
        }

        private void BecomeStart()
        {
            var nameSpace = GetNameSpaceFromPattern(_configuration.TopicsPattern);
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewGetTopicsOfNamespaceRequest(nameSpace.ToString(), requestId, CommandGetTopicsOfNamespace.Mode.Persistent);
            var payload = new Payload(request, requestId, "GetTopicsOfNamespace");
            _network.Tell(payload);
            Receive<NamespaceTopics>(t =>
            {
                var topics = TopicsPatternFilter(t.Topics, _topicsPattern);
                _configuration.TopicNames = new HashSet<string>(topics);
                Context.ActorOf(MultiTopicsManager.Prop(_clientConfiguration, _configuration, _network, true, _seek, _pulsarManager));
            });
            Receive<ConsumedMessage>(m =>
            {
                if (_configuration.ConsumptionType is ConsumptionType.Listener)
                    _messageListener.Received(m.Consumer, m.Message);
                else if (_configuration.ConsumptionType is ConsumptionType.Queue) _pulsarManager.Tell(new ConsumedMessage(m.Consumer, m.Message));
            });
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
