
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
            _configuration = configuration;
            var routees = new List<string>();
            var path = Context.Self.Path;
            for (var i = 0; i < configuration.Partitions; i++)
            {
                routees.Add($"{path}/Partition/{Interlocked.Increment(ref IdGenerators.ProducerId)}");
            }
            //Surely this is pulsar's custom routing policy ;)
            _router = Context.ActorOf(Producer.Prop(clientConfiguration, configuration, Interlocked.Increment(ref IdGenerators.ProducerId), network, true).WithRouter(new ConsistentHashingGroup(routees)), "Partition");
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
        public static Props Prop(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, IActorRef network)
        {
            return Props.Create(() => new PartitionedProducer(clientConfiguration, configuration, network));
        }
    }
}
