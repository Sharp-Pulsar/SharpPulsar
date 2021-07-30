using System.Net.Http;
using Akka.Actor;
using SharpPulsar.Common.Naming;
using SharpPulsar.EventSource.Messages.Pulsar;
using SharpPulsar.Interfaces;
using SharpPulsar.Configuration;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.Messages.Consumer;

namespace SharpPulsar.EventSource.Pulsar.Tagged
{
    public class EventsByTagActor<T> : ReceiveActor
    {
        private readonly EventsByTag<T> _message;
        private readonly HttpClient _httpClient;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly ISchema<T> _schema;
        private readonly BufferBlock<IMessage<T>> _buffer;
        private readonly Admin.Public.Admin _admin;
        public EventsByTagActor(EventsByTag<T> message, HttpClient httpClient, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema)
        {
            _admin = new Admin.Public.Admin(message.AdminUrl, httpClient);
            _message = message;
            _httpClient = httpClient;
            _schema = schema;
            _client = client;
            _cnxPool = cnxPool;
            _lookup = lookup;
            _generator = generator;
            _buffer = new BufferBlock<IMessage<T>>();
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata(message.Tenant, message.Namespace, message.Topic);
            Setup(partitions.Body, topic);

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
                    Context.ActorOf(PulsarTaggedSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, msgId.End, true, _httpClient, _message, _message.Tag, msgId.Start.Index, _schema));

                }
            }
            else
            {
                var msgId = GetMessageIds(TopicName.Get(topic));
                var config = PrepareConsumerConfiguration(_message.Configuration, topic, msgId.Start, (int)(msgId.End.Index - msgId.Start.Index));
                Context.ActorOf(PulsarTaggedSourceActor<T>.Prop(_message.ClientConfiguration, config, _client, _lookup, _cnxPool, _generator, msgId.End, true, _httpClient, _message, _message.Tag, msgId.Start.Index, _schema));
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
            var adminRestapi = new User.Admin(_message.AdminUrl, _httpClient);
            var statsResponse = adminRestapi.GetInternalStats(topic.NamespaceObject.Tenant, topic.NamespaceObject.LocalName, topic.LocalName);
            var start = MessageIdHelper.Calculate(_message.FromSequenceId, statsResponse.Body);
            var startMessageId = new EventMessageId(start.Ledger, start.Entry, start.Index);
            var end = MessageIdHelper.Calculate(_message.ToSequenceId, statsResponse.Body);
            var endMessageId = new EventMessageId(end.Ledger, end.Entry, end.Index);
            return (startMessageId, endMessageId);
        }
        public static Props Prop(EventsByTag<T> message, HttpClient httpClient, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef generator, ISchema<T> schema)
        {
            return Props.Create(() => new EventsByTagActor<T>(message, httpClient, client, lookup, cnxPool, generator, schema));
        }
    }
}