using System.Net.Http;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Presto;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.Admin.Admin.Models;
using SharpPulsar.EventSource.Messages;

namespace SharpPulsar.EventSource.Presto.Tagged
{
    public class EventsByTagActor : ReceiveActor
    {
        private readonly EventsByTag _message;
        private readonly Admin.Public.Admin _admin;
        private readonly BufferBlock<IEventEnvelope> _buffer;
        public EventsByTagActor(EventsByTag message, BufferBlock<IEventEnvelope> buffer)
        {
            _admin = new Admin.Public.Admin(message.AdminUrl, new HttpClient());
            _message = message;
            _buffer = buffer;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata(message.Tenant, message.Namespace, message.Topic, true);
            Setup(partitions.Body);
        }

        private void Setup(PartitionedTopicMetadata p)
        {
            if (p.Partitions > 0)
            {
                for (var i = 0; i < p.Partitions; i++)
                {
                    Context.ActorOf(PrestoTaggedSourceActor.Prop(_buffer, true, _message, _message.Tag));

                }
            }
            else
            {
                Context.ActorOf(PrestoTaggedSourceActor.Prop(_buffer, true, _message, _message.Tag));
            }
        } 
        public static Props Prop(EventsByTag message, BufferBlock<IEventEnvelope> buffer)
        {
            return Props.Create(() => new EventsByTagActor(message, buffer));
        }
    }
}