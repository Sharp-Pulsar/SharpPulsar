using System.Collections.Generic;
using System.Threading;
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
    public class MultiTopicsManager:ReceiveActor, IWithUnboundedStash
    {
        private ClientConfigurationData _clientConfiguration;
        private ConsumerConfigurationData _consumerConfiguration;
        private IActorRef _network;
        private IMessageListener _listener;
        private IConsumerEventListener _event;
        private bool _hasParentConsumer;
        private Seek _seek;
        private readonly Dictionary<long, Payload> _pendingLookupRequests = new Dictionary<long, Payload>();
        public MultiTopicsManager(ClientConfigurationData clientConfiguration, ConsumerConfigurationData configuration, IActorRef network, bool hasParentConsumer, Seek seek)
        {
            _seek = seek;
            _consumerConfiguration = configuration;
            _clientConfiguration = clientConfiguration;
            _network = network;
            _hasParentConsumer = hasParentConsumer;
            _listener = configuration.MessageListener;
            _event = configuration.ConsumerEventListener;
            BecomeReady();
        }

        private void BecomeReady()
        {
            Receive<Partitions>(p =>
            {
                if (p.Partition > 0)
                {
                    for (var i = 0; i < p.Partition; i++)
                    {
                        var partitionName = TopicName.Get(p.Topic).GetPartition(i).ToString();
                        Context.ActorOf(Consumer.Prop(_clientConfiguration, partitionName, _consumerConfiguration, Interlocked.Increment(ref IdGenerators.ConsumerId), _network, true, i, SubscriptionMode.Durable, _seek), $"Consumer{DateTimeHelper.CurrentUnixTimeMillis()}");
                    }
                }
                else
                {
                    Context.ActorOf(Consumer.Prop(_clientConfiguration, p.Topic, _consumerConfiguration, Interlocked.Increment(ref IdGenerators.ConsumerId), _network, true, 0, SubscriptionMode.Durable, _seek), $"Consumer{DateTimeHelper.CurrentUnixTimeMillis()}");
                }

                _pendingLookupRequests.Remove(p.RequestId);
            });
            Receive<ConsumedMessage>(m =>
            {
                if (_hasParentConsumer)
                    Context.Parent.Tell(m);
                else
                    _listener.Received(m.Consumer, m.Message);
            });
            ReceiveAny(a =>
            {
                _event.Log($"{a.GetType().Name}, not supported!");
            });
            foreach (var topic in _consumerConfiguration.TopicNames)
            {
                var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
                var request = Commands.NewPartitionMetadataRequest(topic, requestId);
                var pay = new Payload(request, requestId, "CommandPartitionedTopicMetadata", topic);
                _pendingLookupRequests.Add(requestId, pay);
                _network.Tell(pay);
            }
           
        }

        public static Props Prop(ClientConfigurationData clientConfiguration, ConsumerConfigurationData configuration, IActorRef network, bool hasParentConsumer, Seek seek)
        {
            return Props.Create(()=> new MultiTopicsManager(clientConfiguration, configuration, network, hasParentConsumer, seek));
        }

        public IStash Stash { get; set; }
    }
}
