using System;
using System.Threading;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Consumer
{
    public class ConsumerManager:ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _network;
        private ClientConfigurationData _config;
        public ConsumerManager(ClientConfigurationData configuration, IActorRef network)
        {
            _network = network;
            _config = configuration;
            Receive<NewConsumer>(NewConsumer);
        }

        protected override void Unhandled(object message)
        {
            Console.WriteLine($"Unhandled message: {message.GetType().Name}");
        }

        public static Props Prop(ClientConfigurationData configuration, IActorRef network)
        {
            return Props.Create(() => new ConsumerManager(configuration, network));
        }
        
        private void NewConsumer(NewConsumer consumer)
        {
            var consumerConfig = consumer.ConsumerConfiguration;
            
            switch (consumer.ConsumerType)
            {
                case ConsumerType.Pattern:
                    Context.ActorOf(PatternMultiTopicsManager.Prop(_config, consumerConfig, _network, consumer.Seek), $"PatternMultiTopics{DateTimeHelper.CurrentUnixTimeMillis()}");
                    break;
                case ConsumerType.Multi:
                    Context.ActorOf(MultiTopicsManager.Prop(_config, consumerConfig, _network, false, consumer.Seek), $"MultiTopics{DateTimeHelper.CurrentUnixTimeMillis()}");
                    break;
                case ConsumerType.Single:
                    var partitionIndex = TopicName.GetPartitionIndex(consumerConfig.SingleTopic);
                    Context.ActorOf(Consumer.Prop(_config, consumerConfig.SingleTopic, consumerConfig, Interlocked.Increment(ref IdGenerators.ConsumerId), _network, false, partitionIndex, SubscriptionMode.Durable, consumer.Seek), $"SingleTopic{DateTimeHelper.CurrentUnixTimeMillis()}");
                    break;
                default:
                    consumerConfig.ConsumerEventListener.Log("Are you high? How am I suppose to know the consumer type you want to create? ;)!");
                    break;
            }
        }
       
        public IStash Stash { get; set; }
    }
}
