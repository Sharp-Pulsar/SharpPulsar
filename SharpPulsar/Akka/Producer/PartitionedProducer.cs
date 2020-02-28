
using System.Collections.Generic;
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
            var routees = new List<string>();
            var path = Context.Self.Path;
            for (var i = 0; i < configuration.Partitions; i++)
            {
                routees.Add($"{path}/Partition/{i}");
            }
            //Surely this is pulsar's custom routing policy ;)
            _router = Context.ActorOf(Producer.Prop(clientConfiguration, configuration, producerid, network, true).WithRouter(new ConsistentHashingGroup(routees)), "Partition");
            Receive<RegisteredProducer>(p =>
            {
                if (_partitions++ == configuration.Partitions)
                {
                    Context.Parent.Tell(new RegisteredProducer(producerid, configuration.ProducerName, configuration.TopicName));
                }
            });
            //listen to partition change messsage
        }
        public static Props Prop(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, long producerid, IActorRef network)
        {
            return Props.Create(() => new PartitionedProducer(clientConfiguration, configuration, producerid, network));
        }
    }
}
