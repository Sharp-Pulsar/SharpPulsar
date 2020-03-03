using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private long _requestid;
        private IActorRef _network;
        private Regex _topicsPattern;
        private IMessageListener _messageListener;
        private ConsumerConfigurationData _configuration;
        private ClientConfigurationData _clientConfiguration;
        public PatternMultiTopicsManager(ClientConfigurationData client, ConsumerConfigurationData consumer, IActorRef network)
        {
            _network = network;
            _clientConfiguration = client;
            _configuration = consumer;
            _messageListener = consumer.MessageListener;
            _topicsPattern = consumer.TopicsPattern;
            var nameSpace = GetNameSpaceFromPattern(consumer.TopicsPattern);
            var requestId = _requestid++;
            var request = Commands.NewGetTopicsOfNamespaceRequest(nameSpace.ToString(), requestId, CommandGetTopicsOfNamespace.Mode.Persistent);
            var payload = new Payload(request, requestId, "GetTopicsOfNamespace");
            _network.Tell(payload);
            Receive<NamespaceTopics>(t =>
            {
                var topics = TopicsPatternFilter(t.Topics, _topicsPattern);
                _configuration.TopicNames = new HashSet<string>(topics);
                Context.ActorOf(MultiTopicsManager.Prop(_clientConfiguration, _configuration, _network, true));
            });
            Receive<ConsumedMessage>(m =>
            {
                 _messageListener.Received(m.Consumer, m.Message);
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
            var pattern = topicsPattern.ToString().Contains("://") ? new Regex(topicsPattern.ToString().Split(@"\:\/\/")[1]) : topicsPattern;

            return original.Select(TopicName.Get).Select(x => x.ToString()).Where(topic => pattern.Match(topic.Split(@"\:\/\/")[1]).Success).ToList();
        }

        public static Props Prop(ClientConfigurationData client, ConsumerConfigurationData consumer, IActorRef network)
        {
            return Props.Create(()=> new PatternMultiTopicsManager(client, consumer,network));
        }
        
        public IStash Stash { get; set; }
    }
}
