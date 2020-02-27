
using Akka.Actor;
using Akka.Routing;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Producer
{
    public class PartitionedProducer: ReceiveActor
    {
        private IActorRef _router;
        private long _producerId;
        private int _partitions;
        public PartitionedProducer(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, long producerid, IActorRef network)
        {
            //Surely this is pulsar's custom routing policy ;)
            _router = Context.ActorOf(Producer.Prop(clientConfiguration, configuration, producerid, network).WithRouter(new ConsistentHashingPool(configuration.Partitions)), "PartitionedProducer");
            Receive<RegisteredProducer>(p =>
            {
                if (_partitions++ == configuration.Partitions)
                {
                    Context.Parent.Tell(new RegisteredProducer(producerid, configuration.ProducerName, configuration.TopicName));
                }
            });
        }
        public static Props Prop(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, long producerid, IActorRef network)
        {
            return Props.Create(() => new PartitionedProducer(clientConfiguration, configuration, producerid, network));
        }
    }
}
