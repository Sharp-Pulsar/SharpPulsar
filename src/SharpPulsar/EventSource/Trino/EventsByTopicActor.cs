using System.Net.Http;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Presto;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.Admin.v2;
using System;

namespace SharpPulsar.EventSource.Trino
{
    public class EventsByTopicActor : ReceiveActor
    {
        private readonly EventsByTopic _message;
        private readonly BufferBlock<IEventEnvelope> _buffer;
        private readonly PulsarAdminRESTAPIClient _admin;
        public EventsByTopicActor(EventsByTopic message, BufferBlock<IEventEnvelope> buffer)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(message.AdminUrl)
            };
            _admin = new PulsarAdminRESTAPIClient(client);
            _buffer = buffer;
            _message = message;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata2Async(message.Tenant, message.Namespace, message.Topic, false, false).GetAwaiter().GetResult();
            Setup(partitions, topic);
        }

        private void Setup(PartitionedTopicMetadata p, string topic)
        {
            if (p.Partitions > 0)
            {
                for (var i = 0; i < p.Partitions; i++)
                {
                    Context.ActorOf(TrinoSourceActor.Prop(_buffer, true, _message));

                }
            }
            else
            {
                Context.ActorOf(TrinoSourceActor.Prop(_buffer, true, _message));
            }
        }
        public static Props Prop(EventsByTopic message, BufferBlock<IEventEnvelope> buffer)
        {
            return Props.Create(() => new EventsByTopicActor(message, buffer));
        }
    }
}