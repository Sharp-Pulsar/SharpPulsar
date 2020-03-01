
using System.Collections.Generic;
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
        private long _producerId;
        private int _partitions;
        private ProducerConfigurationData _configuration;
        public PartitionedProducer(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, long producerid, IActorRef network)
        {
            _configuration = configuration;
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
        public static Props Prop(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, long producerid, IActorRef network)
        {
            return Props.Create(() => new PartitionedProducer(clientConfiguration, configuration, producerid, network));
        }
    }
}
