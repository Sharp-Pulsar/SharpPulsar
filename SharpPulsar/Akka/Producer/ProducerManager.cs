﻿using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
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
        private long _producerIdGenerator = 0;

        private long _requestIdGenerator = 0;
        private readonly Dictionary<long, Payload> _pendingLookupRequests = new Dictionary<long, Payload>();
        private readonly Dictionary<string, IActorRef> _producers = new Dictionary<string, IActorRef>();
        public ProducerManager(ClientConfigurationData configuration)
        {
            _config = configuration;
            Become(()=>Init(configuration));
        }

        private void Open()
        {
            Stash.UnstashAll();
            Receive<NewProducer>(NewProducer);
            Receive<TcpClosed>(_ =>
            {
                Become(()=>Connecting(_config));
                _network.Tell(new TcpReconnect());
            });
        }

        private void RegisterProducer(ProducerConfigurationData conf)
        {
            Receive<RegisteredProducer>(p =>
            {
                _producers.Add(p.Topic, Sender);
                Become(Open);
                Stash.UnstashAll();
            });
            ReceiveAny(m=> Stash.Stash());
        }
        private void PartitionedTopicMetadata(ProducerConfigurationData conf)
        {
            Receive<Partitions>(x =>
            {
                conf.Partitions = x.Partition;
                conf.UseTls = _config.UseTls;
                _pendingLookupRequests.Remove(x.RequestId);
                Context.ActorOf(Producer.Prop(_config, conf, _producerIdGenerator++, _network));
                Become(()=> RegisterProducer(conf));
            });
            ReceiveAny(_=> Stash.Stash());
            var requestId = _requestIdGenerator++;
            var request = Commands.NewPartitionMetadataRequest(conf.TopicName, requestId);
            var pay = new Payload(request.Array, requestId, "CommandPartitionedTopicMetadata");
            _pendingLookupRequests.Add(requestId, pay);
            _network.Tell(pay);
        }
        private void LookupSchema(ProducerConfigurationData conf)
        {
            Receive<SchemaResponse>(s =>
            {
                var info = new SchemaInfo
                {
                    Name = s.Name, Schema = (sbyte[]) (object) s.Schema, Properties = s.Properties
                };
                if (s.Type == Schema.Types.Type.None)
                {
                    info.Type = SchemaType.Bytes;
                }
                else if (s.Type == Schema.Types.Type.Json)
                {
                    info.Type = SchemaType.Json;
                }
                else
                {
                    info.Type = SchemaType.Date;
                }
                conf.Schema = ISchema.GetSchema(info);
                _pendingLookupRequests.Remove(s.RequestId);
                Become(()=> PartitionedTopicMetadata(conf));
            });
            ReceiveAny(_=> Stash.Stash());
        }
        private void Init(ClientConfigurationData configuration)
        {
            _network = Context.ActorOf(NetworkManager.Prop(Self, configuration));
            Receive<TcpSuccess>(s =>
            {
                Console.WriteLine($"Pulsar handshake completed with {s.Name}");
                Become(Open);
            });
            ReceiveAny(_=>Stash.Stash());
        }
        private void Connecting(ClientConfigurationData configuration)
        {
            Receive<TcpSuccess>(s =>
            {
                Console.WriteLine($"Pulsar handshake completed with {s.Name}");
                Become(Open);
            });
            ReceiveAny(m=>
            {
                Stash.Stash();
            });
        }
        public static Props Prop(ClientConfigurationData configuration)
        {
            return Props.Create(()=> new ProducerManager(configuration));
        }

        private void NewProducer(NewProducer producer)
        {
            var schema = producer.ProducerConfiguration.Schema;
            var clientConfig = producer.Configuration;
            var producerConfig = producer.ProducerConfiguration;
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

            var topic = producerConfig.TopicName;

            if (!TopicName.IsValid(topic))
            {
                Sender.Tell(new ErrorMessage(new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'")));
                return;
            }
            var topicName = TopicName.Get(topic);
            producerConfig.TopicName = topicName.ToString();
            if (schema is AutoProduceBytesSchema autoProduceBytesSchema)
            {
               
                if (autoProduceBytesSchema.SchemaInitialized())
                {
                    Become(() => PartitionedTopicMetadata(producerConfig));
                }
                else
                {
                    var requestId = _requestIdGenerator++;
                    var request = Commands.NewGetSchema(requestId, topicName.ToString(), BytesSchemaVersion.Of(null));
                    var payload = new Payload(request.Array, requestId, "CommandGetSchema");
                    _network.Tell(payload);
                    _pendingLookupRequests.Add(requestId, payload);
                    Become(() => LookupSchema(producerConfig));
                }
            }
            Become(()=> PartitionedTopicMetadata(producerConfig));
		}

        public IStash Stash { get; set; }
    }
}