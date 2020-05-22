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
        private readonly IActorRef _network;
        private readonly ClientConfigurationData _config;
        private ProducerConfigurationData _producerConfiguration;
        private IProducerEventListener _listener;
        private IActorRef _group;
        private IActorRef _pulsarManager;
        public ProducerManager(ClientConfigurationData configuration, IActorRef network, IActorRef pulsarManager)
        {
            _pulsarManager = pulsarManager;
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
                _group = Sender;
                Become(() => CreatingGroupMember(n, n.Title));
            });

            Stash.UnstashAll();
        }

        private void CreatingProducer(NewProducer np)
        {
            Common();
            Receive<RegisteredProducer>(p =>
            {
                //_producers.Add(p.Topic, Sender);
                Become(Init);
                Stash.UnstashAll();
            });
            ReceiveAny(x =>
            {
                Stash.Stash();
            });
            NewProducer(np);
        }
        private void CreatingGroupMember(NewProducerGroupMember member, string title)
        {
            var gm = new NewProducer(member.Schema, member.Configuration, member.ProducerConfiguration); 
            Common(true);
            Receive<RegisteredProducer>(p =>
            {
                _group.Forward(p);
                Become(Init);
                Stash.UnstashAll();
            });
            ReceiveAny(x =>
            {
                Stash.Stash();
            });
            NewProducer(gm, title);
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
                    Context.ActorOf(PartitionedProducer.Prop(_config, _producerConfiguration, _network, _pulsarManager, parent), pn);
                else
                    Context.ActorOf(Producer.Prop(_config, _producerConfiguration.TopicName, _producerConfiguration, Interlocked.Increment(ref IdGenerators.ProducerId), _network, _pulsarManager, isgroup: parent), pn);
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
        
        public static Props Prop(ClientConfigurationData configuration, IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(()=> new ProducerManager(configuration, network, pulsarManager));
        }

        private void NewProducer(NewProducer producer, string title = "")
        {
            var p = Regex.Replace(producer.ProducerConfiguration.ProducerName, @"[^\w\d]", "");
            var child = Context.Child(p);
            if (!child.IsNobody())
            {
                _listener.Log($"Producer with name '{producer.ProducerConfiguration.ProducerName}' already exist for topic '{producer.ProducerConfiguration.TopicName}'");
                if(!string.IsNullOrWhiteSpace(title))
                   Self.Tell(new RegisteredProducer(-1, producer.ProducerConfiguration.ProducerName, string.IsNullOrWhiteSpace(title) ? producer.ProducerConfiguration.TopicName: title, false), child);
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
