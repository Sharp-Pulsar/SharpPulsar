﻿
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
        private int _partitions;
        private readonly bool _hasParent;
        public PartitionedProducer(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, IActorRef network, bool hasParent)
        {
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
                var c = Context.ActorOf(Producer.Prop(clientConfiguration, partitionName, configuration, produceid, network, true), $"{i}");
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
                _partitions += 1;
                if (_partitions == configuration.Partitions)
                {
                    IdGenerators.PartitionIndex = 0;//incase we want to create multiple partitioned producer
                    if (hasParent)
                        Context.Parent.Tell(p);
                    else
                    {
                        configuration1.ProducerEventListener.ProducerCreated(new CreatedProducer(Self, configuration1.TopicName, configuration1.ProducerName));
                    }
                    
                }

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
        public static Props Prop(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, IActorRef network, bool hasParent = false)
        {
            return Props.Create(() => new PartitionedProducer(clientConfiguration, configuration, network, hasParent));
        }
    }
}
