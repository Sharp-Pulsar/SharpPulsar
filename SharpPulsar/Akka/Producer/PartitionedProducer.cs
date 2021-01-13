
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Producer
{
    public class PartitionedProducer: ReceiveActor
    {
        private int _partitions;
        private readonly bool _hasParent;
        private readonly IActorRef _pulsarManager;
        public PartitionedProducer(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, IActorRef network, IActorRef pulsarManager, bool hasParent)
        {
            _pulsarManager = pulsarManager;
            IActorRef router;
            _hasParent = hasParent;
            var configuration1 = configuration;
            var routees = new List<string>();
            var topic = configuration.TopicName;
            //var path = Context.Parent.Path;
            for (var i = 0; i < configuration.Partitions; i++)
            {
                var partitionName = TopicName.Get(topic).GetPartition(i).ToString();
                var produceid = Interlocked.Increment(ref IdGenerators.ProducerId);
                var c = Context.ActorOf(Producer.Prop(clientConfiguration, partitionName, configuration, produceid, network, pulsarManager, true), $"{i}");
                routees.Add(c.Path.ToString());
            }

            switch (configuration.MessageRoutingMode)
            {
                case MessageRoutingMode.ConsistentHashingMode:
                    router = Context.System.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(routees)), $"Partition{DateTimeHelper.CurrentUnixTimeMillis()}");
                    break;
                case MessageRoutingMode.BroadcastMode:
                    router = Context.System.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(routees)), $"Partition{DateTimeHelper.CurrentUnixTimeMillis()}");
                    break;
                case MessageRoutingMode.RandomMode:
                    router = Context.System.ActorOf(Props.Empty.WithRouter(new RandomGroup(routees)), $"Partition{DateTimeHelper.CurrentUnixTimeMillis()}");
                    break;
                default:
                    router = Context.System.ActorOf(Props.Empty.WithRouter(new RoundRobinGroup(routees)), $"Partition{DateTimeHelper.CurrentUnixTimeMillis()}");
                    break;
            }
            Receive<RegisteredProducer>(p =>
            {
                _partitions++;
                if (_partitions == configuration.Partitions)
                {
                    IdGenerators.PartitionIndex = 0;//incase we want to create multiple partitioned producer
                    if (hasParent)
                        Context.Parent.Tell(new RegisteredProducer(-1, configuration.ProducerName, configuration.TopicName, p.IsNew));
                    else
                    {
                        _pulsarManager.Tell(new CreatedProducer(Self, configuration1.TopicName, configuration1.ProducerName));
                    }
                    
                }

            });

            Receive<PulsarError>(e =>
            {
                _partitions++;
                configuration.ProducerEventListener.Log($"{e.Error}: {e.Message}");
            });
            Receive<Send>(s =>
            {
                if (configuration1.MessageRoutingMode == MessageRoutingMode.ConsistentHashingMode)
                {
                    var msg = new ConsistentHashableEnvelope(s, s.RoutingKey);
                    router.Tell(msg);
                }
                else
                {
                    router.Tell(s);
                }
            });
            
            Receive<BulkSend>(s =>
            {
                if (configuration1.MessageRoutingMode == MessageRoutingMode.ConsistentHashingMode)
                {
                    foreach (var m in s.Messages)
                    {
                        var msg = new ConsistentHashableEnvelope(m, m.RoutingKey);
                        router.Tell(msg);
                    }
                }
                else
                {
                    foreach (var m in s.Messages)
                    {
                        router.Tell(m);
                    }
                }
                
            });
            
        }
        public static Props Prop(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, IActorRef network, IActorRef pulsarManager, bool hasParent = false)
        {
            return Props.Create(() => new PartitionedProducer(clientConfiguration, configuration, network, pulsarManager, hasParent));
        }
    }
}
