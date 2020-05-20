using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Api;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Shared;

namespace SharpPulsar.Akka.Producer
{
    public class ProducerManager:ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _network;
        private ClientConfigurationData _config;
        private ProducerConfigurationData _producerConfiguration;
        private IProducerEventListener _listener;
        public ProducerManager(ClientConfigurationData configuration, IActorRef network)
        {
            _network = network;
            _config = configuration;
        }

        protected override void PreStart()
        {
            Become(Init);
        }

        private void Init()
        {
            Receive<NewProducer>(n =>
            {
                Become(()=>CreatingProducer(n));
            });
            Receive<NewProducerGroupMember>(n =>
            {
                Become(() => CreatingGroupMember(n));
            });
        }

        private void CreatingProducer(NewProducer np)
        {
            Common();
            Receive<RegisteredProducer>(p =>
            {
                //_producers.Add(p.Topic, Sender);
                Become(Init);
            });
            NewProducer(np);
        }
        private void CreatingGroupMember(NewProducerGroupMember member)
        {
            var gm = new NewProducer(member.Schema, member.Configuration, member.ProducerConfiguration); 
            Common(true);
            Receive<RegisteredProducer>(p =>
            {
                Context.Parent.Forward(p);
                Become(Init);
            });
            NewProducer(gm);
        }

        private void Common(bool parent = false)
        {
            Receive<Partitions>(x =>
            {
                var pn = Regex.Replace(_producerConfiguration.ProducerName, @"[^\w\d]", "");
                _listener.Log($"Found {x.Partition} partition for Topic: {_producerConfiguration.TopicName}");
                _producerConfiguration.Partitions = x.Partition;
                _producerConfiguration.UseTls = _config.UseTls;
                if (x.Partition > 0)
                    Context.ActorOf(PartitionedProducer.Prop(_config, _producerConfiguration, _network), pn);
                else
                    Context.ActorOf(Producer.Prop(_config, _producerConfiguration.TopicName, _producerConfiguration, Interlocked.Increment(ref IdGenerators.ProducerId), _network, parent), pn);
            });

            Receive<SchemaResponse>(s =>
            {
                var info = new SchemaInfo
                {
                    Name = s.Name,
                    Schema = (sbyte[])(object)s.Schema,
                    Properties = s.Properties
                };
                if (s.Type == Schema.Type.None)
                {
                    info.Type = SchemaType.Bytes;
                }
                else if (s.Type == Schema.Type.Json)
                {
                    info.Type = SchemaType.Json;
                }
                else
                {
                    info.Type = SchemaType.Avro;
                }
                _producerConfiguration.Schema = ISchema.GetSchema(info);
                SendPartitionMetadataRequestCommand(_producerConfiguration);
            });
            ReceiveAny(x =>
            {
                Console.WriteLine($"Stashing message of type {x.GetType()}: producer not created!");
                Stash.Stash();
            });
        }
        
        protected override void Unhandled(object message)
        {
            Console.WriteLine($"unhandled '{message.GetType()}'");
        }

        private void SendPartitionMetadataRequestCommand(ProducerConfigurationData conf)
        {
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewPartitionMetadataRequest(conf.TopicName, requestId);
            var pay = new Payload(request, requestId, "CommandPartitionedTopicMetadata");
            _network.Tell(pay);
        }
        
        public static Props Prop(ClientConfigurationData configuration, IActorRef network)
        {
            return Props.Create(()=> new ProducerManager(configuration, network));
        }

        private void NewProducer(NewProducer producer)
        {
            var p = Regex.Replace(producer.ProducerConfiguration.ProducerName, @"[^\w\d]", "");
            if (!Context.Child(p).IsNobody())
            {
                _listener.Log($"Producer with name '{producer.ProducerConfiguration.ProducerName}' already exist for topic '{producer.ProducerConfiguration.TopicName}'");
                return;
            }

            var schema = producer.ProducerConfiguration.Schema;
            var clientConfig = producer.Configuration;
            var producerConfig = producer.ProducerConfiguration;
            _producerConfiguration = producerConfig;
            _listener = _producerConfiguration.ProducerEventListener;
            if (clientConfig == null)
            {
                _listener.Log("Producer configuration undefined");
                return;
            }

            if (schema is AutoConsumeSchema)
            {
                _listener.Log("AutoConsumeSchema is only used by consumers to detect schemas automatically");
                return;
            }

            if (schema is AutoProduceBytesSchema autoProduceBytesSchema)
            {
               
                if (autoProduceBytesSchema.SchemaInitialized())
                {
                    SendPartitionMetadataRequestCommand(_producerConfiguration);
                }
                else
                {
                    var requestId = Interlocked.Increment(ref IdGenerators.RequestId); 
                    var request = Commands.NewGetSchema(requestId, producerConfig.TopicName, BytesSchemaVersion.Of(null));
                    var payload = new Payload(request, requestId, "CommandGetSchema");
                    _network.Tell(payload);
                }
            }
            else
            {
                SendPartitionMetadataRequestCommand(_producerConfiguration); 
            }
		}

        public IStash Stash { get; set; }
    }
}
