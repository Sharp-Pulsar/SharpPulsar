using System.Net.Http;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Presto;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.Admin.v2;
using System;

namespace SharpPulsar.EventSource.Trino.Tagged
{
    public class EventsByTagActor : ReceiveActor
    {
        private readonly EventsByTag _message;
        private readonly PulsarAdminRESTAPIClient _admin;
        private readonly BufferBlock<IEventEnvelope> _buffer;
        public EventsByTagActor(EventsByTag message, BufferBlock<IEventEnvelope> buffer)
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri($"{message.AdminUrl}/admin/v2/")
            };
            _admin = new PulsarAdminRESTAPIClient(http);
            _message = message;
            _buffer = buffer;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata2Async(message.Tenant, message.Namespace, message.Topic, false, false).GetAwaiter().GetResult();
            Setup(partitions);
        }

        private void Setup(PartitionedTopicMetadata p)
        {
            if (p.Partitions > 0)
            {
                for (var i = 0; i < p.Partitions; i++)
                {
                    Context.ActorOf(TrinoTaggedSourceActor.Prop(_buffer, true, _message, _message.Tag));

                }
            }
            else
            {
                Context.ActorOf(TrinoTaggedSourceActor.Prop(_buffer, true, _message, _message.Tag));
            }
        }
        public static Props Prop(EventsByTag message, BufferBlock<IEventEnvelope> buffer)
        {
            return Props.Create(() => new EventsByTagActor(message, buffer));
        }
    }
}