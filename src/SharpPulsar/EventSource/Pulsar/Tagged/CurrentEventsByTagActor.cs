using System;
using System.Linq;
using System.Net.Http;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Pulsar;
using SharpPulsar.Common.Naming;
using SharpPulsar.Interfaces;
using SharpPulsar.Configuration;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.Admin.Admin.Models;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Utils;

namespace SharpPulsar.EventSource.Pulsar.Tagged
{
    public class CurrentEventsByTagActor<T> : ReceiveActor
    {
        private readonly CurrentEventsByTag<T> _message;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly ISchema<T> _schema;
        private readonly BufferBlock<IMessage<T>> _buffer;
        private readonly Admin.Public.Admin _admin;
        public CurrentEventsByTagActor(CurrentEventsByTag<T> message, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema)
        {
            _admin = new Admin.Public.Admin(message.AdminUrl, new HttpClient());
            _message = message;
            _schema = schema;
            _client = client;
            _buffer = new BufferBlock<IMessage<T>>();
            _cnxPool = cnxPool;
            _lookup = lookup;
            _generator = generator;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata(message.Tenant, message.Namespace, message.Topic);
            Setup(partitions.Body, topic);
            Receive<Akka.Actor.Terminated>(t =>
            {
                var children = Context.GetChildren();
                if (!children.Any())
                {
                    Context.System.Log.Info($"All children exited, shutting down in 5 seconds :{Self.Path}");
                    Self.GracefulStop(TimeSpan.FromSeconds(5));
                }
            });
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
                    var child = Context.ActorOf(PulsarTaggedSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, _message.FromMessageId, _message.ToMessageId, false,_message.Tag, _schema));
                    Context.Watch(child);
                }
            }
            else
            {
                var msgId = GetMessageId();
                var config = PrepareConsumerConfiguration(_message.Configuration, topic, msgId, (int)(_message.ToMessageId - _message.FromMessageId));
                var child = Context.ActorOf(PulsarTaggedSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, _message.FromMessageId, _message.ToMessageId, false, _message.Tag, _schema));
                Context.Watch(child);
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
        public static Props Prop(CurrentEventsByTag<T> message, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema)
        {
            return Props.Create(() => new CurrentEventsByTagActor<T>(message, client, lookup, cnxPool, generator, schema));
        }
    }
}