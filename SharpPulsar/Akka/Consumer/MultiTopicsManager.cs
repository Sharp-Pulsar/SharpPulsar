using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;

namespace SharpPulsar.Akka.Consumer
{   /// <summary>
    /// Agregates consumers
    /// </summary>
    public class MultiTopicsManager:ReceiveActor
    {
        private ClientConfigurationData _clientConfiguration;
        private ConsumerConfigurationData _consumerConfiguration;
        private long _requestid;
        private IActorRef _network;
        private long _consumerid;
        private IMessageListener _listener;
        private IConsumerEventListener _event;
        private bool _hasParentConsumer;
        private readonly Dictionary<long, Payload> _pendingLookupRequests = new Dictionary<long, Payload>();
        public MultiTopicsManager(ClientConfigurationData clientConfiguration, ConsumerConfigurationData configuration, IActorRef network, bool hasParentConsumer)
        {
            _consumerConfiguration = configuration;
            _clientConfiguration = clientConfiguration;
            _network = network;
            _hasParentConsumer = hasParentConsumer;
            _listener = configuration.MessageListener;
            _event = configuration.ConsumerEventListener;
            foreach (var topic in configuration.TopicNames)
            {
                var requestId = _requestid++;
                var request = Commands.NewPartitionMetadataRequest(topic, requestId);
                var pay = new Payload(request.Array, requestId, "CommandPartitionedTopicMetadata", topic);
                _pendingLookupRequests.Add(requestId, pay);
                _network.Tell(pay);
            }
            Receive<Partitions>(p =>
            {
                for (var i = 0; i < p.Partition; i++)
                {
                    var partitionName = TopicName.Get(p.Topic).GetPartition(i).ToString();
                    Context.ActorOf(Consumer.Prop(_clientConfiguration, partitionName, _consumerConfiguration, _consumerid++, _network, true, i, SubscriptionMode.Durable));
                }

                _pendingLookupRequests.Remove(p.RequestId);
            });
            Receive<ConsumedMessage>(m =>
            {
                if(_hasParentConsumer)
                    Context.Parent.Tell(m);
                else
                    _listener.Received(m.Consumer, m.Message);
            });
            ReceiveAny(a =>
            {
                _event.Log($"{a.GetType()}, unhandled");
            });
        }
        
        public static Props Prop(ClientConfigurationData clientConfiguration, ConsumerConfigurationData configuration, IActorRef network, bool hasParentConsumer)
        {
            return Props.Create(()=> new MultiTopicsManager(clientConfiguration, configuration, network, hasParentConsumer));
        }
    }
}
