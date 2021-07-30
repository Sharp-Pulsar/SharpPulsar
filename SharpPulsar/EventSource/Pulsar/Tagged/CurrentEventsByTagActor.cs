using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Pulsar;
using SharpPulsar.Messages;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility;
using SharpPulsar.Interfaces;
using SharpPulsar.Queues;
using SharpPulsar.Configuration;

namespace SharpPulsar.EventSource.Pulsar.Tagged
{
    public class CurrentEventsByTagActor<T> : ReceiveActor
    {
        private readonly CurrentEventsByTag<T> _message;
        private readonly HttpClient _httpClient;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly ISchema<T> _schema;
        private readonly ConsumerQueueCollections<T> _buffer;
        private readonly User.Admin _admin;
        public CurrentEventsByTagActor(CurrentEventsByTag<T> message, HttpClient httpClient, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema, ConsumerQueueCollections<T> queue)
        {
            _admin = new User.Admin(message.AdminUrl, httpClient);
            _message = message;
            _httpClient = httpClient;
            _schema = schema;
            _client = client;
            _cnxPool = cnxPool;
            _lookup = lookup;
            _generator = generator;
            _buffer = queue;
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
        }

        private void Setup(Admin.Models.PartitionedTopicMetadata p, string topic)
        {
            if (p.Partitions > 0)
            {
                for (var i = 0; i < p.Partitions; i++)
                {
                    var partitionTopic = TopicName.Get(topic).GetPartition(i);
                    var partitionName = partitionTopic.ToString();
                    var msgId = GetMessageIds(partitionTopic);
                    var config = PrepareConsumerConfiguration(_message.Configuration, partitionName, msgId.Start,
                        (int)(msgId.End.Index - msgId.Start.Index));
                    var child = Context.ActorOf(PulsarTaggedSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, msgId.End, false, _httpClient, _message, _message.Tag, msgId.Start.Index, _schema, _buffer));
                    Context.Watch(child);
                }
            }
            else
            {
                var msgId = GetMessageIds(TopicName.Get(topic));
                var config = PrepareConsumerConfiguration(_message.Configuration, topic, msgId.Start, (int)(msgId.End.Index - msgId.Start.Index));
                var child = Context.ActorOf(PulsarTaggedSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, msgId.End, false, _httpClient, _message, _message.Tag, msgId.Start.Index, _schema, _buffer));
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
        public static Props Prop(CurrentEventsByTag<T> message, HttpClient httpClient, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema, ConsumerQueueCollections<T> queue)
        {
            return Props.Create(() => new CurrentEventsByTagActor<T>(message, httpClient, client, lookup, cnxPool, generator, schema, queue));
        }
    }
}