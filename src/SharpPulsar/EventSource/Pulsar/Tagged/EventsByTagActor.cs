﻿using System.Net.Http;
using Akka.Actor;
using SharpPulsar.Common.Naming;
using SharpPulsar.EventSource.Messages.Pulsar;
using SharpPulsar.Interfaces;
using SharpPulsar.Configuration;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Utils;
using SharpPulsar.Admin.v2;
using System;

namespace SharpPulsar.EventSource.Pulsar.Tagged
{
    public class EventsByTagActor<T> : ReceiveActor
    {
        private readonly EventsByTag<T> _message;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly ISchema<T> _schema;
        private readonly BufferBlock<IMessage<T>> _buffer;
        private readonly PulsarAdminRESTAPIClient _admin;
        public EventsByTagActor(EventsByTag<T> message, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema)
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri(message.AdminUrl)
            };
            _admin = new PulsarAdminRESTAPIClient(http);
            _message = message;
            _schema = schema;
            _client = client;
            _cnxPool = cnxPool;
            _lookup = lookup;
            _generator = generator;
            _buffer = new BufferBlock<IMessage<T>>();
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata2Async(message.Tenant, message.Namespace, message.Topic, false, false).GetAwaiter().GetResult();
            Setup(partitions, topic);
            Receive<ReceivedMessage<T>>(m =>
            {
                _buffer.Post(m.Message);
            });
            Receive<Messages.Receive>(_ =>
            {
                if (_buffer.TryReceive(out var message))
                    Sender.Tell(new AskResponse(message));
                else
                    Sender.Tell(new AskResponse(null));
            });
        }

        private void Setup(PartitionedTopicMetadata p, string topic)
        {
            if (p.Partitions > 0)
            {
                for (var i = 0; i < p.Partitions; i++)
                {
                    var partitionTopic = TopicName.Get(topic).GetPartition(i);
                    var partitionName = partitionTopic.ToString();
                    var msgId = GetMessageId();
                    var config = PrepareConsumerConfiguration(_message.Configuration, partitionName, new MessageId(msgId.LedgerId, msgId.EntryId, i),
                        (int)(_message.ToMessageId - _message.FromMessageId));
                    Context.ActorOf(PulsarTaggedSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, _message.FromMessageId, _message.ToMessageId, true, _message.Tag,  _schema));

                }
            }
            else
            {
                var msgId = GetMessageId();
                var config = PrepareConsumerConfiguration(_message.Configuration, topic, msgId, (int)(_message.ToMessageId - _message.FromMessageId));
                Context.ActorOf(PulsarTaggedSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, _message.FromMessageId, _message.ToMessageId, true, _message.Tag, _schema));
            }
        }
        private ReaderConfigurationData<T> PrepareConsumerConfiguration(ReaderConfigurationData<T> readerConfiguration, string topic, MessageId messageId, int permits)
        {
            readerConfiguration.TopicName = topic;
            readerConfiguration.StartMessageId = messageId;
            readerConfiguration.ReceiverQueueSize = permits;
            return readerConfiguration;
        }

        private MessageId GetMessageId()
        {
            return (MessageId)MessageIdUtils.GetMessageId(_message.FromMessageId);
        }
        public static Props Prop(EventsByTag<T> message, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema)
        {
            return Props.Create(() => new EventsByTagActor<T>(message, client, lookup, cnxPool, generator, schema));
        }
    }
}