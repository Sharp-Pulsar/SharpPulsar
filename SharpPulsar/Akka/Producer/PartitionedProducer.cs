
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Producer
{
    public class PartitionedProducer: ReceiveActor
    {
        private IActorRef _router;
        private int _partitions;
        private ProducerConfigurationData _configuration;
        public PartitionedProducer(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, IActorRef network)
        {
            _partitions = configuration.Partitions;
            _configuration = configuration;
            //Surely this is pulsar's custom routing policy ;)
            _router = Context.ActorOf(Producer.Prop(clientConfiguration, configuration, Interlocked.Increment(ref IdGenerators.ProducerId), network, true, Self).WithRouter(new ConsistentHashingPool(configuration.Partitions)), "Partition");
            Receive<RegisteredProducer>(p =>
            {
                if (_partitions++ == configuration.Partitions)
                {
                    IdGenerators.PartitionIndex = 0;//incase we want to create multiple partitioned producer
                    _configuration.ProducerEventListener.ProducerCreated(new CreatedProducer(Self, _configuration.TopicName));
                }
            });
            Receive<Send>(s =>
            {
                var msg = new ConsistentHashableEnvelope(s, s.RoutingKey);
                _router.Tell(msg);
            });
            Receive<BulkSend>(s =>
            {
                var msg = new ConsistentHashableEnvelope(s, s.RoutingKey);
                _router.Tell(msg);
            });
            //listen to partition change messsage
        }
        public static Props Prop(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, IActorRef network)
        {
            return Props.Create(() => new PartitionedProducer(clientConfiguration, configuration, network));
        }
    }
}
