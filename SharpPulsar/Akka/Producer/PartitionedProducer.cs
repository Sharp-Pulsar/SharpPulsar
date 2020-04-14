
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
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
            var topic = configuration.TopicName;
            //var path = Context.Parent.Path;
            for (var i = 0; i < configuration.Partitions; i++)
            {
                var partitionName = TopicName.Get(topic).GetPartition(i).ToString();
                var produceid = Interlocked.Increment(ref IdGenerators.ProducerId);
                var c = Context.ActorOf(Producer.Prop(clientConfiguration, partitionName, configuration, produceid, network, true, Self), $"{i}");
                routees.Add(c.Path.ToString());
            }
            //Surely this is pulsar's custom routing policy ;)
            if(configuration.MessageRoutingMode == MessageRoutingMode.RoundRobinPartition)
                 _router = Context.System.ActorOf(Props.Empty.WithRouter(new RoundRobinGroup(routees)), $"Partition{DateTimeHelper.CurrentUnixTimeMillis()}");
            else
                _router = Context.System.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(routees)), $"Partition{DateTimeHelper.CurrentUnixTimeMillis()}");
            Receive<RegisteredProducer>(p =>
            {
                _partitions += 1;
                if (_partitions == configuration.Partitions)
                {
                    IdGenerators.PartitionIndex = 0;//incase we want to create multiple partitioned producer
                    _configuration.ProducerEventListener.ProducerCreated(new CreatedProducer(Self, _configuration.TopicName, _configuration.ProducerName));
                }
            });
            Receive<Send>(s =>
            {
                var msg = new ConsistentHashableEnvelope(s, s.RoutingKey);
                _router.Tell(msg);
            });
            
            Receive<BulkSend>(s =>
            {
                foreach (var m in s.Messages)
                {
                    var msg = new ConsistentHashableEnvelope(m, m.RoutingKey);
                    _router.Tell(msg);
                }
            });
            
        }
        public static Props Prop(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, IActorRef network)
        {
            return Props.Create(() => new PartitionedProducer(clientConfiguration, configuration, network));
        }
    }
}
