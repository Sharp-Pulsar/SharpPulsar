using System;
using System.Linq;
using System.Net.Http;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Pulsar;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.Utils;
using SharpPulsar.Admin.v2;

namespace SharpPulsar.EventSource.Pulsar
{
    public class CurrentEventsByTopicActor<T> : ReceiveActor
    {
        private readonly CurrentEventsByTopic<T> _message;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly ISchema<T> _schema;
        private readonly PulsarAdminRESTAPIClient _admin;
        private readonly BufferBlock<IMessage<T>> _buffer;
        public CurrentEventsByTopicActor(CurrentEventsByTopic<T> message, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema)
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri(message.AdminUrl)
            };
            _admin = new PulsarAdminRESTAPIClient(http);
            _message = message;
            _schema = schema;
            _buffer = new BufferBlock<IMessage<T>>();
            _client = client;
            _cnxPool = cnxPool;
            _lookup = lookup;
            _generator = generator;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata2Async(message.Tenant, message.Namespace, message.Topic, false, false).GetAwaiter().GetResult();
            Setup(partitions, topic);
            Receive<Terminated>(t =>
            {
                var children =  Context.GetChildren();
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
                        (int) (_message.ToMessageId - _message.FromMessageId)); 
                    var child = Context.ActorOf(PulsarSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, _message.FromMessageId, _message.ToMessageId, false, _schema));
                    Context.Watch(child);
                }
            }
            else
            {
                var msgId = GetMessageId();
                var permits = (int)(_message.ToMessageId - _message.FromMessageId);
                var config = PrepareConsumerConfiguration(_message.Configuration, topic, msgId, permits);
                var child = Context.ActorOf(PulsarSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, _message.FromMessageId, _message.ToMessageId, false, _schema));
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
        public static Props Prop(CurrentEventsByTopic<T> message, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema)
        {
            return Props.Create(()=> new CurrentEventsByTopicActor<T>(message, client, lookup, cnxPool, generator, schema));
        }
    }
}