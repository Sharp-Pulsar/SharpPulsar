using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using Nito.AsyncEx;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Protocol;

namespace SharpPulsar.Akka.Consumer
{   /// <summary>
    /// Agregates consumers
    /// </summary>
    public class MultiTopicsManager:ReceiveActor, IWithUnboundedStash
    {
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly ConsumerConfigurationData _consumerConfiguration;
        private readonly IActorRef _network;
        private readonly IMessageListener _listener;
        private readonly IConsumerEventListener _event;
        private readonly bool _hasParentConsumer;
        private readonly Seek _seek;
        private readonly IActorRef _pulsarManager;
        public MultiTopicsManager(ClientConfigurationData clientConfiguration, ConsumerConfigurationData configuration, IActorRef network, bool hasParentConsumer, Seek seek, IActorRef pulsarManager)
        {
            _pulsarManager = pulsarManager;
            _seek = seek;
            _consumerConfiguration = configuration;
            _clientConfiguration = clientConfiguration;
            _network = network;
            _hasParentConsumer = hasParentConsumer;
            _listener = configuration.MessageListener;
            _event = configuration.ConsumerEventListener;
            foreach (var topic in _consumerConfiguration.TopicNames)
            {
                var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
                var request = Commands.NewPartitionMetadataRequest(topic, requestId);
                var pay = new Payload(request, requestId, "CommandPartitionedTopicMetadata", topic);
                var ask = _network.Ask<Partitions>(pay);
                var p = SynchronizationContextSwitcher.NoContext(async () => await ask).Result;
                HandlePartition(p);
            }
            BecomeReady();
        }

        private void BecomeReady()
        {
            Receive<UpdatePatternTopicsSubscription>(s =>
            {
                var oldConsumers = Context.GetChildren().Select(x=> x.Path.Name).ToList();

                var newConsumer = s.Topics.Select(x => new {Regexed = Regex.Replace(x, @"[^\w\d]", ""), Raw = x }).ToList();
                foreach (var stop in oldConsumers)
                {
                    var alive = newConsumer.Select(x=> x.Regexed).Contains(stop);
                    if (!alive)
                    {
                        Context.Stop(Context.Child(stop));
                        oldConsumers.Remove(stop);
                    }
                }

                var news = newConsumer.Where(x => !oldConsumers.Contains(x.Regexed));
                foreach (var topic in news)
                {
                    var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
                    var request = Commands.NewPartitionMetadataRequest(topic.Raw, requestId);
                    var pay = new Payload(request, requestId, "CommandPartitionedTopicMetadata", topic.Raw);
                    var ask = _network.Ask<Partitions>(pay);
                    var p = SynchronizationContextSwitcher.NoContext(async () => await ask).Result;
                    HandlePartition(p);
                }
            });
            Receive<ConsumedMessage>(m =>
            {
                if (_hasParentConsumer)
                    Context.Parent.Tell(m);
                else
                {
                    if (_consumerConfiguration.ConsumptionType == ConsumptionType.Listener)
                        _listener.Received(m.Consumer, m.Message);
                    else if (_consumerConfiguration.ConsumptionType == ConsumptionType.Queue)
                        _pulsarManager.Tell(m);
                }
            });
            ReceiveAny(a =>
            {
                _event.Log($"{a.GetType().Name}, not supported!");
            });
        }

        private void HandlePartition(Partitions p)
        {
            var name = _consumerConfiguration.ConsumerName;
            if (p.Partition > 0)
            {
                for (var i = 0; i < p.Partition; i++)
                {
                    var partitionName = TopicName.Get(p.Topic).GetPartition(i).ToString();
                    Context.ActorOf(Consumer.Prop(_clientConfiguration, partitionName, _consumerConfiguration, Interlocked.Increment(ref IdGenerators.ConsumerId), _network, true, i, SubscriptionMode.Durable, _seek, _pulsarManager), $"{name}{i}");
                }
            }
            else
            {
                Context.ActorOf(Consumer.Prop(_clientConfiguration, p.Topic, _consumerConfiguration, Interlocked.Increment(ref IdGenerators.ConsumerId), _network, true, 0, SubscriptionMode.Durable, _seek, _pulsarManager), name);
            }
        }
        public static Props Prop(ClientConfigurationData clientConfiguration, ConsumerConfigurationData configuration, IActorRef network, bool hasParentConsumer, Seek seek, IActorRef pulsarManager)
        {
            return Props.Create(()=> new MultiTopicsManager(clientConfiguration, configuration, network, hasParentConsumer, seek, pulsarManager));
        }

        public IStash Stash { get; set; }
    }
}
