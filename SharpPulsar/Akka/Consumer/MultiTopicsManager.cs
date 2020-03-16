﻿using System.Collections.Generic;
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
        private readonly Dictionary<long, Payload> _pendingLookupRequests = new Dictionary<long, Payload>();
        public MultiTopicsManager(ClientConfigurationData clientConfiguration, ConsumerConfigurationData configuration, IActorRef network, bool hasParentConsumer)
        {
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
            ReceiveAny(x => Stash.Stash());
            foreach (var topic in _consumerConfiguration.TopicNames)
            {
                var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
                var request = Commands.NewPartitionMetadataRequest(topic, requestId);
                var pay = new Payload(request, requestId, "CommandPartitionedTopicMetadata", topic);
                _pendingLookupRequests.Add(requestId, pay);
                _network.Tell(pay);
            }
            Become(Ready);
        }

        private void Ready()
        {
            /*Receive<TimestampSeek>(s =>
            {
                foreach (var c in Context.GetChildren())
                {
                    c.Forward(s);
                }
            });*/
            Receive<Partitions>(p =>
            {
                if (p.Partition > 0)
                {
                    for (var i = 0; i < p.Partition; i++)
                    {
                        var partitionName = TopicName.Get(p.Topic).GetPartition(i).ToString();
                        Context.ActorOf(Consumer.Prop(_clientConfiguration, partitionName, _consumerConfiguration, Interlocked.Increment(ref IdGenerators.ConsumerId), _network, true, i, SubscriptionMode.Durable));
                    }
                }
                else
                {
                    Context.ActorOf(Consumer.Prop(_clientConfiguration, p.Topic, _consumerConfiguration, Interlocked.Increment(ref IdGenerators.ConsumerId), _network, true, 0, SubscriptionMode.Durable));
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
            Stash.UnstashAll();
        }
        public static Props Prop(ClientConfigurationData clientConfiguration, ConsumerConfigurationData configuration, IActorRef network, bool hasParentConsumer)
        {
            return Props.Create(()=> new MultiTopicsManager(clientConfiguration, configuration, network, hasParentConsumer));
        }

        public IStash Stash { get; set; }
    }
}
