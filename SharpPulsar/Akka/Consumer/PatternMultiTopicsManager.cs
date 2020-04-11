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
        private IActorRef _network;
        private Regex _topicsPattern;
        private IMessageListener _messageListener;
        private ConsumerConfigurationData _configuration;
        private ClientConfigurationData _clientConfiguration;
        private Seek _seek;
        public PatternMultiTopicsManager(ClientConfigurationData client, ConsumerConfigurationData consumer, IActorRef network, Seek seek)
        {
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
                Context.ActorOf(MultiTopicsManager.Prop(_clientConfiguration, _configuration, _network, true, _seek));
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
            var pattern = topicsPattern.ToString().Contains("://") ? new Regex(Regex.Split(topicsPattern.ToString(), @"\:\/\/")[1]) : topicsPattern;

            return original.Select(TopicName.Get).Select(x => x.ToString()).Where(topic => pattern.Match(Regex.Split(topic,@"\:\/\/")[1]).Success).ToList();
        }

        public static Props Prop(ClientConfigurationData client, ConsumerConfigurationData consumer, IActorRef network, Seek seek)
        {
            return Props.Create(()=> new PatternMultiTopicsManager(client, consumer,network, seek));
        }
        
        public IStash Stash { get; set; }
    }
}
