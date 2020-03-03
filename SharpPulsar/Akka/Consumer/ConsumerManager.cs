using System;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Common.Naming;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Consumer
{
    public class ConsumerManager:ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _network;
        private long _consumerid;
        private ClientConfigurationData _config;
        public ConsumerManager(ClientConfigurationData configuration)
        {
            _config = configuration;
            Become(() => Init(configuration));
        }

        public static Props Prop(ClientConfigurationData configuration)
        {
            return Props.Create(() => new ConsumerManager(configuration));
        }
        private void Open()
        {
            Receive<NewConsumer>(NewConsumer);
            Receive<TcpClosed>(_ =>
            {
                Become(Connecting);
            });
            Receive<TcpSuccess>(f =>
            {
                try
                {
                    
                    Console.WriteLine($"TCP connection success from {f.Name}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            Stash.UnstashAll();
        }

        private void NewConsumer(NewConsumer consumer)
        {
            var schema = consumer.ConsumerConfiguration.Schema;
            var clientConfig = consumer.Configuration;
            var consumerConfig = consumer.ConsumerConfiguration;
            if (clientConfig == null)
            {
                Sender.Tell(new ErrorMessage(new PulsarClientException.InvalidConfigurationException("Producer configuration undefined")));
                return;
            }

            switch (consumer.ConsumerType)
            {
                case ConsumerType.Pattern:
                    Context.ActorOf(PatternMultiTopicsManager.Prop(clientConfig, consumerConfig, _network));
                    break;
                case ConsumerType.Multi:
                    Context.ActorOf(MultiTopicsManager.Prop(clientConfig, consumerConfig, _network, false), "MultiTopicsManager");
                    break;
                case ConsumerType.Single:
                    var partitionIndex = TopicName.GetPartitionIndex(consumerConfig.SingleTopic);
                    Context.ActorOf(Consumer.Prop(clientConfig, consumerConfig.SingleTopic, consumerConfig, _consumerid++, _network, false, partitionIndex, SubscriptionMode.Durable), "SingleTopic");
                    break;
                default:
                    Sender.Tell(new ErrorMessage(new PulsarClientException.InvalidConfigurationException("Are you high? How am I suppose to know the consumer type you want to create? ;)!")));
                    break;
            }
        }
        private void Connecting()
        {
            _network.Tell(new TcpReconnect());
            Receive<TcpSuccess>(s =>
            {
                Console.WriteLine($"Pulsar handshake completed with {s.Name}");
                Become(Open);
            });
            ReceiveAny(m =>
            {
                Stash.Stash();
            });
        }
        private void Init(ClientConfigurationData configuration)
        {
            _network = Context.ActorOf(NetworkManager.Prop(Self, configuration), "NetworkManager");
            Receive<TcpSuccess>(s =>
            {
                Console.WriteLine($"Pulsar handshake completed with {s.Name}");
                Become(Open);
            });
            ReceiveAny(_ => Stash.Stash());
        }

        public IStash Stash { get; set; }
    }
}
