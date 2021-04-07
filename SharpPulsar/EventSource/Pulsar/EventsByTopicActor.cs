using System.Net.Http;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Pulsar;
using SharpPulsar.Common.Naming;
using SharpPulsar.Interfaces;
using SharpPulsar.Queues;
using SharpPulsar.Configuration;

namespace SharpPulsar.EventSource.Pulsar
{
    public class EventsByTopicActor<T> : ReceiveActor
    {
        private readonly EventsByTopic<T> _message;
        private readonly HttpClient _httpClient;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly ISchema<T> _schema;
        private readonly ConsumerQueueCollections<T> _buffer;
        private readonly User.Admin _admin;
        public EventsByTopicActor(EventsByTopic<T> message, HttpClient httpClient, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema, ConsumerQueueCollections<T> queue)
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
                    var config = PrepareConsumerConfiguration(_message.Configuration, partitionName, msgId.Start, (int)(msgId.End.Index - msgId.Start.Index));
                    Context.ActorOf(PulsarSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, msgId.End, true, _httpClient, _message, msgId.Start.Index, _schema, _buffer));
                    
                }
            }
            else
            {
                var msgId = GetMessageIds(TopicName.Get(topic));
                var config = PrepareConsumerConfiguration(_message.Configuration, topic, msgId.Start, (int)(msgId.End.Index - msgId.Start.Index));
                Context.ActorOf(PulsarSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator,  msgId.End, true, _httpClient, _message, msgId.Start.Index, _schema, _buffer));
            }
        }
        private ReaderConfigurationData<T> PrepareConsumerConfiguration(ReaderConfigurationData<T> readerConfiguration, string topic, EventMessageId start, int permits)
        {
            readerConfiguration.TopicNames.Add(topic);
            readerConfiguration.StartMessageId = new MessageId(start.LedgerId, start.EntryId, -1);
            readerConfiguration.ReceiverQueueSize = permits;
            return readerConfiguration;
        }

        private (EventMessageId Start, EventMessageId End) GetMessageIds(TopicName topic)
        {
            var adminRestapi = new User.Admin(_message.AdminUrl, _httpClient);
            var statsResponse = adminRestapi.GetInternalStats(topic.NamespaceObject.Tenant, topic.NamespaceObject.LocalName, topic.LocalName);
            var start = MessageIdHelper.Calculate(_message.FromSequenceId, statsResponse.Body);
            var startMessageId = new EventMessageId(start.Ledger, start.Entry, start.Index);
            var end = MessageIdHelper.Calculate(_message.ToSequenceId, statsResponse.Body);
            var endMessageId = new EventMessageId(end.Ledger, end.Entry, end.Index);
            return (startMessageId, endMessageId);
        }
        public static Props Prop(EventsByTopic<T> message, HttpClient httpClient, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema, ConsumerQueueCollections<T> queue)
        {
            return Props.Create(() => new EventsByTopicActor<T>(message, httpClient, client, lookup, cnxPool, generator, schema, queue));
        }
    }
}