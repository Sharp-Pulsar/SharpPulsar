using System.Net.Http;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Presto;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.Admin.Admin.Models;
using SharpPulsar.EventSource.Messages;

namespace SharpPulsar.EventSource.Presto
{
    public class EventsByTopicActor : ReceiveActor
    {
        private readonly EventsByTopic _message;
        private readonly BufferBlock<IEventEnvelope> _buffer;
        private readonly Admin.Public.Admin _admin;
        public EventsByTopicActor(EventsByTopic message, BufferBlock<IEventEnvelope> buffer)
        {
            _admin = new Admin.Public.Admin(message.AdminUrl, new HttpClient());
            _buffer = buffer;
            _message = message;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata(message.Tenant, message.Namespace, message.Topic);
            Setup(partitions.Body, topic);
        }

        private void Setup(PartitionedTopicMetadata p, string topic)
        {
            if (p.Partitions > 0)
            {
                for (var i = 0; i < p.Partitions; i++)
                {
                    Context.ActorOf(PrestoSourceActor.Prop(_buffer, true, _message));
                    
                }
            }
            else
            {
                Context.ActorOf(PrestoSourceActor.Prop(_buffer,true,  _message));
            }
        }
        public static Props Prop(EventsByTopic message, BufferBlock<IEventEnvelope> buffer)
        {
            return Props.Create(() => new EventsByTopicActor(message, buffer));
        }
    }
}