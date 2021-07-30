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
using SharpPulsar.Admin.Admin.Models;

namespace SharpPulsar.EventSource.Pulsar
{
    public class CurrentEventsByTopicActor<T> : ReceiveActor
    {
        private readonly CurrentEventsByTopic<T> _message;
        private readonly HttpClient _httpClient;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly ISchema<T> _schema;
        private readonly Admin.Public.Admin _admin;
        private readonly BufferBlock<IMessage<T>> _buffer;
        public CurrentEventsByTopicActor(CurrentEventsByTopic<T> message, HttpClient httpClient, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema)
        {
            _admin = new Admin.Public.Admin(message.AdminUrl, httpClient);
            _message = message;
            _httpClient = httpClient;
            _schema = schema;
            _buffer = new BufferBlock<IMessage<T>>();
            _client = client;
            _cnxPool = cnxPool;
            _lookup = lookup;
            _generator = generator;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata(message.Tenant, message.Namespace, message.Topic);
            Setup(partitions.Body, topic);
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
                    var msgId = GetMessageIds(partitionTopic);
                    var config = PrepareConsumerConfiguration(_message.Configuration, partitionName, msgId.Start,
                        (int) (msgId.End.Index - msgId.Start.Index)); 
                    var child = Context.ActorOf(PulsarSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, msgId.End, false, _httpClient, _message, msgId.Start.Index, _schema));
                    Context.Watch(child);
                }
            }
            else
            {
                var msgId = GetMessageIds(TopicName.Get(topic));
                var permits = (int)(msgId.End.Index - msgId.Start.Index);
                var config = PrepareConsumerConfiguration(_message.Configuration, topic, msgId.Start, permits);
                var child = Context.ActorOf(PulsarSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, msgId.End, false, _httpClient, _message, msgId.Start.Index, _schema));
                Context.Watch(child);
            }
        }
        private ReaderConfigurationData<T> PrepareConsumerConfiguration(ReaderConfigurationData<T> readerConfiguration, string topic, EventMessageId start, int permits)
        {
            readerConfiguration.TopicName = topic;
            readerConfiguration.StartMessageId = new MessageId(start.LedgerId, start.EntryId, -1);
            readerConfiguration.ReceiverQueueSize = permits;
            return readerConfiguration;
        }
        private (EventMessageId Start, EventMessageId End) GetMessageIds(TopicName topic)
        {
            var stats = _admin.GetInternalStats(topic.NamespaceObject.Tenant, topic.NamespaceObject.LocalName, topic.LocalName);
            var start = MessageIdHelper.Calculate(_message.FromSequenceId, stats.Body);
            var startMessageId = new EventMessageId(start.Ledger, start.Entry, start.Index);
            var end = MessageIdHelper.Calculate(_message.ToSequenceId, stats.Body);
            var endMessageId = new EventMessageId(end.Ledger, end.Entry, end.Index);
            return (startMessageId, endMessageId);
        }
        public static Props Prop(CurrentEventsByTopic<T> message, HttpClient httpClient, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema)
        {
            return Props.Create(()=> new CurrentEventsByTopicActor<T>(message, httpClient, client, lookup, cnxPool, generator, schema));
        }
    }
}