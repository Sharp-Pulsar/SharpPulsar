using System;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Api;
using SharpPulsar.Common.Schema;
using SharpPulsar.Exceptions;
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
        private readonly Dictionary<long, Payload> _pendingLookupRequests = new Dictionary<long, Payload>();
        private readonly Dictionary<string, IActorRef> _producers = new Dictionary<string, IActorRef>();
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
            Receive<NewProducer>(NewProducer);
            Receive<Partitions>(x =>
            {

                _listener.Log($"Found {x.Partition} partition for Topic: {_producerConfiguration.TopicName}");
                _producerConfiguration.Partitions = x.Partition;
               _producerConfiguration.UseTls = _config.UseTls;
                _pendingLookupRequests.Remove(x.RequestId);
                if (x.Partition > 0)
                    Context.ActorOf(PartitionedProducer.Prop(_config, _producerConfiguration,  _network), "partitionedproducer");
                else
                    Context.ActorOf(Producer.Prop(_config, _producerConfiguration.TopicName, _producerConfiguration, Interlocked.Increment(ref IdGenerators.ProducerId), _network), "producer");
            });
            Receive<RegisteredProducer>(p =>
            {
                _producers.Add(p.Topic, Sender);
                Become(Open);
            });
            Receive<SchemaResponse>(s =>
            {
                _listener.Log($"Found {s.Name} schema for Topic: {_producerConfiguration.TopicName}");
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
                    info.Type = SchemaType.Date;
                }
                _producerConfiguration.Schema = ISchema.GetSchema(info);
                _pendingLookupRequests.Remove(s.RequestId);
                SendPartitionMetadataRequestCommand(_producerConfiguration);
            });
            ReceiveAny(x =>
            {
                Console.WriteLine($"Stashing message of type {x.GetType()}: producer not created!");
                Stash.Stash();
            });
        }
        private void Open()
        {
            Receive<Send>(s =>
            {
                if(_producers.ContainsKey(s.Topic))
                    _producers[s.Topic]?.Tell(s);
                else
                    _listener.Log($"{s.Topic} producer not created");
            });
            Receive<BulkSend>(s =>
            {
                if (_producers.ContainsKey(s.Topic))
                    _producers[s.Topic]?.Tell(s);
                else
                    _listener.Log($"{s.Topic} producer not created");
            });
            Stash.UnstashAll();
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
            _pendingLookupRequests.Add(requestId, pay);
            _network.Tell(pay);
        }
        
        public static Props Prop(ClientConfigurationData configuration, IActorRef network)
        {
            return Props.Create(()=> new ProducerManager(configuration, network));
        }

        private void NewProducer(NewProducer producer)
        {
            var schema = producer.ProducerConfiguration.Schema;
            var clientConfig = producer.Configuration;
            var producerConfig = producer.ProducerConfiguration;
            _producerConfiguration = producerConfig;
            _listener = _producerConfiguration.ProducerEventListener;
            _listener.Log($"creating producer for topic: {producerConfig.TopicName}");
            if (clientConfig == null)
            {
                Sender.Tell(new ErrorMessage(new PulsarClientException.InvalidConfigurationException("Producer configuration undefined")));
                return;
            }

            if (schema is AutoConsumeSchema)
            {
                Sender.Tell(new ErrorMessage(new PulsarClientException.InvalidConfigurationException("AutoConsumeSchema is only used by consumers to detect schemas automatically")));
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
                    _pendingLookupRequests.Add(requestId, payload);
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
